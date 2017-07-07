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
using Monahrq.Sdk.Modules.Wings;

using Monahrq.Infrastructure.Data;
using System.Windows;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Core;

namespace Monahrq.Wing.BaseData.LoadStrategy
{
    [Export]
    public class StateStrategy : IEntityDataReaderStrategy<State, int>
    {
        public State LoadFromReader(System.Data.IDataReader dr)
        {
            return new State()
            {
                Abbreviation = dr.Guard<string>("StateAbbreviation"),
                FIPSState = dr.Guard<string>("FIPSState"),
                Name = dr.Guard<string>("StateName"),
                Centroid = new Point(dr.Guard<double>("x0"), dr.Guard<double>("y0")),
                BoundingRegion = new Rect(
                        new Point(dr.Guard<double>("MinX"), dr.Guard<double>("MinY"))
                        , new Point(dr.Guard<double>("MaxX"), dr.Guard<double>("MaxY")))
            };
        }
    }

    [Export(typeof(IDataLoader))]
    public partial class StateLoader : EntityImporter<StateStrategy, State, int>
    {
        [ImportingConstructor]
        public StateLoader([Import(LogNames.Session)] ILogWriter logger,
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
                return Version_0_1_ReaderNames.State;
            }
        }
    }

}
