using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Monahrq.DataSets.HospitalRegionMapping.Events;
using Monahrq.DataSets.HospitalRegionMapping.Mapping;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Services.Hospitals;

using Monahrq.Sdk.ViewModels;
using NHibernate;
using PropertyChanged;
using System.Windows;
using NHibernate.Linq;

namespace Monahrq.DataSets.HospitalRegionMapping.Categories
{
    /// <summary>
    /// The hospital categories listing view model.
    /// This view model is also responsibile for the CRUD operations on the hospital categories.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.ViewModels.ListTabViewModel{Monahrq.Infrastructure.Entities.Domain.Hospitals.HospitalCategory}" />
    [Export(typeof(CategoriesViewModel)), ImplementPropertyChanged]
    public class CategoriesViewModel : ListTabViewModel<HospitalCategory>
    {

        #region Fields and Constants

        private string _newCategoryName;

        #endregion

        #region Properties

        private string _filterText;
        /// <summary>
        /// Gets or sets the filter text.
        /// </summary>
        /// <value>
        /// The filter text.
        /// </value>
        public string FilterText //{ get; set; }
        {
            get { return _filterText; }
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                }
                OnFilterCollection(_filterText);
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Called when [filter collection].
        /// </summary>
        /// <param name="filterText">The filter text.</param>
        private void OnFilterCollection(string filterText)
        {
            CollectionItems.Filter = item =>
            {
                var category = item as HospitalCategory;

                if (category == null || string.IsNullOrEmpty(filterText)) return true;

                return category.Name.ContainsIgnoreCase(filterText);
            };
        }

        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        [Import]
        IHospitalRegistryService Service { get; set; }

        /// <summary>
        /// Gets the new length of the categorie name maximum.
        /// </summary>
        /// <value>
        /// The new length of the categorie name maximum.
        /// </value>
        public int NewCategorieNameMaxLength { get { return 200; } }

        /// <summary>
        /// Gets or sets the new name of the category.
        /// </summary>
        /// <value>
        /// The new name of the category.
        /// </value>
        [StringLength(199,ErrorMessage ="Category Name must be less than 200 characters")]
        public string NewCategoryName
        {
            get
            {
                // do not Trim because user needs to be able to type space character
                return _newCategoryName ?? string.Empty;
            }
            set
            {
                _newCategoryName = value;
                AddCategoryCommand.RaiseCanExecuteChanged();
                Validate();
            }
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        //public IModuleController Controller { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public MappingViewModel Parent { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets or sets the add category command.
        /// </summary>
        /// <value>
        /// The add category command.
        /// </value>
        public DelegateCommand AddCategoryCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoriesViewModel"/> class.
        /// </summary>
        public CategoriesViewModel()
        { }

        #endregion

        #region Methods


        /// <summary>
        /// Initializes the commands.
        /// </summary>
        protected override void InitCommands()
        {
            base.InitCommands();
            AddCategoryCommand = new DelegateCommand(OnAddCategory, CanAddNewCategory);
        }

        /// <summary>
        /// Initializes the properties.
        /// </summary>
        protected override void InitProperties()
        {
            base.InitProperties();
            Index = 1;
          
        }

        /// <summary>
        /// Handles the IsActiveChanged event of the ListTabViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        protected override void ListTabViewModel_IsActiveChanged(object sender, EventArgs e)
        {
            base.ListTabViewModel_IsActiveChanged(sender, e);
            NewCategoryName = string.Empty;
        }

        // called when user clicks Add Category to add a new category name
        /// <summary>
        /// Called when [add category].
        /// </summary>
        private void OnAddCategory()
        {
            var category = new HospitalCategory { Name = NewCategoryName };

            if (category.Name.Length < NewCategorieNameMaxLength)
            {
                OnSaveSelectedItem(category);
                ForceLoad();
                NewCategoryName = string.Empty;
            }
        }

        /// <summary>
        /// Determines whether this instance [can add new category].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can add new category]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanAddNewCategory()
        {
            return !string.IsNullOrWhiteSpace(NewCategoryName);
        }

        // called when editing a category is complete, user clicked Save
        /// <summary>
        /// Called when [save].
        /// </summary>
        protected override void OnSave()
        {
            //var vm = CurrentItem;
            //if (vm == null) return;
            //try
            //{
            //    var toSave = vm.HospitalCategory;
            //    if (!CurrentItem.IsEditing)
            //    {
            //        if (string.IsNullOrWhiteSpace(NewCategoryName)) return;
            //        toSave = Controller.Service.CreateHospitalCategory(NewCategoryName);
            //    }
            //    vm = new CategoryViewModel(new HospitalCategoryDto { Category = toSave /*, HospitalCount = 0*/ });
            //    Controller.Service.Save(toSave);
            //    Events.GetEvent<CategoriesViewModelSavedEvent>().Publish(this);

            //    ItemsView.MoveCurrentTo(vm);

            //    Events.GetEvent<GenericNotificationEvent>().Publish(string.Format("Category {0} has been updated", vm.HospitalCategory.Name));
            //    NewCategoryName = string.Empty;
            //}
            //catch (Exception ex)
            //{
            //    Events.GetEvent<ErrorNotificationEvent>().Publish(ex);
            //}
            //finally
            //{
            //    Items = Controller.RefreshCategories().Items;
            //    ItemsView = new ListCollectionView(Items);
            //    Reset(Items, "Title");
            //    CurrentItem = vm;
            //}
        }

        /// <summary>
        /// Executes the load.
        /// </summary>
        /// <param name="session">The session.</param>
        protected override void ExecLoad(ISession session)
        {
            CollectionItems = session.Query<HospitalCategory>()
                                     .OrderByDescending(x => x.Id)
                                     .Select(x => x).ToListCollectionView();

            ReloadSelectedStates(session);
            FilterText = null;
        }

        /// <summary>
        /// Reloads the selected states.
        /// </summary>
        /// <param name="session">The session.</param>
        public void ReloadSelectedStates(ISession session)
        {
           // var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            var selectedStates = ConfigurationService.HospitalRegion.DefaultStates.OfType<string>().ToList();

            CollectionItems.OfType<HospitalCategory>().ForEach(x =>
            {
                x.HospitalCountForSelectedRegion = GetHospitalCountForCategory(session, x.Id,
                    selectedStates);
            });
        }

        /// <summary>
        /// Get the individual counts for each category from the database for the given category and states.
        /// </summary>
        /// <param name="session">Database session.</param>
        /// <param name="categoryId">Id of current category.</param>
        /// <param name="stateAbbrevs">States separated by strings.</param>
        /// <returns></returns>
        public int GetHospitalCountForCategory(ISession session, int categoryId, IEnumerable<string> stateAbbrevs)
        {
            var statesString = "'" + String.Join("','", stateAbbrevs) + "'";

            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine("SELECT count(h.[Id]) ");
            queryBuilder.AppendLine("FROM [dbo].[Hospitals] h ");
            queryBuilder.AppendLine("	 inner join Hospitals_HospitalCategories hc on hc.Hospital_Id = h.Id ");
            queryBuilder.AppendLine("Where h.IsArchived = 0 and h.IsDeleted=0 ");
            queryBuilder.AppendLine("and hc.Category_Id = " + categoryId + " ");
            queryBuilder.AppendLine("	and h.State in (" + statesString + ") ");

            return session.CreateSQLQuery(queryBuilder.ToString()).UniqueResult<int>();
        }

        /// <summary>
        /// Called when [save selected item].
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="showSuccessConfirmation">if set to <c>true</c> [show success confirmation].</param>
        protected override void OnSaveSelectedItem(HospitalCategory obj, bool showSuccessConfirmation = true)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj.Name)) return;

            base.OnSaveSelectedItem(obj);            
        }

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            EventAggregator.GetEvent<CategoriesViewModelReadyEvent>().Publish(this);

            NewCategoryName = string.Empty;

            PropertyChanged += (o, e) => ValidateTitle(Title);


            EventAggregator.GetEvent<ContextAppliedEvent>().Subscribe(s =>
            {
                if (s.Equals("Saved") && IsActive) OnLoad();
            });
        }

        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        /// <summary>
        /// Called to determine if this instance can handle the navigation request.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        /// <see langword="true" /> if this instance accepts the navigation request; otherwise, <see langword="false" />.
        /// </returns>
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        /// <summary>
        /// Validates the title.
        /// </summary>
        /// <param name="val">The value.</param>
        private void ValidateTitle(string val)
        {
            var titlePropertyName = ExtractPropertyName(() => Title);
            ClearErrors(() => Title);
            var errors = GetErrors(NewCategoryName);
            if (errors != null)
            {
                //SetError(titlePropertyName, errors.ToString());
            }
        }

        /// <summary>
        /// Determines whether this instance can commit the specified argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>
        ///   <c>true</c> if this instance can commit the specified argument; otherwise, <c>false</c>.
        /// </returns>
        protected bool CanCommit(object argument)
        {
            //var temp = base.CanCommit(argument);
            //return temp && !string.IsNullOrWhiteSpace(NewCategoryName);
            return true;
        }

        /// <summary>
        /// Called when [delete].
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected override void OnDelete(HospitalCategory entity)
        {

            if (entity.HospitalCountForSelectedRegion != 0)
            {
                MessageBox.Show("Before deleting this Hospital Category, please remove all hospitals with references to it.", "Can't Delete Hospital Category with References",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show(string.Format("Delete Category: {0}", entity.Name), "Delete Category?",
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
            {
                return;
            }

            var name = entity.Name;
            base.OnDelete(entity);
            OnLoad();
        }

        /// <summary>
        /// Called when [cancel].
        /// </summary>
        protected override void OnCancel()
        {
            base.OnCancel();

            OnLoad();
        }
        #endregion
    }
}
