using Microsoft.Practices.Prism.Regions;

namespace Monahrq.Infrastructure.Modules
{
    /// <summary>
    /// The module regions registrar inteface/contract.
    /// </summary>
    public interface IModuleRegionRegistrar
    {
        /// <summary>
        /// Registers the regions.
        /// </summary>
        /// <param name="regionManager">The region manager.</param>
        void RegisterRegions(IRegionManager regionManager);
    }
}
