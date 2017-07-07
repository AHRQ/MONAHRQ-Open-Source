using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System;
using System.ComponentModel.Composition;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="Race"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataEnumImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.Race, System.Int32, Monahrq.Infrastructure.Domain.Wings.Race}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
    public class RaceStrategy : BaseDataEnumImporter<Race, int, Domain.Wings.Race>
    {
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(Race));
            var version = typeof(MonahrqContext).Assembly.GetName().Version;
            VersionStrategy.Filename = string.Format("Race-{0}-{1}-{2}", version.Major, version.Minor, version.Revision);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public override Race GetEntity(object val)
        {
            var race = new Race
            {
                Id = (int) val, 
                Name = Enum.GetName(typeof (Domain.Wings.Race), val),
                Value = (int)val
            };

            return race.Name == "Exclude" || race.Name == "Retain" ? null : race;
        }
    }
}
