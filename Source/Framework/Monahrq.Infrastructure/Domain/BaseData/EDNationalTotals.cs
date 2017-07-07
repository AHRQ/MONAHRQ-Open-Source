using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;
using System;

namespace Monahrq.Infrastructure.Domain.BaseData
{
    [ImplementPropertyChanged]
    [Serializable, EntityTableName("Base_EDNationalTotals")]
    public class EDNationalTotals : Entity<int>
    {
        public virtual int CCSID { get; set; }
        public virtual int NumEdVisits { get; set; }
        public virtual int NumEdVisitsStdErr { get; set; }
        public virtual int NumAdmitHosp { get; set; }
        public virtual int NumAdmitHospStdErr { get; set; }
        public virtual int DiedEd { get; set; }
        public virtual int DiedEdStdErr { get; set; }
        public virtual int DiedHosp { get; set; }
        public virtual int DiedHospStdErr { get; set; }
    }
}
