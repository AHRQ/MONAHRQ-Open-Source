using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Prism.Commands;
using PropertyChanged;
using System;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Prism.Events;
using Monahrq.Measures.Events;

namespace Monahrq.Measures.ViewModels
{
    /// <summary>
    /// Class for measures filter
    /// </summary>
    [ImplementPropertyChanged]
    public class MeasuresFilter
    {
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        private ManageMeasuresViewModel Model { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasuresFilter"/> class.
        /// </summary>
        /// <param name="vm">The vm.</param>
        public MeasuresFilter(ManageMeasuresViewModel vm)
        {
            Model = vm;
            vm.ApplyDataSetFilterCommand = new DelegateCommand(ApplyDataSetFilter);

            vm.PropertyChanged += (sender, args) =>
            {
                if (string.Equals(args.PropertyName, "SelectedDataSet", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(args.PropertyName, "SelectedProperty", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(args.PropertyName, "PropertyFilterText", StringComparison.OrdinalIgnoreCase))
                {
                    vm.ApplyDataSetFilterCommand.Execute();
                }
            };

        }

        // TODO: WEBSITE FILTER
        /// <summary>
        /// Models the property filter.
        /// </summary>
        /// <param name="measure">The measure.</param>
        /// <returns></returns>
        bool ModelPropertyFilter(MeasureModel measure)
        {
            if (measure == null) return false;
            if (Model.SelectedProperty == ModelPropertyFilterValues.NONE) return true;
            if (Model.SelectedProperty == ModelPropertyFilterValues.MEASURE_CODE)
            {
                return measure.Measure.MeasureCode.ToLower().Contains(Model.PropertyFilterText.ToLower());
            }

            if (Model.SelectedProperty == ModelPropertyFilterValues.MEASURE_NAME)
            {
                return measure.Measure.MeasureTitle.Clinical.ToLower().Contains(Model.PropertyFilterText.ToLower());
            }

            if (Model.SelectedProperty == ModelPropertyFilterValues.TOPIC_NAME)
            {
                return measure.Measure.Topics.Any(t => t.Owner.Name.ToLower().Contains(Model.PropertyFilterText.ToLower()));
            }
            if (Model.SelectedProperty == ModelPropertyFilterValues.SUBTOPIC_NAME)
            {
                return measure.Measure.Topics.Any(t => t.Name.ToLower().Contains(Model.PropertyFilterText.ToLower()));
            }
            if (Model.SelectedProperty == ModelPropertyFilterValues.WEBSITE_NAME)
            {
                return measure.Websites.Any(w=> w.ToLower().Contains(Model.PropertyFilterText.ToLower()));
            }
            return Model.SelectedProperty != ModelPropertyFilterValues.WEBSITE_NAME;
        }

        /// <summary>
        /// Models the dataset filter.
        /// </summary>
        /// <param name="measure">The measure.</param>
        /// <returns></returns>
        bool ModelDatasetFilter(MeasureModel measure)
        {
            return measure != null && measure.DataSetName == Model.SelectedDataSet;
        }

        /// <summary>
        /// Gets the data set filter.
        /// </summary>
        /// <value>
        /// The data set filter.
        /// </value>
        public System.Predicate<object> DataSetFilter
        {
            get
            {
                if (Model.MeasuresCollectionView == null
                    || Model.SelectedDataSet == BaseTabViewModel.ALL_DATASETS)
                {
                    return o => true;
                }
                return (o) => ModelDatasetFilter(o as MeasureModel);
            }
        }

        /// <summary>
        /// Gets the property filter.
        /// </summary>
        /// <value>
        /// The property filter.
        /// </value>
        public System.Predicate<object> PropertyFilter
        {
            get
            {
                if (Model.MeasuresCollectionView == null
                    || string.IsNullOrEmpty((Model.PropertyFilterText ?? string.Empty).Trim()))
                {
                    return o => true;
                }
                return o => ModelPropertyFilter(o as MeasureModel);
            }
        }

        /// <summary>
        /// Applies the data set filter.
        /// </summary>
        void ApplyDataSetFilter()
        {
            try
            {
                if (Model.MeasuresCollectionView.IsEditingItem) return;
                Model.MeasuresCollectionView.Filter = (o) => PropertyFilter(o) && DataSetFilter(o);
                Model.MeasuresCollectionView.MoveCurrentToFirst();
            }
            finally
            {
                ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<MeasureFilterApplied>().Publish(EventArgs.Empty);
            }
        }
    }
}