using Monahrq.Infrastructure.Entities.Domain.Wings;
using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.DataSets.Model
{
    /// <summary>
    /// The dataset wizard field mapping histogram.
    /// </summary>
    public class Histogram
    {
        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        private IDictionary<string, FieldEntry> Fields { get; set; }
        /// <summary>
        /// Gets or sets the column lookup.
        /// </summary>
        /// <value>
        /// The column lookup.
        /// </value>
        private List<string> ColumnLookup { get; set; }

        /// <summary>
        /// Gets the field entries.
        /// </summary>
        /// <value>
        /// The field entries.
        /// </value>
        public IEnumerable<FieldEntry> FieldEntries
        {
            get
            {
                foreach (var item in Fields.Values)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Histogram"/> class.
        /// </summary>
        /// <param name="columns">The columns.</param>
        public Histogram(IEnumerable<DataColumn> columns)
        {
            Fields = columns.ToDictionary(k => k.ColumnName, col => new FieldEntry(col));
            ColumnLookup = columns.OrderBy(col => col.Ordinal).Select(col => col.ColumnName).ToList();
        }

        /// <summary>
        /// Loads the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Load(object[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                var temp = data[i];
                temp = temp == DBNull.Value ? null : temp;
                Fields[ColumnLookup[i]].Bin.AddValue(temp);
            }
        }

        /// <summary>
        /// Gets the <see cref="FieldEntry"/> with the specified fieldname.
        /// </summary>
        /// <value>
        /// The <see cref="FieldEntry"/>.
        /// </value>
        /// <param name="fieldname">The fieldname.</param>
        /// <returns></returns>
        public FieldEntry this[string fieldname]
        {
            get
            {
                return Fields[fieldname];
            }
        }

        /// <summary>
        /// Gets the <see cref="FieldEntry"/> with the specified columnindex.
        /// </summary>
        /// <value>
        /// The <see cref="FieldEntry"/>.
        /// </value>
        /// <param name="columnindex">The columnindex.</param>
        /// <returns></returns>
        public FieldEntry this[int columnindex]
        {
            get
            {
                return Fields[ColumnLookup[columnindex]];
            }
        }

        /// <summary>
        /// Gets the field count.
        /// </summary>
        /// <value>
        /// The field count.
        /// </value>
        public int FieldCount
        {
            get { return Fields.Count; }
        }
    }

    /// <summary>
    /// The field entry entity.
    /// </summary>
    public class FieldEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldEntry"/> class.
        /// </summary>
        /// <param name="column">The column.</param>
        public FieldEntry(DataColumn column)
        {
            Column = column;
            Bin = new CrosswalkScopeBin(column);
        }

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <value>
        /// The column.
        /// </value>
        public DataColumn Column
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the bin.
        /// </summary>
        /// <value>
        /// The bin.
        /// </value>
        public CrosswalkScopeBin Bin { get; private set; }
    }

     

    /// <summary>
    /// The dataset import wizard crosswalk scope collection.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{Monahrq.DataSets.Model.CrosswalkScope}" />
    public class CrosswalkScopeBin : IEnumerable<CrosswalkScope>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrosswalkScopeBin"/> class.
        /// </summary>
        /// <param name="column">The column.</param>
        public CrosswalkScopeBin(DataColumn column)
        {
            Column = column;
        }

        /// <summary>
        /// The hash
        /// </summary>
        private Hashtable hash = new Hashtable();

        /// <summary>
        /// The null key
        /// </summary>
        public static readonly object NullKey = new Guid("{1C6F7F2B-F536-4BA5-9426-8FFA9E1901C0}");

        /// <summary>
        /// Gets the <see cref="CrosswalkScope"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="CrosswalkScope"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public CrosswalkScope this[object index]
        {
            get
            {
                var key = (object)(index ?? NullKey);
                var items = hash.Keys.OfType<object>().ToList();
                return hash[key] as CrosswalkScope;
            }
        }

        /// <summary>
        /// Adds the value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void AddValue(object value)
        {
            var key = value ?? NullKey;
            TotalValues += value == null ? 0 : 1;
            CrosswalkScope temp = hash[key] as CrosswalkScope;
            if (temp == null)
            {
                temp = (hash[key] = new CrosswalkScope(this, value)) as CrosswalkScope;
            }
            else
            {
                temp.Count++;
            }
        }

        /// <summary>
        /// Gets the total values.
        /// </summary>
        /// <value>
        /// The total values.
        /// </value>
        public int TotalValues
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<CrosswalkScope> GetEnumerator()
        {
            return hash.Values.OfType<CrosswalkScope>().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <value>
        /// The column.
        /// </value>
        public DataColumn Column { get; private set; }
    }

    /// <summary>
    /// The crosswalk scope.
    /// </summary>
    /// <seealso cref="System.IComparable{Monahrq.DataSets.Model.CrosswalkScope}" />
    [ImplementPropertyChanged]
    public class CrosswalkScope : IComparable<CrosswalkScope>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrosswalkScope"/> class.
        /// </summary>
        /// <param name="bin">The bin.</param>
        /// <param name="value">The value.</param>
        public CrosswalkScope(CrosswalkScopeBin bin, object value)
        {
            Bin = bin;
            SourceValue = value;
            Count = 1;
        }

        /// <summary>
        /// Gets the source value.
        /// </summary>
        /// <value>
        /// The source value.
        /// </value>
        public object SourceValue { get; private set; }
        /// <summary>
        /// Gets or sets the scope value.
        /// </summary>
        /// <value>
        /// The scope value.
        /// </value>
        public ScopeValue ScopeValue { get; set; }
        /// <summary>
        /// Gets or sets the value to import.
        /// </summary>
        /// <value>
        /// The value to import.
        /// </value>
        public object ValueToImport { get; set; }
        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get; set; }
        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        public int SortOrder
        {
            get;
            set;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="other" /> in the sort order.  Zero This instance occurs in the same position in the sort order as <paramref name="other" />. Greater than zero This instance follows <paramref name="other" /> in the sort order.
        /// </returns>
        public int CompareTo(CrosswalkScope other)
        {
            if (this.SourceValue == other.SourceValue) return 0;
            if (this.SourceValue == null) return -1;
            if (other.SourceValue == null) return 1;
            var temp = NumericCompareTo(other.SourceValue);
            if (temp.HasValue) return temp.Value;
            return this.SourceValue.ToString().CompareTo(other.SourceValue.ToString());
        }

        /// <summary>
        /// Numerics the compare to.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        private int? NumericCompareTo(object other)
        {
            double leftD, rightD;
            var leftIsNumeric = double.TryParse(SourceValue.ToString(), out leftD);
            var rightIsNumeric = double.TryParse(other.ToString(), out rightD);
            if (leftIsNumeric && rightIsNumeric) return leftD.CompareTo(rightD);
            if (leftIsNumeric) return -1;
            if (rightIsNumeric) return 1;
            return null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} represents {1} - {2}",
                        (SourceValue ?? "<<NULL>>").ToString() 
                        , ((ScopeValue == null? null: ScopeValue.Value) ?? "<<NULL>>").ToString()
                        , ((ScopeValue == null? null: ScopeValue.Name ) ?? "<<NULL>>").ToString());
        }

        /// <summary>
        /// Gets the bin.
        /// </summary>
        /// <value>
        /// The bin.
        /// </value>
        public CrosswalkScopeBin Bin { get; private set; }
    }
}
