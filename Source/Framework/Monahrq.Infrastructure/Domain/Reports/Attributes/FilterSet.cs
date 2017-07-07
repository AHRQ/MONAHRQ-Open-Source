using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Utility.Extensions;
using PropertyChanged;
using Monahrq.Infrastructure.Extensions;
using System.Collections;

namespace Monahrq.Infrastructure.Entities.Domain.Reports.Attributes
{

    public interface IEnumerationSetCollection<T> : ICollection, INotifyPropertyChanged
    {
        T Value { get; set; }
    }

    [ImplementPropertyChanged]
    public class EnumSelector<T> : INotifyPropertyChanged
    {
        public EnumSelector()
        {
        }

        static uint AsUint(object item)
        {
            var temp = Convert.ChangeType(item, typeof(uint));
            return (uint)temp;
        }

        private IDictionary<T, string> _selectorCaptions;
        public IDictionary<T, string> SelectorCaptions
        {
            get
            {
                if (_selectorCaptions == null)
                {
                    var values = Enum.GetValues(typeof(T)).OfType<object>().Select(item => AsUint(item)).ToArray();
                    uint filters = 0;
                    values.ToList().ForEach(filter => filters |= (uint)filter);
                    Enum result = Enum.ToObject(typeof(T), filters) as Enum;
                    var descriptions = result.GetAttributeValues<System.ComponentModel.DescriptionAttribute, string>(x => x.Description);
                    _selectorCaptions = descriptions.ToDictionary(tuple => (T)(object)tuple.Item1, value => value.Item2);
                }
                return _selectorCaptions;
            }
        }

        public EnumSelector(T enumeration)
        {
            var type = enumeration.GetType();
            if (!type.IsEnum) throw new ArgumentException("value must be an enumeration", "enumeration");
            this.Id = enumeration;
        }

        public T Id
        {
            get;
            private set;
        }

        public string Caption
        {
            get
            {
                string result = string.Empty;
                if (!SelectorCaptions.TryGetValue(Id, out result))
                {
                    result = Id.ToString();
                }
                return string.IsNullOrEmpty(result) ? Id.ToString() : result;
            }
        }

        bool _isSelected = true;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    RaiseValueChanged();
                }
            }
        }

        public virtual T Value
        {
            get
            {
                return IsSelected ? Id : default(T);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged(this, args);
        }

        protected void RaiseValueChanged()
        {
            OnPropertyChanged(new PropertyChangedEventArgs("Value"));
        }

        public override string ToString()
        {
            return this.Caption;
        }

    }

    [Serializable]
    [ImplementPropertyChanged]
    public class FilterItem : Entity<int>
    {
        private EnumSelector<ReportFilter> enumSelector;
        public FilterItem()
        {
        }

        public  FilterItem(ReportFilter filter, Action onValueChanged)
        {

                this.enumSelector = new EnumSelector<ReportFilter>(filter);
                this.ReportAttributeId = this.enumSelector.Id;


            this.PropertyChanged += (o, e) =>
            {
                if (string.Equals(e.PropertyName, "Value", StringComparison.OrdinalIgnoreCase))
                {
                    onValueChanged();
                }
            };
        }

        public ReportFilter ReportAttributeId
        {
            get;
            set;
        }

        private string _caption;
        public string Caption
        {
            get
            {
                if (this.enumSelector!=null && !this.enumSelector.SelectorCaptions.TryGetValue(ReportAttributeId, out _caption))
                {
                    _caption = ReportAttributeId.ToString();
                }
                _caption = string.IsNullOrEmpty(_caption) ? ReportAttributeId.ToString() : _caption;
                return _caption;
            }
            set
            {
                _caption = value;
            }
        }

        public bool IsSelected
        {
            get;
            set;
        }

        public bool IsEnabled { get; set; }

        private ReportFilter _value;
        public ReportFilter Value
        {
            get
            {
                _value = IsSelected ? ReportAttributeId : ReportAttributeId.ExtractGroup();
                return _value;
            }
            set
            {
                _value = value;
            }
        }
    }

    public class FilterSet : EnumSelector<ReportFilter>
    {

        public static FilterItem Create(ReportFilter filter, Action action)
        {
            return new FilterItem(filter, action);
        }

        internal FilterSet(ReportFilter temp)
            : base(temp.ExtractGroup())
        {
            var values = Id.GetValuesForGroup();
            Filters = new ObservableCollection<FilterItem>(values.Select(filter => Create(filter, RaiseValueChanged)));
        }

        public IEnumerable<FilterItem> Filters
        {
            get;
            private set;
        }

        public override ReportFilter Value
        {
            get
            {
                var result = default(ReportFilter);
                Filters.ToList().ForEach(filter =>
                {
                    result |= filter.Value;
                });
                return result;
            }
        }

        public bool IsVisible { get; set; }

    }

    [ImplementPropertyChanged]
    public class FilterSetCollection : ReadOnlyCollection<FilterSet>, IEnumerationSetCollection<ReportFilter>
    {
        static IEnumerable<FilterSet> InitialList
        {
            get
            {
                return new List<FilterSet>(ReportFilterHelper.FilterGroups.Select(filterGroup => new FilterSet(filterGroup)));
            }
        }

        public FilterSetCollection()
            : this(new Report() { Filter = ReportFilter.None })
        {

        }

        public FilterSetCollection(Report report): base(InitialList.ToList())
        {
            Value = report.Filter;
            this.SelectMany(set => set.Filters)
                .ToList()
                .ForEach(item => item.PropertyChanged += (o, e) =>
                {
                    item.IsEnabled = !report.IsDefaultReport;
                    if (string.Equals(e.PropertyName, "Value", StringComparison.OrdinalIgnoreCase))
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                    }
                });

            // Determine if the filter items herein are part of this report 

            foreach (var o in this.Items)
            {
                string reportFilter = o.Id.ToString();
                var reportAttributeNames = report.SourceTemplate.ReportAttributes.ToString().Split(',', ' ');

                if (reportAttributeNames.Any(r => r.ToLower().Trim() == reportFilter.ToLower()))
                {
                    o.IsVisible = true;
                }
            }

            //report.FilterItems = this.SelectMany(set => set.Filters).ToList();
        }

        public ReportFilter Value
        {
            get
            {
                var result = default(ReportFilter);
                this.ToList().ForEach(filter => result |= filter.Value);
                return result;
            }
            set
            {
                this.SelectMany(set => set.Filters)
                   .ToList()
                   .ForEach(filter =>
                       {
                           filter.IsSelected = (value & filter.ReportAttributeId) == filter.ReportAttributeId;
                       });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public override string ToString()
        {
            return string.Join(", ", this.SelectMany(item => item.Filters).Where(item => item.IsSelected).Select(item => item.Caption));
        }
    }

    public abstract class SetCollectionBase<T> : ReadOnlyCollection<EnumSelector<T>>, IEnumerationSetCollection<T>
    {
        public static T AllValues
        {
            get
            {
                var result = EnumExtensions.AllFlags<T>();
                return result;
            }
        }

        public static IList<EnumSelector<T>> InitialSelectors
        {
            get
            {
                var None = default(T);
                var result = new List<EnumSelector<T>>();
                var items = Enum.GetValues(typeof(T)).OfType<T>()
                        .Where(item => !object.Equals(item, None))
                        .Select(item => new EnumSelector<T>(item))
                        .OrderBy(item => item.Caption).ToList();
                return items;
            }
        }

        public SetCollectionBase()
            : this(AllValues)
        {

        }

        public SetCollectionBase(T initalValues)
            : base(InitialSelectors)
        {
            Value = SpecificInitialItems(initalValues);
            this.ToList()
                .ForEach(item => item.PropertyChanged += (o, e) =>
                {
                    if (string.Equals(e.PropertyName, "Value", StringComparison.OrdinalIgnoreCase))
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                    }
                });

        }

        static bool CalculateSelected(T incoming, T test)
        {
            var incomingInt = (int)Convert.ChangeType(incoming, typeof(int));
            var testInt = (int)Convert.ChangeType(test, typeof(int));
            return (incomingInt & testInt) == testInt;
        }

        public T Value
        {
            get
            {
                var result = (int)Convert.ChangeType(default(T), typeof(int));
                //this.Select(item => item.Value).Cast<int>().ToList().ForEach(item => result |= item);
                var temp = (T)Enum.ToObject(typeof(T), result);
                return temp;
            }
            set
            {
                this.ToList()
                   .ForEach(item =>
                   {
                       item.IsSelected = CalculateSelected(value, item.Id);
                   });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual T SpecificInitialItems(T source)
        {
            return source;
        }

        public override string ToString()
        {
            return string.Join(", ", this.Where(item => item.IsSelected).Select(item => item.Caption));
        }
    }

    [ImplementPropertyChanged]
    public class SetCollection<T, TRules> : SetCollectionBase<T> where TRules : SetCollectionRuleSet<T>, new()
    {

        TRules RuleSet { get; set; }
        static SetCollection()
        {

        }

        static void Combine(ref int temp, int val)
        {

        }

        public SetCollection()
            : this(AllValues)
        {

        }

        public SetCollection(T initialValues)
            : base(initialValues)
        {
            RuleSet = new TRules();
            RuleSet.Collection = this;
        }

    }

    public class SetCollection<T> : SetCollection<T, SetCollectionRuleSet<T>>
    {
        public SetCollection() : base() { }
        public SetCollection(T initalValues) : base(initalValues) { }
    }

    [ImplementPropertyChanged]
    public class AudienceSetCollection : SetCollection<Audience>,
        IEnumerationSetCollection<Audience>
    {
        public AudienceSetCollection(Audience audience) : base(audience) { }
        public AudienceSetCollection() : base() { }
    }

    [ImplementPropertyChanged]
    public class ReportProfileSetCollection : SetCollection<ReportProfileDisplayItem, ReportProfileSetCollectionRuleSet>
    {
        protected override ReportProfileDisplayItem SpecificInitialItems(ReportProfileDisplayItem source)
        {
            return source ;
        }

        public ReportProfileSetCollection(ReportProfileDisplayItem profile) : base(profile) { }
        public ReportProfileSetCollection() : base() { }
    }

    public class ReportProfileSetCollectionRuleSet : SetCollectionRuleSet<ReportProfileDisplayItem>
    {

        protected override void WireCollection()
        {
            var medicare = Collection.First(item => item.Id == ReportProfileDisplayItem.CostToChargeMedicare);
            var allPatients = Collection.First(item => item.Id == ReportProfileDisplayItem.CostToChargeAllPatients);
            medicare.PropertyChanged += delegate
            {
                if ((Collection.Value & ReportProfileDisplayItem.CostToChargeMedicare)
                        == ReportProfileDisplayItem.CostToChargeMedicare)
                {
                    Collection.Value &= ~ReportProfileDisplayItem.CostToChargeAllPatients;
                }
            };

            allPatients.PropertyChanged += delegate
            {
                if ((Collection.Value & ReportProfileDisplayItem.CostToChargeAllPatients)
                        == ReportProfileDisplayItem.CostToChargeAllPatients)
                {
                    Collection.Value &= ~ReportProfileDisplayItem.CostToChargeMedicare;
                }
            };

        }
    }

    public class SetCollectionRuleSet<T>
    {
        private SetCollectionBase<T> _collection;

        public SetCollectionBase<T> Collection
        {
            get { return _collection; }
            set
            {
                _collection = value;
                WireCollection();
            }
        }

        protected virtual void WireCollection()
        {

        }

    }
}
