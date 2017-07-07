using System;
namespace Monahrq.Sdk.Services.Contracts
{
    public interface IDatasetImportRecord<T>
    {
        System.Collections.Generic.IEnumerable<T> DataItems { get; set; }
        string Name { get; set; }
        string TimePeriod { get; set; }
    }
}
