using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Dynamic.Models;
using PropertyChanged;
using System;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Services.BaseData;
using NHibernate.Linq;

namespace Monahrq.Wing.Dynamic.ViewModels
{
    /// <summary>
    /// View model class for selecting a source
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{Monahrq.Wing.Dynamic.Models.WizardContext}" />
    [ImplementPropertyChanged]
    public class SimpleSelectSourceViewModel : WizardStepViewModelBase<WizardContext>
    {
        /// <summary>
        /// Gets the select file command.
        /// </summary>
        /// <value>
        /// The select file command.
        /// </value>
        public DelegateCommand SelectFileCommand { get; private set; }
        /// <summary>
        /// Gets the base data service.
        /// </summary>
        /// <value>
        /// The base data service.
        /// </value>
        public IBaseDataService BaseDataService { get; private set; }
        /// <summary>
        /// Gets or sets the delete file command.
        /// </summary>
        /// <value>
        /// The delete file command.
        /// </value>
        public DelegateCommand<string> DeleteFileCommand { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSelectSourceViewModel"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public SimpleSelectSourceViewModel(WizardContext context) : base(context)
        {
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

            LoadData();
            SelectFileCommand = new DelegateCommand(SelectFile);
            DeleteFileCommand = new DelegateCommand<string>(DeleteFile);
        }

        /// <summary>
        /// The default year select text
        /// </summary>
        const string DEFAULT_YEAR_SELECT_TEXT = "Please Select Reporting Year";
        /// <summary>
        /// The default quarter select text
        /// </summary>
        const string DEFAULT_QUARTER_SELECT_TEXT = "Please Select Reporting Quarter";

        /// <summary>
        /// The target disabled message
        /// </summary>
        private const string TARGET_DISABLED_MESSAGE =
            "This dataset has been disabled. Please re-enable this dataset in the Manage Wings and Flutters in the MONAHRQ settings and try again.";

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void LoadData()
        {
            Years = BaseDataService.ReportingYears;
            Years.Insert(0, DEFAULT_YEAR_SELECT_TEXT);

            Quarters = BaseDataService.ReportingQuarters.Select(rq => rq.Text).ToObservableCollection();
            Quarters.Insert(0, DEFAULT_QUARTER_SELECT_TEXT);

            SelectedQuarter = Quarters[0];
            SelectedYear = Years[0];

            if (DataContextObject.DatasetItem.IsReImport)
            {
                Title = DataContextObject.DatasetItem.File;
                SelectedYear = DataContextObject.DatasetItem.ReportingYear;

                if (!string.IsNullOrEmpty(DataContextObject.DatasetItem.ReportingQuarter))
                    SelectedQuarter = DataContextObject.DatasetItem.ReportingQuarter;
            }
        }

        #region Properties
        /// <summary>
        /// Gets or sets the selected file.
        /// </summary>
        /// <value>
        /// The selected file.
        /// </value>
        public string SelectedFile { get; set; }
      
        string _title;
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (value.Length > 200)
                {
                    MessageBox.Show("Please enter 'Title' using fewer than 200 characters.");
                    return;
                }

                _title = value;
            }
        }

        /// <summary>
        /// Gets or sets the quarters.
        /// </summary>
        /// <value>
        /// The quarters.
        /// </value>
        public ObservableCollection<string> Quarters { get; set; }
        /// <summary>
        /// Gets or sets the selected quarter.
        /// </summary>
        /// <value>
        /// The selected quarter.
        /// </value>
        public string SelectedQuarter{ get; set; }
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
        /// Gets the disable message.
        /// </summary>
        /// <value>
        /// The disable message.
        /// </value>
        public string DisableMessage { get { return TARGET_DISABLED_MESSAGE; } }

        /// <summary>
        /// Gets a value indicating whether this instance's target is disabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is target disabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsTargetDisabled { get { return DataContextObject.CustomTarget.IsDisabled; } }

        #endregion

        // Let the user browse for the file to import
        /// <summary>
        /// Lets the user browse for the file to import, Selects the file.
        /// </summary>
        private void SelectFile()
        {
            // Set filter for file extension and default file extension 
            var configSvc = ServiceLocator.Current.GetInstance<IConfigurationService>();

            var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    DefaultExt = "*.csv",
                    InitialDirectory = configSvc.LastDataFolder,
                    Filter = "All Files (*.*)|*.*|CSV Files (*.csv)|*.csv",
                    FilterIndex = 1,
                    Multiselect = false
                };

            if (dlg.ShowDialog() == true)
            {
                SelectedFile = dlg.FileName;
                configSvc.LastDataFolder = Path.GetDirectoryName(SelectedFile);
            }
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        private void DeleteFile(string fileName)
        {
            if (!string.IsNullOrEmpty(SelectedFile) && SelectedFile.EqualsIgnoreCase(fileName))
                SelectedFile = null;
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public override string DisplayName
        {
            get { return "Select Input File"; }
        }

        /// <summary>
        /// Returns true if <see cref="SimpleSelectSourceViewModel"/> is valid.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsValid()
        {
            if (null == SelectedFile) return false; // no selected file 
            if (!File.Exists(SelectedFile)) return false; // if any file is missing 
            if (string.IsNullOrEmpty(Title)) return false; // title is required 
            if (SelectedYear == DEFAULT_YEAR_SELECT_TEXT) return false; // reporting year is required 
            if (DataContextObject.CustomTarget.IsDisabled) return false; // If disabled, do not let user continue.

            return true;
        }

        /// <summary>
        /// For when you need to save some values that can't be directly bound to UI elements.
        /// Not called when moving previous (see WizardViewModel.MoveToNextStep).
        /// </summary>
        /// <returns>
        /// An object that may modify the route
        /// </returns>
        public override Theme.Controls.Wizard.Helpers.RouteModifier OnNext()
        {
            DataContextObject.DatasetItem.File = Title;

            // Year is required, but Quarter isn't. So if SelectedQuarter is the default choice, don't use it.
            //DataContextObject.CurrentContentItem.Description = SelectedQuarter.Equals(DEFAULT_QUARTER_SELECT_TEXT, StringComparison.OrdinalIgnoreCase)
            //    ? SelectedYear
            //    : string.Format("{0} {1}", SelectedQuarter, SelectedYear);

            DataContextObject.DatasetItem.ReportingQuarter = string.IsNullOrEmpty(SelectedQuarter) || SelectedQuarter.Equals(DEFAULT_QUARTER_SELECT_TEXT, StringComparison.OrdinalIgnoreCase) ? null : SelectedQuarter;
            DataContextObject.DatasetItem.ReportingYear = SelectedYear;
            DataContextObject.DatasetItem.DateImported = DateTime.Now;

            if (DataContextObject.DatasetItem.IsReImport)
                DataContextObject.DatasetItem.IsFinished = true;
     
            // this updates the the ContentType, and sets CurrentContentItem in the DataContext, and saves the record.
            // TODO: we might not need to call Save until the Wizard finishes, but if not, we'll need to set ContentType and CurrentContentItem anyway.
            DataContextObject.SaveImportEntry(DataContextObject.DatasetItem);

            DataContextObject.File = new FileProgress
                {
                    FileName = SelectedFile,
                    LinesDone = 0,
                    PercentComplete = 0,
                    TotalLines = 0

                };
            return base.OnNext();
        }

       
    }
}
