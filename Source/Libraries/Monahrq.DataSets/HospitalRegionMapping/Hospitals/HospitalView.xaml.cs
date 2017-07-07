using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Sdk.Attributes;
using NHibernate.Mapping;
using System.Windows.Media;

namespace Monahrq.DataSets.HospitalRegionMapping.Hospitals
{
    [ViewExport(typeof(HospitalsView), RegionName = Mapping.RegionNames.Hospitals)]
    public partial class HospitalsView
    {
        public HospitalsView()
        {
            InitializeComponent();
            // this.HospitalGrid.SelectedCellsChanged +=HospitalGrid_SelectedCellsChanged;
        }

        [Import]
        public HospitalsViewModel Model
        {
            get
            {
                return DataContext as HospitalsViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        private int _lastCheckedItemIndex { get; set; }
        private void cbSelectionCheckBox_KeyUp(object sender, KeyEventArgs e)
        {
            CheckBox cbSelectionCheckBox = sender as CheckBox;
            Hospital newCheckedItem = cbSelectionCheckBox.DataContext as Hospital;

            int newCheckedItemIndex = Model.TestCollectionItems.IndexOf(newCheckedItem);

            if ((System.Windows.Input.Keyboard.IsKeyUp(System.Windows.Input.Key.LeftShift) || System.Windows.Input.Keyboard.IsKeyUp(System.Windows.Input.Key.RightShift)) && newCheckedItemIndex != _lastCheckedItemIndex)
            {
                int start = Math.Min(_lastCheckedItemIndex, newCheckedItemIndex);
                int end = Math.Max(_lastCheckedItemIndex, newCheckedItemIndex);

                int countToTake = end - start;
                
                // var itemsToCheck = Model.TestCollectionItems.SelectedItems.Skip(start).Take(countToTake).ToList();

                var itemsToCheck = new List<Hospital>();
                foreach (var o in Model.TestCollectionItems.OfType<Hospital>().Select((x, i) => new { x, i }))
                {
                   if(o.i > start && o.i <end)
                        itemsToCheck.Add(o.x);
                }

                foreach (Hospital itemToCheck in itemsToCheck)
                {
                    itemToCheck.IsSelected = true;
                }
                _lastCheckedItemIndex = -1;
            }
            else
            {
                _lastCheckedItemIndex = newCheckedItemIndex;
            }
        }
    }

    public class ControlTemplateVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var isSourceFromBaseData = (bool)value;
            if (parameter == null)
                return isSourceFromBaseData ? Visibility.Visible : Visibility.Collapsed;

            return isSourceFromBaseData ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class CmsProviderConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var hospital = value as Hospital;
            if (hospital == null) return null;

            return hospital.CmsProviderID ?? hospital.CmsCollection.FirstOrDefault();
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class MultiCmdParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values != null)
            {
                var arrayValues = new List<object>(values);
                return arrayValues.ToArray();
            }
            else return new object();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
