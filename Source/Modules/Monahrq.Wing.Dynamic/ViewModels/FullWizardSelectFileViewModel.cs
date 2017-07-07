using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Model;
using Monahrq.Default.DataProvider.Administration.File;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.BaseData.ViewModel;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services.BaseData;
using Monahrq.Sdk.Types;
using Monahrq.Theme.Controls.Wizard.Helpers;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Dynamic.Models;
using NHibernate.Linq;
using PropertyChanged;

namespace Monahrq.Wing.Dynamic.ViewModels
{
    /// <summary>
    /// View model class for full wizard for select file
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{Monahrq.Wing.Dynamic.Models.WizardContext}" />
    [Export]
    [ImplementPropertyChanged]
    public class FullWizardSelectFileViewModel : WizardStepViewModelBase<WizardContext>
    {
        /// <summary>
        /// Gets the manager.
        /// </summary>
        /// <value>
        /// The manager.
        /// </value>
        public IRegionManager Manager
        {
            get { return ServiceLocator.Current.GetInstance<IRegionManager>(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullWizardSelectFileViewModel"/> class.
        /// </summary>
        /// <param name="c">The c.</param>
        public FullWizardSelectFileViewModel(WizardContext c)
            : base(c)
        {
            DatasourceModel = ServiceLocator.Current.GetInstance<IFileDatasourceViewModel>();
            BaseDataService = ServiceLocator.Current.GetInstance<IBaseDataService>();

            if (!DataContextObject.ExistingDatasetId.HasValue || DataContextObject.ExistingDatasetId.Value <= 0)
                DataContextObject.DatasetItem = new Dataset();
            else
            {
                using (var session = DataContextObject.Provider.SessionFactory.OpenSession())
                {
                    DataContextObject.DatasetItem = session.Query<Dataset>()
                                  .FirstOrDefault(ds => ds.Id == DataContextObject.ExistingDatasetId) ?? new Dataset();
                }
                //DataContextObject.DatasetItem.IsReImport = DataContextObject.DatasetItem.IsPersisted;
            }
            
            OnInitializeCommands();
            GenerateData();
            c.Histogram = null;
        }

        /// <summary>
        /// Gets the datasource model.
        /// </summary>
        /// <value>
        /// The datasource model.
        /// </value>
        [Import]
        public IFileDatasourceViewModel DatasourceModel { get; private set; }

        /// <summary>
        /// Gets the base data service.
        /// </summary>
        /// <value>
        /// The base data service.
        /// </value>
        [Import]
        public IBaseDataService BaseDataService { get; private set; }

        /// <summary>
        /// Gets or sets the execute mapping file upload.
        /// </summary>
        /// <value>
        /// The execute mapping file upload.
        /// </value>
        public ICommand ExecuteMappingFileUpload { get; set; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public override string DisplayName
        {
            get { return "Select data file"; }
        }

        const string DEFAULT_YEAR_SELECT_TEXT = "Please Select Reporting Year";
        //const string DEFAULT_QUARTER_SELECT_TEXT = "Please Select Reporting Quarter";

        /// <summary>
        /// Generates the data.
        /// </summary>
        private void GenerateData()
        {
            Years = BaseDataService.ReportingYears;
            Years.Insert(0, DEFAULT_YEAR_SELECT_TEXT);

            Quarters = BaseDataService.ReportingQuarters;

            ReportingQuarter = 0;
            SelectedYear = Years[0];

            var datasets = new List<SelectListItem> { new SelectListItem { Text = "System Default Mapping", Value = -1 } };
            using (var session = DataContextObject.Provider.SessionFactory.OpenSession())
            {
                foreach (var datasetEntry in DataContextObject.SelectedDataType.RecordsList.Select(r => r.Entry).OrderBy(r => r.Id).ToList())
                {
                    session.Refresh(datasetEntry);

                    if (datasetEntry.Infoset.Element.HasElements)
                    {
                        datasets.Add(new SelectListItem { Text = datasetEntry.File, Value = datasetEntry.Id });
                    }
                }
            }

            ExistingsDatasets = new ObservableCollection<SelectListItem>(datasets);
            OnPropertyChanged(@"ExistingsDatasets");

            SelectedMappingDatasetId = -1;
            OnPropertyChanged(@"SelectedMappingDatasetId");

            if (DataContextObject.DatasetItem.Id > 0)
            {
                SelectedYear = DataContextObject.DatasetItem.ReportingYear;

                var selectedQuarter = 0;
                if (!string.IsNullOrEmpty(DataContextObject.DatasetItem.ReportingQuarter) && int.TryParse(DataContextObject.DatasetItem.ReportingQuarter, out selectedQuarter))
                    ReportingQuarter = selectedQuarter;

                Title = DataContextObject.DatasetItem.File;
                DataContextObject.DatasetItem.IsFinished = true;

            }
        }

        #region Properties

        /// <summary>
        /// Gets the maximum length of the title.
        /// </summary>
        /// <value>
        /// The maximum length of the title.
        /// </value>
        public int TitleMaxLength
        {
            get { return 200; }            // max length for the user to type in the Title textbox
        }

        /// <summary>
        /// The title
        /// </summary>
        string _title;
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        /// <exception cref="ApplicationException">Textbox MaxLength is not set</exception>
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (value.Length > TitleMaxLength) throw(new ApplicationException("Textbox MaxLength is not set"));

                _title = value;
            }
        }

        /// <summary>
        /// Gets or sets the reporting quarter.
        /// </summary>
        /// <value>
        /// The reporting quarter.
        /// </value>
        public int? ReportingQuarter { get; set; }
        /// <summary>
        /// Gets or sets the quarters.
        /// </summary>
        /// <value>
        /// The quarters.
        /// </value>
        public ObservableCollection<EntityViewModel<ReportingQuarter, int>> Quarters { get; set; }
        /// <summary>
        /// Gets or sets the years.
        /// </summary>
        /// <value>
        /// The years.
        /// </value>
        public ObservableCollection<string> Years { get; set; }
        /// <summary>
        /// Gets or sets the selected year.
        /// </summary>
        /// <value>
        /// The selected year.
        /// </value>
        public string SelectedYear { get; set; }

        /// <summary>
        /// Gets or sets the type of the selected mapping.
        /// </summary>
        /// <value>
        /// The type of the selected mapping.
        /// </value>
        public MappingType SelectedMappingType { get; set; }
        /// <summary>
        /// Gets or sets the selected mapping dataset identifier.
        /// </summary>
        /// <value>
        /// The selected mapping dataset identifier.
        /// </value>
        public int SelectedMappingDatasetId { get; set; }
        /// <summary>
        /// Gets or sets the select data mapping file.
        /// </summary>
        /// <value>
        /// The select data mapping file.
        /// </value>
        public string SelectDataMappingFile { get; set; }
        /// <summary>
        /// Gets or sets the existings datasets.
        /// </summary>
        /// <value>
        /// The existings datasets.
        /// </value>
        public ObservableCollection<SelectListItem> ExistingsDatasets { get; set; }

        #endregion

        /// <summary>
        /// Returns true if this instance is valid.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsValid()
        {
            var result = string.Compare(SelectedYear, DEFAULT_YEAR_SELECT_TEXT, StringComparison.Ordinal) != 0
                && !string.IsNullOrEmpty(Title)
                && DatasourceModel != null 
                && DatasourceModel.ConnectionElement != null
                && !string.IsNullOrEmpty(DatasourceModel.ConnectionElement.SelectFrom);

            if (SelectedMappingType == MappingType.ExistingFile && string.IsNullOrEmpty(SelectDataMappingFile))
                result = false;

            return result;
        }

        /// <summary>
        /// For when you need to save some values that can't be directly bound to UI elements.
        /// Not called when moving previous (see WizardViewModel.MoveToNextStep).
        /// </summary>
        /// <returns>
        /// An object that may modify the route
        /// </returns>
        public override RouteModifier OnNext()
        {
            DataContextObject.Reset();
            DataContextObject.Title = Title;
            DataContextObject.Year = SelectedYear;
            DataContextObject.Quarter = ReportingQuarter;
            DataContextObject.MappingType = SelectedMappingType;
            DataContextObject.SelectDataMappingFile = SelectDataMappingFile;
            DataContextObject.SelectedMappingDatasetId = SelectedMappingDatasetId;

            DataContextObject.DatasourceDefinition = DatasourceModel;

            //var item = DataContextObject.DatasetItem ?? new Dataset();

            if (!DataContextObject.Title.EqualsIgnoreCase(DataContextObject.DatasetItem.File))
                DataContextObject.DatasetItem.File = DataContextObject.Title;


            // BUG #246 AND 247
            // Year is required, but Quarter isn't. So if SelectedQuarter is the default choice, don't use it.

            DataContextObject.DatasetItem.Description = null;
            DataContextObject.DatasetItem.Description = DataContextObject.DatasetItem.File + " - " + (ReportingQuarter == null || ReportingQuarter.Value == 0
                ? SelectedYear
                : string.Format("{0} {1}", Quarters[ReportingQuarter.Value], SelectedYear));

            DataContextObject.DatasetItem.ReportingQuarter = ReportingQuarter != null && ReportingQuarter.Value > 0 ? Quarters[ReportingQuarter.Value].ToString() : null;
            DataContextObject.DatasetItem.ReportingYear = SelectedYear;
            DataContextObject.DatasetItem.DateImported = DateTime.Now;
     
            if (!DataContextObject.DatasetItem.IsReImport)
                DataContextObject.SaveImportEntry(DataContextObject.DatasetItem);

            return base.OnNext();
        }

        /// <summary>
        /// Called when [initialize commands].
        /// </summary>
        private void OnInitializeCommands()
        {
            ExecuteMappingFileUpload = new DelegateCommand(OnMappingFileUpload, () => true);
        }

        /// <summary>
        /// Called when maaping file upload action is performed, it opens the file exlorer.
        /// </summary>
        private void OnMappingFileUpload()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = "*.xml",
                InitialDirectory = MonahrqContext.MappingFileExportDirPath,
                Filter = "Xml Files (*.xml)|*.xml",
                //Filter = "Xml Files (*.xml)|*.xml", All Files (*.*)|*.*|
                FilterIndex = 1,
                Multiselect = false
            };

            if (dlg.ShowDialog() != true) return;

            SelectDataMappingFile = dlg.FileName;
            OnPropertyChanged(@"SelectDataMappingFile");
        }
    }
}
