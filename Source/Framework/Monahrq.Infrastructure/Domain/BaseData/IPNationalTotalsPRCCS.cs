using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class IPNationalTotalsPRCCS : Entity<int>
    {
        public virtual int PRCCSID { get; set; }
        public virtual int Region { get; set; }
        public virtual float Discharges { get; set; }
        public virtual float DischargesStdErr { get; set; }
        public virtual float MeanCharges { get; set; }
        public virtual float MeanChargesStdErr { get; set; }
        public virtual float MeanCost { get; set; }
        public virtual float MeanCostStdErr { get; set; }
        public virtual float MeanLOS { get; set; }
        public virtual float MeanLOSStdErr { get; set; }
        public virtual float MedianCharges { get; set; }
        public virtual float MedianCost { get; set; }
        public virtual float MedianLOS { get; set; }
    }
}

