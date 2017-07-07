using System;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Entities.Core.Import;

namespace Monahrq.Infrastructure.Entities.Core
{
    /// <summary>
    /// The custom ersioned data reader dictionary export attribute.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Core.VersionedComponentExportAttribute" />
    [Obsolete("This attribute has been deprecated in Monahrq version 6.0 Build 2.")]
    public class VersionedDataReaderDictionaryExportAttribute : VersionedComponentExportAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedDataReaderDictionaryExportAttribute"/> class.
        /// </summary>
        /// <param name="contractName">Name of the contract.</param>
        /// <param name="major">The major.</param>
        /// <param name="minor">The minor.</param>
        public VersionedDataReaderDictionaryExportAttribute(string contractName, sbyte major, sbyte minor)
            : base(contractName, typeof(IDataReaderDictionary), major, minor)
        {}
    }

    /// <summary>
    /// The Versioned Component Export Attribute.
    /// </summary>
    /// <seealso cref="System.ComponentModel.Composition.ExportAttribute" />
    public class VersionedComponentExportAttribute : ExportAttribute
    {
        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public int Priority { get; set; }
        /// <summary>
        /// Gets the major.
        /// </summary>
        /// <value>
        /// The major.
        /// </value>
        public sbyte Major { get; private set; }
        /// <summary>
        /// Gets the minor.
        /// </summary>
        /// <value>
        /// The minor.
        /// </value>
        public sbyte Minor { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedComponentExportAttribute"/> class.
        /// </summary>
        /// <param name="contractName">Name of the contract.</param>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="major">The major.</param>
        /// <param name="minor">The minor.</param>
        public VersionedComponentExportAttribute(string contractName, Type exportType, sbyte major, sbyte minor)
            : base(contractName, exportType)
        {
            Major = major; 
            Minor = minor;
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public decimal Version
        {
            get
            {
                return decimal.Parse(string.Format("{0}.{1}", Major.ToString(), Minor.ToString()));
            }
        }
    }
}
