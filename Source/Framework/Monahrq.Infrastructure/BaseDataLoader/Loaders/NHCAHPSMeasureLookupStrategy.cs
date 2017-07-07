using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System.ComponentModel.Composition;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="NHCAHPSMeasureLookup"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.NHCAHPSMeasureLookup, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
    public class NHCAHPSMeasureLookupStrategy : BaseDataDataReaderImporter<NHCAHPSMeasureLookup, int>
	{
        /// <summary>
        /// Gets the type of the import.
        /// </summary>
        /// <value>
        /// The type of the import.
        /// </value>
        protected override BaseDataImportStrategyType ImportType { get { return BaseDataImportStrategyType.Replace; } }
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
        protected override string Fileprefix { get { return "NHCAHPSMeasureLookup"; } }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
		{
			base.OnImportsSatisfied();
			VersionStrategy = new YearOnlyBaseDataVersionStrategy(Logger, DataProvider, typeof(NHCAHPSMeasureLookup));
		}

        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override NHCAHPSMeasureLookup LoadFromReader(System.Data.IDataReader dr)
		{
			var item = new NHCAHPSMeasureLookup
			{
				MeasureId = dr.Guard<string>("MeasureId"),
				MeasureType = dr.Guard<string>("MeasureType"),
				CAHPSQuestionType = dr.Guard<string>("CAHPSQuestionType"),
			};
			return item;
		}
	}
}
