using System;
using System.ComponentModel.Composition;
using Monahrq.Sdk.Regions;

namespace Monahrq.Sdk.Attributes
{
    /// <summary>
    /// Monahrq view / usercontrol custom MEF attribute that is utilized in conjunction with Prism.
    /// </summary>
    /// <seealso cref="System.ComponentModel.Composition.ExportAttribute" />
    /// <seealso cref="Monahrq.Sdk.Regions.IViewRegionRegistration" />
    [Export]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [MetadataAttribute]
    public class ViewExportAttribute : ExportAttribute, IViewRegionRegistration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewExportAttribute"/> class.
        /// </summary>
        /// <param name="viewType">Type of the view.</param>
        public ViewExportAttribute(Type viewType)
            : base(viewType)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewExportAttribute"/> class.
        /// </summary>
        /// <param name="contractName">Name of the contract.</param>
        /// <param name="viewType">Type of the view.</param>
        public ViewExportAttribute(string contractName,Type viewType)
            : base(contractName, viewType)
        { }

        /// <summary>
        /// Gets the name of the view.
        /// </summary>
        /// <value>
        /// The name of the view.
        /// </value>
        public string ViewName { get { return ContractName; } }

        /// <summary>
        /// Gets or sets the name of the region.
        /// </summary>
        /// <value>
        /// The name of the region.
        /// </value>
        public string RegionName { get; set; }
    }
}
