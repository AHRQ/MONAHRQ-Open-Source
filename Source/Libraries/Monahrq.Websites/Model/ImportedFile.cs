using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Extensions;
using PropertyChanged;

namespace Monahrq.Websites.Model
{
    public interface IImportedFile
    {
        int Id { get; set; }
        bool IsSelected { get; set; }
        DataSetFile DataSetFile { get; set; }
        ImportedFileRowTypes RowType { get; set; }
        string Label { get; }
    }

    // To display special strings in combobox
    public enum ImportedFileRowTypes
    {
        Normal,
        PleaseSelect,
        NoneImported,
        AllFilesAdded
    }

    [Export]
    [ImplementPropertyChanged]
    public class ImportedFile : IImportedFile
    {
        public ImportedFile(
			int id, bool IsSelected, string name,
			string year, string quarter,
			string versionMonth, string versionYear, 
			ImportedFileRowTypes RowType = ImportedFileRowTypes.Normal, Dataset importedDataset = null)
        {
            this.Id = id;
            DatasetRecord = importedDataset;
            this.IsSelected = IsSelected;
            DataSetFile = new DataSetFile(id, name, year, quarter, versionMonth,versionYear, importedDataset);
            this.RowType = RowType;
        }

        public int Id { get; set; }
        public bool IsSelected { get; set; }
        public DataSetFile DataSetFile { get; set; }
        public ImportedFileRowTypes RowType { get; set; }

        public Dataset DatasetRecord { get; set; }

        public string Label 
        {
            get 
            {
                return	  RowType == ImportedFileRowTypes.NoneImported ? "No files have been imported for this dataset"
						: RowType == ImportedFileRowTypes.PleaseSelect ? "Please select an imported file to add"
						: RowType == ImportedFileRowTypes.AllFilesAdded ? "All imported files have been added"
						: DatasetRecord != null && DatasetRecord.ContentType.Name.EqualsAny(
							"Nursing Home Compare Data",
							"Medicare Provider Charge Data",
							"Hospital Compare Data")  ?
								string.Format("{0}, {1} {2}", DataSetFile.Name, DataSetFile.VersionMonth,DataSetFile.VersionYear)
						: DataSetFile.Name.StartsWithAny(
							"Hospital Compare Data") ?
								string.Format("{0}, {1} {2}", DataSetFile.Name, DataSetFile.VersionMonth, DataSetFile.VersionYear)
						: string.IsNullOrWhiteSpace(DataSetFile.Quarter) ? string.Format("{0}, {1}", DataSetFile.Name, DataSetFile.Year)
						: string.Format("{0}, {1}, {2}", DataSetFile.Name, DataSetFile.Year, DataSetFile.Quarter);
            }
        }
    }
}
