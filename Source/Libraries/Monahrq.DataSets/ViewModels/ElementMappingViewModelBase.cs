using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Model;
using Monahrq.Theme.Controls.Wizard.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using NHibernate.Linq;
using Monahrq.DataSets.Services;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Events;
using PropertyChanged;
using DelegateCommand = Microsoft.Practices.Prism.Commands.DelegateCommand;
using System.Collections;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.DataSets.ViewModels
{
    /// <summary>
    /// The dataset import wizard element mapping view model base class.
    /// </summary>
    /// <typeparam name="TWizardDataContextObject">The type of the wizard data context object.</typeparam>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{TWizardDataContextObject}" />
    public abstract class ElementMappingViewModelBase<TWizardDataContextObject> : WizardStepViewModelBase<TWizardDataContextObject>, IElementMappingViewModel 
        where TWizardDataContextObject : DatasetContext
    {
        /// <summary>
        /// All
        /// </summary>
        private const string ALL = "ALL";
        /// <summary>
        /// The mapped
        /// </summary>
        private const string MAPPED = "MAPPED";
        /// <summary>
        /// The automapped
        /// </summary>
        private const string AUTOMAPPED = "AUTOMAPPED";
        /// <summary>
        /// The unmapped
        /// </summary>
        private const string UNMAPPED = "UNMAPPED";
        /// <summary>
        /// The name
        /// </summary>
        private const string NAME = "Name";
        /// <summary>
        /// The order
        /// </summary>
        private const string ORDER = "Field Order";

        /// <summary>
        /// Gets or sets the save or cancel popup mapping command.
        /// </summary>
        /// <value>
        /// The save or cancel popup mapping command.
        /// </value>
        public ICommand SaveOrCancelPopupMappingCommand { get; set; }
        /// <summary>
        /// Gets or sets the remove mapping command.
        /// </summary>
        /// <value>
        /// The remove mapping command.
        /// </value>
        public ICommand RemoveMappingCommand { get; set; }
        /// <summary>
        /// Gets or sets the show sample values command.
        /// </summary>
        /// <value>
        /// The show sample values command.
        /// </value>
        public ICommand ShowSampleValuesCommand { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementMappingViewModelBase{TWizardDataContextObject}"/> class.
        /// </summary>
        /// <param name="c">The c.</param>
        protected ElementMappingViewModelBase(TWizardDataContextObject c)
            : base(c)
        {
            SaveOrCancelPopupMappingCommand = new DelegateCommand<object>(UpdateMapping, CanUpdateMapping);
            RemoveMappingCommand = new DelegateCommand<MOriginalField>(ExecuteRemoveMapping, arg => true);
            ShowSampleValuesCommand = new DelegateCommand<string>(ExecuteShowSampleValuesCommand, arg => true);

            MappedFieldsForValues = new ObservableCollection<MOriginalField>();
        }

        /// <summary>
        /// Executes the show sample values command.
        /// </summary>
        /// <param name="action">The action.</param>
        private void ExecuteShowSampleValuesCommand(string action)
        {
            if (string.IsNullOrEmpty(action)) return;

            switch (action.ToUpper())
            {
                case "SHOW":

                    MappedFieldsForValues = SourceFields.OfType<MOriginalField>()
                                           .Where(f => f.IsMapped && f.TargetField != null && f.TargetField.IsMapped)
                                           .ToObservableCollection();

                    ShowMappedValuesPopup = true;
                    break;
                default:
                    ShowMappedValuesPopup = false;

                    MappedFieldsForValues = new ObservableCollection<MOriginalField>();
                    break;
            }
        }

        /// <summary>
        /// Gets or sets the index of the target fields tab.
        /// </summary>
        /// <value>
        /// The index of the target fields tab.
        /// </value>
        public int TargetFieldsTabIndex { get; set; }

        /// <summary>
        /// Executes the remove mapping.
        /// </summary>
        /// <param name="mappedField">The mapped field.</param>
        public void ExecuteRemoveMapping(MOriginalField mappedField)
        {
            if (mappedField == null) return;

            mappedField.RemoveMapping = true; // If set to false and the TargetField property is still mapped then triggers functionality to finsh unmapping.

            if (mappedField.IsAutoMapped)
                mappedField.IsAutoMapped = false;

            var target = mappedField.TargetField;

            if (target == null) return;

            if (target.IsRequired)
            {
                RequiredTargetFields.AddNewItem(target);
                RequiredTargetFields.CommitNew();
                RequiredTargetFields.Refresh();

                TargetFieldsTabIndex = 0;
            }
            else
            {
                OptionalTargetFields.AddNewItem(target);
                OptionalTargetFields.CommitNew();
                OptionalTargetFields.Refresh();

                TargetFieldsTabIndex = 1;
            }

            mappedField.OnMappingChanged(EventArgs.Empty);

            mappedField.RemoveMapping = false;

            UpdateMappedFieldsCount();
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show mapped values popup].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show mapped values popup]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowMappedValuesPopup { get; set; }

        /// <summary>
        /// Gets the mappings.
        /// </summary>
        /// <value>
        /// The mappings.
        /// </value>
        protected abstract MappingDictionary Mappings { get; }

        /// <summary>
        /// Elements the filter.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        protected abstract bool ElementFilter(Element element);

        /// <summary>
        /// Gets or sets the dataservices.
        /// </summary>
        /// <value>
        /// The dataservices.
        /// </value>
        private IDataContextServices Dataservices { get; set; }

        /// <summary>
        /// The is loading
        /// </summary>
        private bool _isLoading;

        /// <summary>
        /// For when yous need to set up some values that can't be directly bound to UI elements.
        /// </summary>
        public override void BeforeShow()
        {
            _isLoading = true;

            Dataservices = DataContextObject.Services;
            var providerFactory = new DataproviderFactory(Dataservices);
            providerFactory.InitDataProvider();
            var stage = new ImportStageModel(providerFactory);

            var temp = stage.SourceFields.Select(fld => new MOriginalField
            {
                Name = fld.Name,
                Values = new ObservableCollection<string>(stage.GetColumnValues(fld.Name, true)),
                FieldOrder = fld.Order
            }).ToList();

            var original = new ObservableCollection<MOriginalField>(temp);
            var factory = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>().SessionFactory;
            int positionIndex = 0;
            using (var session = factory.OpenSession())
            {
                DataContextObject.TargetElements = session.Query<Element>()
                                                          .Where(elem => elem.Owner.Id == DataContextObject.SelectedDataType.Target.Id)
                                                          .ToList();
            }

            var targetFields = DataContextObject.TargetElements
                                                .Where(ElementFilter)
                                                .Select(elem => new MTargetField(elem, original)
                                                {
                                                    Position = positionIndex++,
                                                    IsRequired = elem.IsRequired
                                                })
                                                .ToList();


            //TargetFields = MakeView(targetFields);
            DataTypeRequiredFieldCount = MappedFieldsCount = targetFields.Count(f => f.IsRequired);

            TargetFields = new ListCollectionView(targetFields);
            SourceFields = MakeView(temp);
            TotalSourceFields = SourceFields.Count;
            //ReconcileTargets();
            ReconcileTargets2();



            foreach (var f in SourceFields.OfType<MOriginalField>())
            {
                f.MappingChanged += f_MappingChanged2;
            }

            FilterText = string.Empty;
            ShowEnumeration = new ObservableCollection<string> { ALL, AUTOMAPPED, MAPPED, UNMAPPED };
            SelectedShow = ShowEnumeration[0];

            FieldSortOrder = new ObservableCollection<string> { ORDER, NAME };
            SelectedSortOrder = FieldSortOrder[0];

            TargetFieldModels.ToList().Clear();
            var newList = TargetFieldModels.Where(field => !SourceFields.OfType<MOriginalField>()
                                                                        .ToList()
                                                                        .Any(f => f.TargetField != null && f.TargetField.Name.EqualsIgnoreCase(field.Name)))
                                           .ToList();

            _targetFields = new ListCollectionView(newList);

            var requiredFields = TargetFieldModels.Where(f => f.IsRequired).OrderBy(f => f.Name).ToList();
            RequiredTargetFields = new ListCollectionView(requiredFields) { CustomSort = new MTargetFieldComparer() };

            var optionalFields = TargetFieldModels.Where(f => !f.IsRequired).OrderBy(f => f.Name).ToList();
            OptionalTargetFields = new ListCollectionView(optionalFields) { CustomSort = new MTargetFieldComparer() };

            _isLoading = false;
        }

        /// <summary>
        /// The data type required field count
        /// </summary>
        protected int DataTypeRequiredFieldCount;

        /// <summary>
        /// Gets or sets the selected unmapped field.
        /// </summary>
        /// <value>
        /// The selected unmapped field.
        /// </value>
        public MOriginalField SelectedUnmappedField { get; set; }

        /// <summary>
        /// Gets or sets the source fields.
        /// </summary>
        /// <value>
        /// The source fields.
        /// </value>
        public ListCollectionView SourceFields
        {
            get { return _sourceFields; }
            set
            {
                _sourceFields = value;
                OnPropertyChanged();
                UpdateMappedFieldsCount();
            }
        }

        /// <summary>
        /// Gets or sets the selected mapped field.
        /// </summary>
        /// <value>
        /// The selected mapped field.
        /// </value>
        public MTargetField SelectedMappedField { get; set; }

        /// <summary>
        /// Gets the target field models.
        /// </summary>
        /// <value>
        /// The target field models.
        /// </value>
        private IEnumerable<MTargetField> TargetFieldModels
        {
            get { return TargetFields.OfType<MTargetField>(); }
        }

        /// <summary>
        /// Reconciles the targets2.
        /// </summary>
        private void ReconcileTargets2()
        {
            //var bag = Mappings;
            Mappings.LoadHints2(TargetFieldModels, DataContextObject, SourceFields.OfType<MOriginalField>().ToList());
            foreach (var field in TargetFieldModels)
            {
                var mappedFrom = Mappings[field.Name];

                if (string.IsNullOrEmpty(mappedFrom)) continue;

                var sourceField = SourceFields.OfType<MOriginalField>()
                                              .FirstOrDefault(fld => fld.Name == mappedFrom);

                if (sourceField == null) continue;

                sourceField.TargetField = field;
                field.MappedField = sourceField;
                sourceField.IsAutoMapped = true;
            }

            UpdateMappedFieldsCount();
        }

        /// <summary>
        /// The pop filter text
        /// </summary>
        private string _popFilterText;

        /// <summary>
        /// Gets or sets the pop filter text.
        /// </summary>
        /// <value>
        /// The pop filter text.
        /// </value>
        public string PopFilterText
        {
            get { return _popFilterText; }
            set
            {
                _popFilterText = value;
                OnPropertyChanged();
                UpdatePopUpDataGrid(PopFilterText);
            }
        }

        /// <summary>
        /// Updates the pop up data grid.
        /// </summary>
        /// <param name="filter">The filter.</param>
        private void UpdatePopUpDataGrid(string filter)
        {
            if (string.IsNullOrEmpty((filter ?? string.Empty).Trim()))
            {
                foreach (var field in SelectedTargetField.OriginalFields)
                {
                    field.FieldVisibility = Visibility.Visible;
                }
            }
            else
            {
                foreach (var field in SelectedTargetField.OriginalFields)
                {
                    field.FieldVisibility =
                        string.IsNullOrEmpty(field.Name)
                            ? Visibility.Visible
                            : field.Name.StartsWith(filter, true, CultureInfo.InvariantCulture)
                                  ? Visibility.Visible
                                  : Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// The selected target field
        /// </summary>
        private MTargetField _selectedTargetField;

        /// <summary>
        /// Gets or sets the selected target field.
        /// </summary>
        /// <value>
        /// The selected target field.
        /// </value>
        public MTargetField SelectedTargetField
        {
            get { return _selectedTargetField; }
            set
            {
                _selectedTargetField = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The selected original field
        /// </summary>
        private MOriginalField _selectedOriginalField;

        /// <summary>
        /// Gets or sets the selected original field.
        /// </summary>
        /// <value>
        /// The selected original field.
        /// </value>
        public MOriginalField SelectedOriginalField
        {
            get { return _selectedOriginalField; }
            set
            {
                _selectedOriginalField = value;
                OnPropertyChanged();
            }
        }

        // Save or undo mappings made in the popup dialog depending on the user clicking Ok or Cancel
        /// <summary>
        /// Updates the mapping.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void UpdateMapping(object parameter)
        {
            if (parameter.ToString() == "Save")
            {
            }
            else if (parameter.ToString() == "Cancel")
            {
                if (SelectedTargetField.MappedField == null)
                {
                    SelectedOriginalField = null;
                }
                else
                {
                    SelectedOriginalField = SelectedTargetField.MappedField;
                }
            }
        }

        /// <summary>
        /// Determines whether this instance [can update mapping] the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can update mapping] the specified parameter; otherwise, <c>false</c>.
        /// </returns>
        private bool CanUpdateMapping(object parameter)
        {
            return true;
        }

        /// <summary>
        /// The mapped field
        /// </summary>
        private int _mappedField;

        /// <summary>
        /// Gets or sets the mapped fields count.
        /// </summary>
        /// <value>
        /// The mapped fields count.
        /// </value>
        public int MappedFieldsCount
        {
            get { return _mappedField; }
            set
            {
                _mappedField = value;
                OnPropertyChanged();
            }
        }

        private static readonly IComparer FieldOrderComparer = new GenericComparer<MOriginalField, int>(x => x.FieldOrder);
        private static readonly IComparer NameComparer = new GenericComparer<MOriginalField, string>(x => x.Name);

        /// <summary>
        /// Sorts the elements.
        /// </summary>
        /// <param name="sortOrder">The sort order.</param>
        private void SortElements(string sortOrder)
        {
            if (_isLoading) return;

            var selectedShow = SelectedShow;
            SelectedShow = ALL;

            SourceFields.CustomSort = string.IsNullOrEmpty(sortOrder ?? string.Empty) || sortOrder.EqualsIgnoreCase(ORDER)
                ? FieldOrderComparer
                : NameComparer;
            SelectedShow = selectedShow;
        }

        /// <summary>
        /// The selected sort
        /// </summary>
        private string _selectedSort;

        /// <summary>
        /// Gets or sets the selected sort.
        /// </summary>
        /// <value>
        /// The selected sort.
        /// </value>
        public string SelectedSort
        {
            get { return _selectedSort; }
            set
            {
                _selectedSort = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The sort enumeration
        /// </summary>
        private ObservableCollection<String> _sortEnumeration;

        /// <summary>
        /// Gets or sets the sort enumeration.
        /// </summary>
        /// <value>
        /// The sort enumeration.
        /// </value>
        public ObservableCollection<String> SortEnumeration
        {
            get { return _sortEnumeration; }
            set
            {
                _sortEnumeration = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// The filter text
        /// </summary>
        private string _filterText;

        /// <summary>
        /// Gets or sets the filter text.
        /// </summary>
        /// <value>
        /// The filter text.
        /// </value>
        public string FilterText
        {
            get { return _filterText; }
            set
            {
                _filterText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The selected show
        /// </summary>
        private string _selectedShow;

        /// <summary>
        /// Gets or sets the selected show.
        /// </summary>
        /// <value>
        /// The selected show.
        /// </value>
        public string SelectedShow
        {
            get { return _selectedShow; }
            set
            {
                if (_selectedShow != value)
                {
                    _selectedShow = value;
                    OnPropertyChanged();
                    CurrentElementFilter = FilterText;
                    ElementFilterChangedCommand.Execute(FilterText);
                }
            }
        }

        /// <summary>
        /// The show enumeration
        /// </summary>
        private ObservableCollection<String> _showEnumeration;

        /// <summary>
        /// Gets or sets the show enumeration.
        /// </summary>
        /// <value>
        /// The show enumeration.
        /// </value>
        public ObservableCollection<String> ShowEnumeration
        {
            get { return _showEnumeration; }
            set
            {
                _showEnumeration = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The selected sort order
        /// </summary>
        private string _selectedSortOrder;

        /// <summary>
        /// Gets or sets the selected sort order.
        /// </summary>
        /// <value>
        /// The selected sort order.
        /// </value>
        public string SelectedSortOrder
        {
            get { return _selectedSortOrder; }
            set
            {
                _selectedSortOrder = value;
                OnPropertyChanged();
                SortElements(_selectedSortOrder);
            }
        }

        /// <summary>
        /// The field sort order
        /// </summary>
        private ObservableCollection<String> _fieldSortOrder;
        /// <summary>
        /// Gets or sets the field sort order.
        /// </summary>
        /// <value>
        /// The field sort order.
        /// </value>
        public ObservableCollection<string> FieldSortOrder
        {
            get { return _fieldSortOrder; }
            set
            {
                _fieldSortOrder = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The source fields
        /// </summary>
        private ListCollectionView _sourceFields;

        /// <summary>
        /// The target fields
        /// </summary>
        private ListCollectionView _targetFields;
        /// <summary>
        /// The mapped fields for values
        /// </summary>
        private ObservableCollection<MOriginalField> _mappedFieldsForValues;
        /// <summary>
        /// The total mapped fields count
        /// </summary>
        private int _totalMappedFieldsCount;
        /// <summary>
        /// The total source fields
        /// </summary>
        private int _totalSourceFields;

        /// <summary>
        /// Gets or sets the target fields.
        /// </summary>
        /// <value>
        /// The target fields.
        /// </value>
        public ListCollectionView TargetFields
        {
            get { return _targetFields; }
            set
            {
                _targetFields = value;
                OnPropertyChanged();
                UpdateMappedFieldsCount();
            }
        }

        // private ListCollectionView _requiredTargetFields;
        /// <summary>
        /// Gets or sets the required target fields.
        /// </summary>
        /// <value>
        /// The required target fields.
        /// </value>
        public ListCollectionView RequiredTargetFields
        //public ObservableCollection<MTargetField> RequiredTargetFields
        {
            get;
            set;
        }

        //private ListCollectionView _optionalRargetFields;
        /// <summary>
        /// Gets or sets the optional target fields.
        /// </summary>
        /// <value>
        /// The optional target fields.
        /// </value>
        public ListCollectionView OptionalTargetFields
        //public ObservableCollection<MTargetField> OptionalTargetFields
        {
            get;
            set;
        }

        //public ListCollectionView OptionalTargetFields
        /// <summary>
        /// Gets or sets the mapped fields for values.
        /// </summary>
        /// <value>
        /// The mapped fields for values.
        /// </value>
        public ObservableCollection<MOriginalField> MappedFieldsForValues
        {
            get { return _mappedFieldsForValues; }
            set
            {
                _mappedFieldsForValues = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the required field count.
        /// </summary>
        /// <value>
        /// The required field count.
        /// </value>
        public int RequiredFieldCount
        {
            get { return RequiredTargetFields.Count; }
        }

        /// <summary>
        /// Gets the optional field count.
        /// </summary>
        /// <value>
        /// The optional field count.
        /// </value>
        public int OptionalFieldCount
        {
            get { return OptionalTargetFields.Count; }
        }

        /// <summary>
        /// Gets or sets the total mapped fields count.
        /// </summary>
        /// <value>
        /// The total mapped fields count.
        /// </value>
        public int TotalMappedFieldsCount
        {
            get { return _totalMappedFieldsCount; }
            set
            {
                _totalMappedFieldsCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the total source fields.
        /// </summary>
        /// <value>
        /// The total source fields.
        /// </value>
        public int TotalSourceFields
        {
            get { return _totalSourceFields; }
            set
            {
                _totalSourceFields = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Updates the mapped fields count.
        /// </summary>
        private void UpdateMappedFieldsCount()
        {
            MappedFieldsCount = SourceFields != null
                                      ? SourceFields.SourceCollection
                                                    .OfType<MOriginalField>()
                                                    .Count(field => field.TargetField != null && field.TargetField.IsRequired && field.TargetField.IsMapped)
                                      : 0;

            TotalMappedFieldsCount = SourceFields != null
                                      ? SourceFields.SourceCollection
                                                    .OfType<MOriginalField>()
                                                    .Count(field => field.TargetField != null && field.TargetField.IsMapped)
                                      : 0;
        }

        /// <summary>
        /// Handles the MappingChanged2 event of the f control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExtendedEventArgs{MOriginalField}"/> instance containing the event data.</param>
        private void f_MappingChanged2(object sender, ExtendedEventArgs<MOriginalField> e)
        {
            ReconcileMappingBag2(e.Data);
            UpdateMappedFieldsCount();
        }

        /// <summary>
        /// Reconciles the mapping bag2.
        /// </summary>
        /// <param name="mSourceField">The m source field.</param>
        private void ReconcileMappingBag2(MOriginalField mSourceField)
        {
            var bag = Mappings;
            if (mSourceField != null && mSourceField.TargetField != null && !mSourceField.RemoveMapping)
            {
                bag[mSourceField.TargetField.Name] = mSourceField.Name;
            }
            else if (mSourceField != null && mSourceField.TargetField != null && mSourceField.RemoveMapping)
            {

                if (mSourceField.RemoveMapping)
                {
                    if (!string.IsNullOrEmpty(Mappings[mSourceField.TargetField.Name]))
                        Mappings.Remove(mSourceField.TargetField.Name);

                    mSourceField.TargetField = null;
                    SourceFields.CommitEdit();
                    SourceFields.Refresh();

                    UpdateMappedFieldsCount();
                }
            }
        }

        #region FILTER FEATURE

        /// <summary>
        /// Gets or sets the element filter changed command.
        /// </summary>
        /// <value>
        /// The element filter changed command.
        /// </value>
        public ICommand ElementFilterChangedCommand { get; set; }

        /// <summary>
        /// Gets or sets the filter target elements command.
        /// </summary>
        /// <value>
        /// The filter target elements command.
        /// </value>
        public ICommand FilterTargetElementsCommand { get; set; }

        /// <summary>
        /// Gets or sets the current element filter.
        /// </summary>
        /// <value>
        /// The current element filter.
        /// </value>
        public string CurrentElementFilter { get; set; }

        /// <summary>
        /// The search text
        /// </summary>
        private string _searchText;
        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        /// <value>
        /// The search text.
        /// </value>
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (_searchText == value) return;

                _searchText = value;
                RequiredTargetFields.Filter = FilterTargetElements;
                OptionalTargetFields.Filter = FilterTargetElements;
            }
        }

        /// <summary>
        /// Makes the view.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        private ListCollectionView MakeView(IEnumerable<MOriginalField> items)
        {
            items = items.DefaultIfEmpty();

            var col = new ObservableCollection<MOriginalField>(items);
            if (ElementFilterChangedCommand == null)
            {
                WireFilterCommand2();
            }

            return (ListCollectionView)CollectionViewSource.GetDefaultView(col);
        }

        /// <summary>
        /// Wires the filter command2.
        /// </summary>
        private void WireFilterCommand2()
        {
            ElementFilterChangedCommand = new DelegateCommand(() =>
            {
                SourceFields.Filter = o => ElementEntryMatchesFilter2(o, CurrentElementFilter);
            });
        }

        /// <summary>
        /// Elements the entry matches filter2.
        /// </summary>
        /// <param name="entryObj">The entry object.</param>
        /// <param name="txt">The text.</param>
        /// <returns></returns>
        private bool ElementEntryMatchesFilter2(object entryObj, string txt)
        {
            var entry = entryObj as MOriginalField;

            if (_isLoading) return true;

            var result = true;

            if (entry == null) return result;
            result = string.IsNullOrEmpty(txt ?? string.Empty) || entry.Name.ToUpper().Contains(txt.ToUpper());

            if (result)
            {
                switch (SelectedShow)
                {
                    case AUTOMAPPED:
                        result = entry.IsMapped && entry.IsAutoMapped;
                        break;
                    case MAPPED:
                        result = entry.IsMapped;
                        break;
                    case UNMAPPED:
                        result = !entry.IsMapped;
                        break;
                    default:
                        result = true;
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Filters the target elements.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private bool FilterTargetElements(object obj)
        {
            if (string.IsNullOrEmpty(SearchText)) return true;

            var originalField = obj as MTargetField;

            if (originalField == null) return false;

            var index = originalField.Name.IndexOf(SearchText, 0, StringComparison.InvariantCultureIgnoreCase);

            return index > -1;
        }

        #endregion
    }
    
    /// <summary>
    /// The target field comparer.
    /// </summary>
    /// <seealso cref="System.Collections.IComparer" />
    public class MTargetFieldComparer : IComparer
    {
        /// <summary>
        /// Compares the specified obj1.
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public int Compare(object obj1, object obj2)
        {
            var item1 = obj1 as MTargetField;
            var item2 = obj2 as MTargetField;

            if (item1 == null && item2 == null) throw new ArgumentException("");

            return item1.Name.CompareTo(item2.Name);
        }
    }
}
