using System.Collections.Generic;
namespace Monahrq.Sdk.Services.Contracts
{
    public class DatasetImportRecord<T> : Monahrq.Sdk.Services.Contracts.IDatasetImportRecord<T>
    {
        public virtual string Name { get; set; }
        public virtual string TimePeriod { get; set; }
        public virtual IEnumerable<T> DataItems { get; set; }
    }
}
