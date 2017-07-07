using System;
using Monahrq.Infrastructure.Data.Conventions;

namespace Monahrq.Infrastructure.Domain.ClinicalDimensions
{
    [Serializable, EntityTableName("Base_DRGs")]
    public class DRG : ClinicalDimension
    {
        public virtual int DRGID { get; set; }
        public virtual int MDCID { get; set; }
    }
}
