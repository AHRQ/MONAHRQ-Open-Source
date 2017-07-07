using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Monahrq.Infrastructure.Services;
using Monahrq.Websites.Events;
using Monahrq.Websites.Services;
using PropertyChanged;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using System.Windows.Data;
using Microsoft.Practices.ServiceLocation;
using NHibernate.Linq;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Websites.ViewModels
{
    [Export]
    [ImplementPropertyChanged]
    public class ManageTopicsViewModel : WebsiteTabViewModel
    {
        public DelegateCommand AddNewTopicCommand { get; set; }
        public DelegateCommand<object> DeleteTopicCommand { get; set; }
        public DelegateCommand FilterTopicCommand { get; set; }
        public DelegateCommand ResetCommand { get; set; }

        #region Properties
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        protected IWebsiteDataService DataService { get; set; }


        public ListCollectionView TopicsCollectionView { get; set; }
        public TopicViewModel SelectedTopic { get; set; }
        public string SelectedFilter { get; set; }
        public ObservableCollection<string> FilterEnumeration { get; set; }
        public string FilterText { get; set; }

        public bool IsBusy { get; set; }
        public new string TabTitle
        {
            get { return BaseTitle + " (" + TopicsCollectionView.Count + ")"; }
        }

        private string _newTopicName;
        public string NewTopicName
        {
            get { return _newTopicName; }
            set
            {
                if (value == _newTopicName) return;
                _newTopicName = value;
                RaisePropertyChanged(() => NewTopicName);
                //_ValidateName(ExtractPropertyName(() => NewTopicName), value);
                //_ValidateUnique(ExtractPropertyName(() => NewTopicName), value);
                Validate();
            }
        }

        public int NewTopicNameMaxLength
        {
            get { return 200; }            // max length for the user to type in the textbox
        }

        private bool _isClearInvoked;
        public bool IsClearInvoked
        {
            get { return _isClearInvoked; }
            set
            {
                if (_isClearInvoked == value) return;
                _isClearInvoked = value;
                if (IsClearInvoked)
                {
                    _Reset();
                }
            }
        }
        #endregion

        #region Validation

        //private void _ValidateName(string prop, string value)
        //{
        //    ClearErrors(prop);

        //    if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
        //    {
        //        SetError(prop, "Name cannot be empty.");
        //    }

        //    AddNewTopicCommand.RaiseCanExecuteChanged();
        //}

        //private void _ValidateUnique(string prop, string value)
        //{
        //    //if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value)) return;

        //    //ClearErrors(prop);

        //    //if (MeasureService.EntityTopics.Any(x => x.Name == value))
        //    //{
        //    //    SetError(prop, "Topic title must be unique.");
        //    //}

        //    //AddNewTopicCommand.RaiseCanExecuteChanged();
        //}

        //protected override void ValidateAll()
        //{
        //    _ValidateName(ExtractPropertyName(() => NewTopicName), NewTopicName);
        //    _ValidateUnique(ExtractPropertyName(() => NewTopicName), NewTopicName);
        //}

        #endregion

        #region Methods

        public override void OnImportsSatisfied()
        {
            InitData();
            EventAggregator.GetEvent<TopicsUpdatedEvent>().Subscribe(RefreshTopicsCollection);
            new TopicsFilter(this);
            base.OnImportsSatisfied();
            DeleteTopicCommand.RaiseCanExecuteChanged();
        }

        protected override void InitProperties()
        {
            InitData();

            BaseTitle = "Manage Topics";
            //IsTabSelected = false;
            FilterEnumeration = new ObservableCollection<string> { 
                ModelPropertyFilterValues.TOPIC_NAME,
                ModelPropertyFilterValues.SUBTOPIC_NAME };

            SelectedFilter = FilterEnumeration.FirstOrDefault();
            FilterText = string.Empty;
        }

        public override void Save()
        {
            throw new NotImplementedException();
        }

        public override void Continue()
        {
            throw new NotImplementedException();
        }

        protected void InitData()
        {
            TopicsCollectionView = CollectionViewSource.GetDefaultView(DataService.GetTopicViewModels().ToObservableCollection()) as ListCollectionView;
            if (TopicsCollectionView != null) TopicsCollectionView.MoveCurrentToFirst();
        }

        private void RefreshTopicsCollection(int id)
        {
            TopicsCollectionView = CollectionViewSource.GetDefaultView(DataService.GetTopicViewModels().ToObservableCollection()) as ListCollectionView;
        }

        #endregion

        #region Commands

        protected override void InitCommands()
        {
            AddNewTopicCommand = new DelegateCommand(OnAddNew, CanAdd);
            ResetCommand = new DelegateCommand(InitProperties);
            DeleteTopicCommand = new DelegateCommand<object>(OnDelete);
        }

        private void OnAddNew()
        {
            SaveCommand.Execute();
            //CommitCommand.Execute(null);
        }

        private bool CanAdd()
        {
            return SaveCommand.CanExecute(); // CommitCommand.CanExecute(null);
        }

        //protected override void OnCommitted()
        //{
        //    //try
        //    //{
        //    //    var topicCategory = new TopicCategory(NewTopicName);
        //    //    MeasureService.SaveOrUpdateTopic(topicCategory);
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    Events.GetEvent<ErrorNotificationEvent>().Publish(ex);
        //    //}

        //    //_Reset(); 
        //}

        private void _Reset()
        {
            NewTopicName = string.Empty;
            ClearErrors(() => NewTopicName);
            //Committed = true;
        }

        private void OnDelete(object obj)
        {
            //var topicViewModel = obj as TopicViewModel;
            //if (topicViewModel == null) return;

            //// query to find out if this topic is assigned to any measures
            //int mcount;
            //var service = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            //using (var session = service.SessionFactory.OpenSession())
            //{
            //    var topics = session.Query<TopicCategory>().Where(a => a.Id == topicViewModel.TopicCategory.Id).SingleOrDefault().Topics.Where(topic => topic.Measures.Any());

            //    var mlist = new List<int>();
            //    foreach (var topic in topics)
            //    {
            //        mlist.AddRange(topic.Measures.Select(x => x.Id));
            //    }

            //    mcount = mlist.Distinct().Count();
            //}

            //if (MessageBox.Show(string.Format("This topic ({0}) is used in {1} measure(s). Are you sure you want to delete it?", topicViewModel.TopicName, mcount),
            //                 "Delete Topic?",
            //                 MessageBoxButton.YesNo,
            //                 MessageBoxImage.Question)
            // != MessageBoxResult.Yes)
            //    return;

            //MeasureService.DeleteTopicCategory(topicViewModel.TopicCategory);
        }

        #endregion
    }
}
