using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Entities.Domain.Wings;

using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Extensions;
using NHibernate.Linq;

namespace Monahrq.Sdk.Modules.Wings
{
    /// <summary>
    /// The wing module extension methods class
    /// </summary>
    public static class WingModuleExtensions
    {
        private readonly static IDomainSessionFactoryProvider _provider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();

        /// <summary>
        /// To the member.
        /// </summary>
        /// <typeparam name="TMapping">The type of the mapping.</typeparam>
        /// <typeparam name="TReturn">The type of the return.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns></returns>
        static MemberInfo ToMember<TMapping, TReturn>(this Expression<Func<TMapping, TReturn>> propertyExpression)
        {
            return Infrastructure.Utility.Extensions.ReflectionHelper.GetMember(propertyExpression);
        }

        /// <summary>
        /// Targets the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="wing">The wing.</param>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        /// <exception cref="TargetException">Target is not a property</exception>
        /// <exception cref="WingTargetAttributeException"></exception>
        /// <exception cref="WingTargetElementAttributeException"></exception>
        public static Element TargetProperty<T>(this IWingModule wing, Expression<Func<T, object>> expr)
        {
            var property = expr.ToMember() as PropertyInfo;
            if (property == null)
            {
                throw new TargetException("Target is not a property");
            }

            var wingGuid = new Guid(wing.Guid);

            try
            {
                var elementAttr = property.GetCustomAttribute<WingTargetElementAttribute>();
                var targetAttr = property.DeclaringType.GetCustomAttribute<WingTargetAttribute>();

                try
                {
                    using (var session = _provider.SessionFactory.OpenSession())
                    {
                        var elem = (from e in session.Query<Element>()
                                    join t in session.Query<Target>() on e.Owner.Id equals t.Id
                                    join w in session.Query<Wing>() on t.Owner.Id equals w.Id
                                    where e.Name == elementAttr.Name
                                        && t.Name == targetAttr.Name
                                        && w.WingGUID == wingGuid
                                    select e)
                                    .FirstOrDefault();

                        return elem;
                    }
                }
                catch
                {
                    throw new WingTargetAttributeException(property.DeclaringType);
                }
            }
            catch
            {
                throw new WingTargetElementAttributeException(property);
            }
        }

        /// <summary>
        /// Applies the mapping hints.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="fieldNames">The field names.</param>
        public static void ApplyMappingHints(this Element element, params string[] fieldNames)
        {
            if (fieldNames != null && fieldNames.Any())
            {
                ListExtensions.ForEach(fieldNames, fn => fn = fn.ToUpper());
            }

            element.MappingHints = fieldNames;

            using (var sess = _provider.SessionFactory.OpenStatelessSession())
            {
                using (var trans = sess.BeginTransaction())
                {
                    try
                    {
                         sess.Update(element);

                         trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        var excToUse = ex.InnerException ?? ex;
                        var logger = ServiceLocator.Current.GetInstance<ILoggerFacade>(LogNames.Session);
                        if (logger != null)
                        {
                            logger.Log(excToUse.Message, Category.Exception, Priority.High);
                        }
                        throw;
                    }
                }
            }
        }
    }
}
