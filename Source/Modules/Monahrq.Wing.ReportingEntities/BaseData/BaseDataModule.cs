using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Modularity;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.BaseDataLoader;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Modules.Wings;

namespace Monahrq.Wing.ReportingEntities.BaseData
{
    /// <summary>
    /// Class hold to constants.
    /// </summary>
    static class BaseDataConstants
    {
        /// <summary>
        /// The wing unique identifier.
        /// </summary>
        public const string WingGuid = "A83E3A02-2B84-417B-BF5E-8D334037D7C9";
        /// <summary>
        /// The wing unique identifier as unique identifier.
        /// </summary>
        public static readonly Guid WingGuidAsGuid = Guid.Parse(WingGuid);
    }

    /// <summary>
    /// Base data module class.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Modules.Wings.WingModule" />
    [WingModuleAttribute(typeof(Module), BaseDataConstants.WingGuid, "Base Data Loader", "Loads the base data", DependsOnModuleNames = new [] { "Configuration Management" },
        InitializationMode = InitializationMode.WhenAvailable)]
    public class BaseDataModule : WingModule
    {
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        private ILogWriter Logger { get; set; }

        /// <summary>
        /// Gets or sets the factory provider.
        /// </summary>
        /// <value>
        /// The factory provider.
        /// </value>
        private IDomainSessionFactoryProvider FactoryProvider { get; set; }
        /// <summary>
        /// Gets or sets the base data loaders.
        /// </summary>
        /// <value>
        /// The base data loaders.
        /// </value>
        private IEnumerable<IBasedataImporter> BaseDataLoaders { get; set; }
        /// <summary>
        /// Occurs when [on feedback].
        /// </summary>
        public event EventHandler<ExtendedEventArgs<string>> OnFeedback = delegate { };
        /// <summary>
        /// Provides the feedback.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        protected void ProvideFeedback(string msg) { OnFeedback(this, new ExtendedEventArgs<string>(msg)); }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDataModule"/> class and initializes other private members.
        /// </summary>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="factoryProvider">The factory provider.</param>
        /// <param name="baseDataLoaders">The base data loaders.</param>
        [ImportingConstructor]
        public BaseDataModule(
                    [Import(LogNames.Session)]ILogWriter logger,
                    IDomainSessionFactoryProvider factoryProvider,
                    [ImportMany(DataImportContracts.BaseDataLoader)] IEnumerable<IBasedataImporter> baseDataLoaders)
        {
            Logger = logger;
            FactoryProvider = factoryProvider;
            BaseDataLoaders = baseDataLoaders.OrderBy(x => x.LoaderPriority);

            OnFeedback += (sender, args) => Events.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = args.Data });
        }

        /// <summary>
        /// Override the initialize method to  perfomr load data operation.
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();

            foreach (var loader in BaseDataLoaders)
            {
                ProvideFeedback(string.Format("Loading {0}", loader.LoaderDescription));
                loader.PreLoadData();
                loader.LoadData();
                loader.PostLoadData();
            }
            Logger.Information("Base data loaded");
        }
    }

}
