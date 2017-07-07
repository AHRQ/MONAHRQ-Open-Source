using Monahrq.Sdk.Services.Contracts;
using Monahrq.Infrastructure.Core.Attributes;

[assembly: WingAssembly("{A8FD3110-6657-41BE-8570-1B954271CB12}", "Hospital Compare Data", "Hospital Compare Data")] 

namespace Monahrq.Wing.HospitalCompare
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Monahrq.Sdk.Services.Contracts.DatasetWing{Monahrq.Wing.HospitalCompare.HospitalCompareTarget}" />
	[DatasetWingExport]
    public class HospitalCompareDatasetWing : DatasetWing<HospitalCompareTarget>
    {}
}
