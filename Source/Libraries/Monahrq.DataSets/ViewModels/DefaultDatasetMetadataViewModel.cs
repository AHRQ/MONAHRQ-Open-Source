using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Monahrq.DataSets.Annotations;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Services.BaseData;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Types;
using PropertyChanged;

namespace Monahrq.DataSets.ViewModels
{
    /// <summary>
    /// The dataset list tab view model partial class. This partial view model handles the edits/updates to the dataset metadata.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.ComponentModel.Composition.IPartImportsSatisfiedNotification" />
    /// <seealso cref="Microsoft.Practices.Prism.Regions.INavigationAware" />
    /// <seealso cref="Monahrq.Sdk.ViewModels.ITabItem" />
    public partial class DataSetListViewModel
    {
        /// <summary>
        /// The show dataset metadata pop up
        /// </summary>
        private bool _showDatasetMetadataPopUp;

        /// <summary>
        /// Gets or sets a value indicating whether [show dataset metadata pop up].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show dataset metadata pop up]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowDatasetMetadataPopUp
        {
            get { return _showDatasetMetadataPopUp; }
            set
            {
                _showDatasetMetadataPopUp = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the selected data set.
        /// </summary>
        /// <value>
        /// The selected data set.
        /// </value>
        public DatasetMetadataViewModel SelectedDataSet { get; set; }

        /// <summary>
        /// Gets or sets the edit dataset metadata command.
        /// </summary>
        /// <value>
        /// The edit dataset metadata command.
        /// </value>
        public ICommand EditDatasetMetadataCommand { get; set; }
        /// <summary>
        /// Gets or sets the hide dataset metadata pop up command.
        /// </summary>
        /// <value>
        /// The hide dataset metadata pop up command.
        /// </value>
        public ICommand HideDatasetMetadataPopUpCommand { get; set; }
        /// <summary>
        /// Gets or sets the update dataset metadata command.
        /// </summary>
        /// <value>
        /// The update dataset metadata command.
        /// </value>
        public ICommand UpdateDatasetMetadataCommand { get; set; }

        /// <summary>
        /// Gets the base data service.
        /// </summary>
        /// <value>
        /// The base data service.
        /// </value>
        [Import]
        public IBaseDataService BaseDataService { get; private set; }

        /// <summary>
        /// Called when [edit dataset metadata].
        /// </summary>
        /// <param name="obj">The object.</param>
        private void OnEditDatasetMetadata(object obj)
        {
            var datasetTypeDetail = obj as DataTypeDetailsViewModel;
            if (datasetTypeDetail == null || datasetTypeDetail.Entry == null) return;

            ShowDatasetMetadataPopUp = true;

            datasetTypeDetail.Entry = Service.Refresh(datasetTypeDetail.Entry);

            SelectedDataSet = new DatasetMetadataViewModel
            {
                Dataset = datasetTypeDetail.Entry,
                DatasetTypeName = datasetTypeDetail.Entry.ContentType.Name,
                DatasetTitle = datasetTypeDetail.Entry.Name,
                SelectedYear = datasetTypeDetail.Entry.ReportingYear,
                SelectedQuarter = datasetTypeDetail.Entry.ReportingQuarter,
                YearItems = BaseDataService.ReportingYears,
                StatesItems =  BaseDataService.States(null).Select(s => new SelectListItem { Text = s.Text, Value = s.Data != null ? s.Data.Abbreviation : null}).ToObservableCollection(),
                QuartersItems = BaseDataService.ReportingQuarters.Select(q => new SelectListItem { Text = q.Text, Value = q.Data != null ? q.Data.Name : null}).ToObservableCollection()
            };

            if (SelectedDataSet.Dataset.ContentType.Name.EqualsIgnoreCase("PHYSICIAN DATA"))
            {
                SelectedDataSet.StatesItems.RemoveAt(0);
                SelectedDataSet.StatesItems.Where(s => s.Value != null).ToList().ForEach(s => 
                {
                   s.IsSelected = SelectedDataSet.Dataset.ProviderStates.ContainsIgnoreCase(s.Value != null ? s.Value.ToString() : null);
                });

                SelectedDataSet.SelectedManagement = SelectedDataSet.Dataset.UseRealtimeData
                                                        ? DatasetMetadataViewModel.REAL_TIME_MANAGEMENT
                                                        : DatasetMetadataViewModel.MANAGE_IN_MONAHRQ;
            }
        }

        /// <summary>
        /// Determines whether this instance [can edit dataset metadata] the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can edit dataset metadata] the specified object; otherwise, <c>false</c>.
        /// </returns>
        private bool CanEditDatasetMetadata(object obj)
        {
            //var datasetTypeDetail = obj as DataTypeDetailsViewModel;
            //return (datasetTypeDetail != null && datasetTypeDetail.Entry != null);
            return true;
        }

        /// <summary>
        /// Called when [close edit dataset metadata].
        /// </summary>
        private void OnCloseEditDatasetMetadata()
        {
            if (MessageBox.Show("Would you like to save your changes?", "Dataset Edits", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                OnUpdateDatasetMetadata();
                ShowDatasetMetadataPopUp = false;
                RaisePropertyChanged(() => ShowDatasetMetadataPopUp);
                SelectedDataSet = null;
                RaisePropertyChanged(() => SelectedDataSet);
                return;
            }
            ShowDatasetMetadataPopUp = false;
            RaisePropertyChanged(() => ShowDatasetMetadataPopUp);
            SelectedDataSet = null;
            RaisePropertyChanged(() => SelectedDataSet);
        }

        /// <summary>
        /// Called when [update dataset metadata].
        /// </summary>
        private void OnUpdateDatasetMetadata()
        {
            if (SelectedDataSet == null || SelectedDataSet.Dataset == null) return;

            SelectedDataSet.Dataset.File = SelectedDataSet.DatasetTitle;
            SelectedDataSet.Dataset.ReportingYear = SelectedDataSet.SelectedYear;
            SelectedDataSet.Dataset.ReportingQuarter = SelectedDataSet.SelectedQuarter;

            if (SelectedDataSet.Dataset.ContentType.Name.EqualsIgnoreCase("PHYSICIAN DATA"))
            {
                SelectedDataSet.Dataset.ProviderStates = string.Join(",", SelectedDataSet.StatesItems.Where(s => s.IsSelected).Select(s => s.Value).ToList());
                SelectedDataSet.Dataset.UseRealtimeData = !string.IsNullOrEmpty(SelectedDataSet.SelectedManagement) &&
                                                          SelectedDataSet.SelectedManagement.EqualsIgnoreCase(DatasetMetadataViewModel.REAL_TIME_MANAGEMENT);
            }

            SelectedDataSet.Dataset.Validate();
            if (SelectedDataSet.Dataset.HasErrors) return;

            Service.Save(SelectedDataSet.Dataset, (o, exception) =>
            {
                if (exception == null)
                {
                    Refresh();
                    ShowDatasetMetadataPopUp = false;
                    Events.GetEvent<GenericNotificationEvent>().Publish(string.Format("Metadata for dataset \"{0}\" was successfully updated.", SelectedDataSet.Dataset.File));
                    SelectedDataSet = null;
                }
                else
                {
                    Logger.Write(exception);
                    Events.GetEvent<ErrorNotificationEvent>().Publish(exception);
                }
            });
        }
    }

    /// <summary>
    /// The dataset metadata view model.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    [ImplementPropertyChanged]
    public class DatasetMetadataViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The manage in monahrq
        /// </summary>
        public const string MANAGE_IN_MONAHRQ = "I want to include descriptive data about physicians in MONAHRQ";
        /// <summary>
        /// The real time management
        /// </summary>
        public const string REAL_TIME_MANAGEMENT = "I want my generated website to get the most current version of Physician data.";

        /// <summary>
        /// The dataset type name
        /// </summary>
        private string _datasetTypeName;
        /// <summary>
        /// The dataset
        /// </summary>
        private Dataset _dataset;

        /// <summary>
        /// Gets or sets the name of the dataset type.
        /// </summary>
        /// <value>
        /// The name of the dataset type.
        /// </value>
        public string DatasetTypeName
        {
            get { return _datasetTypeName; }
            set
            {
                if (_datasetTypeName != value)
                {
                    _datasetTypeName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the dataset.
        /// </summary>
        /// <value>
        /// The dataset.
        /// </value>
        public Dataset Dataset
        {
            get { return _dataset; }
            set
            {
                if (_dataset != value)
                {
                    _dataset = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the dataset title.
        /// </summary>
        /// <value>
        /// The dataset title.
        /// </value>
        public string DatasetTitle { get; set; }
        /// <summary>
        /// Gets or sets the selected year.
        /// </summary>
        /// <value>
        /// The selected year.
        /// </value>
        public string SelectedYear { get; set; }
        /// <summary>
        /// Gets or sets the selected quarter.
        /// </summary>
        /// <value>
        /// The selected quarter.
        /// </value>
        public string SelectedQuarter { get; set; }
        /// <summary>
        /// Gets or sets the selected management.
        /// </summary>
        /// <value>
        /// The selected management.
        /// </value>
        public string SelectedManagement { get; set; }

        /// <summary>
        /// Gets or sets the year items.
        /// </summary>
        /// <value>
        /// The year items.
        /// </value>
        public ObservableCollection<string> YearItems { get; set; }
        /// <summary>
        /// Gets or sets the quarters items.
        /// </summary>
        /// <value>
        /// The quarters items.
        /// </value>
        public ObservableCollection<SelectListItem> QuartersItems { get; set; }
        /// <summary>
        /// Gets or sets the states items.
        /// </summary>
        /// <value>
        /// The states items.
        /// </value>
        public ObservableCollection<SelectListItem> StatesItems { get; set; }
        /// <summary>
        /// Gets the physician management option.
        /// </summary>
        /// <value>
        /// The physician management option.
        /// </value>
        public ObservableCollection<string> PhysicianManagementOption
        {
            get
            {
                return new ObservableCollection<string> { MANAGE_IN_MONAHRQ, REAL_TIME_MANAGEMENT };
            }
        }

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

        }
        #endregion
    }

    /// <summary>
    /// The dataset template slector.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.DataTemplateSelector" />
    public class DatasetTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the default template.
        /// </summary>
        /// <value>
        /// The default template.
        /// </value>
        public DataTemplate DefaultTemplate { get; set; }

        //You override this function to select your data template based in the given item
        /// <summary>
        /// When overridden in a derived class, returns a <see cref="T:System.Windows.DataTemplate" /> based on custom logic.
        /// </summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        /// <returns>
        /// Returns a <see cref="T:System.Windows.DataTemplate" /> or null. The default value is null.
        /// </returns>
        public override DataTemplate SelectTemplate(object item,
                        DependencyObject container) 
        {

            //Window win = Application.Current.MainWindow;
            var selectedDataset = item as DatasetMetadataViewModel;

            if (selectedDataset == null || string.IsNullOrEmpty(selectedDataset.DatasetTypeName)) return DefaultTemplate;

            if (!selectedDataset.Dataset.ContentType.IsCustom)
            {
                switch (selectedDataset.DatasetTypeName.ToUpper())
                {
                    case "NURSING HOME COMPARE DATA":
                    case "HOSPITAL COMPARE DATA":
                        return ((FrameworkElement)container).FindResource("HospitalNursingHomeCompareTemplate") as DataTemplate;
                    //    return win.FindResource("HospitalNursingHomeCompareTemplate") as DataTemplate;
                    //case "AHRQ-QI AREA DATA":
                    //case "AHRQ-QI COMPOSITE DATA":
                    //case "AHRQ-QI PROVIDER DATA":
                    //    return win.FindResource("AHRQQITemplate") as DataTemplate;
                    case "PHYSICIAN DATA":
                        return ((FrameworkElement)container).FindResource("PhysicianDataTemplate") as DataTemplate;
                    //return win.FindResource("PhysicianTemplate") as DataTemplate;
                    case "MEDICARE PROVIDER CHARGE DATA":
                    default:
                        return DefaultTemplate;
                }
            }

            return selectedDataset.Dataset.ContentType.ImportType == DynamicStepTypeEnum.Simple
                                                                    ? DefaultTemplate
                                                                    : ((FrameworkElement)container).FindResource("DynamicSimpleTemplate") as DataTemplate;
        }
    }
}
