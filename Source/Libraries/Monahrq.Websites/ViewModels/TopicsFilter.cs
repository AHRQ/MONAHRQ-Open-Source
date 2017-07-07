using Microsoft.Practices.Prism.Commands;
using Monahrq.Websites.Events;
using PropertyChanged;
using System;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;

namespace Monahrq.Websites.ViewModels
{
    [ImplementPropertyChanged]
    public class TopicsFilter
    {
        private ManageTopicsViewModel Model { get; set; }

        public TopicsFilter(ManageTopicsViewModel vm)
        {
            Model = vm;
            Model.FilterTopicCommand = new DelegateCommand(ApplyTopicsFilter);

            Model.PropertyChanged += (sender, args) =>
            {
                if (string.Equals(args.PropertyName, "FilterText", StringComparison.OrdinalIgnoreCase))
                {
                    Model.FilterTopicCommand.Execute();
                }
            };
        }

        void ApplyTopicsFilter()
        {
            try
            {
                //if (Model.TopicsCollectionView.IsEditingItem) return;
                //Model.TopicsCollectionView.Filter = (o) => TopicCategoryFilter(o);
                //Model.TopicsCollectionView.OfType<ITreeViewItemViewModel>()
                //    .ForEach(model => model.ChildrenCollectionView.Filter = (o) => TopicFilter(o));
                Model.TopicsCollectionView.MoveCurrentToFirst();
            }
            finally
            {
                ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<TopicFilterApplied>().Publish(EventArgs.Empty);
            }

        }

        public Predicate<object> TopicFilter
        {
            get
            {
                if (Model.TopicsCollectionView == null
                  || string.IsNullOrEmpty((Model.FilterText ?? string.Empty).Trim()))
                {
                    return o => true;
                }
                return o => ModelTopicFilter(o as SubTopicViewModel);
            }
        }

        private bool ModelTopicFilter(SubTopicViewModel subTopicViewModel)
        {
            var parent = subTopicViewModel.Parent as TopicViewModel;
            return parent != null && parent.TopicCategory.Name.ToUpper().Contains(Model.FilterText.ToUpper()) ||
                   subTopicViewModel.Entity.Name.ToUpper().Contains(Model.FilterText.ToUpper());
        }


        public Predicate<object> TopicCategoryFilter
        {
            get 
            {
                return Model.TopicsCollectionView == null || string.IsNullOrEmpty((Model.FilterText ?? string.Empty).Trim())
                           ? (Predicate<object>) (o => true)
                           : (o => ModelCategoryFilter(o as TopicViewModel));
            }
        }

        private bool ModelCategoryFilter(TopicViewModel topicViewModel)
        {
            if (topicViewModel == null) return false;
            var isMatch = topicViewModel.TopicCategory.Name.ToUpper().Contains(Model.FilterText.ToUpper());

            if (!isMatch)
            {
                var names = topicViewModel.ChildrenCollectionView.OfType<SubTopicViewModel>().Select(m => m.Entity.Name.ToUpper()).ToList();
                var foundNames = names.Where(n => n.ToUpper().Contains(Model.FilterText.ToUpper()));
                isMatch = foundNames.Any();


            }
            return isMatch;
        }
    }
}
