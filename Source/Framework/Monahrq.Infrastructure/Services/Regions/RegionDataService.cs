using System.ComponentModel.Composition;

namespace Monahrq.Infrastructure.Services.Regions
{
    /// <summary>
    /// The hospital regions data domain service interface/contract.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Services.IDataServiceBase" />
    public interface IRegionDataService : IDataServiceBase
    {}

    /// <summary>
    /// The hospital regions data domain service
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Services.DataServiceBase" />
    /// <seealso cref="Monahrq.Infrastructure.Services.Regions.IRegionDataService" />
    [Export(typeof(IRegionDataService))]
    public class RegionDataService : DataServiceBase, IRegionDataService
    {}
}
