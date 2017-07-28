using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Logging;
using Monahrq.Wing.HospitalCompare.Model;
using NHibernate;

namespace Monahrq.Wing.HospitalCompare.Helpers
{
    public sealed class ImportProcess
    {
        private static readonly string TargetTableName = typeof(HospitalCompareTarget).EntityTableName();

        static ImportProcess()
        {
            ConfigurationService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            Logger = ServiceLocator.Current.GetInstance<ILogWriter>(LogNames.Session);
            SessionFactoryProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
        }

        // variables that do not change
        private static readonly ILogWriter Logger;
        private static readonly IConfigurationService ConfigurationService;
        private static readonly IDomainSessionFactoryProvider SessionFactoryProvider;

        // variables that are set during the course of the import procedure
        private Action<string> progressCallbackAction;
        private ImportConfiguration config;
        private string originalPath;

        public ImportConfiguration Execute(ImportType type, string path, int datasetId, Action<string> progressCallback)
        {
            this.progressCallbackAction = progressCallback;
            
            this.config = new ImportConfiguration();
            this.config.ImportType = type;

            this.originalPath = path;

            progressCallback("Preparing to load data...");
            using (var sourceDataConnection = this.GetSourceConnection(path))
            using (var targetSqlConnection = this.GetTargetConnection())
            using (var workingTable = this.GetWorkingTable(targetSqlConnection))
            using (var session = SessionFactoryProvider.SessionFactory.OpenStatelessSession(targetSqlConnection))
            {
                // prepare bulk loader 
                var bulkLoader = new SqlBulkCopy(targetSqlConnection);
                bulkLoader.DestinationTableName = TargetTableName;

                foreach (var cmd in this.GetImportCommands(datasetId))
                {
                    int ct;

                    if (cmd.ExecuteAgainstTarget)
                    {
                        ct = session
                            .CreateSQLQuery(cmd.Command)
                            .ExecuteUpdate();
                    }
                    else
                        ct = this.GetImportBatch(cmd, bulkLoader, workingTable, sourceDataConnection, datasetId);

                    progressCallback(string.Format("{0} ({1} rows loaded)", cmd.Description, ct));
                }
            }

            return this.config;
        }

        private OleDbConnection GetSourceConnection(string path)
        {
            var workingPath = this.GetWorkingPath(path);
            var conn = this.OpenConnection(workingPath);

            // identify source schema version
            this.DetectImportConfiguration(conn);
            this.progressCallbackAction("Detected schema version: " + this.config.SchemaVersion);
            Logger.Information("CMS Dataset AccessDBSchemaVersion: {0}", this.config.SchemaVersion);
            if (!this.config.IsValidSchemaVersion)
            {
                conn.Dispose();
                throw new Exception("Unsupported data source format");
            }

            return conn;
        }

        private SqlConnection GetTargetConnection()
        {
            var c = new SqlConnection(ConfigurationService.ConnectionSettings.ConnectionString);
            c.Open();
            return c;
        }

        private string GetWorkingPath(string path)
        {
            if (this.config.ImportType != ImportType.ZippedCsvDir)
                // no special handling required
                return path;

            // user provided a zip file; extract it and return the path of the extracted files
            var tempDirectory = Path.Combine(
                Environment.ExpandEnvironmentVariables("%TEMP%"),
                Guid.NewGuid().ToString("D"));
            while (Directory.Exists(tempDirectory))
            {
                tempDirectory = Path.Combine(
                    Environment.ExpandEnvironmentVariables("%TEMP%"),
                    Guid.NewGuid().ToString("D"));
            }
            config.TemporaryDirectory = Directory.CreateDirectory(tempDirectory);

            this.progressCallbackAction?.Invoke(string.Format("Extracting file '{0}' to temporary directory", path));
            ZipFile.ExtractToDirectory(path, tempDirectory);
            return tempDirectory;
        }

        private OleDbConnection OpenConnection(string workingPath)
        {
            // construct connection string to 
            string connectionString;
            if (!ConfigurationService.DataAccessComponentsInstalled) // TODO: After Beta Release to a smart check for Access Data Components to resolve access connection string
                connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + workingPath + ";Jet OLEDB:System Database=" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Access\\system.mdw";
            else
                connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + workingPath + ";Persist Security Info=False;";

            // write CSV parameters to connection string *and* schema.ini (https://stackoverflow.com/questions/5182767/microsoft-ace-oledb-12-0-csv-connectionstring)
            if (this.config.IsCsvImport)
            {
                connectionString += "Extended Properties=\"Text;HDR=Yes\"";
                this.WriteCsvSchemaFile(workingPath);
            }

            // create connection
            try
            {
                var c = new OleDbConnection(connectionString);
                c.Open();
                return c;
            }
            catch (Exception e)
            {
                throw new Exception("Error opening connection to data source", e);
            }
        }

        private void WriteCsvSchemaFile(string workingPath)
        {
            var foundFiles = false;
            try
            {
                using (var schemaFileStream = File.Open(Path.Combine(workingPath, "schema.ini"),
                        FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var schemaFile = new StreamWriter(schemaFileStream, Encoding.ASCII))
                {
                    foreach (var file in Directory.GetFiles(workingPath, "*.csv"))
                    {
                        foundFiles = true;
                        schemaFile.WriteLine("[{0}]", Path.GetFileName(file));
                        schemaFile.WriteLine("Format=CSVDelimited");
                        schemaFile.WriteLine("ColNameHeader=True");
                        schemaFile.WriteLine("MaxScanRows=0");
                        // we need the DB engine to look at every row to determine type; otherwise we won't catch the occasional string value in an int column
                        schemaFile.WriteLine("CharacterSet=ANSI");
                        schemaFile.WriteLine();
                    }
                    schemaFile.Flush();
                    schemaFile.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error preparing the data for import. Verify that you have write access to the directory " + workingPath + ".", e);
            }
            if (!foundFiles)
                throw new Exception(this.config.ImportType == ImportType.CsvDir
                    ? "Unable to find any CSV files in the specified directory, " + workingPath
                    : "Unable to find any CSV files in the specified archive, " + this.originalPath);
        }

        private void DetectImportConfiguration(OleDbConnection conn)
        {
            try
            {
                var tables = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                    new object[] { null, null, null, "TABLE" });
                foreach (DataRow row in tables.Rows)
                {
                    config.SourceTables.Add(row[2].ToString());
                    DateTime? tempDate = row[7] != null ? DateTime.Parse(row[7].ToString()) : (DateTime?)null;
                    if (tempDate != null)
                        config.DatabaseCreationDate = tempDate;
                }
                var tableName =
                    config.SourceTables.FirstOrDefault(
                        table => table.StartsWith("Hvbp_ami_", StringComparison.InvariantCultureIgnoreCase));
                if (config.IsCsvImport)
                {
                    if (tableName != null && tableName.EndsWith("csv", StringComparison.InvariantCultureIgnoreCase))
                        tableName = tableName.Substring(0, tableName.Length - 4);
                    else
                        throw new Exception(
                            "CSV import data was not in the expected format. Expected \"#csv\" table name suffix which was not found.");
                }

                // Get version Information.
                var sqlStatement = new StreamReader(@"Resources\HospitalCompare\version.sql").ReadToEnd();
                sqlStatement = sqlStatement.Replace("[@@Table_Schema_Name@@]", tableName);

                using (var oleCmd = new OleDbCommand(sqlStatement, conn))
                {
                    var oleRdr = oleCmd.ExecuteReader();
                    if (oleRdr.Read())
                    {
                        config.SchemaVersionMonth = oleRdr[oleRdr.GetOrdinal("SchemaVersionMonth")].ToString();
                        config.SchemaVersionYear = oleRdr[oleRdr.GetOrdinal("SchemaVersionYear")].ToString();
                        config.SchemaVersion = oleRdr[oleRdr.GetOrdinal("SchemaVersion")].ToString();

                        config.MeasureColumnMappings = Enumerable
                            .Range(0, oleRdr.FieldCount).Select(oleRdr.GetName).Where(n => n.StartsWith("Measures_"))
                            .Select(x => new KeyValuePair<string, string>(x, oleRdr[oleRdr.GetOrdinal(x)].ToString()))
                            .ToList();
                        if (config.IsCsvImport)
                            // not used in CSV import
                            config.TableColumnMappings = new List<KeyValuePair<string, string>>();
                        else
                            config.TableColumnMappings = Enumerable
                                .Range(0, oleRdr.FieldCount)
                                .Select(oleRdr.GetName)
                                .Where(n => n.StartsWith("MDB_TABLE_"))
                                .Select(
                                    x => new KeyValuePair<string, string>(x, oleRdr[oleRdr.GetOrdinal(x)].ToString()))
                                .ToList();
                    }
                }

                // special handling for recent hospital compare schemas 
                if (tableName == "hvbp_ami_11_14_2016")
                {
                    // 2016 and 2017v1 use the same schema. 2017v2 uses a newer schema.
                    if (config.SourceTables
                        .Any(t => t.StartsWith("MORT_READM_July2017", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        config.SchemaVersion = "Schema.Version.5";
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error detecting source data set schema version", e);
            }
        }

        private DataTable GetWorkingTable(SqlConnection targetSqlConnection)
        {
            var workingTable = new DataTable();
            using (var sqlDa = new SqlDataAdapter(
                "SELECT TOP 0 * FROM " + TargetTableName,
                targetSqlConnection))
            {
                sqlDa.Fill(workingTable);
            }
            return workingTable;
        }

        private IEnumerable<ImportCommand> GetImportCommands(int datasetId)
        {
            //  ImportSql file has deliberate (*read: weird) structure.  
            //	The file is divided into sections that are all run independently.   The sections are
            //	separated by a comment line with format: --<text>--.  Note section separators start and end with the comment symbol "--"
            //	Note: MS Access does not allow for inline comments, so dont try to use them....
            //	Note: Algorithm is dependent on the first text in the file being a comment header.  i.e. starting with: "--"
            //	New: if a section header contains text: "[GLOBAL]":
            //		a) the section will not be run as a independent section, but instead
            //		b) the section will be prepended to all other run sections on their run.
            //	New: if a section header contains text: "[MONAHRQ-DB]":
            //		The query will be ran against the Monahrq Database.

            var importFilename = !this.config.IsCsvImport
                ? @"Resources\HospitalCompare\" + this.config.SchemaVersion + ".sql"
                : @"Resources\HospitalCompare\" + this.config.SchemaVersion + ".csv.sql";
            var importSqls = new StreamReader(importFilename).ReadToEnd()
                .Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries);

            // identify statement that need to be prefixed to every import command
            var globalSql = new StringBuilder();
            for (var i = 0; i < importSqls.Length; i += 2)
                if (importSqls[i].Contains("[GLOBAL]"))
                    globalSql.AppendLine(importSqls[i + 1]);
            var globalSqls = globalSql.ToString();

            for (var i = 0; i < importSqls.Length; i += 2)
            {
                var header = importSqls[i];
                var sqlStatement = importSqls[i + 1];

                if (header.Contains("[GLOBAL]"))
                    continue; // Skip GLOBAL sqls; they've already been extracted to globalSqls.

                //  Replace placeholders in <schema>.SQL [mmmyyyy] with values obtained from version.sql

                var measureGroupCount = 0;
                var measureGroupIgnoreCount = 0;
                foreach (var col in this.config.MeasureColumnMappings)
                {
                    //  Tally the count used measure group validity.
                    if (sqlStatement.Contains(String.Format("@@{0}@@", col.Key)))
                    {
                        ++measureGroupCount; // This measure group is being used.
                        if (col.Value.EqualsAny("unused", "unknown"))
                            ++measureGroupIgnoreCount;
                        // This measure group is not set to a valid/useful value.
                    }

                    sqlStatement = sqlStatement.Replace(
                        String.Format("@@{0}@@", col.Key),
                        String.Join(",", col.Value.Split(',').Select(val => String.Format("'{0}'", val))));
                }
                //  Checks if this query is valid for this Dataset (version).
                //	- Determines if all used measure groups were Invalid groups.  If so, skip this query.
                //	- At least one measure group has to be used for this result to matter.
                if (measureGroupCount > 0 && measureGroupCount == measureGroupIgnoreCount)
                    continue;

                foreach (var col in this.config.TableColumnMappings)
                {
                    String colValue = col.Value; // Needed to avoid lambda/dynamic error.
                    sqlStatement = sqlStatement.Replace(
                        String.Format("@@{0}@@", col.Key),
                        String.Join(",", colValue.Split(',').Select(val => String.Format("{0}", val))));
                }

                var executeAgainstTarget = header.Contains("[MONAHRQ-DB]");
                if (executeAgainstTarget)
                    sqlStatement = sqlStatement.Replace(
                        String.Format("@@{0}@@", "Dataset_id"),
                        datasetId.ToString());
                else if (globalSqls.Length > 0)
                    sqlStatement = globalSqls + sqlStatement;

                yield return new ImportCommand
                {
                    ExecuteAgainstTarget = executeAgainstTarget,
                    Command = sqlStatement,
                    Description = header
                };
            }
        }

        private int GetImportBatch(ImportCommand cmd,
            SqlBulkCopy bulkLoader,
            DataTable workingTable,
            OleDbConnection sourceDataConnection,
            int datasetId)
        {
            using (var oleCmd = new OleDbCommand(cmd.Command, sourceDataConnection))
            using (var oleRdr = oleCmd.ExecuteReader())
            {
                while (oleRdr.Read())
                {
                    var sqlRow = workingTable.NewRow();
                    sqlRow["Dataset_id"] = datasetId;
                    for (var c = 0; c < oleRdr.FieldCount; c++)
                    {
                        var name = oleRdr.GetName(c);
                        object val;
                        if (name.EqualsIgnoreCase("Rate"))
                        {
                            try
                            {
                                val = oleRdr[name];
                                if (string.Equals(val as string, "Not Available",
                                    StringComparison.InvariantCultureIgnoreCase))
                                    val = -1;
                            }
                            catch (InvalidOperationException)
                            {
                                val = -1;
                            }
                        }
                        else if (name.EqualsIgnoreCase("CMSProviderID"))
                        {
                            // CSV reads sometimes strip out leading zeros despite treating data as a string
                            var strVal = oleRdr[name].ToString();
                            val = strVal.PadLeft(6, '0');
                        }
                        else
                            val = oleRdr[name];

                        sqlRow[name] = val;
                    }
                    workingTable.Rows.Add(sqlRow);
                }
                try
                {
                    bulkLoader.WriteToServer(workingTable);
                }
                catch (SqlException e)
                {
                    //todo: catch schema changed exception and retry
                    throw;
                }
                var count = workingTable.Rows.Count;
                workingTable.Clear();
                return count;
            }
        }
    }
}
