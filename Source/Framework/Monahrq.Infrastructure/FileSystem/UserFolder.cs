using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Exceptions;
using Monahrq.Infrastructure.Validation;

namespace Monahrq.Infrastructure.FileSystem
{
    /// <summary>
    /// The user folder entity.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.FileSystem.IUserFolder" />
    [Export(typeof(IUserFolder))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class UserFolder : IUserFolder
    {
        private readonly IUserFolderRoot _root;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFolder"/> class.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configService">The configuration service.</param>
        [ImportingConstructor]
        public UserFolder(IUserFolderRoot root
            , [Import(LogNames.Session)]ILogWriter logger
           , IConfigurationService configService)
        {
            ConfigService = configService;
            _root = root;
            Logger = logger;
        }

        /// <summary>
        /// Gets or sets the configuration service.
        /// </summary>
        /// <value>
        /// The configuration service.
        /// </value>
        IConfigurationService ConfigService { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFolder"/> class.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="logger">The logger.</param>
        public UserFolder(IUserFolderRoot root, ILogWriter logger)
            : this(root, logger, new ConfigurationService())
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFolder"/> class.
        /// </summary>
        public UserFolder() : this(new UserFolderRoot(), NullLogger.Instance) { }


        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public ILogWriter Logger { get; set; }



        /// <summary>
        /// Gets the root folder.
        /// </summary>
        /// <value>
        /// The root folder.
        /// </value>
        public string RootFolder
        {
            get
            {
                return _root.RootFolder;
            }
        }

        /// <summary>
        /// Gets the temporary folder.
        /// </summary>
        /// <value>
        /// The temporary folder.
        /// </value>
        public string TempFolder
        {
            get
            {
                return _root.TempFolder;
            }
        }

        /// <summary>
        /// Makes the destination file name available.
        /// </summary>
        /// <param name="destinationFileName">Name of the destination file.</param>
        /// <exception cref="MonahrqCoreException"></exception>
        private void MakeDestinationFileNameAvailable(string destinationFileName)
        {
            bool isDirectory = Directory.Exists(destinationFileName);
            // Try deleting the destination first
            try
            {
                if (isDirectory)
                    Directory.Delete(destinationFileName);
                else
                    File.Delete(destinationFileName);
            }
            catch
            {
                // We land here if the file is in use, for example. Let's move on.
            }

            if (isDirectory && Directory.Exists(destinationFileName))
            {
                Logger.Warning("Could not delete recipe execution folder {0} under {1}", destinationFileName, RootFolder);
                return;
            }
            // If destination doesn't exist, we are good
            if (!File.Exists(destinationFileName))
                return;

            // Try renaming destination to a unique filename
            const string extension = "deleted";
            for (int i = 0; i < 100; i++)
            {
                var newExtension = (i == 0 ? extension : string.Format("{0}{1}", extension, i));
                var newFileName = Path.ChangeExtension(destinationFileName, newExtension);
                try
                {
                    File.Delete(newFileName);
                    File.Move(destinationFileName, newFileName);

                    // If successful, we are done...
                    return;
                }
                catch
                {
                    // We need to try with another extension
                }
            }

            // Try again with the original filename. This should throw the same exception
            // we got at the very beginning.
            try
            {
                File.Delete(destinationFileName);
            }
            catch (Exception e)
            {
                throw new MonahrqCoreException(string.Format("Unable to make room for file \"{0}\" in \"{1}\" folder", destinationFileName, RootFolder), e);
            }
        }

        /// <summary>
        /// Combine a set of virtual paths relative to "~/App_Data" into an absolute physical path
        /// starting with "_basePath".
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns></returns>
        public string CombineToPhysicalPath(params string[] paths)
        {
            return PathValidation.ValidatePath(RootFolder, Path.Combine(RootFolder, Path.Combine(paths)).Replace('/', Path.DirectorySeparatorChar));
        }


        /// <summary>
        /// Creates the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="content">The content.</param>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public void CreateFile(string path, string content)
        {
            using (var stream = CreateFile(path))
            {
                using (var tw = new StreamWriter(stream))
                {
                    tw.Write(content);
                }
            }
        }

        /// <summary>
        /// Creates the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public Stream CreateFile(string path)
        {
            var filePath = CombineToPhysicalPath(path);
            var folderPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return File.Create(filePath);
        }

        /// <summary>
        /// Reads the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public string ReadFile(string path)
        {
            var physicalPath = CombineToPhysicalPath(path);
            return File.Exists(physicalPath) ? File.ReadAllText(physicalPath) : null;
        }

        /// <summary>
        /// Opens the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public Stream OpenFile(string path)
        {
            return File.OpenRead(CombineToPhysicalPath(path));
        }

        /// <summary>
        /// Stores the file.
        /// </summary>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="destinationPath">The destination path.</param>
        public void StoreFile(string sourceFileName, string destinationPath)
        {
            Logger.Information("Storing file \"{0}\" as \"{1}\" in \"{2}\" folder", sourceFileName, destinationPath, RootFolder);

            var destinationFileName = CombineToPhysicalPath(destinationPath);
            MakeDestinationFileNameAvailable(destinationFileName);
            File.Copy(sourceFileName, destinationFileName);
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="path">The path.</param>
        public void DeleteFile(string path)
        {
            Logger.Information("Deleting file \"{0}\" from \"{1}\" folder", path, RootFolder);
            MakeDestinationFileNameAvailable(CombineToPhysicalPath(path));
        }

        /// <summary>
        /// Gets the file last write time UTC.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public DateTime GetFileLastWriteTimeUtc(string path)
        {
            return File.GetLastWriteTimeUtc(CombineToPhysicalPath(path));
        }

        /// <summary>
        /// Files the exists.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public bool FileExists(string path)
        {
            return File.Exists(CombineToPhysicalPath(path));
        }

        /// <summary>
        /// Directories the exists.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(CombineToPhysicalPath(path));
        }

        /// <summary>
        /// Lists the files.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public IEnumerable<string> ListFiles(string path)
        {
            var directoryPath = CombineToPhysicalPath(path);
            if (!Directory.Exists(directoryPath))
                return Enumerable.Empty<string>();

            var files = Directory.GetFiles(directoryPath);

            return files.Select(file =>
            {
                var fileName = Path.GetFileName(file);
                return CombineToPhysicalPath(path, fileName);
            });
        }

        /// <summary>
        /// Lists the directories.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public IEnumerable<string> ListDirectories(string path)
        {
            var directoryPath = CombineToPhysicalPath(path);
            if (!Directory.Exists(directoryPath))
                return Enumerable.Empty<string>();

            var files = Directory.GetDirectories(directoryPath);

            return files.Select(file =>
            {
                var fileName = Path.GetFileName(file);
                return CombineToPhysicalPath(path, fileName);
            });
        }

        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="path">The path.</param>
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(CombineToPhysicalPath(path));
        }

        /// <summary>
        /// Maps the path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public string MapPath(string path)
        {
            return CombineToPhysicalPath(path);
        }

        /// <summary>
        /// Gets the data model folder.
        /// </summary>
        /// <value>
        /// The data model folder.
        /// </value>
        public string DataModelFolder
        {
            get 
            { 
                return "DataModel"; 
            }
        }

        /// <summary>
        /// Gets the data model file.
        /// </summary>
        /// <value>
        /// The data model file.
        /// </value>
        public string DataModelFile
        {
            get 
            { 
                return Path.Combine(DataModelFolder, "configcache.bin"); 
            }
        }

        /// <summary>
        /// Gets the scratch pad folder.
        /// </summary>
        /// <value>
        /// The scratch pad folder.
        /// </value>
        public string ScratchPadFolder
        {
            get 
            { 
                const string scratchPadFolder = "{F8090174-541B-4C5E-AD5D-A421FDC98008}";
                if(!this.DirectoryExists(scratchPadFolder))
                {
                    this.CreateDirectory(scratchPadFolder);
                }
                return CombineToPhysicalPath(scratchPadFolder);
            }
        }

        /// <summary>
        /// Guards the ignore.
        /// </summary>
        /// <param name="action">The action.</param>
        static private void GuardIgnore(Action action)
        {
            try
            {
                action();
            }
            catch { }
        }

        /// <summary>
        /// Empties the scratch pad.
        /// </summary>
        public void EmptyScratchPad()
        {
            Directory.GetFiles(ScratchPadFolder).ToList().ForEach(f => GuardIgnore(() => File.Delete(f)));
        }


        /// <summary>
        /// Gets the scratch pad file.
        /// </summary>
        /// <value>
        /// The scratch pad file.
        /// </value>
        public string ScratchPadFile
        {
            get { return Path.Combine(ScratchPadFolder, Path.GetFileName(Path.GetRandomFileName())); }
        }
    }
}
