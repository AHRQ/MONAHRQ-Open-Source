using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Utility;


namespace Monahrq.Infrastructure.Data.Conventions
{

    /// <summary>
    /// Custom FluentNHibernate Conventions for naming table schemas. 
    /// The record Table Name Convention implements a custom naming conventions for the monahrq database schema.
    /// </summary>
    /// <seealso cref="FluentNHibernate.Conventions.IClassConvention" />
    [ConventionExportAttribute]
    public class RecordTableNameConvention : IClassConvention
    {
        /// <summary>
        /// Applies the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void Apply(IClassInstance instance)
        {
            instance.Table(instance.EntityType.EntityTableName());
        }
    }
}