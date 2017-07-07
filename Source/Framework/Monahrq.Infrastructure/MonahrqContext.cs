using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Monahrq.Infrastructure.Diagnostics;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Extensibility;

namespace Monahrq.Infrastructure
{
    /// <summary>
    /// The Monahrq Context utility/helper class.
    /// </summary>
    public static class MonahrqContext
    {
        private static List<string> _reportingStates;

        public static IMonahrqShell MonahrqShell { get; set; }

        /// <summary>
        /// Gets or sets the reporting states context.
        /// </summary>
        /// <value>
        /// The reporting states context.
        /// </value>
        public static List<string> ReportingStatesContext
        {
            get
            {
                return _reportingStates ?? new List<string>();
            }
            set
            {
                _reportingStates = value;
            }
        }

        /// <summary>
        /// Gets the bin folder path.
        /// </summary>
        /// <value>
        /// The bin folder path.
        /// </value>
        public static string BinFolderPath
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Gets the grouper version.
        /// </summary>
        /// <value>
        /// The grouper version.
        /// </value>
        public static string GrouperVersion
        {
            get
            {
                return "33";
            }
        }

        /// <summary>
        /// Gets the name of the get user.
        /// </summary>
        /// <value>
        /// The name of the get user.
        /// </value>
        public static string GetUserName
        {
            get
            {
                return MonahrqDiagnostic.GetUserName;
            }
        }

        /// <summary>
        /// Determines whether this instance is administrator.
        /// </summary>
        /// <returns></returns>
        public static bool IsAdministrator()
        {
            return MonahrqDiagnostic.IsAdministrator();
        }

        /// <summary>
        /// Gets the help folder path.
        /// </summary>
        /// <value>
        /// The help folder path.
        /// </value>
        public static string HelpFolderPath
        {
            get { return Path.Combine(BinFolderPath, @"Resources\Help\Monahrq Help-Default.chm"); }
        }

        /// <summary>
        /// Gets the website help folder path.
        /// </summary>
        /// <value>
        /// The help folder path.
        /// </value>
        public static string WebsiteHelpFolderPath
        {
            get { return Path.Combine(BinFolderPath, @"Resources\Help\MONAHRQ_Errors_Warnings.pdf"); }
        }


        /// <summary>
        /// Gets or sets the type of the selected region.
        /// </summary>
        /// <value>
        /// The type of the selected region.
        /// </value>
        public static Type SelectedRegionType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        public static string ApplicationName
        {
            get { return "Monahrq 7"; }
        }

        /// <summary>
        /// Gets the old name of the application.
        /// </summary>
        /// <value>
        /// The old name of the application.
        /// </value>
        public static string OldApplicationName
        {
            get { return "Monahrq 6.0"; }
        }

        /// <summary>
        /// Gets the application version.
        /// </summary>
        /// <value>
        /// The application version.
        /// </value>
        public static string ApplicationVersion
        {
            get
            {
                //var version = typeof(MonahrqContext).Assembly.GetName().Version;
                //return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.MinorRevision);
                return Application.ProductVersion.SubStrBeforeLast(".");
                //return "6.0"; //.0.0
            }
        }

        /// <summary>
        /// Gets the product version.
        /// </summary>
        /// <value>
        /// The product version.
        /// </value>
        public static string ProductVersion
        {
            get
            {
                return Application.ProductVersion;
            }
        }

        /// <summary>
        /// Gets the data folder.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="versionNumb">The version numb.</param>
        /// <returns></returns>
        public static string GetDataFolder(string contract, string versionNumb)
        {
            var contractName = (contract == null || string.IsNullOrWhiteSpace(contract)) ? "Default" : contract;
            var version = (versionNumb == null || string.IsNullOrWhiteSpace(versionNumb)) ? (0.0M).ToString(CultureInfo.InvariantCulture) : versionNumb;
            var path = Path.Combine(GetUserApplicationDataFolder(), contractName, string.Format("{0}", version));

            return path;
        }

        /// <summary>
        /// Gets the user application data folder.
        /// </summary>
        /// <returns></returns>
        public static string GetUserApplicationDataFolder()
        {
            var assy = Assembly.GetExecutingAssembly();
            var company = assy.GetCustomAttribute<AssemblyCompanyAttribute>() ?? new AssemblyCompanyAttribute("{39049C7F-E216-4404-8FA7-4A213365DD19}");
            var product = assy.GetCustomAttribute<AssemblyProductAttribute>() ?? new AssemblyProductAttribute("{9022C9CA-A2E7-46CD-9FBE-AE7A77C44F53}");
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), company.Company, product.Product);

            return path;
        }

        /// <summary>
        /// Gets the user application data folder.
        /// </summary>
        /// <returns></returns>
        public static string GetUserApplicatioTempFolder()
        {
           
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "Monahrq");
            return path;
        }


        /// <summary>
        /// Gets or sets a value indicating whether this instance is inializing.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is inializing; otherwise, <c>false</c>.
        /// </value>
        public static bool IsInializing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [force database up grade].
        /// </summary>
        /// <value>
        /// <c>true</c> if [force database up grade]; otherwise, <c>false</c>.
        /// </value>
        public static bool ForceDbUpGrade { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [force database recreate].
        /// </summary>
        /// <value>
        /// <c>true</c> if [force database recreate]; otherwise, <c>false</c>.
        /// </value>
        public static bool ForceDbRecreate { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is major database upgrade.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is major database upgrade; otherwise, <c>false</c>.
        /// </value>
        public static bool IsMajorDbUpgrade { get; set; }
        /// <summary>
        /// Gets or sets the database name to up grade.
        /// </summary>
        /// <value>
        /// The database name to up grade.
        /// </value>
        public static string DbNameToUpGrade { get; set; }

        /// <summary>
        /// Checks if connected to internet.
        /// </summary>
        /// <param name="throwEvent">if set to <c>true</c> [throw event].</param>
        /// <returns></returns>
        public static bool CheckIfConnectedToInternet(bool throwEvent = true)
        {
            return MonahrqDiagnostic.CheckIfConnectedToInternet(throwEvent);
        }

        /// <summary>
        /// Gets my documents application dir path.
        /// </summary>
        /// <value>
        /// My documents application dir path.
        /// </value>
        public static string MyDocumentsApplicationDirPath
        {
            get
            {
                var directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                directoryPath = Path.Combine(directoryPath, "Monahrq");
                CreateDirectoryIfNotExists(directoryPath);
                return directoryPath;
            }
        }

        /// <summary>
        /// Gets the file exports dir path.
        /// </summary>
        /// <value>
        /// The file exports dir path.
        /// </value>
        public static string FileExportsDirPath
        {
            get
            {
                var directoryPath = Path.Combine(MyDocumentsApplicationDirPath, "Exports");
                CreateDirectoryIfNotExists(directoryPath);
                return directoryPath;
            }
        }

        /// <summary>
        /// Gets the mapping file export dir path.
        /// </summary>
        /// <value>
        /// The mapping file export dir path.
        /// </value>
        public static string MappingFileExportDirPath
        {
            get
            {
                var directoryPath = Path.Combine(MyDocumentsApplicationDirPath, "Mapping");
                CreateDirectoryIfNotExists(directoryPath);
                return directoryPath;
            }
        }

        #region Helper Methods

        /// <summary>
        /// Creates the directory if not exists.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        private static void CreateDirectoryIfNotExists(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }
        #endregion
    }
}
