using System;
using System.Linq;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Wing.Ahrq.Model;
using Monahrq.Wing.Ahrq.ViewModels;

namespace Monahrq.Wing.Ahrq.Area
{
    /// <summary>
    /// Class for import functionality.
    /// </summary>
    /// <seealso cref="Monahrq.Wing.Ahrq.ViewModels.ProcessFileViewModel" />
    public class ImportProcessor : ProcessFileViewModel
    {
        public override string HeaderLine { get { return "MODULE,INDICATOR NUMBER,COUNTY"; } }
        public override string HeaderLineV6 {  get { return "MODULE,INDICATOR NUMBER,COUNTY,OBSERVED NUMERATOR,OBSERVED DENOMINATOR,OBSERVED RATE"; } }
        public override string ImportTypeName { get { return "AHRQ-QI Area Data"; } }

        public ImportProcessor(WizardContext context)
            : base(context)
        {}

        //protected override bool ValidateFileHeader(string fileHeader, FileProgress fileProgress, out string errorMessage)
        //{
        //    if (HeaderLineV6.ToUpper().EqualsIgnoreCase(fileHeader.Trim().ToUpper()))
        //    {
        //        errorMessage = string.Format("The input file [{0}] does not appear to be of the correct file type.", fileProgress.FileName) + "\n\n" +
        //                                     "If you are trying to import the reports genrated by AHRQ QI v6.0 ICD-10 version, please note that MONAHRQ will not be able to process the file and can't import it at this time.";
        //        return false;
        //    }

        //    return base.ValidateFileHeader(fileHeader, fileProgress, out errorMessage);
        //}

        // This processes a single line for AREA files
        // TODO: since this saves 1 line at a time to SQL, we should set up NH to use batches
        /// <summary>
        /// Lines the function.
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

            // In the main import of the rows.
            var cols = Enumerable.ToList(inputLine.Split(new[] { "," }, StringSplitOptions.None));
            var columnCount = cols.Count;

            // Process if it's not a blank line.
            var measureCodeModule = cols[0].Trim();
            var measureCodeNumb = cols[1].Trim();

            if (!string.IsNullOrEmpty(measureCodeNumb) && measureCodeNumb.Length == 1)
            {
                measureCodeNumb = measureCodeNumb.PadLeft(2, '0');
            }

            var measureCode = string.Format("{0} {1}", measureCodeModule, measureCodeNumb.Trim());
            if (measureCode.Length == 0) throw new Exception("Measure code length can't must be greater than zero");
            // Make sure it's not a total row

            var col2Val = cols[2].Trim();
            var isStratification = col2Val.Equals("TOTAL");

            var areaTarget = new AreaTarget
            {
                Dataset = DataContextObject.DatasetItem,
                MeasureCode = measureCode,
                Stratification = isStratification ? "AVG" : string.Empty,
                CountyFIPS = isStratification ? string.Empty : col2Val,
                ObservedNumerator = GetIntFromString(cols[3]),
                ObservedDenominator = GetIntFromString(cols[4]),
                ObservedRate = GetDecimalFromString(cols[5]),
                ObservedCILow = GetDecimalFromString(columnCount == 15 ? cols[6] : null),
                ObservedCIHigh = GetDecimalFromString(columnCount == 15 ? cols[7] : null),
                ExpectedRate = GetDecimalFromString(columnCount == 13 ? cols[6] : cols[8]),
                RiskAdjustedRate = GetDecimalFromString(columnCount == 13 ? cols[9] : cols[11]),
                RiskAdjustedCILow = GetDecimalFromString(columnCount == 13 ? cols[10] : cols[12]),
                RiskAdjustedCIHigh = GetDecimalFromString(columnCount == 13 ? cols[11] : cols[13])
            };

            // Check for total costs column.
            //decimal? totalCost = 0;
            //if (columnCount >= 16)
            //{
            //    totalCost = GetDecimalFromString(cols[15]) ?? 0M;
            //}
            //areaTarget.TotalCost = totalCost == 0M ? null : totalCost;

            if (AnyCounty<AreaTarget>(measureCode, areaTarget.CountyFIPS ?? "AVG", areaTarget.Dataset.Id)) return false;

            Insert(areaTarget);
            return true;
        }
    }
}