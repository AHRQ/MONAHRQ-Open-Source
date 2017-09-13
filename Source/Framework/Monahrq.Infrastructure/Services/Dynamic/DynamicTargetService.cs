using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.Flutters;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Extensibility;
using Monahrq.Sdk.Extensibility.ContentManagement.Records;
using Monahrq.Sdk.Extensibility.Data.Migration;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;
using ReflectionHelper = Monahrq.Infrastructure.Utility.Extensions.ReflectionHelper;
using Version = Monahrq.Infrastructure.Domain.Wings.Version;

namespace Monahrq.Infrastructure.Services.Dynamic
{
    /// <summary>
    /// The dynamic open source target wing constants.
    /// </summary>
    public static class DynamicTargetWingConstants
    {
        /// <summary>
        /// The wing module unique identifier
        /// </summary>
        public const string WING_MODULE_GUID = "C29B7B3E-1F28-41B8-87E1-AD7439C49B41";
        /// <summary>
        /// The wing module name
        /// </summary>
        public const string WING_MODULE_NAME = "Dynamic Targets Module";
        /// <summary>
        /// The wing module description
        /// </summary>
        public const string WING_MODULE_DESCRIPTION = "Provides Services, import and report generation for open source wing target Data";
        /// <summary>
        /// The wing depends on module names
        /// </summary>
        public static readonly string[] WingDependsOnModuleNames = new[] { "Base Data" };
        /// <summary>
        /// The wing module order
        /// </summary>
        public const int WING_MODULE_ORDER = 999;
    }

    /// <summary>
    /// The dynamic open source dataset target service.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Services.Dynamic.IDynamicTargetService" />
    [Export(typeof(IDynamicTargetService)), 
     PartCreationPolicy(CreationPolicy.Shared)]
    public class DynamicTargetService : IDynamicTargetService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicTargetService"/> class.
        /// </summary>
        public DynamicTargetService()
        {
            InstalledDynamicTargets = new ObservableCollection<DynamicTarget>();
        }

        #region Imports
        /// <summary>
        /// The context
        /// </summary>
        Lazy<IMonahrqShell> _context;

        //[Import]
        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        public Lazy<IMonahrqShell> Context
        {
            get
            {
                if (_context == null && MonahrqContext.MonahrqShell != null)
                    _context = new Lazy<IMonahrqShell>(() => MonahrqContext.MonahrqShell);

                if (_context == null)
                    _context = InitializeMonahrqShell();

                return _context;
            }
        }

        /// <summary>
        /// Initializes the monahrq shell.
        /// </summary>
        /// <returns></returns>
        private Lazy<IMonahrqShell> InitializeMonahrqShell()
        {
            return new Lazy<IMonahrqShell>(() => ServiceLocator.Current.GetInstance<IMonahrqShell>(), true);
        }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        [Import]
        IDomainSessionFactoryProvider Provider { get; set; }

        /// <summary>
        /// Gets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        [Import]
        IConfigurationService ConfigService { get; set; }

        /// <summary>
        /// Gets the operations logger.
        /// </summary>
        /// <value>
        /// The operations logger.
        /// </value>
        [Import(LogNames.Session, typeof(ILogWriter))]
        ILogWriter Logger
        {
            get;
            set;
        }

        #endregion

        #region Wings Related

        #region Properties

        /// <summary>
        /// Gets or sets the installed dynamic wing targets.
        /// </summary>
        /// <value>
        /// The installed dynamic wings.
        /// </value>
        public ObservableCollection<DynamicTarget> InstalledDynamicTargets { get; set; }

        private string _wingsFolder;

        /// <summary>
        /// Gets the wings folder.
        /// </summary>
        /// <value>
        /// The wings folder.
        /// </value>
        public string WingsFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_wingsFolder)) {
                    _wingsFolder = Path.Combine(MonahrqContext.MyDocumentsApplicationDirPath, @"Custom\Wings\");
                    if (!Directory.Exists(_wingsFolder))
                        Directory.CreateDirectory(_wingsFolder);
                }
                return _wingsFolder;
            }
        }

        #endregion

        /// <summary>
        /// Gets all the installed open source wing targets.
        /// </summary>
        /// <returns></returns>
        public bool GetInstalledOSTargets()
        {
            bool result;
            try
            {
                //var wingXmls = Directory.GetFiles(WingsFolder, "*.xml", SearchOption.AllDirectories);
                var wingXmls = SearchforFiles(WingsFolder);
                wingXmls.ToList().ForEach(wingXml =>
                    {
                        var dynamicTarget = Deserialize(wingXml);

                        if (InstalledDynamicTargets.All(t => t.Id != dynamicTarget.Id))
                            InstalledDynamicTargets.Add(dynamicTarget);
                    });

                result = true;
            }
            catch (Exception)
            {

                result = false;
            }
            return result;
        }

        /// <summary>
        /// Searchfors the files.
        /// </summary>
        /// <param name="searchPath">The search path.</param>
        /// <param name="searchCriteria">The search criteria.</param>
        /// <returns></returns>
        private IEnumerable<string> SearchforFiles(string searchPath, string searchCriteria = "*.xml") 
        {
            var wingXmls = Directory.GetFiles(searchPath, searchCriteria, SearchOption.AllDirectories).ToList();
            return wingXmls;
        }

        /// <summary>
        /// Deserializes the specified XML path.
        /// </summary>
        /// <param name="xmlPath">The XML path.</param>
        /// <returns></returns>
        private DynamicTarget Deserialize(string xmlPath)
        {
            try
            {
                var xml = File.ReadAllText(xmlPath);

                if (xml.Contains("AudienceType=\"AllAudiences\""))
                    xml = xml.Replace("AudienceType=\"AllAudiences\"", "AudienceType=\"Professionals\"");

                DynamicTarget target;
                using (var rdr = new StringReader(xml))
                {
                    var ser = new XmlSerializer(typeof(DynamicTarget));
                    target = ser.Deserialize(rdr) as DynamicTarget;
                }

                if (target != null)
                {
                    target.WingTargetXmlFilePath = xmlPath.SubStrAfter(MonahrqContext.MyDocumentsApplicationDirPath);
                }

                return target;
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "Error deserializing dynamic target from XML file {0}", xmlPath);
                throw;
            }
        }

        /// <summary>
        /// Copies the files to directory.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public IEnumerable<DynamicTarget> CopyTargetFilesToDirectory(string filePath)
        {
            var targetsToInstall = new List<DynamicTarget>();

            if (string.IsNullOrEmpty(filePath)) return targetsToInstall;

            
            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists) return targetsToInstall;

            if (fileInfo.Extension.EqualsIgnoreCase(".xml"))
            {
                var fileName = fileInfo.Name;
                var installPath = Path.Combine(WingsFolder, fileName.Replace(fileInfo.Extension, null), fileName);
                var installDirecortyPath = Path.Combine(WingsFolder, fileName.Replace(fileInfo.Extension, null));

                if (!Directory.Exists(installDirecortyPath))
                    Directory.CreateDirectory(installDirecortyPath);

                fileInfo.CopyTo(installPath, overwrite: true);

                DynamicTarget dynamicTarget = Deserialize(installPath);

                if (targetsToInstall.All(t => t.Id != dynamicTarget.Id))
                    targetsToInstall.Add(dynamicTarget);

                return targetsToInstall;
            }

            if (fileInfo.Extension.EqualsIgnoreCase(".zip"))
            {
                //var fileName = fileInfo.Name;
                //var installPath = Path.Combine(WingsFolder, fileName);
                //var wingTempPath = Path.Combine(WingsFolder, "Temp");

                var zipWingDirecToryPath = Path.Combine(WingsFolder, fileInfo.Name.Replace(fileInfo.Extension, null));
                var wingTempFilePath = Path.Combine(zipWingDirecToryPath, fileInfo.Name);

                if (!Directory.Exists(zipWingDirecToryPath))
                    Directory.CreateDirectory(zipWingDirecToryPath);

                File.Copy(fileInfo.FullName, wingTempFilePath, overwrite: true);

                CompressionHelper.Extract(wingTempFilePath, zipWingDirecToryPath);

                var dynamicTargetXmlFiles = SearchforFiles(zipWingDirecToryPath);

                dynamicTargetXmlFiles.ToList().ForEach(targetXml =>
                    {
                        var dt = Deserialize(targetXml);

                        if (targetsToInstall.All(t => t.Id != dt.Id))
                            targetsToInstall.Add(dt);
                    });

                if (targetsToInstall.Any())
                {
                    // first add to collection
                    targetsToInstall.ForEach(dt =>
                        {
                            if (InstalledDynamicTargets.All(t => t.Id != dt.Id))
                                InstalledDynamicTargets.Add(dt);
                        });
                }

                return targetsToInstall;
            }

            return new List<DynamicTarget>();
        }

        /// <summary>
        /// Checks if installed already.
        /// </summary>
        /// <param name="dynamicTarget">The dynamic target.</param>
        /// <param name="target">The target.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        private bool CheckIfInstalledAlready(DynamicTarget dynamicTarget, out Target target, ISession session = null)
        {
            if (session != null)
            {
                target = session.Query<Target>()
                                .FirstOrDefault(t => t.IsCustom && t.Name.ToLower() == dynamicTarget.Name.ToLower());

                return target != null;
            }

            using (session = Provider.SessionFactory.OpenSession())
            {
                target = session.Query<Target>()
                                .FirstOrDefault(t => t.Name.ToLower() == dynamicTarget.Name.ToLower());

                return target != null;
            }
        }

        /// <summary>
        /// Installs the dynamic target.
        /// </summary>
        /// <param name="dynamicTarget">The dynamic target.</param>
        /// <param name="wing">The wing.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public OpenSourceInstallResult InstallDynamicTarget(DynamicTarget dynamicTarget, Wing wing, ISession session)
        {
            try
            {
                Target target;
                if(CheckIfInstalledAlready(dynamicTarget, out target, session))
                {
                    return new OpenSourceInstallResult { Status = OpenSourceInstallFlags.AlreadyExists, Target = target };
                }

                using (var tx = session.BeginTransaction())
                {
                    target = dynamicTarget.CreateTarget(wing);
                    target.LoadCustomElements(dynamicTarget);
                    target.LoadCustomScopes(dynamicTarget);

                    Thread.Sleep(1000);

                    session.SaveOrUpdate(wing);
                    session.Flush();

                    tx.Commit();
                }

                OnApplyDynamicTargetDatasetHints(target, dynamicTarget, session);

                return new OpenSourceInstallResult { Status = OpenSourceInstallFlags.Success, Target = target};

            }
            catch (Exception exc)
            {
                Logger.Write(exc, "Error installing dynamic target definition {0}", dynamicTarget.Name);

                var message = new StringBuilder();
                message.AppendFormat($@"An error occurred while trying to install {dynamicTarget.Name} open source wing.
Error Message: {exc.GetBaseException().Message}
If the error continues, please contact the wing developer for technical support:
    Publisher: {dynamicTarget.Publisher}
    Publisher Email: {dynamicTarget.PublisherEmail}");
                if (!string.IsNullOrEmpty(dynamicTarget.PublisherWebsite))
                    message.AppendLine($"\tPublisher Website: {dynamicTarget.PublisherWebsite}");

                return new OpenSourceInstallResult { Status = OpenSourceInstallFlags.Error, Message = message.ToString() };
            }
        }

        /// <summary>
        /// Installs the dynamic target asynchronous.
        /// </summary>
        /// <param name="dynamicTarget">The dynamic target.</param>
        /// <param name="wing">The wing.</param>
        /// <param name="session">The session.</param>
        /// <param name="monahrqShell">The monahrq shell.</param>
        /// <param name="progressCallback">The progress callback.</param>
        /// <param name="exceptionCallBack">The exception call back.</param>
        /// <returns></returns>
        public async Task<bool> InstallDynamicTargetAsync(DynamicTarget dynamicTarget, Wing wing, ISession session, IMonahrqShell monahrqShell, Action<OpenSourceInstallResult> progressCallback)
        {
            try
            {
                var result = InstallDynamicTarget(dynamicTarget, wing, session);

                switch (result.Status)
                {
                    case OpenSourceInstallFlags.AlreadyExists:
                        progressCallback(new OpenSourceInstallResult
                        {
                            Status = OpenSourceInstallFlags.AlreadyExists,
                            Message = string.Format("Wing \"{0}\" has already been installed.", dynamicTarget.Name),
                            Target = result.Target
                        });
                        return true;

                    case OpenSourceInstallFlags.Error:
                        progressCallback(new OpenSourceInstallResult
                        {
                            Status = OpenSourceInstallFlags.Error,
                            Message =
                                string.Format(
                                    "An error occurred while trying to install wing \"{0}\". Please try again and if the error persists, please contact technical assistance for help.",
                                    dynamicTarget.Name),
                            Target = null
                        });
                        return false;
                        
                    case OpenSourceInstallFlags.Success:
                        progressCallback(new OpenSourceInstallResult
                        {
                            Status = OpenSourceInstallFlags.StatusUpdate,
                            Message =
                                string.Format(
                                    "Start installation of associated measures, reports, wing dataset target tables for wing \"{0}\".",
                                    dynamicTarget.Name),
                            Target = null
                        });

                        if (result.Target != null)
                        {
                            progressCallback(new OpenSourceInstallResult
                            {
                                Status = OpenSourceInstallFlags.StatusUpdate,
                                Message =
                                    string.Format("Creating dataset target table \"[dbo].[{0}]\" for wing \"{1}\".",
                                        dynamicTarget.DbSchemaName, dynamicTarget.Name),
                                Target = null
                            });

                            ReconcileMigrations(dynamicTarget, monahrqShell);

                            await Task.Delay(250);

                            progressCallback(new OpenSourceInstallResult
                            {
                                Status = OpenSourceInstallFlags.StatusUpdate,
                                Message = string.Format("Importing measures for wing \"{0}\".", dynamicTarget.Name),
                                Target = null
                            });

                            ImportMeasures(result.Target, dynamicTarget, session);

                            await Task.Delay(250);

                            progressCallback(new OpenSourceInstallResult
                            {
                                Status = OpenSourceInstallFlags.StatusUpdate,
                                Message = string.Format("Importing reports for wing \"{0}\".", dynamicTarget.Name),
                                Target = null
                            });

                            ImportReports(dynamicTarget, session);

                            await Task.Delay(250);

                            progressCallback(new OpenSourceInstallResult
                            {
                                Status = OpenSourceInstallFlags.StatusUpdate,
                                Message = string.Format("Finalizing installation for wing \"{0}\".", dynamicTarget.Name),
                                Target = null
                            });

                            await Task.Delay(500);

                            progressCallback(new OpenSourceInstallResult
                            {
                                Status = OpenSourceInstallFlags.Success,
                                Message = string.Format("Installation for wing \"{0}\" successfully completed.",
                                    dynamicTarget.Name),
                                Target = result.Target
                            });
                        }
                        return true;

                    default:
                        return false; // this should never happen
                }
            }
            catch (Exception exc)
            {
                this.Logger.Write(exc, $"Error installing dynamic target {dynamicTarget.Name} from wing {wing.Name}");
                return false;
            }
        }


        #region Migrations
        /// <summary>
        /// Reconciles the migrations.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="monahrqSell">The monahrq sell.</param>
        public void ReconcileMigrations(DynamicTarget target, IMonahrqShell monahrqSell = null)
        {
            var context = monahrqSell ?? Context.Value; // ServiceLocator.Current.GetInstance<IMonahrqShell>();
            
            try
            {
                if (TestForContentType(target.DbSchemaName))
                {
                    Logger.Information("Updating module data objects: {0}", target.Name);
                    ExecuteContentExtensionMigrations(context);
                }
                Logger.Information("Create module data objects: {0}", target.Name);
                
                var migration = ServiceLocator.Current.GetInstance<ITargetMigration>();
                if(migration != null)
                {
                    var m = CreateMigration(migration.GetType(), context);
                    m.Target = target;
                    m.Create();
                }
            }
            finally
            {}
        }

        /// <summary>
        /// Creates the migration.
        /// </summary>
        /// <param name="migration">The migration.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static IDataMigration CreateMigration(Type migration, IMonahrqShell context)
        {
            var factoryType = typeof(MigrationFactory<>).MakeGenericType(migration);
            var factory = factoryType.GetConstructor(new[] { typeof(IMonahrqShell) }).Invoke(new object[] { context });
            var method = factoryType.GetMethod("CreateDataMigration");
            return method.Invoke(factory, new object[0]) as IDataMigration;
        }

        /// <summary>
        /// Executes the content extension migrations.
        /// </summary>
        /// <param name="context">The context.</param>
        private void ExecuteContentExtensionMigrations(IMonahrqShell context)
        {
            CreateMigration(typeof(Migrations), context).Create();
        }

        /// <summary>
        /// Tests the type of for content.
        /// </summary>
        /// <param name="targetDbName">Name of the target database.</param>
        /// <returns></returns>
        private bool TestForContentType(string targetDbName)
        {
            var settings = ConfigService.ConnectionSettings;
            return 0 < (int)settings.ExecuteScalar("select count(*) from INFORMATION_SCHEMA.TABLES where TABLE_NAME = @contenttype",
                                                    new Dictionary<string, object>
                                                    {
                                                        {"@contenttype", targetDbName}
                                                    });
        }
        #endregion

        #region Measures and Reports
        /// <summary>
        /// Imports the measures.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="dynamicTarget">The dynamic target.</param>
        /// <param name="session">The session.</param>
        public void ImportMeasures(Target target, DynamicTarget dynamicTarget, ISession session = null)
        {
            if (session != null && session.IsOpen)
            {
                ImportMeasurePrivate(target, dynamicTarget, session);
            }
            else
            {
                using (session = Provider.SessionFactory.OpenSession())
                {
                    ImportMeasurePrivate(target, dynamicTarget, session);
                }
            }
        }
        /// <summary>
        /// Imports the measure private.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="dynamicTarget">The dynamic target.</param>
        /// <param name="session">The session.</param>
        private void ImportMeasurePrivate(Target target, DynamicTarget dynamicTarget, ISession session)
        {
            try
            {
                var measures = dynamicTarget.Measures.ToList()
                                         .Select(dynamicMeasure => dynamicMeasure.CreateMeasure(dynamicTarget, target, session))
                                         .ToList();

                using (var trans = session.BeginTransaction())
                {
                    foreach (var measure in measures)
                    {
                        if (session.Query<Measure>().Any(m => string.Equals(m.Name, measure.Name, StringComparison.CurrentCultureIgnoreCase)))
                            continue;

                        session.SaveOrUpdate(measure);
                    }
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex, $"Error importing measures from dynamnic target {dynamicTarget.Name}");
            }
        }
        /// <summary>
        /// Imports the reports.
        /// </summary>
        /// <param name="dynamicTarget">The dynamic target.</param>
        /// <param name="session">The session.</param>
        public void ImportReports(DynamicTarget dynamicTarget, ISession session = null)
        {
            if (session != null && session.IsOpen)
            {
                ImportReportsPrivate(dynamicTarget, session);
            }
            else
            {
                using (session = Provider.SessionFactory.OpenSession())
                {
                    ImportReportsPrivate(dynamicTarget, session);
                }
            }
        }
        /// <summary>
        /// Imports the reports private.
        /// </summary>
        /// <param name="dynamicTarget">The dynamic target.</param>
        /// <param name="session">The session.</param>
        public void ImportReportsPrivate(DynamicTarget dynamicTarget, ISession session)
        {
            try
            {
                using (var trans = session.BeginTransaction())
                {
                    foreach (var manifest in dynamicTarget.Reports.ToList())
                    {
                        if (string.IsNullOrEmpty(manifest.RptId ?? "") || manifest.RptId.EqualsIgnoreCase(Guid.Empty.ToString()))
                            manifest.RptId = Guid.NewGuid().ToString();

                        if (session.Query<Report>().Any(r => r.IsDefaultReport && r.Name == manifest.Name)) continue;

                        var date = DateTime.Now;
                        var report = new Report(manifest)
                                        {
                                            IsDefaultReport = true,
                                            IsCustom = true,
                                            SkipAudit = true,
                                            Publisher = dynamicTarget.Publisher,
                                            PublisherEmail = dynamicTarget.PublisherEmail,
                                            PublisherWebsite = dynamicTarget.PublisherWebsite,
                                            Version = new Version {Number = dynamicTarget.Version},
                                            LastReportManifestUpdate = date,
                                            DateCreated = date, 

                                        };

                        // TODO: Add code to check for Measure dependences such as Report columns.
                        foreach (var column in report.Columns.ToList())
                        {
                            if (column.IsMeasure && !string.IsNullOrEmpty(column.MeasureCode))
                            {
                                var measureTitle = session.CreateCriteria<Measure>()
                                                          .Add(Restrictions.InsensitiveLike("Name", column.MeasureCode))
                                                          .SetProjection(Projections.Property("MeasureTitle.Plain"))
                                                          .FutureValue<string>().Value;

                                if (!string.IsNullOrEmpty(measureTitle))
                                    column.Name = measureTitle;
                            }
                        }

                        session.SaveOrUpdate(report);
                    }

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex, $"Error importing reports from dynamic target {dynamicTarget.Name}");
            }
        }
        #endregion

        /// <summary>
        /// Called when [apply dynamic target dataset hints].
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="dynamicTarget">The dynamic target.</param>
        /// <param name="session">The session.</param>
        public void OnApplyDynamicTargetDatasetHints(Target target, DynamicTarget dynamicTarget, ISession session = null)
        {
            if (session != null)
            {
                OnApplyDynamicTargetDatasetHintsPrivate(session, target, dynamicTarget);
            }
            else
            {
                using (session = Provider.SessionFactory.OpenSession())
                {
                    OnApplyDynamicTargetDatasetHintsPrivate(session, target, dynamicTarget);
                }
            }
        }

        /// <summary>
        /// Called when [apply dynamic target dataset hints private].
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="target">The target.</param>
        /// <param name="wingTarget">The wing target.</param>
        public void OnApplyDynamicTargetDatasetHintsPrivate(ISession session, Target target, DynamicTarget wingTarget)
        {
            //target.Elements

            foreach (var element in target.Elements)
            {
                var column = wingTarget.Columns.FirstOrDefault(e => e.Name.ToUpper() == element.Name.ToUpper());

                if (column == null) continue;

                element.MappingHints.ToList().Add(column.Description); // For now we will just do
            }

            using (var trans = session.BeginTransaction())
            {
                try
                {
                    session.SaveOrUpdate(target);
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    Logger?.Write(ex, "Error applying dynamic target dataset hints");
                    throw;
                }
            }
        }

        /// <summary>
        /// Uninstalls the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="statusCallback">The status callback.</param>
        /// <param name="exceptionCallback">The exception callback.</param>
        /// <returns></returns>
        public async Task<bool> Uninstall(Target target, CancellationToken cancellationToken, Action<OpenSourceUnInstallResult> statusCallback, Action<OpenSourceUnInstallResult> exceptionCallback)
        {
            return await Task.Run(() =>
            {
                using (var session = Provider.SessionFactory.OpenStatelessSession())
                {
                    using (var trans = session.BeginTransaction())
                    {
                        try
                        {
                            statusCallback(new OpenSourceUnInstallResult
                            {
                                Status = OpenSourceUnInstallFlags.StatusUpdate,
                                Message = "Start uninstalling related reports..."
                            });

                            UninstallReports(session, target);

                            statusCallback(new OpenSourceUnInstallResult
                            {
                                Status = OpenSourceUnInstallFlags.StatusUpdate,
                                Message = "Finished uninstalling related reports..."
                            });

                            statusCallback(new OpenSourceUnInstallResult
                            {
                                Status = OpenSourceUnInstallFlags.StatusUpdate,
                                Message = "Start uninstalling related measures..."
                            });

                            UninstallMeasures(session, target);

                            statusCallback(new OpenSourceUnInstallResult
                            {
                                Status = OpenSourceUnInstallFlags.StatusUpdate,
                                Message = "Finished uninstalling related measures..."
                            });

                            statusCallback(new OpenSourceUnInstallResult
                            {
                                Status = OpenSourceUnInstallFlags.StatusUpdate,
                                Message = "Start uninstalling related datasets..."
                            });

                            UnistallDatasets(session, target);

                            statusCallback(new OpenSourceUnInstallResult
                            {
                                Status = OpenSourceUnInstallFlags.StatusUpdate,
                                Message = "Finished uninstalling related datasets..."
                            });

                            statusCallback(new OpenSourceUnInstallResult
                            {
                                Status = OpenSourceUnInstallFlags.StatusUpdate,
                                Message = "Start uninstalling related wing targets..."
                            });

                            UninstallWingTarget(session, target);

                            statusCallback(new OpenSourceUnInstallResult
                            {
                                Status = OpenSourceUnInstallFlags.Success,
                                Message = "Finished uninstalling related wing targets..."
                            });

                            trans.Commit();

                            DeleteWingXmlfile(target);

                            return Task.FromResult(true);
                        }
                        catch (Exception exc)
                        {
                            trans.Rollback();
                            exceptionCallback(new OpenSourceUnInstallResult
                            {
                                Status = OpenSourceUnInstallFlags.Error,
                                Exception = exc,
                                Message = exc.GetBaseException().Message
                            });
                            return Task.FromResult(false);
                        }
                    }
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Uninstalls the reports.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="target">The target.</param>
        private void UninstallReports(IStatelessSession session, Target target)
        {
            var query = ReflectionHelper.ReadEmbeddedResourceFile(
                        GetType().Assembly, "Monahrq.Infrastructure.Services.Dynamic.Resources.Uninstall_DynamicTarget_Reports.sql");

            query = query.Replace("[@@DynamicTargetId@@]", target.Id.ToString())
                         .Replace("[@@DynamicTargetName@@]", target.Name);

            session.CreateSQLQuery(query)
                   .SetTimeout(1000)
                   .ExecuteUpdate();
        }

        /// <summary>
        /// Uninstalls the measures.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="target">The target.</param>
        private void UninstallMeasures(IStatelessSession session, Target target)
        {
            var query = ReflectionHelper.ReadEmbeddedResourceFile(
                        GetType().Assembly, "Monahrq.Infrastructure.Services.Dynamic.Resources.Uninstall_DynamicTarget_Measures.sql");

            query = query.Replace("[@@DynamicTargetId@@]", target.Id.ToString())
                         .Replace("[@@DynamicTargetName@@]", target.Name);

            session.CreateSQLQuery(query)
                   .SetTimeout(1000)
                   .ExecuteUpdate();
        }

        /// <summary>
        /// Unistalls the datasets.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="target">The target.</param>
        private void UnistallDatasets(IStatelessSession session, Target target)
        {
            var query = ReflectionHelper.ReadEmbeddedResourceFile(
                        GetType().Assembly, "Monahrq.Infrastructure.Services.Dynamic.Resources.Uninstall_DynamicTarget_Datasets.sql");

            query = query.Replace("[@@DynamicTargetId@@]", target.Id.ToString())
                         .Replace("[@@DynamicTargetName@@]", target.Name);

            session.CreateSQLQuery(query)
                   .SetTimeout(1000)
                   .ExecuteUpdate();
        }

        /// <summary>
        /// Uninstalls the wing target.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="target">The target.</param>
        private void UninstallWingTarget(IStatelessSession session, Target target)
        {
            var query = ReflectionHelper.ReadEmbeddedResourceFile(
                        GetType().Assembly, "Monahrq.Infrastructure.Services.Dynamic.Resources.Uninstall_DynamicTarget_WingTargets.sql");

            query = query.Replace("[@@DynamicTargetId@@]", target.Id.ToString())
                         .Replace("[@@DynamicTargetName@@]", target.Name);

            session.CreateSQLQuery(query)
                   .SetTimeout(1000)
                   .ExecuteUpdate();
        }

        /// <summary>
        /// Deletes the wing xmlfile.
        /// </summary>
        /// <param name="target">The target.</param>
        private static void DeleteWingXmlfile(Target target)
        {
            if (target == null || string.IsNullOrEmpty(target.WingTargetXmlFilePath)) return;

            var wingTargetFilePath = string.Format("{0}{1}", MonahrqContext.MyDocumentsApplicationDirPath, target.WingTargetXmlFilePath);

            var fileInfo = new FileInfo(wingTargetFilePath);

            if (!fileInfo.Exists || (fileInfo.Directory == null || !fileInfo.Directory.Exists)) return;

            foreach (var file in fileInfo.Directory.GetFiles().ToList())
                File.Delete(file.FullName);

            if(fileInfo.Directory.Exists)
                Directory.Delete(fileInfo.Directory.FullName, true);
        }
        #endregion

        #region Flutters Related

        #region Properties
        private string _fluttersFolder;

        /// <summary>
        /// Gets the flutters folder.
        /// </summary>
        /// <value>
        /// The flutters folder.
        /// </value>
        public string FluttersFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_fluttersFolder))
                {
                    _fluttersFolder = Path.Combine(MonahrqContext.MyDocumentsApplicationDirPath, @"Custom\Flutters\");

                    if (!Directory.Exists(_fluttersFolder))
                        Directory.CreateDirectory(_fluttersFolder);
                }
                return _fluttersFolder;
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Uploads the and extract flutter files.
        /// </summary>
        /// <param name="uploadFilePath">The upload file path.</param>
        /// <param name="ctx">The CTX.</param>
        /// <param name="statusCallback">The status callback.</param>
        /// <param name="exceptionCallback">The exception callback.</param>
        /// <returns></returns>
        public async Task<bool> InstallFlutterFiles(string uploadFilePath, CancellationToken ctx, 
            Action<OSFlutterInstallResult> statusCallback, Action<OSFlutterInstallResult> exceptionCallback)
        {
            return await Task.Run(async () =>
            {
                string installPath = null;
                Flutter flutter = null;
                try
                {
                    if (string.IsNullOrEmpty(uploadFilePath)) return true;

                    var fileInfo = new FileInfo(uploadFilePath);

                    if (!fileInfo.Exists) return false;

                    statusCallback(new OSFlutterInstallResult
                    {
                        Status = OSFlutterInstallFlags.StatusUpdate,
                        Message = string.Format("Start extraction \"{0}\" to Monahrq flutter directory.", fileInfo.Name)
                    });

                    var fileName = fileInfo.Name;
                    installPath = Path.Combine(FluttersFolder, fileName.Replace(fileInfo.Extension, null));

                    DeleteFlutterFiles(installPath);

                    var config = CopyFlutterFilesToDirectory(fileInfo, installPath);

                    if (config != null)
                    {
                        flutter = new Flutter
                        {
                            Name = config.DisplayName.Replace("Flutter", null),
                            InstallPath = installPath.Replace(MonahrqContext.MyDocumentsApplicationDirPath, null),
                            OutputPath = string.Format("flutters\\{0}", config.DisplayName.ToLower().Replace("Flutter", null).Replace(" ", "-")),
                            ConfigurationId = config.Id
                        };

                        statusCallback(new OSFlutterInstallResult
                        {
                            Status = OSFlutterInstallFlags.StatusUpdate,
                            Message = "Files successfully extracted to flutter directory.",
                            Config = config,
                            Flutter = flutter
                        });

                        var reportsResult = await GetReportsForFlutter(config);
                        if (reportsResult.Count == 0) {
                            statusCallback(new OSFlutterInstallResult {
                                Status = OSFlutterInstallFlags.StatusUpdate,
                                Message = "+++ You must have a Wing loaded for this Flutter. Installation cancelled. +++"
                            });
                            return false;
                        }
                        string reportsNames = null;
                        if (reportsResult != null && reportsResult.Any())
                        {
                            var applicableReports = reportsResult.ToObservableCollection();

                            reportsNames = applicableReports.Any()
                                ? string.Join(",", applicableReports.Select(r => r.ReportType))
                                : "N/A";

                            statusCallback(new OSFlutterInstallResult
                            {
                                Status = OSFlutterInstallFlags.StatusUpdate,
                                Message = string.Format("Flutter \"{0}\" will be mapped to the following wing reports: {1}",
                                                        config.DisplayName, reportsNames),
                                Config = config,
                                Flutter = flutter
                            });
                        }

                        statusCallback(new OSFlutterInstallResult
                        {
                            Status = OSFlutterInstallFlags.StatusUpdate,
                            Message = "Saving flutter and wing report mappings in flutter registry",
                            Config = config,
                            Flutter = flutter
                        });

                        flutter.AssociatedReportsTypes = !string.IsNullOrEmpty(reportsNames) ? reportsNames : null;
                        var saveResult = await SaveFlutter(flutter);

                        if (saveResult)
                        {
                            statusCallback(new OSFlutterInstallResult
                            {
                                Status = OSFlutterInstallFlags.Success,
                                Message = string.Format("Flutter \"{0}\" was successfully installed", flutter.Name),
                                Config = config,
                                Flutter = flutter
                            });
                        }
                        else
                        {
                            var message = string.Format("An error occurred while saving flutter \"{0}\" and wing report mapping. Rolling back changes.", flutter.Name);
                            throw new Exception(message);
                        }
                    }
                    else
                        throw new Exception("An error occurred during extracting of flutter files");

                    return true;
                }
                catch (Exception exc)
                {
                    if (!string.IsNullOrEmpty(installPath))
                    {
                        var dirInfo = new DirectoryInfo(installPath);
                        DeleteFlutterFiles(dirInfo.FullName);
                    }

                    if (flutter != null)
                    {
                        using (var session = Provider.SessionFactory.OpenSession())
                        {
                            using (var trans = session.BeginTransaction())
                            {
                                session.Delete(flutter);
                                trans.Commit();
                            }
                        }
                    }

                    exceptionCallback(new OSFlutterInstallResult
                    {
                        Status = OSFlutterInstallFlags.Error,
                        Message = exc.GetBaseException().Message
                    });
                    return false;
                }

            }, ctx);
        }

        /// <summary>
        /// Deletes the flutter files.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        private void DeleteFlutterFiles(string directoryPath)
        {
            if (directoryPath == null) return;

            var dirInfo = new DirectoryInfo(directoryPath);

            if (!dirInfo.Exists) return;

            foreach (var file in dirInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList())
            {
                file.Delete();
            }

            dirInfo.Delete(true);
        }

        /// <summary>
        /// Gets the reports for flutter.
        /// </summary>
        /// <param name="flutterConfig">The flutter configuration.</param>
        /// <returns></returns>
        public async Task<List<Report>> GetReportsForFlutter(FlutterConfig flutterConfig)
        {
            var reports = new List<Report>();
            using (var session = Provider.SessionFactory.OpenSession())
            {
                foreach (var report in flutterConfig.Reports.Where(report => report.Type != null))
                {
                    var reportNames = session.CreateCriteria<Report>()
                                             .Add(Restrictions.InsensitiveLike("ReportType", report.Type))
                                             .Add(Restrictions.Eq("IsCustom", true))
                                             .SetProjection(Projections.ProjectionList()
                                                            .Add(Projections.Alias(Projections.Property("Id"), "Id"))
                                                            .Add(Projections.Alias(Projections.Property("Name"), "Name"))
                                                            .Add(Projections.Alias(Projections.Property("ReportType"), "ReportType")))
                                            .SetResultTransformer(new AliasToBeanResultTransformer(typeof(Report)))
                                            .Future<Report>().ToList();

                    reports.AddRange(reportNames);
                }
            }

            return await Task.FromResult(reports.DistinctBy(x => x.Name).ToList());
        }

        /// <summary>
        /// Copies the flutter files to directory.
        /// </summary>
        /// <param name="extractFileInfo">The extract file information.</param>
        /// <param name="installPath">The install path.</param>
        /// <returns></returns>
        public FlutterConfig CopyFlutterFilesToDirectory(FileInfo extractFileInfo, string installPath)
        {
            if (extractFileInfo.Extension.EqualsIgnoreCase(".zip"))
            {
                CompressionHelper.Extract(extractFileInfo.FullName, installPath);

                var flutterConfig = SearchforFiles(installPath, "*-config.js") ??
                                    SearchforFiles(installPath, "*_config.js");

                if (flutterConfig == null || !flutterConfig.Any())
                    return null;

                var jsonString = File.ReadAllText(flutterConfig.First());

                jsonString = jsonString.Trim();

                var stringToDeserialize = jsonString.SubStrAfter("{");
                if (!stringToDeserialize.StartsWith("{"))
                    stringToDeserialize = "{" + stringToDeserialize;

                if (stringToDeserialize.EndsWith(";"))
                    stringToDeserialize = stringToDeserialize.Replace(";", null);

                return JsonHelper.Deserialize<FlutterConfig>(stringToDeserialize);
            }

            return null;
        }

        /// <summary>
        /// Saves the flutter.
        /// </summary>
        /// <param name="flutter">The flutter.</param>
        /// <returns></returns>
        public async Task<bool> SaveFlutter(Flutter flutter)
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    try
                    {
                        var flutterToSave = session.Query<Flutter>()
                                                   .FirstOrDefault(f => f.Name.ToUpper() == flutter.Name.ToUpper());

                        if (flutterToSave != null)
                        {
                            flutterToSave.Name = flutter.Name;
                            flutterToSave.ConfigurationId = flutter.ConfigurationId;
                            flutterToSave.AssociatedReportsTypes = flutter.AssociatedReportsTypes;
                            flutterToSave.InstallPath = flutter.InstallPath;
                            flutterToSave.OutputPath = flutter.OutputPath;
                        }
                        else
                            flutterToSave = flutter;

                        session.SaveOrUpdate(flutterToSave);

                        trans.Commit();

                        return await Task.FromResult(true);
                    }
                    catch (Exception exc)
                    {
                        Logger.Write(exc, $"Error saving flutter {flutter.Name}");
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Uninstalls the specified flutter.
        /// </summary>
        /// <param name="flutter">The flutter.</param>
        /// <param name="ctx">The CTX.</param>
        /// <param name="resultCallback">The result callback.</param>
        /// <param name="exceptionCallback">The exception callback.</param>
        /// <returns></returns>
        public async Task<bool> Uninstall(Flutter flutter, CancellationToken ctx, Action<OpenSourceUnInstallResult> resultCallback, Action<OpenSourceUnInstallResult> exceptionCallback)
        {
            return Task<bool>.Run(() =>
            {
                using (var session = Provider.SessionFactory.OpenSession())
                {
                    using (var trans = session.BeginTransaction())
                    {
                        try
                        {
                            var flutterName = flutter.Name;
                            var assocRpt = flutter.AssociatedReportsTypes;
                            resultCallback(new OpenSourceUnInstallResult
                            {
                                Status = OpenSourceUnInstallFlags.StatusUpdate,
                                Message = string.Format("Removing flutter \"{0}\" to wing report \"{1}\" mapping registry entry.", flutterName, assocRpt)
                            });

                            Task.Delay(500, ctx);

                            session.Delete(flutter);

                            resultCallback(new OpenSourceUnInstallResult
                            {
                                Status = OpenSourceUnInstallFlags.StatusUpdate,
                                Message = string.Format("Flutter \"{0}\" to wing report \"{1}\" mapping registry entry removal complete.", flutterName, assocRpt)
                            });

                            var flutterFilePath = string.Concat(MonahrqContext.MyDocumentsApplicationDirPath, flutter.InstallPath);

                            if (!Directory.Exists(flutterFilePath))
                                throw new Exception(
                                    "The flutter directory has been removed and/or you do not have permissions.");

                            Task.Delay(500, ctx);

                            resultCallback(new OpenSourceUnInstallResult
                            {
                                Status = OpenSourceUnInstallFlags.Success,
                                Message = string.Format("Removing files for flutter \"{0}\".", flutterName)
                            });

                            DeleteFlutterFiles(flutterFilePath);

                            resultCallback(new OpenSourceUnInstallResult
                            {
                                Status = OpenSourceUnInstallFlags.Success,
                                Message = string.Format("Removal of files for flutter \"{0}\" have been successfully deleted.", flutterName)
                            });

                            trans.Commit();

                            Task.Delay(500, ctx);

                            return Task.FromResult(true);
                        }
                        catch (Exception exc)
                        {
                            trans.Rollback();

                            Logger.Write(exc, $"Error uninstalling flutter {flutter.Name}");

                            exceptionCallback(new OpenSourceUnInstallResult
                            {
                                Status = OpenSourceUnInstallFlags.Error,
                                Message = exc.GetBaseException().Message
                            });

                            return Task.FromResult(false);
                        }
                    }
                }
            }, ctx).Result;
        }

        #endregion

        #endregion
    }

    #region Wing Related
    /// <summary>
    /// The open source install result class.
    /// </summary>
    public class OpenSourceInstallResult
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public OpenSourceInstallFlags Status { get; set; }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public Target Target { get; set; }
    }

    /// <summary>
    /// The dynamic open source install flags enumeration.
    /// </summary>
    public enum OpenSourceInstallFlags
    {
        /// <summary>
        /// The already exists
        /// </summary>
        AlreadyExists,
        /// <summary>
        /// The error
        /// </summary>
        Error,
        /// <summary>
        /// The status update
        /// </summary>
        StatusUpdate,
        /// <summary>
        /// The success
        /// </summary>
        Success
    }

    /// <summary>
    /// The dynamic open source uninstall result class.
    /// </summary>
    public class OpenSourceUnInstallResult
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public OpenSourceUnInstallFlags Status { get; set; }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// The dynamic open source uninstall flags enumeration.
    /// </summary>
    public enum OpenSourceUnInstallFlags
    {
        /// <summary>
        /// The status update
        /// </summary>
        StatusUpdate,
        /// <summary>
        /// The error
        /// </summary>
        Error,
        /// <summary>
        /// The success
        /// </summary>
        Success
    }
    #endregion

    #region Flutter Related
    /// <summary>
    /// The open source flutter instal result.
    /// </summary>
    public class OSFlutterInstallResult
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public OSFlutterInstallFlags Status { get; set; }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
        /// <summary>
        /// Gets or sets the flutter.
        /// </summary>
        /// <value>
        /// The flutter.
        /// </value>
        public Flutter Flutter { get; set; }
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public FlutterConfig Config { get; set; }
    }

    /// <summary>
    /// The open source flutter install flags.
    /// </summary>
    public enum OSFlutterInstallFlags
    {
        /// <summary>
        /// The error status flag.
        /// </summary>
        Error,
        /// <summary>
        /// The not installed status flag.
        /// </summary>
        NotInstalled,
        /// <summary>
        /// The upload file complete status flag.
        /// </summary>
        UploadFileComplete,
        /// <summary>
        /// The flutter wing association complete status flag.
        /// </summary>
        FlutterWingAssociationComplete,
        /// <summary>
        /// The status update status flag.
        /// </summary>
        StatusUpdate,
        /// <summary>
        /// The success status flag.
        /// </summary>
        Success
    }
    #endregion
}
