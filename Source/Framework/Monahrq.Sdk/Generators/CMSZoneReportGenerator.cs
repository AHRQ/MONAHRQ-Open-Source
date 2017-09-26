
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Configuration;
using NHibernate.Transform;

namespace Monahrq.Sdk.Generators
{
    /// <summary>
    /// The Static Page / Dynamic Report Page Generator. This will take the user
    /// edits and generate Json data to place in predefined zones in the generated website.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
    /// <seealso cref="Monahrq.Sdk.Generators.IReportGenerator" />
    [Export(//"CMSZoneReportGenerator", 
		typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.NonShared)]
	[ReportGenerator(
		null,	//new[] { "9FDA51A1-42E9-465D-96D8-0CEFF6048D5C" },
		new [] { "CMS Zone Report", "Base Data" },
		null,	//new[] { typeof(object) },
		0)]
	public class CMSZoneReportGenerator : BaseReportGenerator , IReportGenerator
	{
        #region Variables.
        /// <summary>
        /// Gets or sets the base data directory.
        /// </summary>
        /// <value>
        /// The base data directory.
        /// </value>
        private string BaseDataDir { get; set; }
        /// <summary>
        /// The primary T-Sql used to retrieve the website pages to be used in the json data output.
        /// </summary>
        private static string sqlGetWebsitePages = @"
					select			wpz.Id [ZoneId]
								,	case
										when (wpz.ZoneType = 'Page') then 'page_template'
										when (wpz.ZoneType = 'Zone') then 'page_zone'
										else 'page_X'
									end as [ZoneType]
								,	wp.PageType as [ZonePageType]
								,	case
										when (wp.PageType = 'Report') then ''
										when (wp.PageType = 'Static') then bwp.TemplateRelativePath
										else 'page_zone'
									end as [ZoneValuePath]
								,	case
										when (wp.PageType = 'Static') then
											case
												when (wpz.Audience = 'Consumers') then 'consumer'
												when (wpz.Audience = 'Professionals') then 'professional'
											end
										when (wp.PageType = 'Report') then
											case
												when (wpz.Audience = 'Consumers') then 'Consumer'
												when (wpz.Audience = 'Professionals') then 'Professional'
											end
									end as [ZoneValueProduct]
								,	wp.ReportName as [ZoneValueReportName]
								,	'' as [ZoneValueReportGUID]										--	Not in DB, has to be retrieved from ReportManifest.
								,	wpz.Name as [ZoneValueZoneName]
								,	wpz.Contents as [ZoneValueTemplateContent]
					from			Websites_WebsitePages wp
						left join	Base_WebsitePages bwp on bwp.Name = wp.Name						--	Some 'base' data is stored in ReportManifest, not DB.
						inner join	Websites_WebsitePageZones wpz on wpz.WebsitePage_Id = wp.Id
						left join	Base_WebsitePageZones bwpz										--	Some 'base' data is stored in ReportManifest, not DB.
										on	bwpz.Name = wpz.Name
										and	bwpz.WebsitePageName = wp.Name
						left join	Reports r on r.Name = wp.ReportName
					where			wp.Website_Id = :websiteID
						and	(		wp.PageType = 'Static'
								or	r.Id in (:applicableReportIds)
							)";
        /// <summary>
        /// The secondary T-Sql used to retrieve the website pages to be used in the json data output.
        /// </summary>
        private static string sqlGetWebsitePagesX = @"
					select			wpz.Id [ZoneId]
								,	case
										when (wpz.ZoneType = 'Page') then 'page_template'
										when (wpz.ZoneType = 'Zone') then 'page_zone'
										else 'page_X'
									end as [z.Type]
								,	wp.PageType as [z.PageType]
								,	case
										when (wp.PageType = 'Report') then ''
										when (wp.PageType = 'Static') then bwp.TemplateRelativePath
										else 'page_zone'
									end as [zv.Path]
								,	case
										when (wp.PageType = 'Static') then
											case
												when (wpz.Audience = 'Consumers') then 'consumer'
												when (wpz.Audience = 'Professionals') then 'professional'
											end
										when (wp.PageType = 'Report') then
											case
												when (wpz.Audience = 'Consumers') then 'Consumer'
												when (wpz.Audience = 'Professionals') then 'Professional'
											end
									end as [zv.Product]
								,	wp.ReportName as [zv.ReportName]
								,	'' as [zv.ReportGUID]											--	Not in DB, has to be retrieved from ReportManifest.
								,	wpz.Name as [zv.ZoneName]
								,	wpz.Contents as [zv.TemplateContent]
					from			Websites_WebsitePages wp
						left join	Base_WebsitePages bwp on bwp.Name = wp.Name						--	Some 'base' data is stored in ReportManifest, not DB.
						inner join	Websites_WebsitePageZones wpz on wpz.WebsitePage_Id = wp.Id
						left join	Base_WebsitePageZones bwpz										--	Some 'base' data is stored in ReportManifest, not DB.
										on	bwpz.Name = wpz.Name
										and	bwpz.WebsitePageName = wp.Name
						left join	Reports r on r.Name = wp.ReportName
					where			wp.Website_Id = :websiteID
						and	(		wp.PageType = 'Static'
								or	r.Id in (:applicableReportIds)
							)";
        #endregion

        #region Methods.
        #region Constructors.
        /// <summary>
        /// Initializes a new instance of the <see cref="CMSZoneReportGenerator"/> class.
        /// </summary>
        /// <param name="sessionFactoryProvider">The session factory provider.</param>
        /// <param name="configurationService">The configuration service.</param>
        [ImportingConstructor]
		public CMSZoneReportGenerator(
			IDomainSessionFactoryProvider sessionFactoryProvider,
			IConfigurationService configurationService)
		{ }
        #endregion

        /// <summary>
        /// Loads the report data needed for the data output.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Something seriously went wrong. The reporting website can not be null.</exception>
        protected override bool LoadReportData()
		{
			try
			{
				if (CurrentWebsite == null)
					throw new InvalidOperationException("Something seriously went wrong. The reporting website can not be null.");

				// Make sure the base directories are created.
				BaseDataDir = Path.Combine(CurrentWebsite.OutPutDirectory, "Data", "Base");
				if (!Directory.Exists(BaseDataDir)) Directory.CreateDirectory(BaseDataDir);

				return true;
			}
			catch (Exception ex)
			{
			    Logger.Write(ex, "Error loading data for report {0}", this.ActiveReport.Name);
				return false;
			}
		}
        /// <summary>
        /// Outputs the report data files.
        /// </summary>
        /// <returns></returns>
        protected override bool OutputDataFiles()
		{
			try
			{
				var fileName = Path.Combine(BaseDataDir, string.Format("CMSEntities.js"));
				return OutputDataFilesEx(fileName);
			}
			catch (Exception ex)
			{
                Logger.Write(ex, "Error generating output data file for report {0}", this.ActiveReport.Name);
                return false;
			}
		}
        /// <summary>
        /// Outputs the data files ex.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public bool OutputDataFilesEx(String filePath)
		{
			try
			{
				var zones = GetCMSZones(DataProvider, CurrentWebsite);

			//	if (zones.Any())
				{

					if (File.Exists(filePath))
						File.Delete(filePath);

				//	GenerateJsonFile(zones, fileName, "$.monahrq.cms=");
					GenerateJsonFile(zones, filePath, "$.monahrq.cms.entities=");
				}
			}
			catch (Exception ex)
			{
                Logger.Write(ex, "Error generating detailed output data file for report {0}", this.ActiveReport.Name);
                return false;
			}
			return true;
		}
        /// <summary>
        /// Initializes any data objects needed by the report generator while executing. This method is call during the application initialization/bootstrap
        /// </summary>
        public override void InitGenerator()
		{
			EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading supporting database objects for WebsitePage Zone reports." });

		}
        /// <summary>
        /// Validates the report(S) dependencies needed to generate the report.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="validationResults">The validation results.</param>
        /// <returns></returns>
        public override bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
		{
			if (base.ValidateDependencies(website, validationResults)) ;

			return true;


			//if (base.ValidateDependencies(website, validationResults))
			//{
			//	//var profileRpt = website.Reports.FirstOrDefault(r => r.Report.SourceTemplate.Name.EqualsIgnoreCase("Hospital Profile Report"));
			//
			//	if (website.Hospitals == null || !website.Hospitals.Any())
			//	{
			//		validationResults.Add(new ValidationResult("The hospital profile base data was not generated due to no hospitals selected for website \"" + CurrentWebsite.Name + "\""));
			//	}
			//}
			//
			//return validationResults == null || validationResults.Count == 0;
		}
        /// <summary>
        /// Gets the CMS zones.
        /// </summary>
        /// <param name="DataProvider">The data provider.</param>
        /// <param name="CurrentWebsite">The current website.</param>
        /// <returns></returns>
        private static IList<CMSZone> GetCMSZones(IDomainSessionFactoryProvider DataProvider, Website CurrentWebsite)
		{
			IList<CMSZone> zones;
			List<int> applicableReportIds = CurrentWebsite.Reports
										   .Select(wr => wr.Report.Id).ToList();

			//	Retrieve CMS Zone data from DB.
			using (var session = DataProvider.SessionFactory.OpenSession())
			{

				//try
				//{
				//	var q2 = (session
				//		.CreateSQLQuery(sqlGetWebsitePagesX)
				//		.AddEntity("z", typeof(CMSZone))
				//		.SetResultTransformer(new DistinctRootEntityResultTransformer()) as NHibernate.ISQLQuery)
				//		.AddJoin("zv", "z.ZoneValue")
				//		.SetParameter("websiteID", CurrentWebsite.Id)
				//		.SetParameterList("applicableReportIds", applicableReportIds); ;
				//
				//	var zone2 = q2.List<CMSZone>();
				//}
				//catch (Exception ex)
				//{
				//	ex.GetType();
				//}

				var query = session
					.CreateSQLQuery(sqlGetWebsitePages)
					.SetResultTransformer(Transformers.AliasToBean(typeof(CMSZoneDTO)))
					.SetParameter("websiteID", CurrentWebsite.Id)
					.SetParameterList("applicableReportIds", applicableReportIds);
				zones = query
					.List<CMSZoneDTO>()
					.Select(zoneDTO => new CMSZone()
					{
						Id = zoneDTO.ZoneId,
						Type = zoneDTO.ZoneType,
						PageType = zoneDTO.ZonePageType,
						CMSZoneValue = new CMSZoneValue()
						{
							Path = zoneDTO.ZoneValuePath.Replace("\\", "/"),
							Product = zoneDTO.ZoneValueProduct,
							ReportName = zoneDTO.ZoneValueReportName,
							ReportGUID = zoneDTO.ZoneValueReportGUID.ToLower(),
							ZoneName = zoneDTO.ZoneValueZoneName.ToLower(),
							TemplateContent = zoneDTO.ZoneValueTemplateContent,
						},
					}).ToList<CMSZone>();
			}

			//	Fill in CMS Zone objects with data from ReportManifest.
			foreach (var zone in zones)
			{
				if (zone.PageType == "Report")
				{
					var report = CurrentWebsite.Reports.Where(r => r.Report.Name == zone.CMSZoneValue.ReportName).FirstOrDefault();
					if (report == null)
					{
						//throw new InvalidOperationException("Something seriously went wrong. The reporting website can not be null.");
						continue;
					}
					zone.CMSZoneValue.ReportGUID = report.Report.SourceTemplate.RptId.ToLower();
				}
			}

			return zones;
		}
		#endregion
	}

	internal class CMSZoneDTO
	{
		public int ZoneId { get; set; }
		public string ZoneType { get; set; }
		public string ZonePageType { get; set; }
		public string ZoneValuePath { get; set; }
		public string ZoneValueProduct { get; set; }
		public string ZoneValueReportName { get; set; }
		public string ZoneValueReportGUID { get; set; }
		public string ZoneValueZoneName { get; set; }
		public string ZoneValueTemplateContent { get; set; }
	}

	[DataContract(Name = "")]
	internal class CMSZone
	{
		#region Properties.

		[DataMember(Name = "id")]
		public int Id { get; set; }


		[DataMember(Name = "type")]
		public String Type { get; set; }

		[IgnoreDataMember()]
		public String PageType { get; set; }


		[DataMember(Name = "value")]
		public CMSZoneValue CMSZoneValue { get; set; }

		#endregion
	}
	[DataContract(Name = "")]
	internal class CMSZoneValue
	{
		#region Properties.
		[DataMember(Name = "path")]
		public string Path { get; set; }

		[DataMember(Name = "product")]
		public string/*Audience*/ Product { get; set; }

		[IgnoreDataMember()]
		public string ReportName { get; set; }

		[DataMember(Name = "report_id")]
		public string ReportGUID { get; set; }

		[DataMember(Name = "zone")]
		public string ZoneName { get; set; }

		[DataMember(Name = "template")]
		public string TemplateContent { get; set; }
		#endregion

		#region Methods.
		internal CMSZoneValue()
		{ }
		#endregion
	}
}
