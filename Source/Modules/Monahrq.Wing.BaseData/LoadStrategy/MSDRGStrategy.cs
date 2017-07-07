using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Core;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Domain.ClinicalDimensions;
using Monahrq.Infrastructure.Entities.Core;
using Monahrq.Infrastructure.Entities.Core.Import;

namespace Monahrq.Wing.BaseData.LoadStrategy
{
    [Export]
    public class MSDRGSourceStrategy : IEntityDataReaderStrategy<MSDRG, int>
    {
        public  MSDRG  LoadFromReader(System.Data.IDataReader rdr)
        {
            return new MSDRG()
            {
                Description = rdr.Guard<string>("Description"),
                MSDRGID = rdr.Guard<int>("MSDRG"),
                MDCID = rdr.Guard<int>("MDC"), 
                FirstYear = rdr.Guard<int>("FirstYear"),
                LastYear = rdr.Guard<int>("LastYear"),               
            };
        }
    }

    [Export(typeof(IDataLoader))]
    public partial class MSDRGSourceLoader : EntityImporter<MSDRGSourceStrategy, MSDRG, int>
    {
        [ImportingConstructor]
        public MSDRGSourceLoader([Import(LogNames.Session)] ILogWriter logger,
            IDomainSessionFactoryProvider sessionFactoryProvider
            , [ImportMany(DataImportContracts.ClincialDimensions)]
            IEnumerable<IDataReaderDictionary> dataproviders
            , Monahrq.Infrastructure.Configuration.IConfigurationService configurationService)
            : base(logger, sessionFactoryProvider, dataproviders, configurationService)
        {
        }

        public override ReaderDefinition Reader
        {
            get
            {
                return Version_0_1_ReaderNames.MSDRG;
            }
        }
    }

}
