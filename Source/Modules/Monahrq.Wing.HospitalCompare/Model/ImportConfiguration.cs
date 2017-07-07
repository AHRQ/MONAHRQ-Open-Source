using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Wing.HospitalCompare.Model
{
    public class ImportConfiguration
    {
        /// <summary>
        /// Description of the temporary directory created for this import, if applicable
        /// </summary>
        public DirectoryInfo TemporaryDirectory { get; set; }
        public List<string> SourceTables { get; }= new List<string>();
        public List<KeyValuePair<string, string>> MeasureColumnMappings { get; set; }
        public List<KeyValuePair<string, string>> TableColumnMappings { get; set; }
        public string SchemaVersion { get; set; }
        public string SchemaVersionMonth { get; set; }
        public string SchemaVersionYear { get; set; }
        public DateTime? DatabaseCreationDate { get; set; }
        public ImportType ImportType { get; set; }
        public bool IsCsvImport => this.ImportType == ImportType.CsvDir || this.ImportType == ImportType.ZippedCsvDir;

        public bool IsValidSchemaVersion => !string.IsNullOrWhiteSpace(this.SchemaVersion)
                                            && this.SchemaVersion != "unknown";
    }
}
