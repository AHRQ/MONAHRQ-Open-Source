using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using NHibernate.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;


namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="HospitalCategory"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.Hospitals.HospitalCategory, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
    public class HospitalCategoriesStrategy : BaseDataDataReaderImporter<HospitalCategory, int>
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
        protected override string Fileprefix { get { return "HospitalCategories"; } }
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
        public static List<string> States { get; set; }
        /// <summary>
        /// Gets or sets the registry.
        /// </summary>
        /// <value>
        /// The registry.
        /// </value>
        public static HospitalRegistry Registry { get; set; }
        /// <summary>
        /// Gets or sets the hospital categories.
        /// </summary>
        /// <value>
        /// The hospital categories.
        /// </value>
        private static List<string> HospitalCategories { get; set; }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new YearOnlyBaseDataVersionStrategy(Logger, DataProvider, typeof(HospitalCategory));
        }

        /// <summary>
        /// Pres the load data.
        /// </summary>
        public override void PreLoadData()
        {
            base.PreLoadData();
            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
            {
                Registry = session.Query<HospitalRegistry>().SingleOrDefault();
                HospitalCategories = session.Query<HospitalCategory>().Select(x => x.Name).ToList();
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
            HospitalCategories = null;
        }

        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override HospitalCategory LoadFromReader(System.Data.IDataReader dr)
        {
            var name = dr.Guard<string>("Description");
            if (HospitalCategories.Contains(name))
                return null;

            var hrr = new HospitalCategory(Registry)    // TODO: Do we need to pass in the registry?
            {
                CategoryID = dr.Guard<int>("CategoryID"),
                Name = dr.Guard<string>("Description"),
                IsSourcedFromBaseData = true,
                Version = Registry.DataVersion
            };

            return hrr;
        }
    }
}
