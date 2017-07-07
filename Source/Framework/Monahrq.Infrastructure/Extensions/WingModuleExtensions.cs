using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Modules.Wings;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Linq;
using System.Reflection;

namespace Monahrq.Sdk.Extensions
{

    public static class WingHelperExtensions
    {
        public static Wing LoadTargets(this Wing wing, WingModule module, ISession session = null)
        {
            //&& typeof(ContentPartRecord).IsAssignableFrom(t)
            var assy = module.GetType().Assembly;

            if (session == null)
            {
                assy.GetTypes()
                    .Where(t => t.Namespace == module.GetType().Namespace &&
                                t.GetCustomAttribute<WingTargetAttribute>() != null)
                    .Select(t => new {Type = t, Attribute = t.GetCustomAttribute<WingTargetAttribute>()})
                    .ToList()
                    .ForEach(v => v.Attribute
                                   .CreateTarget(wing, v.Attribute.Guid, v.Type, v.Attribute.IsTrendingEnabled,v.Attribute.DisplayOrder)
                                   .LoadElements(v.Type));
            }
            else
            {
                var targetTypeAttributes = assy
                        .GetTypes()
                        .Where(t => t.GetCustomAttribute<WingTargetAttribute>() != null)
                        .Select(t => new { Type = t, Attribute = t.GetCustomAttribute<WingTargetAttribute>() })
                        .ToList();

                foreach (var attribute in targetTypeAttributes)
                {
                    if (session.Query<Target>().Any(t1 => t1.Guid != attribute.Attribute.Guid))
                        continue;

                    attribute.Attribute
                             .CreateTarget(wing, attribute.Attribute.Guid, attribute.Type,attribute.Attribute.IsTrendingEnabled, attribute.Attribute.DisplayOrder)
                             .LoadElements(attribute.Type);
                }
            }
            
            return wing;
        }

        /// <summary>
        /// For every <see cref="WingTargetElementAttribute"/>, create an <see cref="Element"/> and associate it with <paramref name="target"/>
        /// </summary>
        public static void LoadElements(this Target target, Type targetType)
        {
            targetType.GetRuntimeProperties()
                 .Where(t => t.GetCustomAttribute<WingTargetElementAttribute>() != null)
                 .Select(prop => new { Property = prop, Attribute = prop.GetCustomAttribute<WingTargetElementAttribute>() })
                 .ToList()
                 .ForEach(item => item.Attribute.CreateElement(target));
        }

        public static Target LoadScopes(this Target target, WingModule module)
        {
            //var attributed = typeof(Target).Assembly.GetTypes()
            var targetType = Type.GetType(target.ClrType);


            var attributed = targetType.GetProperties()
                                       //.Where(prop => Attribute.IsDefined(prop, typeof(WingScopeAttribute))).ToList();
                                       .Where(t => t.GetCustomAttributes<WingTargetElementAttribute>().Any()).ToList();

            //var namespaced = attributed.Where(t => t.GetCustomAttribute<WingScopeAttribute>(). == module.GetType().Namespace || t.Namespace == typeof(Target).Namespace)
            //                           .ToList();

            ////var namespaced = attributed.Where(t => t.Namespace == module.GetType().Namespace || t.Namespace == typeof(Target).Namespace)
            ////                           .ToList();

            //var enums = namespaced.Where(t => t.IsEnum || (t.IsGenericType && t.GetGenericArguments().First().IsEnum));



            var theScopes = attributed.Where(t => t.PropertyType.IsEnum || (t.PropertyType.IsGenericType && t.PropertyType.GetGenericArguments().First().IsEnum))
                                      .Select(prop => new { Type = prop.PropertyType, Attribute = prop.PropertyType.GetGenericArguments().First().GetCustomAttributes<WingScopeAttribute>().FirstOrDefault() })
                                      .ToList();

            theScopes.ForEach(v => v.Attribute.CreateScope(target, v.Type.IsGenericType ? v.Type.GetGenericArguments().First() : v.Type));

            return target;
        }

        public static Target ReconcileElementScopes(this Target target)
        {
            var targetData = new
            {
                Target = target,
                PropertiesElementsScopes = new {
                    Properties = Type.GetType(target.ClrType).GetProperties()
                    .Where(p => p.GetCustomAttribute<WingTargetElementAttribute>() != null)
                    .ToDictionary(p => p.GetCustomAttribute<WingTargetElementAttribute>().Name),
                    Elements = target.Elements.ToDictionary(e => e.Name),
                }
            };

            var scopes = target.Scopes.DistinctBy(x => x.ClrType).Where(s => s.ClrType != null).Select(s => s).ToList();
            var props = targetData.PropertiesElementsScopes.Properties;
            var elems = targetData.PropertiesElementsScopes.Elements;
            foreach (var e in elems.ToList())
            {
                var prop = props[e.Value.Name];
                var propKey = prop.PropertyType.IsGenericType ?
                    prop.PropertyType.GetGenericArguments()[0]
                    : prop.PropertyType;
                if (scopes.Any(s => s.ClrType.EqualsIgnoreCase(propKey.AssemblyQualifiedName)))
                {
                    e.Value.Scope = scopes.First(x => x.ClrType.EqualsIgnoreCase(propKey.AssemblyQualifiedName));
                }
            }
            return target;
        }

        public static Wing FactoryWing(this WingModule module)
        {
            var attr = module.GetType().GetCustomAttribute<WingModuleAttribute>();
            if (attr == null)
            {
                throw new InvalidWingModuleException(string.Format("{0} does not have proper metadata attribute", module.Description));
            }
            return attr.FactoryWing();
        }

        public static Target LoadTarget(this WingModule module, Target target)
        {
            target.LoadScopes(module).ReconcileElementScopes();
            return target;
        }

    }

}
