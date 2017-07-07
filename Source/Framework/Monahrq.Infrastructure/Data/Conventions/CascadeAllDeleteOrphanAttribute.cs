using System;
using FluentNHibernate.Conventions.Instances;


namespace Monahrq.Infrastructure.Data.Conventions
{

    /// <summary>
    /// The Cascade All Delete Orphan Custom attribute to be utilized in conjunction with the <see cref="CascadeAllDeleteOrphanConvention"/>
    /// for the FluentNHibernate mapping.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    public class CascadeAllDeleteOrphanAttribute : Attribute
    {
    }

    /// <summary>
    /// The Fluent Nhibernate Cascade All Delete Orphan Convention
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Data.Conventions.AttributeCollectionConvention{Monahrq.Infrastructure.Data.Conventions.CascadeAllDeleteOrphanAttribute}" />
    public class CascadeAllDeleteOrphanConvention :
        AttributeCollectionConvention<CascadeAllDeleteOrphanAttribute>
    {

        /// <summary>
        /// Applies the specified attribute.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="instance">The instance.</param>
        protected override void Apply(CascadeAllDeleteOrphanAttribute attribute, ICollectionInstance instance)
        {
            instance.Cascade.AllDeleteOrphan();
        }
    }
}
