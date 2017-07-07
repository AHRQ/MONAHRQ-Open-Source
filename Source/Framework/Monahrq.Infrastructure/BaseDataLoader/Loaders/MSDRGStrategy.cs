using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Domain.ClinicalDimensions;
using Monahrq.Infrastructure.Entities.Core.Import;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="MSDRG"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Domain.ClinicalDimensions.MSDRG, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
    public class MSDRGStrategy : BaseDataDataReaderImporter<MSDRG, int>
    {
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "MSDRG"; } }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(MSDRG));
        }

        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override MSDRG LoadFromReader(System.Data.IDataReader dr)
        {
            return new MSDRG()
            {
                MSDRGID = dr.Guard<int>("MSDRG"),
                MDCID = dr.Guard<int>("MDC"),
                Description = dr.Guard<string>("Description"),
                FirstYear = dr.Guard<int>("FirstYear"),
                LastYear = dr.Guard<int>("LastYear")
            };
        }
    }
}
