using System;
using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;

namespace Monahrq.Infrastructure.Data.Conventions
{
    /// <summary>
    /// This attribute is used to mark relationships which need to be eagerly fetched with the parent object,
    /// thus defining an aggregate in terms of DDD
    /// </summary>
    public class AggregateAttribute : Attribute
    {
    }

    /// <summary>
    /// The custom FluentNhibernate ReferenceConvention.
    /// </summary>
    /// <seealso cref="FluentNHibernate.Conventions.IReferenceConvention" />
    /// <seealso cref="FluentNHibernate.Conventions.IReferenceConventionAcceptance" />
    /// <seealso cref="FluentNHibernate.Conventions.IHasManyConvention" />
    /// <seealso cref="FluentNHibernate.Conventions.IHasManyConventionAcceptance" />
    public class ReferenceConvention : IReferenceConvention, IReferenceConventionAcceptance, IHasManyConvention, IHasManyConventionAcceptance
    {
        /// <summary>
        /// Applies the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void Apply(IManyToOneInstance instance)
        {
            instance.Fetch.Join();
        }

        /// <summary>
        /// Accepts the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        public void Accept(IAcceptanceCriteria<IManyToOneInspector> criteria)
        {
            criteria.Expect(x => x.Property != null && x.Property.MemberInfo.GetCustomAttributes(typeof(AggregateAttribute), false).Any());
        }

        /// <summary>
        /// Applies the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void Apply(IOneToManyCollectionInstance instance)
        {
            instance.Fetch.Select();
            instance.Cache.ReadWrite();
        }

        /// <summary>
        /// Accepts the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        public void Accept(IAcceptanceCriteria<IOneToManyCollectionInspector> criteria)
        {
            criteria.Expect(x => x.Member != null && x.Member.IsDefined(typeof(AggregateAttribute), false));
        }
    }
}

