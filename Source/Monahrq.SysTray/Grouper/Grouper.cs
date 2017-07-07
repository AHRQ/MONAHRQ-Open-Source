using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.SysTray.Grouper
{
    /// <summary>
    /// Class for Grouper element
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class Grouper : IDisposable
    {
        private readonly string _inputFileName;
        private StreamWriter _inputFile;
        private bool _inputFileIsOpen;
        private readonly string _uploadFileName;
        private StreamReader _uploadFile;
        private bool _uploadFileIsOpen;
        private readonly string _logFileName;
        private static readonly string _appPath;
        private static readonly string _grouperFilePath = Path.Combine(Path.GetTempPath(),"Monahrq\\Grouper\\");

        /// <summary>
        /// Static constructor.
        /// </summary>
        static Grouper()
        {
            _appPath = AppDomain.CurrentDomain.BaseDirectory;
            //grouperFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ahrq", "Monahrq"); //
            if (_appPath.Contains(@"\MONAHRQ\Source\Trunk\Test"))
            {
                // HACK: Get around the problem with the tests
                _appPath = _appPath.Substring(0, _appPath.IndexOf(@"\MONAHRQ\Source\Trunk\Test", StringComparison.Ordinal));
                _appPath = Path.Combine(_appPath, @"MONAHRQ\Source\Trunk\MONAHRQ.Infrastructure\bin\Debug\");
            }
            _appPath = Path.Combine(_appPath, "grouper", "Msgmce");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grouper"/> class.
        /// </summary>
        public Grouper()
        {
 
            //_logFileName = !string.IsNullOrEmpty(logfilePath)
            //    ? logfilePath
            //    : Path.Combine(_appPath, "Monahrq.SysTry.log");
           // _grouperFilePath = Path.Combine(Path.GetTempPath(),"Monahrq\\Grouper\\");

           var uniqueFileName = Guid.NewGuid().ToString();
            _inputFileName = string.Format("monahrq_{0}.inp", uniqueFileName);
            _uploadFileName = string.Format("monahrq_{0}.upl", uniqueFileName); // .upl
            _logFileName = string.Format("msgmce_{0}.log", uniqueFileName);
            _inputFileIsOpen = OpenInputFile();
            _uploadFileIsOpen = false;
        }

        /// <summary>
        /// Cleans up input output files.
        /// </summary>
        public async static void CleanUpInputOutputFiles()
        {
            await Task.Run(() =>
            {
                var tempDirectory = new DirectoryInfo(_grouperFilePath);

                if (tempDirectory.Exists)
                {
                    // Delete Input files
                    tempDirectory.GetFiles("*.inp").ToList().ForEach(file =>
                    {
                        if (file.Exists)
                            file.Delete();
                    }); 

                    // Delete Upload files
                    tempDirectory.GetFiles("*.upl").ToList().ForEach(file =>
                    {
                        if (file.Exists)
                            file.Delete();
                    });

                    tempDirectory.Delete(true);
                }
            });
        }

        /// <summary>
        /// Copies the and delete grouper log files.
        /// </summary>
        /// <param name="logfilePath">The logfile path.</param>
        public async static void CopyAndDeleteGrouperLogFiles(string logfilePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    var grouperLogFilePath = Path.Combine(_appPath, "msgmce");
                    var grouperLogFileDirectory = new DirectoryInfo(grouperLogFilePath);

                    if (grouperLogFileDirectory.Exists)
                    {
                        foreach (var file in grouperLogFileDirectory.GetFiles("*.log").ToList())
                        {
                            var newFilePath = Path.Combine(logfilePath, file.Name);
                            file.CopyTo(newFilePath, true);
                        }

                        grouperLogFileDirectory.Delete(true);
                    }
                }
                catch (Exception exc)
                {
                    
                }
            });
        }

        /// <summary>
        /// Opens the input file.
        /// </summary>
        /// <returns></returns>
        private bool OpenInputFile()
        {
            try
            {
                if (!_inputFileIsOpen)
                {
                    if (!Directory.Exists(_grouperFilePath))
                        Directory.CreateDirectory(_grouperFilePath);

                    var filePath = Path.Combine(_grouperFilePath, _inputFileName);
                    //if (File.Exists(filePath))
                    //    File.Delete(filePath);

                    //File.Create(filePath);
                    //inputFile = new StreamWriter(Path.Combine(".\\Grouper\\", inputFileName));
                    //inputFile = new StreamWriter(Path.Combine(TempPath, inputFileName));
                    _inputFile = new StreamWriter(filePath);
                    _inputFileIsOpen = true;
                }
                return true;
            }
            catch 
            {
                _inputFileIsOpen = false;
                throw;
            }
        }

        /// <summary>
        /// Closes the input file.
        /// </summary>
        private void CloseInputFile()
        {
            try
            {
                if (_inputFileIsOpen)
                {
                    _inputFile.Close();
                    _inputFile.Dispose();
                    _inputFileIsOpen = false;
                }
            }
            catch 
            {
                _inputFileIsOpen = false;
                throw;
            }
        }

        /// <summary>
        /// Opens the upload file.
        /// </summary>
        private void OpenUploadFile()
        {
            try
            {
                //uploadFile = new StreamReader(Path.Combine(".\\Grouper\\", uploadFileName));
                //uploadFile = new StreamReader(Path.Combine(TempPath, uploadFileName));
                var uploadFileName = Path.Combine(_grouperFilePath, _uploadFileName);
                //if (!File.Exists(uploadFileName))
                //    File.Create(uploadFileName);

                _uploadFile = new StreamReader(uploadFileName);
                _uploadFileIsOpen = true;
            }
            catch 
            {
                _uploadFileIsOpen = false;
                throw;
            }
        }

        /// <summary>
        /// Closes the upload file.
        /// </summary>
        /// <returns></returns>
        public bool CloseUploadFile()
        {
            try
            {
                if (_uploadFileIsOpen)
                {
                    _uploadFile.Close();
                    _uploadFile.Dispose();
                    _uploadFileIsOpen = false;

                    // When closing the upload file (i.e. done reading it into the application), delete it and the input file for privacy's sake.
                    //File.Delete(Path.Combine(".\\Grouper\\", uploadFileName));
                    //File.Delete(Path.Combine(TempPath, uploadFileName));

                    // TODO: Why are we spinning off a new thread? Answer=> Delete by fire and fortget, no need to wait till this is done
                    //var task = Task.Factory.StartNew(() =>
                    //    {
                    //        File.Delete(Path.Combine(_grouperFilePath, _uploadFileName));
                    //    });
                }
                return true;
            }
            catch
            {
                _uploadFileIsOpen = false;
                throw;
            }
            finally
            {
                var uploadFileFullPath = Path.Combine(_grouperFilePath, _uploadFileName);
                var inputFileFullPath = uploadFileFullPath.Replace(".upl", ".inp");

                if(File.Exists(inputFileFullPath))
                    File.Delete(inputFileFullPath);

                if (File.Exists(uploadFileFullPath))
                    File.Delete(uploadFileFullPath);
            }
        }

        /// <summary>
        /// The add record to be grouped lock object
        /// </summary>
        private readonly object _addRecordToBeGroupedLockObject = new object();
        /// <summary>
        /// Adds the record to be grouped.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.IOException">Could not open input file.</exception>
        public bool AddRecordToBeGrouped(GrouperInputRecord record)
        {
            lock (_addRecordToBeGroupedLockObject)
            {
                if (!_inputFileIsOpen)
                {
                    if (!OpenInputFile())
                    {
                        // TODO: Use other error logging.
                        throw new IOException("Could not open input file.");
                    }
                }
                if (_inputFileIsOpen)
                {
                    _inputFile.WriteLine(record.ToString());
                    _inputFile.Flush();
                }
                return true;
            }
        }

        /// <summary>
        /// Gets the grouper output records.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GrouperOutputRecord> GetGrouperOutputRecords()
        {
            if (_uploadFileIsOpen)
            {
                while (_uploadFile.Peek() >= 0)
                {
                    var record = new GrouperOutputRecord();

                    string uploadLine = _uploadFile.ReadLine();
                    if (uploadLine == null)
                    {
                        // We've reached the end of the file so try to close and dispose of it.
                        CloseUploadFile();
                        yield break;
                    }

                    // We have a good record, so try to deserialize it.
                    // TODO: error check the deserialize process
                    record.OutputRecord = uploadLine;

                    if (record.OutputRecordValid)
                        yield return record;
                }

            }
        }

        /// <summary>
        /// Gets the grouped record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <returns></returns>
        public bool GetGroupedRecord(GrouperOutputRecord record)
        {
            if (_uploadFileIsOpen)
            {
                string uploadLine = _uploadFile.ReadLine();
                if (uploadLine == null)
                {
                    // We've reached the end of the file so try to close and dispose of it.
                    CloseUploadFile();
                }
                else
                {
                    // We have a good record, so try to deserialize it.
                    // TODO: error check the deserialize process
                    record.OutputRecord = uploadLine;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Runs the grouper.
        /// </summary>
        /// <returns></returns>
        public bool RunGrouper()
        {
            //DateTime startTime = DateTime.Now;
            // Make sure the files are close before running.
            if (_inputFileIsOpen)
            {
                CloseInputFile();
            }
            if (_uploadFileIsOpen)
            {
                CloseUploadFile();
            }

            // Call the grouper.
            using (var grouperProc = new Process())
            {
                grouperProc.StartInfo.WorkingDirectory = _appPath;
                grouperProc.StartInfo.FileName = Path.Combine(_appPath, "jre\\bin\\javaw.exe");
                //grouperProc.StartInfo.Arguments = @"-Xms64m -Xmx512m -cp """ + Path.Combine(_appPath, "msgmce.jar") +
                //                                  @""" msgmce/MsgBatch -i " + _inputFileName + " -u " + _uploadFileName;
                //grouperProc.StartInfo.Arguments = @"-Xms512m -Xmx1024m -cp """ + Path.Combine(_appPath, "msgmce.jar") +
                //                                  @""" com.mmm.his.msgmce.Main -i """ + Path.Combine(_appPath, _inputFileName) +
                //                                  @""" -u """ + Path.Combine(_appPath, _uploadFileName)+ @"""";
                grouperProc.StartInfo.Arguments = @"-Xms512m -Xmx1024m -cp """ + Path.Combine(_appPath, "msgmce.jar") +
                                                  @""" com.mmm.his.msgmce.Main -i " + Path.Combine(_grouperFilePath, _inputFileName) +
                                                  @" -u " + Path.Combine(_grouperFilePath, _uploadFileName);


                grouperProc.StartInfo.UseShellExecute = false;
                grouperProc.StartInfo.CreateNoWindow = true;
                //grouperProc.StartInfo.RedirectStandardOutput = true;
                grouperProc.StartInfo.RedirectStandardError = true;
                grouperProc.Start();
                //outputText = grouperProc.StandardOutput.ReadToEnd();
                //grouperProc.WaitForExit();

                do
                {
                    //Trace.WriteLine("Info  => " + grouperProc.StandardOutput.ReadLineAsync().Result);
                    Trace.WriteLine("Error => " + grouperProc.StandardError.ReadLineAsync().Result);
                }
                while (!grouperProc.HasExited);


                //Trace.WriteLine("Error => " + grouperProc.StandardError.ReadToEnd());
                
            }

            // Once the grouper is run, delete the input file and log file privacy.

            //var deleteTask = Task.Factory.StartNew(() =>
            //{
            //    File.Delete(Path.Combine(_grouperFilePath, _inputFileName));
            //    File.Delete(Path.Combine(_appPath, _logFileName));
            //});
            //deleteTask.Wait();

            // Open the upload file for processing the output from the grouper.
            OpenUploadFile();

            return true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if(_inputFileIsOpen)
                CloseInputFile();

            if(_uploadFileIsOpen)
                CloseUploadFile();
        }
    }
}

