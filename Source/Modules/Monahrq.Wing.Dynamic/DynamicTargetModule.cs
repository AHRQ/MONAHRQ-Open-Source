using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Modularity;
using Monahrq.DataSets.Model;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Exceptions;
using Monahrq.Infrastructure.Services.Dynamic;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Sdk.Utilities;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Dynamic.Models;
using NHibernate;
using NHibernate.Linq;

namespace Monahrq.Wing.Dynamic
{
    /// <summary>
    /// Class for Dynamic module
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Modules.Wings.WingModule" />
    [WingModule(typeof(DynamicTargetModule), 
        DynamicTargetWingConstants.WING_MODULE_GUID, 
        DynamicTargetWingConstants.WING_MODULE_NAME,
        DynamicTargetWingConstants.WING_MODULE_DESCRIPTION,
        DependsOnModuleNames = new[] { "Base Data" }, InitializationMode = InitializationMode.WhenAvailable,
        DisplayOrder = DynamicTargetWingConstants.WING_MODULE_ORDER)]
    public class DynamicTargetModule : WingModule
    {
        /// <summary>
        /// Gets or sets the dynamic target service.
        /// </summary>
        /// <value>
        /// The dynamic target service.
        /// </value>
        [Import]
        protected IDynamicTargetService DynamicTargetService { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicTargetModule"/> class.
        /// </summary>
        public DynamicTargetModule()
        {}

        /// <summary>
        /// Called when initialized.
        /// </summary>
        protected override void OnInitialize()
        {
            DynamicTargetService.GetInstalledOSTargets();

            base.OnInitialize();
            Subscribe();
        }

        /// <summary>
        /// Called when target is added.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="target">The target.</param>
        /// <param name="wingTarget">The wing target.</param>
        private void OnTargetAdded(ISession session, Target target, DynamicTarget wingTarget)
        {
            OnWingAdded();

            DynamicTargetService.ReconcileMigrations(wingTarget);
            DynamicTargetService.ImportMeasures(target, wingTarget, session);
            DynamicTargetService.ImportReports(wingTarget, session);
        }

        /// <summary>
        /// Reconciles this instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="MonahrqCoreException"></exception>
        protected override bool Reconcile()
        {
            var instalResult = true;
            Events.GetEvent<MessageUpdateEvent>()
                  .Publish(new MessageUpdateEvent
                  {
                      Message = "Loading Open Source Wings"
                  });

            try
            {
                //Check that wing is not already registered
                using (var session = Provider.SessionFactory.OpenSession())
                {
                    Infrastructure.Entities.Domain.Wings.Wing wing;

                    if (session.Query<Infrastructure.Entities.Domain.Wings.Wing>()
                               .Any<Infrastructure.Entities.Domain.Wings.Wing>(w => w.WingGUID == new Guid(DynamicTargetWingConstants.WING_MODULE_GUID)))
                    {
                        wing = session.Query<Infrastructure.Entities.Domain.Wings.Wing>()
                                      .FirstOrDefault(w => w.WingGUID == new Guid(DynamicTargetWingConstants.WING_MODULE_GUID));
                    }
                    else
                    {
                        wing = this.FactoryWing();
                        session.Save(wing);
                    }

                    foreach (var wingTarget in DynamicTargetService.InstalledDynamicTargets.ToList())
                    {
                        Events.GetEvent<MessageUpdateEvent>()
                              .Publish(new MessageUpdateEvent
                              {
                                  Message = "Loading " + Inflector.Titleize2(wingTarget.Name)
                              });

                        var result = DynamicTargetService.InstallDynamicTarget(wingTarget, wing, session);

                        switch (result.Status)
                        {
                            case OpenSourceInstallFlags.Error:
                                throw new MonahrqCoreException(result.Message);
                            case OpenSourceInstallFlags.AlreadyExists:
                                continue;
                            case OpenSourceInstallFlags.Success:
                                var target = result.Target;

                                if (target != null)
                                {
                                    OnTargetAdded(session, target, wingTarget);
                                }
                                break;
                        }

                        //DynamicTargetService.ImportMeasures(target, wingTarget, session);
                        //DynamicTargetService.ImportReports(wingTarget, session);

                        //Target target;

                        //var contentTarget = session.Query<Target>()
                        //                           .FirstOrDefault(t => t.Name.ToLower() == wingTarget.Name.ToLower());

                        //using (var tx = session.BeginTransaction())
                        //{

                        //    if (contentTarget != null &&
                        //        session.Query<Target>().Any(w => w.Name.ToUpper() == contentTarget.Name.ToUpper() &&
                        //                                         w.Version.Number.ToUpper() == contentTarget.Version.Number.ToUpper()))
                        //    {
                        //        continue;
                        //    }

                        //    target = wingTarget.CreateTarget(wing);
                        //    target.LoadCustomElements(wingTarget);
                        //    target.LoadCustomScopes(wingTarget);

                        //    Thread.Sleep(1000);

                        //    session.SaveOrUpdate(wing);
                        //    session.Flush();

                        //    tx.Commit();
                        //}

                        //OnTargetAdded(session, target, wingTarget);
                        //DynamicTargetService.OnApplyDynamicTargetDatasetHints(target, wingTarget, session);

                        //if (target != null)
                        //{
                        //    DynamicTargetService.ReconcileMigrations(wingTarget);
                        //}

                        //instalResult = true;
                    }
                }
            }
            catch (Exception exc)
            {
                base.Logger.Write(exc, "Error reconciling dynamic module {0}", this.Description);
                instalResult = false;
            }

            return instalResult;
        }

        //private void OnApplyDatasetHints(ISession session, Target target, DynamicTarget wingTarget)
        //{
        //    //target.Elements

        //    foreach (var element in target.Elements)
        //    {
        //        var column = wingTarget.Columns.FirstOrDefault(e => e.Name.ToUpper() == element.Name.ToUpper());

        //        if (column == null) continue;

        //        element.MappingHints.ToList().Add(column.Description); // For now we will just do
        //    }

        //    using (var trans = session.BeginTransaction())
        //    {
        //        try
        //        {
        //            session.SaveOrUpdate(target);
        //            trans.Commit();
        //        }
        //        catch (Exception ex)
        //        {
        //            if (Logger != null)
        //                Logger.Write(ex.InnerException ?? ex);
        //            throw;
        //        }
        //    }
        //}

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        [Import(LogNames.Session)]
        private ILogWriter Logger { get; set; }

        /// <summary>
        /// Gets or sets the measure SVC.
        /// </summary>
        /// <value>
        /// The measure SVC.
        /// </value>
        [Import]
        private IMeasureService MeasureSvc { get; set; }

        /// <summary>
        /// Factories the wizard steps.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <returns></returns>
        protected IStepCollection FactoryWizardSteps(DataTypeModel model, int? datasetId)
        {
            return new WizardSteps(model, datasetId);
        }

        /// <summary>
        /// Subscribes this instance.
        /// </summary>
        private void Subscribe()
        {
            Events.GetEvent<WizardStepsRequestEvent<DataTypeModel, Guid, int?>>()
                  .Subscribe(args =>
                      {
                          if (args.WingId != new Guid(DynamicTargetWingConstants.WING_MODULE_GUID) || !args.Data.Target.IsCustom) return;

                          args.WizardSteps = FactoryWizardSteps(args.Data, args.ExistingDatasetId);
                      });
        }

        /// <summary>
        /// Installs the database.
        /// </summary>
        /// <returns></returns>
        public override bool InstallDb()
        {
            return true;
        }

        ///// <summary>
        ///// Imports the measures.
        ///// </summary>
        ///// <param name="session">The session.</param>
        ///// <param name="target">The target.</param>
        ///// <param name="wingTarget">The wing target.</param>
        //private void ImportMeasures(ISession session, Target target, DynamicTarget wingTarget)
        //{
        //    try
        //    {
        //        var measures = wingTarget.Measures.ToList()
        //                                 .Select(dynamicMeasure => dynamicMeasure.CreateMeasure(wingTarget, target, session))
        //                                 .ToList();

        //        using (var trans = session.BeginTransaction())
        //        {
        //            foreach (var measure in measures)
        //            {
        //                if (session.Query<Measure>().Any(m => m.Name.ToLower() == measure.Name.ToLower()))
        //                    continue;

        //                session.SaveOrUpdate(measure);
        //            }
        //            trans.Commit();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Write(ex);
        //    }
        //}

        //private void ImportReports(ISession session, DynamicTarget wingTarget)
        //{
        //    try
        //    {
        //        //if (!string.IsNullOrEmpty(wingTarget.AddReportsScript ?? string.Empty))
        //        //{
        //        //    session.CreateSQLQuery(wingTarget.AddReportsScript)
        //        //           .ExecuteUpdate();
        //        //}
        //        //else
        //        //{
        //        using (var trans = session.BeginTransaction())
        //        {
        //            foreach (var manifest in wingTarget.Reports.ToList())
        //            {
        //                if (string.IsNullOrEmpty(manifest.RptId ?? "") || manifest.RptId.EqualsIgnoreCase(System.Guid.Empty.ToString()))
        //                    manifest.RptId = System.Guid.NewGuid().ToString();

        //                var report = new Report(manifest) { IsDefaultReport = true , IsCustom = true };

        //                // TODO: Add code to check for Measure dependences such as Report columns.
        //                report.Publisher = wingTarget.Publisher;
        //                report.PublisherEmail = wingTarget.PublisherEmail;
        //                report.PublisherWebsite = wingTarget.PublisherWebsite;
        //                report.Version = new Version { Number = wingTarget.Version };
        //                report.SkipAudit = true;
                            
        //                if (session.Query<Report>().Any(r => r.IsDefaultReport && r.Name == manifest.Name)) continue;

        //                session.SaveOrUpdate(report);
        //            }

        //            trans.Commit();
        //        }
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Write(ex);
        //    }
        //}

        //private void ReconcileMigrations(IMonahrqShell context, DynamicTarget target)
        //{
        //    var migrationImple = typeof(DataMigrationImpl);
        //    if (TestForContentType(target.DbSchemaName))
        //    {
        //        OperationsLogger.Write(string.Format("Updating module data objects: {0}", target.Name), Category.Info, Priority.Medium);
        //        ExecuteContentExtensionMigrations(context);
        //    }
        //    OperationsLogger.Write(string.Format("Create module data objects: {0}", target.Name), Category.Info, Priority.Medium);
        //    var migrations = GetType().Assembly.ExportedTypes.Where(t => !t.IsAbstract && migrationImple.IsAssignableFrom(t)).ToList();

        //    foreach (var migration in migrations)
        //    {
        //        var m = CreateMigration(migration, context);
        //        m.Target = target; 
        //        m.Create();
        //    }

            //if(target == null) // Not a dynamic wing target.
            //    context.SessionFactoryHolder.Reinitialize();
        //}

        //IDataMigration CreateMigration(Type migration, IMonahrqShell context)
        //{
        //    var factoryType = typeof(MigrationFactory<>).MakeGenericType(migration);
        //    var factory = factoryType.GetConstructor(new[] { typeof(IMonahrqShell) }).Invoke(new[] { context });
        //    var method = factoryType.GetMethod("CreateDataMigration");
        //    return method.Invoke(factory, new object[0]) as IDataMigration;
        //}

        //private void ExecuteContentExtensionMigrations(IMonahrqShell context)
        //{
        //    CreateMigration(typeof(Migrations), context).Create();
        //}

        //private bool TestForContentType(string targetDbName)
        //{
        //    var settings = ConfigurationService.ConnectionSettings;
        //    // var sql = "select count(*) from INFORMATION_SCHEMA.TABLES where TABLE_NAME = @contenttype";
        //    return 0 < (int)settings.ExecuteScalar("select count(*) from INFORMATION_SCHEMA.TABLES where TABLE_NAME = @contenttype",
        //                                            new Dictionary<string, object>
        //                                            {
        //                                                {"@contenttype", targetDbName}
        //                                            });
        //}
    }


}
