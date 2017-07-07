using System;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Core.Import;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="Gender"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataEnumImporter{Monahrq.Infrastructure.Domain.BaseData.Gender, System.Int32, Monahrq.Infrastructure.Domain.Wings.GenderEnum}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class GenderStrategy : BaseDataEnumImporter<Gender, int, Domain.Wings.GenderEnum>
    {
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(RateProvider));
            var version = typeof(MonahrqContext).Assembly.GetName().Version;
            VersionStrategy.Filename = string.Format("Gender-{0}-{1}-{2}", version.Major, version.Minor, version.Revision);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public override Gender GetEntity(object val)
        {
            var payer = new Gender
            {
                Id = (int)val,
                Name = Enum.GetName(typeof(Domain.Wings.GenderEnum), val),
                Value = (int)val
            };

            return payer.Name == "Exclude" || payer.Name == "Retain" ? null : payer;
        }
    }
}