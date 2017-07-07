using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using System.Windows.Data;
using Monahrq.Websites.Events;

namespace Monahrq.Websites.ViewModels
{
    public abstract class TreeViewItemViewModel : BaseViewModel, ITreeViewItemViewModel
    {
        //public DelegateCommand<object> CancelCommand { get; set; }
        //public DelegateCommand<object> DeleteSubtopicCommand { get; set; }
        //public DelegateCommand EnableEditSubtopicCommand { get; set; }

        #region Properties

        //protected IWebsiteMeasureService MeasureService
        //{
        //    get { return ServiceLocator.Current.GetInstance<IWebsiteMeasureService>(); }
        //}

        public Topic Entity { get; protected set; }
        public TopicCategory TopicCategory { get; protected set; }

        private bool _isSubtopic;

        public bool IsSubTopic
        {
            get { return _isSubtopic; }
            set
            {
                _isSubtopic = value;
                RaisePropertyChanged();
            }
        }

        public int Id
        {
            get { return Entity == null ? TopicCategory.Id : Entity.Id; }
        }

        protected string _oldTopicCategoryName; //store original name to facilitate cancel on edit name

        public string TopicName
        {
            get { return TopicCategory.Name; }
            set
            {
                if (value == TopicCategory.Name) return;
                TopicCategory.Name = value;
                RaisePropertyChanged(() => TopicName);
                _ValidateName(ExtractPropertyName(() => TopicName), value);
            }
        }

        protected string _oldSubtopicName;

        public string SubtopicName
        {
            get { return Entity == null ? string.Empty : Entity.Name; }
            set
            {
                if (value == Entity.Name) return;
                Entity.Name = value;
                RaisePropertyChanged(() => SubtopicName);
                _ValidateName(ExtractPropertyName(() => SubtopicName), value);
            }
        }

        public ObservableCollection<ITreeViewItemViewModel> ChildrenCollectionView { get; protected set; }
        public bool IsSelected { get; set; }
        public Visibility Visibility { get; set; }
        public ITreeViewItemViewModel Parent { get; private set; }
        //protected ObservableCollection<ITreeViewItemViewModel> Childitems { get; set; }

        #endregion

        #region Methods

        private void InitCommands()
        {
            /*Save command is generic and implemented in a base class CommitCommand*/
            //CancelCommand = new DelegateCommand<object>(obj => _CancelConfirm());
            //DeleteSubtopicCommand = new DelegateCommand<object>(DeleteSubtopic, CanDeleteSubtopic);
            //EnableEditSubtopicCommand = new DelegateCommand(OnEnableEditSubtopic);

            PropertyChanged += (o, e) =>
                {
                    if (e.PropertyName != "IsSelected") return;

                    if (TriggeredSelection == null)
                    {
                        TriggeredSelection = o as ITreeViewItemViewModel;
                    }
                    if (o == TriggeredSelection)
                    {
                        Recurse(ChildrenCollectionView.OfType<ITreeViewItemViewModel>(), this.IsSelected);
                        if (Parent != null && !IsSelected)
                        {
                            Parent.IsSelected = false;
                        }
                        if (Parent != null)
                        {

                            Events.GetEvent<TopicSelectedChanged>().Publish(null);
                        }
                    }
                    TriggeredSelection = null;
                };
        }

        public void LoadChildren(IEnumerable<Topic> children)
        {
            //children = children ?? Enumerable.Empty<Topic>();
            //var items = children.OrderBy(t => t.Name).Select(child => new SubTopicViewModel(child, this));
            //Childitems = new ObservableCollection<ITreeViewItemViewModel>(items);
            //ChildrenCollectionView = CollectionViewSource.GetDefaultView(Childitems) as ListCollectionView;
            children = children ?? Enumerable.Empty<Topic>();
            var items = children.OrderBy(t => t.Name).Select(child => new SubTopicViewModel(child, this));
            ChildrenCollectionView = new ObservableCollection<ITreeViewItemViewModel>(items);
        }

        private static ITreeViewItemViewModel TriggeredSelection { get; set; }

        protected TreeViewItemViewModel(ITreeViewItemViewModel parent)
        {
            InitCommands();

            Parent = parent;
            ChildrenCollectionView = new ObservableCollection<ITreeViewItemViewModel>();

            Visibility = Visibility.Visible;
        }

        private void Recurse(IEnumerable<ITreeViewItemViewModel> children, bool select)
        {
            foreach (var item in children
                                ?? Enumerable.Empty<ITreeViewItemViewModel>())
            {
                item.IsSelected = select;
                Recurse(item.ChildrenCollectionView.OfType<ITreeViewItemViewModel>(), select);
            }
        }

        #endregion

        #region Input Validation

        private void _ValidateName(string prop, string value)
        {
            ClearErrors(prop);

            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
            {
                SetError(prop, "Name cannot be empty.");
            }
        }

        protected override void ValidateAll()
        {
            if (TopicCategory != null)
            {
                _ValidateName(ExtractPropertyName(() => TopicName), TopicCategory.Name);
            }
            if (Entity != null)
            {
                _ValidateName(ExtractPropertyName(() => SubtopicName), Entity.Name);
            }
        }

        #endregion

        #region Commands

        private void OnEnableEditSubtopic()
        {
            ClearErrors(() => SubtopicName);
            _Reset(false);
            CommitCommand.RaiseCanExecuteChanged();
        }

        /*On Cancel , reset changes and close edit mode*/

        protected void _CancelConfirm()
        {
            _Reset(true);
        }

        protected void _Reset(bool isCancel)
        {
            //*Cancel changes made to the name, and restore original value*/
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

            //Committed = true;
            //DeleteSubtopicCommand.RaiseCanExecuteChanged();
        }

        /* Check if any measures depend on this subtopic, and if so, get user confirmation
        */

        //private bool CanDeleteSubtopic(object arg)
        //{
        //    return true;
        //}

        //    public async void DeleteSubtopic(object subtopic)
        //    {
        //        var vm = subtopic as SubTopicViewModel;
        //        if (vm == null) return;

        //        string warningMessage;

        //        // query to find out if this subtopic is assigned to any measures
        //        int count = 0;
        //        var service = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
        //        using (var session = service.SessionFactory.OpenSession())
        //        {
        //            var topic = session.Query<Topic>().Where(a => a.Id == vm.Entity.Id).SingleOrDefault();
        //            if (topic != null)
        //                count = topic.Measures.Count;
        //        }

        //        if (count > 0)
        //        {
        //            warningMessage = string.Format("Are you sure you want to delete this subtopic:\n\n{0} \n\nDeleting it will also remove it from the {1} measure(s) in which it is used.", vm.SubtopicName, count);
        //        }
        //        else
        //        {
        //            warningMessage= string.Format("Are you sure you want to delete this subtopic:\n\n{0} \n\nThis subtopic is not currently used by any measures, it is safe to delete.", vm.SubtopicName);
        //        }

        //        // Prompt user to confirm delete
        //        //TODO add normal pop-up
        //        if (MessageBox.Show(warningMessage,
        //                            "Delete Subtopic?",
        //                            MessageBoxButton.YesNo,
        //                            MessageBoxImage.Question)
        //            != MessageBoxResult.Yes)
        //            return;

        //        MeasureService.DeleteTopic(vm.Entity,true);
        //        ChildrenCollectionView.Remove(vm);
        //        ChildrenCollectionView.Refresh();
        //    }

        #endregion

        //    bool IsMeasureServiceReady { get; set; }
        }
}
