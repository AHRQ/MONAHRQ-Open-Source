using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="ZipCodeToLatLong"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataSqlBulkImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.ZipCodeToLatLong, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
    public class ZipCodeToLatLongStrategy : BaseDataSqlBulkImporter<ZipCodeToLatLong, int>
    {
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "ZipCodeToLatLong"; } }
        /// <summary>
        /// Gets the format file.
        /// </summary>
        /// <value>
        /// The format file.
        /// </value>
        protected override string FormatFile { get { return "ZipCodeToLatLong.fmt"; } }

        /// <summary>
        /// Gets the loader priority.
        /// </summary>
        /// <value>
        /// The loader priority.
        /// </value>
        public override int LoaderPriority { get { return 2; } }

        /// <summary>
        /// Gets a value indicating whether [turn off indexes during impport].
        /// </summary>
        /// <value>
        /// <c>true</c> if [turn off indexes during impport]; otherwise, <c>false</c>.
        /// </value>
        public override bool TurnOffIndexesDuringImpport
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new YearOnlyBaseDataVersionStrategy(Logger, DataProvider, typeof(ZipCodeToLatLong));
        }
    }
}
