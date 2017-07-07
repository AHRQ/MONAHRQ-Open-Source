using Monahrq.Sdk.Extensibility.ContentManagement.Records;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace Monahrq.Sdk.Services.Contracts
{
    [Serializable]
    [ImplementPropertyChanged]
    public class DatasetImportEntry :  IDatasetImportEntry
    {
        static DatasetImportEntry _null = new DatasetImportEntry();
        public static IDatasetImportEntry Null
        {
            get
            {
                return _null;
            }
        }

        string InternalFileName { get; set; }
        string InternalTimePeriod { get; set; }
        int InternalRecordID { get; set; }
        string InternalDataType { get; set; }
        public string FileName
        {
            get { return InternalFileName; }
            set
            {
                if (this == Null) return;
                InternalFileName = value;
            }
        }

        public string TimePeriod
        {
            get { return InternalTimePeriod; }
            set
            {
                if (this == Null) return;
                InternalTimePeriod = value;
            }
        }

        public int RecordID
        {
            get { return InternalRecordID; }
            set
            {
                if (this == Null) return;
                InternalRecordID = value;
            }
        }
        public string DataType
        {
            get { return InternalDataType; }
            set
            {
                if (this == Null) return;
                InternalDataType = value;
            }
        }
    }

    public static class DatasetImportEntryExtension
    {
        public static XElement Serialize(this IDatasetImportEntry item)
        {
            var formatter = new JavaScriptSerializer();
            return new XElement("data", Uri.EscapeDataString(formatter.Serialize(item)));
        }

        public static IDatasetImportEntry Deserialize(this ContentItemRecord source)
        {
            var xml = XElement.Parse(source.Data);
            var formatter = new JavaScriptSerializer();
            var result = formatter.Deserialize<DatasetImportEntry>(Uri.UnescapeDataString(xml.Value));
            result.RecordID = source.Id;
            return result;
        }
    }
}
