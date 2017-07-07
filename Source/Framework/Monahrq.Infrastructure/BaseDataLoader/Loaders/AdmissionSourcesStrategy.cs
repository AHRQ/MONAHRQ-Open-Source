using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System;
using System.ComponentModel.Composition;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="AdmissionSource"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataEnumImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.AdmissionSource, System.Int32, Monahrq.Infrastructure.Domain.Wings.AdmissionSource}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
    public class AdmissionSourcesStrategy : BaseDataEnumImporter<AdmissionSource, int, Domain.Wings.AdmissionSource>
    {
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(AdmissionSource));
            var version = typeof(MonahrqContext).Assembly.GetName().Version;
            VersionStrategy.Filename = string.Format("AdmissionSource-{0}-{1}-{2}", version.Major, version.Minor, version.Revision);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public override AdmissionSource GetEntity(object val)
        {
            var admissionSource = new AdmissionSource
            {
                Id = (int) val,
                Name = Enum.GetName(typeof (Domain.Wings.AdmissionSource), val),
                Value = (int) val
            };

            return admissionSource.Name == "Exclude" ? null : admissionSource;
        }
    }
}
