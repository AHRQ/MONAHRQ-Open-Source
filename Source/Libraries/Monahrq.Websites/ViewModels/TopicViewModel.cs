using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using PropertyChanged;

namespace Monahrq.Websites.ViewModels
{
    [ImplementPropertyChanged]
    public class TopicViewModel : TreeViewItemViewModel
    {
        public DelegateCommand<object> AddSubtopicCommand { get; set; }
        public DelegateCommand<object> CancelSubtopicCommand { get; set; }
        public DelegateCommand EnableAddSubtopicCommand { get; set; }
        public DelegateCommand EnableTopicEditCommand { get; set; }

        public TopicViewModel(string topicName)
            : base(null)
        {
            TopicCategory = new TopicCategory(topicName);
        }

        public TopicViewModel(TopicCategory ts)
            : base(null)
        {
            _oldTopicCategoryName = ts.Name;
            IsSubTopic = false;
            TopicCategory = ts;
            LoadChildren(ts.Topics);
            //AddSubtopicCommand = new DelegateCommand<Object>(OnAddSubtopic, CanExecuteAdd);
            //CancelSubtopicCommand = new DelegateCommand<object>(OnCancel);
            //EnableAddSubtopicCommand = new DelegateCommand(OnEnableAddSubtopic);
            //EnableTopicEditCommand=new DelegateCommand(OnEnableEdit);
        }

        //private void OnCancel(object o)
        //{
        //    NewSubtopicTitle = string.Empty;
        //    ClearErrors(()=>NewSubtopicTitle);
        //   _Reset(false);
        //   AddSubtopicCommand.RaiseCanExecuteChanged();
        //}

        //private bool CanExecuteAdd(object name)
        //{
        //    return !HasErrors && !Committed;
        //}

        //private async void OnAddSubtopic(object name)
        //{
        //    //_ValidateNewSubtopicTitle();

        //    //if (HasErrors) return;

        //    //var newtopic = new Topic(TopicCategory, NewSubtopicTitle);
        //    //MeasureService.SaveOrUpdateTopic(newtopic.Owner);
        //    //NewSubtopicTitle = string.Empty;
        //    //_Reset(false);
        //}

        //private void OnEnableAddSubtopic()
        //{
        //    _Reset(false);
        //    AddSubtopicCommand.RaiseCanExecuteChanged();
        //}

        //private void OnEnableEdit()
        //{
        //    _Reset(false);
        //    CommitCommand.RaiseCanExecuteChanged();
        //}

        public IList<Topic> SelectedTopics
        {
            get
            {
                var returnValue =  ChildrenCollectionView.OfType<SubTopicViewModel>()
                    .Where(s => s.IsSelected)
                    .Select(s => s.Entity)
                    .ToList();

                return returnValue;
            }
        }

        private string _newSubtopicTitle;
        public string NewSubtopicTitle
        {
            get { return _newSubtopicTitle; }
            set
            {
                if (_newSubtopicTitle == value) return;
                _newSubtopicTitle = value;
                RaisePropertyChanged();
                _ValidateNewSubtopicTitle();
            }
        }
    
        private void _ValidateNewSubtopicTitle()
        {
            var prop = ExtractPropertyName(() => NewSubtopicTitle);
            ClearErrors(prop);

            if (string.IsNullOrEmpty(_newSubtopicTitle))
            {
                SetError(prop, "Subtopic name cannot be empty.");
            }

            //AddSubtopicCommand.RaiseCanExecuteChanged();
        }
    }

    [ImplementPropertyChanged]
    public class SubTopicViewModel : TreeViewItemViewModel
    {
        public SubTopicViewModel(string name, ITreeViewItemViewModel parent)
            : base(null)
        {
            Entity = new Topic(parent.TopicCategory, name);
            _oldSubtopicName = Entity.Name;
        }
       
        public SubTopicViewModel(Topic subtopic, ITreeViewItemViewModel parent)
            : base(parent)
        {
            IsSubTopic = true;
            Entity = subtopic;
            _oldSubtopicName = Entity.Name;
        }
    }
}
