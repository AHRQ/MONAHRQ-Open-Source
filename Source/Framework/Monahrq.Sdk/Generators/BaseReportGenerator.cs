using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Extensions;
using Monahrq.Sdk.Services.Generators;
using NHibernate.Criterion;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.Sdk.Generators
{
    /// <summary>
    /// Abstract implementation of <see cref="IReportGenerator"/> which provides common imports and functionality
    /// </summary>
    public abstract class BaseReportGenerator : IReportGenerator
    {
        #region Imported Services & Managers

        /// <summary>
        /// Gets or sets the NHibernate database session factory
        /// </summary>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        protected IDomainSessionFactoryProvider DataProvider { get; set; }

        /// <summary>
        /// Gets or sets the configuration service.
        /// </summary>
        [Import]
        protected IConfigurationService ConfigurationService { get; set; }

        /// <summary>
        /// Gets or sets the session logger
        /// </summary>
        [Import(LogNames.Session)]
        protected ILogWriter Logger { get; set; }

        /// <summary>
        /// Gets or sets the Prism <see cref="IModuleManager"/>
        /// </summary>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        protected IModuleManager ModuleManager { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IEventAggregator"/> instance used throughout MONAHRQ
        /// </summary>
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
        protected IEventAggregator EventAggregator { get; set; }

        #endregion

        #region Fields & conts

        private readonly ReportGeneratorAttribute _generatorAttribute;

        #endregion

        /// <summary>
        /// Gets or sets the <see cref="Website"/> for which a report is being generated
        /// </summary>
        public Website CurrentWebsite { get; set; }
        
        /// <summary>
        /// Gets the path to the base data directory for the <see cref="CurrentWebsite"/>
        /// </summary>
        protected string BaseDataDirectoryPath { get; private set; }

        /// <summary>
        /// Gets the string representation of report GUIDs for which this <see cref="IReportGenerator"/> can generate reports. Values are obtained from 
        /// the <see cref="ReportGeneratorAttribute"/> decorating this <see cref="IReportGenerator"/>.
        /// </summary>
        /// <seealso cref="_generatorAttribute">Cached <see cref="ReportGeneratorAttribute"/> instance</seealso>
        public string[] ReportIds { get; private set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Hospital"/> records included in the <see cref="CurrentWebsite"/>
        /// </summary>
        /// <remarks>
        /// This is a shortcut for <code><see cref="CurrentWebsite"/>.Hospitals.Select(x => x.Hospital)</code>
        /// </remarks>
        protected IList<Hospital> Hospitals { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="DataTable"/> containing a single "ID" column that contains selected <see cref="Hospital.Id"/> values for the <see cref="CurrentWebsite"/>
        /// </summary>
        protected DataTable HospitalIds { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="DataTable"/> containing a single "ID" column that contains selected <see cref="State.Id"/> values for the <see cref="CurrentWebsite"/>
        /// </summary>
        protected DataTable StateIds { get; set; }

        /// <summary>
        /// Gets or sets the file io time.
        /// </summary>
        /// <value>
        /// The file io time.
        /// </value>
        protected TimeSpan FileIOTime { get; set; }
        
        /// <inheritdoc/>
        public Report ActiveReport { get; set; }

        /// <summary>
        /// Gets the execution order.
        /// </summary>
        /// <value>
        /// The execution order.
        /// </value>
        public int ExecutionOrder
        {
            get
            {
                var rgAttr = this.GetType().GetCustomAttribute<ReportGeneratorAttribute>();
                return (rgAttr == null) ? 0 : rgAttr.ExecutionOrder;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseReportGenerator"/> class.
        /// </summary>
        /// <exception cref="InvalidOperationException">A the report generator does not have a valid ReportGeneratorAttribute defined. Please ensure that your report generator is properly configured.</exception>
        protected BaseReportGenerator()
        {
            _generatorAttribute = GetType().GetCustomAttributes(true)
                                           .OfType<ReportGeneratorAttribute>()
                                           .FirstOrDefault();

            if (_generatorAttribute == null)
            {
                throw new InvalidOperationException(@"A the report generator does not have a valid ReportGeneratorAttribute defined. Please ensure that your report generator is properly configured.");
            }

            ReportIds = _generatorAttribute.ReportIds.Select(id => id.ToLowerInvariant()).ToArray();
        }

        #region Methods

        /// <summary>
        /// Main function for all report generators that handles the worfkow for each report generator that derives from the BaseReportGenerator class.
        /// </summary>
        /// <param name="website">The website object used to generate reporting website.</param>
        /// <param name="publishTask">The publish task. This parameter is optional with the default value being PublishTask.Full</param>
        /// <exception cref="ArgumentNullException">website;Please provide a valid website to continue.</exception>
        /// <exception cref="AggregateException"></exception>
        public virtual void GenerateReport(Website website, PublishTask publishTask = PublishTask.Full)
        {
            if (website == null)
            {
                throw new ArgumentNullException("website", @"Please provide a valid website to continue.");
            }
             
            //if(CurrentWebsite == null)
            InitReportDependencies(website);

            // Create default data directory if doesn't exist all ready.
            BaseDataDirectoryPath = Path.Combine(CurrentWebsite.OutPutDirectory, "Data");
            if (!Directory.Exists(BaseDataDirectoryPath))
                Directory.CreateDirectory(BaseDataDirectoryPath);

            var validationResults = new List<ValidationResult>();
            if (ValidateDependencies(website, validationResults))
            {
                RefreshRptDataObjects();

                if (LoadReportData())
                {
                    OutputDataFiles();
                }
            }
            else
            {
                var generationErrors = new List<PublishException>();
                foreach (var result in validationResults)
                {
                    var validationMessage = result.ToString();
					if (result.Type == ValidationResult.ValidationResultTypeEnum.Error)
						LogMessage(validationMessage,PubishMessageTypeEnum.Error);
						//Logger.Write(validationMessage);
					else
						LogMessage(validationMessage);
						//Logger.Information(validationMessage);

					generationErrors.Add(new PublishException(validationMessage));
                }

                if (generationErrors != null && generationErrors.Any())
                    throw new AggregateException(generationErrors);
            }
        }

        /// <summary>
        /// Refreshes the report data objects.
        /// </summary>
        protected virtual void RefreshRptDataObjects()
        { }

        /// <summary>
        /// Initializes the report dependencies on before calling the GenerateReport method is executed.
        /// </summary>
        /// <param name="website">The website.</param>
        protected void InitReportDependencies(Website website)
        {
            CurrentWebsite = website;

            Hospitals = website.Hospitals.Select(wh => wh.Hospital).ToList();

            //SetHospitalSelectedRegionID();
            //var hospListString = string.Join(",", CurrentWebsite.Hospitals.Select(h => h.Hospital.Id).ToList());

            HospitalIds = new DataTable();
            HospitalIds.Columns.Add("ID", typeof(int));
            foreach (var hospitalId in CurrentWebsite.Hospitals.Select(h => h.Hospital.Id).ToList())
            {
                HospitalIds.Rows.Add(hospitalId);
            }

            StateIds = new DataTable();
            StateIds.Columns.Add("ID", typeof(int));
            IList<int> sIds = new List<int>();
            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
            {
                sIds = session.CreateCriteria<State>()
                              .Add(Restrictions.In("Abbreviation", CurrentWebsite.SelectedReportingStates))
                              .SetProjection(Property.ForName("Id"))
                              .List<int>();
            }

            foreach (var stateId in sIds)
            {
                StateIds.Rows.Add(stateId);
            }
        }

        /// <summary>
        /// Validates the report(S) dependencies needed to generate the report.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="validationResults">The validation results.</param>
        /// <returns></returns>
        public virtual bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
        {
            if (_generatorAttribute == null)
            {
                validationResults.Add(new ValidationResult(new InvalidOperationException(string.Format("The ReportGenerator class \"{0}\" is missing a ReportGeneratorAttribute.",
                                                                                                       GetType().Name))));
                return false;
            }

            // Validate Output Direction
            if (string.IsNullOrEmpty(website.OutPutDirectory))
            {
                validationResults.Add(new ValidationResult("This valid Output directory is required."));
            }

            // module dependency check
            var modules = ModuleManager.ModuleCatalog.Modules.ToList();
            foreach (var module in _generatorAttribute.ModuleDependencies)
            {
                if (!modules.Any(m => m.ModuleName.Equals(module) && m.State == ModuleState.Initialized))
                {
                    validationResults.Add(new ValidationResult(string.Format("This report is dependent upon module \"{0}\" being loaded and initialized.", module)));
                }
            }

            // TODO: Add validation logic for determining if target dataset type has been uploaded. - Jason


            return validationResults == null || validationResults.Count == 0;
        }


        /// <summary>
        /// Performs a pre execution checks to see if the derived reprt generator should be executed.
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckIfCanRun()
        {
            return true;
        }


        /// <summary>
        /// Loads the report data needed for the data output.
        /// </summary>
        /// <returns></returns>
        protected abstract bool LoadReportData();

        /// <summary>
        /// Outputs the report data files.
        /// </summary>
        /// <returns></returns>
        protected abstract bool OutputDataFiles();

        /// <summary>
        /// Initializes any data objects needed by the report generator while executing. This method is call during the application initialization/bootstrap
        /// </summary>
        public abstract void InitGenerator();

        /// <summary>
        /// Logs the message the session log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="generationStatus">The generation status.</param>
        /// <param name="publishTask">The publish task.</param>
        protected void LogMessage(
			string message,
			PubishMessageTypeEnum messageType = PubishMessageTypeEnum.Information,
			WebsiteGenerationStatus generationStatus = WebsiteGenerationStatus.ReportsGenerationInProgress,
			PublishTask publishTask = PublishTask.Full)
        {
			var region = new WebsitePublishEventRegion(this);
			EventAggregator.GetEvent<WebsitePublishEvent>().Publish(
				new ExtendedEventArgs<WebsitePublishEventArgs>(
					new WebsitePublishEventArgs(
						region,
						message,
						messageType,
						generationStatus,
						DateTime.Now,
						publishTask)
            ));
        }

        /// <summary>
        /// Creates the report data directory for a specified path.
        /// </summary>
        /// <param name="path">The directory path.</param>
        protected static void CreateDir(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        #endregion Methods

        #region SQL Execution Methods

        /// <summary>
        /// Runs a specified store procedure.
        /// </summary>
        /// <param name="sqlStatement">The stored procedure name.</param>
        /// <param name="timingDescription">The timing description.</param>
        /// <param name="parameters">The parameters be used in the soecified stored procedure.</param>
        public virtual void RunSproc(string sqlStatement, string timingDescription = "", params KeyValuePair<string, object>[] parameters)
        {
            try
            {
                using (var session = DataProvider.SessionFactory.OpenStatelessSession())
                {
                    using (var sqlCommand = session.Connection.CreateCommand() as SqlCommand)
                    {
                        sqlCommand.Connection = session.Connection as SqlConnection;
                        sqlCommand.CommandTimeout = DefaultTimeOut;
                        sqlCommand.CommandText = sqlStatement;
                        sqlCommand.CommandType = CommandType.StoredProcedure;

                        if (parameters != null && parameters.Any())
                        {
                            foreach (var parameter in parameters)
                            {
                                if (parameter.Value.GetType() != typeof(DataTable))
                                    sqlCommand.Parameters.Add(new SqlParameter(parameter.Key, parameter.Value));
                                else
                                {
                                    SqlParameter sqlParam = sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                                    sqlParam.SqlDbType = SqlDbType.Structured;
                                }
                            }
                        }
                        DateTime start = DateTime.Now;

                        sqlCommand.ExecuteNonQuery();
                        TimeSpan timeDiff = DateTime.Now - start;
                        if (timingDescription.Length > 0)
                        {
                            Logger.Write(string.Format("{0}Sproc completed in {1:c}", timingDescription + " - ", timeDiff));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Error running sproc {0}", sqlStatement));
                Logger.Write(ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the default time out.
        /// </summary>
        /// <value>
        /// The default time out.
        /// </value>
        protected virtual int DefaultTimeOut { get { return 300000; } }

        /// <summary>
        /// Runs the specified stored procedue and return result in data table.
        /// </summary>
        /// <param name="sqlStatement">The SQL.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public virtual DataTable RunSprocReturnDataTable(string sqlStatement, params KeyValuePair<string, object>[] parameters)
        {
            try
            {
                DataTable result = new DataTable();
                using (var session = DataProvider.SessionFactory.OpenStatelessSession())
                {
                    using (var sqlCommand = session.Connection.CreateCommand() as SqlCommand)
                    {
                        if (sqlCommand.Connection.State == ConnectionState.Closed)
                            sqlCommand.Connection.Open();

                        sqlCommand.Connection = session.Connection as SqlConnection;
                        sqlCommand.CommandText = sqlStatement;
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.CommandTimeout = DefaultTimeOut;

                        if (parameters != null && parameters.Any())
                        {
                            foreach (var parameter in parameters)
                            {
                                if (parameter.Value.GetType() != typeof(DataTable))
                                    sqlCommand.Parameters.Add(new SqlParameter(parameter.Key, parameter.Value));
                                else
                                {
                                    SqlParameter sqlParam = sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                                    sqlParam.SqlDbType = SqlDbType.Structured;
                                }
                            }
                        }

                        using (var adapter = sqlCommand.ExecuteReader())
                        {
                            result.Load(adapter);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Error running sproc {0}", sqlStatement));
                Logger.Write(ex);
                throw;
            }
        }

        /// <summary>
        /// Excutes the specified stored procedure and returns the results as a data table. 
        /// Before returning the table, this methods runs a specified action against the data table.
        /// </summary>
        /// <param name="sqlStatement">The stored procedure.</param>
        /// <param name="tableAction">The table action.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public virtual DataTable RunSprocReturnDataTableWithAction(string sqlStatement, Action<DataTable> tableAction, params KeyValuePair<string, object>[] parameters)
        {
            try
            {
                var table = RunSprocReturnDataTable(sqlStatement, parameters);
                tableAction(table);
                return table;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Runs a T-Sql statement and returns the results as a data table.
        /// </summary>
        /// <param name="sqlStatement">The T-Sql statement.</param>
        /// <param name="parameters">The parameters(s) used for the t-Sql statement</param>
        /// <returns></returns>
        public virtual DataTable RunSqlReturnDataTable(string sqlStatement, params KeyValuePair<string, object>[] parameters)
        {
            try
            {
                var result = new DataTable();
                using (var session = DataProvider.SessionFactory.OpenStatelessSession())
                {
                    using (var sqlCommand = session.Connection.CreateCommand() as SqlCommand)
                    {
                        if (sqlCommand.Connection.State == ConnectionState.Closed)
                            sqlCommand.Connection.Open();

                        //sqlCommand.Connection = session.Connection as SqlConnection;
                        sqlCommand.CommandText = sqlStatement;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandTimeout = DefaultTimeOut;

                        if (parameters != null && parameters.Any())
                        {
                            foreach (var parameter in parameters.ToList())
                            {
                                if (parameter.Value.GetType() != typeof(DataTable))
                                    sqlCommand.Parameters.Add(new SqlParameter(parameter.Key, parameter.Value));
                                else
                                {
                                    SqlParameter sqlParam = sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                                    sqlParam.SqlDbType = SqlDbType.Structured;
                                }
                            }
                        }

                        using (var adapter = sqlCommand.ExecuteReader())
                        {
                            result.Load(adapter);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Error running sproc {0}", sqlStatement));
                Logger.Write(ex);
                throw;
            }
        }

        /// <summary>
        /// Runs a specified store procedure and returns the results as a dataset.
        /// </summary>
        /// <param name="sqlStatement">The store procedure .</param>
        /// <param name="parameters">The parameters need for the stored procedure.</param>
        /// <returns></returns>
        protected DataSet RunSprocReturnDataSet(string sqlStatement, params KeyValuePair<string, object>[] parameters)
        {
            try
            {
                var result = new DataSet();
                using (var session = DataProvider.SessionFactory.OpenStatelessSession())
                {
                    using (var sqlCommand = session.Connection.CreateCommand() as SqlCommand)
                    {
                        if (sqlCommand.Connection.State == ConnectionState.Closed)
                            sqlCommand.Connection.Open();

                        sqlCommand.Connection = session.Connection as SqlConnection;
                        sqlCommand.CommandText = sqlStatement;
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.CommandTimeout = DefaultTimeOut;

                        if (parameters != null && parameters.Any())
                        {
                            foreach (var parameter in parameters)
                            {
                                if (parameter.Value.GetType() != typeof(DataTable))
                                    sqlCommand.Parameters.Add(new SqlParameter(parameter.Key, parameter.Value));
                                else
                                {
                                    SqlParameter sqlParam = sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                                    sqlParam.SqlDbType = SqlDbType.Structured;
                                }
                            }
                        }

                        using (var adapter = new SqlDataAdapter(sqlCommand as SqlCommand))
                        {
                            adapter.Fill(result);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Error running sproc {0}", sqlStatement));
                Logger.Write(ex);
                throw;
            }
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="timingDescription">The timing description.</param>
        /// <param name="parameters">The parameters.</param>
        protected void ExecuteNonQuery(string sql, string timingDescription = "", IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            try
            {
                parameters = parameters ?? Enumerable.Empty<KeyValuePair<string, object>>();

                using (var session = DataProvider.SessionFactory.OpenStatelessSession())
                {
                    using (var cmd = session.Connection.CreateCommand())
                    {
                        if (cmd.Connection.State == ConnectionState.Closed)
                            cmd.Connection.Open();
                        // TODO: Look at how we're doing this. I think long timeout was only 5 minutes.
                        //cmd.CommandTimeout = (int)Math.Round(MonahrqConfiguration.SettingsGroup.MonahrqSettings().LongTimeout.TotalSeconds);
                        cmd.CommandTimeout = DefaultTimeOut;
                        cmd.CommandText = sql;
                        foreach (var p in parameters)
                        {
                            var prm = cmd.CreateParameter();
                            prm.ParameterName = p.Key;
                            prm.Value = p.Value;
                            cmd.Parameters.Add(prm);
                        }
                        DateTime start = DateTime.Now;
                        cmd.ExecuteNonQuery();
                        TimeSpan timeDiff = DateTime.Now - start;
                        if (timingDescription.Length > 0)
                        {
                            Logger.Write(string.Format("{0}Query completed in {1:c}", timingDescription + " - ", timeDiff));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Error executing sql {0}", sql));
                Logger.Write(ex);
                throw;
            }
        }

        /// <summary>
        /// Runs the SQL scripts.
        /// </summary>
        /// <param name="scriptsPath">The scripts path.</param>
        /// <param name="files">The files.</param>
        public void RunSqlScripts(string scriptsPath, string[] files)
        {
            try
            {
                foreach (var installScript in files)
                {
                    var query = File.ReadAllText(Path.Combine(scriptsPath, installScript));
                    if (!string.IsNullOrEmpty(query))
                    {
                        ExecuteSqlFile(query);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Error running sql scripts {0} {1}", scriptsPath, files));
                Logger.Write(ex);
                throw;
            }
        }
        /// <summary>
        /// Runs the SQL script file.
        /// </summary>
        /// <param name="scriptFilePath">The script file path.</param>
        public void RunSqlScriptFile(string scriptFilePath)
		{
			try
			{
				var query = File.ReadAllText(scriptFilePath);
				if (!string.IsNullOrEmpty(query))
				{
					ExecuteSqlFile(query);
				}
			}
			catch (Exception ex)
			{
				Logger.Write(string.Format("Error running sql scripts {0}", scriptFilePath));
				Logger.Write(ex);
				throw;
			}
		}

        /// <summary>
        /// Executes the t-sql stored in a Sql file on the disk space.
        /// </summary>
        /// <param name="sql">The T-Sql from a file on the disk drive.</param>
        protected void ExecuteSqlFile(string sql)
        {
            Regex regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string[] lines = regex.Split(sql);
            using (var session = DataProvider.SessionFactory.OpenSession())
            {
                using (var tx = session.Connection.BeginTransaction())
                {
                    using (var cmd = session.Connection.CreateCommand())
                    {
                        if (cmd.Connection.State == ConnectionState.Closed)
                            cmd.Connection.Open();
                        cmd.Transaction = tx;
                        foreach (string line in lines)
                        {
                            if (line.Length > 0)
                            {
                                try
                                {
                                    cmd.CommandTimeout = (int)Math.Round(MonahrqConfiguration.SettingsGroup.MonahrqSettings().LongTimeout.TotalSeconds);
                                    cmd.CommandText = line;
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandTimeout = DefaultTimeOut;
                                    cmd.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    tx.Rollback();
                                    Logger.Write(string.Format("Error executing sql {0}", sql));
                                    Logger.Write(ex);
                                    throw;
                                }
                            }
                        }
                    }
                    tx.Commit();
                }
            }
        }

        /// <summary>
        /// Flushes the SQL caches stored in Sql Server.
        /// </summary>
        public void FlushSqlCaches()
        {
            // Clears out the sql caches to free up memory.
            try
            {
                using (var session = DataProvider.SessionFactory.OpenSession())
                {
                    using (var cmd = session.Connection.CreateCommand())
                    {
                        string freeCachesql = @"
                            CHECKPOINT
                            DBCC DROPCLEANBUFFERS
                            DBCC FREESYSTEMCACHE ('ALL')
                            DBCC FREESESSIONCACHE
                            DBCC FREEPROCCACHE
                        ";

                        if (cmd.Connection.State == ConnectionState.Closed)
                            cmd.Connection.Open();
                        cmd.CommandTimeout = DefaultTimeOut;
                        cmd.CommandText = freeCachesql;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Error freeing sql caches."));
                Logger.Write(ex);
                throw;
            }
        }

        /// <summary>
        /// Creates new and/or updates existing database objects such as table schemas, store procedures, user defined functions (UDF) and custom sql types.
        /// </summary>
        /// <param name="objectName">Name of the database object.</param>
        /// <param name="sqlStatement">The T-Sql statement.</param>
        /// <param name="sqlParams">The SQL parameters.</param>
        /// <param name="dataObjectType">Type of the sql data object.</param>
        public void CreateOrUpdateDbObject(string objectName, string sqlStatement, string sqlParams, DataObjectTypeEnum dataObjectType)
        {
            try
            {
                // AF = Aggregate function (CLR)
                // C  = CHECK constraint
                // D  = DEFAULT (constraint or stand-alone)
                // F  = FOREIGN KEY constraint
                // FN = SQL scalar function
                // FS = Assembly (CLR) scalar-function
                // FT = Assembly (CLR) table-valued function
                // IF = SQL inline table-valued function
                // IT = Internal table
                // P  = SQL Stored Procedure
                // PC = Assembly (CLR) stored-procedure
                // PG = Plan guide
                // PK = PRIMARY KEY constraint
                // R  = Rule (old-style, stand-alone)
                // RF = Replication-filter-procedure
                // S  = System base table
                // SN = Synonym
                // SQ = Service queue
                // TA = Assembly (CLR) DML trigger
                // TF = SQL table-valued-function
                // TR = SQL DML trigger
                // TT = Table type
                // U  = Table (user-defined)
                // UQ = UNIQUE constraint
                // V  = View
                // X  = Extended stored procedure

                var completeSqlStatement = new StringBuilder();

                switch (dataObjectType)
                {
                    case DataObjectTypeEnum.Type:
                        completeSqlStatement.AppendLine(string.Format("IF NOT EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = '{0}') ", objectName));
                        //                        completeSqlStatement.AppendLine(string.Format("DROP TYPE {0}", objectName));
                        completeSqlStatement.AppendLine(sqlStatement);
                        ExecuteNonQuery(completeSqlStatement.ToString());

                        //ConfigurationService.ConnectionSettings.ExecuteNonQuery(sqlStatement);

                        break;

                    case DataObjectTypeEnum.StoredProcedure:
                        completeSqlStatement.AppendLine(string.Format("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].{0}') AND type in (N'P', N'PC'))", objectName));
                        completeSqlStatement.AppendLine(string.Format("DROP PROCEDURE {0};", objectName));
                        ExecuteNonQuery(completeSqlStatement.ToString());

                        completeSqlStatement.Clear();
                        completeSqlStatement.AppendLine(string.Format("CREATE PROCEDURE {0}", objectName));
                        completeSqlStatement.AppendLine(sqlParams);
                        completeSqlStatement.AppendLine("AS");
                        completeSqlStatement.AppendLine("BEGIN");
                        completeSqlStatement.AppendLine("    SET NOCOUNT ON;");
                        completeSqlStatement.AppendLine(sqlStatement);
                        completeSqlStatement.AppendLine("END");
                        ExecuteNonQuery(completeSqlStatement.ToString());

                        break;

                    case DataObjectTypeEnum.UserDefinedView:
                        completeSqlStatement.AppendLine(string.Format("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].{0}') AND type in (N'V'))", objectName));
                        completeSqlStatement.AppendLine(string.Format("DROP VIEW {0};", objectName));
                        ExecuteNonQuery(completeSqlStatement.ToString());

                        ExecuteNonQuery(sqlStatement);

                        break;

                    case DataObjectTypeEnum.UserDefinedFunction:
                        completeSqlStatement.AppendLine(string.Format("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].{0}') AND type in (N'FN', N'IF', N'TF'))", objectName));
                        completeSqlStatement.AppendLine(string.Format("DROP FUNCTION {0};", objectName));
                        ExecuteNonQuery(completeSqlStatement.ToString());

                        ExecuteNonQuery(sqlStatement);

                        break;

                    case DataObjectTypeEnum.Table:
                        completeSqlStatement.AppendLine(string.Format("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].{0}') AND type in (N'U', N'TT'))", objectName));
                        completeSqlStatement.AppendLine(string.Format("DROP TABLE {0};", objectName));
                        ExecuteNonQuery(completeSqlStatement.ToString());

                        ExecuteNonQuery(sqlStatement);

                        break;

                    default:
                        completeSqlStatement.AppendLine(string.Format("IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].{0}') AND type in (N'U'))", objectName));
                        completeSqlStatement.AppendLine("BEGIN ");
                        completeSqlStatement.AppendLine();
                        completeSqlStatement.AppendLine(sqlStatement);
                        completeSqlStatement.AppendLine();
                        completeSqlStatement.AppendLine("END");
                        ExecuteNonQuery(completeSqlStatement.ToString());

                        break;
                }

                //using (var session = DataProvider.SessionFactory.OpenSession())
                //{
                //    var sqlQuery = session.CreateSQLQuery(ConstructSqlStatement(objectName, sqlStatement, sqlParams, dataObjectType));
                //    sqlQuery.ExecuteUpdate();

                //}
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        #endregion SQL Execution Methods

        #region JSON File Generation Methods

        /// <summary>
        /// Generates the json file.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileheader">The fileheader.</param>
        /// <param name="isWingOuput">if set to <c>true</c> [is wing ouput].</param>
        /// <param name="fileheaderMapping">The fileheader mapping.</param>
        protected void GenerateJsonFile(object item, string fileName, string fileheader = null, bool isWingOuput = false, string fileheaderMapping = null)
        {
            if (!isWingOuput)
                JsonHelper.GenerateJsonFile(item, fileName, fileheader);
            else
                Infrastructure.Utility.JsonHelper.GenerateWingJsonFile(item, fileName, fileheader, fileheaderMapping);
        }

        /// <summary>
        /// Generates the utilization json file combinations.
        /// </summary>
        /// <param name="dataDir">The data dir.</param>
        /// <param name="subDir">The sub dir.</param>
        /// <param name="jsonDomain">The json domain.</param>
        /// <param name="summarySproc">The summary sproc.</param>
        /// <param name="detailSproc">The detail sproc.</param>
        /// <param name="baseParams">The base parameters.</param>
        /// <param name="outerDataTable">The outer data table.</param>
        /// <param name="outerParam">The outer parameter.</param>
        /// <param name="outerParamName">Name of the outer parameter.</param>
        /// <param name="innerDataTable">The inner data table.</param>
        /// <param name="innerParam">The inner parameter.</param>
        /// <param name="innerParamName">Name of the inner parameter.</param>
        /// <returns></returns>
        protected TimeSpan GenerateUtilizationJsonFileCombinations(
                        string dataDir, string subDir, string jsonDomain,
                        string summarySproc, string detailSproc,
                        KeyValuePair<string, object>[] baseParams,
                        DataTable outerDataTable, string outerParam, string outerParamName,
                        DataTable innerDataTable, string innerParam, string innerParamName)
        {
            try
            {
                DateTime start = DateTime.Now;
                DataSet ds = new DataSet();

                // Create summary page for all outer records combined
                // \CCS\Outer\Outer_0\summary.json
                ds = RunSprocReturnDataSet(summarySproc, baseParams);
                SaveUtilizationDatasetToJsonFile(ds, Path.Combine(dataDir, subDir + "0", "summary.js"), jsonDomain);


                // Create details page for all outer and inner records combined.
                // \CCS\Outer\Outer_0\details.json
                ds = RunSprocReturnDataSet(detailSproc, baseParams);
                SaveUtilizationDatasetToJsonFile(ds, Path.Combine(dataDir, subDir + "0", "details.js"), jsonDomain);

                // Create details page for all outer records combined for one inner record.
                // \CCS\Outer\Outer_0\details_[InnterID].json
                foreach (DataRow innerRow in innerDataTable.Rows)
                {
                    ds = RunSprocReturnDataSet(detailSproc,
                                baseParams.Add(new KeyValuePair<string, object>(innerParam, innerRow[innerParamName].ToString())));
                    SaveUtilizationDatasetToJsonFile(ds, Path.Combine(dataDir, subDir + "0", "details_" + innerRow[innerParamName].ToString() + ".js"), jsonDomain);
                }

                foreach (DataRow outerRow in outerDataTable.Rows)
                {
                    // Create summary page for one outer record.
                    // \CCS\Outer\Outer_[OuterID]\summary.json
                    ds = RunSprocReturnDataSet(summarySproc,
                                baseParams.Add(new KeyValuePair<string, object>(outerParam, outerRow[outerParamName].ToString())));
                    SaveUtilizationDatasetToJsonFile(ds, Path.Combine(dataDir, subDir + outerRow[outerParamName], "summary.js"), jsonDomain);

                    // Create details page for one outer record and all inner records combined.
                    // \CCS\Outer\Outer_[OuterID]\details.json
                    ds = RunSprocReturnDataSet(detailSproc,
                                baseParams.Add(new KeyValuePair<string, object>(outerParam, outerRow[outerParamName].ToString())));
                    SaveUtilizationDatasetToJsonFile(ds, Path.Combine(dataDir, subDir + outerRow[outerParamName], "details.js"), jsonDomain);

                    // Create details page for one outer and inner record.
                    // \CCS\Outer\Outer_[OuterID]\details_[InnerID].json
                    foreach (DataRow innerRow in innerDataTable.Rows)
                    {
                        ds = RunSprocReturnDataSet(detailSproc,
                                    baseParams.Add(new KeyValuePair<string, object>(outerParam, outerRow[outerParamName].ToString()))
                                              .Add(new KeyValuePair<string, object>(innerParam, innerRow[innerParamName].ToString())));
                        SaveUtilizationDatasetToJsonFile(ds, Path.Combine(dataDir, subDir + outerRow[outerParamName], "details_" + innerRow[innerParamName].ToString() + ".js"), jsonDomain);
                    }
                }
                return DateTime.Now - start;
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Saves the utilization dataset to json file.
        /// </summary>
        /// <param name="ds">The ds.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileheader">The fileheader.</param>
        protected void SaveUtilizationDatasetToJsonFile(DataSet ds, string fileName, string fileheader = null)
        {
            try
            {
                // Check to make sure the dataset isn't null, contains 3 tables, and the 3rd table has data. If so, save to a json file.
                if (ds != null && ds.Tables.Count == 3 && ds.Tables[2].Rows.Count > 0)
                {
                    ds.Tables[0].TableName = "NationalData";
                    ds.Tables[1].TableName = "TotalData";
                    ds.Tables[2].TableName = "TableData";
                    DateTime start = DateTime.Now;
                    JsonHelper.GenerateJsonFile(ds, fileName, fileheader);
                    FileIOTime = FileIOTime + (DateTime.Now - start);
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Error saving JSON file {0}", fileName));
                Logger.Write(ex);
                throw;
            }
        }

        #endregion JSON File Generation Methods

        #region Index Manipulation Methods

        /// <summary>
        /// Enables the disable table indexes.
        /// </summary>
        /// <param name="disable">if set to <c>true</c> [disable].</param>
        /// <param name="tableNames">The table names.</param>
        protected void EnableDisableTableIndexes(bool disable, params string[] tableNames)
        {
            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
            {
                foreach (var tableName in tableNames)
                {
                    session.CreateSQLQuery(EnableDisableNonClusteredIndexes(tableName, disable)) //ALL
                       .SetTimeout(600)
                       .ExecuteUpdate();
                }
            }
        }

        /// <summary>
        /// Enables the disable non clustered indexes.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="disable">if set to <c>true</c> [disable].</param>
        /// <returns></returns>
        private string EnableDisableNonClusteredIndexes(string tableName, bool disable = false)
        {
            var sqlStatement = new StringBuilder();

            sqlStatement.AppendLine("DECLARE @sql AS VARCHAR(MAX)='';" + System.Environment.NewLine);
            sqlStatement.Append("SELECT	 @sql = @sql + 'ALTER INDEX ' + sys.indexes.Name + ' ON " + tableName + " " + (disable ? "DISABLE" : "REBUILD") + "; ");

            if (!disable)
            {
                sqlStatement.Append(" ALTER INDEX ' + sys.indexes.Name + ' ON " + tableName + " REORGANIZE;");
            }

            sqlStatement.Append("' + CHAR(13) + CHAR(10)");
            sqlStatement.AppendLine();

            sqlStatement.AppendLine("FROM	 sys.indexes" + System.Environment.NewLine);
            sqlStatement.AppendLine("JOIN    sys.objects ON sys.indexes.object_id = sys.objects.object_id");
            sqlStatement.AppendLine("WHERE sys.indexes.type_desc = 'NONCLUSTERED'");
            sqlStatement.AppendLine("  AND sys.objects.type_desc = 'USER_TABLE'");
            sqlStatement.AppendLine("  AND sys.objects.Name = '" + tableName + "';");

            sqlStatement.AppendLine();
            sqlStatement.AppendLine("exec(@sql);");

            return sqlStatement.ToString();
        }

        #endregion Index Manipulation Methods

        #region Suppression Methods

        /// <summary>
        /// Gets the suppression.
        /// </summary>
        /// <param name="measureName">Name of the measure.</param>
        /// <returns></returns>
        protected decimal GetSuppression(string measureName)
        {
            decimal suppression = 0;
            var dischargeMeasure = CurrentWebsite.Measures.FirstOrDefault(m => m.OriginalMeasure.MeasureCode == measureName);
            if (dischargeMeasure != null)
            {
                if (dischargeMeasure.OverrideMeasure != null && dischargeMeasure.OverrideMeasure.SuppressionNumerator.HasValue)
                {
                    suppression = dischargeMeasure.OverrideMeasure.SuppressionNumerator.Value;
                }
                else
                {
                    if (dischargeMeasure.OriginalMeasure != null && dischargeMeasure.OriginalMeasure.SuppressionNumerator.HasValue)
                    {
                        suppression = dischargeMeasure.OriginalMeasure.SuppressionNumerator.Value;
                    }
                }
            }

            return suppression;
        }

        #endregion Suppression Methods


    }

    public class ValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ValidationResult(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public ValidationResult(Exception exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public ValidationResult(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }

        public string Message { get; private set; }
        public Exception Exception { get; private set; }
        public ValidationResultTypeEnum Type
        {
            get { return (Exception != null) ? ValidationResultTypeEnum.Error : ValidationResultTypeEnum.Info; }
        }

        /// <summary>
        /// The validation result type enumeration. Type can either be an Error or Info.
        /// </summary>
        public enum ValidationResultTypeEnum
        {
            Error,
            Info
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (Exception != null)
            {
                if (string.IsNullOrEmpty(Message))
                {
                    Message = (Exception.InnerException ?? Exception).Message;
                }
            }
            return Message;
        }
    }
}