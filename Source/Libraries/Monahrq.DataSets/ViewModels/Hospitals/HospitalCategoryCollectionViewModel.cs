using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Events;
using Monahrq.DataSets.Services;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.DataSets.ViewModels.Hospitals;
using Microsoft.Practices.Prism.Commands;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Events;

namespace Monahrq.DataSets.ViewModels.Hospitals
{


    using Monahrq.Infrastructure.Entities.Domain.Hospitals;

    [Export(typeof(HospitalCategoryCollectionViewModel))]
    public class HospitalCategoryCollectionViewModel : BaseViewModel, INavigationAware, IPartImportsSatisfiedNotification, IEntityCollection
    {
        

        [Import("HospitalCategoryViewModels", AllowRecomposition = true)] //this import suposse to update it self when collection changes
        public ObservableCollection<HospitalCategoryViewModel> HospitalCategoryViewModels { get; set; }

        public DelegateCommand AddCategoryCommand { get; set; }

        public IHospitalDataService HospitalDataService { get; set; }

        [ImportingConstructor]
        public HospitalCategoryCollectionViewModel(IHospitalDataService service)
        {
            EntityType = typeof(HospitalCategory);
            HospitalDataService = service;
            EventAggregator.GetEvent<HospitalCategoryCollectionChangedEvent>().Subscribe(_reconcile);
        }

        private void _reconcile(bool obj)
        {
           //
        }

        //TODO abastrat : shitty code alert (for demo)
        protected override void OnCommitted()
        {
            if(string.IsNullOrEmpty(NewCategoryViewModel.Name)) return;

            try
            {
                var cat = new HospitalCategory(HospitalDataService.LazyHospitalRegistry.Value)
                    {
                        Name = NewCategoryViewModel.Name
                        
                    };

                var vm = new HospitalCategoryViewModel(cat);
                HospitalDataService.HospitalCategoryViewModels.Add(vm);


                EventAggregator.GetEvent<HospitalCategoryCollectionChangedEvent>().Publish(true);
                HospitalDataService.HospitalCategoryViewModels.Move(HospitalCategoryViewModels.Count-1,0);
                SelectedCategory = vm;
                EventAggregator.GetEvent<GenericNotificationEvent>().Publish(string.Format("Category {0} has updated", vm.Name));

                NewCategoryViewModel=new HospitalCategoryViewModel(new HospitalCategory(null));
                
                
            }
            catch (Exception ex)
            {
                //TODO FIX BUGS IN THE INFASTRUCTURE
                //EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(ex);
            }
           
            _reset();

        }

        private void _reset()
        {
           
           NewCategoryViewModel =new HospitalCategoryViewModel(new HospitalCategory(HospitalDataService.LazyHospitalRegistry.Value));
            NewCategoryViewModel.Name = string.Empty;
            ClearErrors(Title);
            Committed = true;

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            //
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //
        }

        public void OnImportsSatisfied()
        {
            if(HospitalCategoryViewModels==null) throw new Exception("Hospitals Category Data Failed to load. Please rebuild data base");

           
            //HospitalCategoryViewModels.CollectionChanged += delegate
            //{
            //    RaisePropertyChanged(() => HospitalCategoryViewModels);
            //    EventAggregator.GetEvent<Events.HospitalCategoryCollectionChangedEvent>().Publish();
            //};

            EventAggregator.GetEvent<Events.HospitalCategoryDeletedEvent>().Subscribe(vm => this.HospitalCategoryViewModels.Remove(vm));
            NewCategoryViewModel =
               new HospitalCategoryViewModel(new HospitalCategory(null));
        }

        private string _title;
        public string Title
        {
            get { return _title;}
            set { _title = value;
            RaisePropertyChanged(()=>Title);
            _validatedTitle(ExtractPropertyName(() => Title), Title);


            }
        }

        private void _validatedTitle(string propertyName, string val)
        {
            ClearErrors(propertyName);

            NewCategoryViewModel.Name = val;
            var errors = GetErrors(NewCategoryViewModel.Name);
            if (errors!=null)
            {
                SetError(propertyName,errors.ToString());
            }
        }

        public Type EntityType { get; set; }

        HospitalCategoryViewModel _selectedCatgeory;
        public HospitalCategoryViewModel SelectedCategory
        {
            get
            {
                return _selectedCatgeory;
            }
            set
            {
                if (value == _selectedCatgeory) return;
                _newCategoryViewModel = value;
                RaisePropertyChanged(() => SelectedCategory);
            }
        }

        HospitalCategoryViewModel _newCategoryViewModel;
        public HospitalCategoryViewModel NewCategoryViewModel
        {
            get
            {
                return _newCategoryViewModel;
            }
            set
            {
                if (value == _newCategoryViewModel) return;
                _newCategoryViewModel = value;
                RaisePropertyChanged(() => NewCategoryViewModel);
            }
        }
    }
}
