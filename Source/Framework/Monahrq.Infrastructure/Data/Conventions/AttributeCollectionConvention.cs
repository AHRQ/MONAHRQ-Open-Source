using System;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;

namespace Monahrq.Infrastructure.Data.Conventions
{
    /// <summary>
    /// The abstract Attribute Collection FluentNhibernate Convention.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="FluentNHibernate.Conventions.ICollectionConvention" />
    /// <seealso cref="FluentNHibernate.Conventions.ICollectionConventionAcceptance" />
    public abstract class AttributeCollectionConvention<T> : ICollectionConvention, ICollectionConventionAcceptance where T : Attribute
    {
        /// <summary>
        /// Accepts the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        public void Accept(IAcceptanceCriteria<ICollectionInspector> criteria)
        {
            criteria.Expect(inspector => GetAttribute(inspector) != null);
        }

        /// <summary>
        /// Applies the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void Apply(ICollectionInstance instance)
        {
            Apply(GetAttribute(instance), instance);
        }

        /// <summary>
        /// Applies the specified attribute.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="instance">The instance.</param>
        protected abstract void Apply(T attribute, ICollectionInstance instance);

        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <param name="inspector">The inspector.</param>
        /// <returns></returns>
        private static T GetAttribute(ICollectionInspector inspector)
        {
            return Attribute.GetCustomAttribute(inspector.Member, typeof(T)) as T;
        }
    }
}