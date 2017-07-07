using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System.ComponentModel.Composition;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="NHProviderToLatLong"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.NHProviderToLatLong, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class NHProviderToLatLongStrategy : BaseDataDataReaderImporter<NHProviderToLatLong, int>
    {
        ///// <summary>
        ///// Gets the loader priority.
        ///// </summary>
        ///// <value>
        ///// The loader priority.
        ///// </value>
        /// <summary>
        /// Gets the loader priority.
        /// </summary>
        /// <value>
        /// The loader priority.
        /// </value>
        public override int LoaderPriority { get { return 5; } }
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "NHProviderToLatLong"; } }
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(NHProviderToLatLong));
        }
        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override NHProviderToLatLong LoadFromReader(System.Data.IDataReader dr)
        {
            var provider = dr.Guard<object>("ProviderId");
            var item = new NHProviderToLatLong
            {
                ProviderId = provider.ToString().Replace("'",null),
                Latitude = dr.Guard<double>("Latitude"),
                Longitude = dr.Guard<double>("Longitude")
            };
            return item;
        }
    }
}
