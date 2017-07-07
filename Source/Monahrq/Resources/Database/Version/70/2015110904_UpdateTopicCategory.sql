BEGIN TRY 
	
	update			t
	set				t.Name = 'Results of care - Complications'
	from			Topics t
		inner join	TopicCategories tc on tc.Id = t.TopicCategory_Id
	where			t.Name in ('Results of care - Complications','Results of care -- Complications')
		and			tc.Name = 'Patient safety';

	update			t
	set				t.Name = 'Results of care - Deaths'
	from			Topics t
		inner join	TopicCategories tc on tc.Id = t.TopicCategory_Id
	where			t.Name in ('Results of care - Deaths','Results of care -- Deaths', 'Results of care - Deaths and Readmissions')
		and			tc.Name = 'Patient safety';

	update			tc
	set				tc.Name = 'COPD-Chronic Obstructive Pulmonary Disease'
	from			TopicCategories tc
	where			tc.Name = 'COPD(Chronic Obstructive Pulmonary Disease)';

	update			t
	set				t.Name = 'Waiting Times'
	from			Topics t
		inner join	TopicCategories tc on tc.Id = t.TopicCategory_Id
	where			t.Name = 'Throughput'
		and			tc.Name = 'Emergency department (ED)';

	update			t
	set				t.Name = 'Infections resulting from hospital care'
	from			Topics t
		inner join	TopicCategories tc on tc.Id = t.TopicCategory_Id
	where			t.Name = 'Results of care - Complications'
		and			tc.Name = 'Infections acquired in the hospital';

	update			t
	set				t.Name = 'Results of Care – Deaths'
	from			Topics t
		inner join	TopicCategories tc on tc.Id = t.TopicCategory_Id
	where			t.Name = 'Results of Care'
		and			tc.Name = 'Other surgeries';

	update			tc
	set				tc.Name = 'Surgeries for Specific Health Conditions'
	from			TopicCategories tc
	where			tc.Name = 'Other surgeries';

	
	---------------------------------------------------------------------------
	
	delete			mt
	from			TopicCategories tc
		left join	Topics t on t.TopicCategory_Id = tc.Id 
		left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
	where			tc.Name = 'Surgeries for Specific Health Conditions'
		and			t.Name in ('Results of care – Deaths', 'Results of Care -- Deaths');
	
	delete			t
	from			TopicCategories tc
		left join	Topics t on t.TopicCategory_Id = tc.Id 
		left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
	where			tc.Name = 'Surgeries for Specific Health Conditions'
		and			t.Name in ('Results of care – Deaths', 'Results of Care -- Deaths');

	---------------------------------------------------------------------------

	
	delete			mt
	from			TopicCategories tc
		left join	Topics t on t.TopicCategory_Id = tc.Id 
		left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
	where			tc.Name = 'Prevention of blood clots';
	
	
	delete			t
	from			TopicCategories tc
		left join	Topics t on t.TopicCategory_Id = tc.Id 
		left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
	where			tc.Name = 'Prevention of blood clots';
	
	
	delete			tc
	from			TopicCategories tc
		left join	Topics t on t.TopicCategory_Id = tc.Id 
		left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
	where			tc.Name = 'Prevention of blood clots';

	---------------------------------------------------------------------------

	delete			mt
	from			Topics t
		inner join	TopicCategories tc on tc.Id = t.TopicCategory_Id
		left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
	where			tc.Name = 'Patient Safety'
		and			t.Name = 'Recommended care';

	delete			t
	from			Topics t
		inner join	TopicCategories tc on tc.Id = t.TopicCategory_Id
		left join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
	where			tc.Name = 'Patient Safety'
		and			t.Name = 'Recommended care';

	---------------------------------------------------------------------------
	
	update			tc
	set				tc.Name = 'Combined Quality and Safety Ratings'
	from			TopicCategories tc
	where			tc.Name = 'Summary Scores';

	update			t
	set				t.Name = 'Results of Care'
	from			Topics t
		inner join	TopicCategories tc on tc.Id = t.TopicCategory_Id
	where			t.Name = 'Results of care - Complications'
		and			tc.Name = 'Surgical patient safety';

		
	---------------------------------------------------------------------------
	
--	delete			mt
--	from			Topics t
--		inner join	TopicCategories tc on tc.Id = t.TopicCategory_Id
--		inner join	Measures_MeasureTopics mt on mt.Topic_Id = t.Id
--		inner join	Measures m on m.Id = mt.Measure_Id
--	where			tc.Name = 'Surgeries for Specific Health Conditions'
--		and			t.Name = 'Practice patterns'
--		and			m.Name in ('IQI 01','IQI 02','IQI 04','IQI 14');
		
	---------------------------------------------------------------------------
END TRY	
BEGIN CATCH
	DECLARE @ErrorMessage VARCHAR(5000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT @ErrorMessage = 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(50)) +' Error: ' + ERROR_MESSAGE(),
           @ErrorSeverity = ERROR_SEVERITY(),
           @ErrorState = ERROR_STATE();

    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState); 
END CATCH


