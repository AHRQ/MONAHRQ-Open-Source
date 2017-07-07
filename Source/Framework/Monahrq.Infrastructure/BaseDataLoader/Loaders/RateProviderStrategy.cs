using System;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Core.Import;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="RateProvider"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataEnumImporter{Monahrq.Infrastructure.Domain.BaseData.RateProvider, System.Int32, Monahrq.Infrastructure.Domain.Wings.RateProviderEnum}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class RateProviderStrategy : BaseDataEnumImporter<RateProvider, int, Domain.Wings.RateProviderEnum>
    {
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(RateProvider));
            var version = typeof(MonahrqContext).Assembly.GetName().Version;
            VersionStrategy.Filename = string.Format("RateProvider-{0}-{1}-{2}", version.Major, version.Minor, version.Revision);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public override RateProvider GetEntity(object val)
        {
            var payer = new RateProvider
            {
                Id = (int)val,
                Name = Enum.GetName(typeof(Domain.Wings.RateProviderEnum), val),
                Value = (int)val
            };

            return payer.Name == "Exclude" || payer.Name == "Retain" ? null : payer;
        }
    }
}