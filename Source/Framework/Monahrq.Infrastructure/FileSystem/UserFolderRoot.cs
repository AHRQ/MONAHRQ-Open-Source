using System;
using System.ComponentModel.Composition;
using System.IO;

namespace Monahrq.Infrastructure.FileSystem
{
    /// <summary>
    /// The user folder root helper file.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.FileSystem.IUserFolderRoot" />
    [Export(typeof(IUserFolderRoot))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class UserFolderRoot : IUserFolderRoot
    {
        /// <summary>
        /// Physical path of root (typically: MapPath(RootPath))
        /// </summary>
        /// <value>
        /// The root folder.
        /// </value>
        public string RootFolder
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ahrq", "Monahrq");
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
                return Path.GetTempPath();
            }
        }
    }
}