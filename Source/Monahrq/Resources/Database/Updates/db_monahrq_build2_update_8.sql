-- =============================================
-- Author:		Shafiul Alam
-- Project:		MONAHRQ 5.0 Build 2
-- Create date: 08-07-2014
-- Description:	This is the update script from older MONAHRQ 5.0 Build 1 AHRQ Targets to the new 
--              MONAHRQ 5.0 Build 2 database schema.
--				'Measures Data'
-- =============================================

--BEGIN TRY

-- Check whether Source database is in DB version 1 as build 1 Db tables 

DECLARE @IsDBVersion1 BIT =0
IF OBJECT_ID('[@@SOURCE@@].[dbo].SchemaVersions') IS NULL
SET @IsDBVersion1=1

/******************************************************************
 *  Create table to preserve Current and New Measure Id Mapping.
 *********************************************************************/
 
IF OBJECT_ID('[@@DESTINATION@@].[dbo].VersionToVersionMeasureMappingTemp') IS NOT NULL
DROP TABLE [@@DESTINATION@@].[dbo].VersionToVersionMeasureMappingTemp

SELECT m.Id       CurrentMeasureId,
       n.Id       NewMeasureId,
       m.OrigincalCurrentMeasure,
       m.name     CurrentMeasure,
       n.name     NewMeasure,
       m.IsOverRide,
       m.WingFeatureName CurrentWingFeatureName,
       CASE WHEN @IsDBVersion1=1 THEN 0 ELSE m.IsLibEdit END IsLibEdit,
       0          NewOverriddenMeasureId
INTO [@@DESTINATION@@].dbo.VersionToVersionMeasureMappingTemp 
from
	(SELECT m.Id ,m.Name OrigincalCurrentMeasure,
			CASE WHEN RTRIM(LTRIM(m.Name)) LIKE 'IQI Cond%' AND m.MeasureType='QIcomposite' AND m.Source='AHRQ' THEN 'IQI 91'
			WHEN RTRIM(LTRIM(m.Name)) LIKE 'IQI Proc%' AND m.MeasureType='QIcomposite' AND m.Source='AHRQ' THEN 'IQI 90'
			WHEN RTRIM(LTRIM(m.Name)) LIKE 'PSI Comp%' AND m.MeasureType='QIcomposite' AND m.Source='AHRQ' THEN 'PSI 90'
			WHEN RTRIM(LTRIM(m.Name)) LIKE '30DAY_MORT_HA%' AND m.MeasureType='Outcome' AND m.Source='CMS' THEN 'MORT-30-AMI'
			WHEN RTRIM(LTRIM(m.Name)) LIKE '30DAY_MORT_HF%' AND m.MeasureType='Outcome' AND m.Source='CMS' THEN 'MORT-30-HF'
			WHEN RTRIM(LTRIM(m.Name)) LIKE '30DAY_MORT_PN%' AND m.MeasureType='Outcome' AND m.Source='CMS' THEN 'MORT-30-PN'
			WHEN RTRIM(LTRIM(m.Name)) LIKE '30DAY_READM_HA%' AND m.MeasureType='Outcome' AND m.Source='CMS' THEN 'READM-30-AMI'
			WHEN RTRIM(LTRIM(m.Name)) LIKE '30DAY_READM_HF%' AND m.MeasureType='Outcome' AND m.Source='CMS' THEN 'READM-30-HF'
			WHEN RTRIM(LTRIM(m.Name)) LIKE '30DAY_READM_PN%' AND m.MeasureType='Outcome' AND m.Source='CMS' THEN 'READM-30-PN'
			WHEN RTRIM(LTRIM(m.Name)) LIKE 'HAI-1-SIR%' AND m.MeasureType='Ratio' AND m.Source='CMS' THEN 'HAI-1'
			WHEN RTRIM(LTRIM(m.Name)) LIKE 'OP-3%' AND m.MeasureType='Process' AND m.Source='CMS' THEN 'OP-3b'
			WHEN RTRIM(LTRIM(m.Name)) LIKE 'PN-2%' AND m.MeasureType='Process' AND m.Source='CMS' THEN 'IMM-1a'
			WHEN (RTRIM(LTRIM(m.Name)) LIKE 'H_COMP_%' 
					OR RTRIM(LTRIM(m.Name)) LIKE 'H_CLEAN_HSP%' 
						OR RTRIM(LTRIM(m.Name)) LIKE 'H_QUIET_HSP%') 
					AND m.MeasureType='Categorical' AND m.Source='CMS' THEN REPLACE(REPLACE(m.Name,'_A_P',''),'_','-')
			WHEN RTRIM(LTRIM(m.Name)) LIKE 'H_COMP_6_Y_P%' AND m.MeasureType='Binary' AND m.Source='CMS' THEN 'H-COMP-6'
			WHEN RTRIM(LTRIM(m.Name)) LIKE 'H_RECMND_DY%' AND m.MeasureType='YNM' AND m.Source='CMS' THEN 'H-RECMND'
			WHEN RTRIM(LTRIM(m.Name)) LIKE 'H_CLEAN_HSP_A_P%' AND m.MeasureType='Categorical' AND m.Source='CMS' THEN 'H-CLEAN-HSP'
			WHEN RTRIM(LTRIM(m.Name)) LIKE 'H_HSP_RATING_9_10%' AND m.MeasureType='Scale' AND m.Source='CMS' THEN 'H-HSP-RATING'
			WHEN RTRIM(LTRIM(m.Name)) LIKE 'ED-%' AND m.MeasureType IS NULL AND m.Source IS NULL AND LEN(RTRIM(LTRIM(m.Name)))=4 THEN REPLACE(m.Name,'ED-','ED-0')
			WHEN RTRIM(LTRIM(m.Name)) LIKE 'IP-%' AND m.MeasureType IS NULL AND m.Source IS NULL AND LEN(RTRIM(LTRIM(m.Name)))=4 THEN REPLACE(m.Name,'IP-','IP-0') 
			ELSE  RTRIM(LTRIM(REPLACE(m.Name,'_','-'))) END NAME,
			RTRIM(LTRIM(COALESCE(m.MeasureType,''))) MeasureType,
			RTRIM(LTRIM(COALESCE(m.Source,''))) Source,
			RTRIM(LTRIM(COALESCE(w.Name,''))) WingFeatureName,
			IsOverRide,
			IsLibEdit
  FROM [@@SOURCE@@].dbo.Measures m
  INNER JOIN [@@SOURCE@@].dbo.Wings_Targets wt
                           ON  m.Target_id = wt.Feature_id
                      INNER JOIN [@@SOURCE@@].dbo.Wings_Features w
                           ON  w.Id = wt.Feature_id) m 
  LEFT JOIN (SELECT n.Id,
					 RTRIM(LTRIM(n.Name)) NAME,
					 RTRIM(LTRIM(COALESCE(n.MeasureType,''))) MeasureType,
					 RTRIM(LTRIM(COALESCE(n.Source,'')))  Source,
					 RTRIM(LTRIM(COALESCE(w.Name,''))) WingFeatureName
              FROM [@@DESTINATION@@].dbo.Measures n
			  INNER JOIN [@@DESTINATION@@].dbo.Wings_Targets wt
                           ON  n.Target_id = wt.Feature_id
                      INNER JOIN [@@DESTINATION@@].dbo.Wings_Features w
                           ON  w.Id = wt.Feature_id) n
  ON m.Name=n.Name
  AND m.MeasureType=n.MeasureType
  AND m.Source=n.Source
  AND m.WingFeatureName=n.WingFeatureName
  
	DECLARE @MeasureId INT,
			@Measure NVARCHAR(100),
	        @MeasureType NVARCHAR(100),
	        @Source NVARCHAR(100),
	        @WingFetureName NVARCHAR(150),
	        @IsOverride BIT;
	DECLARE @Dataset_Cursor CURSOR;
	
	DECLARE @NewMeasureId INT,@NewTopicId INT

	SET @Dataset_Cursor =  CURSOR FOR
	/********************************************************************
	 *  Get all measures overwridden by user in context of an website
	 *****************************************************************/
	SELECT m.Id,m.Name Measure,
	       m.MeasureType,m.Source
	  FROM [@@SOURCE@@].dbo.Measures m
	WHERE m.IsOverride=1
	
	OPEN @Dataset_Cursor;
	FETCH NEXT FROM @Dataset_Cursor INTO  @MeasureId,@Measure, @MeasureType, @Source

	WHILE @@fetch_status = 0
	BEGIN

		PRINT 'Measure: ' + @Measure;
		PRINT 'Measure Id: '+CAST(@MeasureId AS NVARCHAR(20));
		PRINT 'MeasureType: ' + @MeasureType;
		PRINT 'Source: ' + @Source;
		Print char(13);
		
	/**************************************************************************
	 *   Handle Overridden Measure which are midified under context of web site
	 *   Note-
	 *   If above type measure is removed in Build 2 then all "Title"
	 *   field value will concatenated with "Deprecated" string.
	 **************************************************************************/
		
		INSERT INTO [@@DESTINATION@@].dbo.Measures
		(
			Name,MeasureType,Source,[Description],MoreInformation,Footnotes,NationalBenchmark,HigherScoresAreBetter,ScaleBy,
			ScaleTarget,RiskAdjustedMethod,RateLabel,NQFEndorsed,NQFID,SuppressionDenominator,SuppressionNumerator,PerformMarginSuppression,
			UpperBound,LowerBound,[Url],UrlTitle,IsOverride,IsLibEdit,Target_id,ClinicalTitle,PlainTitle,PolicyTitle,SelectedTitle,ProvidedBenchmark,CalculationMethod
		)
		SELECT
		    CASE WHEN v.NewMeasureId IS NOT NULL THEN m2.Name
					  ELSE m.Name END,
			CASE WHEN v.NewMeasureId IS NOT NULL THEN m2.MeasureType
					  ELSE m.MeasureType END,
			CASE WHEN v.NewMeasureId IS NOT NULL THEN m2.Source
					  ELSE m.Source END,
			m.[Description],
			m.MoreInformation,
			m.Footnotes,
			m.NationalBenchmark,
			m.HigherScoresAreBetter,
			m.ScaleBy,
			m.ScaleTarget,
			m.RiskAdjustedMethod,
			m.RateLabel,
			m.NQFEndorsed,
			CASE WHEN v.NewMeasureId IS NOT NULL THEN m2.NQFID
					  ELSE m.NQFID END,
			m.SuppressionDenominator,
			m.SuppressionNumerator,
			m.PerformMarginSuppression,
			m.UpperBound,
			m.LowerBound,
			m.[Url],
			m.UrlTitle,
			m.IsOverride,
			v.IsLibEdit IsLibEdit,
			CASE WHEN v.NewMeasureId IS NOT NULL THEN m2.Target_id
			     ELSE (
			              SELECT wf.Id
			              FROM   [@@DESTINATION@@].dbo.Wings_Features wf
			              WHERE  wf.Name = v.CurrentWingFeatureName
			          )
			END Target_id,
			CASE WHEN v.NewMeasureId IS NOT NULL THEN m.ClinicalTitle
			ELSE m.ClinicalTitle+' [Deprecated]' END ClinicalTitle,
			CASE WHEN v.NewMeasureId IS NOT NULL THEN m.PlainTitle
			ELSE m.PlainTitle+' [Deprecated]' END PlainTitle,
			CASE WHEN v.NewMeasureId IS NOT NULL THEN m.PolicyTitle
			ELSE m.PolicyTitle+' [Deprecated]' END PolicyTitle,
			CASE WHEN v.NewMeasureId IS NOT NULL THEN m.SelectedTitle
			ELSE m.SelectedTitle+' [Deprecated]' END SelectedTitle,
			m.ProvidedBenchmark,
			m.CalculationMethod
		FROM [@@SOURCE@@].dbo.[Measures] m
		      INNER JOIN [@@DESTINATION@@].dbo.VersionToVersionMeasureMappingTemp v
		      ON m.Id=v.CurrentMeasureId
		      LEFT JOIN (SELECT * 
		                 FROM [@@DESTINATION@@].dbo.Measures
		                 WHERE IsOverride=0) m2
		      ON COALESCE(v.NewMeasureId,0)=COALESCE(m2.Id,0)
		WHERE m.IsOverride=1
		AND m.Id=@MeasureId
		
		SELECT @NewMeasureId = IDENT_CURRENT('[@@DESTINATION@@].[dbo].[Measures]');
		
		/*******************************************
		 *  Update Build2 new Overridden MeasureId 
		 *******************************************/
		
		UPDATE [@@DESTINATION@@].dbo.VersionToVersionMeasureMappingTemp
		SET NewOverriddenMeasureId=@NewMeasureId
		WHERE IsOverRide=1
		AND CurrentMeasureId=@MeasureId
		  
		/*******************************************
		 *    Get TopicId from Build 2 database
		 *******************************************/
		
		SELECT @NewTopicId=A.TopicId
		FROM   (
           SELECT RTRIM(LTRIM(T.Name))      Topic,
                  RTRIM(LTRIM(Tc.Name))     TopicCategory,
                  T.Id                      TopicId
           FROM   [@@DESTINATION@@].dbo.Topics T
                  INNER JOIN [@@DESTINATION@@].dbo.TopicCategories tc
                       ON  tc.Id = T.TopicCategory_id) A
       INNER JOIN (
                SELECT RTRIM(LTRIM(T.Name)) Topic,
                       RTRIM(LTRIM(Tc.Name)) TopicCategory
                FROM   [@@SOURCE@@].dbo.Topics T
                       INNER JOIN [@@SOURCE@@].dbo.TopicCategories tc
                            ON  tc.Id = T.TopicCategory_id
                       INNER JOIN [@@SOURCE@@].dbo.Measures_MeasureTopics  mmt
                            ON  mmt.Topic_Id = T.Id
                WHERE  mmt.Measure_id = @MeasureId) B
            ON  A.Topic = B.Topic
            AND A.TopicCategory = B.TopicCategory
            
            PRINT 'New Topic Id: '+CAST(@NewTopicId AS NVARCHAR(50))
            
            /*******************************************
             *  Measures_MeasureTopics
             *******************************************/
            IF NOT EXISTS(SELECT 1 FROM [@@DESTINATION@@].dbo.Measures_MeasureTopics
                          WHERE Measure_Id=@NewMeasureId
                          AND Topic_Id=@NewTopicId)
            
            INSERT INTO [@@DESTINATION@@].dbo.Measures_MeasureTopics
            (
            	Measure_Id,
            	Topic_Id
            )
            VALUES
            (
            	@NewMeasureId,
            	@NewTopicId
            )

	FETCH NEXT FROM @Dataset_Cursor INTO @MeasureId,@Measure, @MeasureType, @Source
	END
	
	
	/********************************************************************
	 *  Get all measures which were overriden from Measure Library
	 *****************************************************************/
     
     IF OBJECT_ID('[@@DESTINATION@@].[dbo].VersionToVersionMeasureMappingLibEditOrDeprecatedTemp') IS NOT NULL
	 DROP TABLE [@@DESTINATION@@].[dbo].VersionToVersionMeasureMappingLibEditOrDeprecatedTemp
     
     -- Check if source db is db version 1
     IF @IsDBVersion1=1
     BEGIN 
		SELECT v.CurrentMeasure Name,
		   COALESCE ([ClinicalTitle],'') [ClinicalTitle],COALESCE ([PlainTitle],'') [PlainTitle],COALESCE ([PolicyTitle],'') [PolicyTitle],
		   COALESCE ([SelectedTitle],'') [SelectedTitle],COALESCE ([ProvidedBenchmark],0) [ProvidedBenchmark],COALESCE ([CalculationMethod],'') [CalculationMethod],
		   COALESCE ([Description],'') [Description],COALESCE ([MoreInformation],'') [MoreInformation],COALESCE ([Footnotes],'') [Footnotes],
		   COALESCE ([NationalBenchmark],0) [NationalBenchmark],COALESCE ([HigherScoresAreBetter],0) [HigherScoresAreBetter],COALESCE ([ScaleBy],0) [ScaleBy],COALESCE ([ScaleTarget],'') [ScaleTarget],
		   COALESCE ([RiskAdjustedMethod],'') [RiskAdjustedMethod],COALESCE ([RateLabel],'') [RateLabel],COALESCE ([SuppressionDenominator],0) [SuppressionDenominator],
		   COALESCE ([SuppressionNumerator],0) [SuppressionNumerator],COALESCE ([PerformMarginSuppression],0) [PerformMarginSuppression],
		   COALESCE ([UpperBound],0) [UpperBound],COALESCE ([LowerBound],0) [LowerBound],COALESCE ([Url],'') [Url],COALESCE ([UrlTitle],'') [UrlTitle]
		   INTO [@@DESTINATION@@].[dbo].VersionToVersionMeasureMappingLibEditOrDeprecatedTemp 
		   FROM [@@SOURCE@@].dbo.Measures m
		   INNER JOIN [@@DESTINATION@@].dbo.VersionToVersionMeasureMappingTemp v
		   ON m.id=v.CurrentMeasureId 
		   WHERE m.IsOverride=0
		   EXCEPT
		   SELECT NAME,
		   COALESCE ([ClinicalTitle],'') [ClinicalTitle],COALESCE ([PlainTitle],'') [PlainTitle],COALESCE ([PolicyTitle],'') [PolicyTitle],
		   COALESCE ([SelectedTitle],'') [SelectedTitle],COALESCE ([ProvidedBenchmark],0) [ProvidedBenchmark],COALESCE ([CalculationMethod],'') [CalculationMethod],
		   COALESCE ([Description],'') [Description],COALESCE ([MoreInformation],'') [MoreInformation],COALESCE ([Footnotes],'') [Footnotes],
		   COALESCE ([NationalBenchmark],0) [NationalBenchmark],COALESCE ([HigherScoresAreBetter],0) [HigherScoresAreBetter],COALESCE ([ScaleBy],0) [ScaleBy],COALESCE ([ScaleTarget],'') [ScaleTarget],
		   COALESCE ([RiskAdjustedMethod],'') [RiskAdjustedMethod],COALESCE ([RateLabel],'') [RateLabel],COALESCE ([SuppressionDenominator],0) [SuppressionDenominator],
		   COALESCE ([SuppressionNumerator],0) [SuppressionNumerator],COALESCE ([PerformMarginSuppression],0) [PerformMarginSuppression],
		   COALESCE ([UpperBound],0) [UpperBound],COALESCE ([LowerBound],0) [LowerBound],COALESCE ([Url],'') [Url],COALESCE ([UrlTitle],'') [UrlTitle]
		   FROM [@@DESTINATION@@].dbo.Measures m
		  WHERE m.IsOverride=0
     END
     ELSE
     	-- Get Libratry edited Measure info.This can be tracked using IsLibEdit field in DBVersion 2 and 3
         SELECT v.CurrentMeasure Name,
		   [ClinicalTitle],[PlainTitle],[PolicyTitle],[SelectedTitle],[ProvidedBenchmark],[CalculationMethod],
		   [Description],[MoreInformation],[Footnotes] ,[NationalBenchmark],[HigherScoresAreBetter],[ScaleBy],[ScaleTarget],
		   [RiskAdjustedMethod],[RateLabel],[SuppressionDenominator],[SuppressionNumerator],[PerformMarginSuppression],
		   [UpperBound],[LowerBound],[Url],[UrlTitle]
		   INTO [@@DESTINATION@@].[dbo].VersionToVersionMeasureMappingLibEditOrDeprecatedTemp 
		   FROM [@@SOURCE@@].dbo.Measures m
		   INNER JOIN [@@DESTINATION@@].dbo.VersionToVersionMeasureMappingTemp v
		   ON m.id=v.CurrentMeasureId 
		   WHERE m.IsLibEdit=1
		   AND m.IsOverride=0  	
     		
	SET @Dataset_Cursor =  CURSOR FOR
	SELECT v2.CurrentMeasureId,v2.OrigincalCurrentMeasure
	  FROM [@@DESTINATION@@].[dbo].VersionToVersionMeasureMappingLibEditOrDeprecatedTemp v1
	           INNER JOIN [@@DESTINATION@@].dbo.VersionToVersionMeasureMappingTemp v2
	           ON v1.Name=v2.CurrentMeasure
	WHERE v2.IsOverRide=0
	
	OPEN @Dataset_Cursor;
	FETCH NEXT FROM @Dataset_Cursor INTO  @MeasureId,@Measure

	PRINT 'Handle Base Measures who had Library edit or Deprecated'

	WHILE @@fetch_status = 0
	BEGIN

		PRINT 'New Measure: ' + @Measure;
		PRINT 'Current Measure Id: '+CAST(@MeasureId AS NVARCHAR(30));
		Print char(13);
		
	/*******************************************
	 * Check if Measure is edited in the library
	 *******************************************/
	 
	 DECLARE @IsLibEditMeasure INT=0
	 
	 SELECT @IsLibEditMeasure = CASE WHEN COUNT(1)=1 THEN 1 ELSE 0 END  FROM [@@DESTINATION@@].[dbo].VersionToVersionMeasureMappingLibEditOrDeprecatedTemp v1
	           INNER JOIN [@@DESTINATION@@].dbo.VersionToVersionMeasureMappingTemp v2
	           ON v1.Name=v2.NewMeasure
	           WHERE v2.CurrentMeasureId=@MeasureId
	
	/***************************************************************
	 * Update Build 1 library edited measure information to build 2 
	 ***************************************************************/

	 IF @IsLibEditMeasure=1
	 BEGIN
			 UPDATE [@@DESTINATION@@].dbo.Measures
			 SET
	 			[Description] = b.[Description],
	 			MoreInformation = b.MoreInformation,
	 			Footnotes = b.Footnotes,
	 			NationalBenchmark = b.NationalBenchmark,
	 			HigherScoresAreBetter = b.HigherScoresAreBetter,
	 			ScaleBy = b.ScaleBy,
	 			ScaleTarget = b.ScaleTarget,
	 			RiskAdjustedMethod = b.RiskAdjustedMethod,
	 			RateLabel = b.RateLabel,
	 			SuppressionDenominator = b.SuppressionDenominator,
	 			SuppressionNumerator = b.SuppressionNumerator,
	 			PerformMarginSuppression = b.PerformMarginSuppression,
	 			UpperBound = b.UpperBound,
	 			LowerBound = b.LowerBound,
	 			[Url] = b.[Url],
	 			UrlTitle = b.UrlTitle,
	 			IsLibEdit = 1,
	 			ClinicalTitle = b.ClinicalTitle,
	 			PlainTitle = b.PlainTitle,
	 			PolicyTitle = b.PolicyTitle,
	 			SelectedTitle = b.SelectedTitle,
	 			ProvidedBenchmark = b.ProvidedBenchmark,
	 			CalculationMethod = b.CalculationMethod
			 FROM [@@DESTINATION@@].dbo.Measures a
			 INNER JOIN (SELECT v.NewMeasureId, m.*
						   FROM [@@SOURCE@@].dbo.Measures m
						 INNER JOIN [@@DESTINATION@@].dbo.VersionToVersionMeasureMappingTemp v
						 ON v.CurrentMeasureId=m.id
						 WHERE m.IsOverRide=0
						 AND m.Id=@MeasureId) b
			ON a.Id=b.NewMeasureId
	END 
	
	/*********************************************************
	*  Handle deprecated Measures which are deleted in Build 2
	**********************************************************/            
	 
	 DECLARE @IsDeprecatedMeasure INT=0
	 
	 SELECT @IsDeprecatedMeasure = CASE WHEN COUNT(1)=1 THEN 1 ELSE 0 END 
			   FROM [@@DESTINATION@@].dbo.VersionToVersionMeasureMappingTemp v2
	           WHERE v2.CurrentMeasureId=@MeasureId
	           AND v2.NewMeasureId IS NULL
	 
	IF (@IsDeprecatedMeasure=1)
	BEGIN			
		
		INSERT INTO [@@DESTINATION@@].dbo.Measures
		(
			Name,MeasureType,Source,[Description],MoreInformation,Footnotes,NationalBenchmark,HigherScoresAreBetter,ScaleBy,
			ScaleTarget,RiskAdjustedMethod,RateLabel,NQFEndorsed,NQFID,SuppressionDenominator,SuppressionNumerator,PerformMarginSuppression,
			UpperBound,LowerBound,[Url],UrlTitle,IsOverride,IsLibEdit,Target_id,ClinicalTitle,PlainTitle,PolicyTitle,SelectedTitle,ProvidedBenchmark,CalculationMethod
		) 
		SELECT m.Name,m.MeasureType,m.Source,m.[Description],m.MoreInformation,
			m.Footnotes,m.NationalBenchmark,m.HigherScoresAreBetter,m.ScaleBy,
			m.ScaleTarget,m.RiskAdjustedMethod,m.RateLabel,m.NQFEndorsed,m.NQFID,
			m.SuppressionDenominator,m.SuppressionNumerator,m.PerformMarginSuppression,
			m.UpperBound,m.LowerBound,m.[Url],m.UrlTitle,m.IsOverride,1 IsLibEdit,
			(
			          SELECT wf.Id
			          FROM   [@@DESTINATION@@].dbo.Wings_Features wf
			          WHERE  wf.Name = v.CurrentWingFeatureName
			) Target_id,
			m.ClinicalTitle+' [Deprecated]' ClinicalTitle,
			m.PlainTitle+' [Deprecated]' PlainTitle,
			m.PolicyTitle+' [Deprecated]' PolicyTitle,
			m.SelectedTitle +' [Deprecated]' SelectedTitle,
			m.ProvidedBenchmark,
			m.CalculationMethod
		FROM [@@SOURCE@@].dbo.[Measures] m
		      INNER JOIN [@@DESTINATION@@].dbo.VersionToVersionMeasureMappingTemp v
		      ON m.Id=v.CurrentMeasureId
		WHERE m.IsOverride=0
		AND m.Id=@MeasureId
		
		SELECT @NewMeasureId = IDENT_CURRENT('[@@DESTINATION@@].[dbo].[Measures]');
		
		/**********************************************************
		 *  Update new Build 2 Measure Id of the deprecated measure. 
		 **********************************************************/
		
		UPDATE [@@DESTINATION@@].dbo.VersionToVersionMeasureMappingTemp
		SET NewMeasureId =@NewMeasureId,IsLibEdit = 1
		WHERE CurrentMeasureId=@MeasureId
		
		/*******************************************
		 *    Get TopicId from Build 2 database
		 *    This will be applicable for deprecated measure
		 *******************************************/
		
		SELECT @NewTopicId=A.TopicId
		FROM   (
           SELECT RTRIM(LTRIM(T.Name))      Topic,
                  RTRIM(LTRIM(Tc.Name))     TopicCategory,
                  T.Id                      TopicId
           FROM   [@@DESTINATION@@].dbo.Topics T
                  INNER JOIN [@@DESTINATION@@].dbo.TopicCategories tc
                       ON  tc.Id = T.TopicCategory_id) A
       INNER JOIN (
                SELECT RTRIM(LTRIM(T.Name)) Topic,
                       RTRIM(LTRIM(Tc.Name)) TopicCategory
                FROM   [@@SOURCE@@].dbo.Topics T
                       INNER JOIN [@@SOURCE@@].dbo.TopicCategories tc
                            ON  tc.Id = T.TopicCategory_id
                       INNER JOIN [@@SOURCE@@].dbo.Measures_MeasureTopics  mmt
                            ON  mmt.Topic_Id = T.Id
                WHERE  mmt.Measure_id = @MeasureId) B
            ON  A.Topic = B.Topic
            AND A.TopicCategory = B.TopicCategory
            
            PRINT 'New Topic Id: '+CAST(@NewTopicId AS NVARCHAR(50))
            
            /*******************************************
             *  Measures_MeasureTopics
             *******************************************/
            IF NOT EXISTS(SELECT 1 FROM [@@DESTINATION@@].dbo.Measures_MeasureTopics
                          WHERE Measure_Id=@NewMeasureId
                          AND Topic_Id=@NewTopicId)
            
            INSERT INTO [@@DESTINATION@@].dbo.Measures_MeasureTopics
            (
            	Measure_Id,
            	Topic_Id
            )
            VALUES
            (
            	@NewMeasureId,
            	@NewTopicId
            )  
	END	
		

	FETCH NEXT FROM @Dataset_Cursor INTO @MeasureId,@Measure
	END		
	 
			/*******************************************
		    *      Websites_WebsiteMeasures
		    *******************************************/
		   
		   IF OBJECT_ID('[@@DESTINATION@@].[dbo].VersionToVersionWebsiteMappingTemp') IS NOT NULL
		   INSERT INTO [@@DESTINATION@@].dbo.Websites_WebsiteMeasures
		   (
		   	-- Id -- this column value is auto-generated
		   	IsSelected,
		   	OriginalMeasure_Id,
		   	OverrideMeasure_Id,
		   	Website_Id,
		   	[Index]
		   )
		   SELECT
		   	IsSelected,
		   	v1.NewMeasureId OriginalMeasure_Id,
		   	(SELECT v2.NewOverriddenMeasureId FROM [@@DESTINATION@@].dbo.VersionToVersionMeasureMappingTemp v2
		   	 WHERE v2.IsOverRide=1 AND v2.CurrentMeasureId=wwm.OverrideMeasure_id),
		   	V.NewWebsiteId Website_Id,
		   	[Index]
		   FROM
		   	[@@SOURCE@@].dbo.Websites_WebsiteMeasures wwm
		   	INNER JOIN [@@DESTINATION@@].dbo.VersionToVersionWebsiteMappingTemp v
		   	ON v.CurrentWebsiteId=wwm.Website_id
		   	INNER JOIN [@@DESTINATION@@].dbo.VersionToVersionMeasureMappingTemp v1
		   	ON v1.CurrentMeasureId=wwm.OriginalMeasure_Id

--END TRY
--BEGIN CATCH
		--IF @@TRANCOUNT > 0
		--ROLLBACK TRANSACTION;

		--PRINT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE AS ErrorMessage;

		--SELECT -1;
--END CATCH

--IF @@TRANCOUNT > 0
--		COMMIT TRANSACTION;

CLOSE @Dataset_Cursor;
DEALLOCATE @Dataset_Cursor;

IF OBJECT_ID('[@@DESTINATION@@].[dbo].VersionToVersionMeasureMappingTemp') IS NOT NULL
DROP TABLE [@@DESTINATION@@].[dbo].VersionToVersionMeasureMappingTemp

IF OBJECT_ID('[@@DESTINATION@@].[dbo].VersionToVersionWebsiteMappingTemp') IS NOT NULL
DROP TABLE [@@DESTINATION@@].[dbo].VersionToVersionWebsiteMappingTemp

IF OBJECT_ID('[@@DESTINATION@@].[dbo].VersionToVersionMeasureMappingLibEditOrDeprecatedTemp') IS NOT NULL
	 DROP TABLE [@@DESTINATION@@].[dbo].VersionToVersionMeasureMappingLibEditOrDeprecatedTemp

SELECT 1;