using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Data;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.ServiceLocation;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using NHibernate;
using NHibernate.Linq;
using PropertyChanged;

namespace Monahrq.DataSets.ViewModels.Crosswalk
{
    /// <summary>
    /// The mapped field entry view model.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="Monahrq.DataSets.ViewModels.Crosswalk.IMappedFieldEntryViewModel" />
    [ImplementPropertyChanged]
    public class MappedFieldEntryViewModel : INotifyPropertyChanged, IMappedFieldEntryViewModel
    {
        /// <summary>
        /// Gets the session factory.
        /// </summary>
        /// <value>
        /// The session factory.
        /// </value>
        ISessionFactory SessionFactory
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>().SessionFactory;
            }
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        ILoggerFacade Logger
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ILoggerFacade>();
            }
        }

        /// <summary>
        /// Gets the field entry.
        /// </summary>
        /// <value>
        /// The field entry.
        /// </value>
        public FieldEntry FieldEntry
        {
            get;
            private set;
        }

        /// <summary>
        /// The internal element
        /// </summary>
        private Element _internalElement;

        /// <summary>
        /// Gets the total entries.
        /// </summary>
        /// <value>
        /// The total entries.
        /// </value>
        public int TotalEntries { get { return CrosswalkModels != null ? CrosswalkModels.Count() : 0; } }

        /// <summary>
        /// Gets the valid entries.
        /// </summary>
        /// <value>
        /// The valid entries.
        /// </value>
        public int ValidEntries { get { return CrosswalkModels != null ? CrosswalkModels.Count(i => i.IsValid) : 0; } }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName
        {
            get
            {
                return Element.Description;
            }
        }

        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <value>
        /// The element.
        /// </value>
        public Element Element
        {
            get { return _internalElement; }

            private set
            {
                _internalElement = value;

                var temp = _internalElement.Scope.Values.OrderBy(val => val.Value)
                                                        .Select(scopeValue =>
                                    {
                                        var key = scopeValue;
                                        var val = scopeValue.ToString();
                                        return new KeyValuePair<ScopeValue, string>(key, val);
                                    });

                ElementScopeValues = CollectionViewSource.GetDefaultView(new ObservableCollection<KeyValuePair<ScopeValue, string>>(temp)) as ListCollectionView;
            }
        }

        /// <summary>
        /// Gets or sets the element scope values.
        /// </summary>
        /// <value>
        /// The element scope values.
        /// </value>
        public ListCollectionView ElementScopeValues
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappedFieldEntryViewModel"/> class.
        /// </summary>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="element">The element.</param>
        /// <param name="entry">The entry.</param>
        public MappedFieldEntryViewModel(PropertyInfo targetProperty, Element element, FieldEntry entry)
        {
            var targetPropertyType = targetProperty.PropertyType;
            if (targetPropertyType.IsGenericType)
            {
                targetPropertyType = targetPropertyType.GetGenericArguments()[0];
            }
            //PropertyChanged += (o, e) => Logger.Log(string.Format("Property changed:  {0}", e.PropertyName), Category.Info, Priority.Low);
            FieldEntry = entry;
            Element = element;
            var xwalk = FieldEntry.Bin
               .Select(item =>
                   {

                       var model = new CrosswalkViewModel(item);
                       var value = model.Crosswalk.SourceValue;
                       var found = item.Bin[value];
                       if (found.ScopeValue == null)
                       {
                           GuessScopeForFound(targetPropertyType, found);
                       }
                       model.Crosswalk.ScopeValue = found == null ? null : found.ScopeValue;
                       model.CandidateScopes = ElementScopeValues;
                       (model.Crosswalk as INotifyPropertyChanged).PropertyChanged += (o, e) => OnPropertyChanged();

                       (model as INotifyPropertyChanged).PropertyChanged += (o, e) => OnPropertyChanged();
                       OnPropertyChanged();
                       return model;
                   })
               .OrderBy(item => item.Crosswalk);

            var temp = new ObservableCollection<CrosswalkViewModel>(xwalk);
            Crosswalks = CollectionViewSource.GetDefaultView(temp) as ListCollectionView;
            CountTotalCrosswalks = new Lazy<int>(() => CrosswalkModels.Count());
        }

        /// <summary>
        /// Guesses the scope for found.
        /// </summary>
        /// <param name="targetPropertyType">Type of the target property.</param>
        /// <param name="found">The found.</param>
        private void GuessScopeForFound(Type targetPropertyType, CrosswalkScope found)
        {
            var tempObj = found.SourceValue ?? 0;
            var temp = ElementMappingModel.GuardValueOnType(tempObj, targetPropertyType);
            ScopeValue result;

            if (temp == null)
            {
                result = default(ScopeValue);
            }
            else
            {
               // IList<ScopeValue> scopeValues = new List<ScopeValue>();
                using (var sess = SessionFactory.OpenSession())
                {
                    result = sess.Query<ScopeValue>()
                                 .Where(value => value.Owner.Id == Element.Scope.Id)
                                 .ToList()
                                 .FirstOrDefault(v =>
                                     {
                                         var test = targetPropertyType.IsEnum
                                                        ? Enum.ToObject(targetPropertyType, v.Value)
                                                        : v.Value;
                                         var areEq = Equals(test, temp);
                                         return areEq;
                                     });
                }
            }

            found.ScopeValue = result;
        }

        /// <summary>
        /// Gets the crosswalk models.
        /// </summary>
        /// <value>
        /// The crosswalk models.
        /// </value>
        public IEnumerable<ICrosswalkViewModel> CrosswalkModels
        {
            get
            {
                return Crosswalks.OfType<CrosswalkViewModel>();
            }
        }

        /// <summary>
        /// Gets the crosswalks.
        /// </summary>
        /// <value>
        /// The crosswalks.
        /// </value>
        public ListCollectionView Crosswalks { get; private set; }

        /// <summary>
        /// Gets the count mapped crosswalks.
        /// </summary>
        /// <value>
        /// The count mapped crosswalks.
        /// </value>
        public int CountMappedCrosswalks
        {
            get
            {
                return CrosswalkModels.Count(item => item.Crosswalk.ScopeValue != null && item.Crosswalk.ScopeValue != ScopeValue.Null);
            }
        }

        /// <summary>
        /// Gets the count unmapped crosswalks.
        /// </summary>
        /// <value>
        /// The count unmapped crosswalks.
        /// </value>
        public int CountUnmappedCrosswalks
        {
            get
            {
                return CrosswalkModels.Count(item => item.Crosswalk.ScopeValue == null || item.Crosswalk.ScopeValue == ScopeValue.Null);
            }
        }

        /// <summary>
        /// Gets the count total crosswalks.
        /// </summary>
        /// <value>
        /// The count total crosswalks.
        /// </value>
        public Lazy<int> CountTotalCrosswalks { get; private set; }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                var msg = string.Format("Calculating MappedFieldEntryViewModel Validity - Target Element: {0}", Element.Description);
                Debug.WriteLine(msg);
                var isValid = TotalEntries == ValidEntries;  //CrosswalkModels.Count(model => !model.IsValid) == 0;
                Debug.WriteLine("Target Element {0}: {1}", Element.Description, isValid ? "Valid" : "Invalid");
                return isValid;
            }
        }

        /// <summary>
        /// Gets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        public ListCollectionView Scope { get; private set; }

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
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged(this, e);
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="prop">The property.</param>
        protected virtual void OnPropertyChanged(string prop)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(prop));
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        protected virtual void OnPropertyChanged()
        {
            OnPropertyChanged(string.Empty);
        }

    }

    /// <summary>
    /// The source value formatter class.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class SourceValueFormatter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var temp = (value ?? "<<NULL>>").ToString();
            return (string.IsNullOrEmpty(temp) ? "<<EMPTY>>" : temp).Trim();
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
