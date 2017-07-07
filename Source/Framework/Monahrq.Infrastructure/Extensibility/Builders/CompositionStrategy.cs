using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Extensibility.Models;
using Monahrq.Sdk.Extensibility.Builders.Models;
using Monahrq.Sdk.Extensibility.Builders;
using Monahrq.Sdk.Extensibility.Environment.Descriptor.Models;
using Monahrq.Sdk.Extensibility.Data.Providers;
using Monahrq.Sdk.Extensibility.Utility;
using System.ComponentModel.Composition;
using System.Configuration;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records; 

namespace Monahrq.Sdk.Extensibility.Builders
{
    /// <summary>
    /// Service at the host level to transform the cachable descriptor into the loadable blueprint.
    /// </summary>
    public interface ICompositionStrategy
    {
        ILogWriter Logger { get; set; }
        /// <summary>
        /// Using information from the IExtensionManager, transforms and populates all of the
        /// blueprint model the shell builders will need to correctly initialize a tenant IoC container.
        /// </summary>
        ShellBlueprint Compose(ConnectionStringSettings settings, ShellDescriptor shellDescriptor);
    }

    [Export(typeof(ICompositionStrategy))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CompositionStrategy : ICompositionStrategy
    {
        IExtensionManager ExtensionManager { get; set; }
        [ImportingConstructor]
        public CompositionStrategy([Import(RequiredCreationPolicy = CreationPolicy.Shared)] IExtensionManager extensionManager)
        {
            ExtensionManager = extensionManager;
            Logger = NullLogger.Instance;
        }

        public ILogWriter Logger { get; set; }

        private static IEnumerable<Feature> BuiltInFeatures()
        {
            return typeof(AbstractDataServicesProvider).Assembly.ExportedTypes
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(type =>
                     new Feature()
                     {
                         Descriptor = new FeatureDescriptor()
                         {
                             Name = type.FullName,
                             Extension = new ExtensionDescriptor()
                         },
                         ExportedTypes = new[] { type }

                     });

        }

        public ShellBlueprint Compose(ConnectionStringSettings settings, ShellDescriptor descriptor)
        {
            Logger.Debug("Composing blueprint");
            var enabledFeatures = ExtensionManager.EnabledFeatures(descriptor);
            var features = ExtensionManager.LoadFeatures(enabledFeatures);
            //var featureNames = features.Select(f => f.Descriptor.Name).ToArray();
            var records = BuildBlueprint(features,
                (call) =>
                {
                    var isRecord = IsRecord(call);
                    return isRecord;
                },
                (t, f) => BuildRecord(t, f, settings))
                .Distinct(new GenericEqualityComparer<RecordBlueprint>((x, y) => FeatureDescriptorUtility.Equals(x.Feature, y.Feature), (x) => FeatureDescriptorUtility.GetHashCode(x.Feature)));

            var result = new ShellBlueprint
            {
                Settings = settings,

                Records = records,
            };

            Logger.Debug("Done composing blueprint");
            return result;
        }


        private static IEnumerable<T> BuildBlueprint<T>(
            IEnumerable<Feature> features,
            Func<Type, bool> predicate,
            Func<Type, Feature, T> selector)
        {
            return features.SelectMany(
                feature => feature.ExportedTypes
                               .Where(predicate)
                               .Select(type => selector(type, feature)))
                .ToArray();
        }

        private static bool IsRecord(Type type)
        {
            var idProp = type.GetProperty("Id");
            if (idProp == null) return false;
            var domainType = typeof(IEntity);
            bool retval = type.GetProperty("Id") != null &&
                   (type.GetProperty("Id").GetAccessors()).All(x => x.IsVirtual) &&
                   !type.IsSealed &&
                   !type.IsAbstract &&
                    domainType.IsAssignableFrom(type);
            return retval;
        }

        private static RecordBlueprint BuildRecord(Type type, Feature feature, ConnectionStringSettings settings)
        {
            return new RecordBlueprint
            {
                Type = type,
                Feature = feature,
                TableName = type.EntityTableName(),
            };
        }
    }
}