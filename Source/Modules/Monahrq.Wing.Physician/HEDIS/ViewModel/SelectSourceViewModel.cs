using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services.BaseData;
using Monahrq.Infrastructure.Utility;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Physician.HEDIS.Model;
using NHibernate.Linq;
using PropertyChanged;

namespace Monahrq.Wing.Physician.HEDIS.ViewModel
{
    [ImplementPropertyChanged]
    public class SelectSourceViewModel : WizardStepViewModelBase<WizardContext>
    {
        public DelegateCommand SelectFileCommand { get; private set; }
        public IBaseDataService BaseDataService { get; private set; }
        public DelegateCommand<string> DeleteFileCommand { get; set; }

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
            }

            Years = BaseDataService.ReportingYears;
            Years.Insert(0, DEFAULT_YEAR_SELECT_TEXT);

            GenerateData();
            SelectFileCommand = new DelegateCommand(SelectFile);
            DeleteFileCommand = new DelegateCommand<string>(DeleteFile);
        }

        const string DEFAULT_YEAR_SELECT_TEXT = "Please Select Reporting Year";
        //const string DEFAULT_QUARTER_SELECT_TEXT = "Please Select Reporting Quarter";

        private void GenerateData()
        {
            if (DataContextObject.DatasetItem.Id > 0)
            {
                SelectedYear = DataContextObject.DatasetItem.ReportingYear;

                
                Title = DataContextObject.DatasetItem.File;
                DataContextObject.DatasetItem.IsFinished = true;
            }
        }

        #region Properties
        public string SelectedFile { get; set; }
      
        string _title;
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

        public ObservableCollection<string> Quarters { get; set; }
        public string SelectedQuarter{ get; set; }
        public ObservableCollection<string> Years { get; set; }
        public string SelectedYear { get; set; }
       
        #endregion

        // Let the user browse for the file to import
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

        private void DeleteFile(string fileName)
        {
            if (!string.IsNullOrEmpty(SelectedFile) && SelectedFile.EqualsIgnoreCase(fileName))
                SelectedFile = null;
        }

        public override string DisplayName
        {
            get { return "Select Input File"; }
        }

        public override bool IsValid()
        {
            if (null == SelectedFile) return false; // no selected file 
            if (!File.Exists(SelectedFile)) return false; // if any file is missing 
            if (string.IsNullOrEmpty(Title)) return false; // title is required 
            if (SelectedYear == DEFAULT_YEAR_SELECT_TEXT) return false; // reporting year is required 

            return true;
        }

        public override Theme.Controls.Wizard.Helpers.RouteModifier OnNext()
        {
            DataContextObject.DatasetItem.File = Title;
            DataContextObject.DatasetItem.ReportingYear = SelectedYear;

            // Year is required, but Quarter isn't. So if SelectedQuarter is the default choice, don't use it.
            //DataContextObject.CurrentContentItem.Description = SelectedQuarter.Equals(DEFAULT_QUARTER_SELECT_TEXT, StringComparison.OrdinalIgnoreCase)
            //    ? SelectedYear
            //    : string.Format("{0} {1}", SelectedQuarter, SelectedYear);

            //DataContextObject.CurrentContentItem.ReportingQuarter = SelectedQuarter.Equals(DEFAULT_QUARTER_SELECT_TEXT, StringComparison.OrdinalIgnoreCase) ? null : SelectedQuarter;
            //DataContextObject.CurrentContentItem.ReportingYear = SelectedYear;
            DataContextObject.DatasetItem.DateImported = DateTime.Now;

            if (!DataContextObject.DatasetItem.IsPersisted)
                DataContextObject.DatasetItem.IsFinished = false;
     
            // this updates the the ContentType, and sets CurrentContentItem in the DataContext, and saves the record.
            // TODO: we might not need to call Save until the Wizard finishes, but if not, we'll need to set ContentType and CurrentContentItem anyway.
            DataContextObject.SaveImportEntry(DataContextObject.DatasetItem);

            //var uploadedFile = SelectedFile.FirstOrDefault();

            //if (uploadedFile == null)
            //    MessageBox.Show("Please select a file.", "Error occurred while importing file.", MessageBoxButton.OK);

            if (DataContextObject.DatasetItem.IsReImport && DataContextObject.ExistingDatasetId.HasValue)
            {
                DeleteExistingIfReport(DataContextObject.ExistingDatasetId.Value);
            }

            DataContextObject.File = new FileProgress
                {
                    FileName = SelectedFile,
                    LinesDone = 0,
                    PercentComplete = 0,
                    LinesDuplicated = 0,
                    LinesErrors = 0,
                    TotalLines = 0

                };
            return base.OnNext();
        }

        private void DeleteExistingIfReport(int datasetId)
        {
            using (var session = DataContextObject.Provider.SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    session.CreateSQLQuery(string.Format("Delete from {0} where [Dataset_Id]={1}", typeof (HEDISTarget).EntityTableName(), datasetId))
                           .ExecuteUpdate();

                    trans.Commit();
                }
            }
        }
    }
}
