using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LinqKit;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Sdk.Extensibility.Builders;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.ServiceLocation;

using NHibernate.Linq;

namespace Monahrq.Sdk.Extensibility
{

    using Models;
    using Infrastructure.Entities.Domain;
    using Infrastructure.Data.Extensibility;
    using Infrastructure.Core.Attributes;

    [Export(typeof(IExtensionManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MonahrqExtensionManager : IExtensionManager
    {
        private static IModule[] modules;
        private static IEnumerable<IModule> AllModules
        {
            get
            {
                if (modules != null)
                    return modules;

                modules = ServiceLocator.Current.GetAllInstances<IModule>().ToArray();
                return modules;
            }
        }

        class TargetBlueprint : IEquatable<TargetBlueprint>
        {
            public TargetBlueprint(Target target, IEnumerable<IModule> modules)
            {
                Target = target;
                LazyClrType = new Lazy<Type>(() =>
                {
                    var modAssys = modules.Select(mod => mod.GetType().Assembly).Distinct();
                    var implementation = modAssys.SelectMany(assy => assy.GetExportedTypes())
                                .FirstOrDefault(type => type.AssemblyQualifiedName == target.ClrType);
                    return implementation;
                });
                LazyAssy = new Lazy<Assembly>(() => LazyClrType.Value == null ? null : LazyClrType.Value.Assembly);
            }

            private Lazy<Type> LazyClrType { get; set; }
            private Lazy<Assembly> LazyAssy { get; set; }


            public Type ClrType { get { return LazyClrType.Value; } }
            public Assembly Assy { get { return LazyAssy.Value; } }
            public Target Target { get; private set; }

            public bool Equals(TargetBlueprint other)
            {
                return ClrType == other.ClrType;
            }
        }
        static readonly object _syncRoot = new object();
        public MonahrqExtensionManager()
        {
            if (LazyFeatures == null || LazyExtensions == null || LazyWingTargets == null)
            {
                lock (_syncRoot)
                {
                    if (LazyFeatures == null || LazyExtensions == null || LazyWingTargets == null)
                    {
                        Reset();
                    }
                }
            }
        }

        public void Reset()
        {
            lock (_syncRoot)
            {
                InitLazyMethods();
            }
        }

        private void InitLazyMethods()
        {
            InitLazyWingTargets();
            InitLazyExtensions();
            InitLazyFeatures();
        }

        private static void InitLazyFeatures()
        {
            LazyFeatures = new Lazy<IEnumerable<FeatureDescriptor>>(() => LazyExtensions.Value.SelectMany(ext => ext.Features));
        }

        private static void InitLazyExtensions()
        {
            LazyExtensions = new Lazy<IEnumerable<ExtensionDescriptor>>(() => WingExtensionAssemblies(PredicateBuilder.True<Wing>()).Select(Create));
        }

        static IEnumerable<TargetBlueprint> _targetBlueprints;
        static IEnumerable<TargetBlueprint> TargetBlueprints
        {

            get
            {
                if (_targetBlueprints == null || !_targetBlueprints.Any())
                {
                    var prov = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
                    using (var sess = prov.SessionFactory.OpenSession())
                    {
                        _targetBlueprints = sess.Query<Target>().ToList()
                                   .Select(t => new TargetBlueprint(t, AllModules))
                                   .Where(t => t.ClrType != null)
                                   .Distinct().ToList();
                    }
                }
                return _targetBlueprints;
            }
        }

        private static void InitLazyWingTargets()
        {
            LazyWingTargets = new Lazy<IEnumerable<TargetBlueprint>>(() =>
            {
                var bp = TargetBlueprints;
                return bp;
            });
        }



        private static Lazy<IEnumerable<TargetBlueprint>> LazyWingTargets { get; set; }

        private static IEnumerable<Assembly> WingExtensionAssemblies(Expression<Func<Wing, bool>> criteria)
        {
            var comp = criteria.Compile();
            return LazyWingTargets.Value.Where(t => comp(t.Target.Owner)).Select(t => t.Assy);
        }

        static Lazy<IEnumerable<ExtensionDescriptor>> LazyExtensions { get; set; }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions()
        {
            return LazyExtensions.Value;
        }

        private static Lazy<IEnumerable<FeatureDescriptor>> LazyFeatures { get; set; }

        public IEnumerable<FeatureDescriptor> AvailableFeatures()
        {
            return LazyFeatures.Value;
        }

        public ExtensionDescriptor GetExtension(string id)
        {
            return WingExtensionAssemblies(PredicateBuilder.True<Wing>().And(w => w.WingGUID == Guid.Parse(id)))
                  .Select(Create).FirstOrDefault();
        }

        public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors)

        {
                // Get the assemblies that contain registered data set extensions AKA "Targets"
            var loadThese = WingExtensionAssemblies(PredicateBuilder.True<Wing>())
                // select the exportedd types from those assemblies
                .SelectMany(assy => assy.ExportedTypes)
                //Union with all the types found the assembly that defined ITypeProvider
                .Union(typeof(ITypeProvider).Assembly.GetExportedTypes())
                .Distinct();

            var results = loadThese.Select(type =>
            {
                var result = new Feature();
                result.Descriptor = CreateFeatureDesc(type, Create(type.Assembly));
                result.ExportedTypes = new[] { type };
                return result;
            });

            return results;
        }

        private static ExtensionDescriptor Create(Assembly wingAssy)
        {
            var attr = wingAssy.GetCustomAttribute<WingAssemblyAttribute>();
            if (attr == null)
                throw new ArgumentException("Assembly is not a wing");
            var result = new ExtensionDescriptor();
            result.Description = attr.Description;
            result.Id = attr.Id.ToString();
            result.Name = attr.Name;
            result.Features = new List<FeatureDescriptor>(wingAssy.ExportedTypes.Select(t => CreateFeatureDesc(t, result)));
            return result;
        }

        private static FeatureDescriptor CreateFeatureDesc(Type type, ExtensionDescriptor extension)
        {
            var result = new FeatureDescriptor();

            result.Name = type.AssemblyQualifiedName;
            var desc = type.GetCustomAttribute<DescriptionAttribute>(true);
            if (desc != null)
            {
                result.Description = desc.Description;
            }
            result.Extension = extension;
            return result;
        }

    }
}
