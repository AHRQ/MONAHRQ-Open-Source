using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{
    [ImplementPropertyChanged]
    public class IPNationalTotalsDRG : Entity<int>
    {
        public virtual int DRGID { get; set; }
        public virtual int Region { get; set; }
        public virtual int Discharges { get; set; }
        public virtual double DischargesStdErr { get; set; }
        public virtual double MeanCharges { get; set; }
        public virtual double MeanChargesStdErr { get; set; }
        public virtual double MeanCost { get; set; }
        public virtual double MeanCostStdErr { get; set; }
        public virtual float MeanLOS { get; set; }
        public virtual float MeanLOSStdErr { get; set; }
        public virtual float MedianCharges { get; set; }
        public virtual float MedianCost { get; set; }
        public virtual float MedianLOS { get; set; }
    }
}

