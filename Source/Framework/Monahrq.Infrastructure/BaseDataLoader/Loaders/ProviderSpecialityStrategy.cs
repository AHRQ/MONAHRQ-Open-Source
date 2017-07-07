using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="ProviderSpeciality"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.ProviderSpeciality, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class ProviderSpecialityStrategy : BaseDataDataReaderImporter<ProviderSpeciality, int>
    {
        ///// <summary>
        ///// Gets the loader priority.
        ///// </summary>
        ///// <value>
        ///// The loader priority.
        ///// </value>
        public override int LoaderPriority { get { return 2; } }
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "ProviderSpecialities"; } }
        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new DefaultBaseDataVersionStrategy(Logger, DataProvider, typeof(ProviderSpeciality));
        }
        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override ProviderSpeciality LoadFromReader(System.Data.IDataReader dr)
        {
            return new ProviderSpeciality
            {
                Name = Inflector.ProperCase(dr.Guard<string>("Name"),@"\/"),
                ProviderTaxonomy = dr.Guard<string>("provider_taxonomy")
            };
        }
    }
}
