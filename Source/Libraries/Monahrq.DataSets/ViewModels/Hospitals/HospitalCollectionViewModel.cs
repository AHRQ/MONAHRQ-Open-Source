using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Forms;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Monahrq.DataSets.Events;
using Monahrq.DataSets.Services;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Hospitals.Mapping;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Services.Import;
using Monahrq.Sdk.Regions;


namespace Monahrq.DataSets.ViewModels.Hospitals
{
    //[Export(typeof(HospitalCollectionViewModel))]
    public class HospitalCollectionViewModel : BaseViewModel, INavigationAware, IPartImportsSatisfiedNotification, IEntityCollection
    {
        //TODO MOVE ALL COMMAND DECALARATIONS INTO GETTER AND SETTER
        //TODO EXTRACT COMMON BEHAVIOR : ON EDIT, ON DELETE, ON ADD NEW 
        //TODO CREATE FILTERABLE COLLECTION VIEW MODEL, INSTEAD OF USING LIST COLLECTION VIEW. 
        //TODO  FIX MEF ISSUES, WHAT EVER U DOING HERE IS NOT WORKING RIGHT ieNUMERABLE  https://skydrive.live.com/?cid=f8b2fd72406fb218&id=F8B2FD72406FB218%21238
        //todo ADD rx observables
        //todo extract common UI elemnts into controls 
        //todo WHY use dependency property on simple property? animation , but why simple properties????? I THINK THATS WRONG TO PUT DEPENCY PROPS ON vm
        //TODO CHANGE BEHAVIOUR OF VALIDATION TO DECLARATIVE  
        //TODO: DECLARATIVE BUSINESS RULES ENGINE , I CANT BELIEVE THEY R GOING TO REVRITE APP EVRY TIME APP CHANGES. 
        //TODO MODEL TYPE ATTRIBUTE 

        public DelegateCommand AddHospitalCommand { get; set; }
        public DelegateCommand CommitCategoryAssigment { get; set; }
        public DelegateCommand ImportHospitalDataCommand { get; set; }
        public DelegateCommand ExportHospitalDataCommand { get; set; }
        public DelegateCommand DeleteHospitalDataCommand { get; set; }
        public DelegateCommand<object> EditHospitalCommand { get; set; }
        public DelegateCommand<object> DeleteHospitalCommand { get; set; }
        public DelegateCommand<string> AssignCategoryCommand { get; set; }

        [Import]
        public IRegionManager RegionManager { get; set; }



        [Import(DataContracts.MAPPING_REFERENCE, AllowRecomposition = true)]
        public HospitalMappingReference RegionMappingReference { get; set; }


        IHospitalDataService HospitalDataService { get; set; }


        public DelegateCommand NavigateToDetailsCommand { get; set; }

        public DelegateCommand ResetCommand { get; set; }

        [ImportingConstructor]
        public HospitalCollectionViewModel(IHospitalDataService service)
        {
            EventAggregator.GetEvent<HospitalCollectionChangedEvent>().Subscribe(UpdateHospitalCollection);
            EventAggregator.GetEvent<HospitalCategorySelectionEvent>().Subscribe(UpdateSelectionTrigger);
            EntityType = typeof(Hospital);
            CommitCategoryAssigment = new DelegateCommand(OnCommitted);
            AssignCategoryCommand = new DelegateCommand<string>(OnAssignCategoryCommand, CanCommiteCategory);
            AddHospitalCommand = new DelegateCommand(OnAddHospital, () => true);
            ImportHospitalDataCommand = new DelegateCommand(OnImportHospitalData, () => true);
            ExportHospitalDataCommand = new DelegateCommand(OnExportHospitalData, CanExportData);
            DeleteHospitalDataCommand = new DelegateCommand(OnDeleteHospitalData, CanDeleteData);
            EditHospitalCommand = new DelegateCommand<object>(OnEdit, CanEdit);
            DeleteHospitalCommand = new DelegateCommand<object>(OnDeleteHospital, CanDeleteHospital);
            FilterEnumerations = new ObservableCollection<string> { "Hospital Name", "Category", "Region", "CMS Provider", "State" };
            SelectedFilter = FilterEnumerations[0];
            HospitalDataService = service;
            SelectedHospitals = new ObservableCollection<HospitalViewModel>();
        }

        private void UpdateSelectionTrigger(HospitalViewModel hospital)
        {
            if (hospital.IsSelected)
            {
                SelectedHospitals.Add(hospital);
            }
            else
            {
                SelectedHospitals.Remove(hospital);
            }
           RaisePropertyChanged(()=>IsAllSelected);
           AssignCategoryCommand.RaiseCanExecuteChanged();
        }

        protected void InitCommands()
        {

            ResetCommand = new DelegateCommand(Reset);

        }

        private void Reset()
        {
            throw new NotImplementedException();
        }

        #region Properties


        public ListCollectionView HospitalsCollectionView { get; set; }

        private HospitalViewModel _selectedHospital;
        public HospitalViewModel SelectedHospital
        {
            get { return _selectedHospital; }
            set
            {
                _selectedHospital = value;
                RaisePropertyChanged(() => SelectedHospital);
            }
        }

        private ObservableCollection<HospitalViewModel> _selectedHospitals;
        public ObservableCollection<HospitalViewModel> SelectedHospitals
        {
            get { return _selectedHospitals; }
            set
            {
                _selectedHospitals = value;
                RaisePropertyChanged(() => SelectedHospitals);
            }
        }
        private string _selectedFilter;

        public string SelectedFilter
        {
            get { return _selectedFilter; }
            set
            {
                _selectedFilter = value;
                RaisePropertyChanged(() => SelectedFilter);
            }
        }


        private ObservableCollection<string> _filterEnumerations;

        public ObservableCollection<string> FilterEnumerations
        {
            get { return _filterEnumerations; }
            set
            {
                _filterEnumerations = value;
                RaisePropertyChanged(() => FilterEnumerations);
            }
        }

        private string _filterText;

        public string FilterText
        {
            get { return _filterText; }
            set
            {
                _filterText = value;
                RaisePropertyChanged(() => FilterText);
            }
        }

        private bool _isCategoryEditOpen;
        public bool IsCategoryEditOpen
        {
            get { return _isCategoryEditOpen; }
            set
            {
                _isCategoryEditOpen = value;
                RaisePropertyChanged(() => IsCategoryEditOpen);
            }
        }
        #endregion

        #region Commands

        private bool _addNewHospitalWindowIsOpen;
        public bool AddNewHospitalWindowIsOpen
        {
            get { return _addNewHospitalWindowIsOpen; }
            set
            {
                _addNewHospitalWindowIsOpen = value;
                RaisePropertyChanged(() => AddNewHospitalWindowIsOpen);
            }
        }

        public ObservableCollection<HospitalCategoryViewModel> HospitalCategoryViewModels
        {
            get
            {
                //TODO do handle the expexception if it gets null here
                return HospitalDataService.HospitalViewModels != null ? HospitalDataService.HospitalCategoryViewModels : null;
            }
        }

        public const string CANCEL = "Cancel";
        public const string SAVE = "SAVE";

        private void OnAssignCategoryCommand(string param)
        {
            RaisePropertyChanged(() => HospitalCategoryViewModels);
            IsCategoryEditOpen = !IsCategoryEditOpen;

            if (CANCEL.Equals(param, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(param))
            {
                //TODO THIS IS CUTTING CORNER FOR DEMO. FIX IT FIX IT
                var h = HospitalDataService.HospitalCategoryViewModels.FirstOrDefault(x => x.Name == string.Empty);
                if (h != null)
                    HospitalDataService.HospitalCategoryViewModels.Remove(h);
                UnselectHospitals();
                return;

            }

            //if (SAVE.Equals(param, StringComparison.OrdinalIgnoreCase)) ;



        }

        private void UnselectHospitals()
        {
            foreach (var hvm in HospitalDataService.HospitalViewModels)
            {
                hvm.IsSelected = false;
            }

            RaisePropertyChanged((() => HospitalDataService.HospitalViewModels));
            RaisePropertyChanged(() => HospitalCategoryViewModels);
        }


        private bool CanCommiteCategory(string s)
        {
            if (HospitalDataService == null || HospitalDataService.HospitalViewModels == null) return false;
            return IsAllSelected || HospitalDataService.HospitalViewModels.Any(x => x.IsSelected);

        }

        protected override void OnCommitted()
        {
            ///TODO : ALL THIS CODE HERE IS TO SUPPORT MULTIPLE SELECTION , REFACTOR THAT INTO BEHAVIOR .

            foreach (var categoryVm in HospitalDataService.HospitalCategoryViewModels)
            {
                foreach (var hospitalVm in HospitalDataService.HospitalViewModels.Where(x => x.IsSelected))
                {
                    if (categoryVm.IsSelected)
                    {
                        if (hospitalVm.Hospital.Categories.All(x => x.Id != categoryVm.HospitalCategory.Id))
                        {

                            hospitalVm.Hospital.Categories.Add(categoryVm.HospitalCategory);
                        }

                    }
                    else
                    {
                        if (hospitalVm.Hospital.Categories.Any(x => x.Id == categoryVm.HospitalCategory.Id))
                        {
                            var c = hospitalVm.Hospital.Categories.First(x => x.Id == categoryVm.HospitalCategory.Id);
                            hospitalVm.Hospital.Categories.Remove(c);
                        }
                    }

                    //Session.Save(hospitalVm.Hospital);

                }
            }

            /*Cleanup*/

            foreach (var vm in HospitalDataService.HospitalCategoryViewModels)
            {
                vm.IsSelected = false;
            }
            IsCategoryEditOpen = false;

            UnselectHospitals();
            EventAggregator.GetEvent<HospitalCategoryCollectionChangedEvent>().Publish(true);
        }
        //TODO FIX CRAPPY CODE THIS IS DEMO VERSION

        private bool _isAllSelected;
        public bool IsAllSelected
        {

            get { return _isAllSelected; }

            set
            {
                _isAllSelected = value;
                ApplySelection(_isAllSelected);
                RaisePropertyChanged(() => IsAllSelected);
                AssignCategoryCommand.RaiseCanExecuteChanged();
            }
        }

        private void ApplySelection(bool value)
        {
            if ((HospitalsCollectionView) == null)
                return ;


            foreach (var m in HospitalsCollectionView.OfType<HospitalViewModel>())
            {
                m.IsSelected = value;
            }

            RaisePropertyChanged(() => HospitalsCollectionView);
           // HospitalsCollectionView.Refresh();
        }


        private bool CanDeleteHospital(object arg)
        {
            return true;
        }

        private void OnDeleteHospital(object obj)
        {
            throw new NotImplementedException();
        }

        private bool CanEdit(object arg)
        {
            return true;
        }

        private void OnEdit(object obj)
        {
            EventAggregator.GetEvent<HospitalsDetailsNavigateEvent>().Publish(SelectedHospital);
            RegionManager.RequestNavigate(RegionNames.MainContent, ViewNames.DetailsView);
        }

        private bool CanDeleteData()
        {
            return true;
        }

        private void OnDeleteHospitalData()
        {
            throw new NotImplementedException();
        }

        private bool CanExportData()
        {
            return true;
        }

        private void OnExportHospitalData()
        {
            throw new NotImplementedException();
        }
        private string _selectedPath;
        public string SelectedPath
        {
            get { return _selectedPath; }
            set
            {
                _selectedPath = value;
                RaisePropertyChanged(() => SelectedPath);
                ImportHospitalDataCommand.RaiseCanExecuteChanged();
            }
        }


        [Import(ImporterContract.Hospital)]
        IEntityFileImporter HospitalImporter { get; set; }

        private void OnImportHospitalData()
        {
            HospitalImporter.Execute();
        }

        private void OnAddHospital()
        {
            AddNewHospitalWindowIsOpen = true;
        }

        #endregion

        #region Navitgation

        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        #endregion

        public void OnImportsSatisfied()
        {
            EventAggregator.GetEvent<HospitalCollectionChangedEvent>().Subscribe(UpdateHospitalCollection);
            HospitalsCollectionView = CollectionViewSource.GetDefaultView(HospitalDataService.HospitalViewModels) as ListCollectionView;
            EventAggregator.GetEvent<SimpleImportCompletedEvent>().Subscribe(payload=>Requery(payload));
        }

        private void Requery(ISimpleImportCompletedPayload payload)
        {
            if (!payload.Inserted.OfType<Hospital>().Any()) return;

            var inserted =  payload.Inserted.Select(hosp => new HospitalViewModel() { Hospital = hosp as Hospital});
            var existing = HospitalsCollectionView  == null ? 
                            Enumerable.Empty<HospitalViewModel>()
                            : HospitalsCollectionView.SourceCollection as IEnumerable<HospitalViewModel>;

            if(HospitalsCollectionView != null) 
            {
                HospitalsCollectionView.DetachFromSourceCollection();
            }
            HospitalsCollectionView = 
                CollectionViewSource.GetDefaultView(new ObservableCollection<HospitalViewModel>(existing.OfType<HospitalViewModel>().Concat(inserted))) as ListCollectionView;
        }

        private void UpdateHospitalCollection(IEnumerable<HospitalViewModel> hospitalViewModels)
        {
            HospitalsCollectionView = CollectionViewSource.GetDefaultView(HospitalDataService.HospitalViewModels) as ListCollectionView;
        }

        public Type EntityType { get; set; }
    }
}
