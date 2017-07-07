namespace Monahrq.Sdk.Extensibility.Data.Migration.Schema {
    public class DropColumnCommand : ColumnCommand {

        public DropColumnCommand(string tableName, string columnName)
            : base(tableName, columnName) {
        }
    }
}
