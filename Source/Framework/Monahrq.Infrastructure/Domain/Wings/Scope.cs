using System;
using System.Collections.Generic;
using Monahrq.Infrastructure.Data.Conventions;

namespace Monahrq.Infrastructure.Entities.Domain.Wings
{
    /// <summary>
    /// Used to persist information about a CLR enum type to the Monahrq database
    /// </summary>
    /// <seealso cref="ScopeValue"/>
    [Serializable, EntityTableName("Wings_Scopes")]
    public partial class Scope
    {
        protected Scope(){}

        public Scope(Target target, string name, Type clrType = null)
            : base(target, name)
        {
            Owner.Scopes.Add(this);
            if (clrType != null)
                ClrType = clrType.AssemblyQualifiedName;
        }

        #region Properties

        /// <summary>
        /// Gets the null.
        /// </summary>
        /// <value>
        /// The null.
        /// </value>
        public static Scope Null
        {
            get
            {
                return new Scope(Target.Null, "<<NULL>>", typeof(DBNull)) { Description = "<<NULL>>" }; 
            }
        }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public virtual IList<ScopeValue> Values { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is custom.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is custom; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsCustom { get; set; }

        /// <summary>
        /// Gets the type of the color.
        /// </summary>
        /// <value>
        /// The type of the color.
        /// </value>
        public virtual string ClrType { get; private set; }
        #endregion

        protected override void Initialize()
        {
            base.Initialize();
            Values = new List<ScopeValue>();
        }
    }
}
