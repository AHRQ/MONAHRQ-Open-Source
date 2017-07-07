using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Prism.Commands;
using PropertyChanged;
using System;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Websites.Services;

namespace Monahrq.Websites.ViewModels
{
    [ImplementPropertyChanged]
    public class MeasuresFilter
    {
        private WebsiteMeasuresViewModel Model { get; set; }

        public MeasuresFilter(WebsiteMeasuresViewModel vm)
        {
            Model = vm;


            vm.ApplyDataSetFilterCommand = new DelegateCommand(ApplyDataSetFilter);
			vm.ApplyBatchDataSetFilterCommand = new DelegateCommand(ApplyBatchDataSetFilter);
            vm.PropertyChanged += (sender, args) =>
            {
                if (string.Equals(args.PropertyName, "SelectedDataSet", StringComparison.OrdinalIgnoreCase)		|| 
                    string.Equals(args.PropertyName, "SelectedProperty", StringComparison.OrdinalIgnoreCase)	||
                    string.Equals(args.PropertyName, "PropertyFilterText", StringComparison.OrdinalIgnoreCase))
                {
                    vm.ApplyDataSetFilterCommand.Execute();
				}
				if (string.Equals(args.PropertyName, "BatchSelectedDataSet", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(args.PropertyName, "BatchSelectedProperty", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(args.PropertyName, "BatchPropertyFilterText", StringComparison.OrdinalIgnoreCase))
				{
					vm.ApplyBatchDataSetFilterCommand.Execute();
				}
            };


        }

		#region WebsiteMeasureView Main Filter Methods.
		// TODO: WEBSITE FILTER
        bool ModelPropertyFilter(WebsiteMeasure measure)
        {
            if (measure == null) return false;
            if (Model.SelectedProperty == ModelPropertyFilterValues.NONE) return true;
            if (Model.SelectedProperty == ModelPropertyFilterValues.MEASURE_CODE)
            {
                return measure.ReportMeasure.MeasureCode.ToLower().Contains(Model.PropertyFilterText.ToLower());
            }

            if (Model.SelectedProperty == ModelPropertyFilterValues.MEASURE_NAME)
            {
                return measure.ReportMeasure.MeasureTitle.Clinical.ToLower().Contains(Model.PropertyFilterText.ToLower());
            }

            if (Model.SelectedProperty == ModelPropertyFilterValues.TOPIC_NAME)
            {
                return measure.ReportMeasure.Topics.Any(t => t.Name.ToLower().Contains(Model.PropertyFilterText.ToLower()) || t.Owner.ToString().ToLower().Contains(Model.PropertyFilterText.ToLower()));
            }
            if (Model.SelectedProperty == ModelPropertyFilterValues.WEBSITE_NAME)
            {
                var WebsiteDataService = ServiceLocator.Current.GetInstance<IWebsiteDataService>();
                var websiteNames=WebsiteDataService.GetWebsiteNamesForMeasure(measure.OriginalMeasure.Id);
                return string.Join(", ", websiteNames).ToLower().Contains(Model.PropertyFilterText.ToLower());
                 
            }
            if (Model.SelectedProperty == ModelPropertyFilterValues.SUBTOPIC_NAME)
            {
                return measure.ReportMeasure.Topics.Any(t => t.Name.ToLower().Contains(Model.PropertyFilterText.ToLower()) || t.Owner.ToString().ToLower().Contains(Model.PropertyFilterText.ToLower()));
            }
            return true;
        }

        bool ModelDatasetFilter(WebsiteMeasure measure)
        {
            return measure != null && measure.ReportMeasure.Owner.Name == Model.SelectedDataSet;
        }

        public System.Predicate<object> DataSetFilter
        {
            get
            {
                if (Model.AvailableMeasuresView == null
                    || Model.SelectedDataSet == WebsiteMeasuresViewModel.ALL_DATASETS)
                {
                    return o => true;
                }
                return (o) => ModelDatasetFilter(o as WebsiteMeasure);
            }
        }

        public System.Predicate<object> PropertyFilter
        {
            get
            {
                if (Model.AvailableMeasuresView == null
                    || string.IsNullOrEmpty((Model.PropertyFilterText ?? string.Empty).Trim()))
                {
                    return o => true;
                }
                return o => ModelPropertyFilter(o as WebsiteMeasure);
            }
        }

        void ApplyDataSetFilter()
        {
            try
            {
                if (Model != null && Model.AvailableMeasuresView != null)
                    Model.AvailableMeasuresView.Filter = (o) => PropertyFilter(o) && DataSetFilter(o);
            }
            finally
            {
                ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<MeasureFilterApplied>().Publish(EventArgs.Empty);
            }
        }
		#endregion

		#region WebsiteMeasureView Batch Filter Methods.
		// TODO: WEBSITE FILTER
		bool ModelBatchPropertyFilter(BatchMeasureModel batchMeasure)
		{
			if (batchMeasure == null) return false;
			if (Model.BatchSelectedProperty == ModelPropertyFilterValues.NONE) return true;
			if (Model.BatchSelectedProperty == ModelPropertyFilterValues.MEASURE_CODE)
			{
				return batchMeasure.ProxyWebsiteMeasure.ReportMeasure.MeasureCode.ToLower().Contains(Model.BatchPropertyFilterText.ToLower());
				//return batchMeasure.ProxyMeasureModel.Measure.MeasureCode.ToLower().Contains(Model.BatchPropertyFilterText.ToLower());
			}
			if (Model.BatchSelectedProperty == ModelPropertyFilterValues.MEASURE_NAME)
			{
				return batchMeasure.ProxyWebsiteMeasure.ReportMeasure.MeasureTitle.Clinical.ToLower().Contains(Model.BatchPropertyFilterText.ToLower());
				//return batchMeasure.ProxyMeasureModel.Measure.MeasureTitle.Clinical.ToLower().Contains(Model.BatchPropertyFilterText.ToLower());
			}

		//	if (Model.SelectedProperty == ModelPropertyFilterValues.TOPIC_NAME)
		//	{
			//		return batchMeasure.ProxyWebsiteMeasure.ReportMeasure.Topics.Any(t => t.Name.ToLower().Contains(Model.BatchPropertyFilterText.ToLower()) || t.Owner.ToString().ToLower().Contains(Model.BatchPropertyFilterText.ToLower()));
			//		return batchMeasure.ProxyMeasureModel.Measure.Topics.Any(t => t.Name.ToLower().Contains(Model.BatchPropertyFilterText.ToLower()) || t.Owner.ToString().ToLower().Contains(Model.BatchPropertyFilterText.ToLower()));
		//	}
		//	if (Model.SelectedProperty == ModelPropertyFilterValues.WEBSITE_NAME)
		//	{
		//		var WebsiteDataService = ServiceLocator.Current.GetInstance<IWebsiteDataService>();
		//		var websiteNames = WebsiteDataService.GetWebsiteNamesForMeasure(batchMeasure.ProxyWebsiteMeasure.OriginalMeasure.Id);
		//		var websiteNames = WebsiteDataService.GetWebsiteNamesForMeasure(batchMeasure.ProxyMeasureModel.OriginalMeasure.Id);
		//		return string.Join(", ", websiteNames).ToLower().Contains(Model.BatchPropertyFilterText.ToLower());
		//
		//	}
			return true;
		}

		bool ModelBatchDatasetFilter(BatchMeasureModel batchMeasure)
		{
			return batchMeasure != null && batchMeasure.ProxyWebsiteMeasure.ReportMeasure.Owner.Name == Model.BatchSelectedDataSet;
			//return batchMeasure != null && batchMeasure.ProxyMeasureModel.Measure.Owner.Name == Model.BatchSelectedDataSet;
		}

		public System.Predicate<object> BatchDataSetFilter
		{
			get
			{
				if (Model.BatchAvailableMeasuresView == null
					|| Model.BatchSelectedDataSet == WebsiteMeasuresViewModel.ALL_DATASETS)
				{
					return o => true;
				}
				return (o) => ModelBatchDatasetFilter(o as BatchMeasureModel);
			}
		}

		public System.Predicate<object> BatchPropertyFilter
		{
			get
			{
				if (Model.BatchAvailableMeasuresView == null
					|| string.IsNullOrEmpty((Model.BatchPropertyFilterText ?? string.Empty).Trim()))
				{
					return o => true;
				}
				return o => ModelBatchPropertyFilter(o as BatchMeasureModel);
			}
		}
		void ApplyBatchDataSetFilter()
		{
			try
			{
				if (Model != null && Model.BatchAvailableMeasuresView != null)
					Model.BatchAvailableMeasuresView.Filter = (o) => BatchPropertyFilter(o) && BatchDataSetFilter(o);
			}
			finally
			{
				ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<MeasureFilterApplied>().Publish(EventArgs.Empty);
			}
		}
		#endregion

    }

    public class MeasureFilterApplied : CompositePresentationEvent<EventArgs> { }
}