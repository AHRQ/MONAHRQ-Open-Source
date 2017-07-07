using System;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Wing.NursingHomeCompare.Deficiency.Model;
using Monahrq.Wing.NursingHomeCompare.Deficiency.Views;

namespace Monahrq.Wing.NursingHomeCompare.Deficiency.ViewModel
{
	/// <summary>
	/// Imports a NHCompare file.
	/// </summary>
	/// <seealso cref="Monahrq.Wing.NursingHomeCompare.Deficiency.ViewModel.ProcessFileViewModel" />
	/// <seealso cref="Monahrq.Wing.NursingHomeCompare.Deficiency.Views.IDataImporter" />
	public class ImportProcessor : ProcessFileViewModel, IDataImporter
    {
		/// <summary>
		/// The header line
		/// </summary>
		public string HeaderLine = "ProviderID,Deficiency_Care,Deficiency_Facility,Deficiency_Life,IsAbuse,IsNeglect,IsImmediate_Jeopardy";

		/// <summary>
		/// Initializes a new instance of the <see cref="ImportProcessor"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		public ImportProcessor(WizardContext context)
            : base(context)
        {}

		/// <summary>
		/// This processes a single line for AREA files
		/// TODO: since this saves 1 line at a time to SQL, we should set up NH to use batches
		/// </summary>
		/// <param name="inputLine">The input line.</param>
		/// <returns></returns>
		/// <exception cref="Exception">Empty line !</exception>
		public bool LineFunction(string inputLine)
        {
            if (string.IsNullOrEmpty(inputLine)) throw new Exception("Empty line !");

            // In the main import of the rows.
            var cols = inputLine.Split(new[] {","}, StringSplitOptions.None);

            // Process if it's not a blank line.
            var providerId = cols[0];
            
            var nhDeficiencyTarget = new NHDeficiencyTarget
            {
                Dataset = DataContextObject.DatasetItem,
                ProviderId = providerId,
                DeficiencyCare = GetIntFromString(cols[1]),
                DeficiencyFacility = GetIntFromString(cols[2]),
                DeficiencyLife = GetIntFromString(cols[3]),
                IsAbuse = !cols[4].IsNullOrEmpty() ? cols[4] == "1" : (bool?)null,
                IsNeglect = !cols[5].IsNullOrEmpty() ? cols[5] == "1" : (bool?)null,
                IsImmediateJeopardy = !cols[6].IsNullOrEmpty() ? cols[6] == "1" : (bool?)null
            };

            var dataRow = NHDeficiencyDataTable.NewRow();

            dataRow["ProviderId"] = nhDeficiencyTarget.ProviderId;
            dataRow["DeficiencyCare"] = nhDeficiencyTarget.DeficiencyCare;
            dataRow["DeficiencyFacility"] = nhDeficiencyTarget.DeficiencyFacility;
            dataRow["DeficiencyLife"] = nhDeficiencyTarget.DeficiencyLife;
            dataRow["IsAbuse"] = nhDeficiencyTarget.IsAbuse;
            dataRow["IsNeglect"] = nhDeficiencyTarget.IsNeglect;
            dataRow["IsImmediateJeopardy"] = nhDeficiencyTarget.IsImmediateJeopardy;

            if (DataContextObject.DatasetItem != null)
                dataRow["Dataset_Id"] = DataContextObject.DatasetItem.Id;

            NHDeficiencyDataTable.Rows.Add(dataRow);

            //if (Any<MedicareProviderChargeTarget>(providerId, drg)) return false;

            // Insert(medicareProviderChargeTarget);
            return true;
        }
    }
}
