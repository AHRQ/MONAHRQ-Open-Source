using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System;
using System.ComponentModel.Composition;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="Sex"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataEnumImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.Sex, System.Int32, Monahrq.Infrastructure.Domain.Wings.Sex}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
    public class SexStrategy : BaseDataEnumImporter<Sex, int, Domain.Wings.Sex>
    {
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(Sex));
            var version = typeof(MonahrqContext).Assembly.GetName().Version;
            VersionStrategy.Filename = string.Format("Sex-{0}-{1}-{2}", version.Major, version.Minor, version.Revision);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public override Sex GetEntity(object val)
        {
            Sex sex = new Sex
            {
                Id = (int) val, 
                Name = Enum.GetName(typeof (Domain.Wings.Sex), val),
                Value = (int)val
            };

            if (sex.Name == "Exclude")
            {
                return null;
            }
            return sex;
        }
    }
}
