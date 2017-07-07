using System;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="HospitalTraumaLevel"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataEnumImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.HospitalTraumaLevel, System.Int32, Monahrq.Infrastructure.Domain.Wings.HospitalTraumaLevel}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
    public class HospitalTraumaLevelStrategy : BaseDataEnumImporter<HospitalTraumaLevel, int, Domain.Wings.HospitalTraumaLevel>
    {
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(HospitalTraumaLevel));
            var version = typeof(MonahrqContext).Assembly.GetName().Version;
            VersionStrategy.Filename = string.Format("HospitalTraumaLevel-{0}-{1}-{2}", version.Major, version.Minor, version.Revision);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public override HospitalTraumaLevel GetEntity(object val)
        {
            var hospitalTraumaLevel = new HospitalTraumaLevel
            {
                Id = (int) val,
                Name = Enum.GetName(typeof (Domain.Wings.HospitalTraumaLevel), val),
                Value = (int) val
            };

            return hospitalTraumaLevel.Name == "Exclude" || hospitalTraumaLevel.Name == "Retain"
                ? null
                : hospitalTraumaLevel;
        }
    }
}
