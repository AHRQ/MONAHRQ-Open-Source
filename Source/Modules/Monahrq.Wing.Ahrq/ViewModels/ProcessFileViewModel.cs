using System.Globalization;
using Monahrq.Wing.Ahrq.Model;
using PropertyChanged;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using NHibernate.Linq;

namespace Monahrq.Wing.Ahrq.ViewModels
{
    /// <summary>
    /// View model class for processing the file.
    /// </summary>
    /// <seealso cref="Monahrq.Wing.Ahrq.SessionedViewModelBase{Monahrq.Wing.Ahrq.Model.WizardContext}" />
    /// <seealso cref="Monahrq.Wing.Ahrq.IDataImporter" />
    [ImplementPropertyChanged]
    public abstract class ProcessFileViewModel : SessionedViewModelBase<WizardContext>, IDataImporter
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ProcessFileViewModel"/> is done.
        /// </summary>
        /// <value>
        ///   <c>true</c> if done; otherwise, <c>false</c>.
        /// </value>
        public bool Done { get; set; }
        // private StreamReader InputFile { get; set; }
        /// <summary>
        /// Gets the header line.
        /// </summary>
        /// <value>
        /// The header line.
        /// </value>
        public abstract string HeaderLine{ get; }
        /// <summary>
        /// Gets the header line v6.
        /// </summary>
        /// <value>
        /// The header line v6.
        /// </value>
        public abstract string HeaderLineV6 { get; }
        /// <summary>
        /// Gets the name of the import type.
        /// </summary>
        /// <value>
        /// The name of the import type.
        /// </value>
        public abstract string ImportTypeName { get; }

        List<string> _logFile;
       protected string _headerLine;
      //  Action<bool,string> _lineFunction;
        private Func<string, bool> _lineFunction ;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessFileViewModel"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ProcessFileViewModel(WizardContext context)
            : base(context)
        {
            Done = false;
        }

        /// <summary>
        /// Initializes the specified header line.
        /// </summary>
        /// <param name="headerLine">The header line.</param>
        /// <param name="lineFunction">The line function.</param>
        public void Initialize(string headerLine, Func<string,bool> lineFunction)
        {
            this._headerLine = headerLine;
            this._lineFunction = lineFunction;
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

        // Import the Footnotes table from csv to SQL server
        /// <summary>
        /// Starts the import.
        /// </summary>
        async public override void StartImport()
        {
            base.StartImport();

            try
            {
                if (DataContextObject.DatasetItem.IsReImport)
                {
                    using (var session = DataContextObject.Provider.SessionFactory.OpenStatelessSession())
                    {
                        using (var trans = session.BeginTransaction())
                        {
                            var query = string.Format("Delete from [{0}] where Dataset_Id={1}", DataContextObject.TargetType.EntityTableName(),
                                DataContextObject.DatasetItem.Id);

                            session.CreateSQLQuery(query).ExecuteUpdate();
                            
                            trans.Commit();
                        }
                    }
                }
                
                await ImportFileAsync(Cts.Token);

                // WPF CommandManager periodically calls IsValid to see if the Next/Done button should be enabled. 
                // In multi-threaded wizard steps, IsValid returns the value of the Done flag. Call InvalidateRequerySuggested here
                // on the UI thread after setting the Done flag to force WPF to call IsValid now so the Next/Done button will become enabled. 
                //Application.Current.DoEvents();
                CommandManager.InvalidateRequerySuggested();
            }
            catch (OperationCanceledException)
            {
               ImportCompleted(false);
               MessageBox.Show(AppendLog("Import cancelled."));
            }
            finally
            {
                Cts.Dispose();
            }
        }

        /// <summary>
        /// Imports the file asynchronous.
        /// </summary>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        async Task<int> ImportFileAsync(CancellationToken ct)
        {

              return await Task.Run(() =>
                {
                    try
                    {
                        _logFile = new List<string>();

                        if (null == DataContextObject || 
                            null == DataContextObject.Files ||
                            !DataContextObject.Files.Any())
                        {
                            AppendLog(@"No file to import !");
                            return 0 ;
                        }

                        if (CountLines() < 1)
                        {
                            ImportCompleted(false);
                            MessageBox.Show(AppendLog("Input file(s) appears to be empty."), "Error importing file", MessageBoxButton.OK, MessageBoxImage.Error);
                            return 0;
                        }

                        var processFileResult = false;
                        DataContextObject.Files.ForEach(f =>
                        {
                            AppendLog("Starting import: " +f.FileName );
                            //ImportCompleted(ProcessFile(f));
                            processFileResult = ProcessFile(f);
                        });
                        ImportCompleted(processFileResult);
                       
                    }
                    catch (Exception e)
                    {
                       ImportCompleted(false);
                       MessageBox.Show(AppendLog("Error importing file: " + e.Message), "Error importing file");
                       return 0;
                    }

                    return 1;
                }, ct);
        }

        // read the file once (without processing) to count lines for progress bar
        /// <summary>
        /// Reads the file and counts the number of lines.
        /// </summary>
        /// <returns></returns>
        private int CountLines()
        {
            if (null == DataContextObject || null == DataContextObject.Files) return 0;

            var totalLines = 0;
            DataContextObject.Files.ForEach(f =>
            {
                totalLines += f.LinesCount();
                AppendLog(string.Format("Finished scanning import file [{0}]. Number of lines: {1}", Path.GetFileName(f.FileName), f.LinesCount()));
              
            });
            AppendLog(string.Format("Finished scanning import file(s). Total lines: {0}", totalLines));
            return totalLines;
        }

        /// <summary>
        /// Validates the file header.
        /// </summary>
        /// <param name="fileHeader">The file header.</param>
        /// <param name="fileProgress">The file progress.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        protected virtual bool ValidateFileHeader(string fileHeader, FileProgress fileProgress, out string errorMessage)
        {
            if (!string.IsNullOrEmpty(HeaderLineV6) && HeaderLineV6.ToUpper().EqualsIgnoreCase(fileHeader.Trim().ToUpper()))
            {
                errorMessage = string.Format("The input file [{0}] does not appear to be of the correct file type.", fileProgress.FileName) + "\n\n" +
                                             "If you are trying to import the reports genrated by AHRQ QI v6.0 ICD-10 version, please note that MONAHRQ will not be able to process the file and can't import it at this time.";
                return false;
            }
            var acceptedHeaders=_headerLine.ToUpper().Split(new string[] { "|" }, StringSplitOptions.None).ToList();

            var result = acceptedHeaders.Any(h => fileHeader.ToUpper().StartsWith(h));

            errorMessage = result
                ? string.Empty
                : string.Format("The input file [{0}] does not appear to be of the correct file type.", fileProgress.FileName);

            return result;
        }

        /// <summary>
        /// Process file and update the file progress.
        /// </summary>
        /// <param name="fileProgress">The file progress.</param>
        /// <returns></returns>
        private bool ProcessFile(FileProgress fileProgress)
        {
            try
            {
                using (var fs = new FileStream(fileProgress.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var inputFile = new StreamReader(fs))
                {
                    // search for the header line
                    string inputLine;
                    var foundHeader = false;
                    var headerValidationErrorMessage = string.Empty;
                    var metadataFound = false;
                    while ((inputLine = inputFile.ReadLine()) != null)
                    {
                        if (inputLine.StartsWith("Report"))
                            metadataFound = true;

                        if (DataContextObject.DatasetItem.ContentType.Name.ToUpper() == ImportTypeName.ToUpper())
                        {
                            if (!inputLine.ToUpper().StartsWith(HeaderLine.SubStrBefore(",").ToUpper()) && metadataFound)
                                continue; // Found metadata

                            metadataFound = false;
                        }

                        ////fileProgress.LinesDone++;
                        //fileProgress.PercentComplete = fileProgress.LinesDone * 100 / fileProgress.TotalLines;
                        //AppendLogLinesDone(fileProgress.LinesDone);

                        if (ValidateFileHeader(inputLine.ToUpper().Trim(), fileProgress, out headerValidationErrorMessage))
                        {
                            // Found the file token in the header. Ready to start import.
                            foundHeader = true;
                            break;
                        }

                        if (fileProgress.LinesDone > 50)
                        {
                            break;
                        }
                    }

                    if (!foundHeader && !string.IsNullOrEmpty(headerValidationErrorMessage))
                    {
                        // Didn't find file token in input file before EOF
                        MessageBox.Show(AppendLog(headerValidationErrorMessage), "Error importing file");
                        return false;
                    }

                    AppendLog(string.Format("Header row found. Continuing from line {0}.", fileProgress.LinesDone));

                    // TODO: move this to a service
                    // save owner before inserting all rows
                    DataContextObject.CurrentImportType = DataContextObject.GetTargetByName(DataContextObject.SelectedDataType.DataTypeName);

                    // import each line
                    while ((inputLine = inputFile.ReadLine()) != null)
                    {
                        try
                        {
                            if ( _lineFunction( inputLine ) )
                                fileProgress.LinesProcessed ++;
                            else 
                                fileProgress.LinesDuplicated++;
                        }
                        catch(Exception e)
                        {
                            fileProgress.LinesErrors++;
                            AppendLog("Error : " + e.Message);
                        }
                        finally
                        {
                            fileProgress.LinesDone++;
                           // fileProgress.PercentComplete = (fileProgress.LinesDone * 100) / fileProgress.TotalLines;
                            AppendLogLinesDone(fileProgress.LinesDone);
                        }
                    }
                }
                AppendLog(fileProgress.LinesDone + " lines processed.");

                //DataContextObject.DatasetItem.IsFinished = true;
                //DataContextObject.SaveImportEntry(DataContextObject.DatasetItem);
            }
            catch (Exception e)
            {
                MessageBox.Show(AppendLog("Error : " + e.Message), string.Format("Error importing file [{0}]",fileProgress.FileName));
                return false;
            }
            
            return true;
        }

        #region LOGGING 

        /// <summary>
        /// Imports the completed.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        protected override void ImportCompleted(bool success)
        {
            //try
            //{
            //    //if (InputFile != null)
            //    //{
            //    //    InputFile.Close();
            //    //    InputFile.Dispose();
            //    //}
            //}
            //catch (Exception e)
            //{
            //    AppendLog("Error closing the import file: " + e.Message);
            //}
            
            try
            {
                if (success)
                {
                    var rootXml = new XElement("LogLines");
                    foreach (string line in _logFile)
                    {
                        rootXml.Add(new XElement("LogLine", line));
                    }
                    DataContextObject.Summary = rootXml.ToString();

                    if (DataContextObject.DatasetItem.IsReImport)
                    {
                        var linesImported = Session.CreateSQLQuery(string.Format("select count(o.[Id]) from {0} o where o.Dataset_Id={1}", DataContextObject.TargetType.EntityTableName(), DataContextObject.DatasetItem.Id))
                            .UniqueResult<int>();

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

                Done = success;
            }
            finally
            {
                base.ImportCompleted(success);
            }
        }

        /// <summary>
        /// Log the count of the lines which are processed.
        /// </summary>
        /// <param name="linesDone">The lines done.</param>
        void AppendLogLinesDone(int linesDone)
        {
            // Update the count of lines processed in the logfile.
            if (_logFile.Last().StartsWith("Lines Loaded:"))
            {
                _logFile.RemoveAt(_logFile.Count() - 1);
            }
            _logFile.Add("Lines Loaded: " + linesDone);
        }

        /// <summary>
        /// Add the message to the log file.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected string AppendLog(string message)
        {
            _logFile.Add(message);
            return message;
        }

        #endregion

        #region DATA_CONVERTERS

        /// <summary>
        /// Gets the float from string.
        /// </summary>
        /// <param name="strFloat">The string float.</param>
        /// <returns></returns>
        protected float? GetFloatFromString(string strFloat)
        {
            float tempFloat;

            return float.TryParse(strFloat, out tempFloat) ? (float?) tempFloat : null;
        }

        /// <summary>
        /// Gets the int from string.
        /// </summary>
        /// <param name="strFloat">The string float.</param>
        /// <returns></returns>
        protected int? GetIntFromString(string strFloat)
        {
            int tempInt;

            return int.TryParse(strFloat, out tempInt) ? (int?) tempInt : null;
        }

        /// <summary>
        /// Gets the decimal from string.
        /// </summary>
        /// <param name="strDecimal">The string decimal.</param>
        /// <returns></returns>
        protected decimal? GetDecimalFromString(string strDecimal)
        {
            decimal tempDecimal;

            const NumberStyles styles = NumberStyles.Float | NumberStyles.AllowDecimalPoint;

            if (decimal.TryParse(strDecimal, styles, CultureInfo.CurrentCulture, out tempDecimal))
                return (decimal?) tempDecimal;
            else 
                return null;
        }

        #endregion

        #region PROPERTIES

        //new public event PropertyChangedEventHandler PropertyChanged;

        //private void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        //{
        //    if (!EqualityComparer<T>.Default.Equals(field, value))
        //    {
        //        field = value;
        //        var handler = PropertyChanged;
        //        if (handler != null)
        //        {
        //            handler(this, new PropertyChangedEventArgs(name));
        //        }
        //    }
        //}

        //public int _TotalLines;
        //public int TotalLines
        //{
        //    get { return _TotalLines; }
        //    set
        //    {
        //        SetProperty(ref _TotalLines, value);
        //    }
        //}

        //public int _LinesDone;
        //public int LinesDone
        //{
        //    get { return _LinesDone; }
        //    set
        //    {
        //        SetProperty(ref _LinesDone, value);
        //    }
        //}

        //public int _PercentComplete;
        //public int PercentComplete
        //{
        //    get { return _PercentComplete; }
        //    set
        //    {
        //        SetProperty(ref _PercentComplete, value);
        //    }
        //}

        #endregion
    }
}
