using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.FileSystem;
using Monahrq.Infrastructure.Services.Hospitals;
using Monahrq.Sdk.Events;

using Monahrq.Sdk.Services.Import.Exceptions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Types;
using System.Windows.Input;
namespace Monahrq.Sdk.Services.Import
{
    /// <summary>
    /// The base class for all reporting entity (hospitals, custom regions, physicians and medical practices, etc.) file importers used in Monahrq
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Services.Import.IEntityFileImporter" />
    /// <seealso cref="Monahrq.Sdk.Events.ISimpleImportCompletedPayload" />
    public abstract class EntityFileImporter : IEntityFileImporter, ISimpleImportCompletedPayload
    {
        /// <summary>
        /// Gets the export attribute.
        /// </summary>
        /// <value>
        /// The export attribute.
        /// </value>
        protected ExportAttribute ExportAttribute
        {
            get
            {
                return GetType().GetCustomAttribute<ExportAttribute>();
            }
        }

        /// <summary>
        /// Gets the provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public IDomainSessionFactoryProvider Provider { get; private set; }
        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        protected ILoggerFacade Logger { get; private set; }
        /// <summary>
        /// Gets the count inserted.
        /// </summary>
        /// <value>
        /// The count inserted.
        /// </value>
        public int CountInserted { get; protected set; }
        /// <summary>
        /// Gets the lines read.
        /// </summary>
        /// <value>
        /// The lines read.
        /// </value>
        public int LinesRead { get; private set; }
        /// <summary>
        /// Gets the number of errors.
        /// </summary>
        /// <value>
        /// The number of errors.
        /// </value>
        public int NumberOfErrors { get; protected set; }
        /// <summary>
        /// Gets the error file.
        /// </summary>
        /// <value>
        /// The error file.
        /// </value>
        public string ErrorFile { get; private set; }
        /// <summary>
        /// Gets the inserted.
        /// </summary>
        /// <value>
        /// The inserted.
        /// </value>
        public List<object> Inserted { get; private set; }

        /// <summary>
        /// Gets or sets the import errors.
        /// </summary>
        /// <value>
        /// The import errors.
        /// </value>
        protected List<ImportError> ImportErrors { get; set; }

        /// <summary>
        /// Creates the please stand by event payload.
        /// </summary>
        /// <returns></returns>
        public PleaseStandByEventPayload CreatePleaseStandByEventPayload()
        {
            return new PleaseStandByEventPayload
                {
                    Message = string.Format("Processing {0} import.", ExportAttribute.ContractName)
                };
        }

        /// <summary>
        /// Validates the name of the import file.
        /// </summary>
        /// <param name="uploadedFilePath">The uploaded file path.</param>
        /// <returns></returns>
        protected bool ValidateImportFileName(string uploadedFilePath)
        {
            bool result = !string.IsNullOrEmpty(Path.GetFileName(uploadedFilePath));

            var matches = Regex.Matches(Path.GetFileName(uploadedFilePath), @"^[A-Za-z\d_]+$", RegexOptions.IgnoreCase);
            result = (result && matches.Count == 0) && !Path.GetFileName(uploadedFilePath).ContainsIgnoreCase("-");
            return result;
        }

        /// <summary>
        /// Occurs when [importing].
        /// </summary>
        public virtual event EventHandler Importing = delegate { };
        /// <summary>
        /// Occurs when [imported].
        /// </summary>
        public virtual event EventHandler Imported = delegate { };

        /// <summary>
        /// Called when [imported].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnImported(object sender, EventArgs e)
        {
            EventHandler handler = Imported;
            if (handler != null)
            {
                handler(this, e);
            }

        }

        /// <summary>
        /// Called when [importing].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnImporting(object sender, EventArgs e)
        {
            EventHandler handler = Importing;
            if (handler != null)
            {
                handler(this, e);
            }

        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        /// <value>
        /// The events.
        /// </value>
        public IEventAggregator Events { get; set; }

        /// <summary>
        /// Gets or sets the folder.
        /// </summary>
        /// <value>
        /// The folder.
        /// </value>
        IUserFolder Folder { get; set; }
        /// <summary>
        /// Gets the hospital registry service.
        /// </summary>
        /// <value>
        /// The hospital registry service.
        /// </value>
        protected IHospitalRegistryService HospitalRegistryService { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFileImporter"/> class.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="hospitalRegistryService">The hospital registry service.</param>
        /// <param name="events">The events.</param>
        /// <param name="logger">The logger.</param>
        [ImportingConstructor]
        protected EntityFileImporter(IUserFolder folder
            , IDomainSessionFactoryProvider provider
            , IHospitalRegistryService hospitalRegistryService
            , IEventAggregator events
            , ILoggerFacade logger)
        {
            Provider = provider;
            Logger = logger;
            Folder = folder;
            Events = events;
            HospitalRegistryService = hospitalRegistryService;
            Description = string.Format("{0} Import", ExportAttribute.ContractName);
            Inserted = new List<object>();
            ImportErrors = new List<ImportError>();
        }

        /// <summary>
        /// Asserts the error file.
        /// </summary>
        protected void AssertErrorFile()
        {
            var temp = MakeErrorFilename();

            while (File.Exists(temp))
            {
                try
                {
                    File.Delete(temp);
                }
                catch { }
                finally
                {
                    temp = MakeErrorFilename();
                }
            }
            ErrorFile = temp;
        }

        /// <summary>
        /// Makes the error filename.
        /// </summary>
        /// <returns></returns>
        string MakeErrorFilename()
        {
            // append .log to the error filename so Notepad can be easily used
            return Folder.ScratchPadFile + ".log";
        }

        /// <summary>
        /// Gets the current states being managed.
        /// </summary>
        /// <value>
        /// The current states being managed.
        /// </value>
        protected List<string> CurrentStatesBeingManaged { get; private set; }

        /// <summary>
        /// Import3s the specified lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        public void Import3(string[] lines)
        {
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            CurrentStatesBeingManaged = new List<string>(configService.HospitalRegion.DefaultStates.OfType<string>().ToList());
            var startTime = DateTime.Now;
            Importing(this, EventArgs.Empty);
            CountInserted = 0;
            NumberOfErrors = 0;
            AssertErrorFile();

            //csvRdr.ReadLine();
            //while (!csvRdr.EndOfData)
            //{
            for (int index = 0; index < lines.Length; index++)
            {
                if (index == 0) continue;

                var line = lines[index];
                int n = 1;

                Events.GetEvent<PleaseStandByMessageUpdateEvent>().Publish("Importing line " + n);
                try
                {
                    var lineArray = line.Split(',');
                    ExtractAndSave3(lineArray, ImportErrors);
                }
                finally
                {
                    n++;
                }
            }

            Events.GetEvent<PleaseStandByMessageUpdateEvent>().Publish("Finalizing import...");

            // what is the purpose of this delay???
            const int maxDelaySeconds = 3;

            var elapsed = DateTime.Now - startTime;
            var seconds = elapsed.TotalSeconds;
            var remaining = maxDelaySeconds - seconds;
            if (remaining > 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(remaining));
            }

            Imported(this, EventArgs.Empty);
            Events.GetEvent<PleaseStandByMessageUpdateEvent>().Publish("Import Complete");
            Events.GetEvent<SimpleImportCompletedEvent>().Publish(this);
        }

        /// <summary>
        /// Extracts the and save3.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="errors">The errors.</param>
        private void ExtractAndSave3(string[] line, IList<ImportError> errors)
        {
            //var line = csvRdr.ReadFields();
            try
            {
                SaveLine2(line, errors);

                if (errors.Count == 0)
                    CountInserted++;
            }
            catch (Exception exc)
            {
                var name = GetName(line);
                var errorMessage =
                        string.Format(
                            "An error occurred while trying to save hospital categories for \"{1}\":{0}{2}."
                            , System.Environment.NewLine, name, (exc.InnerException ?? exc).Message);

                errors.Add(ImportError.Create("Hospital", name, errorMessage));
                //try
                //{
                //    using (var sw = new StreamWriter(ErrorFile, true))
                //    {
                //        WriteError(sw, ex);
                //        sw.WriteLine("Data: {0}", line);
                //        sw.WriteLine();
                //    }
                //}
                //finally
                //{
                //    this.NumberOfErrors++;
                //}
            }
            finally
            {
                if (errors.Count > 0)
                {
                    //Task.Factory.StartNew(() =>
                    //    {
                    using (var sw = new StreamWriter(ErrorFile, true))
                    {
                        foreach (var error in errors)
                        {
                            WriteError2(sw, error.ErrorMessage);
                            sw.WriteLine("Data: {0}", line);
                            sw.WriteLine();

                            this.NumberOfErrors++;
                        }
                    }
                    // });
                }
                LinesRead++;
            }
        }

        /// <summary>
        /// Import2s the specified CSV RDR.
        /// </summary>
        /// <param name="csvRdr">The CSV RDR.</param>
        public void Import2(Microsoft.VisualBasic.FileIO.TextFieldParser csvRdr)
        {
            //CurrentStatesBeingManaged = new List<State>(HospitalRegion.Default.SelectedStates);
            var startTime = DateTime.Now;
            Importing(this, EventArgs.Empty);
            CountInserted = 0;
            NumberOfErrors = 0;
            AssertErrorFile();

            csvRdr.ReadLine();
            while (!csvRdr.EndOfData)
            {
                int n = 1;

                IList<ImportError> errors = new List<ImportError>();

                Events.GetEvent<PleaseStandByMessageUpdateEvent>()
                      .Publish("Importing line " + n.ToString());
                try
                {
                    ExtractAndSave2(csvRdr, errors);

                    ImportErrors.AddRange(errors);
                }
                finally
                {
                    n++;
                }
            }

            FinalProcessing();

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

            Imported(this, EventArgs.Empty);
            Events.GetEvent<PleaseStandByMessageUpdateEvent>().Publish("Import Complete");
            Events.GetEvent<SimpleImportCompletedEvent>().Publish(this);
        }

        /// <summary>
        /// Extracts the and save2.
        /// </summary>
        /// <param name="csvRdr">The CSV RDR.</param>
        /// <param name="errors">The errors.</param>
        private void ExtractAndSave2(Microsoft.VisualBasic.FileIO.TextFieldParser csvRdr, IList<ImportError> errors)
        {
            string[] line = null;
            try
            {
                line = csvRdr.ReadFields();

                SaveLine2(line, errors);

                if (!errors.Any())
                    CountInserted++;
            }
            catch (Exception exc)
            {
                var name = GetName(line);
                var errorMessage =
                        string.Format(
                            "An error occurred while trying to save hospital categories for \"{1}\":{0}{2}."
                            , System.Environment.NewLine, name, (exc.InnerException ?? exc).Message);

                errors.Add(ImportError.Create("Hospital", name, errorMessage));
            }
            finally
            {
                if (errors.Any())
                {
                    //Task.Factory.StartNew(() =>
                    //    {
                    using (var sw = new StreamWriter(ErrorFile, true))
                    {
                        foreach (var error in errors)
                        {
                            WriteError2(sw, error.ErrorMessage);
                            sw.WriteLineAsync(line != null
                                ? string.Format("Data: {0}", string.Join(",", line.ToArray()))
                                : error.ErrorMessage);

                            sw.WriteLine();

                            NumberOfErrors++;
                        }
                    }
                }
                LinesRead++;
            }
        }

        /// <summary>
        /// Writes the error.
        /// </summary>
        /// <param name="sw">The sw.</param>
        /// <param name="ex">The ex.</param>
        private void WriteError(StreamWriter sw, Exception ex)
        {
            sw.WriteLine("Line {0}:  {1}", LinesRead + 1, ex.Message);
            if (ex.InnerException != null)
                WriteInnerError(sw, ex.InnerException);
        }

        /// <summary>
        /// Writes the error2.
        /// </summary>
        /// <param name="sw">The sw.</param>
        /// <param name="errorMessage">The error message.</param>
        protected void WriteError2(StreamWriter sw, string errorMessage)
        {
            sw.WriteLine("Line {0}:  {1}", LinesRead + 1, errorMessage);
            //if (ex.InnerException != null)
            //    WriteInnerError(sw, ex.InnerException);
        }

        /// <summary>
        /// Writes the inner error.
        /// </summary>
        /// <param name="sw">The sw.</param>
        /// <param name="ex">The ex.</param>
        private void WriteInnerError(StreamWriter sw, Exception ex)
        {
            sw.WriteLine("Inner Exception: {0}", ex.Message);
            if (ex.InnerException != null)
            {
                WriteInnerError(sw, ex.InnerException);
            }
        }

        /// <summary>
        /// Processes from array.
        /// </summary>
        /// <param name="vals">The vals.</param>
        /// <param name="errors">The errors.</param>
        void ProcessFromArray(string[] vals, IList<ImportError> errors)
        {
            ValidateValueCount(vals, errors);

            if (errors.Count > 0) return;

            ProcessValues(vals, errors);
        }

        /// <summary>
        /// Processes the values.
        /// </summary>
        /// <param name="vals">The vals.</param>
        /// <param name="errors">The errors.</param>
        protected abstract void ProcessValues(string[] vals, IList<ImportError> errors);

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="vals">The vals.</param>
        /// <returns></returns>
        protected abstract string GetName(string[] vals);

        /// <summary>
        /// Begins the import.
        /// </summary>
        protected virtual void BeginImport()
        {
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            CurrentStatesBeingManaged = new List<string>(configService.HospitalRegion.DefaultStates.OfType<string>().ToList());
        }

        /// <summary>
        /// Gets the expected count of values per line.
        /// </summary>
        /// <value>
        /// The expected count of values per line.
        /// </value>
        protected abstract int ExpectedCountOfValuesPerLine
        {
            get;
        }

        /// <summary>
        /// Gets the expected high count of values per line.
        /// </summary>
        /// <value>
        /// The expected high count of values per line.
        /// </value>
        protected virtual int ExpectedHighCountOfValuesPerLine
        {
            get { return ExpectedCountOfValuesPerLine; }
        }

        /// <summary>
        /// Validates the value count.
        /// </summary>
        /// <param name="vals">The vals.</param>
        /// <param name="errors">The errors.</param>
        public virtual void ValidateValueCount(string[] vals, IList<ImportError> errors)
        {
            if (vals.Length != ExpectedCountOfValuesPerLine)
            {
                var errorMessage = string.Format("{0} import requires {1} values per line. {2} values read.",
                              ExportAttribute.ContractName, ExpectedCountOfValuesPerLine,
                              vals.Length);
                errors.Add(ImportError.Create("", GetName(vals), errorMessage));

                //throw new Exception(string.Format("{0} import requires {1} values per line. {2} values read.",
                //        ExportAttribute.ContractName, ExpectedCountOfValuesPerLine,
                //        vals.Length));
            }
        }

        /// <summary>
        /// Tests for empty.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <exception cref="ArgumentException">Line is missing one or more values</exception>
        protected void TestForEmpty(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) throw new ArgumentException("Line is missing one or more values");
        }

        /// <summary>
        /// Saves the line2.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="errors">The errors.</param>
        private void SaveLine2(string[] line, IList<ImportError> errors)
        {
            if (errors.Any()) return;

            ProcessFromArray(line, errors);
        }

        /// <summary>
        /// Saves the line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="errors">The errors.</param>
        private void SaveLine(string line, IList<ImportError> errors)
        {


            ProcessFromArray(line.Split(','), errors);
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <param name="abbrev">The abbrev.</param>
        /// <returns></returns>
        /// <exception cref="StateLookupFailed">
        /// </exception>
        protected State GetState(string abbrev)
        {
            try
            {
                return HospitalRegistryService.GetStates(new[] { abbrev }).Single();
            }
            catch (InvalidOperationException ex)
            {
                throw new StateLookupFailed(string.Format("State not found: {0}", abbrev.ToUpper()), ex);
            }
            catch (Exception ex)
            {
                throw new StateLookupFailed(string.Format("State lookup failed: {0}", ex.Message), ex);
            }
        }
        
        /// <summary>
        /// Gets the existing region.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryExpression">The query expression.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Sdk.Services.Import.Exceptions.ExistingEntityLookupException">null</exception>
        protected T GetExistingRegion<T>(Expression<Func<T, bool>> queryExpression) where T : Region
        {
            try
            {
                return HospitalRegistryService.Get(queryExpression);
            }
            catch (Exception ex)
            {
                throw new ExistingEntityLookupException(ExportAttribute, null, ex);
            }
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public virtual void Execute()
        {
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
                var message = string.Format("The file \"{0}\" could not be import due to the file name containing special characters (*,&,!,@,#, etc.) and/or dashes ( - ). Please replace any special characters and/or dashes to spaces and/or underscores ( _ ) and try again.", Path.GetFileName(dlg.FileName));
                MessageBox.Show(message, "MONAHRQ Import File Open Error", MessageBoxButton.OK);
                return;
            }

            try
            {
                using (ApplicationCursor.SetCursor(Cursors.Wait))
                {
                    BeginImport();
                    //var lines = File.ReadAllLines(dlg.FileName);
                    //Import3(lines);

                    using (var csvRdr = new Microsoft.VisualBasic.FileIO.TextFieldParser(dlg.FileName))
                    {
                        csvRdr.SetDelimiters(new[] { "," });
                        csvRdr.TrimWhiteSpace = true;
                        csvRdr.HasFieldsEnclosedInQuotes = true;

                        Import2(csvRdr);

                    }

                    //using (var fs = File.OpenRead(dlg.FileName))
                    //{
                    //    Import(fs);
                    //}
                }
            }
            catch (IOException exc)
            {
                var error = exc.InnerException ?? exc;
                Logger.Log(error.Message, Category.Exception, Priority.High);

                var message = string.Format("Please close file\"{0}\" before trying to import.",
                                            dlg.FileName.SubStrAfterLast(@"\"));
                MessageBox.Show(message, "MONAHRQ Import File Open Error", MessageBoxButton.OK);
            }
            finally
            {
                EndImport();
            }
        }

        /// <summary>
        /// Finals the processing.
        /// </summary>
        protected virtual void FinalProcessing() { }

        /// <summary>
        /// Ends the import.
        /// </summary>
        protected virtual void EndImport() { }

        /// <summary>
        /// Gets the continue prompt.
        /// </summary>
        /// <value>
        /// The continue prompt.
        /// </value>
        protected abstract string ContinuePrompt { get; }

        /// <summary>
        /// Prompts the user for continue.
        /// </summary>
        /// <returns></returns>
        protected MessageBoxResult PromptUserForContinue()
        {
            return MessageBox.Show(ContinuePrompt, "Confirm Import", MessageBoxButton.YesNo);
        }

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <value>
        /// The file extension.
        /// </value>
        protected abstract string FileExtension { get; }
    }

    public class ImportError
    {
        public string ImportType { get; set; }
        public string EntityName { get; set; }
        public string ErrorMessage { get; set; }

        public static ImportError Create(string importType, string entityName, string errorMessage)
        {
            return new ImportError { ImportType = importType, EntityName = entityName, ErrorMessage = errorMessage };
        }
    }
}
