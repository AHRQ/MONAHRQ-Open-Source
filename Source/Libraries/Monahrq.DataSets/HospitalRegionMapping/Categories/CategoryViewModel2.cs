using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Monahrq.Default.ViewModels;
using System.Windows;
using Monahrq.Infrastructure.Domain.Categories;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Events;
using System;
using Monahrq.DataSets.HospitalRegionMapping.Events;
using Monahrq.Infrastructure.Services.Hospitals;

// Usually put Save in entity view model itself, but put Delete in parent/owner that has collection of entity view model.
// BaseViewModel has CommitCommand that must be over-ridden in Save in each entity view model.
// Cancel command goes in entity view model (cancel edits).
// Cancel command does 2 things: 1) rollback; 2) set committed flag = true. Poor-man's state machine.

// CommitCommand is in BaseViewModel and provides CanCommit. Just override OnCommit in entity view model to call service.

namespace Monahrq.DataSets.HospitalRegionMapping.Categories
{
    [Export(typeof(CategoryViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CategoryViewModel : BaseViewModel, ICloneable
    {
        public DelegateCommand EditCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }

        public bool IsEditing { get; set; }         // TODO: do we need this?
        public bool IsCreatingNew { get; set; }

        public int HospitalCount
        {
            get;
            set;
        }

        //[Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        //public IModuleController ModuleController { get; set; }

        [Import]
        public IRegionManager RegionManager { get; set; }

        [ImportingConstructor]
        public CategoryViewModel(HospitalCategoryDto hospitalCategory)
        {
            HospitalCategory = hospitalCategory.Category;
            HospitalCount = hospitalCategory.HospitalCount;
            Init();
            IsCreatingNew = false;
            //ModuleController = ServiceLocator.Current.GetInstance<IModuleController>();
        }

        private void Init()
        {
            EditCommand = new DelegateCommand(OnEdit, CanEdit);
            CancelCommand = new DelegateCommand(OnCancel, CanCancel);
            DeleteCommand = new DelegateCommand(OnDelete, CanDelete);
            IsEditing = false;
            Committed = true;
        }


        #region Commands


        private bool CanDelete()
        {
            // always true so OnDelete can give user an explanation for why delete isn't allowed
            return true;
        }

        private void OnDelete()
        {
            //var count = ModuleController.Service.Get<HospitalCategory, int>(HospitalCategory.Id, cat => cat.Hospitals.Count);
            var count = HospitalCategory.OwnerCount;
            if (count != 0)
            {
                MessageBox.Show("Before deleting this Hospital Category, please remove all hospitals and/or regions with references to it.",
                    "Can't Delete Hospital Category with References",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show(
                        string.Format("Delete Category: {0}", HospitalCategory.Name),
                        "Delete Category?",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.No) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                Events.GetEvent<CategoryViewModelDeletedEvent>().Publish(this);
            }
            catch (Exception ex)
            {
                Events.GetEvent<ErrorNotificationEvent>().Publish(ex);
            }
            finally
            {
                IsEditing = false;
            }
        }

        private bool CanEdit()
        {
            return true;
        }

        private void OnEdit()
        {
            Events.GetEvent<CategoryViewModelEditingEvent>().Publish(new CancelableEventArgs<CategoryViewModel>(this));
            IsEditing = true;
            Committed = false;
            RaisePropertyChanged(() => IsEditing);
        }

        protected override void OnCommitted()
        {
            try
            {
                if (string.IsNullOrEmpty(HospitalCategory.Name)) return;
                Events.GetEvent<CategoryViewModelSavedEvent>().Publish(this);
                ModuleController.Service.Save(HospitalCategory);
                Events.GetEvent<GenericNotificationEvent>().Publish(string.Format("Category {0} has been updated", HospitalCategory.Name));
            }
            catch (Exception ex)
            {
                Events.GetEvent<ErrorNotificationEvent>().Publish(ex);
            }
            finally
            {
                Committed = true;
                IsEditing = false;
            }
        }

        private bool CanCancel()
        {
            return true;
        }

        // reset and rollback
        private void OnCancel()
        {
            _reset();
        }

        // this includes rollback
        void _reset()
        {
            Events.GetEvent<CategoryViewModelEditCanceledEvent>().Publish(this);
            IsEditing = false;
            Committed = true;
            CancelCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region Properties

        public int CategoryMaxLength
        {
            get { return 200; }            // max length for the user to type in the Category name textbox
        }

        //void _validateName(string propertyName, string name)
        //{
        //    ClearErrors(propertyName);
        //    if (string.IsNullOrWhiteSpace(name) || name.Length > CategoryMaxLength)
        //        SetError(propertyName, "Please ensure category name is 1-200 characters in length.");
        //}

        // before Committed in base class saves, it calls here to verify all properties
        protected override void ValidateAll()
        {
            //ClearErrors(Name);
        }

        private Category _hospitalCategory;
        public Category HospitalCategory
        {
            get { return _hospitalCategory; }
            set
            {
                _hospitalCategory = value;
                RaisePropertyChanged(() => HospitalCategory);
            }
        }

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
        #endregion

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            return HospitalCategory.Name;
        }
    }

}
