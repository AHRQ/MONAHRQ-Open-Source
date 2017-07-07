using System;
using System.ComponentModel.Composition;

namespace Monahrq.Infrastructure.BaseDataLoader
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.Composition.ExportAttribute" />
    public class BaseDataImportAttribute : ExportAttribute
    {
        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public int Priority { get; set; }
        /// <summary>
        /// Gets or sets the version strategy.
        /// </summary>
        /// <value>
        /// The version strategy.
        /// </value>
        public Type VersionStrategy { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDataImportAttribute"/> class.
        /// </summary>
        /// <param name="contractName">Name of the contract.</param>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="versionStrategy">The version strategy.</param>
        /// <param name="priority">The priority.</param>
        /// <exception cref="ArgumentException">Wrong Version Strategy class;versionStrategy</exception>
        public BaseDataImportAttribute(string contractName, Type exportType, Type versionStrategy, int? priority = null)
            : base(contractName, exportType)
        {
            if (versionStrategy != null && versionStrategy.IsSubclassOf(typeof(BaseDataVersionStrategy)))
            {
                throw new ArgumentException(@"Wrong Version Strategy class", "versionStrategy");
            }

            VersionStrategy = versionStrategy;
            Priority = priority ?? 10;
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version
        {
            get
            {
                //switch (VersionType)
                //{
                //    case BaseDataVersionType.MajorMinor:
                //        return string.Format("{0}.{1}", Major, Minor);
                //    case BaseDataVersionType.MonthAndYear:
                //        return string.Format("{0:00}/{1:0000}", Month, Year);
                //    case BaseDataVersionType.YearOnly:
                //        return string.Format("{0:0000}", Year);
                //    default:
                //        return "";
                //}
                return null;
            }
        }
    }

}
