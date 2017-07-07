using System;
using System.Collections.Generic;
using System.IO;

namespace Monahrq.Infrastructure.FileSystem
{
    /// <summary>
    /// Abstraction of Users folder. 
    /// </summary>
    public interface IUserFolder
    {
        /// <summary>
        /// Gets the scratch pad folder.
        /// </summary>
        /// <value>
        /// The scratch pad folder.
        /// </value>
        string ScratchPadFolder { get; }
        /// <summary>
        /// Gets the scratch pad file.
        /// </summary>
        /// <value>
        /// The scratch pad file.
        /// </value>
        string ScratchPadFile { get; }
        /// <summary>
        /// Empties the scratch pad.
        /// </summary>
        void EmptyScratchPad();

        /// <summary>
        /// Lists the files.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        IEnumerable<string> ListFiles(string path);
        /// <summary>
        /// Lists the directories.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        IEnumerable<string> ListDirectories(string path);

        /// <summary>
        /// Files the exists.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        bool FileExists(string path);
        /// <summary>
        /// Creates the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="content">The content.</param>
        void CreateFile(string path, string content);
        /// <summary>
        /// Creates the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        Stream CreateFile(string path);
        /// <summary>
        /// Reads the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        string ReadFile(string path);
        /// <summary>
        /// Opens the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        Stream OpenFile(string path);
        /// <summary>
        /// Stores the file.
        /// </summary>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="destinationPath">The destination path.</param>
        void StoreFile(string sourceFileName, string destinationPath);
        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="path">The path.</param>
        void DeleteFile(string path);

        /// <summary>
        /// Gets the file last write time UTC.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        DateTime GetFileLastWriteTimeUtc(string path);

        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="path">The path.</param>
        void CreateDirectory(string path);
        /// <summary>
        /// Directories the exists.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        bool DirectoryExists(string path);
        /// <summary>
        /// Maps the path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        string MapPath(string path);

        /// <summary>
        /// Gets the temporary folder.
        /// </summary>
        /// <value>
        /// The temporary folder.
        /// </value>
        string TempFolder { get; }
        /// <summary>
        /// Combines to physical path.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns></returns>
        string CombineToPhysicalPath(params string[] paths);
        /// <summary>
        /// Gets the data model folder.
        /// </summary>
        /// <value>
        /// The data model folder.
        /// </value>
        string DataModelFolder { get; }
        /// <summary>
        /// Gets the data model file.
        /// </summary>
        /// <value>
        /// The data model file.
        /// </value>
        string DataModelFile { get; }
    }

    /// <summary>
    /// Abstraction over the root location of "~/App_Data", mainly to enable
    /// unit testing of AppDataFolder.
    /// </summary>]
    public interface IUserFolderRoot
    {
        /// <summary>
        /// Physical path of root (typically: MapPath(RootPath))
        /// </summary>
        /// <value>
        /// The root folder.
        /// </value>
        string RootFolder { get; }
        /// <summary>
        /// Gets the temporary folder.
        /// </summary>
        /// <value>
        /// The temporary folder.
        /// </value>
        string TempFolder { get; }
    }
}