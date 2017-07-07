using System;
using Monahrq.Sdk.Modules.Wings;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain.Wings.Repository; 

using Microsoft.Practices.Prism.MefExtensions.Modularity;

namespace Monahrq.Sdk.Attributes.Wings
{
    /// <summary>
    /// The custom Monahrq pluggable wing (plugin module) to load modules during application initialization via the bootstrapper.
    /// </summary>
    /// <seealso cref="Microsoft.Practices.Prism.MefExtensions.Modularity.PluginModuleExportAttribute" />
    /// <seealso cref="Monahrq.Sdk.Modules.Wings.IWingModule" />
    [AttributeUsage(AttributeTargets.Class)]
    public class WingModuleAttribute : PluginModuleExportAttribute, IWingModule
    {
        /// <summary>
        /// Gets the unique identifier of this Wing module
        /// </summary>
        public string Guid { get; private set; }

        /// <summary>
        /// Gets the description of this Wing module
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the display order of this Wing module; lower values are displayed first
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WingModuleAttribute"/> class.
        /// </summary>
        /// <param name="moduleType">Type of the module.</param>
        /// <param name="wingGuid">The wing unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        public WingModuleAttribute(Type moduleType, string wingGuid, string name, string description)
            : base(name, moduleType)
        {
            Guid = new Guid(wingGuid).ToString();
            Description = description;
        }

        /// <summary>
        /// Creates a new <see cref="Wing"/> object using the metadata in this <see cref="WingModuleAttribute"/> as a template.
        /// </summary>
        /// <returns>The created <see cref="Wing"/></returns>
        internal Wing FactoryWing()
        {
            var wing = WingRepository.New(ModuleName);
            wing.Description = Description;
            wing.WingGUID = System.Guid.Parse(Guid);
			wing.LastWingUpdate = DateTime.Now;
            return wing;
        }
    }
}