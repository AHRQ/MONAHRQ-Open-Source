using Monahrq.Infrastructure.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Core.Import;
using System.ComponentModel.Composition;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="MeasureFilter"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataSqlBulkImporter{Monahrq.Infrastructure.Domain.BaseData.MeasureFilter, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
    public class MeasureFiltersStrategy : BaseDataSqlBulkImporter<MeasureFilter, int>
    {
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "MeasureFilters"; } }
        /// <summary>
        /// Gets the format file.
        /// </summary>
        /// <value>
        /// The format file.
        /// </value>
        protected override string FormatFile { get { return "MeasureFilters.fmt"; } }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new YearOnlyBaseDataVersionStrategy(Logger, DataProvider, typeof(MeasureFilter));
        }
    }
}
