using System;
using Monahrq.Infrastructure.Data.Conventions;

namespace Monahrq.Infrastructure.Domain.ClinicalDimensions
{
    [Serializable, EntityTableName("Base_DXCCSs")]
    public class DXCCS : ClinicalDimension
    {
        public virtual int DXCCSID { get; set; }
    }
}
