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
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Core;

namespace Monahrq.Wing.BaseData.LoadStrategy
{
    [Export]
    public class DrgSourceStrategy : IEntityDataReaderStrategy<DRG, int>
    {
        public DRG LoadFromReader(System.Data.IDataReader rdr)
        {
            return new DRG()
            {
                Description = rdr.Guard<string>("Description"),
                DRGID = rdr.Guard<int>("DRG"),
                FirstYear = rdr.Guard<int>("FirstYear"),
                LastYear = rdr.Guard<int>("LastYear"),
                MDCID = rdr.Guard<int>("MDC")
            };
        }
    }

    [Export(typeof(IDataLoader))]
    public partial class DrgSourceLoader : EntityImporter<DrgSourceStrategy, DRG, int>
    {
        [ImportingConstructor]
        public DrgSourceLoader([Import(LogNames.Session)] ILogWriter logger,
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
                return Version_0_1_ReaderNames.DRG;
            }
        }
    }

}
