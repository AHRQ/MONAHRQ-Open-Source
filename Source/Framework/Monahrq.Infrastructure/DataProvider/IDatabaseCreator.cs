using Monahrq.Sdk.Events;
using System;
namespace Monahrq.Sdk.DataProvider
{
    public static class CreatorNames
    {
        public const string Sql = "SQL";
        public const string Monahrq = "MONAHRQ";
    }

    public interface IDatabaseCreator
    {
        void Create(string connectionString,string schemaVersion);
        void Test(string connectionString);
        void Delete(string connectionString);
        void DeleteAndCreate(string connectionString, string schemaVersion);
        void TruncateDbLogFile(string connectionString);

        event EventHandler<ConnectionStringSuccessEventArgs> Created;
        event EventHandler<ConnectionStringFailedEventArgs> CreateFailed;

        event EventHandler<ConnectionStringSuccessEventArgs> Tested;
        event EventHandler<ConnectionStringFailedEventArgs> TestFailed;

        event EventHandler<ConnectionStringSuccessEventArgs> Deleted;
        event EventHandler<ConnectionStringFailedEventArgs> DeleteFailed;

        event EventHandler<ConnectionStringSuccessEventArgs> DeleteAndCreated;
        event EventHandler<ConnectionStringFailedEventArgs> DeleteAndCreatedFailed;

        event EventHandler<ConnectionStringSuccessEventArgs> UpgradeCompleted;
        event EventHandler<ConnectionStringFailedEventArgs> UpgradeCompletedFailed;
    }

    public class DatabaseCreatorException : Exception
    {
        public DatabaseCreatorException(string message)
            : base(message)
        {
        }
    }
}
