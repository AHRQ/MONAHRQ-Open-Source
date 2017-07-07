using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Websites.Model
{
    public class DatasetSummary
    {
        // DatasetId is for identifying the dataset saved with the website...
        public int DatasetId { get; private set; }
        // DatasetTypeId is for matching the imported datasets to the right dropdown list in the UI...
        public int DatasetTypeId { get; private set; }
        public Dataset DatasetImport { get; private set; }

        public string Dataset { get; private set; }
        public string Name { get; private set; }
        public int? Year { get; private set; }
		public string Quarter { get; private set; }
		public string VersionMonth { get; private set; }
		public string VersionYear { get; private set; }

        public DatasetSummary(Dataset item, Target type)
        {
            DatasetId = item.Id;
            DatasetTypeId = type.Id;
            Dataset = type.Name;
            DatasetImport = item;
            Name = item.File;
            //Year = null;
            //Quarter = string.Empty;
            Year = !string.IsNullOrEmpty(item.ReportingYear) ? int.Parse(item.ReportingYear) : (int?)null;
			Quarter = !string.IsNullOrEmpty(item.ReportingQuarter) ? item.ReportingQuarter : string.Empty;
			VersionMonth = !string.IsNullOrEmpty(item.VersionMonth) ? item.VersionMonth : string.Empty;
			VersionYear = !string.IsNullOrEmpty(item.VersionYear) ? item.VersionYear : string.Empty;
        }
    }
}
