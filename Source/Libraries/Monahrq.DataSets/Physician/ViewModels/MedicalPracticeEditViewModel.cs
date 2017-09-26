using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Monahrq.DataSets.HospitalRegionMapping.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;

using Monahrq.Sdk.Regions;
using Monahrq.Sdk.ViewModels;
using NHibernate;
using NHibernate.Linq;
using PropertyChanged;
using Monahrq.Infrastructure.Domain.Physicians;
using MessageBox = System.Windows.MessageBox;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Types;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Utilities;

namespace Monahrq.DataSets.Physician.ViewModels
{
    [Export]
    [ImplementPropertyChanged]
    public class MedicalPracticeEditViewModel : DetailsViewModel<MedicalPractice>
    {
        #region Properites

        public ObservableCollection<string> AvailableStates { get; set; }

        public string ViewLabel { get; set; }

        public string NumberOfMembers { get; set; }

        #endregion

        #region Imports

        public Visibility ShowAddressPopup { get; set; }

        public bool IsEditingAddress { get; set; }

        public MedicalPracticeAddress SelectedAddress { get; set; }

        #endregion

        #region Commands
        public DelegateCommand<MedicalPractice> AddNewAdress { get; set; }

        public DelegateCommand<MedicalPracticeAddress> EditAddress { get; set; }

        public DelegateCommand<MedicalPracticeAddress> RemoveAddress { get; set; }

        public DelegateCommand SaveAddress { get; set; }

        public DelegateCommand CancelAddress { get; set; }

        public DelegateCommand CancelMedicalPractice { get; set; }

        public DelegateCommand SaveMedicalPractice { get; set; }

        #endregion

        #region Methods

        protected override void InitCommands()
        {
            base.InitCommands();

            AddNewAdress = new DelegateCommand<MedicalPractice>(OnNewAddAdress);
            CancelMedicalPractice = new DelegateCommand(OnCancelMedicalPractice);
            EditAddress = new DelegateCommand<MedicalPracticeAddress>(OnEditAddress);
            CancelAddress = new DelegateCommand(OnCancelAddress);
            SaveAddress = new DelegateCommand(OnSaveNewAddress);
            RemoveAddress = new DelegateCommand<MedicalPracticeAddress>(OnRemoveAddress);
        }

        private void OnRemoveAddress(MedicalPracticeAddress obj)
        {
            if (obj == null || Model == null || Model.Addresses == null) return;

            Model.Addresses.Remove(obj);

            var tempAddresses = Model.Addresses;
            Model.Addresses = new List<MedicalPracticeAddress>();
            Model.Addresses = tempAddresses;
        }

        private void OnSaveNewAddress()
        {

            Model.Validate();
            if (Model.HasErrors) return;

            SelectedAddress.Validate();
            if (SelectedAddress.HasErrors) return;

            ShowAddressPopup = Visibility.Collapsed;

            if (Model.Addresses == null)
                Model.Addresses = new List<MedicalPracticeAddress>();

            if (IsEditingAddress)
            {
                IsEditingAddress = false;
                return;
            }

            Model.Addresses.Add(SelectedAddress);

            var tempAddresses = Model.Addresses;
            Model.Addresses = new List<MedicalPracticeAddress>();
            Model.Addresses = tempAddresses;
        }

        private void OnCancelAddress()
        {
            IsEditingAddress = false;
            ShowAddressPopup = Visibility.Collapsed;
            SelectedAddress = null;
        }

        private void OnEditAddress(MedicalPracticeAddress obj)
        {
            IsEditingAddress = true;
            SelectedAddress = obj;
            ShowAddressPopup = Visibility.Visible;
        }

        private void OnCancelMedicalPractice()
        {
            ShowAddressPopup = Visibility.Collapsed;
        }

        private void OnNewAddAdress(MedicalPractice obj)
        {
            if (Model == null) return;

            IsEditingAddress = false;
            SelectedAddress = new MedicalPracticeAddress();
            SelectedAddress.State = AvailableStates.Select(state => state).FirstOrDefault();
            SelectedAddress.PropertyChanged += (o, e) => SelectedAddress.Validate();
            ShowAddressPopup = Visibility.Visible;
        }

        public override async void OnSave(bool enableNotificantions = false)
        {
            if (string.IsNullOrEmpty(Model.Name) || string.IsNullOrEmpty(Model.GroupPracticePacId))
            {
                MessageBox.Show("Orginization Name and Group Id fields are required", "Invalid Input");
                return;
            }

            int num;
            if (int.TryParse(NumberOfMembers, out num))
            {
                Model.NumberofGroupPracticeMembers = num;
            }
            else if (!string.IsNullOrEmpty(NumberOfMembers))
            {
                MessageBox.Show("Please enter a valid number for Group Practice Members", "Invalid Input");
                return;
            }

            Validate();
            Model.Validate();
            Model.Addresses.ForEach(a => a.Validate());

            if (HasErrors || Model.HasErrors || Model.Addresses.Any(a => a.HasErrors)) return;

            try
            {
                using (var session = DataServiceProvider.SessionFactory.OpenStatelessSession())
                {
                    if (session.Query<MedicalPractice>().FirstOrDefault(x => x.Id != Model.Id && x.GroupPracticePacId == Model.GroupPracticePacId) != null)
                    {
                        MessageBox.Show("There is already a medical practice with the same Group Practice Pac Id. Group Practice Pac Id must be unique.", "Notification");
                        return;
                    }
                }

                Model.IsEdited = true;

                var errorOccurred = false;
                Exception errorException = null;
                var progressService = new ProgressService();

                progressService.SetProgress("Saving " + Inflector.Titleize2(typeof(MedicalPractice).Name), 0, false, true);

               // await Task.Delay(500);

                var operationComplete = await progressService.Execute(() => OnSave(Model), 
                                                result =>
                                                {
                                                    if (!result.Status && result.Exception != null)
                                                    {
                                                        errorOccurred = true;
                                                        errorException = result.Exception;
                                                    }
                                                    else
                                                    {
                                                        errorOccurred = false;
                                                        errorException = null;
                                                    }
                                                }, 
                                                new CancellationToken());

                if (operationComplete)
                {
                    progressService.SetProgress("Completed", 100, true, false);

                    if (!errorOccurred || errorException == null)
                    {
                        EventAggregator.GetEvent<GenericNotificationEvent>()
                                       .Publish(string.Format("MedicalPractice {0} has been saved successfully.", Model.Name));
                        EventAggregator.GetEvent<RequestLoadMappingTabEvent>().Publish(typeof (MedicalPracticeEditViewModel));
                        NavigateBack();
                    }
                    else
                    {
                        Logger.Write(errorException, "Error saving Medical Practice \"{0}\"", Model.Name);
                    }
                }

            }
            catch (Exception exc)
            {
                Logger.Write(exc, "Error saving Medical Practice \"{0}\"", Model.Name);
            }
        }

        public override void OnCancel()
        {
            NavigateBack();
        }

        private void NavigateBack()
        {
            var q = new UriQuery { { "TabIndex", "3" } };
            RegionManager.RequestNavigate(RegionNames.MainContent, new Uri(ViewNames.MainDataSetView + q, UriKind.Relative));
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            ShowAddressPopup = Visibility.Collapsed;

            base.OnNavigatedTo(navigationContext);

            var medicalPracticeId = navigationContext.Parameters["MedicalPracticeId"] != null
                                       ? int.Parse(navigationContext.Parameters["MedicalPracticeId"])
                                       : (int?)null;
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            AvailableStates = configService.HospitalRegion.DefaultStates.OfType<string>().ToObservableCollection();

            if (medicalPracticeId.HasValue && medicalPracticeId.Value > 0)
            {
                LoadModel(medicalPracticeId);
                ViewLabel = string.Format("Edit Medical Practice: {0}", Model.Name);
            }
            else
            {
                Model = new MedicalPractice();
                ViewLabel = "Add New Medical Practice";
            }

            NumberOfMembers = Model.NumberofGroupPracticeMembers.HasValue ? Model.NumberofGroupPracticeMembers.ToString() : string.Empty;

            Validate();

   

            //using (var session = DataServiceProvider.SessionFactory.OpenStatelessSession())
            //{
            //    AvailableStates = session.Query<State>()
            //        .Where(state => state.Name != null)
            //            .ToObservableCollection();
            //}
        }

        protected override void ExecLoad(ISession session, object id)
        {
            //Model = session.Query<MedicalPractice>().First(x => x.Id == (int)id);
            base.ExecLoad(session, id);
            var tempAddresses = Model.Addresses.DistinctBy(x => x.Id).ToObservableCollection();
            Model.Addresses = null;
            Model.Addresses = tempAddresses.RemoveNullValues();
        }

        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        #endregion

    }
}
