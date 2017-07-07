using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.ViewModels.Crosswalk;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Dynamic.Models;
using NHibernate.Linq;
using PropertyChanged;

namespace Monahrq.Wing.Dynamic.ViewModels
{
    /// <summary>
    /// View model Class for full wizard fields
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{Monahrq.Wing.Dynamic.Models.WizardContext}" />
    [ImplementPropertyChanged]
    public class FullWizardFieldsViewModel : WizardStepViewModelBase<WizardContext>
    {
        /// <summary>
        /// Gets or sets the element filter changed command.
        /// </summary>
        /// <value>
        /// The element filter changed command.
        /// </value>
        public ICommand ElementFilterChangedCommand { get; set; }
        /// <summary>
        /// Gets or sets the field entry filter changed command.
        /// </summary>
        /// <value>
        /// The field entry filter changed command.
        /// </value>
        public ICommand FieldEntryFilterChangedCommand { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullWizardFieldsViewModel"/> class.
        /// </summary>
        public FullWizardFieldsViewModel()
            : this(new WizardContext())
        {
            _mappedFieldEntries = MakeView(Enumerable.Empty<MappedFieldEntryViewModel>());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullWizardFieldsViewModel"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public FullWizardFieldsViewModel(WizardContext context)
            : base(context)
        {

        }

        /// <summary>
        /// Gets the caption.
        /// </summary>
        /// <value>
        /// The caption.
        /// </value>
        public string Caption
        {
            get
            {
                return "Map each value to a meaning:";
            }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get
            {
                return "The value of the following variables have specific meanings. Choose the description for the meaning of each value in your input file.";
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
            get
            {
                return "Map Values";
            }
        }

        /// <summary>
        /// Returns true if full wizard fields are valid.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsValid()
        {
            var models = MappedFieldEntries != null ? MappedFieldEntries.OfType<MappedFieldEntryViewModel>().ToList() : null;
            ValidCount = models != null ? models.Count(x => x.IsValid) : 0;
            TotalCount = models != null ? models.Count() : 0;
            ModelIsValid = ValidCount == TotalCount;
            return ModelIsValid;

        }

        /// <summary>
        /// Gets or sets a value indicating whether this model is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if model is valid; otherwise, <c>false</c>.
        /// </value>
        public bool ModelIsValid
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        /// <value>
        /// The total count.
        /// </value>
        public int TotalCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the valid count.
        /// </summary>
        /// <value>
        /// The valid count.
        /// </value>
        public int ValidCount
        {
            get;
            set;
        }

        /// <summary>
        /// Makes the view.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        ListCollectionView MakeView(IEnumerable<MappedFieldEntryViewModel> items)
        {
            var viewItems = new List<MappedFieldEntryViewModel>(items ?? Enumerable.Empty<MappedFieldEntryViewModel>());

            TotalCount = viewItems.Count;
            ValidCount = viewItems.Count(i => i.IsValid);

            PropertyChangedEventHandler handler = (o, e) =>
            {
                IsValid();
                OnPropertyChanged();
            };

            foreach (var item in viewItems)
            {
                var model = item; // as INotifyPropertyChanged;

                if (model == null) continue;

                model.PropertyChanged -= handler;
                model.PropertyChanged += handler;
            }

            var col = new ObservableCollection<MappedFieldEntryViewModel>(viewItems);
            col.CollectionChanged += (s, e) => OnPropertyChanged();
            return (ListCollectionView)CollectionViewSource.GetDefaultView(col);
        }

        /// <summary>
        /// Gets or sets the mapped field models
        /// </summary>
        /// <value>
        /// The mapped field models.
        /// </value>
        public IEnumerable<MappedFieldEntryViewModel> MappedFieldModels
        {
            get
            {
                return _mappedFieldEntries == null
                    ? Enumerable.Empty<MappedFieldEntryViewModel>()
                    : _mappedFieldEntries.SourceCollection as IEnumerable<MappedFieldEntryViewModel>;
            }
            set
            {
                ApplyMappedFields(value);
            }
        }

        /// <summary>
        /// Gets or sets the source fields.
        /// </summary>
        /// <value>
        /// The source fields.
        /// </value>
        public ListCollectionView SourceFields { get; set; }

        /// <summary>
        /// Gets or sets the mapped field entries.
        /// </summary>
        /// <value>
        /// The mapped field entries.
        /// </value>
        private ListCollectionView _mappedFieldEntries { get; set; }
        /// <summary>
        /// This is the view (not typed)
        /// </summary>
        public ListCollectionView MappedFieldEntries
        {
            get { return _mappedFieldEntries; }
        }

        /// <summary>
        /// Applies the mapped fields.
        /// </summary>
        /// <param name="mappedFields">The mapped fields.</param>
        private void ApplyMappedFields(IEnumerable<MappedFieldEntryViewModel> mappedFields)
        {
            _mappedFieldEntries = MakeView(mappedFields);
            ApplySourceFields();
            OnPropertyChanged();
        }

        /// <summary>
        /// Applies the source fields.
        /// </summary>
        private void ApplySourceFields()
        {
            var entries = MappedFieldModels.Where(model => model != null).Select(model => model.FieldEntry.Column.ColumnName).ToList();
            entries.Insert(0, string.Empty);
            SourceFields = (ListCollectionView)CollectionViewSource.GetDefaultView(
                new ObservableCollection<string>(entries));


            ElementFilterChangedCommand = new DelegateCommand(() =>
            {
                MappedFieldEntries.Filter = o => ElementEntryMatchesFilter(o, CurrentElementFilter);

                //if (string.IsNullOrEmpty(CurrentElementFilter))
                //{
                //    ElementPredicate = null;
                //}
                //else
                //{
                //}
                ////                MappedFieldEntries.Filter = CompositePredicate;
            });
        }

        /// <summary>
        /// Gets the composite predicate.
        /// </summary>
        /// <value>
        /// The composite predicate.
        /// </value>
        private Predicate<object> CompositePredicate
        {
            get
            {
                var LeftPred = ElementPredicate ?? (o => true);
                var RightPred = FieldEntryPredicate ?? (o => true);
                return o =>
                {
                    var temp = LeftPred(o);

                    // TODO: uncomment below for composite filter feature; and make dropdown visible in Xaml
                    return temp;   // && (bool)RightPred(o);
                };
            }
        }

        /// <summary>
        /// Gets or sets the element predicate.
        /// </summary>
        /// <value>
        /// The element predicate.
        /// </value>
        Predicate<object> ElementPredicate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the field entry predicate.
        /// </summary>
        /// <value>
        /// The field entry predicate.
        /// </value>
        Predicate<object> FieldEntryPredicate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current element filter.
        /// </summary>
        /// <value>
        /// The current element filter.
        /// </value>
        public string CurrentElementFilter          // left-side
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current field entry filter.
        /// </summary>
        /// <value>
        /// The current field entry filter.
        /// </value>
        public string CurrentFieldEntryFilter       // right-side
        {
            get;
            set;
        }

        /// <summary>
        /// The entry matches filter.
        /// </summary>
        /// <param name="entryObj">The entry object.</param>
        /// <param name="txt">The text.</param>
        /// <returns></returns>
        static bool ElementEntryMatchesFilter(object entryObj, string txt)
        {
            if (string.IsNullOrEmpty(txt)) return true;
            var entry = entryObj as MappedFieldEntryViewModel;
            return entry != null && entry.Element.Description.ToUpper().Contains(txt.ToUpper());
        }

        /// <summary>
        /// Field entry match filter.
        /// </summary>
        /// <param name="entryObj">The entry object.</param>
        /// <returns></returns>
        bool FieldEntryMatchesFilter(object entryObj)
        {
            var entry = entryObj as MappedFieldEntryViewModel;
            if (entry == null) return false;

            return Comparer.Default.Compare(entry.FieldEntry.Column.ColumnName, CurrentFieldEntryFilter) == 0;
        }

        /// <summary>
        /// For when you need to save some values that can't be directly bound to UI elements.
        /// Not called when moving previous (see WizardViewModel.MoveToNextStep).
        /// </summary>
        /// <returns>
        /// An object that may modify the route
        /// </returns>
        public override Theme.Controls.Wizard.Helpers.RouteModifier OnNext()
        {
            DataContextObject.CurrentCrosswalk = MappedFieldModels;
            return base.OnNext();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is changed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is changed; otherwise, <c>false</c>.
        /// </value>
        public bool IsChanged
        {
            get;
            set;
        }

        /// <summary>
        /// Reconciles the crosswalk from data context.
        /// </summary>
        public void ReconcileCrosswalkFromDataContext()
        {
            var prov = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            using (var session = prov.SessionFactory.OpenSession())
            {
                foreach (var model in DataContextObject.CrosswalkCache)
                {
                    var xwalk = MappedFieldModels.FirstOrDefault(field => string.Equals(field.Element.Name, model.ElementName, StringComparison.OrdinalIgnoreCase));
                    if (xwalk != null)
                    {
                        foreach (var xwalkModel in model.Mappings)
                        {
                            var scope = xwalk.CrosswalkModels.FirstOrDefault(x => Equals(x.Crosswalk.SourceValue, xwalkModel.Source));
                            if (scope != null)
                            {
                                var temp = (from thisScopeValue in session.Query<ScopeValue>()
                                    join thisScope in session.Query<Scope>() on thisScopeValue.Owner.Id equals thisScope.Id
                                    where thisScope.Id == xwalk.Element.Scope.Id
                                          && thisScopeValue.Value == xwalkModel.Value
                                    select thisScopeValue).FirstOrDefault();
                                scope.Crosswalk.ScopeValue = temp;
                            }
                        }
                    }
                }
            }
        }
    }
}