using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Extensibility.Data.Migration.Schema;
using Monahrq.Sdk.Extensibility.Data.Providers; 

namespace Monahrq.Sdk.Extensibility.Data.Migration.Generator
{


    using NHibernate.Cfg;
    using NHibernate.Dialect;
    using NHibernate.Mapping;
    using NHibernate.Tool.hbm2ddl; 
    using Monahrq.Sdk.Extensibility.Environment.Descriptor.Models;
    using Monahrq.Sdk.Extensibility.Builders;
    using System.Configuration;
    using Monahrq.Infrastructure.Configuration;

    using Cfg = NHibernate.Cfg.Configuration;
    using Monahrq.Sdk.Extensibility.Utility;

    public class SchemaCommandGenerator : ISchemaCommandGenerator
    {
        private readonly ISessionFactoryHolder _sessionFactoryHolder;
        private readonly IExtensionManager _extensionManager;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly ConnectionStringSettings _connection;
        private readonly IDataServicesProviderFactory _dataServicesProviderFactory;

        public SchemaCommandGenerator(
            ISessionFactoryHolder sessionFactoryHolder,
            IExtensionManager extensionManager,
            ICompositionStrategy compositionStrategy,
            IConfigurationService configService,
            IDataServicesProviderFactory dataServicesProviderFactory)
        {
            _sessionFactoryHolder = sessionFactoryHolder;
            _extensionManager = extensionManager;
            _compositionStrategy = compositionStrategy;
            _connection = configService.ConnectionSettings;
            _dataServicesProviderFactory = dataServicesProviderFactory;
        }

        /// <summary>
        /// Generates SchemaCommand instances in order to create the schema for a specific feature
        /// </summary>
        public IEnumerable<SchemaCommand> GetCreateWingCommands(IEnumerable<Type> wingTypes, bool drop)
        {
            var these = _extensionManager.AvailableFeatures().Select(f => f.Name)
                .Intersect(wingTypes.Select(w => w.AssemblyQualifiedName)).ToList();

            var thesefeatures = _extensionManager.AvailableFeatures().Where(ext => these.Contains(ext.Name));
            if (!thesefeatures.Any())
            {
                yield break;
            }
            var shellFeatures = new[] 
            {
                    new ShellFeature { Name = "Monahrq.Infrastructure" }
            };

            var shellDescriptor = new ShellDescriptor
            {
                Features = shellFeatures
            };


            var shellBlueprint = _compositionStrategy.Compose(_connection, shellDescriptor);

            if (!shellBlueprint.Records.Any())
            {
                yield break;
            }

            //var features = dependencies.Select(name => new ShellFeature {Name = name}).Union(new[] {new ShellFeature {Name = feature}, new ShellFeature {Name = "Orchard.Framework"}});

            var parameters = _sessionFactoryHolder.GetSessionFactoryParameters();
            parameters.RecordDescriptors = shellBlueprint.Records.ToList();

            var configuration = _dataServicesProviderFactory
                .CreateProvider(parameters)
                .BuildConfiguration(parameters);

            Dialect.GetDialect(configuration.Properties);
            var mapping = configuration.BuildMapping();

            // get the table mappings using reflection
            var tablesField = typeof(Cfg).GetField("tables", BindingFlags.Instance | BindingFlags.NonPublic);
            var tables = ((IDictionary<string, Table>)tablesField.GetValue(configuration)).Values;

            foreach (var wingType in thesefeatures.Select(f => Type.GetType(f.Name)))
            {

                string prefix = wingType.EntityTableName() + "_";
         
                foreach (var table in tables.Where(t => parameters.RecordDescriptors.Any(rd => rd.Type.FullName == wingType.FullName && rd.TableName == t.Name)))
                {

                    string tableName = table.Name;
                    var recordType = parameters.RecordDescriptors.First(rd => rd.Feature.Descriptor.Name == wingType.AssemblyQualifiedName && rd.TableName == tableName).Type;
                    var isContentPart = true;

                    if (tableName.StartsWith(prefix))
                    {
                        tableName = tableName.Substring(prefix.Length);
                    }

                    if (drop)
                    {
                        yield return new DropTableCommand(tableName);
                    }

                    var command = new CreateTableCommand(tableName);

                    foreach (var column in table.ColumnIterator)
                    {
                        // create copies for local variables to be evaluated at the time the loop is called, and not lately when the la;bda is executed
                        var tableCopy = table;
                        var columnCopy = column;

                        var sqlType = columnCopy.GetSqlTypeCode(mapping);
                        command.Column(column.Name, sqlType.DbType,
                            action =>
                            {
                                if (tableCopy.PrimaryKey.Columns.Any(c => c.Name == columnCopy.Name))
                                {
                                    action.PrimaryKey();

                                    if (!isContentPart)
                                    {
                                        action.Identity();
                                    }
                                }


                                if (columnCopy.IsLengthDefined()
                                    && new[] { DbType.StringFixedLength, DbType.String, DbType.AnsiString, DbType.AnsiStringFixedLength }.Contains(sqlType.DbType)
                                    && columnCopy.Length != Column.DefaultLength)
                                {
                                    action.WithLength(columnCopy.Length);
                                }

                                if (columnCopy.IsPrecisionDefined())
                                {
                                    action.WithPrecision((byte)columnCopy.Precision);
                                    action.WithScale((byte)columnCopy.Scale);
                                }
                                if (columnCopy.IsNullable)
                                {
                                    action.Nullable();
                                }

                                if (columnCopy.IsUnique)
                                {
                                    action.Unique();
                                }

                                if (columnCopy.DefaultValue != null)
                                {
                                    action.WithDefault(columnCopy.DefaultValue);
                                }
                            });
                    }

                    yield return command;
                }
            }
        }


        /// <summary>
        /// Automatically updates a db to a functionning schema
        /// </summary>
        public void UpdateDatabase()
        {
            var configuration = _sessionFactoryHolder.GetConfiguration();
            new SchemaUpdate(configuration).Execute(false, true);
        }

        /// <summary>
        /// Automatically creates a db with a functionning schema
        /// </summary>
        public void CreateDatabase()
        {
            var configuration = _sessionFactoryHolder.GetConfiguration();
            new SchemaExport(configuration).Execute(false, true, false);
        }

    }
}
