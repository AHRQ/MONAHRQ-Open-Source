using Microsoft.Practices.Prism.Commands;
using PropertyChanged;
using System;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Measures.Events;

namespace Monahrq.Measures.ViewModels
{

    /// <summary>
    /// Topics filter class
    /// </summary>
    [ImplementPropertyChanged]
    public class TopicsFilter
    {
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        private ManageTopicsViewModel Model { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicsFilter"/> class.
        /// </summary>
        /// <param name="vm">The vm.</param>
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

        /// <summary>
        /// Applies the topics filter.
        /// </summary>
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


        /// <summary>
        /// Gets the topic filter.
        /// </summary>
        /// <value>
        /// The topic filter.
        /// </value>
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

        /// <summary>
        /// Models the topic filter.
        /// </summary>
        /// <param name="subTopicViewModel">The sub topic view model.</param>
        /// <returns></returns>
        private bool ModelTopicFilter(SubTopicViewModel subTopicViewModel)
        {
            var parent = subTopicViewModel.Parent as TopicViewModel;
            return parent != null && parent.TopicCategory.Name.ToUpper().Contains(Model.FilterText.ToUpper()) ||
                   subTopicViewModel.Entity.Name.ToUpper().Contains(Model.FilterText.ToUpper());
        }


        /// <summary>
        /// Gets the topic category filter.
        /// </summary>
        /// <value>
        /// The topic category filter.
        /// </value>
        public Predicate<object> TopicCategoryFilter
        {
            get
            {
                return Model.TopicsCollectionView == null || string.IsNullOrEmpty((Model.FilterText ?? string.Empty).Trim())
                           ? (Predicate<object>) (o => true)
                           : (o => ModelCategoryFilter(o as TopicViewModel));
            }
        }

        /// <summary>
        /// Models the category filter.
        /// </summary>
        /// <param name="topicViewModel">The topic view model.</param>
        /// <returns></returns>
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
