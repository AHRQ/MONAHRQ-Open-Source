using System;
namespace Monahrq.Sdk.Services.Contracts
{
    public interface IDatasetImportEntry
    {
        string DataType { get; }
        string FileName { get; }
        int RecordID { get; set; }
        string TimePeriod { get; }
    }
}
