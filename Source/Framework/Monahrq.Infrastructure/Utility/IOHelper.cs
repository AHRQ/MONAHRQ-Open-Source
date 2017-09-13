using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Logging;

namespace Monahrq.Infrastructure.Utility
{
    public class IOHelper
    {
        /// <summary>
        /// Directories the copy2.
        /// </summary>
        /// <param name="sourceDirPath">The source dir path.</param>
        /// <param name="destDirPath">The dest dir path.</param>
        public static void DirectoryCopy2(string sourceDirPath, string destDirPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourceDirPath, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourceDirPath, destDirPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourceDirPath, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourceDirPath, destDirPath), true);
        }

        /// <summary>
        /// Directories the copy.
        /// </summary>
        /// <param name="sourceDirPath">The source dir path.</param>
        /// <param name="destDirPath">The dest dir path.</param>
        /// <param name="copySubDirs">if set to <c>true</c> [copy sub dirs].</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.IO.DirectoryNotFoundException">Source directory does not exist or could not be found:
        /// + sourceDirPath</exception>
        public static void DirectoryCopy(string sourceDirPath, string destDirPath, bool copySubDirs, ILogWriter logger = null)
        {
            // Get the subdirectories for the specified directory.
            try
            {
                DirectoryInfo dir = new DirectoryInfo(sourceDirPath);
                DirectoryInfo[] dirs = dir.GetDirectories();

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + sourceDirPath);
                }

                // If the destination directory doesn't exist, create it. 
                if (!Directory.Exists(destDirPath))
                {
                    Directory.CreateDirectory(destDirPath);
                }

                // Get the files in the directory and copy them to the new location.
                var files = dir.GetFiles();

                Parallel.ForEach(files, file =>
                    {
                        try
                        {
                            string temppath = Path.Combine(destDirPath, file.Name);
                            var destFile = file.CopyTo(temppath, true);
                            destFile.LastAccessTime = DateTime.Now;
                            destFile.LastAccessTimeUtc = DateTime.UtcNow;
                            destFile.IsReadOnly = false;
                        }
                        catch (Exception exc)
                        {
                            logger?.Write($"Error copying file {file.FullName}: {(exc.InnerException ?? exc).Message}", TraceEventType.Error);
                        }
                    });

                // If copying subdirectories, copy them and their contents to new location. 
                if (copySubDirs)
                {
                    Parallel.ForEach(dirs, subdir =>
                        {
                            try
                            {
                                string temppath = Path.Combine(destDirPath, subdir.Name);
                                DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                            }
                            catch (Exception exc)
                            {
                                logger?.Write(exc, "Error copying subdirectory {0}", subdir);
                            }
                        });
                }

            }
            catch (Exception exc)
            {
                logger?.Write(exc, "Error copying directory \"{0}\" to \"{1}\"", sourceDirPath, destDirPath);

                throw exc;
            }
        }

        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="sourceDirPath">The source dir path.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">sourceDirPath;@Please provide a valid source directory path.</exception>
        public static void CreateDirectory(string sourceDirPath, bool overwrite = false, ILogWriter logger = null)
        {
            if (string.IsNullOrEmpty(sourceDirPath))
                throw new ArgumentNullException("sourceDirPath", @"Please provide a valid source directory path.");

            var directory = new DirectoryInfo(sourceDirPath);

            if (directory.Exists && overwrite)
            {
                // Delete all non-read only directories
                Parallel.ForEach(directory.GetDirectories(), dir =>
                {

                    try
                    {
                        if (!dir.Attributes.HasFlag(FileAttributes.ReadOnly))
                        {
                            dir.Delete(true);

                        }
                        else
                        {
                            dir.Attributes = FileAttributes.Normal;
                            dir.Delete(true);
                        }
                    }
                    catch (Exception exc)
                    {
                        logger?.Write(exc);
                    }
                });

                // Retrieve the rest of the files
                var files = directory.GetFiles("*", SearchOption.AllDirectories).ToList();

                // Delete existing files
                Parallel.ForEach(files, file => Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            file.IsReadOnly = false;
                            file.Delete();
                        }
                        catch (Exception exc) /*Don't care if it fails*/
                        {
                            logger?.Write(exc);
                        }
                    }));
                directory.Create();
                logger?.Information("Creating output directory: \"" + directory.Name + "\"", Category.Exception, Priority.High);
            }

            if (!directory.Exists)
            {
                directory.Create();
                logger?.Information("Creating output directory: \"" + directory.Name + "\"", Category.Exception, Priority.High);
            }
        }

        /// <summary>
        /// Deletes the folder recursive.
        /// </summary>
        /// <param name="baseDir">The base dir.</param>
        public static void DeleteFolderRecursive(DirectoryInfo baseDir)
        {
            baseDir.Attributes = FileAttributes.Normal;
            foreach (var childDir in baseDir.GetDirectories("*", SearchOption.AllDirectories))
                DeleteFolderRecursive(childDir);

            foreach (var file in baseDir.GetFiles("*", SearchOption.AllDirectories))
                file.IsReadOnly = false;

            baseDir.Delete(true);
        }

        /// <summary>
        /// Renames the directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="newDirectory">The new directory.</param>
        /// <param name="directoryExclusionPattern">The directory exclusion pattern.</param>
        /// <param name="fileExclusionPattern">The file exclusion pattern.</param>
        /// <param name="skipIfExists">if set to <c>true</c> [skip if exists].</param>
        public static void RenameDirectory(DirectoryInfo directory, DirectoryInfo newDirectory, string directoryExclusionPattern = null, string fileExclusionPattern = null, bool skipIfExists = false)
        {
            if (directory == null || newDirectory == null) return;

            //var directory = new DirectoryInfo(directoryPath);
            //var newDirectory = new DirectoryInfo(newDirectoryPath);

            if (!directory.Exists) return;

            //if(newDirectory.Exists)
            //    DeleteFolderRecursive(newDirectory);

            //var directoriesToCopy = directory.GetDirectories("*", SearchOption.AllDirectories).ToArray();

            //if (directoriesToSkip != null && directoriesToSkip.Any())
            //{
            //    directoriesToCopy = directoriesToCopy.Where(d => !d.Name.In(directoriesToSkip)).ToArray();
            //}

            // Go through the Directories and recursively call the DeepCopy Method for each one
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                if (!string.IsNullOrEmpty(directoryExclusionPattern))
                {
                    var match = Regex.Match(dir.Name, directoryExclusionPattern);

                    if (!match.Success)
                        RenameDirectory(dir, newDirectory.CreateSubdirectory(dir.Name), directoryExclusionPattern);
                }
                else
                {
                    RenameDirectory(dir, newDirectory.CreateSubdirectory(dir.Name));
                }
            }

            // Go ahead and copy each file to the target directory
            foreach (FileInfo file in directory.GetFiles())
            {
                if (skipIfExists && File.Exists(Path.Combine(newDirectory.FullName, file.Name)))
                    continue;

                if (!string.IsNullOrEmpty(fileExclusionPattern))
                {
                    var match = Regex.Match(file.Name, fileExclusionPattern);

                    if (!match.Success)
                        file.CopyTo(Path.Combine(newDirectory.FullName, file.Name), true);
                }
                else
                {
                    file.CopyTo(Path.Combine(newDirectory.FullName, file.Name), true);
                }
            }

            //if (directory.Exists)
            //{
            //    //Task.Run()
            //    DeleteFolderRecursive(directory);
            //}
        }
    }
}