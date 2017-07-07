using System;
using System.Globalization;
using System.Text;
using System.Data;
using System.Collections.Generic;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Exceptions;
using NHibernate.Dialect;
using NHibernate.SqlTypes;
using Monahrq.Sdk.Extensibility.Data.Migration.Schema;
using Monahrq.Infrastructure;
using System.ComponentModel.Composition;

namespace Monahrq.Sdk.Extensibility.Data.Migration.Interpreters
{
    [Export(typeof(ICommandInterpreter))]
    public class SqlServerCommandInterpreter :
        ICommandInterpreter<CreateTableCommand>,
        ICommandInterpreter<AlterColumnCommand>,
        ICommandInterpreter<AddIndexCommand>,
        ICommandInterpreter<DropIndexCommand>
    {
        public static string DataProviderString
        {
            get { return "SqlServer"; }
        }

        private readonly Dialect _dialect;

        //private const char SPACE = ' ';

        public SqlServerCommandInterpreter()
        {
            Logger = NullLogger.Instance;

            var configuration = MonahrqNHibernateProvider.Configuration;
            _dialect = Dialect.GetDialect(configuration.Properties);
        }

        public ILogWriter Logger { get; set; }


        public string DataProvider
        {
            get { return DataProviderString; }
        }

        public string[] CreateStatements(AlterColumnCommand command)
        {
            var statements = new List<string>();
            var builder = new StringBuilder();

            builder.AppendFormat("alter table {0} alter column {1} ",
                 _dialect.QuoteForTableName(PrefixTableName(command.TableName)),
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
                    throw new MonahrqCoreException("Error while executing data migration: you need to specify the field's type in order to change it's properies");
                }
            }

            statements.Add(builder.ToString());

            // [default value]
            if (command.Default != null)
            {
                statements.Add(
                    string.Format(" alter table {0} add constraint [D_{1}] default {2} for {3} ",
                            _dialect.QuoteForTableName(PrefixTableName(command.TableName)),
                            Guid.NewGuid().ToString("N").Substring(0, 8),
                            ConvertToSqlValue(command.Default),
                            _dialect.QuoteForColumnName(command.ColumnName)
                    )
                );
            }
            return statements.ToArray();
        }

        public string[] CreateStatements(AddIndexCommand command)
        {
            return new [] {
                 string.Format(" create index {1} on {0}({2}) ",
                     _dialect.QuoteForTableName(PrefixTableName(command.TableName)),
                     _dialect.QuoteForColumnName(command.IndexName),
                     String.Join(", ", command.ColumnNames)
                 )
             };
        }

        public string[] CreateStatements(DropIndexCommand command)
        {
            return new [] {
                 string.Format(" drop index {1} on {0} ",
                     _dialect.QuoteForTableName(PrefixTableName(command.TableName)),
                     _dialect.QuoteForColumnName(command.IndexName)
                 )
             };
        }


        private string PrefixTableName(string tableName)
        {
            //if (string.IsNullOrEmpty(_shellSettings.DataTablePrefix))
            return tableName;
            //return _shellSettings.DataTablePrefix + "_" + tableName;
        }

        private string GetTypeName(DbType dbType, int? length, byte precision, byte scale)
        {
            return precision > 0
                       ? _dialect.GetTypeName(new SqlType(dbType, precision, scale))
                       : length.HasValue
                             ? _dialect.GetTypeName(new SqlType(dbType, length.Value))
                             : _dialect.GetTypeName(new SqlType(dbType));
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

        public string[] CreateStatements(CreateTableCommand command)
        {
            return new [] {
                 string.Format(" create Table index {1} on {0} ",
                     _dialect.QuoteForTableName(PrefixTableName(command.Name)),
                     _dialect.QuoteForColumnName(command.Name)
                 )
             };
        }
    }
}
