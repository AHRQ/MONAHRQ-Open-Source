using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Services;
using Monahrq.Measures.Events;
using System.Windows.Data;
using Monahrq.Infrastructure;
using Monahrq.Measures.Service;
using Monahrq.Sdk.Utilities;
using NHibernate.Linq;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Events;

namespace Monahrq.Measures.ViewModels
{
    using Monahrq.Sdk.Extensions;

    /// <summary>
    /// Class for Tree view item
    /// </summary>
    /// <seealso cref="Monahrq.Default.ViewModels.BaseViewModel" />
    /// <seealso cref="Monahrq.Measures.ViewModels.ITreeViewItemViewModel" />
    public abstract class TreeViewItemViewModel : BaseViewModel, ITreeViewItemViewModel
    {
        #region Fields and Constants

        /// <summary>
        /// The is subtopic
        /// </summary>
        private bool _isSubtopic;
        /// <summary>
        /// The old subtopic name
        /// </summary>
        protected string _oldSubtopicName;
        /// <summary>
        /// The is selected
        /// </summary>
        private bool _isSelected;
        /// <summary>
        /// The old topic category name,stores original name to facilitate cancel on edit name
        /// </summary>
        protected string _oldTopicCategoryName; //store original name to facilitate cancel on edit name

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the cancel command.
        /// </summary>
        /// <value>
        /// The cancel command.
        /// </value>
        public DelegateCommand<object> CancelCommand { get; set; }
        /// <summary>
        /// Gets or sets the delete subtopic command.
        /// </summary>
        /// <value>
        /// The delete subtopic command.
        /// </value>
        public DelegateCommand<object> DeleteSubtopicCommand { get; set; }
        /// <summary>
        /// Gets or sets the enable edit subtopic command.
        /// </summary>
        /// <value>
        /// The enable edit subtopic command.
        /// </value>
        public DelegateCommand EnableEditSubtopicCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the measure service.
        /// </summary>
        /// <value>
        /// The measure service.
        /// </value>
        protected IMeasureServiceSync MeasureService { get; } = ServiceLocator.Current.GetInstance<IMeasureServiceSync>();

        /// <summary>
        /// Gets the event aggregator.
        /// </summary>
        /// <value>
        /// The event aggregator.
        /// </value>
        protected IEventAggregator EventAggregator { get; } = ServiceLocator.Current.GetInstance<IEventAggregator>();

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        protected ILogWriter Logger { get; } = ServiceLocator.Current.GetInstance<ILogWriter>();

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public Topic Entity { get; protected set; }

        /// <summary>
        /// Gets or sets the topic category.
        /// </summary>
        /// <value>
        /// The topic category.
        /// </value>
        public TopicCategory TopicCategory { get; protected internal set; }


        /// <summary>
        /// Gets or sets a value indicating this is sub topic.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is sub topic; otherwise, <c>false</c>.
        /// </value>
        public bool IsSubTopic
        {
            get { return _isSubtopic; }
            set
            {
                _isSubtopic = value;
                Committed = false;
                RaiseErrorsChanged(() => IsSubTopic);
            }
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id
        {
            get { return Entity == null ? TopicCategory.Id : Entity.Id; }
        }


        /// <summary>
        /// Gets or sets the name of the topic.
        /// </summary>
        /// <value>
        /// The name of the topic.
        /// </value>
        public string TopicName
        {
            get { return TopicCategory.Name; }
            set
            {
                if (value == TopicCategory.Name) return;
                TopicCategory.Name = value;
                RaisePropertyChanged(() => TopicName);
                ValidateName(ExtractPropertyName(() => TopicName), value);
            }
        }

        /// <summary>
        /// Gets or sets the type of the category.
        /// </summary>
        /// <value>
        /// The type of the category.
        /// </value>
        public string CategoryType
        {
            get { return TopicCategory.CategoryType.ToString(); }
            set { TopicCategory.CategoryType = EnumExtensions.GetEnumValueFromString<TopicCategoryTypeEnum>(value); }
        }


        /// <summary>
        /// Gets or sets the name of the subtopic.
        /// </summary>
        /// <value>
        /// The name of the subtopic.
        /// </value>
        public string SubtopicName
        {
            get { return Entity == null ? string.Empty : Entity.Name; }
            set
            {
                if (value == Entity.Name) return;
                Entity.Name = value;
                RaisePropertyChanged(() => SubtopicName);
                ValidateName(ExtractPropertyName(() => SubtopicName), value);
                Committed = false;
            }
        }

        //todo : should it be public or protected?

        /// <summary>
        /// Gets or sets the children collection view.
        /// </summary>
        /// <value>
        /// The children collection view.
        /// </value>
        public ObservableCollection<ITreeViewItemViewModel> ChildrenCollectionView { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                Recurse(ChildrenCollectionView, _isSelected);
                RaiseErrorsChanged(() => IsSelected);
                Committed = false;
            }
        }

        //todo: move visibility property to control exstension it should not be here: 
        //todo: INGA this UI issue .
        /// <summary>
        /// Gets or sets the visibility.
        /// </summary>
        /// <value>
        /// The visibility.
        /// </value>
        public Visibility Visibility { get; set; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public ITreeViewItemViewModel Parent { get; private set; }

        /// <summary>
        /// Gets the topic category types.
        /// </summary>
        /// <value>
        /// The topic category types.
        /// </value>
        public List<string> TopicCategoryTypes
        {
            get { return EnumExtensions.GetEnumStringValues<TopicCategoryTypeEnum>(); }
        }
        // 752 quynnorchard bld : patomac oaks : 101

        /// <summary>
        /// Gets or sets a value indicating whether this instance is user created.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is user created; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsUserCreated { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitCommands()
        {
            /*Save command is generic and implemented in a base class CommitCommand*/
            CancelCommand = new DelegateCommand<object>(obj => CancelConfirm());
            DeleteSubtopicCommand = new DelegateCommand<object>(DeleteSubtopic, CanDeleteSubtopic);
            EnableEditSubtopicCommand = new DelegateCommand(OnEnableEditSubtopic);

            //            PropertyChanged += (o, e) =>
            //                {
            //                    if (e.PropertyName != "IsSelected") return;

            ////                    if (TriggeredSelection == null)
            ////                    {
            ////                        TriggeredSelection = o as ITreeViewItemViewModel;
            ////                    }
            ////                    if (o == TriggeredSelection)
            ////                    {
            ////                        if (ChildrenCollectionView != null)
            ////// ReSharper disable RedundantEnumerableCastCall
            ////                            Recurse(ChildrenCollectionView.OfType<ITreeViewItemViewModel>(), IsSelected);
            ////// ReSharper restore RedundantEnumerableCastCall
            ////                        if (Parent != null && !IsSelected)
            ////                        {
            ////                            Parent.IsSelected = false;
            ////                        }
            ////                        if (Parent!=null)
            ////                        {

            ////                            EventAggregator.GetEvent<TopicSelectedChanged>().Publish(null);
            ////                        }
            ////                    }
            ////                    TriggeredSelection = null;
            //                };
        }

        /// <summary>
        /// Loads the children.
        /// </summary>
        /// <param name="children">The children.</param>
        public void LoadChildren(IEnumerable<Topic> children)
        {
            children = children.Where(c => !c.Name.Contains("--")) ?? Enumerable.Empty<Topic>();          
            var items = children.OrderBy(t => t.Name).Select(child => new SubTopicViewModel(child, this));
       
            ChildrenCollectionView = new ObservableCollection<ITreeViewItemViewModel>(items);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewItemViewModel"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        protected TreeViewItemViewModel(ITreeViewItemViewModel parent)
        {
            InitCommands();

            Parent = parent;
            ChildrenCollectionView = new ObservableCollection<ITreeViewItemViewModel>();

            Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Recurses the specified children.
        /// </summary>
        /// <param name="children">The children.</param>
        /// <param name="select">if set to <c>true</c> [select].</param>
        private static void Recurse(IEnumerable<ITreeViewItemViewModel> children, bool select)
        {
            foreach (var item in children
                                 ?? Enumerable.Empty<ITreeViewItemViewModel>())
            {
                item.IsSelected = select;
                Recurse(item.ChildrenCollectionView, select);
            }
        }

        /// <summary>
        /// Validates the name.
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <param name="value">The value.</param>
        private void ValidateName(string prop, string value)
        {
            ClearErrors(prop);

            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
            {
                SetError(prop, "Name cannot be empty.");
            }
        }

        /// <summary>
        /// Use this to validate any late-bound fields before committing
        /// </summary>
        protected override void ValidateAll()
        {
            if (TopicCategory != null)
            {
                ValidateName(ExtractPropertyName(() => TopicName), TopicCategory.Name);
            }
            if (Entity != null)
            {
                ValidateName(ExtractPropertyName(() => SubtopicName), Entity.Name);
            }
        }

        /// <summary>
        /// Called when sub topic edit is enabled.
        /// </summary>
        private void OnEnableEditSubtopic()
        {
            ClearErrors(() => SubtopicName);
            Reset(false);
            CommitCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Use this to perform whatever action is needed when committed
        /// </summary>
        /// <returns></returns>
        protected override Task<bool> OnCommitted()
        {
            //todo: crappy code alert. refactor
            try
            {
                if (TopicCategory != null)
                {
                    Debug.Assert(MeasureService != null);
                    //todo: add loging 
                    //todo: who is working on exception handling architecture
                    //todo we need this (error handling procedure) documented
                    MeasureService.SaveOrUpdateTopic(TopicCategory);
                    _oldTopicCategoryName = TopicCategory.Name;
                }
                if (Entity != null)
                {
                    MeasureService.SaveOrUpdateTopic(Entity.Owner);
                    _oldSubtopicName = Entity.Name;
                }

                Events.GetEvent<TopicsUpdatedEvent>().Publish(1);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Events.GetEvent<ErrorNotificationEvent>().Publish(ex);
                return Task.FromResult(false);
            }
            finally
            {
                Reset(false); // => invoke reset as non cancel action 
            }
        }

        /// <summary>
        /// Cancels the confirm.
        /// </summary>
        protected void CancelConfirm()
        {
            Reset(true);
        }

        /// <summary>
        /// Resets the specified is cancel.
        /// </summary>
        /// <param name="isCancel">if set to <c>true</c> [is cancel].</param>
        protected void Reset(bool isCancel)
        {
            /*Cancel changes made to the name, and restore original value*/
            if (isCancel && !Committed)
            {
                if (TopicCategory != null)
                {
                    TopicName = _oldTopicCategoryName;

                }
                if (Entity != null)
                {
                    SubtopicName = _oldSubtopicName;
                }
            }

            Committed = true;
            DeleteSubtopicCommand.RaiseCanExecuteChanged();
        }

        /* Check if any measures depend on this subtopic, and if so, get user confirmation */

        /// <summary>
        /// Determines whether this instance [can delete subtopic] the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can delete subtopic] the specified argument; otherwise, <c>false</c>.
        /// </returns>
        private static bool CanDeleteSubtopic(object arg)
        {
            return true;
        }

        /// <summary>
        /// Deletes the subtopic.
        /// </summary>
        /// <param name="subtopic">The subtopic.</param>
        public void DeleteSubtopic(object subtopic)
        {
            var vm = subtopic as SubTopicViewModel;
            if (vm == null) return;
            var count = 0;
            var measures = new List<MeasureTopic>();
          
            // query to find out if this subtopic is assigned to any measures
            var dataServiceProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();

            using (var session = dataServiceProvider.SessionFactory.OpenSession())
            {
                measures = session.Query<MeasureTopic>().Where(m => m.Measure != null && m.Measure.Id == vm.Entity.Id).ToList();
                count = session.Query<MeasureTopic>().Count(m => m.Measure != null && m.Measure.Id == vm.Entity.Id);
                //measures = session.Query<Measure>().Where(m => m.Topics.Any(t => t.Id == vm.Entity.Id)).ToList();
            }
         
            var warningMessage = count > 0
                ? string.Format(
                    "Are you sure you want to delete this subtopic:\n\n{0} \n\nDeleting it will also remove it from the {1} measure(s) in which it is used.",
                    vm.SubtopicName, count)
                : string.Format(
                    "Are you sure you want to delete this subtopic:\n\n{0} \n\nThis subtopic is not currently used by any measures, it is safe to delete.",
                    vm.SubtopicName);

            // Prompt user to confirm delete
            //TODO add normal pop-up
            if (
                MessageBox.Show(warningMessage, @"Delete Subtopic?", MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                MessageBoxResult.Yes)
                return;

            var deleteStatement =
                string.Format("Delete from {0}_MeasureTopics Where Measure_Id in ({1}) and Topic_Id = {2}",
                    Inflector.Pluralize(typeof(Measure).Name), string.Join(",", measures.Select(x => x.Id)),
                    vm.Entity.Id);
            var name = vm.SubtopicName;

            try
            {
                using (var session = dataServiceProvider.SessionFactory.OpenSession())
                {
                    using (var trx = session.BeginTransaction())
                    {
                        if (count > 0) session.CreateSQLQuery(deleteStatement).ExecuteUpdate();
                        session.Delete(vm.Entity);
                        trx.Commit();
                    }
                }
                EventAggregator.GetEvent<GenericNotificationEvent>().Publish(string.Format("{0} has been deleted.", name));
            }
            catch (Exception exec)
            {
                Logger.Write(exec);
            }

            RaiseErrorsChanged(() => SubtopicName);
            Events.GetEvent<TopicsUpdatedEvent>().Publish(1);
        }

        #endregion

    }
}
