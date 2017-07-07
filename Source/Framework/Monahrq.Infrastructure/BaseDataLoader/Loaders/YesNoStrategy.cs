using System;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Core.Import;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="YesNo"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataEnumImporter{Monahrq.Infrastructure.Domain.BaseData.YesNo, System.Int32, Monahrq.Infrastructure.Domain.Wings.YesNoEnum}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class YesNoStrategy : BaseDataEnumImporter<YesNo, int, Domain.Wings.YesNoEnum>
    {
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(YesNo));
            var version = typeof(MonahrqContext).Assembly.GetName().Version;
            VersionStrategy.Filename = string.Format("YesNo-{0}-{1}-{2}", version.Major, version.Minor, version.Revision);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public override YesNo GetEntity(object val)
        {
            var payer = new YesNo
            {
                Id = (int)val,
                Name = Enum.GetName(typeof(Domain.Wings.YesNoEnum), val),
                Value = (int)val
            };

            return payer.Name == "Exclude" || payer.Name == "Retain" ? null : payer;
        }
    }
}
