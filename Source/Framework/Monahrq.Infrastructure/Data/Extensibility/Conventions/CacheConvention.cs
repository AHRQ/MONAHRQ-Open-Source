using FluentNHibernate;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using System.ComponentModel.Composition;
using System.Linq;
using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Data.Extensibility.Conventions
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.Conventions.TypeConvention" />
    /// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.IExtensibilityConvention" />
    /// <seealso cref="FluentNHibernate.Conventions.IConventionAcceptance{FluentNHibernate.Conventions.Inspections.IClassInspector}" />
    [Export(typeof(IExtensibilityConvention))]
    [ConventionExportAttribute]
    public class CacheConvention : TypeConvention
            , IExtensibilityConvention
        , IConventionAcceptance<IClassInspector>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheConvention"/> class.
        /// </summary>
        /// <param name="typeSource">The type source.</param>
        [ImportingConstructor]
        public CacheConvention([Import(RequiredCreationPolicy = CreationPolicy.Shared)]
                ITypeSource typeSource)
            : base(typeSource)
        {

        }

        /// <summary>
        /// Applies the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public override void Apply(IClassInstance instance)
        {
            instance.Cache.ReadWrite();
        }

        /// <summary>
        /// Whether this convention will be applied to the target.
        /// </summary>
        /// <param name="criteria">Instace that could be supplied</param>
        public void Accept(IAcceptanceCriteria<IClassInspector> criteria)
        {
            criteria.Expect(x => PersistedTypes.Any(d => d == x.EntityType));
        }
    }
}
