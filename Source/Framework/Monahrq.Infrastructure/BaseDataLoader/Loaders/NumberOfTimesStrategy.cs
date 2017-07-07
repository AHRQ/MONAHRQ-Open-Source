using System;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Core.Import;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="NumberOfTimes"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataEnumImporter{Monahrq.Infrastructure.Domain.BaseData.NumberOfTimes, System.Int32, Monahrq.Infrastructure.Domain.Wings.NumberOfTimesEnum}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class NumberOfTimesStrategy : BaseDataEnumImporter<NumberOfTimes, int, Domain.Wings.NumberOfTimesEnum>
    {
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(NumberOfTimes));
            var version = typeof(MonahrqContext).Assembly.GetName().Version;
            VersionStrategy.Filename = string.Format("NumberOfTimes-{0}-{1}-{2}", version.Major, version.Minor, version.Revision);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public override NumberOfTimes GetEntity(object val)
        {
            var payer = new NumberOfTimes
            {
                Id = (int)val,
                Name = Enum.GetName(typeof(Domain.Wings.NumberOfTimesEnum), val),
                Value = (int)val
            };

            return payer.Name == "Exclude" || payer.Name == "Retain" ? null : payer;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataEnumImporter{Monahrq.Infrastructure.Domain.BaseData.NumberOfTimes2, System.Int32, Monahrq.Infrastructure.Domain.Wings.NumberOfTimes2Enum}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class NumberOfTimes2Strategy : BaseDataEnumImporter<NumberOfTimes2, int, Domain.Wings.NumberOfTimes2Enum>
    {
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(NumberOfTimes2));
            var version = typeof(MonahrqContext).Assembly.GetName().Version;
            VersionStrategy.Filename = string.Format("NumberOfTimes2-{0}-{1}-{2}", version.Major, version.Minor, version.Revision);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public override NumberOfTimes2 GetEntity(object val)
        {
            var payer = new NumberOfTimes2
            {
                Id = (int)val,
                Name = Enum.GetName(typeof(Domain.Wings.NumberOfTimes2Enum), val),
                Value = (int)val
            };

            return payer.Name == "Exclude" || payer.Name == "Retain" ? null : payer;
        }
    }
}