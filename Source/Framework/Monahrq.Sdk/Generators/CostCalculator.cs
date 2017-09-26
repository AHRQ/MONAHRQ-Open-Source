using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Sdk.Generators
{
    /// <summary>
    /// The Cost Calcular class. This makes calls to the WinQI "discharge_main" table schema, if WinQI is installed and the WinQI 
    /// connection string is set up correctly in Monahrq. Runs calculations against the "discharge_main" table schema" to calculate the 
    /// Costs for Counties during the generation of the avoidable stays reports.
    /// </summary>
    public class CostCalculator
    {
        private readonly string _monAhrqConnectionStr;
        private readonly string _winQiConnectionStr;
        private int _datasetRecord;
        readonly List<string> _measures = new List<string>();
        readonly List<string> _counties = new List<string>();

        private readonly ILogWriter _logger;

        //TODO: Dictionary key should be string for local hospital ID
        readonly Dictionary<string, decimal> _costToChargeRatio = new Dictionary<string, decimal>();
        //Calculated total cost and discharges with cost for each QI and County;
        /// <summary>
        /// The measure county cost dictionary
        /// </summary>
        public Dictionary<string, Dictionary<string, decimal>> MeasureCountyCostDictionary = new Dictionary<string, Dictionary<string, decimal>>();
        /// <summary>
        /// The measure county cost dinom dictionary
        /// </summary>
        public Dictionary<string, Dictionary<string, int>> MeasureCountyCostDinomDictionary = new Dictionary<string, Dictionary<string, int>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CostCalculator"/> class.
        /// </summary>
        /// <param name="winQiConnectionStr">The win qi connection string.</param>
        /// <param name="monAhrqConnectionStr">The mon ahrq connection string.</param>
        /// <param name="costToChargeRatios">The cost to charge ratios.</param>
        /// <param name="datasetRecord">The dataset record.</param>
        public CostCalculator(string winQiConnectionStr, string monAhrqConnectionStr, Dictionary<string, decimal> costToChargeRatios, int datasetRecord)
        {
            _winQiConnectionStr = winQiConnectionStr;
            _monAhrqConnectionStr = monAhrqConnectionStr;
            _costToChargeRatio = costToChargeRatios;
            _datasetRecord = datasetRecord;

            _logger = ServiceLocator.Current.GetInstance<ILogWriter>();
        }

        /// <summary>
        /// Zips the code.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        private static string ZipCode(int v)
        {
            try
            {
                if (v < 0 || v > 99999) return "*****";
                var s = v.ToString(CultureInfo.InvariantCulture);
                s = "00000" + s;
                s = s.Substring(s.Length - 5);
                return s;
            }
            catch
            {
                return "*****";
            }

        }

        /// <summary>
        /// Loads the mon ahrq data.
        /// </summary>
        /// <returns></returns>
        public CostCalculator LoadMonAhrqData()
        {

            _measures.Clear();
            _counties.Clear();
            //_costToChargeRatio.Clear();

            #region Inline SQL

            const string sqlGetAreaMeasures =
                @"SELECT DISTINCT Replace(MeasureCode, N'  ', N' ') 'MeasureCode' FROM Targets_AhrqTargets where TargetType='area'";
            const string sqlGetStratIds =
                @"SELECT DISTINCT CountyFIPS FROM Targets_AhrqTargets where TargetType='area'";

            // TODO: Add where local hospital id is not null?

            string sqlHospAndCostCharge = @"
                WITH LastProviderChargeYear (ProviderID,LastYear) AS (
                    SELECT ProviderID, MAX([Year])
                    FROM dbo.Base_CostToCharges
                    GROUP BY ProviderID
                )
                SELECT  Hosp.LocalHospitalId, C2C.Ratio
                FROM dbo.Base_CostToCharges AS C2C
                    INNER JOIN dbo.Hospitals Hosp ON Hosp.CmsProviderID = C2C.ProviderID
                    INNER JOIN LastProviderChargeYear LPC on LPC.ProviderID = C2C.ProviderID AND LPC.LastYear = C2C.[Year]
                WHERE LocalHospitalID IS NOT NULL AND Hosp.IsArchived=0 AND Hosp.IsDeleted=0;
                ";
            #endregion

            using (var monAhrqConnection = new SqlConnection(_monAhrqConnectionStr))
            {
                monAhrqConnection.Open();
                using (var reader = new SqlCommand(sqlGetAreaMeasures, monAhrqConnection)
                    .ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _measures.Add(reader.GetString(0));
                    }
                }

                using (var reader = new SqlCommand(sqlGetStratIds, monAhrqConnection)
                   .ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _counties.Add(reader.GetString(0));
                    }
                }

                if (_costToChargeRatio == null || !_costToChargeRatio.Any())
                {
                    using (var reader = new SqlCommand(sqlHospAndCostCharge, monAhrqConnection).ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string hospID = reader.GetValue(0).ToString();

                            //if (int.TryParse(reader.GetValue(0).ToString(), out hospID))
                            {
                                decimal costToCharge;
                                if (decimal.TryParse(reader.GetValue(1).ToString(), out costToCharge))
                                {
                                    _costToChargeRatio[hospID] = costToCharge;
                                }
                            }
                            //_costToChargeRatio[reader.GetInt32(0)] =  reader.GetDouble(1);
                        }
                    }
                }
            }

            return this;

        }
        /// <summary>
        /// Calculates the cost.
        /// </summary>
        /// <returns></returns>
        public CostCalculator CalculateCost()
        {
            var monAhrqConnection=new SqlConnection(_monAhrqConnectionStr);
            monAhrqConnection.Open();
            const string sqlCostMappedCheck = @"SELECT COUNT (custom1) FROM [dbo].[discharge_main] where custom1 is not null";

            // we're using dynamic sql because we don't know whether the indicator exists; this tests for the presence of the 
            // indicator column and returns no records if the column is missing.
            const string sqlMeasureCost = @"DECLARE @Statement AS NVARCHAR(500)
SET @Statement = '
SELECT	PSTCO
	,	HOSPID
	,	SUM(
			CASE 
				WHEN CUSTOM1 IS NULL OR CUSTOM1 = '''' OR ISNUMERIC(CUSTOM1) = 0 THEN 0 
				ELSE CAST(LTRIM(RTRIM(REPLACE(CUSTOM1, '','', ''''))) AS Decimal(19,2)) 
			END
		) AS TOTCHG
	,	SUM(
			CASE 
				WHEN CUSTOM1 IS NULL OR CUSTOM1 = '''' OR ISNUMERIC(CUSTOM1) = 0 THEN 0 
				ELSE 1 
			END
		) AS N
FROM discharge_main
'

IF COL_LENGTH(N'discharge_main', @Indicator) = 1
	SET @Statement = @Statement + 'WHERE ' + @Indicator + '=1'
ELSE
	SET @Statement = @Statement + 'WHERE 2=1'

SET @Statement = @Statement + '
GROUP BY PSTCO, HOSPID 
ORDER BY PSTCO, HOSPID'

PRINT @Statement
EXEC (@Statement)";

            using (var winQiConnection = new SqlConnection(_winQiConnectionStr))
            {
                winQiConnection.Open();

                using (var winQiCommand = new SqlCommand(sqlCostMappedCheck, winQiConnection))
                {
                    winQiCommand.CommandTimeout = 50000;
                    if ((int)winQiCommand.ExecuteScalar() == 0) // all null ! Can't Update Cost 
                        return this;
                }

                using (var measureCostCmd = new SqlCommand(sqlMeasureCost, winQiConnection))
                {
                    var indicatorParameter = measureCostCmd.Parameters.Add("@Indicator", SqlDbType.NVarChar);
                    var test = 0;
                    test = test++;
                    foreach (var m in _measures)
                    {
                        try
                        {
                            var meas = m;
                            var countyCost = new Dictionary<string, decimal>();
                            var countyCostDenom = new Dictionary<string, int>();

                            // get measure code in WinQI column format
                            if (string.IsNullOrEmpty(meas))
                                continue;
                            meas = meas.Replace("  ", " ");
                            var measureCodes = meas.Split(new[] {@" "}, StringSplitOptions.RemoveEmptyEntries).ToList();
                            if (measureCodes.Count == 1)
                                continue;
                            if (measureCodes[1].Length <= 2 && measureCodes[1].StartsWith("0"))
                                meas = $"{measureCodes[0]}{measureCodes[1].SubStrAfterLast("0")}";
                            else
                                meas = meas.Replace(" ", null);

                            indicatorParameter.Value = meas;
                            using (var dr = measureCostCmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    if (dr.IsDBNull(0) || dr.IsDBNull(1) || dr.IsDBNull(2))
                                        continue;
                                    
                                    var countyZip = ZipCode(dr.GetInt32(0));
                                    var hospid = dr.GetString(1);

                                    int nhospid = int.TryParse(hospid, out nhospid) ? nhospid : -1;

                                    object hospCmsProviderID = null;
                                    //todo get cms from hospid
                                    using (var cmd = new SqlCommand(
                                        string.Format(
                                            "select cmsproviderid from hospitals where LocalHospitalId='{0}' and [IsArchived]=0 and [IsDeleted]=0 ",
                                            hospid), 
                                        monAhrqConnection))
                                    {
                                        cmd.CommandTimeout = 50000;
                                        hospCmsProviderID = cmd.ExecuteScalar();
                                    }

                                    var cmsID = hospCmsProviderID?.ToString() ?? "";
                                    // TODO: Errors here
                                    //if (_costToChargeRatio.ContainsKey(nhospid))
                                    if (_costToChargeRatio.ContainsKey(cmsID))
                                    {
                                        countyCost[countyZip] = countyCost.ContainsKey(countyZip)
                                            ? countyCost[countyZip] + dr.GetDecimal(2) * _costToChargeRatio[cmsID]
                                            //nhospid
                                            : dr.GetDecimal(2) * _costToChargeRatio[cmsID]; //nhospid
                                    }
                                    countyCostDenom[countyZip] = countyCostDenom.ContainsKey(countyZip)
                                        ? countyCostDenom[countyZip] + (dr.IsDBNull(3) ? 0 : dr.GetInt32(3))
                                        : dr.IsDBNull(3) ? 0 : dr.GetInt32(3);
                                }
                            }

                            MeasureCountyCostDictionary[m] = countyCost;
                            MeasureCountyCostDinomDictionary[m] = countyCostDenom;
                        }
                        catch (Exception exception)
                        {
                            _logger.Write(exception, "Error calculating cost for Measure Code \"{0}\"", m);
                        }
                    }
                }
            }
            monAhrqConnection.Close();
            return this;
        }

        /// <summary>
        /// Updates the cost into the Targets_AhrqTargets table (Area QI data rows) in the Monahrq database..
        /// </summary>
        public void UpdateCost()
        {
            var sqlGetMeasures = @"SELECT MeasureCode, CountyFIPS, ISNULL(ObservedNumerator,0) AS ObservedNumerator FROM Targets_AhrqTargets where [TargetType]='area' and LTRIM(RTRIM(CountyFIPS)) <> '' and Dataset_Id=" + _datasetRecord.ToString();
            var sqlUpdateTargetsWithCost = @"UPDATE Targets_AhrqTargets SET TotalCost = {0}  WHERE MeasureCode = '{1}' AND  CountyFIPS='{2}' AND [TargetType]='area' and Dataset_Id=" + _datasetRecord.ToString();
            
                var batchUpdate = new StringBuilder();
            using (var monAhrqConnection = new SqlConnection(_monAhrqConnectionStr))
            {
                monAhrqConnection.Open();

                using (var cmd = new SqlCommand(sqlGetMeasures, monAhrqConnection))
                {
                    cmd.CommandTimeout = 50000;
                    using (var reader = cmd.ExecuteReader())
                    {
                    while (reader.Read())
                    {
                        var measureCode = reader.GetString(0);
                        var stratId = reader.GetString(1);
                        var observedNumerator = !reader.IsDBNull(2) ? reader.GetInt32(2) : 0;

                        var totalCost = GetCost(measureCode, stratId, observedNumerator);
                        if (totalCost > 0)
                                batchUpdate.AppendFormat("; " + sqlUpdateTargetsWithCost, totalCost, measureCode,
                                                         stratId);
                        }
                    }
                    }
                if (batchUpdate.Length == 0) return;
                
                using (var cmd2 = new SqlCommand(batchUpdate.ToString(), monAhrqConnection))
                {
                    cmd2.CommandTimeout = 50000;
                    cmd2.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Calculates the cost the cost.
        /// </summary>
        /// <param name="qi">The qi.</param>
        /// <param name="stratId">The strat identifier.</param>
        /// <param name="observedNumerator">The observed numerator.</param>
        /// <returns></returns>
        private decimal GetCost(string qi, string stratId, int observedNumerator)
        {
            if (!MeasureCountyCostDictionary.ContainsKey(qi))
                return new decimal(0.0);

            var countyCost = MeasureCountyCostDictionary[qi];
            var countyCostDenom = MeasureCountyCostDinomDictionary[qi];

            if (!countyCostDenom.ContainsKey(stratId) ||
                !countyCost.ContainsKey(stratId))
                return new decimal(0.0);

            var n = countyCostDenom[stratId]; // charge 
            var cost = countyCost[stratId];

            if (n == 0) return new decimal(0.0);

            var mcost = cost / n;
            mcost = Math.Round(mcost, 2, MidpointRounding.AwayFromZero);
            return (mcost * observedNumerator);
        }
    }
}