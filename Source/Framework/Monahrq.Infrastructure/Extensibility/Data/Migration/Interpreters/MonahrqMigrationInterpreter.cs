using System.Data.SqlClient;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Exceptions;
using Monahrq.Sdk.Extensibility.Data.Migration.Schema;
using NHibernate.Dialect;
using NHibernate.SqlTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Sdk.Extensibility.Data.Migration.Interpreters
{

    [Export(typeof(IDataMigrationInterpreter))]
    public class MonahrqMigrationInterpreter: AbstractDataMigrationInterpreter
            , IDataMigrationInterpreter
        , IPartImportsSatisfiedNotification
    {

        private Dialect _dialect;

        [Import(RequiredCreationPolicy=CreationPolicy.Shared)]
        IMonahrqShell Shell {get;set;}

        [Import]
        IConfigurationService ConfigurationService {get;set;}

        [Import(LogNames.Session)]
        ILogWriter Logger {get;set;}    

        public void OnImportsSatisfied()
        {
            _dialect = Dialect.GetDialect(Shell.SessionFactoryHolder.GetConfiguration().Properties);
            SqlStatementList = new List<string>();
        }

        List<string> SqlStatementList {get;set;}
        
        public IEnumerable<string> SqlStatements
        {
            get {return SqlStatementList;}
        }

        public override void Visit(CreateTableCommand command)
        {

            if (ExecuteCustomInterpreter(command))
            {
                return;
            }

            var builder = new StringBuilder();

            builder.Append(_dialect.CreateMultisetTableString)
                .Append(' ')
                .Append(_dialect.QuoteForTableName(command.Name))
                .Append(" (");

            var appendComma = false;
            foreach (var createColumn in command.TableCommands.OfType<CreateColumnCommand>())
            {
                if (appendComma)
                {
                    builder.Append(", ");
                }
                appendComma = true;

                Visit(builder, createColumn);
            }

            var primaryKeys = command.TableCommands.OfType<CreateColumnCommand>().Where(ccc => ccc.IsPrimaryKey).Select(ccc => ccc.ColumnName);
            if (primaryKeys.Any())
            {
                if (appendComma)
                {
                    builder.Append(", ");
                }

                builder.Append(_dialect.PrimaryKeyString)
                    .Append(" ( ")
                    .Append(String.Join(", ", primaryKeys.ToArray()))
                    .Append(" )");
            }

            builder.Append(" )");
            SqlStatementList.Add(builder.ToString());

            RunPendingStatements();
        }

        public override void Visit(DropTableCommand command)
        {
            if (ExecuteCustomInterpreter(command))
            {
                return;
            }

            var builder = new StringBuilder();

            builder.Append(_dialect.GetDropTableString(command.Name));
            SqlStatementList.Add(builder.ToString());

            RunPendingStatements();
        }

        public override void Visit(AlterTableCommand command)
        {
            if (ExecuteCustomInterpreter(command))
            {
                return;
            }

            if (command.TableCommands.Count == 0)
            {
                return;
            }

            // drop columns
            foreach (var dropColumn in command.TableCommands.OfType<DropColumnCommand>())
            {
                var builder = new StringBuilder();
                Visit(builder, dropColumn);
                RunPendingStatements();
            }

            // add columns
            foreach (var addColumn in command.TableCommands.OfType<AddColumnCommand>())
            {
                var builder = new StringBuilder();
                Visit(builder, addColumn);
                RunPendingStatements();
            }

            // alter columns
            foreach (var alterColumn in command.TableCommands.OfType<AlterColumnCommand>())
            {
                var builder = new StringBuilder();
                Visit(builder, alterColumn);
                RunPendingStatements();
            }

            // add index
            foreach (var addIndex in command.TableCommands.OfType<AddIndexCommand>())
            {
                var builder = new StringBuilder();
                Visit(builder, addIndex);
                RunPendingStatements();
            }

            // drop index
            foreach (var dropIndex in command.TableCommands.OfType<DropIndexCommand>())
            {
                var builder = new StringBuilder();
                Visit(builder, dropIndex);
                RunPendingStatements();
            }

        }

        public void Visit(StringBuilder builder, AddColumnCommand command)
        {
            if (ExecuteCustomInterpreter(command))
            {
                return;
            }

            builder.AppendFormat("alter table {0} add ", _dialect.QuoteForTableName(command.TableName));

            Visit(builder, (CreateColumnCommand)command);
            SqlStatementList.Add(builder.ToString());
        }

        public void Visit(StringBuilder builder, DropColumnCommand command)
        {
            if (ExecuteCustomInterpreter(command))
            {
                return;
            }

            builder.AppendFormat("alter table {0} drop column {1}",
                _dialect.QuoteForTableName(command.TableName),
                _dialect.QuoteForColumnName(command.ColumnName));
            SqlStatementList.Add(builder.ToString());
        }

        private const char Space = ' ';

        public void Visit(StringBuilder builder, AlterColumnCommand command)
        {
            if (ExecuteCustomInterpreter(command))
            {
                return;
            }

            builder.AppendFormat("alter table {0} alter column {1} ",
                _dialect.QuoteForTableName(command.TableName),
                _dialect.QuoteForColumnName(command.ColumnName));

            // type
            if (command.DbType != DbType.Object)
            {
                builder.Append(GetTypeName(command.DbType, command.Length, command.Precision, command.Scale));
            }
            else
            {
                if (command.Length > 0 || command.Precision > 0 || command.Scale > 0)
                {
                    throw new MonahrqCoreException("Error while executing data migration: you need to specify the field's type in order to change its properties");
                }
            }

            // [default value]
            if (command.Default != null)
            {
                builder.Append(" set default ").Append(ConvertToSqlValue(command.Default)).Append(Space);
            }
            SqlStatementList.Add(builder.ToString());
        }


        public void Visit(StringBuilder builder, AddIndexCommand command)
        {
            if (ExecuteCustomInterpreter(command))
            {
                return;
            }

            builder.AppendFormat("create index {1} on {0} ({2}) ",
                _dialect.QuoteForTableName(command.TableName),
                _dialect.QuoteForColumnName(command.IndexName),
                String.Join(", ", command.ColumnNames));

            SqlStatementList.Add(builder.ToString());
        }

        public void Visit(StringBuilder builder, DropIndexCommand command)
        {
            if (ExecuteCustomInterpreter(command))
            {
                return;
            }

            builder.AppendFormat("drop index {0} ON {1}",
                _dialect.QuoteForColumnName(command.IndexName),
                _dialect.QuoteForTableName(command.TableName));
            SqlStatementList.Add(builder.ToString());
        }

        public override void Visit(SqlStatementCommand command)
        {
            if (command.Providers.Count != 0 && !command.Providers.Contains(ConfigurationService.ConnectionSettings.ProviderName))
            {
                return;
            }

            if (ExecuteCustomInterpreter(command))
            {
                return;
            }
            SqlStatementList.Add(command.Sql);

            RunPendingStatements();
        }

        public override void Visit(CreateForeignKeyCommand command)
        {
            if (ExecuteCustomInterpreter(command))
            {
                return;
            }

            var builder = new StringBuilder();

            builder.Append("alter table ")
                .Append(_dialect.QuoteForTableName(command.SrcTable));

            builder.Append(_dialect.GetAddForeignKeyConstraintString(command.Name,
                command.SrcColumns,
                _dialect.QuoteForTableName(command.DestTable),
                command.DestColumns,
                false));

            SqlStatementList.Add(builder.ToString());

            RunPendingStatements();
        }

        public override void Visit(DropForeignKeyCommand command)
        {
            if (ExecuteCustomInterpreter(command))
            {
                return;
            }

            var builder = new StringBuilder();

            builder.AppendFormat("alter table {0} drop constraint {1}", _dialect.QuoteForTableName(command.SrcTable), command.Name);
            SqlStatementList.Add(builder.ToString());

            RunPendingStatements();
        }

        private string GetTypeName(DbType dbType, int? length, byte precision, byte scale)
        {

            // NHibernate has a bug in MsSqlCeDialect, as it's declaring the decimal type as this:
            // NUMERIC(19, $1), where $1 is the Length parameter, and it's wrong. It should be 
            // NUMERIC(19, $s) in order to use the Scale parameter, as it's done for SQL Server dialects
            // https://nhibernate.jira.com/browse/NH-2979
            if (_dialect is MsSqlCeDialect
                && dbType == DbType.Decimal
                && scale != 0)
            {
                return _dialect.GetTypeName(new SqlType(dbType), scale, precision, scale);
            }

            return precision > 0
                       ? _dialect.GetTypeName(new SqlType(dbType, precision, scale))
                       : length.HasValue
                             ? _dialect.GetTypeName(new SqlType(dbType, length.Value))
                             : _dialect.GetTypeName(new SqlType(dbType));
        }

        private void Visit(StringBuilder builder, CreateColumnCommand command)
        {
            if (ExecuteCustomInterpreter(command))
            {
                return;
            }

            // name
            builder.Append(_dialect.QuoteForColumnName(command.ColumnName)).Append(Space);

            if (!command.IsIdentity || _dialect.HasDataTypeInIdentityColumn)
            {
                builder.Append(GetTypeName(command.DbType, command.Length, command.Precision, command.Scale));
            }

            // append identity if handled
            if (command.IsIdentity && _dialect.SupportsIdentityColumns)
            {
                builder.Append(Space).Append(_dialect.IdentityColumnString);
            }

            // [default value]
            if (command.Default != null)
            {
                builder.Append(" default ").Append(ConvertToSqlValue(command.Default)).Append(Space);
            }

            // nullable
            builder.Append(command.IsNotNull
                               ? " not null"
                               : !command.IsPrimaryKey && !command.IsUnique
                                     ? _dialect.NullColumnString
                                     : string.Empty);

            // append unique if handled, otherwise at the end of the satement
            if (command.IsUnique && _dialect.SupportsUnique)
            {
                builder.Append(" unique");
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Nothing comes from user input.")]
        private void RunPendingStatements()
        {
            try
            {
                var session = Shell.SessionFactoryHolder.GetSessionFactory().OpenSession();
                var connection = session.Connection;

                foreach (var sqlStatement in SqlStatements.Distinct())
                {
                    Logger.Debug(sqlStatement);
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandTimeout =
                            (int)
                            Math.Round(MonahrqConfiguration.SettingsGroup.MonahrqSettings().LongTimeout.TotalSeconds);
                        command.CommandText = sqlStatement;
                        command.ExecuteNonQuery();
                    }

                    Logger.Information("Data Migration{0}\tExecuting SQL Query: {1}", System.Environment.NewLine,
                                       String.Format("{0}", sqlStatement));
                }
            }
            finally
            {
                SqlStatementList.Clear();
            }
        }

        private bool ExecuteCustomInterpreter<T>(T command) where T : ISchemaBuilderCommand
        {
            var interpreter =  Shell.CommandInterpreters
                .Where(ici => ici.DataProvider == ConfigurationService.ConnectionSettings.ProviderName)
                .OfType<ICommandInterpreter<T>>()
                .FirstOrDefault();

            if (interpreter != null)
            {
                SqlStatementList.AddRange(interpreter.CreateStatements(command));
                RunPendingStatements();
                return true;
            }

            return false;
        }

        private static string ConvertToSqlValue(object value)
        {
            if (value == null)
            {
                return "null";
            }

            TypeCode typeCode = Type.GetTypeCode(value.GetType());
            switch (typeCode)
            {
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.String:
                case TypeCode.Char:
                    return String.Concat("'", Convert.ToString(value).Replace("'", "''"), "'");
                case TypeCode.Boolean:
                    return (bool)value ? "1" : "0";
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return Convert.ToString(value, CultureInfo.InvariantCulture);
                case TypeCode.DateTime:
                    return String.Concat("'", Convert.ToString(value, CultureInfo.InvariantCulture), "'");
            }

            return "null";
        }

    }
}
