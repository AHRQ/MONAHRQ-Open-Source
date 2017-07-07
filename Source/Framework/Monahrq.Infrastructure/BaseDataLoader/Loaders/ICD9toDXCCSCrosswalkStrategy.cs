using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System.ComponentModel.Composition;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="ICD9toDXCCSCrosswalk"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.ICD9toDXCCSCrosswalk, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class ICD9toDXCCSCrosswalkStrategy : BaseDataDataReaderImporter<ICD9toDXCCSCrosswalk, int>
    {
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "ICD9toDXCCSCrosswalk"; } }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new YearOnlyBaseDataVersionStrategy(Logger, DataProvider, typeof(ICD9toDXCCSCrosswalk));
        }

        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override ICD9toDXCCSCrosswalk LoadFromReader(System.Data.IDataReader dr)
        {
            return new ICD9toDXCCSCrosswalk()
            {
                ICD9ID = dr.Guard<string>("ICD9").Replace("'", string.Empty),
                DXCCSID = dr.Guard<int>("CCSID")
            };
        }
    }
}
