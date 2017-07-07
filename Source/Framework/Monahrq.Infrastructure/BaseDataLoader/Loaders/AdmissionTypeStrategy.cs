using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System;
using System.ComponentModel.Composition;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="AdmissionType"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataEnumImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.AdmissionType, System.Int32, Monahrq.Infrastructure.Domain.Wings.AdmissionType}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
    public class AdmissionTypesStrategy : BaseDataEnumImporter<AdmissionType, int, Domain.Wings.AdmissionType>
    {
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(AdmissionType));
            var version = typeof(MonahrqContext).Assembly.GetName().Version;
            VersionStrategy.Filename = string.Format("AdmissionType-{0}-{1}-{2}", version.Major, version.Minor, version.Revision);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public override AdmissionType GetEntity(object val)
        {
            var admissionType = new AdmissionType
            {
                Id = (int) val,
                Name = Enum.GetName(typeof (Domain.Wings.AdmissionType), val),
                Value = (int) val
            };

            return admissionType.Name == "Exclude" ? null : admissionType;
        }
    }
}
