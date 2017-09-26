using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Domain.Websites;
using System.Dynamic;
using Monahrq.Infrastructure;

namespace Monahrq.Sdk.Generators
{

    /// <summary>
    /// The Cost Quality Calculator
    /// </summary>
    public class CostQualityCalculator
	{
        #region Types.
        /// <summary>
        /// internal enumeration designating the Cost Quality specific measure types
        /// </summary>
        public enum CostQualityCalculatorMeasureType
		{
            /// <summary>
            /// The IQI 12
            /// </summary>
            IQI_12,
            /// <summary>
            /// The IQI 14
            /// </summary>
            IQI_14,
		}
        /// <summary>
        /// The Cost Quality Calculation object used in the calculator.
        /// </summary>
        public class CostQualityCalculation
		{
            /// <summary>
            /// Gets or sets the type.
            /// </summary>
            /// <value>
            /// The type.
            /// </value>
            public CostQualityCalculatorMeasureType Type { get; set; }
            /// <summary>
            /// Gets or sets the measure quantity value.
            /// </summary>
            /// <value>
            /// The measure quantity value.
            /// </value>
            public string MeasureQuantityValue { get; set; }
            /// <summary>
            /// Gets or sets the measure average cost value.
            /// </summary>
            /// <value>
            /// The measure average cost value.
            /// </value>
            public string MeasureAverageCostValue { get; set; }
            /// <summary>
            /// Gets or sets the hospital identifier.
            /// </summary>
            /// <value>
            /// The hospital identifier.
            /// </value>
            public int HospitalId { get; set; }
		}
        #endregion

        #region Variables.
        /// <summary>
        /// Gets or sets the mon ahrq connection string.
        /// </summary>
        /// <value>
        /// The mon ahrq connection string.
        /// </value>
        private string MonAhrqConnectionStr { get; set; }
        /// <summary>
        /// Gets or sets the win qi connection string.
        /// </summary>
        /// <value>
        /// The win qi connection string.
        /// </value>
        private string WinQiConnectionStr { get; set; }
        /// <summary>
        /// Gets the current website.
        /// </summary>
        /// <value>
        /// The current website.
        /// </value>
        public Website CurrentWebsite { get; private set; }
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        private ILogWriter Logger { get; set; }

        /// <summary>
        /// The cost quality calculations dictionary
        /// </summary>
        public Dictionary<CostQualityCalculatorMeasureType, Dictionary<int,CostQualityCalculation>> CQCalculations;
        #endregion

        #region Methods.
        #region Constructor Methods.
        /// <summary>
        /// Initializes a new instance of the <see cref="CostQualityCalculator"/> class.
        /// </summary>
        /// <param name="winQiConnectionStr">The win qi connection string.</param>
        /// <param name="monAhrqConnectionStr">The mon ahrq connection string.</param>
        /// <param name="currentWebsite">The current website.</param>
        public CostQualityCalculator(string winQiConnectionStr, string monAhrqConnectionStr, Website currentWebsite)
		{
			WinQiConnectionStr = winQiConnectionStr;
			MonAhrqConnectionStr = monAhrqConnectionStr;
			CurrentWebsite = currentWebsite;
			Logger = ServiceLocator.Current.GetInstance<ILogWriter>();
			CQCalculations = new Dictionary<CostQualityCalculatorMeasureType, Dictionary<int, CostQualityCalculation>>();
		}
        /// <summary>
        /// Calculates this instance.
        /// </summary>
        public void Calculate()
		{
			Calculate(CostQualityCalculatorMeasureType.IQI_12);
			Calculate(CostQualityCalculatorMeasureType.IQI_14);
		}
        /// <summary>
        /// Calculates the specified measure type.
        /// </summary>
        /// <param name="measureType">Type of the measure.</param>
        private void Calculate(CostQualityCalculatorMeasureType measureType)
		{
			//	Query Variables.
			var measureColPrefix = 
				measureType == CostQualityCalculatorMeasureType.IQI_12 ? "IQI12" :
				measureType == CostQualityCalculatorMeasureType.IQI_14 ? "IQI14" : "";
            var sqlWinQICostMappedCheck = @"select count(*) from discharge_main dm where dm.custom1 is not null";
			var sqlWinQIGetCostDataA = string.Format(@"
				select			count(*) Quantity
							,	sum(
									case when (isnumeric(isnull(dm.custom1,'')) = 0)
										then	cast(0 as decimal(19,2))
										else	cast(ltrim(rtrim(replace(isnull(dm.custom1,0), ',', ''))) as decimal(19,2))
								end) Cost_Numerator
							,	sum(
									case when cast(ltrim(rtrim(replace(isnull(dm.custom1,0), ',', ''))) as decimal(19,2)) = 0
										then	0
										else	1
								end) Cost_Denomiator
				from			discharge_main dm
				where			coalesce(dm.{0},-1) in (0,1)
					and			dm.hospid in (@SelectedHospProviderIDs)",
				measureColPrefix);

			var sqlMonGetCostToChargeData = string.Format(@"
				select			ctc.ProviderId
							,	ctc.Ratio
						--	,	ctc.Year
				from			Base_CostToCharges ctc
					inner join	(
									select			ctc.ProviderId
												,	max(ctc.Year) EffectYear
									from			Base_CostToCharges ctc
									where			ctc.ProviderId in (@SelectedHospProviderIDs)
										and			ctc.Year <= @WebsiteYear
									group by		ctc.ProviderId
								) ctcEY
									on	ctcEy.ProviderID = ctc.ProviderID
									and	ctcEy.EffectYear = ctc.Year");

			var sqlWinQIGetCostDataB = string.Format(@"
				select			dm.hospid as Hosp_Local_ID
							,	sum(cast(
									case when (isnumeric(ltrim(rtrim(replace(isnull(dm.custom1,0), ',', '')))) = 0)
										then	cast(0 as decimal(19,2))
										else	ltrim(rtrim(replace(isnull(dm.custom1,0), ',', '')))
									end
									as decimal(19,2))) Hosp_Cost
							,	sum(
									case when cast(ltrim(rtrim(replace(isnull(dm.custom1,0), ',', ''))) as decimal(19,2)) = 0
										then	0
										else	1
								end) Hosp_CostQuantity
							,	count(*) Hosp_Quantity
				from			discharge_main dm
				where			coalesce(dm.{0},-1) in (0,1)
				group by		dm.hospId",
				measureColPrefix);

			//	WinQI Connection
			using (var monAhrqConnection = new SqlConnection(MonAhrqConnectionStr))
			using (var winQiConnection = new SqlConnection(WinQiConnectionStr))
			{
				monAhrqConnection.Open();
				winQiConnection.Open();

				//	Get hospital ids from Monahrq DB.
				var hospCmsProviderIds = CurrentWebsite.Hospitals.Where(h => !h.Hospital.CmsProviderID.IsNullOrEmpty()).Select(h => h.Hospital.CmsProviderID).ToList();
				var hospLocalProviderIds = CurrentWebsite.Hospitals.Where(h => !h.Hospital.CmsProviderID.IsNullOrEmpty()).Select(h => h.Hospital.LocalHospitalId).ToList();

				//	Validate WinQI DB table: check if all null ! Can't Update Cost 
				if ((int)new SqlCommand(sqlWinQICostMappedCheck, winQiConnection).ExecuteScalar() == 0)
					return;

				//	DB Stuff.
				var quantity = 0;
				var totalCost = 0.0;
				var totalCostCount = 0;
				var hospCostToCharges = new Dictionary<string, double>();
				var hospCosts = new List<dynamic>(); //<string, double>();

				sqlWinQIGetCostDataA = sqlWinQIGetCostDataA.Replace("@SelectedHospProviderIDs", String.Join(",", hospLocalProviderIds.Select(id => String.Format("'{0}'",id))));
				sqlMonGetCostToChargeData = sqlMonGetCostToChargeData.Replace("@SelectedHospProviderIDs", String.Join(",", hospCmsProviderIds.Select(id => String.Format("'{0}'", id))));

				//	Retrieve CostToCharage data from Monahrq DB.
				var cmd = new SqlCommand(sqlMonGetCostToChargeData, monAhrqConnection);
				cmd.Parameters.Add("@WebsiteYear", SqlDbType.NVarChar).Value = CurrentWebsite.ReportedYear;
				using (var dr = cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						var hospId = dr.Guard<String>("ProviderId");
						var costToCharge = dr.Guard<double>("Ratio");
						hospCostToCharges[hospId] = costToCharge;
					}
				}

				//	Retrieve Hosp Values from WinQI DB.
				using (var dr = new SqlCommand(sqlWinQIGetCostDataB, winQiConnection).ExecuteReader())
				{
					while (dr.Read())
					{
						dynamic item = new ExpandoObject();
						item.HospLocalId = dr.Guard<String>("Hosp_Local_ID");
						item.Cost = dr.Guard<double>("Hosp_Cost");
						item.Quantity = dr.Guard<double>("Hosp_Quantity");
						item.CostQuantity = dr.Guard<double>("Hosp_CostQuantity");
						var hosp = CurrentWebsite.Hospitals.Where(h => h.Hospital.LocalHospitalId == item.HospLocalId).FirstOrDefault();
						item.CmsProviderId = hosp == null ? null : hosp.Hospital.CmsProviderID;
						item.HospitalId = hosp == null ? -1 : hosp.Hospital.Id;

						if (hosp != null)	hospCosts.Add(item);
					}
				}

				//	Retrieve Total Values from WinQI DB.
				using (var dr = new SqlCommand(sqlWinQIGetCostDataA, winQiConnection).ExecuteReader())
				{
					if (dr.Read())
					{
						quantity = dr.Guard<int>("Quantity");
						totalCost = dr.Guard<double>("Cost_Numerator");
						totalCostCount = dr.Guard<int>("Cost_Denomiator");
					}
				}

				//	Calculate Measure Hospital Values.
				CQCalculations[measureType] = new Dictionary<int, CostQualityCalculation>();

				var totalCostWithCTC = 0.0;
				foreach (var hospCost in hospCosts)
				{
					var hospCostToCharge = hospCostToCharges.GetValueOrDefault((String)hospCost.CmsProviderId, 1.0);
					totalCostWithCTC += hospCost.Cost * hospCostToCharge;


					CQCalculations[measureType][hospCost.HospitalId] = new CostQualityCalculation()
					{
						Type = measureType,
						MeasureQuantityValue = hospCost.Quantity <= 5 ? "c" : hospCost.Quantity.ToString(),
						MeasureAverageCostValue = hospCost.Quantity <= 5 ? "c" : (hospCost.Cost * hospCostToCharge / hospCost.CostQuantity).ToString("F2"),
						HospitalId = hospCost.HospitalId
					};
				}

				//	Calculate Total Measure Values.
				CQCalculations[measureType][0] = new CostQualityCalculation()
				{
					Type = measureType,
					MeasureQuantityValue = quantity <= 5 ? "c" : quantity.ToString(),
					MeasureAverageCostValue = totalCostCount == 0 ? "0" : (totalCostWithCTC / totalCostCount).ToString("C0"),
					HospitalId = 0
				};
			}
		}
		#endregion
		#endregion
	}
}