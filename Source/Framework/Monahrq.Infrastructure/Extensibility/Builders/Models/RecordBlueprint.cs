using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Sdk.Extensibility.Models;
using System.ComponentModel.Composition;
using Monahrq.Sdk.Extensibility.Environment.Descriptor.Models;
using Monahrq.Infrastructure.Configuration;
using System.Configuration;

namespace Monahrq.Sdk.Extensibility.Builders.Models
{

    public interface IShellBlueprintFactory
    {
        ShellBlueprint CreateBlueprint();
        void Reset();
    }

    [Export(typeof(IShellBlueprintFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShellBlueprintFactory : IShellBlueprintFactory
    {
        ICompositionStrategy CompositionStrategy { get; set; }
        IConfigurationService ConfigurationService { get; set; }
        IShellDescriptorFactory DescriptorFactory { get; set; }

        [ImportingConstructor]
        public ShellBlueprintFactory(
            ICompositionStrategy compositionStrategy,
            IConfigurationService configurationService,
            [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
            IShellDescriptorFactory descriptorFactory)
        {
            CompositionStrategy = compositionStrategy;
            ConfigurationService = configurationService;
            DescriptorFactory = descriptorFactory;
        }

        public void Reset()
        {
            DescriptorFactory.Reset();
        }

        public ShellBlueprint CreateBlueprint()
        {
            return CompositionStrategy.Compose(ConfigurationService.ConnectionSettings, DescriptorFactory.CreateShellDescriptor());
        }

    }

    public class ShellBlueprint
    {
        public ConnectionStringSettings Settings { get; set; }
        public IEnumerable<RecordBlueprint> Records { get; set; }
    }

    public class ShellBlueprintItem
    {
        public Type Type { get; set; }
        public Feature Feature { get; set; }
        public override string ToString()
        {
            return string.Format("{0}\n{1}\n{2}", base.ToString(), Type.ToString(), Feature.ToString());
        }
    }

    public class RecordBlueprint : ShellBlueprintItem
    {
        public string TableName { get; set; }
        public override string ToString()
        {
            return string.Format("{0}\n{1}", base.ToString(), TableName);
        }
    }
}
