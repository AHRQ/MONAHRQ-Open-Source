using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Extensibility.Data;
using Monahrq.Sdk.Logging;
using Monahrq.Wing.NursingHomeCompare.NHC.Model;
using NHibernate.Linq;
using PropertyChanged;

namespace Monahrq.Wing.NursingHomeCompare.NHC.ViewModel
{
	/// <summary>
	/// Imports a NHCompare file.
	/// </summary>
	/// <seealso cref="NursingHomeTarget" />
	[ImplementPropertyChanged]
    public class ProcessFileViewModel : SessionedViewModelBase<WizardContext, NursingHomeTarget>
    {

		/// <summary>
		/// The session logger
		/// </summary>
		private readonly SessionLogger _sessionLogger = new SessionLogger(new CallbackLogger());
		// Need to update Access DB tables - stand alone app?
		// Need to transfer data from Access to SQL - Stand alone app?
		// Need to setup SProcs - Part of Monahrq 5 setup

		// CrunchCMS - run at time of website creation?

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="ProcessFileViewModel"/> is cancelled.
		/// </summary>
		/// <value>
		///   <c>true</c> if cancelled; otherwise, <c>false</c>.
		/// </value>
		private bool Cancelled { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="ProcessFileViewModel"/> is done.
		/// </summary>
		/// <value>
		///   <c>true</c> if done; otherwise, <c>false</c>.
		/// </value>
		public bool Done { get; set; }
		/// <summary>
		/// Gets or sets the access connection string.
		/// </summary>
		/// <value>
		/// The access connection string.
		/// </value>
		private string AccessConnString { get; set; }
		/// <summary>
		/// Gets or sets the SQL connection string.
		/// </summary>
		/// <value>
		/// The SQL connection string.
		/// </value>
		private string SqlConnString { get; set; }
		/// <summary>
		/// Gets or sets the SQL connection.
		/// </summary>
		/// <value>
		/// The SQL connection.
		/// </value>
		private SqlConnection SqlConn { get; set; }
		/// <summary>
		/// Gets or sets the access connection.
		/// </summary>
		/// <value>
		/// The access connection.
		/// </value>
		private OleDbConnection AccessConn { get; set; }
		//private const int batchSize = 1000;
		/// <summary>
		/// Gets or sets the log file.
		/// </summary>
		/// <value>
		/// The log file.
		/// </value>
		public ObservableCollection<string> LogFile { get; set; }
		/// <summary>
		/// Gets or sets the wing data set.
		/// </summary>
		/// <value>
		/// The wing data set.
		/// </value>
		public Dataset WingDataSet { get; set; }
		/// <summary>
		/// Gets or sets the repository.
		/// </summary>
		/// <value>
		/// The repository.
		/// </value>
		public IExtensionRepository<NursingHomeTarget> Repository { get; set; }
		/// <summary>
		/// Occurs when [notify UI].
		/// </summary>
		public event EventHandler<ExtendedEventArgs<Action>> NotifyUi = delegate { };
		//private List<Tuple<string, string>> ModifySqlStatements { get; set; }
		//private ITransaction Transaction { get; set; }
		//private DataTable Hospitals { get; set; }
		//private DataTable Footnotes { get; set; }
		//private DataTable HospCompDataDt { get; set; }
		//private SqlBulkCopy HospCompDataBc { get; set; }
		//private int Lines { get; set; }
		/// <summary>
		/// Gets or sets the dataset identifier.
		/// </summary>
		/// <value>
		/// The dataset identifier.
		/// </value>
		private int DatasetID { get; set; }

		/// <summary>
		/// The start
		/// </summary>
		DateTime _start;
		/// <summary>
		/// The end
		/// </summary>
		DateTime _end;

		/// <summary>
		/// The configuration service
		/// </summary>
		readonly IConfigurationService _configurationService;

		/// <summary>
		/// The synchronize lock
		/// lock object for synchronization;
		/// </summary>
		private static readonly object _syncLock = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="ProcessFileViewModel"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		public ProcessFileViewModel(WizardContext context)
            : base(context)
        {
            _configurationService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            SqlConnString = _configurationService.ConnectionSettings.ConnectionString;
            //LogFile = new BackgroundObservableCollection<string>();
            LogFile = new ObservableCollection<string>();
            BindingOperations.EnableCollectionSynchronization(LogFile, _syncLock);
            Done = false;
            Cancelled = false;
        }

		/// <summary>
		/// Gets the display name.
		/// </summary>
		/// <value>
		/// The display name.
		/// </value>
		public override string DisplayName
        {
            get { return "Import Data"; }
        }

		/// <summary>
		/// Returns true if ... is valid.
		/// </summary>
		/// <returns></returns>
		public override bool IsValid()
        {
            return Done;
        }

		/// <summary>
		/// Starts the import.
		/// </summary>
		public override void StartImport()
        {
            base.StartImport();
            LogFile.Clear();
            _start = DateTime.Now;
            AppendLog("Starting import.");
            AppendLog("Opening database connections.");
            //-------------
            // Open connection to SQL server
            try
            {
                SqlConn = new SqlConnection(SqlConnString);
                SqlConn.Open();
            }
            catch (Exception e)
            {
                AppendLog("Error opening SQL server connection:");
                AppendLog(e.Message);
                NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));

                return;     // don't take further steps if this fails
            }

            try
            {
                // Open the connection to the Access DB
                if (!_configurationService.DataAccessComponentsInstalled) // TODO: After Beta Release to a smart check for Access Data Components to resolve access connection string
                    AccessConnString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + DataContextObject.FileName + ";Jet OLEDB:System Database=" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Access\\system.mdw";
                else
                    AccessConnString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + DataContextObject.FileName;

                AccessConn = new OleDbConnection(AccessConnString);
                AccessConn.Open();
            }
            catch (Exception e)
            {
                AppendLog("Error opening Access database connection:");
                AppendLog(e.Message);
                NotifyUi(this, new ExtendedEventArgs<Action>(AbortImport));
                return;     // don't take further steps if this fails
            }

            OleDbCommand oleCmd;
            OleDbDataReader oleRdr;
            string sqlStatement;

            List<string> accessTables = new List<string>();
            // oleCmd = new OleDbCommand("SELECT Name, DateCreate from MSysObjects Where Type=1 and Flags=0", AccessConn);
            
            var tables = AccessConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

            //Console.WriteLine("The tables are:");
            foreach (DataRow row in tables.Rows)
                accessTables.Add(row[2].ToString());

            List<string> columnNames = new List<string>();

            foreach (string accessTableName in accessTables)
            {
                sqlStatement = "SELECT TOP 1 * from [" + accessTableName + "]";
                using (oleCmd = new OleDbCommand(sqlStatement, AccessConn))
                {
                    oleRdr = oleCmd.ExecuteReader();
                    if (oleRdr != null)
                    {
                        while (oleRdr.Read())
                        {
                            for (int c = 0; c < oleRdr.FieldCount; c++)
                            {
                                columnNames.Add(oleRdr.GetName(c));
                            }
                        }
                    }
                }
            }

            columnNames.Sort();

            var columnList = string.Join(",", columnNames);

            string columnListMd5Hash;
            using (MD5 md5Hash = MD5.Create())
            {
                columnListMd5Hash = GetMd5Hash(md5Hash, columnList);
            }

            string accessDBSchemaVersion = null;
            string reportProcessVersion = null;
            string isVesionMatched = null;
            string[] lines = File.ReadAllLines(@"Resources\NursingHomeCompare\version.txt");
            foreach (var line in lines)
            {
                var columnListMd5Version = line.Split(',')[0];
                accessDBSchemaVersion = line.Split(',')[1];
                reportProcessVersion = line.Split(',')[2];
                isVesionMatched = string.Compare(columnListMd5Version, columnListMd5Hash, StringComparison.OrdinalIgnoreCase).ToString();
                if (isVesionMatched == "0")
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(isVesionMatched) || isVesionMatched != "0")
            {
                ImportCompleted(false);
                AppendLog("There has been a problem loading this Dataset.");
                AppendLog("You might be downloading an incorrect version of the Nursing Home Compare database, which is unsupported.");
                AppendLog("MONAHRQ supports MS Access file type. For more information on Nursing Home databases that are supported in MONAHRQ, please visit http://www.ahrq.gov/professionals/systems/monahrq/resources/index.html.");
                AppendLog("Please check the dataset you are trying to import and try again.");
            }
            else
            {
				try
				{
					AppendLog("AccessDBSchemaVersion : " + accessDBSchemaVersion);
					//---- NH provider --//
					using (var cleanCommand = SqlConn.CreateCommand())
					{
						cleanCommand.CommandText = "delete from [NursingHomes]";
						cleanCommand.ExecuteNonQuery();
					}

					var nhpTableName = typeof(NursingHome).EntityTableName();
					sqlStatement = "SELECT TOP 0 * FROM " + nhpTableName;    // Dummy select to return 0 rows.
					var nhpDataDt = new DataTable();
					using (var sqlDa = new SqlDataAdapter(sqlStatement, SqlConn))
					{
						sqlDa.Fill(nhpDataDt);
					}

					using (var nhpDataBc = new SqlBulkCopy(SqlConn))
					{
						nhpDataBc.DestinationTableName = nhpTableName;
						var importNHPSqls = ReadFileContent(@"Resources\NursingHomeCompare\" + accessDBSchemaVersion + "_Provider.sql");
						using (oleCmd = new OleDbCommand(importNHPSqls, AccessConn))
						{
							AppendLog("Getting Nursing Homes data from Access.");
							oleRdr = oleCmd.ExecuteReader();

							if (oleRdr != null)
							{
								while (oleRdr.Read())
								{
									var sqlRow = nhpDataDt.NewRow();
									for (var c = 0; c < oleRdr.FieldCount; c++)
									{
										sqlRow[oleRdr.GetName(c)] = oleRdr[oleRdr.GetName(c)];
									}
									nhpDataDt.Rows.Add(sqlRow);
								}
							}
						}

						AppendLog("Saving Nursing Homes data to SQLserver.");

						//Finding FileDate to update Title for dataset. 
						if (nhpDataDt.Columns.Contains("FileDate") && nhpDataDt.Rows.Count > 0 && DataContextObject.DatasetItem != null)
						{
							var dt = (DateTime)nhpDataDt.Rows[0].ItemArray[nhpDataDt.Columns["FileDate"].Ordinal];
							var item = DataContextObject.DatasetItem;
							item.File = string.Format("{0} [Dataset release date: {1}]", item.File, dt.ToString("MMMM dd, yyyy"));
							item.Description = dt.ToShortDateString();
							item.ReportProcessVersion = reportProcessVersion;
							DataContextObject.SaveImportEntry(item);
						}

						nhpDataBc.WriteToServer(nhpDataDt);
						nhpDataDt.Clear();
					}

					//-------------------//

					//--- apply user customization ---//
					var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
					var updateText = string.Empty;
					using (var conn = new SqlConnection(configService.ConnectionSettings.ConnectionString))
					{
						conn.Open();
						using (var command = conn.CreateCommand())
						{
							command.CommandText = "SELECT [ProviderId],[PropertyName],[NewPropertyValue] from [dbo].[NursingHomes_Audits] WHERE  [ProviderId] IS NOT NULL AND [PropertyName] IS NOT NULL";
							var dataRead = command.ExecuteReader();

							while (dataRead.Read())
							{
								updateText += string.Format("Update [dbo].[NursingHomes] set [{0}]='{1}' where [ProviderId]='{2}'; {3}", dataRead[1], dataRead[2], dataRead[0], Environment.NewLine);
							}
						}
					}

					if (!string.IsNullOrEmpty(updateText))
					{
						using (var updateConn = new SqlConnection(configService.ConnectionSettings.ConnectionString))
						{
							updateConn.Open();
							using (var updateCommand = updateConn.CreateCommand())
							{
								updateCommand.CommandText = updateText;
								updateCommand.ExecuteNonQuery();
							}
						}
					}
					//--------------------------------//

					var targetTableName = typeof(NursingHomeTarget).EntityTableName();
					sqlStatement = "SELECT TOP 0 * FROM " + targetTableName;    // Dummy select to return 0 rows.
					var nhTargetDataDt = new DataTable();
					using (var sqlDa = new SqlDataAdapter(sqlStatement, SqlConn))
					{
						sqlDa.Fill(nhTargetDataDt);
					}

					using (var nhTargetDataBc = new SqlBulkCopy(SqlConn))
					{
						nhTargetDataBc.DestinationTableName = targetTableName;

						DatasetID = DataContextObject.DatasetItem.Id;
						var importSqls = ReadFileContent(@"Resources\NursingHomeCompare\" + accessDBSchemaVersion + "_Target.sql").Split(new[] { "--" }, StringSplitOptions.RemoveEmptyEntries);
						for (int i = 0; i < importSqls.Length; i += 2)
						{
							AppendLog(importSqls[i]);

							sqlStatement = importSqls[i + 1];
							using (oleCmd = new OleDbCommand(sqlStatement, AccessConn))
							{
								AppendLog("Getting Target data from Access.");
								oleRdr = oleCmd.ExecuteReader();

								if (oleRdr != null)
								{
									while (oleRdr.Read())
									{
										DataRow sqlRow = nhTargetDataDt.NewRow();
										sqlRow["Dataset_id"] = DatasetID;
										for (int c = 0; c < oleRdr.FieldCount; c++)
										{
											sqlRow[oleRdr.GetName(c)] = oleRdr[oleRdr.GetName(c)];
										}
										nhTargetDataDt.Rows.Add(sqlRow);
									}
								}
							}
							AppendLog("Saving Target data to SQLserver.");
							nhTargetDataBc.WriteToServer(nhTargetDataDt);
							nhTargetDataDt.Clear();

						}
					}
					ImportCompleted(true);
				}
				catch (Exception ex)
				{
					ImportCompleted(false);
					AppendLog(String.Format("Nursing Home Compare Exception: {0}",ex.Message));
				}
			}
        }

		/// <summary>
		/// Reads the content of the file.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns></returns>
		public string ReadFileContent(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName)) return string.Empty;

            return File.ReadAllText(fileName);
        }

		/// <summary>
		/// Gets the MD5 hash.
		/// </summary>
		/// <param name="md5Hash">The MD5 hash.</param>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		protected string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }

		/// <summary>
		/// Imports the completed.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		protected override void ImportCompleted(bool success)
        {
            _end = DateTime.Now;
            var elapsed = _end.Subtract(_start);
            AppendLog(elapsed.ToString("hh\\:mm\\:ss") + " elapsed.");

            AppendLog("Closing database connections.");
            try
            {
                AccessConn.Close();
                AccessConn.Dispose();
            }
            catch (Exception e)
            {
                AppendLog("******************************************************************************");
                AppendLog("Error closing Access database table:");
                AppendLog(e.Message);
                AppendLog("******************************************************************************");
            }

            try
            {
                //if (HospCompDataBc != null)
                //{
                //    HospCompDataBc.Close();
                //    HospCompDataDt.Dispose();
                //}
                SqlConn.Close();
                SqlConn.Dispose();
            }
            catch (Exception e)
            {
                AppendLog("******************************************************************************");
                AppendLog("Error closing SQL database table:");
                AppendLog(e.Message);
                AppendLog("******************************************************************************");
            }

            try
            {
                if (success)
                {
                    var rootXml = new XElement("LogLines");
                    foreach (var line in LogFile)
                    {
                        rootXml.Add(new XElement("LogLine", line));
                    }
                    DataContextObject.Summary = rootXml.ToString();
                    DataContextObject.Summary = rootXml.ToString();

                    var fi = new DateTimeFormatInfo();
                    DataContextObject.DatasetItem.VersionMonth = fi.GetMonthName(DataContextObject.Month);
                    DataContextObject.DatasetItem.VersionYear = DataContextObject.Year;

                    if (DataContextObject.DatasetItem.IsReImport)
                    {
                        var linesImported = Session.Query<NursingHomeTarget>()
                                                   .Count(hc => hc.Dataset.Id == DataContextObject.DatasetItem.Id);

                        if (DataContextObject.DatasetItem.File.Contains(" (#"))
                            DataContextObject.DatasetItem.File = DataContextObject.DatasetItem.File.SubStrBefore(" (#");

                        DataContextObject.DatasetItem.File += " (# Rows Imported: " + linesImported + ")";
                    }
                    AppendLog("Import completed successfully.");
                }
                else
                {
                    AppendLog("Import was not completed successfully.");
                }

                Done = true;
                // WPF CommandManager periodically calls IsValid to see if the Next/Done button should be enabled. 
                // In multi-threaded wizard steps, IsValid returns the value of the Done flag. Call InvalidateRequerySuggested here
                // on the UI thread after setting the Done flag to force WPF to call IsValid now so the Next/Done button will become enabled. 
                //Application.Current.DoEvents();
                NotifyUi(this, new ExtendedEventArgs<Action>(CommandManager.InvalidateRequerySuggested));
            }
            finally
            {
                base.ImportCompleted(success);
            }
        }

		/// <summary>
		/// Aborts the import.
		/// </summary>
		private void AbortImport()
        {
            if (Cancelled) return;
            try
            {
                DataContextObject.NotifyDeleteEntry();
                AppendLog("Import aborted.");
            }
            catch (Exception)
            {
                AppendLog("Error aborting import.");
            }
            ImportCompleted(false);
        }

		//private void DatabaseNotRecognized()
		//{
		//    AppendLog("Database not recognized.");
		//    AbortImport();
		//}

		/// <summary>
		/// Appends the log.
		/// </summary>
		/// <param name="message">The message.</param>
		void AppendLog(string message)
        {
            LogFile.Add(message);
            _sessionLogger.Write(message);
            System.Windows.Forms.Application.DoEvents();

        }

        //void OnNotifyUI(Action action)
        //{
        //    NotifyUi(this, new ExtendedEventArgs<Action>(action));
        //}

        //private int GetCondition(string s)
        //{
        //    return
        //        (s.StartsWith("Heart Attack")) ? (byte)1 :
        //        (s.StartsWith("Heart Failure")) ? 2 :
        //        (s.StartsWith("Pneumonia")) ? 3 :
        //        (s.StartsWith("Surgical")) ? 4 :
        //        (s.StartsWith("Children")) ? 5 : // has HTML symbol for Children's Asthma Care
        //                                         // 6 = HOSP_IMG_XWLK
        //        (s.StartsWith("Hospital-Wide")) ? 7 :
        //        0;
        //}

        //private float? GetFloatFromString(string inputString, float? defaultVal)
        //{
        //    if (inputString == null)
        //    {
        //        return defaultVal;
        //    }

        //    float tempFloat;
        //    return float.TryParse(inputString, out tempFloat) ? tempFloat : defaultVal;
        //}

        //private double? GetDoubleFromString(string inputString, double? defaultVal)
        //{
        //    if (inputString == null)
        //    {
        //        return defaultVal;
        //    }
        //    double tempDouble;
        //    return double.TryParse(inputString, out tempDouble) ? tempDouble : defaultVal;
        //}

        //private static int? GetIntFromString(string inputString, int? defaultVal)
        //{
        //    if (inputString == null)
        //    {
        //        return defaultVal;
        //    }
        //    int tempInt;
        //    return int.TryParse(inputString, out tempInt) ? tempInt : defaultVal;
        //}
    }
}
