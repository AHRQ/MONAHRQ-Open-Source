using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System.ComponentModel.Composition;
using System.Linq;
using Monahrq.Infrastructure.Domain;
using NHibernate.Linq;

namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="CostToCharge"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.BaseData.CostToCharge, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class CostToChargeStrategy : BaseDataDataReaderImporter<CostToCharge, int>
    {
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "CostToChargeRatio"; } }

        /// <summary>
        /// Gets the loader priority.
        /// </summary>
        /// <value>
        /// The loader priority.
        /// </value>
        public override int LoaderPriority { get { return 4; } }
        /// <summary>
        /// Gets the type of the import.
        /// </summary>
        /// <value>
        /// The type of the import.
        /// </value>
        protected override BaseDataImportStrategyType ImportType { get { return BaseDataImportStrategyType.Append; } }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new YearOnlyBaseDataVersionStrategy(Logger, DataProvider, typeof(CostToCharge));
        }

        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override CostToCharge LoadFromReader(System.Data.IDataReader dr)
        {
            var providerId = dr.Guard<string>(0).Replace("'", string.Empty);
            var year = dr.Guard<string>(1);
            var ratio = System.Math.Round(dr.Guard<double>(2), 5);

            return new CostToCharge
            {
                ProviderID = providerId,
                Year =year,
                Ratio = ratio
            };
        }

        /// <summary>
        /// Pres the load data.
        /// </summary>
        public override void PreLoadData()
        {
            base.PreLoadData();

            using (var session = DataProvider.SessionFactory.OpenSession())
            {
                var schemaVersion = session.Query<SchemaVersion>().FirstOrDefault(x => x.Name.ToLower() == "Base_CostToCharges".ToLower() && x.Version.ToLower() == "2013.0.0".ToLower());

                if (schemaVersion == null) return;

                using (var trans = session.BeginTransaction())
                {
                    session.Evict(schemaVersion);

                    var sqlQuery = "truncate table [dbo].[Base_CostToCharges];" +
                                   "delete from [dbo].[SchemaVersions] where [Name]='Base_CostToCharges';";

                    var query = session.CreateSQLQuery(sqlQuery);
                    query.ExecuteUpdate();

                    trans.Commit();
                }
            }
        }
    }
}
