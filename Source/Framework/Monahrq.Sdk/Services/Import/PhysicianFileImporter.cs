using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.Physicians;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.FileSystem;
using Monahrq.Infrastructure.Services.Hospitals;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Events;

using NHibernate.Linq;
using ReflectionHelper = Monahrq.Infrastructure.Utility.Extensions.ReflectionHelper;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;

namespace Monahrq.Sdk.Services.Import
{
    /// <summary>
    /// The Physicians / Medical Practices data file importer.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Services.Import.EntityFileImporter" />
    [Export(ImporterContract.Physician, typeof(IEntityFileImporter))]
    public class PhysicianFileImporter : EntityFileImporter
    {
        private DataTable _physiciansTable = new DataTable("Targets_PhysicianTargets");
        private readonly IConfigurationService _configurationService;
        private const int BATCH_SIZE = 10000;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicianFileImporter"/> class.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="hospitalRegistryService">The hospital registry service.</param>
        /// <param name="events">The events.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configurationService">The configuration service.</param>
        [ImportingConstructor]
        public PhysicianFileImporter(IUserFolder folder
                                     , IDomainSessionFactoryProvider provider
                                     , IHospitalRegistryService hospitalRegistryService
                                     , IEventAggregator events
                                     , [Import(LogNames.Session)] ILogWriter logger
                                     , IConfigurationService configurationService)
            : base(folder, provider, hospitalRegistryService, events, logger)
        {
            _configurationService = configurationService;
        }

        /// <summary>
        /// Begins the import.
        /// </summary>
        protected override void BeginImport()
        {
            base.BeginImport();

            InitializeDataTable();

            ClearTempTable();

            NumberOfErrors = 0;
            CountInserted = 0;
            ImportErrors = new List<ImportError>();
        }

        /// <summary>
        /// Initializes the data table.
        /// </summary>
        private void InitializeDataTable()
        {
            if (_physiciansTable == null)
                _physiciansTable = new DataTable("Targets_PhysicianTargets");

            using (var session = Provider.SessionFactory.OpenStatelessSession())
            {
                using (var adpater = new SqlDataAdapter("SELECT TOP 0 * FROM [dbo].[Targets_PhysicianTargets]", session.Connection as SqlConnection))
                {
                    adpater.Fill(_physiciansTable);
                }
            }
			if (_physiciansTable.Columns["Npi"].DataType == typeof(int))
				_physiciansTable.Columns["Npi"].DataType = typeof(Int64);
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public override void Execute()
        {
			var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
			var obj = new object[] { };
            var version = DateTime.Now.Ticks;

            NumberOfErrors = 0;
            AssertErrorFile();

            var startTime = DateTime.Now;
            var states = configService.HospitalRegion.DefaultStates.OfType<string>().ToList();

            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = FileExtension,
                Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*"
            };

            if (!dlg.ShowDialog().GetValueOrDefault()) return;

            if (PromptUserForContinue() == MessageBoxResult.No)
            {
                return;
            }

            if (!ValidateImportFileName(dlg.FileName))
            {
                var message = string.Format("The file \"{0}\" could not be import due to the name containing special characters and/or dashes (-). Please remove these special characters and dashes to spaces and/or underscores and try again.", Path.GetFileName(dlg.FileName));
                MessageBox.Show(message, "MONAHRQ Import File Open Error", MessageBoxButton.OK);
                return;
            }

            try
            {
                OnImporting(this, EventArgs.Empty);

                BeginImport();

                var uploadFilePath = dlg.FileName;
                //var fileName = Regex.Replace(Path.GetFileName(uploadFilePath), "[^a-zA-Z0-9_]+", "_").Replace("-", "_"); 

                var tb = new DataTable("Temp");
                lock (obj)
                {
                    using (var connection = new OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0; Data Source= " +
                                                Path.GetDirectoryName(uploadFilePath) + "; Extended Properties= \"Text;HDR=YES;FMT=Delimited\""))
                    {
                        connection.Open();


                        using (var cmd = new OleDbCommand("SELECT * FROM " + Path.GetFileName(uploadFilePath), connection))
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    tb.Load(reader);
                                }
                            }
                        }
                    }
                }

                var actualStates = tb.Rows.OfType<DataRow>()
                                          .Where(dr => dr["State"] != DBNull.Value)
                                          .Select(dr => dr["State"].ToString())
                                          .Distinct()
                                          .ToList();

                int rowCount = 0;
                foreach (DataRow row in tb.Rows)
                {
                    var state = row["State"].ToString();
                    long npi = 0;

                    if (row["NPI"] != DBNull.Value)
                        npi = long.Parse(row["NPI"].ToString());

                    if (!states.Contains(state))
                    {
                        var message = new StringBuilder();
                        message.AppendLine(
                            string.Format(
                                "The Physician record being imported is not with in the selected state context of \"{0}\"",
                                string.Join(",", states)));
                        message.AppendLine(string.Format("Data: {0}", string.Join(",", row.ItemArray)));
                        ImportErrors.Add(ImportError.Create("Physician", "Physician", message.ToString()));
                        continue;
                    }

                    if (CheckIfAlreadyExists(npi, state))
                    {
                        var message = new StringBuilder();
                        message.AppendLine(
                            string.Format(
                                "The Physician record with NPI \"{0}\" already exists and can't be imported. Please choose an unique NPI number.",
                                npi));
                        message.AppendLine(string.Format("Data: {0}", string.Join(",", row.ItemArray)));
                        ImportErrors.Add(ImportError.Create("Physician", "Physician", message.ToString()));
                        continue;
                    }

                    var tRow = _physiciansTable.NewRow();

                    tRow["Npi"] = npi;
                    tRow["PacId"] = row["PAC ID"];
                    tRow["ProfEnrollId"] = row["Professional Enrollment ID"];
                    tRow["LastName"] = row["Last Name"];
                    tRow["FirstName"] = row["First Name"];
                    tRow["MiddleName"] = row["Middle Name"];
                    tRow["Suffix"] = row["Suffix"];
                    tRow["Gender"] = row["Gender"];
                    tRow["Credential"] = row["Credential"];
                    tRow["MedicalSchoolName"] = row["Medical school name"];
                    tRow["GraduationYear"] = row["Graduation year"];
                    tRow["PrimarySpecialty"] = row["Primary specialty"];
                    tRow["SecondarySpecialty1"] = row["Secondary specialty 1"];
                    tRow["SecondarySpecialty2"] = row["Secondary specialty 2"];
                    tRow["SecondarySpecialty3"] = row["Secondary specialty 3"];
                    tRow["SecondarySpecialty4"] = row["Secondary specialty 4"];
                    tRow["AllSecondarySpecialties"] = row["All secondary specialties"];
                    tRow["OrgLegalName"] = row["Organization legal name"];
                    tRow["DBAName"] = DBNull.Value; // row["Organization DBA name"];
                    tRow["GroupPracticePacId"] = row["Group Practice PAC ID"];
                    tRow["NumberofGroupPracticeMembers"] = row["Number of Group Practice members"];
                    tRow["Line1"] = row["Line 1 Street Address"];
                    tRow["Line2"] = row["Line 2 Street Address"];
                    tRow["MarkerofAdressLine2Suppression"] = row["Marker of address line 2 suppression"] ==
                                                                     DBNull.Value ||
                                                                     row["Marker of address line 2 suppression"]
                                                                         .ToString() == "N"
                                                                         ? 0
                                                                         : 1;
                    tRow["City"] = row["City"];
                    tRow["State"] = row["State"];
                    tRow["ZipCode"] = row["Zip Code"];

                    if (row["Hospital affiliation CCN 1"] != DBNull.Value)
                        tRow["HospitalAffiliationCCN1"] = row["Hospital affiliation CCN 1"].ToString().PadLeft(6, '0');

                    tRow["HospitalAffiliationLBN1"] = row["Hospital affiliation LBN 1"];
                    if (row["Hospital affiliation CCN 2"] != DBNull.Value)
                        tRow["HospitalAffiliationCCN2"] = row["Hospital affiliation CCN 2"].ToString().PadLeft(6, '0');

                    tRow["HospitalAffiliationLBN2"] = row["Hospital affiliation LBN 2"];
                    if (row["Hospital affiliation CCN 3"] != DBNull.Value)
                        tRow["HospitalAffiliationCCN3"] = row["Hospital affiliation CCN 3"].ToString().PadLeft(6, '0');

                    tRow["HospitalAffiliationLBN3"] = row["Hospital affiliation LBN 3"];
                    if (row["Hospital affiliation CCN 4"] != DBNull.Value)
                        tRow["HospitalAffiliationCCN4"] = row["Hospital affiliation CCN 4"].ToString().PadLeft(6, '0');

                    tRow["HospitalAffiliationLBN4"] = row["Hospital affiliation LBN 4"];
                    if (row["Hospital affiliation CCN 5"] != DBNull.Value)
                        tRow["HospitalAffiliationCCN5"] = row["Hospital affiliation CCN 5"].ToString().PadLeft(6, '0');

                    tRow["HospitalAffiliationLBN5"] = row["Hospital affiliation LBN 5"];

                    tRow["AcceptsMedicareAssignment"] = row["Professional accepts Medicare Assignment"];
                    tRow["ParticipatesInERX"] = DBNull.Value;
                    tRow["ParticipatesInPQRS"] = row["Reported Quality Measures"] == DBNull.Value ||
                                                         row["Reported Quality Measures"].ToString() == "N"
                                                             ? 0
                                                             : 1;
                    tRow["ParticipatesInEHR"] = row["Used electronic health records"] == DBNull.Value ||
                                                        row["Used electronic health records"].ToString() == "N"
                                                            ? 0
                                                            : 1;

                    //tRow["ParticipatesInERX"] = row["Participating in eRx"] == DBNull.Value ||
                    //                                    row["Participating in eRx"].ToString() == "N"
                    //                                        ? 0
                    //                                        : 1;
                    //tRow["ParticipatesInPQRS"] = row["Participating in PQRS"] == DBNull.Value ||
                    //                                     row["Participating in PQRS"].ToString() == "N"
                    //                                         ? 0
                    //                                         : 1;
                    //tRow["ParticipatesInEHR"] = row["Participating in EHR"] == DBNull.Value ||
                    //                                    row["Participating in EHR"].ToString() == "N"
                    //                                        ? 0
                    //                                        : 1;
                    tRow["IsEdited"] = 1;

                    _physiciansTable.Rows.Add(tRow);

                    rowCount++;

                    Events.GetEvent<PleaseStandByMessageUpdateEvent>()
                          .Publish("Importing line " + rowCount.ToString());

                    
                }

                //if (_physiciansTable.Rows.Count == BATCH_SIZE)
                //{
                BulkInsert(_physiciansTable, _configurationService.ConnectionSettings.ConnectionString,
                           _physiciansTable.Rows.Count);
                CountInserted = _physiciansTable.Rows.Count;
                //}_

                foreach (var contextState in actualStates)
                {
                    var physicianScript = ReflectionHelper.ReadEmbeddedResourceFile(GetType().Assembly,
                                                                                    "Monahrq.Sdk.Resources.PhysiciansImport.ImportUpdatePhysicians.sql")
                                                          .Replace("[@@State@@]", contextState);
                    var medPracticeScript = ReflectionHelper.ReadEmbeddedResourceFile(GetType().Assembly,
                                                                                      "Monahrq.Sdk.Resources.PhysiciansImport.ImportUpdateMedicalPractices.sql")
                                                            .Replace("[@@State@@]", contextState);
                    var physicianMedPracticeScript = ReflectionHelper.ReadEmbeddedResourceFile(GetType().Assembly,
                                                                                               "Monahrq.Sdk.Resources.PhysiciansImport.ImportUpdatePhysiciansMedPractices.sql")
                                                                     .Replace("[@@State@@]", contextState);

                    var completQuery = new StringBuilder();

                    completQuery.AppendLine(physicianScript);
                    completQuery.AppendLine(medPracticeScript);
                    completQuery.AppendLine(physicianMedPracticeScript);

                    var combinedQueries = completQuery.ToString();

                    using (var session = Provider.SessionFactory.OpenStatelessSession())
                    {
                        using (var trans = session.BeginTransaction())
                        {
                            session.CreateSQLQuery(combinedQueries)
                                   .SetTimeout(10000)
                                   .ExecuteUpdate();

                            trans.Commit();
                        }
                    }

                    //CountInserted = GetTotalItemsSaved(typeof(Physician), "Npi",
                    //                                              string.Format("Where [States]='{0}';", contextState));
                    //var totalMedicalPracticesSaved = GetTotalItemsSaved(typeof (MedicalPractice), "GroupPracticePacId",
                    //                                                    string.Format("Where [State]='{0}';",
                    //                                                                  contextState));
                }

                Events.GetEvent<PleaseStandByMessageUpdateEvent>().Publish("Finalizing import...");

                // what is the purpose of this delay???
                const int MAX_DELAY_SECONDS = 3;

                var elapsed = DateTime.Now - startTime;
                var seconds = elapsed.TotalSeconds;
                var remaining = MAX_DELAY_SECONDS - seconds;
                if (remaining > 0)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(remaining));
                }
            }
            catch (IOException exc)
            {

                Logger.Write(exc, "Error importing data from file {0}", dlg.FileName);

                var message = string.Format("Please close file\"{0}\" before trying to import.",
                                            dlg.FileName.SubStrAfterLast(@"\"));
                MessageBox.Show(message, "MONAHRQ Import File Open Error", MessageBoxButton.OK);
            }
            catch (Exception exc)
            {
                Logger.Write(exc, "Error importing data from file {0}", dlg.FileName);
                ImportErrors.Add(ImportError.Create("Physician", "Physician", exc.GetBaseException().Message));
            }
            finally
            {
                EndImport();

                if (ImportErrors.Any())
                {
                    //Task.Factory.StartNew(() =>
                    //{
                    using (var sw = new StreamWriter(ErrorFile, true))
                    {
                        foreach (var error in ImportErrors)
                        {
                            WriteError2(sw, error.ErrorMessage);
                            sw.WriteLine();

                            NumberOfErrors++;
                        }
                    }
                    //});
                }

                OnImported(this, EventArgs.Empty);
                Events.GetEvent<PleaseStandByMessageUpdateEvent>().Publish("Import Complete");
                Events.GetEvent<SimpleImportCompletedEvent>().Publish(this);
            }
        }

        /// <summary>
        /// Ends the import.
        /// </summary>
        protected override void EndImport()
        {
            base.EndImport();

            _physiciansTable.Clear();
            _physiciansTable.Dispose();

            ClearTempTable();

            //NumberOfErrors = 0;
            //CountInserted = 0;
            //ImportErrors = new List<ImportError>();
        }

        /// <summary>
        /// Clears the temporary table.
        /// </summary>
        private void ClearTempTable()
        {
            using (var session = Provider.SessionFactory.OpenStatelessSession())
                session.CreateSQLQuery("truncate table [dbo].[" + _physiciansTable.TableName + "];")
                       .ExecuteUpdate();
        }

        /// <summary>
        /// Checks if already exists.
        /// </summary>
        /// <param name="npi">The npi.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        private bool CheckIfAlreadyExists(long npi, string state)
        {
            var returnedValue = new List<long>();

            using (var session = Provider.SessionFactory.OpenStatelessSession())
                returnedValue = session.Query<Physician>().Where(p => p.Npi.Value == npi).Select(p => p.Npi.Value).ToList();

            return returnedValue.Any();
        }

        /// <summary>
        /// Gets the total items saved.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="columnToCheck">The column to check.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <returns></returns>
        private int GetTotalItemsSaved(Type entityType, string columnToCheck, string whereClause)
        {
            var query = new StringBuilder();
            query.AppendLine(string.Format("SELECT Count(Distinct [{0}]) from [dbo].[{1}] ", columnToCheck, entityType.EntityTableName()));
            query.AppendLine(whereClause);

            using (var session = Provider.SessionFactory.OpenStatelessSession())
            {
                return session.CreateSQLQuery(query.ToString()).UniqueResult<int>();
            }
        }

        /// <summary>
        /// Inserts the specified entity using sqlbulkcopy.
        /// </summary>
        /// <param name="targetTable">The target date table.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="batchSize">Size of the batch.</param>
        static void BulkInsert(DataTable targetTable, string connectionString, int batchSize = BATCH_SIZE)
        {
            var targetTableName = string.Format("[dbo].[{0}]", targetTable.TableName);

            // Open a sourceConnection to the AdventureWorks database. 
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.DestinationTableName = targetTableName;
                        bulkCopy.BatchSize = targetTable.Rows.Count < batchSize ? targetTable.Rows.Count : batchSize;
                        bulkCopy.BulkCopyTimeout = 10000;

                        foreach (DataColumn column in targetTable.Columns)
                            bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName,
                                                                                     column.ColumnName));
                        // Write from the source to the destination.
                        bulkCopy.WriteToServer(targetTable);
                    }

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Processes the values.
        /// </summary>
        /// <param name="vals">The vals.</param>
        /// <param name="errors">The errors.</param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void ProcessValues(string[] vals, IList<ImportError> errors)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="vals">The vals.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected override string GetName(string[] vals)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the expected count of values per line.
        /// </summary>
        /// <value>
        /// The expected count of values per line.
        /// </value>
        /// <exception cref="NotImplementedException"></exception>
        protected override int ExpectedCountOfValuesPerLine
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the continue prompt.
        /// </summary>
        /// <value>
        /// The continue prompt.
        /// </value>
        protected override string ContinuePrompt
        {
            get { return "Importing this file may overwrite existing physicians and/or medical practices, " +
                    "if any.\n\n Do you want to continue?";
             }
        }

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <value>
        /// The file extension.
        /// </value>
        protected override string FileExtension
        {
            get { return ".csv"; }
        }
    }
}
