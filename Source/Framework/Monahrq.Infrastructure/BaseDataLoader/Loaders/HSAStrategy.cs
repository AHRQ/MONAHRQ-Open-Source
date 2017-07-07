using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Extensions;
using NHibernate.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="HospitalServiceArea"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Domain.Regions.HospitalServiceArea, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class HSAStrategy : BaseDataDataReaderImporter<HospitalServiceArea, int>
    {
        /// <summary>
        /// Gets the type of the import.
        /// </summary>
        /// <value>
        /// The type of the import.
        /// </value>
        protected override BaseDataImportStrategyType ImportType { get { return BaseDataImportStrategyType.Append; } }

        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "HSA"; } }
        /// <summary>
        /// Gets the loader priority.
        /// </summary>
        /// <value>
        /// The loader priority.
        /// </value>
        public override int LoaderPriority { get { return 2; } }

        /// <summary>
        /// Gets or sets the states.
        /// </summary>
        /// <value>
        /// The states.
        /// </value>
        private static List<string> States { get; set; }
        /// <summary>
        /// Gets or sets the registry.
        /// </summary>
        /// <value>
        /// The registry.
        /// </value>
        private static HospitalRegistry Registry { get; set; }
        /// <summary>
        /// Gets or sets the hs as.
        /// </summary>
        /// <value>
        /// The hs as.
        /// </value>
        private static List<string> HSAs { get; set; }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new YearOnlyBaseDataVersionStrategy(Logger, DataProvider, typeof(HospitalServiceArea));
        }

        /// <summary>
        /// Pres the load data.
        /// </summary>
        public override void PreLoadData()
        {
            base.PreLoadData();
            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
            {
                States = session.Query<State>().Select(x => x.Abbreviation).ToList();
                Registry = session.Query<HospitalRegistry>().SingleOrDefault();
                HSAs = session.Query<HospitalServiceArea>().Select(x => x.Code).ToList();
            }
        }

        /// <summary>
        /// Posts the load data.
        /// </summary>
        public override void PostLoadData()
        {
            base.PostLoadData();
            States = null;
            Registry = null;
            HSAs = null;
        }

        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override HospitalServiceArea LoadFromReader(System.Data.IDataReader dr)
        {
            var state = States.FirstOrDefault(x => x == dr.Guard<string>(2).Trim()); //HSAState
            var code = string.Format("HSA{0}{1}", dr.Guard<int>(0), dr.Guard<string>(2)); //"HSANumber" || HSAState
            if (HSAs.Contains(code) || state.IsNullOrEmpty())
                return null;

            var hsa = new HospitalServiceArea(Registry)
            {
                Code = code,
                Version = Registry.DataVersion,     // TODO: Is this version really needed here?!?
                ImportRegionId = dr.Guard<int>(0),
                State = state,
                City = dr.Guard<string>(1), //HSACity
                IsSourcedFromBaseData = true
            };

            return hsa;
        }
    }
}
