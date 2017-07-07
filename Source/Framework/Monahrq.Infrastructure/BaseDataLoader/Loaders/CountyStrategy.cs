using System;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="County"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.County, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class CountyStrategy : BaseDataDataReaderImporter<County, int>
    {
        /// <summary>
        /// Gets the loader priority.
        /// </summary>
        /// <value>
        /// The loader priority.
        /// </value>
        public override int LoaderPriority { get { return 2; } }
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "Counties"; } }
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(County));
        }
        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override County LoadFromReader(System.Data.IDataReader dr)
        {
            return new County
                {
                    Name = dr.Guard<string>("NAME"),
                    CountyFIPS = dr.Guard<string>("GEOID").PadLeft(5, '0'),
                    CountySSA = dr.ColumnExists("SSA") ?
                                    !string.IsNullOrEmpty(dr.Guard<string>("SSA")) ? dr.Guard<string>("SSA").PadLeft(3, '0') : null
                                    : null,
                    State = dr.Guard<string>("USPS")
                };
        }
    }
}