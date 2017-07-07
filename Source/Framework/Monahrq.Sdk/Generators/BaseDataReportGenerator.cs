using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Services.Generators;
using NHibernate.Transform;
using Monahrq.Infrastructure.Extensions;
using Microsoft.Practices.ServiceLocation;

namespace Monahrq.Sdk.Generators
{
    /// <summary>
    /// The Base Data Report Generator. Generates all base data the generated website needs to function.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
    [Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.NonShared)]
	[ReportGenerator(
		null,
		new[] { "Base Report", "Base Data" },
		null,
		0)]
	public class BaseDataReportGenerator : BaseReportGenerator
	{
        //private ConnectionStringSettings ConnectionSettings { get { return ConfigurationService.ConnectionSettings; }}


        /// <summary>
        /// Gets or sets the report identifier.
        /// </summary>
        /// <value>
        /// The report identifier.
        /// </value>
        private string ReportID { get; set; }
        /// <summary>
        /// Gets or sets the zip distances.
        /// </summary>
        /// <value>
        /// The zip distances.
        /// </value>
        private DataTable ZipDistances { get; set; }

        /// <summary>
        /// Gets or sets the base data dir.
        /// </summary>
        /// <value>
        /// The base data dir.
        /// </value>
        private string BaseDataDir { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDataReportGenerator"/> class.
        /// </summary>
        /// <param name="sessionFactoryProvider">The session factory provider.</param>
        /// <param name="configurationService">The configuration service.</param>
        [ImportingConstructor]
		public BaseDataReportGenerator(IDomainSessionFactoryProvider sessionFactoryProvider,
									   IConfigurationService configurationService)
		{ }

		// TODO: Move these into BaseData class?
		#region UserDefinedTypes

		#region idsTableType

		private const string idsTableType = @"
            /*
             *      Name:           IDsTableType
             *      Version:        1.0
             *      Last Updated:   3/27/14
             *      Used In:        BaseDataReportGenerator.cs
             *      Description:    Used by sprocs to pass in a list of IDs of type int
             */

            CREATE TYPE IDsTableType AS TABLE (ID int)
            ";

		#endregion idsTableType

		#region uniqueIDsTableType

		private const string uniqueIDsTableType = @"
            /*
             *      Name:           UniqueIDsTableType
             *      Version:        1.0
             *      Last Updated:   3/28/14
             *      Used In:        BaseDataReportGenerator.cs
             *      Description:    Used by sprocs to pass in a list of IDs of type uniqueidentifier
             */

            CREATE TYPE UniqueIDsTableType AS TABLE (ID uniqueidentifier)
            ";

		#endregion uniqueIDsTableType

		#region stringsTableType

		private const string stringsTableType = @"
            /*
             *      Name:           StringsTableType
             *      Version:        1.0
             *      Last Updated:   3/28/14
             *      Used In:        BaseDataReportGenerator.cs
             *      Description:    Used by sprocs to pass in a list of IDs of type string
             */

            CREATE TYPE StringsTableType AS TABLE (ID nvarchar(MAX))
            ";

		#endregion stringsTableType

		#endregion UserDefinedTypes

		#region Tables

		#region tableStratificationVals

		private const string tableBaseStratificationVals = @"
            /*
             *      Name:           Base_StratificationVals
             *      Version:        1.0
             *      Last Updated:   3/28/14
             *      Used In:        BaseDataReportGenerator.cs
             *      Description:    Used to hold stratification names
             */

             CREATE TABLE Base_StratificationVals(
                                CatID int NOT NULL,
                                CatVal int NOT NULL,
                                Name varchar(max) NULL
                            ) ON [PRIMARY]
            ";
		#endregion tableStratificationVals

		#region tableAges

		private const string tableBaseAges = @"
            /*
             *      Name:           Base_Ages
             *      Version:        1.0
             *      Last Updated:   3/28/14
             *      Used In:        BaseDataReportGenerator.cs
             *      Description:    Used to hold age range names
             */

             CREATE TABLE Base_Ages(
                                ID int NOT NULL,
                                Name nvarchar(50) NOT NULL
                            ) ON [PRIMARY]
            ";
		#endregion tableAges

		#endregion Tables

		#region Functions

		private const string GET_COST_TO_CHARGE_RATIO_UDF = @"create function [dbo].[fnGetCostToChargeRatio]
                                                    (
	                                                    @ReportingYear nvarchar(4),
	                                                    @CMSProvider nvarchar(50)
                                                    )
                                                    returns real
                                                    as
                                                    begin
	                                                    declare @returnValue real

	                                                    if exists (select top 1 [Ratio] from [dbo].[Base_CostToCharges] where [Year] = @ReportingYear)
	                                                    begin 
		                                                    Set @returnValue = (select top 1 [Ratio] from [dbo].[Base_CostToCharges] where [Year] = @ReportingYear and [ProviderID] = @CMSProvider order by [Year] desc)
	                                                    end
	                                                    else
	                                                    begin
		                                                    Set @returnValue = (select Top 1 [Ratio] from [dbo].[Base_CostToCharges] where [ProviderID] = @CMSProvider order by [Year] desc)
	                                                    end
	
	                                                    return @returnValue
                                                    end";

		private const string _fnFormatString = @"
				create function [dbo].[fnFormatString]
				(
					@Format NVARCHAR(4000) ,
					@Parameters NVARCHAR(4000)
				)	RETURNS NVARCHAR(MAX)
				AS
				BEGIN
					declare @Message NVARCHAR(400)
					declare @Delimiter CHAR(1)
					DECLARE @ParamTable TABLE ( ID INT IDENTITY(0,1), Paramter VARCHAR(1000) )

					SELECT @Message = @Format, @Delimiter = ',';

					WITH CTE (StartPos, EndPos) AS
					(
						SELECT 1, CHARINDEX(@Delimiter, @Parameters)
						UNION ALL
						SELECT EndPos + (LEN(@Delimiter)), CHARINDEX(@Delimiter,@Parameters, EndPos + (LEN(@Delimiter)))
						FROM CTE
						WHERE EndPos > 0
					)
					INSERT INTO @ParamTable ( Paramter )
					SELECT	[ID] = SUBSTRING ( @Parameters, StartPos, CASE WHEN EndPos > 0 THEN EndPos - StartPos ELSE 4000 END )
					FROM	CTE
	
					UPDATE @ParamTable SET @Message = REPLACE ( @Message, '{'+CONVERT(VARCHAR,ID) + '}', Paramter )
					RETURN @Message
				END";
		private const string _fnToProperCase = @"
				create function [dbo].[fnToProperCase]
				(
					@string NVARCHAR(4000),
					@delimitersEx NVARCHAR(100) = ''	-- Use <DEFAULT>
				) 
				RETURNS NVARCHAR(4000)
				AS
				BEGIN
				  DECLARE @i INT           -- index
				  DECLARE @l INT           -- input length
				  DECLARE @c NCHAR(1)      -- current char
				  DECLARE @f INT           -- first letter flag (1/0)
				  DECLARE @o VARCHAR(255)  -- output string
				  DECLARE @w VARCHAR(20)   -- characters considered as white space

				  SET @w = '[' + CHAR(13) + CHAR(10) + CHAR(9) + CHAR(160) + ' ' + @delimitersEx + ']'
				  SET @i = 0
				  SET @l = LEN(@string)
				  SET @f = 1
				  SET @o = ''''

				  WHILE @i <= @l
				  BEGIN
					SET @c = SUBSTRING(@string, @i, 1);
					IF (@f = 1)
					BEGIN
					 SET @o = @o + @c;
					 SET @f = 0;
					END
					ELSE
					BEGIN
					 SET @o = @o + LOWER(@c);
					END

					IF @c LIKE @w SET @f = 1;

					SET @i = @i + 1;
				  END

				  RETURN @o;
				END";
		#endregion

		#region SProcs

		#region spGetAdmissionSources

		private readonly string[] _spGetAdmissionSources = new[]
        {
             "    SELECT Id, Name"
            ,"    FROM Base_AdmissionSources"
        };

		#endregion spGetAdmissionSources

		#region spGetAdmissionTypes

		private readonly string[] _spGetAdmissionTypes = new[]
        {
             "    SELECT Id, Name"
            ,"    FROM Base_AdmissionTypes"
        };

		#endregion spGetAdmissionTypes

		#region spInsertAgeVals

		private string[] spInitializeAgeVals = new string[]
        {
            "INSERT INTO Base_Ages (Id, Name)"
            ,"SELECT 1, '<18'"
            ,"UNION"
            ,"SELECT 2, '18-44'"
            ,"UNION"
            ,"SELECT 3, '45-64'"
            ,"UNION"
            ,"SELECT 4, '65+'"
        };

		#endregion spInsertAgeVals

		#region spGetAges

		private string[] spGetAges = new string[]
        {
            "SELECT Id, Name"
            ,"FROM Base_Ages"
        };

		#endregion spGetAges

		#region spGetDispositionCodes

		private string[] spGetDispositionCodes = new string[]
        {
             "    SELECT Id, Name"
            ,"    FROM Base_DispositionCodes"
        };

		#endregion spGetDispositionCodes

		#region spGetPayers

		private string[] spGetPayers = new string[]
        {
             "    SELECT Id, Name"
            ,"    FROM Base_Payers"
        };

		#endregion spGetPayers

		#region spGetPointOfOrigins

		private string[] spGetPointOfOrigins = new string[]
        {
             "    SELECT Id, Name"
            ,"    FROM Base_PointOfOrigins"
        };

		#endregion spGetPointOfOrigins

		#region spGetRaces

		private string[] spGetRaces = new string[]
        {
             "    SELECT Id, Name"
            ,"    FROM Base_Races"
        };

		#endregion spGetRaces

		#region spGetSexes

		private string[] spGetSexes = new string[]
        {
             "    SELECT Id, Name"
            ,"    FROM Base_Sexes"
        };

		#endregion spGetSexes

		#region spGetStratifications

		private string[] spGetStratifications = new string[]
        {
             "    SELECT 0 AS Id, 'Total' AS Name, 'Total' AS Caption"
            ,"    UNION"
            ,"    SELECT 1, 'Age', 'Age Group'"
            ,"    UNION"
            ,"    SELECT 2, 'Sex', 'Gender'"
            ,"    UNION"
            ,"    SELECT 3, 'Payer', 'Payer'"
            ,"    UNION"
            ,"    SELECT 4, 'Race', 'Race/Ethnicity'"
        };

		#endregion spGetStratifications

		#region spInitializeStratificationVals

		private string[] spInitializeStratificationVals = new string[]
        {
            "DELETE FROM Base_StratificationVals"   
            ,"INSERT INTO Base_StratificationVals (CatID, CatVal, Name)"
            ,"SELECT 0, 0, 'Total'"
            ,"UNION"
            ,"SELECT 1, Id, Name"
            ,"FROM Base_Ages"
            ,"UNION"
            ,"SELECT 2, Id, Name"
            ,"FROM Base_Sexes"
            ,"UNION"
            ,"SELECT 3, Id, Name"
            ,"FROM Base_Payers"
            ,"UNION"
            ,"SELECT 4, Id, Name"
            ,"FROM Base_Races"
        };

		#endregion spInitializeStratificationVals

		#region spGetStratificationVals

		private string[] spGetStratificationVals = new string[]
        {
             "SELECT *"
            ,"FROM Base_StratificationVals"
        };

		#endregion spGetStratificationVals

		#region fnGetHospitalRegion

		private readonly string _fnGetHospitalRegion = @"	    
                                                            CREATE FUNCTION [dbo].[fnGetHospitalRegion]
                                                            (
	                                                            @Hospital_Id int,
	                                                            @RegionType nvarchar(50)
                                                            )
                                                            RETURNS int
                                                            AS
                                                            BEGIN

	                                                            DECLARE @SelectedRegion_Id INT = NULL;

                                                                IF ((SELECT DISTINCT h1.[CustomRegion_Id] FROM [dbo].[Hospitals] h1 WHERE h1.[Id] = @Hospital_Id AND h1.[IsArchived] = 0 AND h1.[IsDeleted] = 0)IS NOT NULL )
																BEGIN
																 SELECT @SelectedRegion_Id = [CustomRegion_Id] FROM [dbo].[Hospitals] WHERE [Id] = @Hospital_Id AND [IsArchived] = 0 AND [IsDeleted] = 0;
																END

																IF(@SelectedRegion_Id IS NULL)
																BEGIN
	                                                            SELECT @SelectedRegion_Id =
		                                                            (CASE 
			                                                            WHEN UPPER(RTRIM(LTRIM(@RegionType))) = 'HEALTHREFERRALREGION' then h.[HealthReferralRegion_Id]
			                                                            WHEN UPPER(RTRIM(LTRIM(@RegionType))) = 'HOSPITALSERVICEAREA' then h.[HospitalServiceArea_Id]
		                                                             END) 
	                                                            FROM Hospitals h
		                                                            WHERE h.[Id] = @Hospital_Id;
																END

	                                                            RETURN @SelectedRegion_Id;
                                                            END";
		#endregion //fnGetHospitalRegion

		#region spGetHospitals

		private readonly string[] _spGetHospitals = new[]
        {
             "	WITH HospitalTypes AS"
            ,"	("
            ,"	    SELECT		ParentTable.Hospital_Id AS HospitalID"
            ,"				,	HospitalCategoryID ="
            ,"						STUFF(("
            ,"						        SELECT ','+ CAST(SubTable.Category_Id AS NVARCHAR(MAX))"
            ,"						        FROM Hospitals_HospitalCategories SubTable"
            ,"						        WHERE SubTable.Hospital_Id = ParentTable.Hospital_Id"
            ,"						        FOR XML PATH('') "
            ,"						    ), 1, 1,'')"
            ,"	    FROM		Hospitals_HospitalCategories ParentTable"
            ,"	)"
            ,""	
            ,"	SELECT DISTINCT	h.Id"
			,"		 		,	h.CmsProviderId AS HospitalProviderId"
			,"		 		,	h.Name"
			,"		 		,	h.City"
			,"		 		,	h.State"
			,"		 		,	h.Zip"
			,"		 		,	ISNULL([dbo].[fnGetHospitalRegion](h.Id, @RegionType), 0) AS RegionID"
			,"		 		,	ISNULL(HospitalTypes.HospitalCategoryID, '') AS HospitalTypes"
			,"		 		,	h.Latitude"
			,"		 		,	h.Longitude"
		//	,"		 		,	dbo.fnFormatString("
		//	,"						'[{0},{1}]',"
		//	,"						cast(h.Latitude as varchar) + ',' + cast(h.Longitude as varchar)) as LatLng"
            ,"	FROM		 	Hospitals h"
            ,"	    LEFT JOIN	HospitalTypes ON h.Id = HospitalTypes.HospitalID " 
            ,"	WHERE		 	h.Id IN ("
            ,"		 					SELECT	ID"
            ,"		 					FROM	@Hospitals)"
			,"		 AND		h.IsArchived=0"
			,"		 AND		h.IsDeleted=0;"
        };

		private const string SP_GET_HOSPITALS_PARAMS = "@Hospitals IDsTableType READONLY, @RegionType NVARCHAR(50)";

		#endregion spGetHospitals

		#region spGetHospitalsByState

		private string[] spGetHospitalsByState = new string[]
        {
             "    SELECT DISTINCT Hospitals.Id, Hospitals.Name, Zip, [dbo].[fnGetHospitalRegion](Hospitals.Id, @RegionType) AS RegionID"
            ,"    FROM [dbo].[Hospitals] AS Hospitals"
            ,"         LEFT JOIN [dbo].[Base_States] AS States"
            ,"                   ON UPPER(Hospitals.[State]) = UPPER(States.Abbreviation) "
            ,"    WHERE States.[Id] IN ("
            ,"        SELECT DISTINCT ID"
            ,"        FROM @States"
            ,"    )"
        };

		private string spGetHospitalsByStateParams = "@States StringsTableType READONLY, @RegionType NVARCHAR(50)";

		#endregion spGetHospitalsByState

		#region spGetHospitalsByStateAbbreviation

		private string[] spGetHospitalsByStateAbbreviation = new string[]
        {
             "    SELECT DISTINCT Hospitals.Id, Hospitals.Name, Zip, [dbo].[fnGetHospitalRegion](Hospitals.Id, @RegionType) AS RegionID"
            ,"    FROM [dbo].[Hospitals] AS Hospitals"
            ,"         LEFT JOIN [dbo].[Base_States] AS States"
            ,"                   ON UPPER(Hospitals.State) = UPPER(States.Abbreviation) "
            ,"    WHERE States.Abbreviation IN ("
            ,"        SELECT DISTINCT ID"
            ,"        FROM @States"
            ,"    )"
        };

		private string spGetHospitalsByStateAbbreviationParams = "@States StringsTableType READONLY, @RegionType NVARCHAR(50)";

		#endregion spGetHospitalsByStateAbbreviation

		#region spGetHospitalTypes

		private string[] spGetHospitalTypes = new[]
        {
            @"select
  DISTINCT c.[Id] 'HospitalTypeID', c.Name
  from [Categories] as c 
  left join [Hospitals_HospitalCategories] as hc on c.[Id]=hc.[Category_Id]
  WHERE hc.Hospital_Id IN (
  SELECT DISTINCT ID
  FROM @Hospitals
        )"
        };

		private string spGetHospitalTypesParams = "@Hospitals IDsTableType READONLY";

		#endregion spGetHospitalTypes

		#region spGetCounties

		// Get the active Counties only for the hospitals being included in the website.
		private string[] spGetHospitalCounties = new string[]
        {
            "    SELECT -1 AS CountyID, 'Unknown' AS CountyName, '0' AS FIPS"
            , "    UNION"
            , "    SELECT C.[Id] AS CountyID, (C.[Name] + ', ' + c.[State]) AS CountyName, C.[CountyFIPS] AS FIPS"
            , "    FROM [dbo].[Base_Counties] C"
            , "    WHERE C.[CountyFIPS] IN"
            , "    ("
            , "        SELECT DISTINCT ISNULL(H.[County], '') AS CountyID"
            , "        FROM Hospitals H"
            , "        WHERE H.[Id] IN ("
            , "            SELECT DISTINCT ID"
            , "            FROM @Hospitals"
            , "        )"
            , "    )"
        };

		private string spGetHospitalCountiesParams = "@Hospitals IDsTableType READONLY";

		private string[] spGetStateCounties = new string[]
        {
            "    SELECT -1 AS CountyID, 'Unknown' AS CountyName, '0' AS FIPS"
            , "    UNION"
            , "    SELECT DISTINCT C.[Id] AS CountyID, (C.[Name] + ', ' + c.[State]) AS CountyName, C.[CountyFIPS] AS FIPS"
            , "    FROM [dbo].[Base_Counties] C WITH(NOLOCK) "
            , "         INNER JOIN [dbo].[Base_States] S WITH(NOLOCK) ON S.[Abbreviation] = C.[State] "
            , "    WHERE S.Id IN"
            , "    ("
            , "            SELECT ID"
            , "            FROM @States"
            , "    )"
        };

		private string spGetStateCountiesParams = "@States IDsTableType READONLY";



		#endregion spGetCounties

		#region spGetRegions

		// Get the active regions only for the hospitals being included in the website.
		private string[] spGetHospitalRegions = new string[]
        {
           @"     SELECT -1 AS RegionID, 'Unknown' AS Name
    UNION
    SELECT hr.Id AS RegionID, hr.Name + N', ' + s.Abbreviation --, hr.[Id]
    FROM [dbo].[Regions] hr
	left join [dbo].[Base_States] s on s.Abbreviation= hr.State
    WHERE hr.Id IN
    (
        SELECT DISTINCT ISNULL([dbo].[fnGetHospitalRegion](h.Id, @RegionType), 0) AS RegionID
        FROM [dbo].[Hospitals] h
		left join [dbo].[Base_ZipCodeToHRRAndHSAs] z on z.Zip=h.Zip 
        WHERE h.Id IN (
            SELECT DISTINCT ID
            FROM @Hospitals
        )
        and h.IsArchived=0 and h.IsDeleted=0
		and z.State=h.State
        and hr.State= h.State
    )"
        };

		private string spGetHospitalRegionsParams = "@Hospitals IDsTableType READONLY, @RegionType NVARCHAR(50)";

		#endregion spGetRegions

		#region spGetHospitalZips

		private const string spGetHospitalZips = @"
           SELECT DISTINCT z.zip AS Zip, ISNULL(zll.Latitude, 0) AS Latitude,ISNULL(zll.Longitude, 0) AS Longitude
  FROM [dbo].[Base_ZipCodeToHRRAndHSAs] z 
       LEFT JOIN [dbo].[Base_ZipCodeToLatLongs] zll on z.Zip=zll.Zip 
       LEFT JOIN [dbo].[Base_States] s on s.[Abbreviation] = z.[State]
  WHERE s.[Id] IN
    (
                SELECT DISTINCT ID
            FROM @States
            )";

		private const string spGetHospitalZipsParams = "@States IDsTableType READONLY";

		#endregion spGetHospitalZips

		// TODO: Need to add categories
		#region spGetDRG

		private string[] spGetDRG = new[]
        {
            "SELECT MSDRGID AS Id, MDCID AS CategoryID, Description"
            ,"FROM Base_MSDRGs"
            ,"WHERE LastYear = 9999"
        };

		#endregion spGetDRG

		#region spGetDRGCategories

		// NOTE: MDC is the "category" for the DRGs
		private const string spGetDRGCategories = @"
            SELECT MDCID AS Id, Description AS Name
            FROM Base_MDCs
            WHERE LastYear = 9999 AND
                MDCID IN (
                    SELECT MDCID
                    FROM Base_MSDRGs
                )";

		#endregion spGetDRGCategories


		// TODO: Need to add categories
		#region spGetMDC

		private string[] spGetMDC = new[]
        {
            "SELECT MDCID as Id, Description"
            ,"FROM Base_MDCs"
            ,"WHERE LastYear = 9999"
        };

		#endregion spGetMDC

		#region spGetDXCCS

		private string[] spGetDXCCS = new[]
        {
            "SELECT DXCCSID AS Id, CategoryID, Description"
            ,"FROM Base_DXCCSLabels"
        };

		#endregion spGetDXCCS

		#region spGetDXCCSCategories

		private string[] spGetDXCCSCategories = new[]
        {
            "SELECT Id, Name"
            ,"FROM Base_DXCCSCategories"
        };

		#endregion spGetDXCCSCategories

		#region spGetPRCCS

		private string[] spGetPRCCS = new[]
        {
            "SELECT PRCCSID AS Id, CategoryID, Description"
            ,"FROM Base_PRCCSLabels"
        };

		#endregion spGetPRCCS

		#region spGetPRCCSCategories

		private string[] spGetPRCCSCategories = new[]
        {
            "SELECT Id, Name"
            ,"FROM Base_PRCCSCategories"
        };

		#endregion spGetPRCCSCategories



		#endregion SProcs

		private void OnInstalled()
		{
			// The wing was just added to Monahrq, so start importing the needed base data.

			// Add user defined types
			CreateOrUpdateDbObject("IDsTableType", string.Join(System.Environment.NewLine, idsTableType), "", DataObjectTypeEnum.Type);
			CreateOrUpdateDbObject("UniqueIDsTableType", string.Join(System.Environment.NewLine, uniqueIDsTableType), "", DataObjectTypeEnum.Type);
			CreateOrUpdateDbObject("StringsTableType", string.Join(System.Environment.NewLine, stringsTableType), "", DataObjectTypeEnum.Type);

			// Add tables
			CreateOrUpdateDbObject("Base_StratificationVals", string.Join(System.Environment.NewLine, tableBaseStratificationVals), "", DataObjectTypeEnum.Table);
			CreateOrUpdateDbObject("Base_Ages", string.Join(System.Environment.NewLine, tableBaseAges), "", DataObjectTypeEnum.Table);

			// Add the sprocs
			CreateOrUpdateDbObject("spGetAdmissionSources", string.Join(System.Environment.NewLine, _spGetAdmissionSources), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetAdmissionTypes", string.Join(System.Environment.NewLine, _spGetAdmissionTypes), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spInitializeAgeVals", string.Join(System.Environment.NewLine, spInitializeAgeVals), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetAges", string.Join(System.Environment.NewLine, spGetAges), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetDispositionCodes", string.Join(System.Environment.NewLine, spGetDispositionCodes), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetPayers", string.Join(System.Environment.NewLine, spGetPayers), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetPointOfOrigins", string.Join(System.Environment.NewLine, spGetPointOfOrigins), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetRaces", string.Join(System.Environment.NewLine, spGetRaces), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetSexes", string.Join(System.Environment.NewLine, spGetSexes), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetStratifications", string.Join(System.Environment.NewLine, spGetStratifications), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spInitializeStratificationVals", string.Join(System.Environment.NewLine, spInitializeStratificationVals), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetStratificationVals", string.Join(System.Environment.NewLine, spGetStratificationVals), "", DataObjectTypeEnum.StoredProcedure);

			CreateOrUpdateDbObject("spGetHospitals", string.Join(System.Environment.NewLine, _spGetHospitals), SP_GET_HOSPITALS_PARAMS, DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetHospitalsByState", string.Join(System.Environment.NewLine, spGetHospitalsByState), spGetHospitalsByStateParams, DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetHospitalsByStateAbbreviation", string.Join(System.Environment.NewLine, spGetHospitalsByStateAbbreviation), spGetHospitalsByStateAbbreviationParams, DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetHospitalCounties", string.Join(System.Environment.NewLine, spGetHospitalCounties), spGetHospitalCountiesParams, DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetStateCounties", string.Join(System.Environment.NewLine, spGetStateCounties), spGetStateCountiesParams, DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetHospitalRegions", string.Join(System.Environment.NewLine, spGetHospitalRegions), spGetHospitalRegionsParams, DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetHospitalZips", string.Join(System.Environment.NewLine, spGetHospitalZips), spGetHospitalZipsParams, DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetHospitalTypes", string.Join(System.Environment.NewLine, spGetHospitalTypes), spGetHospitalTypesParams, DataObjectTypeEnum.StoredProcedure);

			// TODO: Need to double check DRG, MDC, PRCCS and add Categories for each of them.
			// TODO: Currently importing current year data only. Need to work on multi-year reporting / DRG labeling.
			CreateOrUpdateDbObject("spGetDRG", string.Join(System.Environment.NewLine, spGetDRG), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetDRGCategories", string.Join(System.Environment.NewLine, spGetDRGCategories), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetMDC", string.Join(System.Environment.NewLine, spGetMDC), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetDXCCS", string.Join(System.Environment.NewLine, spGetDXCCS), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetDXCCSCategories", string.Join(System.Environment.NewLine, spGetDXCCSCategories), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetPRCCS", string.Join(System.Environment.NewLine, spGetPRCCS), "", DataObjectTypeEnum.StoredProcedure);
			CreateOrUpdateDbObject("spGetPRCCSCategories", string.Join(System.Environment.NewLine, spGetPRCCSCategories), "", DataObjectTypeEnum.StoredProcedure);

			CreateOrUpdateDbObject("fnGetCostToChargeRatio", string.Join(System.Environment.NewLine, GET_COST_TO_CHARGE_RATIO_UDF), "", DataObjectTypeEnum.UserDefinedFunction);
			CreateOrUpdateDbObject("fnFormatString", string.Join(System.Environment.NewLine, _fnFormatString), "", DataObjectTypeEnum.UserDefinedFunction);
			CreateOrUpdateDbObject("fnToProperCase", string.Join(System.Environment.NewLine, _fnToProperCase), "", DataObjectTypeEnum.UserDefinedFunction);
			CreateOrUpdateDbObject("fnGetHospitalRegion", string.Join(System.Environment.NewLine, _fnGetHospitalRegion), "", DataObjectTypeEnum.UserDefinedFunction);

			// Initialize data
			RunSproc("spInitializeAgeVals");  // Note: Must be run before initializing stratification values.
			RunSproc("spInitializeStratificationVals");
		}

		public override void InitGenerator()
		{
			// This runs every time the application starts up.
			// TODO: The following should only be run once, but we don't have the infrastructure setup like we do in wings yet.
			OnInstalled();

			EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading supporting database objects for Reports" });
			Application.Current.DoEvents();
		}

		public override bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
		{
			return true;
		}

		public override void GenerateReport(Website website, PublishTask publishTask)
		{
			// This is the one that should be called first.
			try
			{
				base.GenerateReport(website);

				// Initialize the data for this report.
				InitializeReportData();

				// Make sure the base directories are created.
				CreateBaseDirectories();

				// Generate the json files for the report.
				GenerateJsonFiles(publishTask);

				// Generate any HTML files for the report.
				GenerateHtml();
			}
			catch (Exception ex)
			{
				Logger.Write(ex);
			}
		}

		private void InitializeReportData()
		{
			try
			{
				#region Get base information about the website - hospitals, measures, datasets, etc.

				ZipDistances = new DataTable();
				ZipDistances.Columns.Add("Distance", typeof(string));
				foreach (int distance in CurrentWebsite.SelectedZipCodeRadii)
				{
					ZipDistances.Rows.Add(distance.ToString());
				}

				#endregion Get base information about the website - hospitals, measures, datasets, etc.

				#region Generate the specific data for this report.

				// Save a report ID for this particular report run.
				ReportID = Guid.NewGuid().ToString();

				#endregion Generate the specific data for this report.

			}
			catch (Exception ex)
			{
				Logger.Write(ex);
			}
		}

		private void CreateBaseDirectories()
		{
			try
			{
				// Make sure the base directories are created.
				BaseDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "Base");
				if (!Directory.Exists(BaseDataDir)) Directory.CreateDirectory(BaseDataDir);
			}
			catch (Exception ex)
			{
				Logger.Write(ex);
			}
		}

		private void GenerateJsonFiles(PublishTask publishTask = PublishTask.Full)
		{
			try
			{
				IConfigurationService configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
				//GenerateReportsTemplatesJsonFiles();

				//if (publishTask == PublishTask.PreviewOnly)
				//{
				//    return;
				//}
				// Generate the json data files for the labels and other base data.
				// NOTE: This is assuming that the directories are deleted before website generation like 4.1.
				//       We are checking if it exists because it could be created in another report.
				if (!File.Exists(Path.Combine(BaseDataDir, "AdmissionSource.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetAdmissionSources", null), Path.Combine(BaseDataDir, "AdmissionSource.js"), "$.monahrq.AdmissionSource=");

				if (!File.Exists(Path.Combine(BaseDataDir, "AdmissionType.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetAdmissionTypes", null), Path.Combine(BaseDataDir, "AdmissionType.js"), "$.monahrq.AdmissionType=");

				if (!File.Exists(Path.Combine(BaseDataDir, "Age.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetAges", null), Path.Combine(BaseDataDir, "Age.js"), "$.monahrq.Age=");

				if (!File.Exists(Path.Combine(BaseDataDir, "DispositionCode.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetDispositionCodes", null), Path.Combine(BaseDataDir, "DispositionCode.js"), "$.monahrq.DispositionCode=");

				if (!File.Exists(Path.Combine(BaseDataDir, "Payer.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetPayers", null), Path.Combine(BaseDataDir, "Payer.js"), "$.monahrq.Payer=");

				if (!File.Exists(Path.Combine(BaseDataDir, "PointOfOrigin.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetPointOfOrigins", null), Path.Combine(BaseDataDir, "PointOfOrigin.js"), "$.monahrq.PointOfOrigin=");

				if (!File.Exists(Path.Combine(BaseDataDir, "Race.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetRaces", null), Path.Combine(BaseDataDir, "Race.js"), "$.monahrq.Race=");

				if (!File.Exists(Path.Combine(BaseDataDir, "Sex.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetSexes", null), Path.Combine(BaseDataDir, "Sex.js"), "$.monahrq.Sex=");

				if (!File.Exists(Path.Combine(BaseDataDir, "Stratification.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetStratifications", null), Path.Combine(BaseDataDir, "Stratification.js"), "$.monahrq.Stratification=");

				if (!File.Exists(Path.Combine(BaseDataDir, "StratificationVals.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetStratificationVals", null), Path.Combine(BaseDataDir, "StratificationVals.js"), "$.monahrq.StratificationVals=");

				if (!File.Exists(Path.Combine(BaseDataDir, "ZipDistances.js")))
					GenerateJsonFile(ZipDistances, Path.Combine(BaseDataDir, "ZipDistances.js"), "$.monahrq.ZipDistances=");

				var selectedRegionContextType = new KeyValuePair<string, object>("@RegionType", CurrentWebsite.RegionTypeContext);

				// Export the hospital information.
				Action<DataTable> dtAction = (DataTable dt) =>
					{
						dt.Columns.Add("LatLng", typeof(Double[]));
						foreach (DataRow row in dt.Rows)
						{
							row["LatLng"] = new Double[] { row["Latitude"].AsDouble(0), row["Longitude"].AsDouble(0) };
						}
						dt.Columns.Remove("Latitude");
						dt.Columns.Remove("Longitude");
					};
				if (!File.Exists(Path.Combine(BaseDataDir, "Hospitals.js")))
					GenerateJsonFile(RunSprocReturnDataTableWithAction("spGetHospitals", dtAction, new KeyValuePair<string, object>("@Hospitals", HospitalIds), selectedRegionContextType), Path.Combine(BaseDataDir, "Hospitals.js"), "$.monahrq.Hospitals=");

				if (!File.Exists(Path.Combine(BaseDataDir, "HospitalCounties.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetStateCounties", new KeyValuePair<string, object>("@States", StateIds)), Path.Combine(BaseDataDir, "HospitalCounties.js"), "$.monahrq.HospitalCounties=");
				//GenerateJsonFile(RunSprocReturnDataTable("spGetHospitalCounties", new KeyValuePair<string, object>("@Hospitals", HospitalIds)), Path.Combine(BaseDataDir, "HospitalCounties.js"), "$.monahrq.HospitalCounties=");

				if (!File.Exists(Path.Combine(BaseDataDir, "HospitalRegions.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetHospitalRegions", new KeyValuePair<string, object>("@Hospitals", HospitalIds), selectedRegionContextType), Path.Combine(BaseDataDir, "HospitalRegions.js"), "$.monahrq.HospitalRegions=");

				if (!File.Exists(Path.Combine(BaseDataDir, "HospitalZips.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetHospitalZips", new KeyValuePair<string, object>("@States", StateIds)), Path.Combine(BaseDataDir, "HospitalZips.js"), "$.monahrq.HospitalZips=");

				if (!File.Exists(Path.Combine(BaseDataDir, "HospitalTypes.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetHospitalTypes", new KeyValuePair<string, object>("@Hospitals", HospitalIds)), Path.Combine(BaseDataDir, "HospitalTypes.js"), "$.monahrq.HospitalTypes=");

				// Export the clinical dimensions.
				if (!File.Exists(Path.Combine(BaseDataDir, "DRG.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetDRG", null), Path.Combine(BaseDataDir, "DRG.js"), "$.monahrq.drg=");

				if (!File.Exists(Path.Combine(BaseDataDir, "DRGCategories.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetDRGCategories", null), Path.Combine(BaseDataDir, "DRGCategories.js"), "$.monahrq.drgcategories=");

				if (!File.Exists(Path.Combine(BaseDataDir, "MDC.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetMDC", null), Path.Combine(BaseDataDir, "MDC.js"), "$.monahrq.mdc=");

				if (!File.Exists(Path.Combine(BaseDataDir, "DXCCS.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetDXCCS", null), Path.Combine(BaseDataDir, "DXCCS.js"), "$.monahrq.dxccs=");

				if (!File.Exists(Path.Combine(BaseDataDir, "DXCCSCategories.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetDXCCSCategories", null), Path.Combine(BaseDataDir, "DXCCSCategories.js"), "$.monahrq.dxccscategories=");

				if (!File.Exists(Path.Combine(BaseDataDir, "PRCCS.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetPRCCS", null), Path.Combine(BaseDataDir, "PRCCS.js"), "$.monahrq.prccs=");

				if (!File.Exists(Path.Combine(BaseDataDir, "PRCCSCategories.js")))
					GenerateJsonFile(RunSprocReturnDataTable("spGetPRCCSCategories", null), Path.Combine(BaseDataDir, "PRCCSCategories.js"), "$.monahrq.prccscategories=");

				//GenerateHospitalProfileJsonFiles();

				GeneratePatientRegionAndCountyJsonFiles();

			}
			catch (Exception ex)
			{
				Logger.Write(ex);
			}
		}

		#region Generate Patient Methods.
		private void GeneratePatientRegionAndCountyJsonFiles()
		{
			DataTable ipDatasetIDs = new DataTable();
			ipDatasetIDs.Columns.Add("ID", typeof(int));
			var ipDatasetId = new List<int>();
			foreach (WebsiteDataset dataSet in CurrentWebsite.Datasets)
			{
				if (!dataSet.Dataset.ContentType.Name.EqualsIgnoreCase("Inpatient Discharge"))
					continue;

				// Add a new IP dataset
				ipDatasetId.Add(dataSet.Dataset.Id);
				ipDatasetIDs.Rows.Add(dataSet.Dataset.Id);
			}

			if (ipDatasetIDs.Rows.Count > 0)
			{
                // Logger.Write("Report DataSet is missing.");
                if (!File.Exists(Path.Combine(BaseDataDir, "PatientRegions.js")))
                    GenerateJsonFile(RunSprocReturnDataTable("spUtilRegionGetTargetRegions",
                                                             new KeyValuePair<string, object>("@IPDataset", ipDatasetIDs),
                                                             new KeyValuePair<string, object>("@RegionType", CurrentWebsite.RegionTypeContext)
                                         ),
                                     Path.Combine(BaseDataDir, "PatientRegions.js"), "$.monahrq.PatientRegions=");
            }

			if (ipDatasetId.Any())
			{
				GeneratePatientCountiesJson(ipDatasetId.ToArray());
			}
		}

		private void GeneratePatientCountiesJson(int[] ipDatasetId)
		{
			if (File.Exists(Path.Combine(BaseDataDir, "PatientCounties.js"))) return;

			IList<PatientCounty> patientCounties = new List<PatientCounty>();

			using (var session = DataProvider.SessionFactory.OpenSession())
			{

				var query = session.CreateQuery("select distinct cast(c.Id as int) as Id, concat(c.Name, ', ', c.State) as Name, cast(c.CountyFIPS as string) as CountyFIPS from County c, InpatientTarget ip where ip.PatientStateCountyCode = c.CountyFIPS and ip.Dataset.Id in (:Dataset_Ids)");
				patientCounties = query.SetParameterList("Dataset_Ids", ipDatasetId)
									   .SetTimeout(5000)
									   .SetResultTransformer(Transformers.AliasToBean<PatientCounty>())
									   .List<PatientCounty>();

			}

			if (patientCounties != null)
			{
				patientCounties.Insert(0, new PatientCounty { Id = -1, Name = "Unknown", CountyFIPS = "0" });
			}

			GenerateJsonFile(patientCounties ?? new List<PatientCounty>(), Path.Combine(BaseDataDir, "PatientCounties.js"),
							 "$.monahrq.PatientCounties=");
		}
		#endregion

		//public void GenerateReportsTemplatesJsonFiles()
		//{

		//    var fileName = Path.Combine(base.BaseDataDirectoryPath, string.Format("ReportConfig.js"));
		//    var result = new List<Object>();
		//    foreach (var report in CurrentWebsite.Reports)
		//    {
		//        //todo : refactor to mapping table
		//        List<string> Display =new List<string>();
		//        string measuresList="";
		//        switch (report.Report.SourceTemplate.Id.ToString())
		//        {
		//            case "5aaf7fba-7102-4c66-8598-a70597e2f826":                      // TODO: Need to change measure names to IP-## if uncomment this section and use again.
		//                measuresList = "IP-8, IP-9, IP-10, IP-11,";
		//                break;
		//            case "2aaf7fba-7102-4c66-8598-a70597e2f827":
		//                measuresList = "ED-1, ED-2, ED-3, ED-4,";
		//                break;
		//            case "2aaf7fba-7102-4c66-8598-a70597e2f824":
		//                measuresList = "IP-1, IP-2, IP-3, IP-4, IP-5, IP-6, IP-7,";
		//                break;
		//            case "2aaf7fba-7102-4c66-8598-a70597e2f825":
		//                measuresList = "IP-1, IP-2, IP-3, IP-4, IP-5, IP-6, IP-7,";
		//                break;
		//            case "5aaf7fba-7102-4c66-8598-a70597e2f825":
		//                measuresList = "IP-8, IP-9, IP-10, IP-11,";
		//                break;
		//            case "2aaf7fba-7102-4c66-8598-a70597e2f828":
		//                measuresList = "ED-1, ED-2, ED-3, ED-4,";
		//                break;
		//            case "7af51434-5745-4538-b972-193f58e737d7":
		//                Display.Add("Basic Descriptive Data");
		//                Display.Add("Cost and Charge Data (All Patients)");
		//                Display.Add("Cost and Charge Data (Medicare)");
		//                Display.Add("Map");
		//                Display.Add("Overall Patient Experience Rating");
		//                break;
		//        }

		//        var reportMeasures = CurrentWebsite.Measures.Where(m => measuresList.Contains(m.OriginalMeasure.MeasureCode+","));
		//        List<object> columnList=new List<object>();
		//        foreach (var rv in reportMeasures)
		//        {
		//            string DataElementLink = "";
		//            string DataFormat = "";
		//            switch (rv.OriginalMeasure.MeasureCode)
		//            {
		//                case "ED-1":
		//                    DataElementLink = "NumEdVisits";
		//                    DataFormat = "number";
		//                    break;
		//                case "ED-2":
		//                    DataElementLink = "NumAdmitHosp";
		//                    DataFormat = "number";
		//                    break;
		//                case "ED-3":
		//                    DataElementLink = "DiedEd";
		//                    DataFormat = "number";
		//                    break;
		//                case "ED-4":
		//                    DataElementLink = "DiedHosp";
		//                    DataFormat = "number";
		//                    break;
		//                case "IP-1":
		//                    DataElementLink = "Discharges";
		//                    DataFormat = "number";
		//                    break;
		//                case "IP-2":
		//                    DataElementLink = "MeanCharges";
		//                    DataFormat = "nfcurrency";
		//                    break;
		//                case "IP-3":
		//                    DataElementLink = "MeanCosts";
		//                    DataFormat = "nfcurrency";
		//                    break;
		//                case "IP-4":
		//                    DataElementLink = "MeanLOS";
		//                    DataFormat = "number";
		//                    break;
		//                case "IP-5":
		//                    DataElementLink = "MedianCharges";
		//                    DataFormat = "nfcurrency";
		//                    break;
		//                case "IP-6":
		//                    DataElementLink = "MedianCosts";
		//                    DataFormat = "nfcurrency";
		//                    break;
		//                case "IP-7":
		//                    DataElementLink = "MedianLOS";
		//                    DataFormat = "number";
		//                    break;
		//                case "IP-8":
		//                    DataElementLink = "Discharges";
		//                    DataFormat = "number";
		//                    break;
		//                case "IP-9":
		//                    DataElementLink = "MeanCosts";
		//                    DataFormat = "nfcurrency";
		//                    break;
		//                case "IP-10":
		//                    DataElementLink = "MedianCosts";
		//                    DataFormat = "nfcurrency";
		//                    break;
		//                case "IP-11":
		//                    DataElementLink = "RateDischarges";
		//                    DataFormat = "number";
		//                    break;


		//            }
		//            columnList.Add(new { Name = rv.OriginalMeasure.Description, DataElementLink = DataElementLink, DataFormat = DataFormat });

		//        }




		//        result.Add(new {
		//            ID = report.Report.SourceTemplate.Id,
		//            //ID = report.Report.Id,
		//            TYPE=report.Report.Name,
		//          //  IsActive=true,
		//            ReportHeader = "",
		//            ReportFooter= "",

		//            GeoInfo= report.Report.FilterItems
		//                                                    .Where(c => c.Value == ReportFilter.HospitalFilters || c.Value == ReportFilter.CountyFilters)
		//                                                    .Select(c => c.Caption).ToArray(),


		//            ClinicalDRGAndDiagnosis =  report.Report.FilterItems
		//                                                    .Where(c => c.Value == ReportFilter.DRGsDischargesFilters || c.Value ==ReportFilter.ConditionsAndDiagnosisFilters)
		//                                                    .Select(c => c.Caption).ToArray(),
		//            Display=Display.ToArray(),

		//            IncludedColumns = columnList,
		//            ShowInterpretationFlag = report.Report.ShowInterpretationText,
		//            InterpretationHTMLDescription =report.Report.InterpretationText,


		//        });
		//    }

		//    GenerateJsonFile(result, fileName, "$.monahrq.ReportConfig=");
		//}

		//private void GenerateHospitalProfileJsonFiles()
		//{

		//}

		private void GenerateHtml()
		{

		}

		protected override bool LoadReportData()
		{
			return true;
		}

		protected override bool OutputDataFiles()
		{
			return true;
		}
	}

	[DataContract(Name = "")]
	internal class HospitalCategoryType
	{
		[DataMember(Name = "type_Id")]
		public int Id { get; set; }
		[DataMember(Name = "type_Name")]
		public string Name { get; set; }
	}

	[DataContract(Name = "")]
	internal class PatientCounty
	{
		[DataMember(Name = "CountyID")]
		public int Id { get; set; }
		[DataMember(Name = "CountyName")]
		public string Name { get; set; }
		[DataMember(Name = "FIPS")]
		public string CountyFIPS { get; set; }
	}
}