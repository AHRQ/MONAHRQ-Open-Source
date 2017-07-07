using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using NHibernate.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="ZipCodeToHRRAndHSA"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.ZipCodeToHRRAndHSA, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class ZipCodeToHRRAndHSAStrategy : BaseDataDataReaderImporter<ZipCodeToHRRAndHSA, int>
    {
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "ZipCodeToHRRAndHSA"; } }

        /// <summary>
        /// Gets the loader priority.
        /// </summary>
        /// <value>
        /// The loader priority.
        /// </value>
        public override int LoaderPriority { get { return 2; } }

        /// <summary>
        /// Gets the type of the import.
        /// </summary>
        /// <value>
        /// The type of the import.
        /// </value>
        protected override BaseDataImportStrategyType ImportType { get { return BaseDataImportStrategyType.Replace;} }

        /// <summary>
        /// Gets or sets the states.
        /// </summary>
        /// <value>
        /// The states.
        /// </value>
        public static List<State> States { get; set; }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new YearOnlyBaseDataVersionStrategy(Logger, DataProvider, typeof(ZipCodeToHRRAndHSA), this.ImportType == BaseDataImportStrategyType.Replace);
        }

        /// <summary>
        /// Pres the load data.
        /// </summary>
        public override void PreLoadData()
        {
            base.PreLoadData();
            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
            {
                States = session.Query<State>().ToList();
            }
        }

        /// <summary>
        /// Posts the load data.
        /// </summary>
        public override void PostLoadData()
        {
            base.PostLoadData();
            States = null;
        }

        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override ZipCodeToHRRAndHSA LoadFromReader(System.Data.IDataReader dr)
        {
            var state = States.FirstOrDefault(x => x.Abbreviation == dr.Guard<string>(3).Trim());
            /*
            var zipCodeToHRRandHSA = new ZipCodeToHRRAndHSA
            {
                Zip = dr.Guard<string>(0).PadLeft(5, '0'),
                HSANumber = dr.Guard<int>(1),
                HRRNumber = dr.Guard<int>(2),
                State = state.Abbreviation,
                StateFIPS = state.FIPSState
            };
            */
       
            var zipCodeToHrrAndHsa = new ZipCodeToHRRAndHSA
            {
                Zip = dr.Guard<string>(0).PadLeft(5, '0'),
                HSANumber = dr.Guard<int>(2),
                HRRNumber = dr.Guard<int>(1),
                State = state.Abbreviation,
                StateFIPS = state.FIPSState
            };

            return zipCodeToHrrAndHsa;
        }
    }
}
