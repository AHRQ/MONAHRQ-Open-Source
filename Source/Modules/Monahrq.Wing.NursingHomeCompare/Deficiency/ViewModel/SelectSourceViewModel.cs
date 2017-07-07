using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Theme.Controls.Wizard.Models;
using PropertyChanged;
using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Services.BaseData;
using Monahrq.Wing.NursingHomeCompare.Deficiency.Model;
using NHibernate.Linq;

namespace Monahrq.Wing.NursingHomeCompare.Deficiency.ViewModel
{
	/// <summary>
	/// Handles user selection of importing NH Deficiency data.
	/// </summary>
	/// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{Monahrq.Wing.NursingHomeCompare.Deficiency.Model.WizardContext}" />
	[ImplementPropertyChanged]
    public class SelectSourceViewModel : WizardStepViewModelBase<WizardContext>
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
        }

		/// <summary>
		/// The default year select text
		/// </summary>
		const string DEFAULT_YEAR_SELECT_TEXT = "Please Select Reporting Year";
		//const string DEFAULT_QUARTER_SELECT_TEXT = "Please Select Reporting Quarter";

		/// <summary>
		/// Generates the data.
		/// </summary>
		private void GenerateData()
        {
            Years = BaseDataService.ReportingYears;
            Years.Insert(0, DEFAULT_YEAR_SELECT_TEXT);

            if (DataContextObject.DatasetItem.Id > 0)
            {
                SelectedYear = DataContextObject.DatasetItem.ReportingYear;

                
                Title = DataContextObject.DatasetItem.File;
                DataContextObject.DatasetItem.IsFinished = true;
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

		/// <summary>
		/// Selects the file.
		/// Let the user browse for the file to import
		/// </summary>
		private void SelectFile()
        {
              // Set filter for file extension and default file extension 
            var configSvc = ServiceLocator.Current.GetInstance<IConfigurationService>();

            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = "*.csv",
                InitialDirectory = configSvc.LastDataFolder,
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
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
		/// Returns true if ... is valid.
		/// </summary>
		/// <returns></returns>
		public override bool IsValid()
        {
            if (null == SelectedFile) return false; // no selected file 
            if (!File.Exists(SelectedFile)) return false; // if any file is missing 
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
		public override Theme.Controls.Wizard.Helpers.RouteModifier OnNext()
        {
            DataContextObject.DatasetItem.File = Title;

            // Year is required, but Quarter isn't. So if SelectedQuarter is the default choice, don't use it.
            //DataContextObject.DatasetItem.Description = SelectedQuarter.Equals(DEFAULT_QUARTER_SELECT_TEXT, StringComparison.OrdinalIgnoreCase)
            //    ? SelectedYear
            //    : string.Format("{0} {1}", SelectedQuarter, SelectedYear);

            //DataContextObject.DatasetItem.ReportingQuarter = SelectedQuarter.Equals(DEFAULT_QUARTER_SELECT_TEXT, StringComparison.OrdinalIgnoreCase) ? null : SelectedQuarter;
            DataContextObject.DatasetItem.ReportingYear = SelectedYear;
            DataContextObject.DatasetItem.DateImported = DateTime.Now;

            if (!DataContextObject.DatasetItem.IsPersisted)
                DataContextObject.DatasetItem.IsFinished = true;
     
            // this updates the the ContentType, and sets CurrentContentItem in the DataContext, and saves the record.
            // TODO: we might not need to call Save until the Wizard finishes, but if not, we'll need to set ContentType and CurrentContentItem anyway.
            DataContextObject.SaveImportEntry(DataContextObject.DatasetItem);

            //var uploadedFile = SelectedFile.FirstOrDefault();

            //if (uploadedFile == null)
            //    MessageBox.Show("Please select a file.", "Error occurred while importing file.", MessageBoxButton.OK);

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
