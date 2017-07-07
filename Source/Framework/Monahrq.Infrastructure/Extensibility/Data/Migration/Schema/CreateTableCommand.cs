using System;
using System.Collections.Generic;
using System.Data;
using Monahrq.Infrastructure.Domain.Wings;

namespace Monahrq.Sdk.Extensibility.Data.Migration.Schema {
    public class CreateTableCommand : SchemaCommand {
        public CreateTableCommand(string name)
            : base(name, SchemaCommandType.CreateTable) {
        }

        //public bool TableExists
        //{
        //    get
        //    {
        //        var sql = string.Format("select count(*) from ( "
        //            + " SELECT OBJECT_ID('{0}') col )t ", Name);
        //        return 0 < (int)(new ConfigurationService()).ConnectionSettings.ExecuteScalar(sql);
        //    }
        //}

        public CreateTableCommand Column(string columnName, DbType dbType, Action<CreateColumnCommand> column = null) {
            var command = new CreateColumnCommand(Name, columnName);
            command.WithType(dbType);

            if ( column != null ) {
                column(command);
            }
            TableCommands.Add(command);
            return this;
        }

        public CreateTableCommand Column<T>(string columnName, Action<CreateColumnCommand> column = null) {
            var dbType = SchemaUtils.ToDbType(typeof (T));
            return Column(columnName, dbType, column);
        }

        public CreateTableCommand AddColumns(IEnumerable<DynamicTargetColumn> targetColumns)
        {
            if (targetColumns == null) return this;

            foreach (var column in targetColumns)
            {
                var command = new CreateColumnCommand(Name, column.Name);

                if (column.Scope != DynamicScopeEnum.None)
                {
                    command = command.WithType(column.DataType == DataTypeEnum.String
                                                                ? DbType.String
                                                                : DbType.Int32);

                    if (column.IsRequired)
                    {
                        command = command.DbType == DbType.Int32
                                                  ? command.WithDefault(0)
                                                  : command.WithDefault("Exclude");
                    }
                }
                else
                {
                    command = command.WithType(column.DbType);
                }

                command = column.IsRequired ? command.NotNull() : command.Nullable();

                if (column.DataType == DataTypeEnum.Decimal)
                {
                    command = command.WithScale((column.Scale != -1 && column.Scale != 0) ? Convert.ToByte(column.Scale) : Convert.ToByte(5));
                    command = command.WithPrecision((column.Precision != -1 && column.Precision != 0) ? Convert.ToByte(column.Precision) : Convert.ToByte(19));
                }

                if (column.DataType == DataTypeEnum.String)
                    command = command.WithLength((column.Length != -1 && column.Length != 0) ? column.Length : 255);

                if (column.IsUnique)
                    command = command.Unique();

                TableCommands.Add(command);
            }

            return this;
        }

        /// <summary>
        /// Defines a primary column as for content parts
        /// </summary>
        public CreateTableCommand DatasetRecord()
        {
            Column<int>("Id", column => column.PrimaryKey().Identity());
            return this;
        }

        /// <summary>
        /// Defines a primary column as for versionnable content parts
        /// </summary>
        public CreateTableCommand DatasetVersionRecord()
        {
            Column<int>("Id", column => column.PrimaryKey().Identity());
            Column<int>("Dataset_id");
            return this;
        }

        /// <summary>
        /// Defines a primary column as for data set reference
        /// </summary>
        public CreateTableCommand DatasetReferenceRecord()
        {
            Column<int>("Dataset_id", c => c.NotNull());
            return this;
        }
    }
}
