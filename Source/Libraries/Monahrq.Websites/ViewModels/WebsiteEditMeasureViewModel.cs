using System;
using System.ComponentModel.Composition;
using PropertyChanged;
using System.Windows.Data;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Sdk.Events;
using Monahrq.Infrastructure.Services;
using System.Linq;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Domain.Hospitals;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Websites.ViewModels
{
	[Export(typeof(WebsiteEditMeasureViewModel))]
	[ImplementPropertyChanged]
	public class WebsiteEditMeasureViewModel : WebsiteTabViewModel
	{
		#region Properties.
		public MeasureModel SelectedMeasure { get; set; }
        public bool IsMeasureTopicsEnabled { get; set; }
		public ListCollectionView TopicsCollectionView { get; set; }

		public event Action MeasureSaved;
		public event Action MeasureEditCancelled;


		public bool AllowTitleSelection { get; set; }
		public bool AllowDescriptionEdit { get; set; }
		public bool AllowClinicalTitleEdit { get; set; }
		public bool AllowAttributeTabEdit { get; set; }
		public bool AllowReferenceTabEdit { get; set; }
		#endregion

		#region Methods.
		public WebsiteEditMeasureViewModel()
		{
			SetViewMode(WebsiteEditMeasureViewModeEnum.Normal);
		}

		#region Navigation Methods.
		public override void Continue()
		{
			throw new NotImplementedException();
		}

		public override void Save()
		{
			//throw new NotImplementedException();
		}
		#endregion

		#region Action Methods.
		public void OnViewLoaded()
		{

		}
		public void OnViewUnloaded()
		{

		}

		/// <summary>
		/// Cancels the edit selected measure.
		/// </summary>
		/// <param name="param">The parameter.</param>
		public void CancelEditSelectedMeasure()
		{
			UnselectTopics();						// Clear all topic selections.
			SelectedMeasure = null;					// Clear Popup ViewModel.
			//ParentVM.IsMeasureWindowOpen = false;	// Close Popup.
			MeasureEditCancelled();
		}

		/// <summary>
		/// Saves the selected measure.
		/// </summary>
		/// <param name="param">The parameter.</param>
		public void SaveSelectedMeasure()
		{

			SaveMeasureModel(SelectedMeasure);

			// Clear all topic selections.
			UnselectTopics();

			// refresh the AvailableMeasures
			//RefreshAvailableMeasures();			// Needs to be called OnClose().
			
			Save();

			//ParentVM.IsMeasureWindowOpen = false;
			MeasureSaved();
		}

		public bool CanSaveSelectedMeasure()
		{
			return
				SelectedMeasure != null &&
				!SelectedMeasure.ClinicalTitleOverride.IsNullOrEmpty() &&
				!SelectedMeasure.PlainTitleOverride.IsNullOrEmpty();
		}

		#endregion

		#region View Mode Methods.
		public void SetViewMode(WebsiteEditMeasureViewModeEnum mode)
		{
			switch (mode)
			{
				case WebsiteEditMeasureViewModeEnum.TitleOnly:
					AllowTitleSelection = false;
					AllowDescriptionEdit = false;
					AllowClinicalTitleEdit = false;
					AllowAttributeTabEdit = false;
					AllowReferenceTabEdit = false;
					break;
				case WebsiteEditMeasureViewModeEnum.Normal:
					AllowTitleSelection = true;
					AllowDescriptionEdit = true;
					AllowClinicalTitleEdit = true;
					AllowAttributeTabEdit = true;
					AllowReferenceTabEdit = true;
					break;
			}

			RaisePropertyChanged(() => AllowDescriptionEdit );
			RaisePropertyChanged(() => AllowClinicalTitleEdit);
			RaisePropertyChanged(() => AllowAttributeTabEdit );
			RaisePropertyChanged(() => AllowReferenceTabEdit );
		}
		#endregion

		#region Measure State Methods.

		/// <summary>
		/// Saves the Measure Model
		/// </summary>
		/// <param name="measureToSave"></param>
		private void SaveMeasureModel(MeasureModel measureToSave, bool isBatchLoading = false)
		{
			if (!isBatchLoading) measureToSave.UpdateForMeasureOverride();
			//measureToSave.ReconcileTopics(this, measureToSave.MeasureOverwrite);
			measureToSave.WasMeasureOverrideEdited = true;

			measureToSave.MeasureOverwrite.MeasureTitle.Selected = (measureToSave.IsPlainTitleSelected)
																				? SelectedMeasuretitleEnum.Plain
																				: SelectedMeasuretitleEnum.Clinical;
			if (!isBatchLoading) ReverseHydrateTopics(measureToSave.MeasureOverwrite);

			WebsiteDataService.SaveMeasureOverride(measureToSave.MeasureOverwrite, (result, error) =>
			{
				if (error == null)
				{
					var msg = string.Format("Measure \"{0}\" was successfully overwritten. Please save website \"{1}\".", measureToSave.MeasureOverwrite.MeasureTitle.Clinical, base.ManageViewModel.WebsiteViewModel.Website.Name);
					EventAggregator.GetEvent<GenericNotificationEvent>().Publish(msg);
					if (!isBatchLoading) HydrateTopics(measureToSave.MeasureOverwrite);
					//EventAggregator.GetEvent<ForceTabSaveEvent>().Publish(true);
					measureToSave.MeasureOverwrite = result;
				}
				else
					EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(error);
			});
			
			if (measureToSave.WebsiteMeasure != null)
				measureToSave.WebsiteMeasure.OverrideMeasure = measureToSave.MeasureOverwrite;
		}

		/// <summary>
		/// Creates a *useable* MeasureModel from a WebsiteMeasure.
		/// </summary>
		/// <param name="websiteMeasure"></param>
		/// <param name="isBatchLoading">
		/// Indicates that we are loading multiple MeasuresModels.  As such, currently no need to hydrate 
		/// topics.  If we ever add topics to Batch editing, we can should add a BatchHydrateTopics function
		/// that would be called at this functions call site, after all batched MeasureModes have been 
		/// generated.  Also, the TopicTree IsSelection type should be changed from bool to bool?, thus 
		/// introducing a 3 state true, false, mixed IsSelected indicator, where mixed means some of the 
		/// batched Measures selected the topic, and some
		/// have not.
		/// </param>
		/// <returns></returns>
		private MeasureModel GenMeasureModel(WebsiteMeasure websiteMeasure, bool isBatchLoading = false)
		{
			var newMeasureModel = new MeasureModel(websiteMeasure);
			newMeasureModel.InitMeasureOverride(newMeasureModel.MeasureOverwrite ?? newMeasureModel.Measure);
			newMeasureModel.IsPlainTitleSelected = (newMeasureModel.MeasureOverwrite.IsPersisted)
										? newMeasureModel.MeasureOverwrite.MeasureTitle.Selected == SelectedMeasuretitleEnum.Plain
										: CurrentWebsite.Audiences.Any(a => a == Audience.Consumers);
			//: CurrentWebsite.Audience == Audience.Consumers;
			newMeasureModel.WasMeasureOverrideEdited = true;
			if (!isBatchLoading) HydrateTopics(newMeasureModel.MeasureOverwrite);
			return newMeasureModel;
		}

		public void AssignSelectedMeasure(WebsiteMeasure websiteMeasure)
		{
			TopicTypeEnum? topicType;
			IsMeasureTopicsEnabled = true;
			
            if (websiteMeasure.OriginalMeasure is NursingHomeMeasure)
            {
                topicType = TopicTypeEnum.NursingHome;
                IsMeasureTopicsEnabled = false;
            } 
            else if (websiteMeasure.OriginalMeasure is HospitalMeasure)
                topicType = TopicTypeEnum.Hospital;
            else
                topicType = null;

			TopicsCollectionView = CollectionViewSource.GetDefaultView(WebsiteDataService.GetTopicViewModels(topicType).ToObservableCollection()) as ListCollectionView;


			SelectedMeasure = GenMeasureModel(websiteMeasure);
		}
		#endregion

		#region Topic Methods.
		private void UnselectTopics()
		{
			if (TopicsCollectionView == null) return;

			foreach (TopicViewModel topic in TopicsCollectionView)
			{
				topic.IsSelected = false;

				if (topic.ChildrenCollectionView == null) continue;

				foreach (SubTopicViewModel subtopic in topic.ChildrenCollectionView)
				{
					subtopic.IsSelected = false;
				}
			}
		}
		private void HydrateTopics(Measure measure, bool doUnselectAlso = true)
		{
			if (measure == null)
				return;

			foreach (var topicViewModel in TopicsCollectionView.Cast<TopicViewModel>())
			{
				if (topicViewModel.ChildrenCollectionView == null || !topicViewModel.ChildrenCollectionView.Any())
					continue;

				foreach (var viewModel in topicViewModel.ChildrenCollectionView.OfType<SubTopicViewModel>().ToList())
				{
					if (measure.Topics.Any(t => t.Id == viewModel.Id))
					{
						viewModel.IsSelected = true;
					}
					else if (doUnselectAlso)
					{
						viewModel.IsSelected = false;
					}
				}
			}
		}
		private void ReverseHydrateTopics(Measure measure)
		{
			if (measure == null) return;
			measure.ClearTopics();

			foreach (var topicViewModel in TopicsCollectionView.Cast<TopicViewModel>())
			{
				if (topicViewModel.ChildrenCollectionView == null || !topicViewModel.ChildrenCollectionView.Any())
					continue;

				foreach (var selectedTopic in topicViewModel.SelectedTopics) //.ChildrenCollectionView.OfType<SubTopicViewModel>().ToList())
				{
					measure.AddTopic(selectedTopic);
				}
			}
		}
		#endregion

		#endregion
	}

	public enum WebsiteEditMeasureViewModeEnum
	{
		Normal,
		TitleOnly,
	}

}
