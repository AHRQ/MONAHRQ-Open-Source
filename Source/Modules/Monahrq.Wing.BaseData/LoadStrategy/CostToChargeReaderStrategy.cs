using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Core;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Core.Import;
using System.Collections.Generic;
using Monahrq.Infrastructure.Entities.Core;

namespace Monahrq.Wing.BaseData.LoadStrategy
{

    [Export]
    public class CostToChargeReaderStrategy : IEntityDataReaderStrategy<CostToCharge, int>
    {
        public CostToCharge LoadFromReader(System.Data.IDataReader rdr)
        {
            return new CostToCharge()
               {
                   ProviderID = rdr["ProviderID"].ToString(),
                   Year = rdr["Year"].ToString(),
                   Ratio = float.Parse(rdr["CostToChargeRatio"].ToString())
               };
        }
    }

    [Export(typeof(IDataLoader))]
    public partial class CostToChargeModule : EntityImporter<CostToChargeReaderStrategy, CostToCharge, int>
    {
        [ImportingConstructor]
        public CostToChargeModule([Import(LogNames.Session)] ILogWriter logger,
            IDomainSessionFactoryProvider sessionFactoryProvider
             , [ImportMany(DataImportContracts.BaseData)]
            IEnumerable<IDataReaderDictionary> dataproviders
            , Monahrq.Infrastructure.Configuration.IConfigurationService configurationService)
            : base(logger, sessionFactoryProvider, dataproviders, configurationService)
        {
        }

        public override ReaderDefinition Reader
        {
            get
            {
                return Version_0_1_ReaderNames.CostToChargeRatio;
            }
        }
    }
}
