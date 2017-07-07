using FluentNHibernate;
using FluentNHibernate.Conventions;
using System;
using System.Collections.Generic;
using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Data.Extensibility.Conventions
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="FluentNHibernate.Conventions.IClassConvention" />
    [ConventionExport]
    public abstract class TypeConvention : IClassConvention
    {

        /// <summary>
        /// Gets or sets the type source.
        /// </summary>
        /// <value>
        /// The type source.
        /// </value>
        ITypeSource TypeSource { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeConvention"/> class.
        /// </summary>
        /// <param name="typeSource">The type source.</param>
        protected TypeConvention(ITypeSource typeSource)
        {
            TypeSource = typeSource;
        }

        /// <summary>
        /// Gets the persisted types.
        /// </summary>
        /// <value>
        /// The persisted types.
        /// </value>
        protected IEnumerable<Type> PersistedTypes
        {
            get
            {
                return TypeSource.GetTypes();
            }
        }

        /// <summary>
        /// Applies the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public abstract void Apply(FluentNHibernate.Conventions.Instances.IClassInstance instance);
    }

}
