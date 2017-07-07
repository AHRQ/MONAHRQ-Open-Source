using System;
using System.Linq;

namespace Monahrq.Infrastructure.Domain.Wings
{
    [Serializable]
    public class Version
    {
        #region Properties
        /// <summary>
        /// Gets or sets the major version.
        /// </summary>
        /// <value>
        /// The major.
        /// </value>
        public virtual int Major { get; set; }
        /// <summary>
        /// Gets or sets the minor version.
        /// </summary>
        /// <value>
        /// The minor.
        /// </value>
        public virtual int Minor { get; set; }
        /// <summary>
        /// Gets or sets the milestone version.
        /// </summary>
        /// <value>
        /// The milestone.
        /// </value>
        public int Milestone { get; set; }
        /// <summary>
        /// Gets or sets the version number string.
        /// </summary>
        /// <value>
        /// The number.
        /// </value>
        public virtual string Number
        {
            get { return string.Format("{0}.{1}.{2}", Major, Minor, Milestone); }
            set
            {
                if (string.IsNullOrEmpty(value)) return;

                var versionItems = value.Split('.').ToList();
                Major = Convert.ToInt32(versionItems[0] ?? "0");
                Minor = Convert.ToInt32(versionItems[1] ?? "0");
                Milestone = Convert.ToInt32(versionItems[2] ?? "0");
            }
        }
        #endregion
    }
}
