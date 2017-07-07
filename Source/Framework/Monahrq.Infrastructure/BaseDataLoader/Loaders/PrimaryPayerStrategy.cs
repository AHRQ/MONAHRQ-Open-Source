using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System;
using System.ComponentModel.Composition;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="Payer"/> (primary payer) base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataEnumImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.Payer, System.Int32, Monahrq.Infrastructure.Domain.Wings.PrimaryPayer}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
    public class PrimaryPayerStrategy : BaseDataEnumImporter<Payer, int, Domain.Wings.PrimaryPayer>
    {
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(Payer));
            var version = typeof(MonahrqContext).Assembly.GetName().Version;
            VersionStrategy.Filename = string.Format("Payer-{0}-{1}-{2}", version.Major, version.Minor, version.Revision);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public override Payer GetEntity(object val)
        {
            var payer = new Payer
            {
                Id = (int) val, 
                Name = Enum.GetName(typeof (Domain.Wings.PrimaryPayer), val),
                Value = (int)val
            };

            return payer.Name == "Exclude" || payer.Name == "Retain" ? null : payer;
        }
    }
}
