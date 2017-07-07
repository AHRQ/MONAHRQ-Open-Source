using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Measures.Events;
using Monahrq.Sdk.ViewModels;
using NHibernate;
using PropertyChanged;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using System.Windows.Data;
using NHibernate.Linq;
using System.Windows.Forms;
using System.IO;
using FluentNHibernate.Utils;

namespace Monahrq.Measures.ViewModels
{
    /// <summary>
    /// View model class for manage topics
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.ViewModels.ListTabViewModel{Monahrq.Infrastructure.Entities.Domain.Measures.Topic}" />
    [ImplementPropertyChanged]
    [Export(typeof(ManageTopicsViewModel))]
    public class ManageTopicsViewModel : ListTabViewModel<Topic> // BaseTabViewModel
    {
        #region Fields and Constants

        /// <summary>
        /// The text for string legnth error message
        /// </summary>
        private const string STRING_LEGNTH_ERROR_MESSAGE = "Please enter a new Topic Name using fewer than 200 characters.";
        /// <summary>
        /// The text for manage topics
        /// </summary>
        private const string MANAGE_TOPICS = "Manage Topics";	//"Manage Topics & Conditions";
        /// <summary>
        /// The is clear invoked
        /// </summary>
        private bool _isClearInvoked;
        /// <summary>
        /// The new topic name
        /// </summary>
        private string _newTopicName;
        /// <summary>
        /// The filter enumeration
        /// </summary>
        private ObservableCollection<string> _filterEnumeration;
        /// <summary>
        /// The filter text
        /// </summary>
        private string _filterText;
        /// <summary>
        /// The new topic category type
        /// </summary>
        private string _newTopicCategoryType;
        /// <summary>
        /// The text for select a type
        /// </summary>
        private const string SelectAType = "Select a Type";
        /// <summary>
        /// The text for topics and conditions
        /// </summary>
        private const string TopicsAndConditions = "Topics"; //"Topics & Conditions";
        /// <summary>
        /// The text for topic category type required
        /// </summary>
        private const string TopicCategoryTypeRequired = "Please select a Type in order to add an item";
        /// <summary>
        /// The is saving
        /// </summary>
        private static bool _isSaving;
        /// <summary>
        /// The selected topic type filter
        /// </summary>
        private string _selectedTopicTypeFilter;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageTopicsViewModel"/> class.
        /// </summary>
        public ManageTopicsViewModel()
        {
            //IsInitialLoad = true;
            Index = 1;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the add new topic command.
        /// </summary>
        /// <value>
        /// The add new topic command.
        /// </value>
        public DelegateCommand AddNewTopicCommand { get; set; }

        /// <summary>
        /// Gets or sets the delete topic command.
        /// </summary>
        /// <value>
        /// The delete topic command.
        /// </value>
        public DelegateCommand<TopicViewModel> DeleteTopicCommand { get; set; }

        /// <summary>
        /// Gets or sets the filter topic command.
        /// </summary>
        /// <value>
        /// The filter topic command.
        /// </value>
        public DelegateCommand FilterTopicCommand { get; set; }

        /// <summary>
        /// Gets or sets the reset command.
        /// </summary>
        /// <value>
        /// The reset command.
        /// </value>
        public DelegateCommand ResetCommand { get; set; }

        /// <summary>
        /// Gets or sets the close new topic pop up command.
        /// </summary>
        /// <value>
        /// The close new topic pop up command.
        /// </value>
        public DelegateCommand CloseNewTopicPopUpCommand { get; set; }

        /// <summary>
        /// Gets or sets the save new topic command.
        /// </summary>
        /// <value>
        /// The save new topic command.
        /// </value>
        public DelegateCommand SaveNewTopicCommand { get; set; }

        /// <summary>
        /// Gets or sets the select directory command.
        /// </summary>
        /// <value>
        /// The select directory command.
        /// </value>
        public DelegateCommand<TopicFacts> SelectDirectoryCommand { get; set; }

        /// <summary>
        /// Gets or sets the edit topic category command.
        /// </summary>
        /// <value>
        /// The edit topic category command.
        /// </value>
        public DelegateCommand<TopicViewModel> EditTopicCategoryCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the topics collection view.
        /// </summary>
        /// <value>
        /// The topics collection view.
        /// </value>
        public ListCollectionView TopicsCollectionView { get; set; }

        /// <summary>
        /// Gets or sets the selected topic.
        /// </summary>
        /// <value>
        /// The selected topic.
        /// </value>
        public TopicViewModel SelectedTopic { get; set; }

        /// <summary>
        /// Gets or sets the selected filter.
        /// </summary>
        /// <value>
        /// The selected filter.
        /// </value>
        public string SelectedFilter { get; set; }

        /// <summary>
        /// Gets or sets the filter enumeration.
        /// </summary>
        /// <value>
        /// The filter enumeration.
        /// </value>
        public ObservableCollection<string> FilterEnumeration
        {
            get
            {
                return _filterEnumeration ?? (_filterEnumeration = new ObservableCollection<string>
                {
                    ModelPropertyFilterValues.TOPIC_NAME,
                    ModelPropertyFilterValues.SUBTOPIC_NAME
                });
            }
            set { _filterEnumeration = value; }
        }

        /// <summary>
        /// Gets or sets the filter text.
        /// </summary>
        /// <value>
        /// The filter text.
        /// </value>
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                if (_filterText != null && _filterText == value) return;
                _filterText = value;
                CollectionItems.Filter = null;
                CollectionItems.Filter += TopicFilter;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is busy.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is busy; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusy { get; set; }

        /// <summary>
        /// The header text
        /// </summary>
        private string _headerText;
        /// <summary>
        /// Gets or sets the header text.
        /// </summary>
        /// <value>
        /// The header text.
        /// </value>
        public string HeaderText
        {
            get
            {
                _headerText = (CollectionItems != null && CollectionItems.Count > 0) ? HeaderName + " (" + CollectionItems.Count + ")" : (CategoryCount > 0) ? HeaderName + " (" + CategoryCount + ")" : HeaderName;
                return _headerText;
            }
            set
            {
                _headerText = value;
            }
        }

        /// <summary>
        /// Gets or sets the new name of the topic.
        /// </summary>
        /// <value>
        /// The new name of the topic.
        /// </value>
        [Unique("TopicCategories", "Name", "Topic title must be unique.")]
        [StringLength(199, ErrorMessage = STRING_LEGNTH_ERROR_MESSAGE)]
        [CustomValidation(typeof(ManageTopicsViewModel), "IsTopicTitleEmpty", ErrorMessage = SelectAType)]
        public string NewTopicName
        {
            get { return _newTopicName; }
            set
            {
                if (value == _newTopicName) return;
                _newTopicName = value;
                RaisePropertyChanged(() => NewTopicName);
            }
        }

        //[CustomValidation(typeof(ManageTopicsViewModel), "IsTopicCategoryTypeSelected", ErrorMessage = SelectAType)]
        /// <summary>
        /// Gets or sets the new type of the topic category.
        /// </summary>
        /// <value>
        /// The new type of the topic category.
        /// </value>
        public string NewTopicCategoryType
        {
            get { return _newTopicCategoryType; }
            set
            {
                _newTopicCategoryType = value;
                RaisePropertyChanged(() => NewTopicCategoryType);
            }
        }

        /// <summary>
        /// Gets the topic category types.
        /// </summary>
        /// <value>
        /// The topic category types.
        /// </value>
        public List<string> TopicCategoryTypes
        {
            get
            {
                var items = EnumExtensions.GetEnumStringValues<TopicCategoryTypeEnum>();
                items.Insert(0, SelectAType);
                return items;
            }
        }

        /// <summary>
        /// Gets the filter by list.
        /// </summary>
        /// <value>
        /// The filter by list.
        /// </value>
        public List<string> FilterByList
        {
            get
            {
                var items = EnumExtensions.GetEnumStringValues<TopicCategoryTypeEnum>();
                items.Insert(0, TopicsAndConditions);
                return items;
            }
        }

        /// <summary>
        /// Gets the new length of the topic name maximum.
        /// </summary>
        /// <value>
        /// The new length of the topic name maximum.
        /// </value>
        public int NewTopicNameMaxLength
        {
            get { return 200; }            // max length for the user to type in the textbox
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is clear invoked.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is clear invoked; otherwise, <c>false</c>.
        /// </value>
        public bool IsClearInvoked
        {
            get { return _isClearInvoked; }
            set
            {
                if (_isClearInvoked == value) return;
                _isClearInvoked = value;
                if (_isClearInvoked)
                {
                    Reset();
                }
            }
        }

        /// <summary>
        /// Gets the category count.
        /// </summary>
        /// <value>
        /// The category count.
        /// </value>
        private int CategoryCount
        {
            get
            {
                if (DataserviceProvider == null) return 0;
                int count;
                using (var session = DataserviceProvider.SessionFactory.OpenStatelessSession())
                {
                    count = session.CreateSQLQuery("select count(Id) from TopicCategories").UniqueResult<int>();
                }
                return count;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is adding new topic.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is adding new topic; otherwise, <c>false</c>.
        /// </value>
        public bool IsAddingNewTopic { get; set; }

        /// <summary>
        /// Gets or sets the selected topic type filter.
        /// </summary>
        /// <value>
        /// The selected topic type filter.
        /// </value>
        public string SelectedTopicTypeFilter
        {
            get { return _selectedTopicTypeFilter; }
            set
            {
                _selectedTopicTypeFilter = value;
                CollectionItems.Filter = null;
                CollectionItems.Filter += TopicFilter;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the topic category type selected or not.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static ValidationResult IsTopicCategoryTypeSelected(string value)
        {
            return SelectAType.Equals(value) ? new ValidationResult(TopicCategoryTypeRequired, new List<string> { "NewTopicCategoryType" }) : ValidationResult.Success;
        }

        /// <summary>
        /// Determines whether the topic title empty or n ot.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static ValidationResult IsTopicTitleEmpty(string value)
        {
            return string.IsNullOrEmpty(value) ? new ValidationResult("Please enter the topic name.", new List<string> { "NewTopicName" }) : ValidationResult.Success;
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected override void InitCommands()
        {
            base.InitCommands();
            AddNewTopicCommand = new DelegateCommand(AddNewTopic, CanAddNewTopic);
            ResetCommand = new DelegateCommand(InitProperties);
            DeleteTopicCommand = new DelegateCommand<TopicViewModel>(OnDelete);
            SaveNewTopicCommand = new DelegateCommand(OnSaveToppic);
            CloseNewTopicPopUpCommand = new DelegateCommand(OnCancelCommand);
            SelectDirectoryCommand = new DelegateCommand<TopicFacts>(OnFolderSelectCommand);
            EditTopicCategoryCommand = new DelegateCommand<TopicViewModel>(OnEditTopicCategory);
            EventAggregator.GetEvent<TopicsUpdatedEvent>().Subscribe(o => OnLoad());
        }

        /// <summary>
        /// Called when when cancel is clicked.
        /// </summary>
        private void OnCancelCommand()
        {
            IsAddingNewTopic = false;
            SelectedTopic.TopicCategory = GetTopicCategory(SelectedTopic.Id);
            Reset();
        }

        /// <summary>
        /// Called when folder is selected.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void OnFolderSelectCommand(TopicFacts obj)
        {
            var directory = string.IsNullOrEmpty(obj.ImagePath) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : File.Exists(obj.ImagePath) ? obj.ImagePath : File.Exists(Path.Combine(Environment.CurrentDirectory, "Resources", "Templates", "Site", obj.ImagePath)) ?
                Path.Combine(Environment.CurrentDirectory, "Resources", "Templates", "Site", obj.ImagePath) : Path.Combine(Environment.CurrentDirectory, "Resources", "Templates", "Site", Path.GetDirectoryName(obj.ImagePath));
            using (var dlg = new OpenFileDialog())
            {
                //dlg.InitialDirectory = string.IsNullOrEmpty(obj.ImagePath) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                //    : System.IO.Path.GetFullPath(obj.ImagePath);
                dlg.InitialDirectory = directory;
                dlg.Filter = "png files (*.png)|*.png|jpeg files (*.jpeg)|(*.jpeg)";
                dlg.FilterIndex = 1;
                dlg.RestoreDirectory = true;
                dlg.Multiselect = false;

                // Get the selected file name and display in a TextBox
                if (dlg.ShowDialog() == DialogResult.OK && obj != null)
                    obj.ImagePath = dlg.FileName;
            }
        }

        /// <summary>
        /// Called when topic category is edited.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void OnEditTopicCategory(TopicViewModel obj)
        {
            SelectedTopic = obj;
            NewTopicName = SelectedTopic.TopicName;
            IsAddingNewTopic = true;
        }

        /// <summary>
        /// Called when save topic is clicked.
        /// </summary>
        private void OnSaveToppic()
        {
            Validate();
            if (SelectedTopic.IsNew)
                if (string.IsNullOrWhiteSpace(NewTopicName) || HasErrors) // || NewTopicCategoryType == SelectAType)
                    return;


            SelectedTopic.TopicCategory.Name = NewTopicName;

            var newTopic = !SelectedTopic.IsNew ? SelectedTopic.TopicCategory :
            new TopicCategory(NewTopicName)
            {
                TopicType = TopicTypeEnum.Hospital,
                //  CategoryType = EnumExtensions.GetEnumValueFromString<TopicCategoryTypeEnum>(NewTopicCategoryType),
                CategoryType = TopicCategoryTypeEnum.Topic,
                IsUserCreated = true
            };

            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.SaveOrUpdate(newTopic);
                    tx.Commit();
                }
            }

            IsAddingNewTopic = false;
            OnLoad();
        }

        /// <summary>
        /// Initializes the properties.
        /// </summary>
        protected override void InitProperties()
        {
            Reset();
            HeaderName = MANAGE_TOPICS;
            SelectedTopicTypeFilter = FilterByList.FirstOrDefault();
            SelectedFilter = FilterEnumeration.FirstOrDefault();
            FilterText = string.Empty;
            NewTopicCategoryType = SelectAType;
            RaisePropertyChanged(() => HeaderText);
        }

        /// <summary>
        /// Handles the IsActiveChanged event of the ListTabViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        protected override void ListTabViewModel_IsActiveChanged(object sender, EventArgs e)
        {
            base.ListTabViewModel_IsActiveChanged(sender, e);

            NewTopicCategoryType = SelectAType;
            RaisePropertyChanged(() => NewTopicCategoryType);
        }

        /// <summary>
        /// Executes the load.
        /// </summary>
        /// <param name="session">The session.</param>
        protected override void ExecLoad(ISession session)
        {
            base.ExecLoad(session);
            CollectionItems = new ListCollectionView(
                session.Query<TopicCategory>()
                .OrderByDescending(x => x.IsUserCreated ? x.DateCreated : x.DateCreated.Value.Date)
                .ThenBy(x => x.Name)
                .Select(x => new TopicViewModel(x)).ToList());
            InitProperties();
        }

        /// <summary>
        /// Adds the new topic.
        /// </summary>
        private void AddNewTopic()
        {
            SelectedTopic = new TopicViewModel("") { IsNew = true };
            IsAddingNewTopic = true;
        }

        /// <summary>
        /// Determines whether this instance [can add new topic].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can add new topic]; otherwise, <c>false</c>.
        /// </returns>
        private static bool CanAddNewTopic()
        {
            return true;
        }



        /// <summary>
        /// Resets this instance.
        /// </summary>
        public override void Reset()
        {

            NewTopicName = string.Empty;
            ClearErrors(() => NewTopicName);
        }

        /// <summary>
        /// Topics the filter.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        private bool TopicFilter(object o)
        {
            var model = o as TopicViewModel;
            if (model == null || (string.IsNullOrEmpty(FilterText) && SelectedTopicTypeFilter == TopicsAndConditions)) return true;

            if (!string.IsNullOrEmpty(FilterText) && SelectedTopicTypeFilter != TopicsAndConditions)
            {
                return model.TopicName.ContainsIgnoreCase(FilterText) && model.CategoryType == SelectedTopicTypeFilter;
            }
            else if (SelectedTopicTypeFilter == TopicsAndConditions && !string.IsNullOrEmpty(FilterText))
            {
                return model.TopicName.ContainsIgnoreCase(FilterText) || model.TopicCategory.Topics.Any(x => x.Name.ContainsIgnoreCase(FilterText));
            }
            else
                return model.CategoryType == SelectedTopicTypeFilter;
        }

        /// <summary>
        /// Called when topic is to be deleted
        /// </summary>
        /// <param name="topicViewModel">The topic view model.</param>
        private void OnDelete(TopicViewModel topicViewModel)
        {
            if (topicViewModel == null) return;

            // query to find out if this topic is assigned to any measures
            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    var topicCategory = session.Query<TopicCategory>().SingleOrDefault(a => a.Id == topicViewModel.TopicCategory.Id);

                    if (topicCategory == null) return;

                    var topics = topicCategory.Topics.Select(x => x.Id);
                    var associatedMeasures = session.Query<Measure>().Where(m => m.MeasureTopics.Any(mt => topics.Contains(mt.Topic.Id)));

                    var mcount = associatedMeasures.Distinct().Count();

                    if (mcount > 0)
                    {
                        System.Windows.MessageBox.Show(string.Format("Please delete all subtopic associations with measures before deleting this topic \"{0}\". There are currently {1} measure(s) associated with this topic",
                                   topicViewModel.TopicName, mcount),
                               "Delete Topic", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }

                    //if (MessageBox.Show(string.Format("This topic ({0}) is used in {1} measure(s). Are you sure you want to delete it?", topicViewModel.TopicName, mcount),
                    //       "Delete Topic?", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    //    return;

                    session.Evict(topicCategory);
                    session.Delete(topicCategory);

                    tx.Commit();
                }
            }
            OnLoad();
        }

        /// <summary>
        /// Gets the topic category.
        /// </summary>
        /// <param name="topicCategoryId">The topic category identifier.</param>
        /// <returns></returns>
        private TopicCategory GetTopicCategory(int topicCategoryId)
        {
            using (var session = DataserviceProvider.SessionFactory.OpenSession())
            {
                return
                    session.Query<TopicCategory>().SingleOrDefault(a => a.Id == topicCategoryId);
            }
        }


        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            // throw new NotImplementedException();
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
            //throw new NotImplementedException();
            return true;
        }

        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //throw new NotImplementedException();
        }

        #endregion

    }
}
