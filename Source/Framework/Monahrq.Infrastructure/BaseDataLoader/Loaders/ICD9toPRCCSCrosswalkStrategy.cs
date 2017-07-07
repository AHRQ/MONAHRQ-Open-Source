using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System.ComponentModel.Composition;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="ICD9toPRCCSCrosswalk"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.ICD9toPRCCSCrosswalk, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class ICD9toPRCCSCrosswalkStrategy : BaseDataDataReaderImporter<ICD9toPRCCSCrosswalk, int>
    {
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "ICD9toPRCCSCrosswalk"; } }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new YearOnlyBaseDataVersionStrategy(Logger, DataProvider, typeof(ICD9toPRCCSCrosswalk));
        }

        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override ICD9toPRCCSCrosswalk LoadFromReader(System.Data.IDataReader dr)
        {
            return new ICD9toPRCCSCrosswalk()
            {
                ICD9ID = dr.Guard<string>("ICD9").Replace("'", string.Empty),
                PRCCSID = dr.Guard<int>("CCSID")
            };
        }
    }
}
