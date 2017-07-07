using System;
using System.Data;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Wing.MedicareProviderCharge.Models;
using Monahrq.Wing.MedicareProviderCharge.Views;

namespace Monahrq.Wing.MedicareProviderCharge.ViewModels
{
    /// <summary>
    /// Class for import process
    /// </summary>
    /// <seealso cref="Monahrq.Wing.MedicareProviderCharge.ViewModels.ProcessFileViewModel" />
    /// <seealso cref="Monahrq.Wing.MedicareProviderCharge.Views.IDataImporter" />
    public class ImportProcessor : ProcessFileViewModel, IDataImporter
    {
        /// <summary>
        /// The header line
        /// </summary>
        public string HeaderLine = "DRG Definition,Provider Id,Provider Name,Provider Street Address,Provider City,Provider State,Provider Zip Code,Hospital Referral Region Description, Total Discharges , Average Covered Charges , Average Total Payments ";

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportProcessor"/> class.
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
            if (String.IsNullOrEmpty(inputLine)) throw new Exception("Empty line !");

            // In the main import of the rows.
            string[] cols = inputLine.Split(new[] {","}, StringSplitOptions.None);

            

            // Process if it's not a blank line.
            var drg = !cols[0].IsNullOrEmpty() ? cols[0] : null;
            var providerId = !cols[1].IsNullOrEmpty() ? cols[1] : null;
            
            var medicareProviderChargeTarget = new MedicareProviderChargeTarget
            {
                Dataset = DataContextObject.DatasetItem,
                DRG = drg,
                ProviderId = providerId,
                //ProviderStateAbbrev = cols[5],
                TotalDischarges = GetIntFromString(cols[8]),
                AverageCoveredCharges = GetDoubleFromString(cols[9]),
                AverageTotalPayments = GetDoubleFromString(cols[10])
            };

            var dataRow = MedicareProviderDataTable.NewRow();

            var stringDrgId = medicareProviderChargeTarget.DRG.SubStrBefore("-").Trim();
            if (stringDrgId.StartsWith("\""))
            {
                stringDrgId = stringDrgId.Replace("\"", null);
            }

            dataRow[DB_COLUMN_DRG_ID] = int.Parse(stringDrgId);
            dataRow[DB_COLUMN_DRG] = medicareProviderChargeTarget.DRG;
            dataRow[DB_COLUMN_PROVIDER_ID] = medicareProviderChargeTarget.ProviderId;
            //dataRow[DB_COLUMN_PROVIDER_STATE_ABBREV] = medicareProviderChargeTarget.ProviderStateAbbrev;
            dataRow[DB_COLUMN_TOTAL_DISCHARGES] = medicareProviderChargeTarget.TotalDischarges.HasValue 
                                                            ? (object) medicareProviderChargeTarget.TotalDischarges.Value 
                                                            : DBNull.Value;
            dataRow[DB_COLUMN_AVERAGE_COVERED_CHARGES] = medicareProviderChargeTarget.AverageCoveredCharges.HasValue
                                                            ? (object)medicareProviderChargeTarget.AverageCoveredCharges.Value
                                                            : DBNull.Value;
            dataRow[DB_COLUMN_AVERAGE_TOTAL_PAYMENTS] = medicareProviderChargeTarget.AverageTotalPayments.HasValue
                                                            ? (object)medicareProviderChargeTarget.AverageTotalPayments.Value
                                                            : DBNull.Value;
            if (DataContextObject.DatasetItem != null)
                dataRow[DB_COLUMN_DATASET_ID] = DataContextObject.DatasetItem.Id;

            MedicareProviderDataTable.Rows.Add(dataRow);

            //if (Any<MedicareProviderChargeTarget>(providerId, drg)) return false;

            //Insert(medicareProviderChargeTarget);
            return true;
        }
    }
}
