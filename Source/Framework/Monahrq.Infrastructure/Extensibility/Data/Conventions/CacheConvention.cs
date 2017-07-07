using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using Monahrq.Sdk.Extensibility.Builders.Models;


namespace Monahrq.Sdk.Extensibility.Data.Conventions
{
    public class CacheConvention : IClassConvention, IConventionAcceptance<IClassInspector>
    {
        private readonly IEnumerable<RecordBlueprint> _descriptors;

        public CacheConvention(IEnumerable<RecordBlueprint> descriptors)
        {
            _descriptors = descriptors;
        }

        public void Apply(IClassInstance instance)
        {
            instance.Cache.ReadWrite();
        }

        public void Accept(IAcceptanceCriteria<IClassInspector> criteria)
        {
            criteria.Expect(x => _descriptors.Any(d => d.Type.Name == x.EntityType.Name));
        }
    }
}