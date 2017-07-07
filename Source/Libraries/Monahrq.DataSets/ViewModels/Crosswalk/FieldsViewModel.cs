using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Model;
using Monahrq.Theme.Controls.Wizard.Models;
using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using NHibernate.Linq;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.DataSets.ViewModels.Crosswalk
{
    /// <summary>
    /// The fields view model. Used in the dataset wizard crosswalk mapping step.
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{Monahrq.DataSets.Model.DatasetContext}" />
    [ImplementPropertyChanged]
    public class FieldsViewModel : WizardStepViewModelBase<DatasetContext>
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
        /// Initializes a new instance of the <see cref="FieldsViewModel"/> class.
        /// </summary>
        public FieldsViewModel()
            : this(new DatasetContext())
        {
            _mappedFieldEntries = MakeView(Enumerable.Empty<MappedFieldEntryViewModel>());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldsViewModel"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public FieldsViewModel(DatasetContext context)
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
        /// Returns true if ... is valid.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            var models = MappedFieldEntries != null ? MappedFieldEntries.OfType<MappedFieldEntryViewModel>().ToList() : null;
            ValidCount = models != null ? models.Count(x => x.IsValid) : 0;
            TotalCount = models != null ? models.Count() : 0;
            ModelIsValid = ValidCount == TotalCount;
            return ModelIsValid;

        }

        /// <summary>
        /// Gets or sets a value indicating whether [model is valid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [model is valid]; otherwise, <c>false</c>.
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

            foreach (var model in items.OfType<INotifyPropertyChanged>())
            {
                model.PropertyChanged -= handler;
                model.PropertyChanged += handler;
            };

            var col = new ObservableCollection<MappedFieldEntryViewModel>(viewItems);
            col.CollectionChanged += (s, e) => OnPropertyChanged();
            return (ListCollectionView)CollectionViewSource.GetDefaultView(col);
        }

        /// <summary>
        /// this is the model
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
        /// <value>
        /// The mapped field entries.
        /// </value>
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
            SourceFields = (ListCollectionView)CollectionViewSource.GetDefaultView(new ObservableCollection<string>(entries));

            ElementFilterChangedCommand = new DelegateCommand(() =>
            {
                MappedFieldEntries.Filter = o => ElementEntryMatchesFilter(o, CurrentElementFilter);
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
                var LeftPred = ElementPredicate ?? new Predicate<object>((o) => true);
                var RightPred = FieldEntryPredicate ?? new Predicate<object>((o) => true);
                return new Predicate<object>((o) =>
                {
                    var temp = (bool)LeftPred(o);
                    
                    return temp;
                });
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
        /// Elements the entry matches filter.
        /// </summary>
        /// <param name="entryObj">The entry object.</param>
        /// <param name="txt">The text.</param>
        /// <returns></returns>
        bool ElementEntryMatchesFilter(object entryObj, string txt)
        {
            if (string.IsNullOrEmpty(txt)) return true;
            var entry = entryObj as MappedFieldEntryViewModel;
            if (entry == null) return false;

            return entry.Element.Description.ToUpper().Contains(txt.ToUpper());
        }

        /// <summary>
        /// Fields the entry matches filter.
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
        /// For when yous need to save some values that can't be directly bound to UI elements.
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
                    var xwalk = this.MappedFieldModels.FirstOrDefault(field => string.Equals(field.Element.Name, model.ElementName, StringComparison.OrdinalIgnoreCase));
                    if (xwalk != null)
                    {
                        foreach (var xwalkModel in model.Mappings)
                        {
                            var scope = xwalk.CrosswalkModels.FirstOrDefault(x => object.Equals(x.Crosswalk.SourceValue, xwalkModel.Source));
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

    /// <summary>
    /// The data import wizard data crosswalk data objectcontext extension methods.
    /// </summary>
    static class XwalkDataObjectContextExtension
    {
        /// <summary>
        /// Creates the crosswalk mapping field view models.
        /// </summary>
        /// <param name="ctxt">The CTXT.</param>
        /// <returns></returns>
        public static IEnumerable<MappedFieldEntryViewModel> CreateCrosswalkMappingFieldViewModels(this DatasetContext ctxt)
        {
            if (ctxt.Histogram == null) return Enumerable.Empty<MappedFieldEntryViewModel>();
            var histogram = ctxt.Histogram;
            return ctxt
                .RequiredMappings.Concat(ctxt.OptionalMappings)
                .Select(
                    (kvp) =>
                    {
                        if (string.IsNullOrEmpty(kvp.Key))
                        {
                            return null;
                        }
                        if(string.IsNullOrEmpty(kvp.Value))
                        {
                            return null;
                        }

                        var findByName = ctxt.TargetElements
                                .Where(elem => elem.Name == kvp.Key);
                        var element = findByName.FirstOrDefault(elem => elem.Scope != null);
                        var field = histogram[kvp.Value];
                        if (element == null || field == null)
                        {
                            return null;
                        }
                        var model = new MappedFieldEntryViewModel(ctxt.TargetProperties[element.Name], element, field);
                        return model;
                    }).Where(item => item != null);
        }
    }
}
