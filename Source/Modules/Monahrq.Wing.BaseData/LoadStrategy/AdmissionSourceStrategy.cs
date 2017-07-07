using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Core;
using Monahrq.Infrastructure.Entities.Core;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Sdk.Modules.Wings;

namespace Monahrq.Wing.BaseData.LoadStrategy
{

    [Export]
    public class AdmissionSourceStrategy : IEntityDataReaderStrategy<AdmissionSource, int>
    {
        public AdmissionSource LoadFromReader(System.Data.IDataReader rdr)
        {
            return new AdmissionSource()
            {
                Id = rdr.GetInt32(rdr.GetOrdinal("AdmissionSourceID")),
                Name = rdr.GetString(rdr.GetOrdinal("Name"))
            };
        }
    }

    [Export(typeof(IDataLoader))]
    public partial class AdmissionSourceLoader : EntityImporter<AdmissionSourceStrategy, AdmissionSource, int>
    {
        [ImportingConstructor]
        public AdmissionSourceLoader([Import(LogNames.Session)] ILogWriter logger,
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
                return Version_0_1_ReaderNames.AdmissionSource;
            }
        }
    }



}
