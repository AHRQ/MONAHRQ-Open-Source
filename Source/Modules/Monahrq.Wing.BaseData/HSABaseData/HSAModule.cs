using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Extensibility;
using Monahrq.Sdk.Extensibility.ContentManagement.Records;
using Monahrq.Sdk.Modules.Wings;
using NHibernate;
using System;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.OleDb;
using System.IO;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Domain.Hospitals;
using Monahrq.Infrastructure.Core;

namespace Monahrq.Wing.BaseData.HSABaseData
{
    [Export(typeof(IDataLoader))]
    public partial class HSAModule :
        HospitalRegionRunner<HospitalServiceAreaRegionStrategy, HospitalServiceArea, int>
    {
        [ImportingConstructor]
        public HSAModule([Import(LogNames.Session)] ILogWriter logger,
            IWingsSessionFactoryProvider sessionFactoryProvider,
            HospitalServiceAreaRegionStrategy entityStrategy
            , BaseDataProvider baseDataProvider)
            : base(logger, sessionFactoryProvider, entityStrategy, baseDataProvider)
        {

        }

        protected override string ReaderName
        {
            get
            {
                return Version_0_1_ReaderNames.HSA;
            }
        }

    }
}
