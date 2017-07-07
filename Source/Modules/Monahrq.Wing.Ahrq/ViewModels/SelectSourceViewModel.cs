using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Win32;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Services.BaseData;
using Monahrq.Theme.Controls.Wizard.Helpers;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Ahrq.Model;
using NHibernate.Linq;
using PropertyChanged;

namespace Monahrq.Wing.Ahrq.ViewModel
{
    /// <summary>
    /// Class for source view model.
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{Monahrq.Wing.Ahrq.Model.WizardContext}" />
    [ImplementPropertyChanged]
    public class SelectSourceViewModel : WizardStepViewModelBase<WizardContext>
    {
        /// <summary>
        /// Gets or sets the selected files.
        /// </summary>
        /// <value>
        /// The selected files.
        /// </value>
        public ObservableCollection<string> SelectedFiles { get; set; }
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
        /// Initializes a new instance of the <see cref="SelectSourceViewModel"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public SelectSourceViewModel(WizardContext context) : base(context)
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

            GenerateData();
            SelectFileCommand = new DelegateCommand(SelectFile);
            DeleteFileCommand = new DelegateCommand<string>(DeleteFile);
            SelectedFiles = new ObservableCollection<string>();
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        private void DeleteFile(string fileName)
        {
            SelectedFiles.Remove(fileName);
            OnPropertyChanged("SelectedFiles");
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

        const string DEFAULT_YEAR_SELECT_TEXT = "Please Select Reporting Year";
        const string DEFAULT_QUARTER_SELECT_TEXT = "Please Select Reporting Quarter";

        /// <summary>
        /// Generates the data.
        /// </summary>
        private void GenerateData()
        {
            Years = BaseDataService.ReportingYears;
            Years.Insert(0, DEFAULT_YEAR_SELECT_TEXT);

            // TODO: should call BaseDataService.ReportingQuarters...
            Quarters = new ObservableCollection<string> { DEFAULT_QUARTER_SELECT_TEXT, "1st Quarter", "2nd Quarter", "3rd Quarter", "4th Quarter" };

            //SelectedQuarter = Quarters[0];
            //SelectedYear = Years[0];

            if (DataContextObject.DatasetItem.Id > 0)
            {
                SelectedYear = DataContextObject.DatasetItem.ReportingYear;

                var selectedQuarter = 0;
                if (!string.IsNullOrEmpty(DataContextObject.DatasetItem.ReportingQuarter) &&
                    int.TryParse(DataContextObject.DatasetItem.ReportingQuarter, out selectedQuarter))
                    SelectedQuarter = selectedQuarter.ToString();
                else
                    SelectedQuarter = Quarters[0];

                Title = DataContextObject.DatasetItem.File;
                //DataContextObject.DatasetItem.IsFinished = false;

            }
        }

        #region Properties

      
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

        #endregion

        // Let the user browse for the file to import
        /// <summary>
        /// To let the user browse for the file to import.
        /// </summary>
        private void SelectFile()
        {
              // Set filter for file extension and default file extension 
            var configSvc = ServiceLocator.Current.GetInstance<IConfigurationService>();

            var directory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);  
            
            if(Directory.Exists(configSvc.LastDataFolder))
            {
                directory = configSvc.LastDataFolder;
            }

            var dlg = new OpenFileDialog
            {
                DefaultExt = "*.csv",
                InitialDirectory = directory,
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                FilterIndex = 1,
                Multiselect = true
            };

            if (dlg.ShowDialog() == true)
            {
                SelectedFiles.AddRange(dlg.FileNames);
                OnPropertyChanged("SelectedFiles");
                configSvc.LastDataFolder = Path.GetDirectoryName(SelectedFiles[0]);
            }

        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            if (null == SelectedFiles || !SelectedFiles.Any()) return false; // no selected file 
            if (SelectedFiles.Any(f => !File.Exists(f))) return false; // if any file is missing 
            if (string.IsNullOrEmpty(Title)) return false; // title is required 
            if (SelectedYear == DEFAULT_YEAR_SELECT_TEXT) return false; // reporting year is required 

            return true;
        }

        /// <summary>
        /// For when yous need to save some values that can't be directly bound to UI elements.
        /// Not called when moving previous (see WizardViewModel.MoveToNextStep).
        /// </summary>
        /// <returns>
        /// An object that may modify the route
        /// </returns>
        public override RouteModifier OnNext()
        {
            DataContextObject.DatasetItem.File = Title;

            // Year is required, but Quarter isn't. So if SelectedQuarter is the default choice, don't use it.


            DataContextObject.DatasetItem.Description = string.IsNullOrEmpty(SelectedQuarter) || SelectedQuarter.Equals(DEFAULT_QUARTER_SELECT_TEXT, StringComparison.OrdinalIgnoreCase)
                ? SelectedYear
                : string.Format("{0} {1}", SelectedQuarter, SelectedYear);

            DataContextObject.DatasetItem.ReportingQuarter = string.IsNullOrEmpty(SelectedQuarter) || SelectedQuarter.Equals(DEFAULT_QUARTER_SELECT_TEXT, StringComparison.OrdinalIgnoreCase) ? null : SelectedQuarter;
            DataContextObject.DatasetItem.ReportingYear = SelectedYear;
            DataContextObject.DatasetItem.DateImported = DateTime.Now;

            //if (DataContextObject.DatasetItem.IsReImport)
            //DataContextObject.DatasetItem.IsFinished = false;

            // this updates the the ContentType, and sets CurrentContentItem in the DataContext, and saves the record.
            // TODO: we might not need to call Save until the Wizard finishes, but if not, we'll need to set ContentType and CurrentContentItem anyway.
            DataContextObject.SaveImportEntry(DataContextObject.DatasetItem);
            DataContextObject.Files = SelectedFiles.Select(s => new FileProgress{
                FileName = s ,
                LinesDone = 0,
                PercentComplete = 0,
                TotalLines = 0
               
            }).ToList();
            return base.OnNext();
        }

       
    }
}
