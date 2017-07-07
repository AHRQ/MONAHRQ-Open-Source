using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using PropertyChanged;

namespace Monahrq.Websites.Model
{
    [ImplementPropertyChanged]
    public class DataSetFile
    {
        public int DatasetId { get; private set; }
        public Dataset Dataset { get; private set; }
        public string Name { get; set; }
        public string Year { get; set; }
		public string Quarter { get; set; }
		public string VersionMonth { get; set; }
		public string VersionYear { get; set; }

        public DataSetFile(
			int id, string name, 
			string year, string quarter, 
			string versionMonth, string versionYear, 
			Dataset dataset = null)
        {
            Dataset = dataset;
            DatasetId = id;
            Name = name;
			Year = year;
			VersionMonth = versionMonth;
			VersionYear = versionYear;
            Quarter = quarter;
        }
    }
}