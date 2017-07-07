using System;

namespace Monahrq.Infrastructure.Data.Conventions
{
    /// <summary>
    /// The custom monahrrq entity table name attribute. Used to designate to 
    /// FluentNHibernate to create a custom table names on Database / table schema creation.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false,  Inherited = false) ]
    public class EntityTableNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTableNameAttribute"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public EntityTableNameAttribute(string tableName)
        {
            TableName = tableName;
        }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; private set; }
    }
}
