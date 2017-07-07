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
    public class ZipCodeStrategy : IEntityDataReaderStrategy<ZipCode, int>
    {
        public ZipCode LoadFromReader(System.Data.IDataReader rdr)
        {
            return new ZipCode()
            {
                Zip = rdr["ZipCode"].ToString(),
                HSANumber = int.Parse(rdr["HSANumber"].ToString()),
                HRRNumber = int.Parse(rdr["HRRNumber"].ToString()),
                Latitude = float.Parse(rdr["Latitude"].ToString()),
                Longitude = float.Parse(rdr["Longitude"].ToString())
            };
        }
    }

    [Export(typeof(IDataLoader))]
    public partial class ZipCodeModule : EntityImporter<ZipCodeStrategy, ZipCode, int>
    {
        [ImportingConstructor]
        public ZipCodeModule([Import(LogNames.Session)] ILogWriter logger,
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
                return Version_0_1_ReaderNames.ZipCode;
            }
        }
    }
}
