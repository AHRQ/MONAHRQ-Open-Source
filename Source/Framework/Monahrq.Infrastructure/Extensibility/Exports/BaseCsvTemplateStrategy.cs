using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.Extensibility.Exports
{
    public abstract class BaseCsvTemplateExportStrategy
    {
        private const string TEMPLATE_FILE_SUFFIX = " template";

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCsvTemplateExportStrategy"/> class.
        /// </summary>
        protected BaseCsvTemplateExportStrategy()
        {
            if (Logger == null) Logger = ServiceLocator.Current.GetInstance<ILogWriter>(LogNames.Session);
            if (DataProvider == null) DataProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            if (ConfigurationService == null) ConfigurationService = ServiceLocator.Current.GetInstance<IConfigurationService>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCsvTemplateExportStrategy"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="configurationService">The configuration service.</param>
        protected BaseCsvTemplateExportStrategy(ILogWriter logger, IDomainSessionFactoryProvider dataProvider, IConfigurationService configurationService)
        {
            Logger = logger;
            DataProvider = dataProvider;
            ConfigurationService = configurationService;
        }

        #region Abstract Members
        /// <summary>
        /// Initializes the Template export strategy.
        /// </summary>
        protected abstract void Initialize();
        /// <summary>
        /// Gets the column definitions.
        /// </summary>
        /// <returns></returns>
        protected abstract IList<TemplateColumnDefinition> GetColumnDefinitions();
        /// <summary>
        /// Gets the template file prefix.
        /// </summary>
        /// <value>
        /// The template file prefix.
        /// </value>
        protected abstract string TemplateFilePrefix { get; }
        /// <summary>
        /// Generates the sample data.
        /// </summary>
        /// <returns></returns>
        protected abstract IList<TemplateRow> GenerateSampleData();
        #endregion

        #region Members
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        protected ILogWriter Logger { get; set; }
        /// <summary>
        /// Gets or sets the data provider.
        /// </summary>
        /// <value>
        /// The data provider.
        /// </value>
        [Import]
        protected IDomainSessionFactoryProvider DataProvider { get; set; }
        /// <summary>
        /// Gets or sets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        [Import]
        protected IConfigurationService ConfigurationService { get; set; }
        /// <summary>
        /// Gets the template directory path.
        /// </summary>
        /// <value>
        /// The template directory path.
        /// </value>
        protected virtual string TemplateDirectoryPath
        {
            get { return Path.Combine(MonahrqContext.MyDocumentsApplicationDirPath, "Templates"); }
        }

        /// <summary>
        /// Executes the generation of the sample template export.
        /// </summary>
        /// <param name="ctx">The cancellation token.</param>
        /// <returns></returns>
        public virtual async Task<ExportResult> ExecuteExport(CancellationToken ctx)
        {
            var result = await Task.Run(() =>
            {
                var asyncResult = new ExportResult();
                    
                try
                {
                    if (!Directory.Exists(TemplateDirectoryPath)) Directory.CreateDirectory(TemplateDirectoryPath);

                    var fullTemplatePath = string.Join(TemplateDirectoryPath, string.Format("{0}{1}.csv", TemplateFilePrefix, TEMPLATE_FILE_SUFFIX));

                    if (File.Exists(fullTemplatePath))
                    {
                        asyncResult.Success = true;
                        asyncResult.Object = fullTemplatePath;
                        return Task.FromResult(asyncResult);
                    }

                    var csvBuilder = new StringBuilder();
                    var columnDefinitions = GetColumnDefinitions().ToList();

                    if (columnDefinitions.Any())
                        csvBuilder.AppendLine(string.Join(",", columnDefinitions.Select(col => col.Name).ToArray()));

                    var rows = GenerateSampleData().ToList();

                    rows.ForEach(row =>
                    {
                        csvBuilder.AppendLine(string.Join(",", row.Values.ToArray()));
                    });

                    File.WriteAllText(fullTemplatePath, csvBuilder.ToString());

                    asyncResult.Success = true;
                    asyncResult.Object = fullTemplatePath;
                    return Task.FromResult(asyncResult);
                }
                catch (Exception exc)
                {
                    var exc2Use = exc.GetBaseException();

                    Logger.Write(exc2Use, TraceEventType.Error);

                    asyncResult.Success = false;
                    asyncResult.Object = null;
                    asyncResult.Exception = exc2Use;

                    return Task.FromResult(asyncResult); ;
                }

            }, ctx);

            return result;
        }

        #endregion
    }

    public sealed class TemplateColumnDefinition
    {
        public string Name { get; set; }
        public Type DataType { get; set; }
        public bool IsReguired { get; set; }
    }

    public sealed class TemplateRow
    {
        public IList<object> Values { get; set; }
    }

    public sealed class ExportResult
    {
        public bool Success { get; set; }
        public object Object { get; set; }
        public Exception Exception { get; set; }
    }
}
