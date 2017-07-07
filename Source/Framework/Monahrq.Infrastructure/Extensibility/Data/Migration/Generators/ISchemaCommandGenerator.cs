using System.Collections.Generic;
using Monahrq.Sdk.Extensibility.Data.Migration.Schema;
using System;

namespace Monahrq.Sdk.Extensibility.Data.Migration.Generator
{
    public interface ISchemaCommandGenerator
    {
        /// <summary>
        /// Returns a set of <see cref="SchemaCommand"/> instances to execute in order to create the tables requiered by the specified feature. 
        /// </summary>
        /// <param name="feature">The name of the feature from which the tables need to be created.</param>
        /// <param name="drop">Whether to generate drop commands for the created tables.</param>
        IEnumerable<SchemaCommand> GetCreateWingCommands(IEnumerable<Type> wingTypes, bool drop);
        /// <summary>
        /// Automatically updates the tables in the database.
        /// </summary>
        void UpdateDatabase();

        /// <summary>
        /// Creates the tables in the database.
        /// </summary>
        void CreateDatabase();

    }
}