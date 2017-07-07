using System;
using System.Linq;
using Monahrq.Wing.Ahrq.Model;
using Monahrq.Wing.Ahrq.ViewModels;

namespace Monahrq.Wing.Ahrq.Provider
{
    /// <summary>
    /// Import process class for provider
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
        public override string HeaderLine { get { return "MODULE,INDICATOR NUMBER,HOSPITAL"; } }
        /// <summary>
        /// Gets the header line v6.
        /// </summary>
        /// <value>
        /// The header line v6.
        /// </value>
        public override string HeaderLineV6 { get { return "MODULE,INDICATOR NUMBER,HOSPITAL ID,OBSERVED NUMERATOR,OBSERVED DENOMINATOR,OBSERVED RATE"; } }
        /// <summary>
        /// Gets the name of the import type.
        /// </summary>
        /// <value>
        /// The name of the import type.
        /// </value>
        public override string ImportTypeName { get { return "AHRQ-QI Provider Data"; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportProcessor"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ImportProcessor(WizardContext context)
            : base(context)
        {}

        //protected override bool ValidateFileHeader(string fileHeader, FileProgress fileProgress, out string errorMessage)
        //{
        //    if (!fileHeader.ToUpper().StartsWith(HeaderLineV6))
        //    {
        //        errorMessage = string.Format("The input file [{0}] does not appear to be of the correct file type.", fileProgress.FileName) + "\n" +
        //                                     "If you are trying to import the reports genrated by AHRQ QI v6.0 ICD-10 version, please note that MONAHRQ will not be able to process the file and can't import it at this time.";
        //        return false;
        //    }

        //    return base.ValidateFileHeader(fileHeader, fileProgress, out errorMessage);
        //}

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
        /// </exception>
        public bool LineFunction(string inputLine)
        {
            if (string.IsNullOrEmpty(inputLine)) throw new Exception("Empty line !");

            var cols = inputLine.Split(new[] { "," }, StringSplitOptions.None).ToList();
            var columnCount = cols.Count;
            //var measureCode = (cols[0].Trim() + cols[1].Trim()).Replace(" ", "");

            var measureCodeModule = cols[0].Trim();
            var measureCodeNumb = cols[1].Trim();

            if (!string.IsNullOrEmpty(measureCodeNumb) && measureCodeNumb.Length == 1)
            {
                measureCodeNumb = measureCodeNumb.PadLeft(2, '0');
            }

            var measureCode = string.Format("{0} {1}", measureCodeModule, measureCodeNumb.Trim());

            if (string.IsNullOrEmpty(measureCode))
                throw new Exception("Measure code length can't must be greater than zero");

            // Make sure it's not a total row
            var col2Val = cols[2].Trim();
            var isStratification = col2Val.Equals("TOTAL");
            var providerTarget = new ProviderTarget 
            {
                Dataset = DataContextObject.DatasetItem,
                MeasureCode = measureCode,
                Stratification = isStratification ? "AVG" : string.Empty,
                LocalHospitalID = isStratification ? string.Empty : col2Val,
                ObservedNumerator = GetIntFromString(cols[3]),
                ObservedDenominator = GetIntFromString(cols[4]),
                ObservedRate = GetDecimalFromString(cols[5]),
                ObservedCILow = GetDecimalFromString(columnCount == 13 ? null : cols[6]),
                ObservedCIHigh = GetDecimalFromString(columnCount == 13 ? null : cols[7]),
                RiskAdjustedRate = GetDecimalFromString(columnCount == 13 ? cols[9] : cols[11]),
                RiskAdjustedCILow = GetDecimalFromString(columnCount == 13 ? cols[10] : cols[12]),
                RiskAdjustedCIHigh = GetDecimalFromString(columnCount == 13 ? cols[11] : cols[13]),
                ExpectedRate = GetDecimalFromString(columnCount == 13 ? cols[6] : cols[8])
            };
            // Check for total costs column.
            //int? totalCost = 0;
            //if (columnCount >= 16)
            //{
            //    totalCost = GetIntFromString(cols[15]) ?? 0;
            //}
            //providerTarget.TotalCost = totalCost;

            if (AnyHospital<ProviderTarget>(measureCode, providerTarget.LocalHospitalID ?? "AVG", providerTarget.Dataset.Id)) return false;
            Insert(providerTarget);

            return true;
        }
    }
}

