using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.HospitalCompare.Model;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Sdk.Services;
using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Services;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using Monahrq.Infrastructure.Extensions;
using NHibernate.Linq;

namespace Monahrq.Wing.HospitalCompare.ViewModel
{
	/// <summary>
	/// Handles user selection of importing HospitalCompare data.
	/// </summary>
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{WizardContext}" />
	[ImplementPropertyChanged]
    public class SelectSourceViewModel : WizardStepViewModelBase<WizardContext>
    {
        public SelectSourceViewModel(WizardContext context)
            : base(context)
        {
            this.SelectFileCommand = new DelegateCommand(this.SelectMdbFile);
            this.SelectZipFileCommand = new DelegateCommand(this.SelectZipFile);
            this.SelectFolderCommand = new DelegateCommand(this.SelectFolder);
            this.ChangeImportTypeCommand = new DelegateCommand<ImportType?>(this.ChangeInputType);
            this.CurrentFile = String.Empty;
            this.CurrentFileYear = null;

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
                Title = DataContextObject.DatasetItem.File;
            }
        }

        public string Title
        {
            get { return _title; }
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
        private string _title;
        

        public ICommand ChangeImportTypeCommand { get; private set; }
        public ICommand SelectFileCommand { get; private set; }
        public ICommand SelectZipFileCommand { get; private set; }
        public ICommand SelectFolderCommand { get; private set; }


        public override string DisplayName
        {
            get { return "Select Input Database"; }
        }

        public string CurrentFileYear
        {
            get { return DataContextObject.Year; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ValidationException("Year must be provided");
                int x;
                if (!int.TryParse(value, out x))
                    throw new ValidationException("The given year is not valid");
                DataContextObject.Year = value;
            }
        }

        public int CurrentFileMonth
        {
            get { return DataContextObject.Month; }
            set
            {
                if (value < 1 || value > 12)
                    throw new ValidationException("Please select a valid month");
                DataContextObject.Month = value; 
            }
        }

        public IEnumerable<KeyValuePair<string, string>> YearList
        {
            get
            {
                const int StartingYear = 2014;
                return Enumerable
                    .Range(StartingYear, DateTime.Now.Year - (StartingYear - 1))
                    .Select(x => new KeyValuePair<string, string>(x.ToString(), x.ToString()));
            }
        }

        public IEnumerable<KeyValuePair<int, string>> MonthList
        {
            get
            {
                var fi = new DateTimeFormatInfo();
                return Enumerable
                    .Range(1, 12)
                    .Select(x => new KeyValuePair<int, string>(x, fi.GetMonthName(x)));
            }
        }

        public string CurrentFile
        {
            get { return DataContextObject.FileName; }
            set { DataContextObject.FileName = value; }
        }

        public bool IsMdbImport
        {
            get { return DataContextObject.ImportType == ImportType.Mdb; }
            set
            {
                if (value)
                    DataContextObject.ImportType = ImportType.Mdb;
                else if (DataContextObject.ImportType == ImportType.Mdb)
                    DataContextObject.ImportType = ImportType.Invalid;
            }
        }

        public bool IsCsvDirImport
        {
            get { return DataContextObject.ImportType == ImportType.CsvDir; }
            set
            {
                if (value)
                    DataContextObject.ImportType = ImportType.CsvDir;
                else if (DataContextObject.ImportType == ImportType.CsvDir)
                    DataContextObject.ImportType = ImportType.Invalid;
            }
        }

        public bool IsZippedCsvDirImport
        {
            get { return DataContextObject.ImportType == ImportType.ZippedCsvDir; }
            set
            {
                if (value)
                    DataContextObject.ImportType = ImportType.ZippedCsvDir;
                else if (DataContextObject.ImportType == ImportType.ZippedCsvDir)
                    DataContextObject.ImportType = ImportType.Invalid;
            }
        }

        [Import]
        public IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// Determines the validity of the selected file to import.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return
                (((this.IsMdbImport || this.IsZippedCsvDirImport) && File.Exists(this.CurrentFile)) 
                  || (this.IsCsvDirImport && Directory.Exists(this.CurrentFile)))
                && Title != null
                && Title.Length.IsBetween(1, 200, true);
        }

		/// <summary>
		/// Prepares user selections for import process.
		/// Base:
		/// For when yous need to save some values that can't be directly bound to UI elements.
		/// Not called when moving previous (see WizardViewModel.MoveToNextStep).
		/// </summary>
		/// <returns>
		/// An object that may modify the route
		/// </returns>
		public override Theme.Controls.Wizard.Helpers.RouteModifier OnNext()
        {
            // var item = DataContextObject.DatasetItem; ?? new Dataset();

            // TODO: update dates when the datafile changes. Is there anyway to read the date dynamically from the file?
            var fileName = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss");
            DataContextObject.DatasetItem.File = Title; // string.Format("Hospital Compare data from {0}", fileName);
            //item.Description = fileName;
            DataContextObject.DatasetItem.Description = null;
            DataContextObject.DatasetItem.Description = string.Format("{0} ({1}){2}", DataContextObject.DatasetItem.File, fileName, ((CurrentFileYear == null)
                ? " - " + CurrentFileYear
                : null));
            DataContextObject.DatasetItem.ReportingYear = CurrentFileYear;
            DataContextObject.DatasetItem.DateImported = DateTime.Now;

            if (DataContextObject.DatasetItem.IsReImport)
                DataContextObject.DatasetItem.IsFinished = true;

            //   DeletePreviousHospitalCompareItems();
            DataContextObject.SaveImportEntry(DataContextObject.DatasetItem);

            if (DataContextObject.DatasetItem.IsReImport)
            {
                DeletePreviousHospitalCompareItems(DataContextObject.DatasetItem.Id);
            }


            return base.OnNext();
        }

        private void SelectMdbFile()
        {
            var configSvc = ServiceLocator.Current.GetInstance<IConfigurationService>();

            // Let the user browse for the file to import
            var dlg = new CommonOpenFileDialog
            {
                DefaultExtension = "*.mdb;*.accdb", // Set filter for file extension and default file extension 
                InitialDirectory = configSvc.LastDataFolder,
                Filters =
                {
                    new CommonFileDialogFilter("Access Databases", "*.mdb;*.accdb") { ShowExtensions = true },
                    new CommonFileDialogFilter("All Files", "*.*") { ShowExtensions = true },
                }
            };

            if (dlg.ShowDialog(Application.Current.MainWindow) != CommonFileDialogResult.Ok)
                return;

            var filename = dlg.FileNames.First();
            CurrentFile = filename;
            configSvc.LastDataFolder = Path.GetDirectoryName(filename);
        }

        private void SelectZipFile()
        {
            var configSvc = ServiceLocator.Current.GetInstance<IConfigurationService>();

            // Let the user browse for the file to import
            var dlg = new CommonOpenFileDialog
            {
                DefaultExtension = "*.zip", // Set filter for file extension and default file extension 
                InitialDirectory = configSvc.LastDataFolder,
                Filters =
                {
                    new CommonFileDialogFilter("Zip Archive ", "*.zip") { ShowExtensions = true },
                    new CommonFileDialogFilter("All Files", "*.*") { ShowExtensions = true },
                }
            };

            if (dlg.ShowDialog(Application.Current.MainWindow) != CommonFileDialogResult.Ok)
                return;
            
            var filename = dlg.FileNames.First();
            CurrentFile = filename;
            configSvc.LastDataFolder = Path.GetDirectoryName(filename);
        }


        private void SelectFolder()
        {
            var configSvc = ServiceLocator.Current.GetInstance<IConfigurationService>();
            var dlg = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dlg.ShowDialog(Application.Current.MainWindow) != CommonFileDialogResult.Ok)
                return;

            var filename = dlg.FileNames.First();
            CurrentFile = filename;
            configSvc.LastDataFolder = Path.GetDirectoryName(filename);
        }

        private void ChangeInputType(ImportType? type)
        {
            base.DataContextObject.ImportType = type ?? ImportType.Invalid;
            base.OnPropertyChanged(nameof(this.IsCsvDirImport));
            base.OnPropertyChanged(nameof(this.IsMdbImport));
            base.OnPropertyChanged(nameof(this.IsZippedCsvDirImport));
        }


        private void DeletePreviousHospitalCompareItems(int datasetId)
        {
            var datasetService = ServiceLocator.Current.GetInstance<IDatasetService>();
            var query = string.Format("From Dataset o where o.ContentType.Name = '{0}' and o.Id={1}", DataContextObject.SelectedDataType.DataTypeName, datasetId);
            datasetService.Query<Dataset>(query, (r, e) =>
            {
                if (e == null)
                {
                    if (r != null && r.Any())
                    {
                        foreach (var item in r)
                        {
                            datasetService.Delete(DataContextObject.TargetType.Name, item);
                        }
                    }
                }
                else
                {
                    EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
                }
            });
        }
    }
}
