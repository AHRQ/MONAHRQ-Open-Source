using System;
using System.Collections;
using Monahrq.Infrastructure.Data.Conventions;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.Wings
{
    /// <summary>
    /// Used to persist information about a CLR enum value to the Monahrq database
    /// </summary>
    /// <seealso cref="Scope"/>
    [Serializable, ImplementPropertyChanged, EntityTableName("Wings_ScopeValues")]
    public  class ScopeValue : ScopedOwnedWingItem<int>, IComparable
    {
        protected ScopeValue()
        { }

        public ScopeValue(Scope scope, string name): base(scope, name)
        {
            this.Owner.Values.Add(this);
        }

        #region Properties
        public static ScopeValue Null
        {
            get
            {
                return new ScopeValue(Scope.Null, "<<NULL>>") { Value = null, Description = "<<NULL>>" };
            }
        }

        public virtual object Value { get; set; }
        #endregion

        #region Methods
        // IsNumeric Function
        static bool IsNumeric(object expression)
        {
            // Variable to collect the Return value of the TryParse method.
            bool isNum;

            // Define variable to collect out parameter of the TryParse method. If the conversion fails, the out parameter is zero.
            double retNum;

            // The TryParse method converts a string in a specified style and culture-specific format to its double-precision floating point number equivalent.
            // The TryParse method does not generate an exception if the conversion fails. If the conversion passes, True is returned. If it does not, False is returned.
            isNum = Double.TryParse(Convert.ToString(expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

        public override string ToString()
        {
            if(IsNumeric(this.Value) && Convert.ToDouble(this.Value) < 0)
            {
                return string.Format("<< {0} >>", this.Description); 
            }
            var temp = (this.Description ?? "<<NULL>>").Trim();
            temp = string.IsNullOrEmpty(temp) ? "<<EMPTY>>" : temp;
            return string.Format("{0} - {1}", (this.Value ?? "<<NULL>>"), temp);
        }

        public int CompareTo(object obj)
        {
            var other = obj as ScopeValue;
            return other == null ? 1
                : other.Value == null ? 1
                    : this.Value == null ? -1
                        : Comparer.Default.Compare(this.Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            var right = obj as ScopeValue;
            return this.Equals(right);
        }

        public bool Equals(ScopeValue right)
        {
            if (right == null) return false;
            return Equals(this.Value, right.Value);
        }

        public override int GetHashCode()
        {
            return (this.Value == null) ?
                new Guid("{9903C3BF-E35E-4591-AAA3-66BF7A4D48FA}").GetHashCode()
                : this.Value.GetHashCode();
        }

        public static bool operator ==(ScopeValue a, ScopeValue b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if ((a as object) == null || (b as object) == null)
            {
                return false;
            }

            // Return true if the fields match:
            return a.Equals(b);
        }

        public static bool operator !=(ScopeValue a, ScopeValue b)
        {
            return !(a == b);
        }
        #endregion
    }
}