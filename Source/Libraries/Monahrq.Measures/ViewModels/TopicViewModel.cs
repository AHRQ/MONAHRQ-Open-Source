using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Measures.Events;

namespace Monahrq.Measures.ViewModels
{
    /// <summary>
    /// View model class for Topics
    /// </summary>
    /// <seealso cref="Monahrq.Measures.ViewModels.TreeViewItemViewModel" />
    public class TopicViewModel : TreeViewItemViewModel
    {

        #region Fields and Constants

        /// <summary>
        /// The new subtopic title
        /// </summary>
        private string _newSubtopicTitle;


        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the add subtopic command.
        /// </summary>
        /// <value>
        /// The add subtopic command.
        /// </value>
        public DelegateCommand<object> AddSubtopicCommand { get; set; }
        /// <summary>
        /// Gets or sets the cancel subtopic command.
        /// </summary>
        /// <value>
        /// The cancel subtopic command.
        /// </value>
        public DelegateCommand<object> CancelSubtopicCommand { get; set; }
        /// <summary>
        /// Gets or sets the enable add subtopic command.
        /// </summary>
        /// <value>
        /// The enable add subtopic command.
        /// </value>
        public DelegateCommand EnableAddSubtopicCommand { get; set; }
        /// <summary>
        /// Gets or sets the enable topic edit command.
        /// </summary>
        /// <value>
        /// The enable topic edit command.
        /// </value>
        public DelegateCommand EnableTopicEditCommand { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Delegates the ItemSelectionChanged event
        /// </summary>
        /// <param name="topicVm">The topic vm.</param>
        /// <param name="isSelected">if set to <c>true</c> [is selected].</param>
        public delegate void ItemSelectionChanged(TopicViewModel topicVm, bool isSelected);
        /// <summary>
        /// Occurs when item selection is changed.
        /// </summary>
        public event ItemSelectionChanged ItemSelectionChangedEvent;

        /// <summary>
        /// Called when item selection changed event is fired.
        /// </summary>
        /// <param name="topicVm">The topic vm.</param>
        /// <param name="isSelected">if set to <c>true</c> [is selected].</param>
        protected virtual void OnItemSelectionChangedEvent(TopicViewModel topicVm, bool isSelected)
        {
            var handler = ItemSelectionChangedEvent;
            if (handler != null) handler(topicVm, isSelected);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public new bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                base.IsSelected = value;
                OnItemSelectionChangedEvent(this, value);
                RaisePropertyChanged(() => IsSelected);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the topic is user created.
        /// </summary>
        /// <value>
        /// <c>true</c> if the topic is created by user; otherwise, <c>false</c>.
        /// </value>
        public override bool IsUserCreated
        {
            get { return TopicCategory.IsUserCreated; }
        }

        /// <summary>
        /// Gets the selected topics.
        /// </summary>
        /// <value>
        /// The selected topics.
        /// </value>
        public IList<Topic> SelectedTopics
        {
            get
            {
                return ChildrenCollectionView.OfType<SubTopicViewModel>()
                    .Where(s => s.IsSelected)
                    .Select(s => s.Entity)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether topic is new.
        /// </summary>
        /// <value>
        ///   <c>true</c> if topic is new; otherwise, <c>false</c>.
        /// </value>
        public bool IsNew { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicViewModel"/> class.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        public TopicViewModel(string topicName)
            : base(null)
        {
            TopicCategory = new TopicCategory(topicName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicViewModel"/> class.
        /// </summary>
        /// <param name="ts">The ts.</param>
        public TopicViewModel(TopicCategory ts)
            : base(null)
        {
            _oldTopicCategoryName = ts.Name;
            IsSubTopic = false;
            TopicCategory = ts;
            LoadChildren(ts.Topics);
            AddSubtopicCommand = new DelegateCommand<Object>(OnAddSubtopic, CanExecuteAdd);
            CancelSubtopicCommand = new DelegateCommand<object>(OnCancel);
            EnableAddSubtopicCommand = new DelegateCommand(OnEnableAddSubtopic);
            EnableTopicEditCommand = new DelegateCommand(OnEnableEdit);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called on cancel action.
        /// </summary>
        /// <param name="o">The o.</param>
        private void OnCancel(object o)
        {
            NewSubtopicTitle = string.Empty;
            ClearErrors(() => NewSubtopicTitle);
            Reset(false);
            AddSubtopicCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Determines whether this instance [can execute add] the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can execute add] the specified name; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecuteAdd(object name)
        {
            return !HasErrors && !Committed;
        }

        /// <summary>
        /// Called when [add subtopic].
        /// </summary>
        /// <param name="name">The name.</param>
        private void OnAddSubtopic(object name)
        {
            ValidateNewSubtopicTitle();

            if (HasErrors) return;

            var newtopic = new Topic(TopicCategory, NewSubtopicTitle) { IsUserCreated = true };
            MeasureService.SaveOrUpdateTopic(newtopic.Owner);
            NewSubtopicTitle = string.Empty;
            ClearErrors(ExtractPropertyName(() => NewSubtopicTitle));
            Reset(false);
            Events.GetEvent<TopicsUpdatedEvent>().Publish(newtopic.Id);

        }

        /// <summary>
        /// Called when [enable add subtopic].
        /// </summary>
        private void OnEnableAddSubtopic()
        {
            Reset(false);
            AddSubtopicCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Called when edit is enabled.
        /// </summary>
        private void OnEnableEdit()
        {
            Reset(false);
            CommitCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Gets or sets the new subtopic title.
        /// </summary>
        /// <value>
        /// The new subtopic title.
        /// </value>
        public string NewSubtopicTitle
        {
            get { return _newSubtopicTitle; }
            set
            {
                if (_newSubtopicTitle == value) return;
                _newSubtopicTitle = value;
                RaisePropertyChanged();
                ValidateNewSubtopicTitle();
            }
        }

        /// <summary>
        /// Validates the new subtopic title.
        /// </summary>
        private void ValidateNewSubtopicTitle()
        {
            var prop = ExtractPropertyName(() => NewSubtopicTitle);
            ClearErrors(prop);

            if (string.IsNullOrWhiteSpace(NewSubtopicTitle))
            {
                SetError(prop, "Subtopic name cannot be empty.");
            }

            AddSubtopicCommand.RaiseCanExecuteChanged();
        }

        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Monahrq.Measures.ViewModels.TreeViewItemViewModel" />
    public class SubTopicViewModel : TreeViewItemViewModel
    {
        /// <summary>
        /// Gets a value indicating whether sub topic is user created.
        /// </summary>
        /// <value>
        /// <c>true</c> if sub topic is user created; otherwise, <c>false</c>.
        /// </value>
        public override bool IsUserCreated
        {
            get { return Entity != null && Entity.IsUserCreated; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubTopicViewModel"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent.</param>
        public SubTopicViewModel(string name, ITreeViewItemViewModel parent)
            : base(null)
        {
            Entity = new Topic(parent.TopicCategory, name);
            _oldSubtopicName = Entity.Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubTopicViewModel"/> class.
        /// </summary>
        /// <param name="subtopic">The subtopic.</param>
        /// <param name="parent">The parent.</param>
        public SubTopicViewModel(Topic subtopic, ITreeViewItemViewModel parent)
            : base(parent)
        {
            IsSubTopic = true;
            Entity = subtopic;
            _oldSubtopicName = Entity.Name;
        }
    }
}
