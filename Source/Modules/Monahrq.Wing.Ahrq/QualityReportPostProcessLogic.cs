using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Linq;

namespace Monahrq.Wing.Ahrq
{
    /// <summary>
    ///  Class for perfoming actions after the quality report process is completed.
    /// </summary>
    class QualityReportPostProcessLogic
	{

        #region Enums.
        /// <summary>
        /// Represents the column shown in output.  (Output Table)
        /// </summary>
        private enum EDisplayColumn
		{
			//	Denominator & Numerator.
			ObservedDenominator,
			ObservedNumerator,
			//	Rates.
			RiskAdjustedRate,
			x100_Minus_RiskAdjustedRate,
			ExpectedRate,
			ObservedRate,
			ObservedLowerCI,
			ObservedUpperCI,
			RiskAdjustedLowerCI,
			RiskAdjustedUpperCI,
			RateAndCI,						// Default
			//	Threshold
			Threshold,
			//	Misc
			NatRating,
			NatFilled,
			PeerRating,
			PeerFilled,
		}
		/// <summary>
		/// Represents column in 'Temp_Quality' table (Input Table).
		/// </summary>
		private enum ETempQualityColumn
		{
			Col1,
			Col2,
			Col3,
			Col4,
			Col5,
			Col6,
			Col7,
			Col8,
			Col9,
			Col10,

			RateAndCI,
			NatRating,
			NatFilled,
			PeerRating,
			PeerFilled,
		}
        /// <summary>
        /// Enum for measure type
        /// </summary>
        private enum EMeasureType
		{
			// EMeasureSource = CMS
			Binary,
			Outcome,
			Process,
			Ratio,
			Categorical,
			Scale,
			Structural,
			YNM,
			// EMeasureSource = AHRQ
			QIarea,
			QIcomposite,
			QIprovider,
			QIvolume,
			QIprovider_MCMC,	// ?? = QIarea,

			Other,
		}
        /// <summary>
        /// Enum for measure sources
        /// </summary>
        private enum EMeasureSource
		{
			CMS,
			AHRQ,
			Other,
		}

        /// <summary>
        /// Enum for field format
        /// </summary>
        private enum EMeasureFieldFormat
		{
			None,				//	Plain/Unformatted.
			Percent,
			Ratio,				//	Treat Ratio as unformatted for now.
			Volume,				//	Treat Volume as unformatted for now.
			Rate,
			Scale,
			Minutes,
		}
        #endregion

        #region Settings Class.
        /// <summary>
        /// Quality group settings class.
        /// </summary>
        private class TempQualityGroupSettings
		{
            /// <summary>
            /// Initializes a new instance of the <see cref="TempQualityGroupSettings"/> class.
            /// </summary>
            public TempQualityGroupSettings()
			{
				SkipDenominatorSuppression = false;
				ColumnMap = new Dictionary<EDisplayColumn, ColumnSettings>();
			}
            /// <summary>
            /// Gets or sets a value indicating whether [skip denominator suppression].
            /// </summary>
            /// <value>
            /// <c>true</c> if [skip denominator suppression]; otherwise, <c>false</c>.
            /// </value>
            public bool SkipDenominatorSuppression { get; set; }
            /// <summary>
            /// Gets or sets the column map.
            /// </summary>
            /// <value>
            /// The column map.
            /// </value>
            public IDictionary<EDisplayColumn, ColumnSettings> ColumnMap { get; set; }
		}
        /// <summary>
        /// Column setting
        /// </summary>
        private class ColumnSettings
		{
           
            public ColumnSettings(ETempQualityColumn target)
			{
				TargetColumn = target;
				SuppressionEvaluationColumn = target;
				Format = EMeasureFieldFormat.None;
			}
			public ColumnSettings(ETempQualityColumn target,ETempQualityColumn evaluation)
			{
				TargetColumn = target;
				SuppressionEvaluationColumn = evaluation;
				Format = EMeasureFieldFormat.None;
			}
			public ColumnSettings(ETempQualityColumn target, EMeasureFieldFormat format)
			{
				TargetColumn = target;
				SuppressionEvaluationColumn = target;
				Format = format;
			}
			public ColumnSettings(ETempQualityColumn target, ETempQualityColumn evaluation, EMeasureFieldFormat format)
			{
				TargetColumn = target;
				SuppressionEvaluationColumn = evaluation;
				Format = format;
			}
			/// <summary>
			/// The column that will be 'acted' on: suppressed, formatted, etc...
			/// </summary>
			public ETempQualityColumn TargetColumn { get; set; }
			/// <summary>
			/// The column that should be evaluated when determining suppression.
			/// </summary>
			public ETempQualityColumn SuppressionEvaluationColumn { get; set; }
			/// <summary>
			/// The format of the field.
			/// </summary>
			public EMeasureFieldFormat Format { get; set; }
		}
        #endregion

        #region Settings Methods.
        #region General Setting Methods.
        /// <summary>
        /// Gets the type of the measure.
        /// </summary>
        /// <param name="websiteMeasure">The website measure.</param>
        /// <returns></returns>
        private static EMeasureType GetMeasureType(WebsiteMeasure websiteMeasure)
		{
			var measureType = EMeasureType.Other;
			if (!Enum.TryParse<EMeasureType>(websiteMeasure.ReportMeasure.MeasureType, true, out measureType))
				return EMeasureType.Other;


			if (measureType == EMeasureType.QIprovider &&
				websiteMeasure.ReportMeasure.RiskAdjustedMethod.Equals("mcmc", StringComparison.CurrentCultureIgnoreCase))
				measureType = EMeasureType.QIprovider_MCMC;

			return measureType;
		}
        /// <summary>
        /// Gets the measure source.
        /// </summary>
        /// <param name="websiteMeasure">The website measure.</param>
        /// <returns></returns>
        private static EMeasureSource GetMeasureSource(WebsiteMeasure websiteMeasure)
		{
			var measureSource = EMeasureSource.Other;
			if (!Enum.TryParse<EMeasureSource>(websiteMeasure.ReportMeasure.Source, true, out measureSource))
				return EMeasureSource.Other;

			return measureSource;
		}
        /// <summary>
        /// Gets the name of the measure.
        /// </summary>
        /// <param name="websiteMeasure">The website measure.</param>
        /// <returns></returns>
        private static String GetMeasureName(WebsiteMeasure websiteMeasure)
		{
			return websiteMeasure.ReportMeasure.Name;
		}
        #endregion

        #region Column Setting Methods.
        /// <summary>
        /// Gets the type of the column settings by.
        /// </summary>
        /// <param name="mSource">The m source.</param>
        /// <param name="mType">Type of the m.</param>
        /// <param name="measureName">Name of the measure.</param>
        /// <returns></returns>
        private static IDictionary<EDisplayColumn, ColumnSettings> GetColumnSettingsByType(EMeasureSource mSource, EMeasureType mType, String measureName)
		{
			var colMap = new Dictionary<EDisplayColumn, ColumnSettings>();

			//	Common mappings.
			colMap[EDisplayColumn.RateAndCI] = new ColumnSettings(ETempQualityColumn.RateAndCI);
			//colMap[EDisplayColumn.NatRating] = new SuppressionTarget(ETempQualityColumn.NatRating);
			//colMap[EDisplayColumn.NatFilled] = new SuppressionTarget(ETempQualityColumn.NatFilled);
			//colMap[EDisplayColumn.PeerRating] = new SuppressionTarget(ETempQualityColumn.PeerRating);
			//colMap[EDisplayColumn.PeerFilled] = new SuppressionTarget(ETempQualityColumn.PeerFilled);
			

			//	Measure Type specific mappings.
			switch (mSource)
			{
				case EMeasureSource.CMS:
					switch (mType)
					{
						case EMeasureType.Outcome:
							colMap[EDisplayColumn.ObservedDenominator] = new ColumnSettings(ETempQualityColumn.Col1);
							colMap[EDisplayColumn.RiskAdjustedRate] = new ColumnSettings(ETempQualityColumn.Col2);
							colMap[EDisplayColumn.RiskAdjustedLowerCI] = new ColumnSettings(ETempQualityColumn.Col3);
							colMap[EDisplayColumn.RiskAdjustedUpperCI] = new ColumnSettings(ETempQualityColumn.Col4);
							colMap[EDisplayColumn.RateAndCI].SuppressionEvaluationColumn = colMap[EDisplayColumn.ObservedDenominator].TargetColumn;
							colMap[EDisplayColumn.RateAndCI].Format = EMeasureFieldFormat.Rate;
							break;
						case EMeasureType.Process:
							colMap[EDisplayColumn.ObservedDenominator] = new ColumnSettings(ETempQualityColumn.Col1);
							colMap[EDisplayColumn.RiskAdjustedRate] = new ColumnSettings(ETempQualityColumn.Col2);
							colMap[EDisplayColumn.RateAndCI].Format = EMeasureFieldFormat.None;
							break;
						case EMeasureType.Ratio:
							colMap[EDisplayColumn.RiskAdjustedRate] = new ColumnSettings(ETempQualityColumn.Col1);
							colMap[EDisplayColumn.RiskAdjustedLowerCI] = new ColumnSettings(ETempQualityColumn.Col2);
							colMap[EDisplayColumn.RiskAdjustedUpperCI] = new ColumnSettings(ETempQualityColumn.Col3);
							colMap[EDisplayColumn.RateAndCI].SuppressionEvaluationColumn = colMap[EDisplayColumn.RiskAdjustedRate].TargetColumn;
							colMap[EDisplayColumn.RateAndCI].Format = EMeasureFieldFormat.Ratio;
							break;
						case EMeasureType.Categorical:
							colMap[EDisplayColumn.ObservedDenominator] = new ColumnSettings(ETempQualityColumn.Col5);
							colMap[EDisplayColumn.RiskAdjustedRate] = new ColumnSettings(ETempQualityColumn.Col1);
							colMap[EDisplayColumn.ExpectedRate] = new ColumnSettings(ETempQualityColumn.Col2);
							colMap[EDisplayColumn.ObservedRate] = new ColumnSettings(ETempQualityColumn.Col3);
							colMap[EDisplayColumn.Threshold] = new ColumnSettings(ETempQualityColumn.Col4);
							colMap[EDisplayColumn.RateAndCI].Format = EMeasureFieldFormat.Rate;
							break;
						case EMeasureType.Scale:
							colMap[EDisplayColumn.ObservedDenominator] = new ColumnSettings(ETempQualityColumn.Col5);
							colMap[EDisplayColumn.RiskAdjustedRate] = new ColumnSettings(ETempQualityColumn.Col1);
							colMap[EDisplayColumn.ExpectedRate] = new ColumnSettings(ETempQualityColumn.Col2);
							colMap[EDisplayColumn.ObservedRate] = new ColumnSettings(ETempQualityColumn.Col3);
							colMap[EDisplayColumn.Threshold] = new ColumnSettings(ETempQualityColumn.Col4);
							colMap[EDisplayColumn.RateAndCI].Format = EMeasureFieldFormat.Scale;
							break;
						case EMeasureType.YNM:
							colMap[EDisplayColumn.ObservedDenominator] = new ColumnSettings(ETempQualityColumn.Col5);
							colMap[EDisplayColumn.RiskAdjustedRate] = new ColumnSettings(ETempQualityColumn.Col1);
							colMap[EDisplayColumn.ExpectedRate] = new ColumnSettings(ETempQualityColumn.Col2);
							colMap[EDisplayColumn.ObservedRate] = new ColumnSettings(ETempQualityColumn.Col3);
							colMap[EDisplayColumn.Threshold] = new ColumnSettings(ETempQualityColumn.Col4);
							colMap[EDisplayColumn.RateAndCI].Format = EMeasureFieldFormat.Rate;
							break;
						case EMeasureType.Binary:
							colMap[EDisplayColumn.ObservedDenominator] = new ColumnSettings(ETempQualityColumn.Col4);
							colMap[EDisplayColumn.RiskAdjustedRate] = new ColumnSettings(ETempQualityColumn.Col1);
							colMap[EDisplayColumn.x100_Minus_RiskAdjustedRate] = new ColumnSettings(ETempQualityColumn.Col2);
							colMap[EDisplayColumn.Threshold] = new ColumnSettings(ETempQualityColumn.Col3);
							colMap[EDisplayColumn.RateAndCI].Format = EMeasureFieldFormat.Rate;
							break;
						default:
							break;
					}
					break;
				case EMeasureSource.AHRQ:
					switch (mType)
					{
						case EMeasureType.QIarea:
							//colMap[EDisplayColumn.RateAndCI].Format = EMeasureFieldFormat.Rate;
							break;
						case EMeasureType.QIcomposite:
							colMap[EDisplayColumn.RiskAdjustedRate] = new ColumnSettings(ETempQualityColumn.Col1);
							colMap[EDisplayColumn.RiskAdjustedLowerCI] = new ColumnSettings(ETempQualityColumn.Col2);
							colMap[EDisplayColumn.RiskAdjustedUpperCI] = new ColumnSettings(ETempQualityColumn.Col3);
							colMap[EDisplayColumn.RateAndCI].SuppressionEvaluationColumn = colMap[EDisplayColumn.RiskAdjustedRate].TargetColumn;
							colMap[EDisplayColumn.RateAndCI].Format = EMeasureFieldFormat.Ratio;
							break;
						case EMeasureType.QIprovider:
							colMap[EDisplayColumn.ObservedNumerator] = new ColumnSettings(ETempQualityColumn.Col1);
							colMap[EDisplayColumn.ObservedDenominator] = new ColumnSettings(ETempQualityColumn.Col2);
							colMap[EDisplayColumn.ObservedRate] = new ColumnSettings(ETempQualityColumn.Col3);
							colMap[EDisplayColumn.ObservedLowerCI] = new ColumnSettings(ETempQualityColumn.Col4);
							colMap[EDisplayColumn.ObservedUpperCI] = new ColumnSettings(ETempQualityColumn.Col5);
							colMap[EDisplayColumn.RateAndCI].SuppressionEvaluationColumn = colMap[EDisplayColumn.ObservedRate].TargetColumn;
							colMap[EDisplayColumn.RateAndCI].Format = EMeasureFieldFormat.Rate;
							break;
						case EMeasureType.QIprovider_MCMC:
							colMap[EDisplayColumn.ObservedNumerator] = new ColumnSettings(ETempQualityColumn.Col1);
							colMap[EDisplayColumn.ObservedDenominator] = new ColumnSettings(ETempQualityColumn.Col2);
							colMap[EDisplayColumn.ObservedRate] = new ColumnSettings(ETempQualityColumn.Col3);
							colMap[EDisplayColumn.ObservedLowerCI] = new ColumnSettings(ETempQualityColumn.Col4);
							colMap[EDisplayColumn.ObservedUpperCI] = new ColumnSettings(ETempQualityColumn.Col5);
							colMap[EDisplayColumn.ExpectedRate] = new ColumnSettings(ETempQualityColumn.Col6);
							colMap[EDisplayColumn.RiskAdjustedRate] = new ColumnSettings(ETempQualityColumn.Col7);
							colMap[EDisplayColumn.RiskAdjustedLowerCI] = new ColumnSettings(ETempQualityColumn.Col8);
							colMap[EDisplayColumn.RiskAdjustedUpperCI] = new ColumnSettings(ETempQualityColumn.Col9);
							colMap[EDisplayColumn.RateAndCI].SuppressionEvaluationColumn = colMap[EDisplayColumn.RiskAdjustedRate].TargetColumn;
							colMap[EDisplayColumn.RateAndCI].Format = EMeasureFieldFormat.Rate;
							break;
						case EMeasureType.QIvolume:
							colMap[EDisplayColumn.ObservedNumerator] = new ColumnSettings(ETempQualityColumn.Col1);
							colMap[EDisplayColumn.RateAndCI].Format = EMeasureFieldFormat.Volume;
							break;
						default:
							break;
					}
					break;
				default:
					break;
			}

			//	Format
			var format = GetColumnSettingsFormat(measureName);
			if (format != EMeasureFieldFormat.None)
				colMap[EDisplayColumn.RateAndCI].Format = format;
			return colMap;
		}



        /// <summary>
        /// Gets the column settings format.
        /// </summary>
        /// <param name="measureName">Name of the measure.</param>
        /// <returns></returns>
        private static EMeasureFieldFormat GetColumnSettingsFormat(String measureName)
		{
			switch (measureName)
			{
				//	TopicCategory == "Childbirth" && UsedForInfographics
				case "PC-01": return EMeasureFieldFormat.Percent;

				//	TopicCategory == "Emergency Department" && UsedForInfographics
				case "OP-18b": return EMeasureFieldFormat.Minutes;
				case "OP-20": return EMeasureFieldFormat.Minutes;
				case "OP-21": return EMeasureFieldFormat.Minutes;

				//	TopicCategory == "COPD" && UsedForInfographics
				case "MORT-30-COPD": return EMeasureFieldFormat.Percent;
				case "READM-30-COPD": return EMeasureFieldFormat.Percent;

				//	TopicCategory == "Heart attack and chest pain" && UsedForInfographics
				case "READM-30-AMI": return EMeasureFieldFormat.Percent;
				case "MORT-30-AMI": return EMeasureFieldFormat.Percent;
				case "OP-2": return EMeasureFieldFormat.Percent;
				case "OP-3b": return EMeasureFieldFormat.Minutes;
				case "OP-4": return EMeasureFieldFormat.Percent;
				case "OP-5": return EMeasureFieldFormat.Minutes;

				//	TopicCategory == "Heart failure" && UsedForInfographics
				case "MORT-30-HF": return EMeasureFieldFormat.Percent;
				case "READM-30-HF": return EMeasureFieldFormat.Percent;
				case "HF-1": return EMeasureFieldFormat.Percent;
				case "HF-2": return EMeasureFieldFormat.Percent;
				case "HF-3": return EMeasureFieldFormat.Percent;
				case "IQI 16": return EMeasureFieldFormat.Percent;

				//	TopicCategory == "Heart surgeries and procedures" && UsedForInfographics
				case "IQI 12": return EMeasureFieldFormat.None;


				//	TopicCategory == "Hip or knee replacement surgery" && UsedForInfographics
				case "READM-30-HIP-KNEE": return EMeasureFieldFormat.Percent;
				case "COMP-HIP-KNEE": return EMeasureFieldFormat.Percent;

				//	TopicCategory == "Imaging" && UsedForInfographics
				case "OP-8": return EMeasureFieldFormat.Percent;
				case "OP-10": return EMeasureFieldFormat.Percent;
				case "OP-11": return EMeasureFieldFormat.Percent;
				case "OP-13": return EMeasureFieldFormat.Percent;
				case "OP-14": return EMeasureFieldFormat.Percent;

				//	TopicCategory == "Infections acquired in the hospital" && UsedForInfographics
				case "HAI-2": return EMeasureFieldFormat.Percent;
				case "HAI-5": return EMeasureFieldFormat.Percent;
				case "HAI-6": return EMeasureFieldFormat.Percent;
				case "IMM-2": return EMeasureFieldFormat.Percent;
				case "HAI-3": return EMeasureFieldFormat.Percent;
				case "HAI-4": return EMeasureFieldFormat.Percent;

				//	TopicCategory == "Other surgeries" && UsedForInfographics
				case "IQI 08": return EMeasureFieldFormat.Percent;
				case "IQI 09": return EMeasureFieldFormat.Percent;
				case "IQI 11": return EMeasureFieldFormat.Percent;

				//	TopicCategory == "Patient safety" && UsedForInfographics
				case "PSI 02": return EMeasureFieldFormat.Percent;
				case "PSI 06": return EMeasureFieldFormat.Percent;
				case "PSI 07": return EMeasureFieldFormat.Percent;
				case "PSI 16": return EMeasureFieldFormat.Percent;
				case "VTE-6": return EMeasureFieldFormat.Percent;

				//	TopicCategory == "Patient survey results" && UsedForInfographics
				case "H-CLEAN-HSP": return EMeasureFieldFormat.Percent;
				case "H-COMP-1": return EMeasureFieldFormat.Percent;
				case "H-COMP-2": return EMeasureFieldFormat.Percent;
				case "H-COMP-3": return EMeasureFieldFormat.Percent;
				case "H-COMP-4": return EMeasureFieldFormat.Percent;
				case "H-COMP-5": return EMeasureFieldFormat.Percent;
				case "H-COMP-6": return EMeasureFieldFormat.Percent;
				case "H-HSP-RATING": return EMeasureFieldFormat.Percent;
				case "H-QUIET-HSP": return EMeasureFieldFormat.Percent;
				case "H-RECMND": return EMeasureFieldFormat.Percent;
				case "H-COMP-7-SA": return EMeasureFieldFormat.Percent;

				//	TopicCategory == "Pneumonia" && UsedForInfographics
				case "IQI 20": return EMeasureFieldFormat.Percent;

				//	TopicCategory == "Stroke" && UsedForInfographics
				case "IQI 17": return EMeasureFieldFormat.Percent;

				//	TopicCategory == "Combined Quality and Safety Ratings" && UsedForInfographics
				case "IQI 90": return EMeasureFieldFormat.None;
				case "IQI 91": return EMeasureFieldFormat.None;
				case "PSI 90": return EMeasureFieldFormat.None;

				//	TopicCategory == "Surgical patient safety" && UsedForInfographics
				case "PSI 05": return EMeasureFieldFormat.None;
				case "PSI 15": return EMeasureFieldFormat.None;

				//	TopicCategory == "Surgical patient safety" 
				case "OP-6": return EMeasureFieldFormat.Percent;
				case "OP-7": return EMeasureFieldFormat.Percent;
				case "OP-22": return EMeasureFieldFormat.Percent;

                case "ED-1b": return EMeasureFieldFormat.Minutes;
                case "ED-2b": return EMeasureFieldFormat.Minutes;
			}
			return EMeasureFieldFormat.None;
		}

        /// <summary>
        /// Gets the temporary quality group settings.
        /// </summary>
        /// <param name="mSource">The m source.</param>
        /// <param name="mType">Type of the m.</param>
        /// <param name="measureName">Name of the measure.</param>
        /// <returns></returns>
        private static TempQualityGroupSettings GetTempQualityGroupSettings(EMeasureSource mSource, EMeasureType mType, String measureName)
		{
			var settings = new TempQualityGroupSettings();

			settings.ColumnMap = GetColumnSettingsByType(mSource, mType, measureName);
			settings.SkipDenominatorSuppression = mType == EMeasureType.QIvolume;

			return settings;
		}
        #endregion
        #endregion



        /// <summary>
        /// Does the post process temporary quality.
        /// </summary>
        /// <param name="tempQualitys">The temporary qualitys.</param>
        /// <param name="currentWebsiteMeasure">The current website measure.</param>
        private static void DoPostProcess_TempQuality(List<TempQuality> tempQualitys, WebsiteMeasure currentWebsiteMeasure)
		{
			//	Variables.
			var denominatorSuppresionThreshold = currentWebsiteMeasure.ReportMeasure.SuppressionDenominator.HasValue ? (double)currentWebsiteMeasure.ReportMeasure.SuppressionDenominator.Value : 0;
			var numeratorSuppresionThreshold = currentWebsiteMeasure.ReportMeasure.SuppressionNumerator.HasValue ? (double)currentWebsiteMeasure.ReportMeasure.SuppressionNumerator.Value : 0;
			var isMarginSuppression = currentWebsiteMeasure.ReportMeasure.PerformMarginSuppression;


			//	Validate objects.
			if (currentWebsiteMeasure == null) return;
			var measureType = GetMeasureType(currentWebsiteMeasure);
			var measureSource = GetMeasureSource(currentWebsiteMeasure);
			var measureName = GetMeasureName(currentWebsiteMeasure);
			if (measureType == EMeasureType.Other) return;
			if (measureSource == EMeasureSource.Other) return;


			//	Get Settings.
			var tqgSettings = GetTempQualityGroupSettings(measureSource, measureType, measureName);

			//	Suppress Process.
			DoSuppressProcess(
				tempQualitys,
				tqgSettings,
				denominatorSuppresionThreshold,
				numeratorSuppresionThreshold,
				isMarginSuppression,
				measureType,
				measureSource);

			//	Format Process.
			DoFormattingProcess(
				tempQualitys,
				tqgSettings,
				measureType,
				measureSource,
				currentWebsiteMeasure.ReportMeasure.ScaleBy.HasValue
					? (double)currentWebsiteMeasure.ReportMeasure.ScaleBy.Value
					: 1.0);
		}

        /// <summary>
        /// Does the post process temporary quality measure.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="reportID">The report identifier.</param>
        /// <param name="measureNames">The measure names.</param>
        internal static void DoPostProcess_TempQualityMeasure(NHibernate.ISession session,string reportID, List<string> measureNames)
		{
			var getScript = @"
				select			m.Name as MeasureName
							,	tqm.PeerRateAndCI as PeerRateAndCI
				from			Temp_Quality_Measures tqm
					inner join	Measures m on m.ID = tqm.MeasureID
				where			m.Name in (:MeasureNames)
					and			tqm.ReportID = :ReportID";

			var updateScript = @"
				update			tqm
				set				tqm.PeerRateAndCI =
									case
										@@SetScript@@
										else tqm.PeerRateAndCI
									end
				from			Temp_Quality_Measures tqm
					inner join	Measures m on m.ID = tqm.MeasureID
				where			m.Name in (:MeasureNames)
					and			tqm.ReportID = :ReportID";

			var setScript = "";
			var hasUpdates = false;

			var measureRates =
				session.CreateSQLQuery(getScript)
					.AddScalar("MeasureName", NHibernate.NHibernateUtil.String)
					.AddScalar("PeerRateAndCI", NHibernate.NHibernateUtil.String)
					.SetParameter("ReportID",reportID)
					.SetParameterList("MeasureNames", measureNames)
					.List<Object[]>()
					.Select(row => new
					{
						MeasureName = row[0] == null ? "" : row[0].ToString(),
						PeerRateAndCI = row[1] == null ? "" : row[1].ToString(),
					});

			foreach (var measureRate in measureRates)
			{
				var format = GetColumnSettingsFormat(measureRate.MeasureName);
				var formattedValue = GetFormattedValue(measureRate.PeerRateAndCI, format);

				if (formattedValue != null)
				{
					hasUpdates = true;
					setScript += String.Format("\n			when m.Name = '{0}' then '{1}'",measureRate.MeasureName,formattedValue);
				}
			}
			if (hasUpdates)
			{
				updateScript = updateScript.Replace("@@SetScript@@", setScript);
				session.CreateSQLQuery(updateScript)
					.SetParameter("ReportID", reportID)
					.SetParameterList("MeasureNames", measureNames)
					.ExecuteUpdate();
			}
		}

		#region Suppress Methods.
		#region Get Input Value Methods.
		/// <summary>
		/// Gets all Suppression input/source values for current TempQuality row.
		/// </summary>
		/// <param name="tempQuality"></param>
		/// <param name="suppressionDataMap"></param>
		/// <param name="denominator"></param>
		/// <param name="numerator"></param>
		/// <param name="riskAdjustedRate"></param>
		/// <param name="riskAdjustedRateLow"></param>
		/// <param name="riskAdjustedRateHigh"></param>
		/// <param name="expectedRate"></param>
		/// <param name="observedRate"></param>
		/// <returns></returns>
		private static bool GetSuppressionInputValues(
			TempQuality tempQuality,
			IDictionary<EDisplayColumn, ColumnSettings> suppressionDataMap,
			out double? denominator,
			out double? numerator,
			out double? riskAdjustedRate,
			out double? riskAdjustedRateLow,
			out double? riskAdjustedRateHigh,
			out double? expectedRate,
			out double? observedRate)
		{
			denominator = null;
			numerator = null;
			riskAdjustedRate = null;
			riskAdjustedRateLow = null;
			riskAdjustedRateHigh = null;
			expectedRate = null;
			observedRate = null;

			denominator = GetSuppressionInputValue(tempQuality, suppressionDataMap, EDisplayColumn.ObservedDenominator);
			riskAdjustedRate = GetSuppressionInputValue(tempQuality, suppressionDataMap, EDisplayColumn.RiskAdjustedRate);
			numerator = GetSuppressionInputValue(tempQuality, suppressionDataMap, EDisplayColumn.ObservedNumerator, (denominator != null && denominator != -1 && riskAdjustedRate != null) ? denominator * riskAdjustedRate / 100.0 : null);
			riskAdjustedRateHigh = GetSuppressionInputValue(tempQuality, suppressionDataMap, EDisplayColumn.RiskAdjustedUpperCI);
			riskAdjustedRateLow = GetSuppressionInputValue(tempQuality, suppressionDataMap, EDisplayColumn.RiskAdjustedLowerCI);
			expectedRate = GetSuppressionInputValue(tempQuality, suppressionDataMap, EDisplayColumn.ExpectedRate);
			observedRate = GetSuppressionInputValue(tempQuality, suppressionDataMap, EDisplayColumn.ObservedRate);
			return true;
		}

		/// <summary>
		/// Returns the suppression input/source value that should be used/evaluated for the given 'suppressionColumn'.
		/// - Suppression not happening yet.  This is only returning a value that should be used for evaluation; which may
		///   eventually lead to the column being suppressed.
		/// </summary>
		/// <param name="tempQuality">The TempQuality row being suppressed.</param>
		/// <param name="suppressionDataMap">Contains a map of where each TempQuality row will look</param>
		/// <param name="suppressionColumn">The Column in the TempQuality row that is being suppressed.</param>
		/// <param name="defaultValue">Default suppression input value.</param>
		/// <returns></returns>
		private static double? GetSuppressionInputValue(
			TempQuality tempQuality,
			IDictionary<EDisplayColumn, ColumnSettings> suppressionDataMap,
			EDisplayColumn suppressionColumn,
			double? defaultValue = null)
		{
			double? value = defaultValue;
			if (suppressionDataMap.ContainsKey(suppressionColumn))
			{
				var num = ParseDouble(tempQuality, suppressionDataMap[suppressionColumn].TargetColumn);
				value = num ?? value;
			}
			return value;
		}
        #endregion

        #region Process Methods.
        /// <summary>
        /// Does the suppress process.
        /// </summary>
        /// <param name="tempQualitys">The temporary qualitys.</param>
        /// <param name="tqgSettings">The TQG settings.</param>
        /// <param name="denominatorSuppresionThreshold">The denominator suppresion threshold.</param>
        /// <param name="numeratorSuppresionThreshold">The numerator suppresion threshold.</param>
        /// <param name="isMarginSuppression">if set to <c>true</c> [is margin suppression].</param>
        /// <param name="measureType">Type of the measure.</param>
        /// <param name="measureSource">The measure source.</param>
        private static void DoSuppressProcess(
			List<TempQuality> tempQualitys,
			TempQualityGroupSettings tqgSettings,
			double denominatorSuppresionThreshold,
			double numeratorSuppresionThreshold,
			bool isMarginSuppression,
			EMeasureType measureType,
			EMeasureSource measureSource)
		{
			//	Variables.
			var suppressWithDash = '-';
			var suppressWithC = 'c';

			//	Do Suppression for each row.
			foreach (var tempQuality in tempQualitys)
			{
				//	Variables.
				double? denominator = null;
				double? numerator = null;
				double? riskAdjustedRate = null;
				double? riskAdjustedRateLow = null;
				double? riskAdjustedRateHigh = null;
				double? expectedRate = null;
				double? observedRate = null;

				//	Get values.
				GetSuppressionInputValues(
					tempQuality,
					tqgSettings.ColumnMap,
					out denominator,
					out numerator,
					out riskAdjustedRate,
					out riskAdjustedRateLow,
					out riskAdjustedRateHigh,
					out expectedRate,
					out observedRate);

				//	Do suppression.
				if (!tqgSettings.SkipDenominatorSuppression &&
					(denominator == null ||
					(denominator <= denominatorSuppresionThreshold) && denominatorSuppresionThreshold != 0))
				{
					if (numerator == null)
					{
						//	Show Rates.
						SuppressNumeratorValues(tempQuality, tqgSettings.ColumnMap, suppressWithDash);
						SuppressDenominatorValues(tempQuality, tqgSettings.ColumnMap, suppressWithDash);
					}
					else
					{
						SuppressRateValues(tempQuality, tqgSettings.ColumnMap, suppressWithDash);
						SuppressNumeratorValues(tempQuality, tqgSettings.ColumnMap, suppressWithDash);
						SuppressDenominatorValues(tempQuality, tqgSettings.ColumnMap, suppressWithDash);
					}
				}
				else
				{
					if (numerator == null)
					{
						//	Show Rates.
						SuppressNumeratorValues(tempQuality, tqgSettings.ColumnMap, suppressWithDash);
						//	Show Denominator.
					}
					else
					{
						if (numerator <= numeratorSuppresionThreshold && numeratorSuppresionThreshold != 0)
						{
							SuppressRateValues(tempQuality, tqgSettings.ColumnMap, suppressWithC);
							SuppressNumeratorValues(tempQuality, tqgSettings.ColumnMap, suppressWithC);
							//	Show Denominator.
						}
						else
						{
							if (isMarginSuppression && 
								(denominator != null && (denominator - numerator) < numeratorSuppresionThreshold))
							{
								SuppressRateValues(tempQuality, tqgSettings.ColumnMap, suppressWithC);
								SuppressNumeratorValues(tempQuality, tqgSettings.ColumnMap, suppressWithC);
								//	Show Denominator.
							}
							else
							{
								//	Show all. No Suppression.
								//	Show Rates.
								//	Show Numerator.
								//	Show Denominator.
							}
						}
					}
				}

				//  Generic/Overall Suppression.
				RunSuppressNotEnoughDataValues(tempQuality, tqgSettings.ColumnMap, suppressWithDash);
			}
		}
        #endregion

        #region Suppress SubComponent Methods.
        /// <summary>
        /// Suppresses the rate values.
        /// </summary>
        /// <param name="tempQuality">The temporary quality.</param>
        /// <param name="suppressionDataMap">The suppression data map.</param>
        /// <param name="suppressionValue">The suppression value.</param>
        private static void SuppressRateValues(
			TempQuality tempQuality,
			IDictionary<EDisplayColumn, ColumnSettings> suppressionDataMap,
			char suppressionValue)
		{
			SuppressValue(tempQuality, suppressionDataMap, EDisplayColumn.RiskAdjustedRate, suppressionValue, false, true);
			SuppressValue(tempQuality, suppressionDataMap, EDisplayColumn.RiskAdjustedUpperCI, suppressionValue, false, true);
			SuppressValue(tempQuality, suppressionDataMap, EDisplayColumn.RiskAdjustedLowerCI, suppressionValue, false, true);
			SuppressValue(tempQuality, suppressionDataMap, EDisplayColumn.x100_Minus_RiskAdjustedRate, suppressionValue, false, true);
			SuppressValue(tempQuality, suppressionDataMap, EDisplayColumn.ObservedRate, suppressionValue, false, true);
			SuppressValue(tempQuality, suppressionDataMap, EDisplayColumn.ExpectedRate, suppressionValue, false, true);
			SuppressValue(tempQuality, suppressionDataMap, EDisplayColumn.RateAndCI, suppressionValue, false, true);
			SuppressValue(tempQuality, suppressionDataMap, EDisplayColumn.ObservedLowerCI, suppressionValue, false, true);
			SuppressValue(tempQuality, suppressionDataMap, EDisplayColumn.ObservedUpperCI, suppressionValue, false, true);
		}
        /// <summary>
        /// Suppresses the numerator values.
        /// </summary>
        /// <param name="tempQuality">The temporary quality.</param>
        /// <param name="suppressionDataMap">The suppression data map.</param>
        /// <param name="suppressionValue">The suppression value.</param>
        private static void SuppressNumeratorValues(
			TempQuality tempQuality,
			IDictionary<EDisplayColumn, ColumnSettings> suppressionDataMap,
			char suppressionValue)
		{
			SuppressValue(tempQuality, suppressionDataMap, EDisplayColumn.ObservedNumerator, suppressionValue, false, true);
		}
        /// <summary>
        /// Suppresses the denominator values.
        /// </summary>
        /// <param name="tempQuality">The temporary quality.</param>
        /// <param name="suppressionDataMap">The suppression data map.</param>
        /// <param name="suppressionValue">The suppression value.</param>
        private static void SuppressDenominatorValues(
			TempQuality tempQuality,
			IDictionary<EDisplayColumn, ColumnSettings> suppressionDataMap,
			char suppressionValue)
		{
			SuppressValue(tempQuality, suppressionDataMap, EDisplayColumn.ObservedDenominator, suppressionValue, false, true);
		}

        /// <summary>
        /// Runs the suppress not enough data values.
        /// </summary>
        /// <param name="tempQuality">The temporary quality.</param>
        /// <param name="suppressionDataMap">The suppression data map.</param>
        /// <param name="suppressionValue">The suppression value.</param>
        private static void RunSuppressNotEnoughDataValues(
			TempQuality tempQuality,
			IDictionary<EDisplayColumn, ColumnSettings> suppressionDataMap,
			char suppressionValue)
		{

			foreach (var dataMapEntry in suppressionDataMap.ToList())
			{
				var num = ParseDouble(tempQuality, dataMapEntry.Value.SuppressionEvaluationColumn);
				if (num != null && num == -1)
					SuppressValue(tempQuality, suppressionDataMap, dataMapEntry.Key, suppressionValue,true,false);
			}
		}

        /// <summary>
        /// Suppresses the value.
        /// </summary>
        /// <param name="tempQuality">The temporary quality.</param>
        /// <param name="suppressionDataMap">The suppression data map.</param>
        /// <param name="suppressionColumn">The suppression column.</param>
        /// <param name="suppressionValue">The suppression value.</param>
        /// <param name="excludeZero">if set to <c>true</c> [exclude zero].</param>
        /// <param name="excludeNegOne">if set to <c>true</c> [exclude neg one].</param>
        private static void SuppressValue(
			TempQuality tempQuality,
			IDictionary<EDisplayColumn, ColumnSettings> suppressionDataMap,
			EDisplayColumn suppressionColumn,
			char suppressionValue,
			bool excludeZero = true,
			bool excludeNegOne = true)
		{
			if (suppressionDataMap.ContainsKey(suppressionColumn))
			{
				if (excludeZero || excludeNegOne)
				{
					var num = ParseDouble(tempQuality, suppressionDataMap[suppressionColumn].SuppressionEvaluationColumn);
					if (num != null)
					{
						if (excludeZero && num == 0) return;
						if (excludeNegOne && num == -1) return;
					}
				}
				tempQuality.SetPropertyValue(suppressionDataMap[suppressionColumn].TargetColumn.ToString(), suppressionValue);
			}
		}
        #endregion
        #endregion

        #region Format Methods.
        /// <summary>
        /// Does the formatting process.
        /// </summary>
        /// <param name="tempQualitys">The temporary qualitys.</param>
        /// <param name="tqgSettings">The TQG settings.</param>
        /// <param name="measureType">Type of the measure.</param>
        /// <param name="measureSource">The measure source.</param>
        /// <param name="measureScaleBy">The measure scale by.</param>
        private static void DoFormattingProcess(
			List<TempQuality> tempQualitys, 
			TempQualityGroupSettings tqgSettings,
			EMeasureType measureType, 
			EMeasureSource measureSource,
			double measureScaleBy)
		{
			//	Do Formatting for each row.
			foreach (var tqRow in tempQualitys)
			{
				foreach (var colSetting in tqgSettings.ColumnMap)
				{
					var colPropertyName = colSetting.Value.TargetColumn.ToString();
					var objValue = tqRow.GetPropertyValue(colPropertyName);
					var formattedValue = GetFormattedValue(objValue, colSetting.Value.Format);
					if (formattedValue != null)
						tqRow.SetPropertyValue(colPropertyName, formattedValue);
				}
			}
		}
        /// <summary>
        /// Gets the formatted value.
        /// </summary>
        /// <param name="objValue">The object value.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        private static string GetFormattedValue(Object objValue, EMeasureFieldFormat format)
		{
			var value = objValue.AsNullableDouble();

			//  If the value cannot be parsed as a double, then don't do any formating.
			if (value != null)
			{
				switch (format)
				{
					case EMeasureFieldFormat.Percent:
						return value.Value.ToString() + "%";
						//	var scaledValue = value.Value * (100.0 / measureScaleBy);
						//	tqRow.SetPropertyValue(colPropertyName, scaledValue.ToString() + "%");
					case EMeasureFieldFormat.Ratio: break;
					case EMeasureFieldFormat.Volume: break;
					case EMeasureFieldFormat.Rate: break;
					case EMeasureFieldFormat.Scale: break;
					case EMeasureFieldFormat.Minutes:
						return value.Value.ToString() + " minutes";
					case EMeasureFieldFormat.None: break;
				}
			}
			return null;
		}
        #endregion

        #region Util Methods.
        /// <summary>
        /// Parses the double.
        /// </summary>
        /// <param name="tempQuality">The temporary quality.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        private static double? ParseDouble(
			TempQuality tempQuality,
			ETempQualityColumn column)
		{
			return tempQuality.GetPropertyValue(
				column.ToString(),
				x => (x == null) ? null : MathExtensions.ParseNullableDouble(x.ToString()));
		}
        /// <summary>
        /// Tries the parse double.
        /// </summary>
        /// <param name="tempQuality">The temporary quality.</param>
        /// <param name="column">The column.</param>
        /// <param name="num">The number.</param>
        /// <returns></returns>
        private static bool TryParseDouble(
			TempQuality tempQuality,
			ETempQualityColumn column,
			out double num)
		{
			num = 0.0;
			var numObj = tempQuality.GetPropertyValue(column.ToString());
			return (numObj != null && double.TryParse(numObj.ToString(), out num));
		}
        #endregion

        /// <summary>
        /// Posts the process quality report.
        /// </summary>
        /// <param name="reportID">The report identifier.</param>
        /// <param name="currentWebsite">The current website.</param>
        /// <param name="dataserviceProvider">The dataservice provider.</param>
        internal static void PostProcessQualityReport(string reportID, Website currentWebsite, IDomainSessionFactoryProvider dataserviceProvider)
		{
			//TempQualityServiceSync.Load<TempQuality>();
			using (var session = dataserviceProvider.SessionFactory.OpenSession())
			using (var trans = session.BeginTransaction())
			{
				var tempQualitys =
					session
						.Query<TempQuality>()
						.Where(x => x.ReportId.ToString().Equals(reportID))
						.ToList();



				//	TempQuality Table.
				foreach (WebsiteMeasure websiteMeasure in currentWebsite.Measures.Where(m => m.IsSelected).ToList())
				{
					int measureID = websiteMeasure.ReportMeasure.Id;
					var curTempQualitys = tempQualitys.Where(x => x.MeasureId == measureID).ToList();
					if (curTempQualitys.Count > 0)
					{
						QualityReportPostProcessLogic.DoPostProcess_TempQuality(curTempQualitys, websiteMeasure);
					}
				}

				//	TempQualityMeasures Table.
				//	- It turns up updating the TQM with symbols breaks the website.
				var measureNames = currentWebsite.Measures.Select(m => m.ReportMeasure.Name).ToList();
				//QualityReportPostProcessLogic.DoPostProcess_TempQualityMeasure(session, reportID, measureNames);

				//TestSuppression(tempQualitys);

				foreach (var tq in tempQualitys)
				{
					session.Update(tq);
				}
				trans.Commit();
			}
		}
        /// <summary>
        /// Formats the measure value.
        /// </summary>
        /// <param name="measureName">Name of the measure.</param>
        /// <param name="origValue">The original value.</param>
        /// <returns></returns>
        public static string FormatMeasureValue(string measureName,string origValue)
		{
			var format = GetColumnSettingsFormat(measureName);
			var formattedValue = GetFormattedValue(origValue, format);
			return formattedValue == null ? origValue : formattedValue;
		}
        /// <summary>
        /// Tests the suppression.
        /// </summary>
        /// <param name="tempQualitys">The temporary qualitys.</param>
        private static void TestSuppression(List<TempQuality> tempQualitys)
		{
			var reportId = tempQualitys[0].ReportId;
			var measureSource = EMeasureSource.AHRQ;
			var measureType = EMeasureType.QIprovider_MCMC;
			var tqgSettings = GetTempQualityGroupSettings(measureSource, measureType,"");


			List<TempQuality> tqs = new List<TempQuality>();
			tqs.Add(new TempQuality()
							{
								ReportId = reportId,


								RateAndCI = "-1.0000(-1.00000,-1.00000)",
								NatFilled = "0",
								NatRating = "1",
								PeerFilled = "0",
								PeerRating = "1",
								Col1 = "-1.00000",
								Col2 = "-1.00000",
								Col3 = "-1.00000",
								Col4 = "-1.00000",
								Col5 = "-1.00000",
								Col6 = "-1.00000",
								Col7 = "-1.00000",
								Col8 = "-1.00000",
								Col9 = "-1.00000",
								Col10 = "-1.00000",
							});


			DoSuppressProcess(
				tqs,
				tqgSettings,
				0,							//denominatorSuppresionThreshold,
				0,							//numeratorSuppresionThreshold,
				false,						//isMarginSuppression,
				measureType,				//measureType,
				measureSource);				//measureSource);


			tempQualitys.AddRange(tqs);
		}
	}
}
