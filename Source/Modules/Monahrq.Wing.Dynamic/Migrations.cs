using System.Collections.Generic;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Sdk.Extensibility.Data.Migration;
using System.ComponentModel.Composition;
using System;
using System.Reflection;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Wing.Dynamic
{
    /// <summary>
    /// Class for data migrations
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Extensibility.Data.Migration.DataMigrationImpl" />
    /// <seealso cref="Monahrq.Sdk.Extensibility.Data.Migration.ITargetMigration" />
    [Export(typeof(ITargetMigration))]
    public class Migrations : DataMigrationImpl, ITargetMigration
    {
        /// <summary>
        /// Gets or sets the target names.
        /// </summary>
        /// <value>
        /// The target names.
        /// </value>
        public IEnumerable<string> TargetNames
        {
            get
            {
                return new string[] {};
            }
            set{}
        }

        /// <summary>
        /// Creates the database schema for a <see cref="DynamicTarget"/>
        /// </summary>
        /// <returns></returns>
        public override int Create()
        {
            try
            {
                var dynamicTarget = Target as DynamicTarget;
                if (dynamicTarget == null) return 0;

                // create table for target type
                SchemaBuilder.CreateTable(dynamicTarget.DbSchemaName, table => table
                    .DatasetRecord()
                    .DatasetReferenceRecord()
                    .AddColumns(dynamicTarget.Columns.ToArray())
                );
                // create FK to datasets table
                SchemaBuilder.CreateForeignKey(
                    string.Format("FK_{0}_Datasets", dynamicTarget.DbSchemaName),
                    dynamicTarget.DbSchemaName,
                    new[] { "Dataset_id" },
                    typeof(Dataset).GetCustomAttribute<EntityTableNameAttribute>().TableName,
                    new[] { "Id" });
            }
            catch(Exception exc)
            {
                SessionLogger.Log(exc.GetBaseException().Message, Microsoft.Practices.Prism.Logging.Category.Exception, Microsoft.Practices.Prism.Logging.Priority.High);
                return 0;
            }

            return 1;
        }
    }
}