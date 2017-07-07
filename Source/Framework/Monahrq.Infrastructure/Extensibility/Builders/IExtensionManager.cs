using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monahrq.Sdk.Extensibility.Environment.Descriptor.Models;
using Monahrq.Sdk.Extensibility.Models; 

namespace Monahrq.Sdk.Extensibility.Builders
{
    public interface IExtensionManager
    {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
        IEnumerable<FeatureDescriptor> AvailableFeatures();

        ExtensionDescriptor GetExtension(string id);

        IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors);

        void Reset();
    }

    public static class ExtensionManagerExtensions
    {
        public static IEnumerable<FeatureDescriptor> EnabledFeatures(this IExtensionManager extensionManager, ShellDescriptor descriptor)
        {
            return extensionManager.AvailableFeatures();
        }
    }
}
