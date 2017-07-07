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
    public class CountyStrategy : IEntityDataReaderStrategy<County, int>
    {
        public County LoadFromReader(System.Data.IDataReader rdr)
        {
            return new County()
            {
                CountyFIPS = rdr["CountyFIPS"].ToString(),
                CountyName = rdr["CountyName"].ToString(),
                StateFIPS = rdr["StateFIPS"].ToString(),
                StateName = rdr["StateName"].ToString(),
                StateAbbreviation = rdr["State"].ToString(),
                MinLongitude = float.Parse(rdr["MinLongitude"].ToString()),
                MaxLongitude = float.Parse(rdr["MaxLongitude"].ToString()),
                MinLatitude = float.Parse(rdr["MinLatitude"].ToString()),
                MaxLatitude = float.Parse(rdr["MaxLatitude"].ToString())
            };
        }
    }

    [Export(typeof(IDataLoader))]
    public partial class CountyModule : EntityImporter<CountyStrategy, County, int>
    {
        [ImportingConstructor]
        public CountyModule([Import(LogNames.Session)] ILogWriter logger,
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
                return Version_0_1_ReaderNames.County;
            }
        }
    }
}
