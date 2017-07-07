using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.BaseData.ViewModel;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services.BaseData;
using Monahrq.Sdk.Types;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Physician.Physicians.Models;
using NHibernate.Linq;
using PropertyChanged;

namespace Monahrq.Wing.Physician.Physicians.ViewModels
{
    [ImplementPropertyChanged]
    public class SelectSourceViewModel : WizardStepViewModelBase<WizardContext>
    {
        #region Fields and Constants

        private string _selectedManagement;
        private List<string> _physicianManagementOption;

        private const string MANAGE_IN_MONAHRQ = "I want to include descriptive data about physicians in MONAHRQ";
        private const string REAL_TIME_MANAGEMENT = "I want my generated website to get the most current version of Physician data.";


        #endregion

        #region Properties

        public string SelectedFile { get; set; }

        public string Title { get; set; }

        public ObservableCollection<string> Quarters { get; set; }

        public string SelectedQuarter { get; set; }

        public ObservableCollection<string> Years { get; set; }

        public string SelectedYear { get; set; }

        public override string DisplayName { get { return "Select Input File"; } }

        public List<SelectListItem> AvailableStates { get; set; }

        public List<string> PhysicianManagementOption
        {
            get
            {
                return _physicianManagementOption ?? (_physicianManagementOption = new List<string>
                {
                  MANAGE_IN_MONAHRQ,
                  REAL_TIME_MANAGEMENT
                });
            }
            set { 
                _physicianManagementOption = value;
        }
        }

        public string SelectedManagement 
        {
            get 
            { 
                return _selectedManagement; 
            }
            set 
            {
                _selectedManagement = value;
                ShowRealTimeApiMessage = !string.IsNullOrEmpty(_selectedManagement) && _selectedManagement.EqualsIgnoreCase(REAL_TIME_MANAGEMENT);

                ConfigurationService.UseApiForPhysicians = !_selectedManagement.EqualsIgnoreCase(MANAGE_IN_MONAHRQ);
            }
        }

        public bool ShowInternetDropMessage { get; set; }

        public bool ShowRealTimeApiMessage { get; set; }

        #endregion

        #region Imports
        public IBaseDataService BaseDataService { get; set; }
        public IEventAggregator EventAggregator { get; set; }
        public IConfigurationService ConfigurationService { get; set; }
        #endregion

        #region Commands
        public DelegateCommand SelectFileCommand { get; private set; }

        public DelegateCommand<string> DeleteFileCommand { get; set; }

        #endregion

        #region Constructor

        public SelectSourceViewModel(WizardContext context)
            : base(context)
        {
            BaseDataService = ServiceLocator.Current.GetInstance<IBaseDataService>();
            EventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            ConfigurationService = ServiceLocator.Current.GetInstance<IConfigurationService>();

            SelectFileCommand = new DelegateCommand(SelectFile);
            DeleteFileCommand = new DelegateCommand<string>(DeleteFile);

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

            AvailableStates = BaseDataService.States(state => state.Name != null)
                .Where(x => x.Data != null)
                .Select(x =>
                    new SelectListItem
                    {
                        Text = x.Data.Name,
                        Model = x,
                        Value = x.Data.Abbreviation
                    }).ToList();

            SelectedManagement = PhysicianManagementOption[0];

            PropertyChanged += (o, e) =>
            {
                if (e.PropertyName.Equals("SelectedManagement"))
                {
                    IsValid();
                }
            };

            MonahrqContext.CheckIfConnectedToInternet();

            EventAggregator.GetEvent<InternetConnectionEvent>().Subscribe(e =>
                {
                    ShowInternetDropMessage = (e == ConnectionState.OffLine);
                });
        }

        #endregion

        #region Methods

        private void SelectFile()
        {
            // Set filter for file extension and default file extension 
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = "*.csv",
                InitialDirectory = ConfigurationService.LastDataFolder,
                Filter = "All Files (*.*)|*.*|CSV Files (*.csv)|*.csv",
                FilterIndex = 1,
                Multiselect = false
            };

            if (dlg.ShowDialog() == true)
            {
                SelectedFile = dlg.FileName;
                ConfigurationService.LastDataFolder = Path.GetDirectoryName(SelectedFile);
            }

        }

        private void DeleteFile(string fileName)
        {
            if (!string.IsNullOrEmpty(SelectedFile) && SelectedFile.EqualsIgnoreCase(fileName))
                SelectedFile = null;
        }

        public override bool IsValid()
        {
            if (string.IsNullOrEmpty(Title) || AvailableStates.All(x => !x.IsSelected) || Title.Length >= 200) return false;

            if (!MonahrqContext.CheckIfConnectedToInternet()) return false;

            //if (!SelectedManagement.EqualsIgnoreCase(RealTimeManagement)) return false;
            
            return true;
        }

        public override Theme.Controls.Wizard.Helpers.RouteModifier OnNext()
        {
            MonahrqContext.CheckIfConnectedToInternet();

            var abbvStates = AvailableStates.Where(x => x.IsSelected).Select(x => ((EntityViewModel<State, int>)x.Model).Data).Distinct();

            //if (!HospitalRegion.Default.IsDefined)
            //    HospitalRegion.Default.SelectedRegionType = typeof(CustomRegion);

           // HospitalRegion.Default.DefaultStates = new StringCollection();
            //var contextStates = HospitalRegion.Default.SelectedStates.ToList();

            var selectedStates = abbvStates as List<State> ?? abbvStates.ToList();

            //selectedStates.ForEach(s =>
            //    {
            //        //if (contextStates.All(s1 => s1.Abbreviation != s.Abbreviation))
            //        //{
            //        //    contextStates.Add(s);
            //        //}
            //    });

            //HospitalRegion.Default.DefaultStates.AddRange(selectedStates.Select(x => x.Abbreviation).ToArray());
            //HospitalRegion.Default.SelectedStates = contextStates.ToList();
            //HospitalRegion.Default.Save();

            DataContextObject.DatasetItem.File = Title;
            DataContextObject.DatasetItem.DateImported = DateTime.Now;

            if (DataContextObject.DatasetItem.IsPersisted)
                DataContextObject.DatasetItem.IsFinished = true;

            DataContextObject.DatasetItem.ProviderStates = string.Join(",", selectedStates.Select(x => x.Abbreviation).ToArray());
            DataContextObject.DatasetItem.UseRealtimeData = PhysicianManagementOption[1] == SelectedManagement;

            // TODO: we might not need to call Save until the Wizard finishes, but if not, we'll need to set ContentType and CurrentContentItem anyway.
            DataContextObject.SaveImportEntry(DataContextObject.DatasetItem);

            DataContextObject.File = new FileProgress
            {
                FileName = SelectedFile,
                LinesDone = 0,
                PercentComplete = 0,
                TotalLines = 0,
            };
            DataContextObject.SelectedStates = AvailableStates.Where(x => x.IsSelected).Select(x => x.Value.ToString()).ToList();
            DataContextObject.IsPhysicianManagedInMONAHRQ = PhysicianManagementOption[0] == SelectedManagement;

            return base.OnNext();
        }

        #endregion
    }
}
