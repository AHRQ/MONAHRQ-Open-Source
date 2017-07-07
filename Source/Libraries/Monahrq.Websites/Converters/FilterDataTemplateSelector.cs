using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Monahrq.Websites.ViewModels;

namespace Monahrq.Websites.Converters
{
    public class FilterDataTemplateSelector : DataTemplateSelector
    {
         public override DataTemplate SelectTemplate(object item, DependencyObject container)
         {
             var viewModel = item as AttributeViewModel;
             if (viewModel != null)
                 switch (viewModel.AttributeType)
                 {
                     case AttributeType.Filters:
                         return FiltersViewModelDataTemplate;
                     case AttributeType.Columns:
                         return ReportColumnsViewModelDataTemplate;
                     case AttributeType.ComparisonKeys:
                         return RatingsFiltersViewModelDataTemplate;
                     case AttributeType.Display:
                         return ProfileDisplayViewModelDataTemplate;
                     default:
                         return null;
                 }

             return null;
         }

         public DataTemplate FiltersViewModelDataTemplate { get; set; }
         public DataTemplate RatingsFiltersViewModelDataTemplate { get; set; }
         public DataTemplate ReportColumnsViewModelDataTemplate { get; set; }
         public DataTemplate ProfileDisplayViewModelDataTemplate { get; set; }
    }
}
