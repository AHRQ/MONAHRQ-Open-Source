using Monahrq.Sdk.Extensibility.Data.Migration.Schema;

namespace Monahrq.Sdk.Extensibility.Data.Migration.Interpreters
{
    /// <summary>
    /// This interface can be implemented to provide a data migration behavior
    /// </summary>
    public interface ICommandInterpreter<in T> : ICommandInterpreter
    where T : ISchemaBuilderCommand {
        string[] CreateStatements(T command);
    }

    public interface ICommandInterpreter  {
        string DataProvider { get; }
    }
}
