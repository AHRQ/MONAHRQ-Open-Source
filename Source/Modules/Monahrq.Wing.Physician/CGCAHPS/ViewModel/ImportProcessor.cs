using System;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Wing.Physician.CGCAHPS.Model;
using Monahrq.Wing.Physician.CGCAHPS.Views;

namespace Monahrq.Wing.Physician.CGCAHPS.ViewModel
{
	/// <summary>
	/// Handles Importing a CGCAHPS line/record.
	/// </summary>
	/// <seealso cref="Monahrq.Wing.Physician.CGCAHPS.ViewModel.ProcessFileViewModel" />
	/// <seealso cref="Monahrq.Wing.Physician.CGCAHPS.Views.IDataImporter" />
	public class ImportProcessor : ProcessFileViewModel, IDataImporter
    {
		/// <summary>
		/// The header line
		/// </summary>
		public string HeaderLine = "DRG Definition, Provider Id, Provider Name, Provider Street Address, Provider City, Provider State, Provider Zip Code, Hospital Referral Region Description, Total Discharges, Average Covered Charges, Average Total Payments";

		/// <summary>
		/// Initializes a new instance of the <see cref="ImportProcessor" /> class.
		/// </summary>
		/// <param name="context">The context.</param>
		public ImportProcessor(WizardContext context)
            : base(context)
        {
            _contentItemRecord = context.DatasetItem;
        }

		/// <summary>
		/// The content item record
		/// </summary>
		private Dataset _contentItemRecord;

		//private int rowId = 1;

		// This processes a single line for AREA files
		// TODO: since this saves 1 line at a time to SQL, we should set up NH to use batches
		/// <summary>
		/// Lines the function.
		/// </summary>
		/// <param name="inputLine">The input line.</param>
		/// <returns></returns>
		/// <exception cref="Exception">Empty line !</exception>
		public bool LineFunction(string inputLine)
        {
            return false;
            
            if (string.IsNullOrEmpty(inputLine)) throw new Exception("Empty line !");

            // In the main import of the rows.
            string[] cols = inputLine.Split(new[] {","}, StringSplitOptions.None);

            // Process if it's not a blank line.
            var nhId = !cols[0].IsNullOrEmpty() ? cols[0] : null;
            var practiceId = !cols[1].IsNullOrEmpty() ? cols[1] : null;
            
            var nhCAHPSSurveyTarget = new CGCAHPSSurveyTarget
            {
                Dataset = DataContextObject.DatasetItem,
                CGPracticeId = practiceId,
                CGId = nhId,
                //TotalDischarges = GetIntFromString(cols[8]),
                //AverageCoveredCharges = GetDoubleFromString(cols[9]),
                //AverageTotalPayments = GetDoubleFromString(cols[10])
            };

            var dataRow = NHCAHPSSurveyDataTable.NewRow();

            //dataRow[DB_COLUMN_DRG_ID] = int.Parse(stringDrgId);
            //dataRow[DB_COLUMN_DRG] = nhCAHPSSurveyTarget.DRG;
            //dataRow[DB_COLUMN_PROVIDER_ID] = nhCAHPSSurveyTarget.ProviderId;
            //dataRow[DB_COLUMN_TOTAL_DISCHARGES] = nhCAHPSSurveyTarget.TotalDischarges.HasValue 
            //                                                ? (object)nhCAHPSSurveyTarget.TotalDischarges.Value 
            //                                                : DBNull.Value;
            //dataRow[DB_COLUMN_AVERAGE_COVERED_CHARGES] = nhCAHPSSurveyTarget.AverageCoveredCharges.HasValue
            //                                                ? (object)nhCAHPSSurveyTarget.AverageCoveredCharges.Value
            //                                                : DBNull.Value;
            //dataRow[DB_COLUMN_AVERAGE_TOTAL_PAYMENTS] = nhCAHPSSurveyTarget.AverageTotalPayments.HasValue
            //                                                ? (object)nhCAHPSSurveyTarget.AverageTotalPayments.Value
            //                                                : DBNull.Value;
            if (DataContextObject.DatasetItem != null)
                dataRow[DB_COLUMN_DATASET_ID] = DataContextObject.DatasetItem.Id;

            NHCAHPSSurveyDataTable.Rows.Add(dataRow);

            //if (Any<MedicareProviderChargeTarget>(providerId, drg)) return false;

            //Insert(medicareProviderChargeTarget);
            return true;
        }
    }
}
