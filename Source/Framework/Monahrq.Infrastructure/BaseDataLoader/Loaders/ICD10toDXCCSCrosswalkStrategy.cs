using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="ICD10toDXCCSCrosswalk"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.ICD10toDXCCSCrosswalk, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class ICD10toDXCCSCrosswalkStrategy : BaseDataDataReaderImporter<ICD10toDXCCSCrosswalk, int>
    {
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "ICD10toDXCCSCrosswalk"; } }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new YearOnlyBaseDataVersionStrategy(Logger, DataProvider, typeof(ICD10toDXCCSCrosswalk));
        }

        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override ICD10toDXCCSCrosswalk LoadFromReader(System.Data.IDataReader dr)
        {
            return new ICD10toDXCCSCrosswalk
            {
                ICDID = dr.Guard<string>("ICDID").Replace("'", string.Empty),
                DXCCSID = dr.Guard<int>("DXCCSID")
            };
        }
    }
}