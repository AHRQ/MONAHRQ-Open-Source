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
    public class DXCCSStrategy : IEntityDataReaderStrategy<DXCCS, int>
    {
        public DXCCS LoadFromReader(System.Data.IDataReader rdr)
        {
            return new DXCCS()
            {
                Description = rdr.Guard<string>("Description"),
                FirstYear = rdr.Guard<int>("FirstYear"),
                LastYear = rdr.Guard<int>("LastYear"),
                DXCCSID = rdr.Guard<int>("DXCCS")
            };
        }
    }

    [Export(typeof(IDataLoader))]
    public partial class DXCCSSourceLoader : EntityImporter<DXCCSStrategy, DXCCS, int>
    {
        [ImportingConstructor]
        public DXCCSSourceLoader([Import(LogNames.Session)] ILogWriter logger,
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
                return Version_0_1_ReaderNames.DXCCS;
            }
        }
    }

}
