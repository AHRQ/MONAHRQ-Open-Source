using System;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure;
using PropertyChanged;

namespace Monahrq.ViewModels
{
    /// <summary>
    /// Class for About window
    /// </summary>
    [Export, ImplementPropertyChanged]
    public class AboutViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AboutViewModel"/> class.
        /// </summary>
        public AboutViewModel()
        {
            // Product Version
            ProductVersion = MonahrqContext.ApplicationVersion.TrimEnd();
            // DisplayVersion
            Version = string.Format("{0} {1}", MonahrqContext.ApplicationName.Replace("Monahrq ", null), DateTime.Now.Year);

            GrouperVersion = MonahrqContext.GrouperVersion;
        }

        /// <summary>
        /// Gets or sets the grouper version.
        /// </summary>
        /// <value>
        /// The grouper version.
        /// </value>
        public string GrouperVersion { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the display version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the actual product build version.
        /// </summary>
        /// <value>
        /// The product version.
        /// </value>
        public string ProductVersion { get; set; }
    }
}
