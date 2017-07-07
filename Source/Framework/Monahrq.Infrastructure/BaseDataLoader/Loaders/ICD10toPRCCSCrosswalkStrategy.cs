using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="ICD10toPRCCSCrosswalk"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.ICD10toPRCCSCrosswalk, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class ICD10toPRCCSCrosswalkStrategy : BaseDataDataReaderImporter<ICD10toPRCCSCrosswalk, int>
    {
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "ICD10toPRCCSCrosswalk"; } }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new YearOnlyBaseDataVersionStrategy(Logger, DataProvider, typeof(ICD10toPRCCSCrosswalk));
        }

        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override ICD10toPRCCSCrosswalk LoadFromReader(System.Data.IDataReader dr)
        {
            return new ICD10toPRCCSCrosswalk
            {
                ICDID = dr.Guard<string>("ICDID").Replace("'", string.Empty),
                PRCCSID = dr.Guard<int>("PRCCSID")
            };
        }
    }
}