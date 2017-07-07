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

namespace Monahrq.Wing.BaseData.HRRBaseData
{
    static class Constants
    {
        public const string WingGuid = "B537FBC0-08E3-429E-8B43-7D72AA7A3ADE";
        public static readonly Guid WingGuidAsGuid = Guid.Parse(WingGuid);
    }

    [Export(typeof(IDataLoader))]
    public partial class HRRModule  
        : HospitalRegionRunner<HealthReferralRegionStrategy, HealthReferralRegion, int>
    {
        [ImportingConstructor]
        public HRRModule([Import(LogNames.Session)] ILogWriter logger,
            IWingsSessionFactoryProvider sessionFactoryProvider
            , HealthReferralRegionStrategy entityStrategy
            , BaseDataProvider baseDataProvider)
            : base(logger, sessionFactoryProvider, entityStrategy, baseDataProvider)
        {

        }

        protected override string ReaderName
        {
            get
            {
                return Version_0_1_ReaderNames.HRR;
            }
        }
 
    }
}
