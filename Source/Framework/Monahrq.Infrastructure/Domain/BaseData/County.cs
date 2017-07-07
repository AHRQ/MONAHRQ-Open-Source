using System;
using System.Data;
using Monahrq.Infrastructure.Data.Conventions;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged,Serializable]
    [EntityTableName("Base_Counties")]
    public class County : Entity<int>
    {
        public virtual string CountyFIPS { get; set; }
        public virtual string CountySSA { get; set; }
        public virtual string State { get; set; }

        public virtual string CoutyFIPSDisplay
        {
            get
            {
                var countyFIPSDisplay = CountyFIPS;
                if (!string.IsNullOrEmpty(Name))
                {
                    countyFIPSDisplay += " - " + Name;
                }
                return countyFIPSDisplay;
            }
        }

        public virtual string CountyFIPSDisplayWithState
        {
            get
            {
                var countyFIPSDisplay = CoutyFIPSDisplay;
                if (State != null)
                {
                    countyFIPSDisplay += " (" + State + ")";
                }
                return countyFIPSDisplay;
            }
        }

        public virtual string CountySSADisplay {
            get {
                var countySSADisplay = CountySSA;
                if (!string.IsNullOrEmpty(Name)) countySSADisplay += " - " + Name;
                else countySSADisplay += "    - " + Name;
                
                return countySSADisplay;
            }
            //set {
            //    var tokens = value.Split(new string[] {" - "}, StringSplitOptions.None);
            //    if (tokens.Length > 0) this.CountySSA = tokens[0];
            //    if (tokens.Length > 1) this.Name = tokens[1];
            //}
        }

        public virtual string CountySSADisplayWithState {
            get {
                var countySSADisplay = CountySSADisplay;
                if (State != null) {
                    countySSADisplay += " (" + State + ")";
                }
                return countySSADisplay;
            }
        }

        public override IBulkMapper CreateBulkInsertMapper<T>(DataTable dataTable, T instance = default(T), Entities.Domain.Wings.Target target = null)
        {
            return new CountyBulkInsertMapper<T>(dataTable);
        }
    }

    public class CountyBulkInsertMapper<T> : BulkMapper<T> where T : class
    {
        public CountyBulkInsertMapper(DataTable dt) : base(dt)
        {
        }

        protected override void ApplyTypeSpecificColumnNameLookup()
        {
            Lookup["State"] =
                t => (t as County).State == null
                    ? null
                    : (object) (t as County).State;
        }
    }
}
