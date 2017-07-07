﻿using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="IPNationalTotalsDRG"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataSqlBulkImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.IPNationalTotalsDRG, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
    public class IPNationalTotalsDRGStrategy : BaseDataSqlBulkImporter<IPNationalTotalsDRG, int>
    {
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "IPNationalTotalsDRG"; } }
        /// <summary>
        /// Gets the format file.
        /// </summary>
        /// <value>
        /// The format file.
        /// </value>
        protected override string FormatFile { get { return "IPNationalTotals.fmt"; } }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new YearOnlyBaseDataVersionStrategy(Logger, DataProvider, typeof(IPNationalTotalsDRG))
            {
                IsReplaced = ImportType == BaseDataImportStrategyType.Replace
            };
        }
    }
}
