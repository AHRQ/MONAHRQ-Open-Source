using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Wing.Physician.Physicians.Models;
using Monahrq.Wing.Physician.Physicians.Views;

namespace Monahrq.Wing.Physician.Physicians.ViewModels
{
    public class ImportProcessor : ProcessFileViewModel, IDataImporter
    {
        public string HeaderLine = "DRG Definition,Provider Id,Provider Name,Provider Street Address,Provider City,Provider State,Provider Zip Code,Hospital Referral Region Description, Total Discharges , Average Covered Charges , Average Total Payments ";

        public ImportProcessor(WizardContext context)
            : base(context)
        {
            _contentItemRecord = context.DatasetItem;
        }

        private Dataset _contentItemRecord;

        //private int rowId = 1;

        // This processes a single line for AREA files
        // TODO: since this saves 1 line at a time to SQL, we should set up NH to use batches
        public bool LineFunction(string inputLine)
        {
            return true;
        }
    }
}
