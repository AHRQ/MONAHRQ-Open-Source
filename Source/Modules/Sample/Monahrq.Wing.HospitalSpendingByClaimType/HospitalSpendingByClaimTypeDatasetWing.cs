using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Sdk.Services.Contracts;

namespace Monahrq.Wing.HospitalSpendingByClaimType
{
    /// <summary>
    /// Describes a single Dataset Wing
    /// </summary>
    /// <remarks>
    /// Nothing needs to be defined in this class because the two metadata properties required by <see cref="IDatasetWing"/> 
    /// are provided by the base class <see cref="DatasetWing{T}"/> and populated by the <see cref="WingTargetAttribute"/>
    /// that decorates <see cref="HospitalSpendingByClaimTypeTarget"/>.
    /// </remarks>
    [DatasetWingExport]
    public class HospitalSpendingByClaimTypeDatasetWing : DatasetWing<HospitalSpendingByClaimTypeTarget>
    {
    }
}
