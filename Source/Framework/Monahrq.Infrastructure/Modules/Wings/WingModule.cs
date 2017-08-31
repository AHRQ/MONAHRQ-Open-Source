using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Modularity;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Extensions;
using Monahrq.Infrastructure.Entities.Domain;
using NHibernate.Linq;

namespace Monahrq.Sdk.Modules.Wings
{
    /// <summary>
    /// Provides key points of interaction between Monahrq and it's plugins
    /// </summary>
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public abstract class WingModule : IModule, IWingModule, IModuleInstaller, IPartImportsSatisfiedNotification
    {
        protected WingModuleAttribute _wingAttribute;
        //private bool _isLoaded = false;

        /// <summary>
        /// Gets the session logger.
        /// </summary>
        [Import(LogNames.Session, typeof(ILogWriter))]
        protected ILogWriter Logger
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="IEventAggregator"/> instance used throughout MONAHRQ
        /// </summary>
        [Import]
        protected IEventAggregator Events
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the configuration service.
        /// </summary>
        [Import]
        protected IConfigurationService ConfigurationService
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Gets or sets the ORM provider factory
        /// </summary>
        [Import]
        protected IDomainSessionFactoryProvider Provider
        {
            get;
            set;
        }


        /// <summary>
        /// The names of modules that should be reformatted as follows: the "Data" suffix is replaced with the word "module"
        /// </summary>
        private readonly string[] _moduleNamesToFormat = new[]
            {
                "Inpatient Data", "Treat and Release Discharge Data", "Hospital Compare Data",
                "Medicare Provider Charge Data", "AHRQ-QI Area Data", "AHRQ-QI Composite Data", "AHRQ-QI Provider Data"
            };

        /// <summary>
        /// Prepares this <see cref="WingModule"/> for use
        /// </summary>
        public async void Initialize()
        {
            try
            {
                _wingAttribute = GetType().GetCustomAttribute<WingModuleAttribute>();

                var moduleName = (!_wingAttribute.ModuleName.In(_moduleNamesToFormat) && !_wingAttribute.ModuleName.EndsWith("Data"))
                                     ? _wingAttribute.ModuleName
                                     : string.Format("{0} module", _wingAttribute.ModuleName.SubStrBeforeLast("Data").TrimEnd());

                Logger.Debug($"Loading {moduleName}...");
                Events.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading " + moduleName });

                if (MonahrqContext.ForceDbUpGrade || MonahrqContext.ForceDbRecreate) return;

                var sw = new Stopwatch();
                sw.Start();
                await Task.Factory.StartNew(this.OnInitialize);
                sw.Stop();
                Logger.Information($"Load of module \"{moduleName}\" completed in {sw.Elapsed}");
            }
            finally
            {
                //Events.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = string.Empty });
                Application.Current.DoEvents();
            }
        }

        /// <summary>
        /// Called after <see cref="Initialize"/>; wraps <see cref="Reconcile"/> to execute installation logic on startup
        /// </summary>
        protected virtual void OnInitialize()
        {
            //Events.GetEvent<MessageUpdateEvent>()
            //     .Publish(new MessageUpdateEvent { Message = "Loading " + _wingAttribute.ModuleName });
            //Application.Current.DoEvents();
            if (_wingAttribute == null)
            {
                Logger.Warning("{0} is not attributed as a wing module", this.GetType().Name);
                return;
            }
            try
            {
                Reconcile();
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "An error occurred in {0} Module while reconciling. Error: {1}", _wingAttribute.ContractName, ex.Message);
            }
        }

        /// <summary>
        /// Installs or updates this <see cref="WingModule"/>
        /// </summary>
        /// <remarks>
        /// Called by <see cref="OnInitialize"/>
        /// </remarks>
        /// <returns>A boolean value indicating whether the operation was successful</returns>
        protected virtual bool Reconcile()
        {
            bool instalResult;

           // var contentTypes = new List<Target>();
            try
            {
                //Check that wing is not already registered
                using (var session = Provider.SessionFactory.OpenSession())
                {
                    using (var tx = session.BeginTransaction())
                    {

                        var wing= session.Query<Wing>().FirstOrDefault(w => w.WingGUID == WingGUID && w.Name.ToUpper() == _wingAttribute.ModuleName.ToUpper());

                        #region Old Code
                        //                 contentTypes = session.Query<Wing>()
                        //                                       .Where(w => w.WingGUID == WingGUID)
                        //                                       .SelectMany(w => w.Targets).ToList();

                        //                 if (contentTypes.Any() && session.Query<Wing>().Any(w => w.WingGUID == WingGUID))
                        //                 {
                        //if (OnWingRefreshed())
                        //{
                        //	var wingx = session.Query<Wing>().FirstOrDefault(w => w.WingGUID == WingGUID);
                        //	if (wingx == null) return false;

                        //	wingx.LastWingUpdate = DateTime.Now;
                        ////	wingx.LoadTargets(this, ForceNamespaceRestriction ? null : session);	//	Needed?
                        ////	wingx.Targets.ToList().ForEach(t => this.LoadTarget(t));				//	Needed?
                        //	session.SaveOrUpdate(wingx);
                        //	tx.Commit();
                        //}

                        //                     //_isLoaded = true;
                        //                     return false;
                        //                 }
                        #endregion

                        if (wing != null /* || wing.Targets.Any()*/)
                        {
                            if (OnWingRefreshed())
                            {
                                //var wingx = session.Query<Wing>().FirstOrDefault(w => w.WingGUID == WingGUID);
                                //if (wingx == null) return false;

                                wing.LastWingUpdate = DateTime.Now;
                                //	wingx.LoadTargets(this, ForceNamespaceRestriction ? null : session);	//	Needed?
                                //	wingx.Targets.ToList().ForEach(t => this.LoadTarget(t));				//	Needed?
                                session.SaveOrUpdate(wing);
                                tx.Commit();
                            }

                            //_isLoaded = true;
                            return false;
                        }
                        else
                        {
                            wing = this.FactoryWing();
                            wing.LoadTargets(this, ForceNamespaceRestriction ? null : session);
                            wing.Targets.ToList().ForEach(t => this.LoadTarget(t));

                            session.SaveOrUpdate(wing);

                            tx.Commit();
                        }
                    }
                }

                OnWingAdded();
                OnApplyDatasetHints();

                instalResult = true;
            }
            catch (Exception exc)
            {
                Logger.Write(exc, "Error reconciling WingModule \"{0}\"", this.Description);
                instalResult = false;
            }
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether [force namespace restriction].
        /// </summary>
        /// <value>
        /// <c>true</c> if [force namespace restriction]; otherwise, <c>false</c>.
        /// </value>
        protected virtual bool ForceNamespaceRestriction
        {
            get { return true; }
        }

        /// <summary>
        /// Called when [apply dataset hints].
        /// </summary>
        protected virtual void OnApplyDatasetHints()
        {
        }

        /// <summary>
        /// Targets the specified expr.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        public Element Target<T>(Expression<Func<T, object>> expr)
        {
            return this.TargetProperty(expr);
        }

        protected virtual void OnWingAdded()
        {
            Install(); // Handle non-database related wing installation tasks...
            InstallDb(); // Handle database related wing installation tasks...
            Update(); // Handle non-database related wing installation update tasks...
            UpdateDb(); // Handle database related wing installation update tasks...
        }

		protected virtual bool OnWingRefreshed()
		{
			var hasRefreshed = false;
			hasRefreshed |= Refresh();      // Handle non-database related wing refresh tasks...
			hasRefreshed |= RefreshDb();    // Handle database related wing refresh tasks...
			return hasRefreshed;
		}

        /// <summary>
        /// Called when [content type added].
        /// </summary>
        protected virtual void OnContentTypeAdded()
        { }

        //private void ReconcileMigrations(IMonahrqShell context)
        //{
        //    var migrationImple = typeof(DataMigrationImpl);
        //    if (!TestForContentType())
        //    {
        //        ExecuteContentExtensionMigrations(context);
        //    }
        //    OperationsLogger.Information("Create module data objects: {0}", GetType().Assembly.FullName);
        //    var migrations = GetType().Assembly.ExportedTypes.Where(t => !t.IsAbstract && migrationImple.IsAssignableFrom(t)).ToList();

        //    foreach (var migration in migrations)
        //    {
        //        CreateMigration(migration, context).Create();
        //    }
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
        //    CreateMigration(typeof(Extensibility.ContentManagement.Records.Migrations), context).Create();
        //}

        //private bool TestForContentType()
        //{
        //    var settings = ConfigurationService.ConnectionSettings;
        //   // var sql = "select count(*) from INFORMATION_SCHEMA.TABLES where TABLE_NAME = @contenttype";
        //    return 0 < (int) settings.ExecuteScalar("select count(*) from INFORMATION_SCHEMA.TABLES where TABLE_NAME = @contenttype", 
        //                                            new Dictionary<string, object>
        //                                            {
        //                                                {"@contenttype", typeof (ContentTypeRecord).EntityTableName()}
        //                                            });
        //}

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get { return _wingAttribute.Description; }
        }

        /// <summary>
        /// Gets the wing unique identifier.
        /// </summary>
        /// <value>
        /// The wing unique identifier.
        /// </value>
        public Guid WingGUID
        {
            get { return System.Guid.Parse(Guid); }
        }

        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public string Guid { get { return _wingAttribute.Guid; } }

        /// <summary>
        /// Installs non-database comonents for this <see cref="WingModule"/>
        /// </summary>
        /// <remarks>
        /// Called by <see cref="OnWingAdded"/>
        /// </remarks>
        /// <returns>A boolean value indicating whether installation was successful</returns>
        public virtual bool Install()
        {
            return true;
        }

        /// <summary>
        /// Installs the database components for this <see cref="WingModule"/>
        /// </summary>
        /// <remarks>
        /// Called by <see cref="OnWingAdded"/>
        /// </remarks>
        /// <returns>A boolean value indicating whether installation was successful</returns>
        public virtual bool InstallDb()
        {
            return true;
        }

		/// <summary>
		/// Gives opportunity to Wing to refresh any data if it seems fit.
		/// Is only called if Wing isn't already persisted.  (So not called if Install/InstallDb/Update/UpdateDb will be called).
		/// </summary>
		/// <returns>
		/// Indicates whether data for the Wing has actually been refreshed.  Important in that setting this flag could prevent
		/// future unnecessary refreshed from occurring.
		/// </returns>
		public virtual bool Refresh()
		{
			return false;
		}

		/// <summary>
		/// Gives opportunity to Wing to refresh any data if it seems fit.
		/// Is only called if Wing isn't already persisted.  (So not called if Install/InstallDb/Update/UpdateDb will be called).
		/// </summary>
		/// <returns>
		/// Indicates whether data for the Wing has actually been refreshed.  Important in that setting this flag could prevent
		/// future unnecessary refreshed from occurring.
		/// </returns>
		public virtual bool RefreshDb()
		{
			return false;
		}


        /// <summary>
        /// Updates non-database components for this <see cref="WingModule"/>
        /// </summary>
        /// <remarks>
        /// Called by <see cref="OnWingAdded"/>
        /// </remarks>
        /// <returns>A boolean value indicating whether the update operation was successful</returns>
        public virtual bool Update()
        {
            return true;
        }

        /// <summary>
        /// Updates database components for this <see cref="WingModule"/>
        /// </summary>
        /// <remarks>
        /// Called by <see cref="OnWingAdded"/>
        /// </remarks>
        /// <returns>A boolean value indicating whether the update operation was successful</returns>
        public virtual bool UpdateDb()
        {
            return true;
        }

        public virtual void OnImportsSatisfied()
        { }
    }
}
