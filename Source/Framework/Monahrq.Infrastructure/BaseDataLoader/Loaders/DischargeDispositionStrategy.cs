using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System;
using System.ComponentModel.Composition;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="DispositionCode"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataEnumImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.DispositionCode, System.Int32, Monahrq.Infrastructure.Domain.Wings.DischargeDisposition}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
    public class DischargeDispositionStrategy : BaseDataEnumImporter<DispositionCode, int, DischargeDisposition>
    {
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(DispositionCode));
            var version = typeof(MonahrqContext).Assembly.GetName().Version;
            VersionStrategy.Filename = string.Format("DischargeDisposition-{0}-{1}-{2}", version.Major, version.Minor, version.Revision);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public override DispositionCode GetEntity(object val)
        {
            var dischargeDisposition = new DispositionCode
            {
                Id = (int) val,
                Name = Enum.GetName(typeof (DischargeDisposition), val),
                Value = (int) val
            };
            return dischargeDisposition.Name == "Exclude" ? null : dischargeDisposition;
        }
    }
}
