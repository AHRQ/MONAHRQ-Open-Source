using Monahrq.Sdk.Extensibility.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Monahrq.Sdk.Extensibility.Environment.Descriptor.Models
{

    /// <summary>
    /// Contains a snapshot of a tenant's enabled features.
    /// The information is drawn out of the shell via IShellDescriptorManager
    /// and cached by the host via IShellDescriptorCache. It is
    /// passed to the ICompositionStrategy to build the ShellBlueprint.
    /// </summary>
    public class ShellDescriptor
    {
        public ShellDescriptor()
        {
            Features = Enumerable.Empty<ShellFeature>();
            Parameters = Enumerable.Empty<ShellParameter>();
        }

        public int SerialNumber { get; set; }
        public IEnumerable<ShellFeature> Features { get; set; }
        public IEnumerable<ShellParameter> Parameters { get; set; }
    }

    public class ShellFeature
    {
        public string Name { get; set; }
    }

    public class ShellParameter
    {
        public string Component { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }


    public interface IShellDescriptorFactory
    {
        ShellDescriptor CreateShellDescriptor();
        void Reset();
    }

    [Export(typeof(IShellDescriptorFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShellDescriptorFactory : IShellDescriptorFactory
    {
        IExtensionManager ExtensionManager { get; set; }
        [ImportingConstructor]
        public ShellDescriptorFactory([Import(RequiredCreationPolicy = CreationPolicy.Shared)] 
                IExtensionManager extensionManager)
        {
            ExtensionManager = extensionManager;
            InitFeatures();
        }

        public void Reset()
        {
            ExtensionManager.Reset();
            InitFeatures();
        }


        void InitFeatures()
        {
            LazyCurrentFeatures = new Lazy<IEnumerable<ShellFeature>>(() => CurrentFeatures().ToArray(), true);
        }

        public ShellDescriptor CreateShellDescriptor()
        {
            return new ShellDescriptor
                {
                    Features = LazyCurrentFeatures.Value
                };
        }

        Lazy<IEnumerable<ShellFeature>> LazyCurrentFeatures { get; set; }

        private IEnumerable<ShellFeature> CurrentFeatures()
        {
            return ExtensionManager.AvailableFeatures().Select(
                  f => new ShellFeature()
                  {
                      Name = f.Name
                  });
        }
    }

}
