using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Data;
using System.Windows;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="State"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.State, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class StateStrategy : BaseDataDataReaderImporter<State, int>
    {
        /// <summary>
        /// Gets the loader priority.
        /// </summary>
        /// <value>
        /// The loader priority.
        /// </value>
        public override int LoaderPriority { get { return 1; } }
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "States"; } }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(State));
        }

        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override State LoadFromReader(System.Data.IDataReader dr)
        {
            return new State
            {
                Abbreviation = dr.Guard<string>("StateAbbreviation"),
                FIPSState = dr.Guard<string>("FIPSState"),
                Name = dr.Guard<string>("StateName"),
                Centroid = new Point(dr.Guard<double>("x0"), dr.Guard<double>("y0")),
                BoundingRegion = new Rect(
                        new Point(dr.Guard<double>("MinX"), dr.Guard<double>("MinY")),
                        new Point(dr.Guard<double>("MaxX"), dr.Guard<double>("MaxY")))
            };
        }
    }
}
