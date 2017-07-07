using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Domain.Regions;

namespace Monahrq.DataSets.HospitalRegionMapping.Regions
{

    public interface IRegionViewModel
    {
        Region Region { get; set; }
        string Name { get; set; }
        bool IsSelected { get; set; }
        DelegateCommand DeleteRegionCommand { get; set; }
    }

    [Export(typeof(IRegionViewModel))]
    public class RegionViewModel : BaseViewModel, IRegionViewModel
    {

        public DelegateCommand DeleteRegionCommand { get; set; }
        public DelegateCommand SaveRegionCommand { get; set; }
        public Region Region { get; set; }

        public string State { get; set; }

        public RegionViewModel()
        {
            Events = ServiceLocator.Current.GetInstance<IEventAggregator>();

            IsSelected = false;
        }

        //[Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        //public IModuleController Controller { get; set; }

        [ImportingConstructor]
        public RegionViewModel(Region region)
            : this()
        {
            //Controller = controller;
            Region = region;
            Id = region.Id;
            Name = Region.Name;

            State = Region.State;
            if (string.IsNullOrEmpty(State))
            {
                //var state = Controller.Service.GetState(region);
                //State = state == null ? string.Empty
                //    : state.Abbreviation;
            }

            //NumberOfStates = Controller.Service.

            DeleteRegionCommand = new DelegateCommand(OnRegionDelete, CanDelete);
            SaveRegionCommand = new DelegateCommand(OnUpdate, () => true);
            RegionType = region is CustomRegion
                ? "Custom Region"
                : region is HospitalServiceArea
                ? "HSA"
                : region is HealthReferralRegion
               ? "HRR"
               : "N/A";
        }

        private void OnUpdate()
        {
            if (string.IsNullOrEmpty(Name) || Name.Length < 1)
                SetError("Name", "The Region Name property is required.");

            if (HasErrors) return;

            OnCommitted();
        }

        private bool CanDelete()
        {
            if (Region.GetType() == typeof(CustomRegion)) return true;
            return false;
        }

        private void OnRegionDelete()
        {
            //    if (MessageBox.Show(
            //             string.Format("Delete Region: {0} ?", Region.Name)
            //             , "Delete Region?",
            //             MessageBoxButton.YesNo,
            //             MessageBoxImage.Question,
            //             MessageBoxResult.No) == MessageBoxResult.No) return;

            //    if (Region.IsPersisted)
            //        Controller.Service.Refresh(Region);

            //    if (Region.HospitalCount > 0)
            //    {
            //        MessageBox.Show(
            //            string.Format("Please delete associations with hospitals before deleting region \"{0}\". There are currently {1} hospitals associated with this region.", Region.Name, Region.HospitalCount),
            //            "Delete Region?",
            //            MessageBoxButton.OK,
            //            MessageBoxImage.Exclamation,
            //            MessageBoxResult.No);

            //        return;
            //    }

            //    try
            //    {
            //        Controller.Service.Delete(Region);
            //        var msg = String.Format("Region {0} has been deleted", Name);
            //        Events.GetEvent<GenericNotificationEvent>().Publish(msg);
            //    }
            //    catch (Exception ex)
            //    {
            //        Events.GetEvent<ErrorNotificationEvent>().Publish(ex);
            //    }
        }

        #region Properties

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public string NameForCombo
        {
            get
            {
                var displayName = Name;

                if (State != null)
                    displayName = string.Format("{0} ({1})", displayName, State);

                return displayName;
            }
        }

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged(() => Name);
            }
        }


        //protected override void OnCommitted()
        //{
        //    if (Name.Length < 1) return;

        //    Region.Name = Name;
        //    try
        //    {
        //        //if(Region.IsPersisted)
        //        var temp = Region as CustomRegion;
        //        if (temp == null) return;
        //        Controller.Service.Save(temp);
        //        Committed = true;
        //        var msg = String.Format("Region {0} has been updated", Region.Name);
        //        Events.GetEvent<GenericNotificationEvent>().Publish(msg);
        //    }
        //    catch (Exception ex)
        //    {
        //        Events.GetEvent<ErrorNotificationEvent>().Publish(ex);
        //    }
        //}

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }

        private string _regionType;
        private int _id;

        public string RegionType
        {
            get { return _regionType; }
            set
            {
                _regionType = value;
                RaisePropertyChanged(() => RegionType);
            }
        }

        public string NumberOfStates { get; set; }

        #endregion

        #region Validation

        //private void _validateName(string property, string val)
        //{
        //    ClearErrors(property);
        //    if (string.IsNullOrWhiteSpace(val))
        //    {
        //        SetError(property, "Region name cannot be empty");
        //    }
        //}

        #endregion

    }
}
