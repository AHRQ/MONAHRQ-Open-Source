using Monahrq.DataSets.Model;
using Monahrq.Default.DataProvider.Administration.File;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.DataProvider;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Data;

namespace Monahrq.DataSets.Services
{
    /// <summary>
    /// The datacontext services interface.
    /// </summary>
    public interface IDataContextServices
    {
        /// <summary>
        /// Resets this instance.
        /// </summary>
        void Reset();
        /// <summary>
        /// Gets the controller.
        /// </summary>
        /// <value>
        /// The controller.
        /// </value>
        IDataProviderController Controller { get; }
        /// <summary>
        /// Gets the current file.
        /// </summary>
        /// <value>
        /// The current file.
        /// </value>
        string CurrentFile  { get; }
        /// <summary>
        /// Gets or sets the connection factory.
        /// </summary>
        /// <value>
        /// The connection factory.
        /// </value>
        Func<IDbConnection> ConnectionFactory { get; set; }
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        INamedConnectionElement Configuration { get;  }
    }

    /// <summary>
    /// The data provider factory class. This class is responsibile to loading the csv files in the dataset import wizard.
    /// </summary>
    public class DataproviderFactory
    {
        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>
        /// The services.
        /// </value>
        public IDataContextServices Services { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataproviderFactory"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public DataproviderFactory(IDataContextServices services)
        {
            Services = services;
        }

        /// <summary>
        /// Gets the schema ini.
        /// </summary>
        /// <value>
        /// The schema ini.
        /// </value>
        string SchemaIni
        {
            get
            {
                return Path.Combine(Path.GetTempPath(), "Schema.ini");
            }
        }

        /// <summary>
        /// Gets the destination.
        /// </summary>
        /// <value>
        /// The destination.
        /// </value>
        string Destination
        {
            get
            {
                return Path.Combine(Path.GetTempPath(), Services.Configuration.SelectFrom);
            }
        }

        /// <summary>
        /// Initializes the data provider.
        /// </summary>
        public void InitDataProvider()
        {
            if (File.Exists(Destination)) File.Delete(Destination);
            if (File.Exists(SchemaIni)) File.Delete(SchemaIni);
            Services.ConnectionFactory = () =>
            {
                var conn = Services.Controller.ProviderFactory.CreateConnection();
                var builder = Services.Controller.ProviderFactory.CreateConnectionStringBuilder();
                builder.ConnectionString = Services.Configuration.ConnectionString;
                RelocateTextBuilder(builder);
                conn.Disposed += (o, e) =>
                {
                    if (File.Exists(Destination))
                    {
                        File.Delete(Destination);
                    }
                    if (File.Exists(SchemaIni))
                    {
                        File.Delete(SchemaIni);
                    }
                };
                conn.ConnectionString = builder.ConnectionString;
                return conn;
            };
        }

        /// <summary>
        /// Relocates the text builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private void RelocateTextBuilder(DbConnectionStringBuilder builder)
        {
            var props = builder["Extended Properties"].ToString().ToUpper().Split(';').ToList();
            var isText = props.Contains("TEXT");
            if (!isText) return;
            var conElem = Services.Configuration;
            var isCsv = props.Contains("FMT=DELIMITED");
            var hasHeader = props.Contains("HDR=YES");
            File.Copy(SourceFile, Destination);
            var list = new List<string>()
                        {
                            string.Format("[{0}]",  conElem.SelectFrom)
                        };
            if (isCsv)
            {
                list.Add("Format=CSVDelimited");
            }
            list.Add(string.Format("ColNameHeader={0}", hasHeader.ToString()));
            list.Add("MaxScanRows=0");

            // the UI dialog put the HasDoubleQuotes property into NamedConnectionElement.SchemaIniSettings,
            // so now we can get all the ini elements here using conElem and add them to the list
            foreach (var element in conElem.SchemaIniSettings.OfType<SchemaIniElement>())
            {
                var line = string.Format("{0}={1}", element.Name, element.Value);
                list.Add(line);
            }
            
            File.WriteAllLines(SchemaIni, list);
            builder["data source"] = Path.GetDirectoryName(Destination);
        }

        /// <summary>
        /// Gets the source file.
        /// </summary>
        /// <value>
        /// The source file.
        /// </value>
        public string SourceFile
        {
            get
            {
                return Services.CurrentFile;
            }
        }
    }

    /// <summary>
    /// The datacontext aware class.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.Services.IDataContextServices" />
    class DatacontextAware : IDataContextServices
    {
        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        DatasetContext Context { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="DatacontextAware"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public DatacontextAware(DatasetContext context)
        {
            Context = context;
            Reset();
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            LazyController = new Lazy<IDataProviderController>(() => Context.DatasourceDefinition.CurrentDataProvider, true);
            LazyConfig = new Lazy<INamedConnectionElement>(()=> Context.DatasourceDefinition.ConnectionElement,true);
            LazyCurrentFile = new Lazy<string>(() => Context.DatasourceDefinition.CurrentFile, true);
        }

        /// <summary>
        /// Gets or sets the lazy controller.
        /// </summary>
        /// <value>
        /// The lazy controller.
        /// </value>
        Lazy<IDataProviderController> LazyController { get; set; }
        /// <summary>
        /// Gets the controller.
        /// </summary>
        /// <value>
        /// The controller.
        /// </value>
        public IDataProviderController Controller
        {
            get { return LazyController.Value; }
        }

        /// <summary>
        /// Gets or sets the lazy current file.
        /// </summary>
        /// <value>
        /// The lazy current file.
        /// </value>
        Lazy<string> LazyCurrentFile{ get; set; }

        /// <summary>
        /// Gets the current file.
        /// </summary>
        /// <value>
        /// The current file.
        /// </value>
        public string CurrentFile
        {
            get { return LazyCurrentFile.Value; }
        }

        /// <summary>
        /// Gets or sets the connection factory.
        /// </summary>
        /// <value>
        /// The connection factory.
        /// </value>
        public Func<IDbConnection> ConnectionFactory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the lazy configuration.
        /// </summary>
        /// <value>
        /// The lazy configuration.
        /// </value>
        Lazy<INamedConnectionElement> LazyConfig { get; set; }
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public INamedConnectionElement Configuration
        {
            get { return LazyConfig.Value; }
        }
    }
}
