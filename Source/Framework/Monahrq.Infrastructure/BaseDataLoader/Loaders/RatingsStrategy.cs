using System;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Core.Import;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="Ratings"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataEnumImporter{Monahrq.Infrastructure.Domain.BaseData.Ratings, System.Int32, Monahrq.Infrastructure.Domain.Wings.Ratings2Enum}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class RatingsStrategy : BaseDataEnumImporter<Ratings, int, Domain.Wings.Ratings2Enum>
    {
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(Ratings));
            var version = typeof(MonahrqContext).Assembly.GetName().Version;
            VersionStrategy.Filename = string.Format("Ratings-{0}-{1}-{2}", version.Major, version.Minor, version.Revision);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public override Ratings GetEntity(object val)
        {
            var payer = new Ratings
            {
                Id = (int)val,
                Name = Enum.GetName(typeof(Domain.Wings.Ratings2Enum), val),
                Value = (int)val
            };

            return payer.Name == "Exclude" || payer.Name == "Retain" ? null : payer;
        }
    }
}