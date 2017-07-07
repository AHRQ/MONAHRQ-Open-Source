using System;
using Monahrq.Infrastructure.Data.Conventions;

namespace Monahrq.Infrastructure.Domain.ClinicalDimensions
{
    [Serializable, EntityTableName("Base_MDCs")]
    public class MDC : ClinicalDimension
    {
        public virtual int MDCID { get; set; }
    }
}
