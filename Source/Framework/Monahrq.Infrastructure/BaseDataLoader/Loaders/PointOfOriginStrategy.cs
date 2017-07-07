using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System;
using System.ComponentModel.Composition;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="PointOfOrigin"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataEnumImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.PointOfOrigin, System.Int32, Monahrq.Infrastructure.Domain.Wings.PointOfOrigin}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
    public class PointOfOriginStrategy : BaseDataEnumImporter<PointOfOrigin, int, Domain.Wings.PointOfOrigin>
    {
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(PointOfOrigin));
            var version = typeof(MonahrqContext).Assembly.GetName().Version;
            VersionStrategy.Filename = string.Format("PointOfOrigin-{0}-{1}-{2}", version.Major, version.Minor, version.Revision);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public override PointOfOrigin GetEntity(object val)
        {
            var pointOfOrigin = new PointOfOrigin
            {
                Id = (int) val,
                Name = Enum.GetName(typeof (Domain.Wings.PointOfOrigin), val),
                Value = (int) val
            };

            return pointOfOrigin.Name == "Exclude" ? null : pointOfOrigin;
        }
    }
}
