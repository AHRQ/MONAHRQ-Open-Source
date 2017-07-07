using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Monahrq.Infrastructure;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Sdk.Extensions;
using Monahrq.Wing.ReportingEntities;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Core;
using Monahrq.Infrastructure.Utility.Extensions;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Wing.ReportingEntities.BaseData
{
    /// <summary>
    /// Class to hold constants.
    /// </summary>
    static class Constants
    {
        /// <summary>
        /// Unique identifier for the Wing
        /// </summary>
        public const string WingGuid = "47622C20-88AB-4761-963F-AD033979B4E9";
        /// <summary>
        /// Generates <see cref="Guid"/> based upon the WingGuid
        /// </summary>
        public static readonly Guid WingGuidAsGuid = Guid.Parse(WingGuid);
    }

    //[WingModuleAttribute(typeof(Module), Constants.WingGuid, "Base Data", "Loads the base data", DependsOnModuleNames = new string[] { "Configuration Management", "Topics", "Base Data Loader" })]
    /// <summary>
    /// Module class.
    /// </summary>
    /// <seealso cref="Monahrq.Wing.ReportingEntities.ReportingEntityModule" />
    [WingModuleAttribute(typeof(Module), Constants.WingGuid, "Base Data", "Loads the base data", DependsOnModuleNames = new string[] { "Configuration Management" })]

    //[WingModuleAttribute(typeof(Module), Constants.WingGuid, "Base Data", "Loads the base data", DependsOnModuleNames = new string[] { "Configuration Management" })]
    public partial class Module : ReportingEntityModule
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Module"/> class and initializes other private members.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="factoryProvider">The factory provider.</param>
        /// <param name="programmability">The programmability.</param>
        /// <param name="dataLoaders">The data loaders.</param>
        [ImportingConstructor]
        public Module(
                     [Import(LogNames.Session)]ILogWriter logger, IDomainSessionFactoryProvider factoryProvider,
                     [ImportMany(DataImportContracts.BaseData)] IEnumerable<IProgrammability> programmability,
                     [ImportMany(DataImportContracts.BaseData)] IEnumerable<IDataLoader> dataLoaders
                     )
            : base(logger, factoryProvider, 
                    dataLoaders.Where(d=>d.DataProvider.VersionAttribute.Version == dataLoaders.Max(d1=>d1.DataProvider.VersionAttribute.Version)).Distinct().Prioritize())
        {

        }


    }

}

