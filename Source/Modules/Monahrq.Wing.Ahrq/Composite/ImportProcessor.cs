using System;
using Monahrq.Wing.Ahrq.Model;
using Monahrq.Wing.Ahrq.ViewModels;

namespace Monahrq.Wing.Ahrq.Composite
{
    /// <summary>
    /// Import process class for composite.
    /// </summary>
    /// <seealso cref="Monahrq.Wing.Ahrq.ViewModels.ProcessFileViewModel" />
    public class ImportProcessor : ProcessFileViewModel
    {
        /// <summary>
        /// Gets the header line.
        /// </summary>
        /// <value>
        /// The header line.
        /// </value>
        public override string HeaderLine { get { return "COMPOSITE_NAME,|Composite Indicator Name,"; } }
        /// <summary>
        /// Gets the header line v6.
        /// </summary>
        /// <value>
        /// The header line v6.
        /// </value>
        public override string HeaderLineV6 { get { return null; } }
        /// <summary>
        /// Gets the name of the import type.
        /// </summary>
        /// <value>
        /// The name of the import type.
        /// </value>
        public override string ImportTypeName { get { return "AHRQ-QI Composite Data"; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportProcessor"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ImportProcessor(WizardContext context)
            : base(context)
        {
        }

        // This processes a single line for AREA files
        // TODO: since this saves 1 line at a time to SQL, we should set up NH to use batches
        /// <summary>
        /// This processes a single line for AREA files.
        /// </summary>
        /// <param name="inputLine">The input line.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Empty line !
        /// or
        /// Measure code length can't must be greater than zero
        /// or
        /// Unexpected token [TOTAL]
        /// </exception>
        public bool LineFunction(string inputLine)
        {
            if (string.IsNullOrEmpty(inputLine)) throw new Exception("Empty line !");

            var cols = inputLine.Split(new[] {","}, StringSplitOptions.None);

            var measureCode = cols[0].Trim()    // Added to handle changes in measure names.
                .Replace("IQI Cond", "IQI 91")
                .Replace("IQI Proc", "IQI 90")
                .Replace("PSI Comp", "PSI 90"); 
            if (string.IsNullOrEmpty(measureCode)) throw new Exception("Measure code length can't must be greater than zero");

            var stratId = cols[1].Trim();

            if (stratId.Length < 0 || stratId.Equals("TOTAL")) throw new Exception("Unexpected token [TOTAL]");
            var compositeTarget = new CompositeTarget
            {
                Dataset = DataContextObject.DatasetItem,
                MeasureCode = measureCode,
                LocalHospitalID = stratId,
                ObservedRate = GetDecimalFromString(cols[5]),
                RiskAdjustedRate = GetDecimalFromString(cols[2]),
                RiskAdjustedCILow = GetDecimalFromString(cols[6]),
                RiskAdjustedCIHigh = GetDecimalFromString(cols[7]),
                ExpectedRate = GetDecimalFromString(cols[3]),
                StandardErr = GetDecimalFromString(cols[4])
            };

            if (AnyHospital<CompositeTarget>(measureCode, stratId, compositeTarget.Dataset.Id)) return false;

            Insert(compositeTarget);
            return true;
        }
    }
}