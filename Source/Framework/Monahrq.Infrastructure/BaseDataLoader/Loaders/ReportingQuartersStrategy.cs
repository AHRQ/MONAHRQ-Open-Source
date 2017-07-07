using System.ComponentModel.Composition;
using System.Data;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="ReportingQuarter"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.ReportingQuarter, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
    public class ReportingQuartersStrategy : BaseDataDataReaderImporter<ReportingQuarter, int>
    {
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "ReportingQuarters"; } }
        /// <summary>
        /// Gets the type of the import.
        /// </summary>
        /// <value>
        /// The type of the import.
        /// </value>
        protected override BaseDataImportStrategyType ImportType { get { return BaseDataImportStrategyType.Replace; } }
        // protected override string FormatFile { get { return "ReportingQuarters.fmt"; } }

        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="rdr">The RDR.</param>
        /// <returns></returns>
        public override ReportingQuarter LoadFromReader(IDataReader rdr)
        {
            return new ReportingQuarter
            {
                Id = rdr.GetInt32(0),
                Name = rdr.GetString(1)
            };
        }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new YearOnlyBaseDataVersionStrategy(Logger, DataProvider, typeof(ReportingQuarter), ImportType == BaseDataImportStrategyType.Replace);
        }
    }
}
