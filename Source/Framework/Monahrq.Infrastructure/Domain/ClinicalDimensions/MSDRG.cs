using System;
using Monahrq.Infrastructure.Data.Conventions;

namespace Monahrq.Infrastructure.Domain.ClinicalDimensions
{
    [Serializable, EntityTableName("Base_MSDRGs")]
    public class MSDRG : ClinicalDimension
    {
        public virtual int MSDRGID { get; set; }
        public virtual int MDCID { get; set; }
    }
}
