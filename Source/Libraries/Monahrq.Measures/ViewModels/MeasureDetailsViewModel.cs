using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ComponentModel.Composition;
using System.Windows.Data;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Services;
using Monahrq.Measures.Service;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.ViewModels;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using NHibernate;
using NHibernate.Linq;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Domain.Common;
using PropertyChanged;
using Monahrq.Infrastructure.Domain.Physicians;

namespace Monahrq.Measures.ViewModels
{
    /// <summary>
    /// View model class for measure details
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.ViewModels.DetailsViewModel{Monahrq.Infrastructure.Entities.Domain.Measures.Measure}" />
    [Export(typeof(MeasureDetailsViewModel))]
    [RegionMemberLifetime(KeepAlive = false)]
    [ImplementPropertyChanged]
    public class MeasureDetailsViewModel : DetailsViewModel<Measure>
    {
        #region Imports

        /// <summary>
        /// Gets or sets the measure service.
        /// </summary>
        /// <value>
        /// The measure service.
        /// </value>
        [Import]
        public IMeasureServiceSync MeasureService { get; set; }

        #endregion

        #region Fields and Constants

        /// <summary>
        /// The selected topic type
        /// </summary>
        private string _selectedTopicType;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the org measure model.
        /// </summary>
        /// <value>
        /// The org measure model.
        /// </value>
        MeasureModel OrgMeasureModel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the topic view models.
        /// </summary>
        /// <value>
        /// The topic view models.
        /// </value>
        public ICollectionView TopicViewModels { get; set; }

        /// <summary>
        /// Gets or sets the measure model.
        /// </summary>
        /// <value>
        /// The measure model.
        /// </value>
        public MeasureModel MeasureModel { get; set; }

        /// <summary>
        /// Gets or sets the topic types.
        /// </summary>
        /// <value>
        /// The topic types.
        /// </value>
        public List<string> TopicTypes { get; set; }

        /// <summary>
        /// Gets or sets the type of the selected topic.
        /// </summary>
        /// <value>
        /// The type of the selected topic.
        /// </value>
        public string SelectedTopicType
        {
            get { return _selectedTopicType; }
            set
            {
                _selectedTopicType = value;
                if (string.IsNullOrEmpty(_selectedTopicType)) return;

                TopicViewModels.Filter = null;
                TopicViewModels.Filter = o =>
                {
                    var topic = o as TopicViewModel;
                    if (topic == null) return true;
                    return topic.TopicCategory.TopicType == EnumExtensions.GetEnumValueFromDescription<TopicTypeEnum>(_selectedTopicType);
                };
            }
        }

        #endregion

        #region Constrcutor

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasureDetailsViewModel"/> class.
        /// </summary>
        public MeasureDetailsViewModel()
        { }

        #endregion

        #region ToBeDeleted
        /// <summary>
        /// Updates the current measure.
        /// </summary>
        /// <param name="m">The m.</param>
        public void UpdateCurrentMeasure(MeasureModel m)
        {
            IsBusy = true;
            try
            {
                MeasureModel = m;
                MeasureModel.InitMeasure(m.Measure);            // added by Scott/Inga 10/14, but the measure being edited is not initialized correctly

                //MeasureModel.PropertyChanged += MeasureModel_PropertyChanged;

                if (MeasureService == null)
                {
                    MeasureService = ServiceLocator.Current.GetInstance<IMeasureServiceSync>();
                }
                TopicViewModels = new CollectionView(MeasureService.TopicViewModels);
                ReconcileTopicsViewModel();

                MeasureModel.Committed = true;  // for brand new init, measure has no changes, Commited=true, disables Cancel and Save buttons.
            }
            catch (Exception ex)
            {
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        //this method marks topics as selected , if measure has that topic

        /// <summary>
        /// Reconciles the topics view model.
        /// </summary>
        private void ReconcileTopicsViewModel()
        {
            if (MeasureModel == null) return;

            foreach (SubTopicViewModel subtopic in MeasureModel.Measure.Topics.SelectMany(
                t => TopicViewModels.OfType<TopicViewModel>().SelectMany(
                    topicViewModel => topicViewModel.ChildrenCollectionView.OfType<SubTopicViewModel>().Where(
                        subtopic => t.Id == subtopic.Id))))
            {
                subtopic.IsSelected = true;
            }


        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is busy.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is busy; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusy { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether this instance [can navigate back].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can navigate back]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanNavigateBack()
        {
            return !IsBusy;
        }

        /// <summary>
        /// Executes the navigate back.
        /// </summary>
        private void ExecuteNavigateBack()
        {
            if (IsBusy) return;

            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.MainMeasuresView, UriKind.Relative));
        }

        /// <summary>
        /// Called when [cancel].
        /// </summary>
        public override void OnCancel()
        {
            //RefreshCurrentModel();
            //MeasureModel.Rollback();
            ExecuteNavigateBack();
        }

        /// <summary>
        /// Called when save action is performed.
        /// </summary>
        /// <param name="navigateback">if set to <c>true</c> [navigateback].</param>
        public override async void OnSave(bool navigateback = false)
        {
           
            MeasureModel.Committ();
            MeasureModel.Committed = true;

            ExecuteNavigateBack();
        }

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            TopicTypes = EnumExtensions.GetEnumDescriptions<TopicTypeEnum>();

            EventAggregator.GetEvent<SetContextualHelpContextEvent>().Publish("Measures");

            var measureId = (navigationContext.Parameters["MeasureId"] != null)
                                        ? int.Parse(navigationContext.Parameters["MeasureId"])
                                        : (int?)null;
            LoadModel(measureId);
            MeasureModel = new MeasureModel(Model) { MeasureService = MeasureService, MeasureDetailsVm = this, Events = EventAggregator };

            //select Topics belonging to the measure 
            foreach (TopicViewModel topicViewModel in TopicViewModels)
            {
                if (topicViewModel.ChildrenCollectionView == null || !topicViewModel.ChildrenCollectionView.Any())
                    continue;

                foreach (var viewModel in topicViewModel.ChildrenCollectionView.OfType<SubTopicViewModel>().ToList())
                {
                    if (MeasureModel.Measure.Topics.Any(t => t.Id == viewModel.Id))
                        viewModel.IsSelected = true;
                }
            }

            if (Model is NursingHomeMeasure) SelectedTopicType = TopicTypeEnum.NursingHome.GetDescription();
            else if (Model is PhysicianMeasure) SelectedTopicType = TopicTypeEnum.Physician.GetDescription();
            //else if (Model is PhysicianMeasure) SelectedTopicType = TopicTypeEnum.Physician.GetDescription();
            else SelectedTopicType = TopicTypeEnum.Hospital.GetDescription();

            MeasureModel.Committed = true;
        }

        /// <summary>
        /// Called to determine if this instance can handle the navigation request.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        /// <see langword="true" /> if this instance accepts the navigation request; otherwise, <see langword="false" />.
        /// </returns>
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);

            MeasureModel = null;
            SelectedTopicType = string.Empty;
        }

        /// <summary>
        /// Executes the load.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="id">The identifier.</param>
        protected override void ExecLoad(ISession session, object id)
        {
            base.ExecLoad(session, id);

            TopicViewModels = CollectionViewSource.GetDefaultView(session.Query<TopicCategory>().OrderBy(x => x.Name).Select(t => new TopicViewModel(t)).ToList());
        }

        #endregion
    }
}
