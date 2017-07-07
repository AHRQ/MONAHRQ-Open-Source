using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Reports.ViewModels;

namespace Monahrq.Reports.Converters
{
    /// <summary>
    /// Data Template selector class.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.DataTemplateSelector" />
    public class FilterDataTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Overrride the SelectTemplate method, this method returns the template based upon the view model type
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var viewModel = item as Filter;
            if (viewModel != null)
                switch (viewModel.Type)
                {
                    case ReportFilterTypeEnum.Hospital:
                    case ReportFilterTypeEnum.DRGsDischarges:
                    case ReportFilterTypeEnum.ConditionsAndDiagnosis:
                        return FiltersViewModelDataTemplate;
                    //case ReportFilterTypeEnum.ReportColumns:
                    //    return ReportColumnsViewModelDataTemplate;
                    //case ReportFilterTypeEnum.KesysForRatings:
                    //    return RatingsFiltersViewModelDataTemplate;
                    case ReportFilterTypeEnum.Display:
                        return ProfileDisplayViewModelDataTemplate;
                    default:
                        return null;

                    //case  AttributeType.Filters:
                    //    return FiltersViewModelDataTemplate;
                    //case AttributeType.Columns:
                    //    return ReportColumnsViewModelDataTemplate;
                    //case AttributeType.ComparisonKeys:
                    //    return RatingsFiltersViewModelDataTemplate;
                    //case AttributeType.Display:
                    //    return ProfileDisplayViewModelDataTemplate;
                    //default:
                    //    return null;
                }

            return null;
        }

        /// <summary>
        /// Gets or sets the filters view model data template.
        /// </summary>
        /// <value>
        /// The filters view model data template.
        /// </value>
        public DataTemplate FiltersViewModelDataTemplate { get; set; }
        /// <summary>
        /// Gets or sets the ratings filters view model data template.
        /// </summary>
        /// <value>
        /// The ratings filters view model data template.
        /// </value>
        public DataTemplate RatingsFiltersViewModelDataTemplate { get; set; }
        /// <summary>
        /// Gets or sets the report columns view model data template.
        /// </summary>
        /// <value>
        /// The report columns view model data template.
        /// </value>
        public DataTemplate ReportColumnsViewModelDataTemplate { get; set; }
        /// <summary>
        /// Gets or sets the profile display view model data template.
        /// </summary>
        /// <value>
        /// The profile display view model data template.
        /// </value>
        public DataTemplate ProfileDisplayViewModelDataTemplate { get; set; }
    }
}
