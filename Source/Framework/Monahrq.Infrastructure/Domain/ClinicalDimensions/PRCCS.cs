using System;
using Monahrq.Infrastructure.Data.Conventions;

namespace Monahrq.Infrastructure.Domain.ClinicalDimensions
{
    [Serializable, EntityTableName("Base_PRCCSs")]
    public class PRCCS : ClinicalDimension
    {
        public virtual int PRCCSID { get; set; }
    }
}
