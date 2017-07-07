using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Services;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.NursingHomeCompare.NHC.Model;
using NHibernate.Linq;
using PropertyChanged;

namespace Monahrq.Wing.NursingHomeCompare.NHC.ViewModel
{
	/// <summary>
	/// Handles user selection of importing NHCompare data.
	/// </summary>
	/// <seealso cref="Wing.NursingHomeCompare.NHC.Model.WizardContext}" />
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
		/// The title
		/// </summary>
		private string _title;
		/// <summary>
		/// The is access file
		/// </summary>
		private bool _isAccessFile;

		/// <summary>
		/// Initializes a new instance of the <see cref="SelectSourceViewModel"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		public SelectSourceViewModel(WizardContext context)
            : base(context)
        {
            SelectFileCommand = new DelegateCommand(SelectFile);
            CurrentFile = String.Empty;
            CurrentFileYear = null;

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
        }

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
		/// Gets the display name.
		/// </summary>
		/// <value>
		/// The display name.
		/// </value>
		public override string DisplayName
        {
            get { return "Select Input Database"; }
        }

		/// <summary>
		/// Returns true if ... is valid.
		/// </summary>
		/// <returns></returns>
		public override bool IsValid()
        {
            return File.Exists(DataContextObject.FileName) && !string.IsNullOrEmpty(Title) && _isAccessFile;
        }

		/// <summary>
		/// Selects the file.
		/// </summary>
		private void SelectFile()
        {
            var configSvc = ServiceLocator.Current.GetInstance<IConfigurationService>();

            // Let the user browse for the file to import
            var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    DefaultExt = "*.mdb;*.accdb", // Set filter for file extension and default file extension 
                    InitialDirectory = configSvc.LastDataFolder,
                    Filter = "Access Databases (*.mdb, *.accdb)|*.mdb;*.accdb",
                    //Filter = "All Files (*.*)|*.*|Access Databases (*.mdb, *.accdb)|*.mdb;*.accdb",
                    FilterIndex = 0
                };

            if (dlg.ShowDialog() == true)
            {
                CurrentFile = dlg.FileName;
                configSvc.LastDataFolder = Path.GetDirectoryName(CurrentFile);
            }
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

        /// <summary>
        /// Gets or sets the current file.
        /// </summary>
        /// <value>
        /// The current file.
        /// </value>
        public string CurrentFile
        {
            get
            {
                return DataContextObject.FileName;
            }

            set
            {
                DataContextObject.FileName = value;

                if (!string.IsNullOrEmpty(DataContextObject.FileName))
                {
                    var extention = Path.GetExtension(DataContextObject.FileName);
                    _isAccessFile = extention.ToLower().Equals(".mdb") || extention.ToLower().Equals(".accdb");
                    if (!_isAccessFile) MessageBox.Show("Please select a valid access database file.", "File Format");
                }
            }
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
            //var item = DataContextObject.DatasetItem ?? new Dataset();

            // TODO: update dates when the datafile changes. Is there anyway to read the date dynamically from the file?
            var fileName = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss");
            DataContextObject.DatasetItem.File = Title;
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
                DeletePreviousNursingHomeCompareItems(DataContextObject.DatasetItem.Id);
            }

            return base.OnNext();
        }

		/// <summary>
		/// Gets or sets the event aggregator.
		/// </summary>
		/// <value>
		/// The event aggregator.
		/// </value>
		[Import]
        public IEventAggregator EventAggregator { get; set; }

		/// <summary>
		/// Deletes the previous nursing home compare items.
		/// </summary>
		/// <param name="datasetId">The dataset identifier.</param>
		private void DeletePreviousNursingHomeCompareItems(int datasetId)
        {
            var datasetService = ServiceLocator.Current.GetInstance<IDatasetService>();
            var query = string.Format("From Dataset o where o.ContentType.Name = '{0}'", DataContextObject.SelectedDataType.DataTypeName);
            datasetService.Query<Dataset>(query, (r, e) =>
            {
                if (e == null)
                {
                    var r2 = r.ToList();
                    if (!r2.Any()) return;

                    foreach (var item in r2)
                    {
                        datasetService.Delete(DataContextObject.TargetType.Name, item);
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
