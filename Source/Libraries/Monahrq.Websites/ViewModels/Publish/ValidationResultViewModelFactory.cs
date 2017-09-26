using System;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Websites.Events;

namespace Monahrq.Websites.ViewModels.Publish
{
    public class ValidationResultViewModelFactory
    {
        WebsiteViewModel SourceViewModel { get; set; }

        public ValidationResultViewModelFactory(WebsiteViewModel sourceViewModel)
        {
            SourceViewModel = sourceViewModel;

        }

        public ValidationResultViewModel BuildResult(ValidationOutcome outcome, string reportingYear = "", string reportName = "", string datasetName = "")
        {
            var result = new ValidationResultViewModel();

            result.Quality = OutcomeQuality(outcome);

            result.HelpTopic = outcome.GetDescription();
            if (string.IsNullOrEmpty(result.HelpTopic))
            {
                result.HelpTopic = "DEPENDENCY CHECK";
                result.HelpText = "Click here for more information";
            }
            else if (result.HelpTopic.ContainsIgnoreCase("Warning") || result.HelpTopic.ContainsIgnoreCase("Error"))
            {
                result.HelpTopic = result.HelpTopic.Replace("Warning ", null).Replace("Error ", null);
                result.HelpText = "Click here for more information";
            }

            if (result.Quality == ValidationLevel.Success)
                return result;

            var temp = OutcomeMesssage(outcome);
            switch (outcome)
            {
                case ValidationOutcome.StaleBaseData:
                    temp = string.Format(temp, SourceViewModel.Website.ReportedYear, reportingYear, reportName);
                    break;
                case ValidationOutcome.CostToChargeUndefined:
                case ValidationOutcome.RealtimePhysicianDataCannotHaveSubReports:
                    temp = string.Format(temp, reportName);
                    break;
                case ValidationOutcome.CmsProviderIdUnmapped:
                case ValidationOutcome.HospitalProfileMissingLocalHospIdOrProviderId:
                case ValidationOutcome.CostQualityAllFamilySelected:
                    temp = string.Format(temp, reportName);
                    break;
                //case ValidationOutcome.HospitalProfileMissingMedicareDataset:
                //case ValidationOutcome.HospitalProfileMissingIPDataset:
                case ValidationOutcome.NoMeasuresForTheDataSet:
                case ValidationOutcome.QIUnMappedHospitalLocalId:
                case ValidationOutcome.UnMappedHospitalLocalId:
                case ValidationOutcome.UnMappedHospitalProfileLocalId:
                    temp = string.Format(temp, datasetName);
                    break;
            }
            result.Message = temp;

            result.CompositionArea = OutcomeArea(outcome);
            return result;
        }

        public static string OutcomeMesssage(ValidationOutcome outcome)
        {
            switch (outcome)
            {
                case ValidationOutcome.AboutUsContent:
                    return ValidationResults.AboutUsContent;
                case ValidationOutcome.CmsProviderIdUnmapped:
                    return ValidationResults.CMSProviderIdUnmapped;
                case ValidationOutcome.CostToChargeUndefined:
                    return ValidationResults.CostToChargeUndefined;
                case ValidationOutcome.HospitalsMissingRegions:
                    return ValidationResults.HospitalsMissingRegions;
                case ValidationOutcome.StaleBaseData:
                    return ValidationResults.StaleBaseData;
                case ValidationOutcome.DatasetsMissing:
                    return ValidationResults.DatasetsMissing;
                case ValidationOutcome.MeasuresMissing:
                    return ValidationResults.MeasuresMissing;
                case ValidationOutcome.OutputFolder:
                    return ValidationResults.OutputFolder;
                case ValidationOutcome.ReportsMissing:
                    return ValidationResults.ReportsMissing;
                case ValidationOutcome.StateNotSelected:
                    return ValidationResults.StateNotSelected;
                case ValidationOutcome.NameMissing:
                    return ValidationResults.NameMissing;
                case ValidationOutcome.DataYearMismatchForDataSetFile:
                    return ValidationResults.DataYearMismatchForDataSetFile;
                case ValidationOutcome.DataYearMismatchForTrendingDataSetFile:
                    return ValidationResults.DataYearMismatchForTrendingDataSetFile;
                case ValidationOutcome.CheckDatasetMappingForCounty:
                    return ValidationResults.CheckDatasetMappingForCounty;
                case ValidationOutcome.EmergencyDepartmentTreatAndReleaseReportEDServices:
                    return ValidationResults.EmergencyDepartmentTreatAndReleaseReportEDServices;
                case ValidationOutcome.EmergencyDepartmentTreatAndReleaseReportIPDataSet:
                    return ValidationResults.EmergencyDepartmentTreatAndReleaseReportIPDataSet;
                case ValidationOutcome.NoMeasuresForTheDataSet:
                    return ValidationResults.NoMeasuresForTheDataset;
                case ValidationOutcome.CustomRegionMissingPopulationCount:
                    return ValidationResults.CustomRegionMissingPopulationCount;
                case ValidationOutcome.MissingCustomRegionAndPopulationMapping:
                case ValidationOutcome.CustomRegionOrCustomPopulationNotDefined:
                    return ValidationResults.MissingCustomRegionPopulationMapping;
                case ValidationOutcome.CustomRegionAndIPCustomRegionNotMatched:
                    return ValidationResults.CustomRegionAndIPCustomRegionNotMatched;
                case ValidationOutcome.MissgingCustomRegion:
                    return ValidationResults.MissingCustomRegion;
                case ValidationOutcome.PopulationMissingRegions:
                    return ValidationResults.PopulationMissingRegions;
                case ValidationOutcome.CustomRegionNotDefined:
                    return ValidationResults.CustomRegionNotDefined;
                case ValidationOutcome.AhrqQiDbConnection:
                    return ValidationResults.AhrqQiDbConnection;
                //case ValidationOutcome.MissingMedicareDataset:
                //    return ValidationResults.MissingMedicareDataset;
                case ValidationOutcome.QIUnMappedHospitalLocalId:
                    return ValidationResults.QIUnMappedHospitalLocalId;
                case ValidationOutcome.UnMappedHospitalLocalId:
                    return ValidationResults.UnMappedHospitalLocalId;
                case ValidationOutcome.UnMappedHospitalProfileLocalId:
                    return ValidationResults.UnMappedHospitalProfileLocalId;
                case ValidationOutcome.HospitalProfileMissingMedicareDataset:
                    return ValidationResults.MissingMedicareDataset;
                case ValidationOutcome.HospitalProfileMissingIPDataset:
                    return ValidationResults.MissingHospitalProfileReportIPDataset;
                case ValidationOutcome.CostQualityIpFileNotImported:
                    return ValidationResults.CostQualityIPFileNotImported;
                case ValidationOutcome.CostQualityQiFileNotImported:
                    return ValidationResults.CostQualityQIProviderFileNotImported;
                case ValidationOutcome.MeasuresDoNotSupportCost:
                    return ValidationResults.NoMeasuresForCostAndQuality;
                case ValidationOutcome.HospitalProfileMissingLocalHospIdOrProviderId:
                    return ValidationResults.UnMappedHospitalProfileLocalId;
                case ValidationOutcome.CustomHeatMap:
                    return ValidationResults.CustomHeatMap;
                case ValidationOutcome.UnMappdedHospitals:
                    return ValidationResults.UnMappdedHospitals;
                case ValidationOutcome.InvalidCountyFips:
                    return ValidationResults.InvalidCountyFips;
                case ValidationOutcome.InvalidRegionId:
                    return ValidationResults.InvalidRegionId;
                case ValidationOutcome.MissingDischargeQuarter:
                    return ValidationResults.MissingDischargeQuarter;
                case ValidationOutcome.TrendingQuarters:
                    return ValidationResults.TrendingQuarters;
                case ValidationOutcome.CostQualityAllFamilySelected:
                    return ValidationResults.CostQualityAllFamilySelected;
                case ValidationOutcome.CostQualityQiDbConnection:
                    return ValidationResults.CostQualityQiDbConnection;
                case ValidationOutcome.InValidHedisDataset:
                    return ValidationResults.InValidPhysicianHedisDataset;
                case ValidationOutcome.MissingPhysicianReport:
                    return ValidationResults.MissingPhysicianReport;
                case ValidationOutcome.RealtimePhysicianDataCannotHaveSubReports:
                    return ValidationResults.RealtimePhysicianDataCannotHaveSubReports;
            }
            return ValidationResults.Success;
        }

        public static ValidationLevel OutcomeQuality(ValidationOutcome outcome)
        {
            switch (outcome)
            {
                case ValidationOutcome.AboutUsContent:
                case ValidationOutcome.CmsProviderIdUnmapped:
                case ValidationOutcome.CostToChargeUndefined:
                case ValidationOutcome.HospitalsMissingRegions:
                case ValidationOutcome.StaleBaseData:
                case ValidationOutcome.DataYearMismatchForDataSetFile:
                case ValidationOutcome.DataYearMismatchForTrendingDataSetFile:
                case ValidationOutcome.CheckDatasetMappingForCounty:
                case ValidationOutcome.NoMeasuresForTheDataSet:
                case ValidationOutcome.CustomRegionMissingPopulationCount:
                case ValidationOutcome.MissgingCustomRegion:
                case ValidationOutcome.PopulationMissingRegions:
                case ValidationOutcome.CustomRegionNotDefined:
                case ValidationOutcome.AhrqQiDbConnection:
                case ValidationOutcome.MissingMedicareDataset:
                case ValidationOutcome.QIUnMappedHospitalLocalId:
                case ValidationOutcome.UnMappedHospitalLocalId:
                case ValidationOutcome.UnMappedHospitalProfileLocalId:
                case ValidationOutcome.HospitalProfileMissingMedicareDataset:
                case ValidationOutcome.HospitalProfileMissingIPDataset:
                case ValidationOutcome.CostQualityIpFileNotImported:
                case ValidationOutcome.CostQualityQiFileNotImported:
                case ValidationOutcome.MeasuresDoNotSupportCost:
                case ValidationOutcome.HospitalProfileMissingLocalHospIdOrProviderId:
                case ValidationOutcome.CustomHeatMap:
                case ValidationOutcome.UnMappdedHospitals:
                case ValidationOutcome.CustomRegionAndIPCustomRegionNotMatched:
                case ValidationOutcome.InvalidCountyFips:
                case ValidationOutcome.InvalidRegionId:
                case ValidationOutcome.MissingDischargeQuarter:
                case ValidationOutcome.TrendingQuarters:
                case ValidationOutcome.CostQualityQiDbConnection:
                case ValidationOutcome.CostQualityAllFamilySelected:
                case ValidationOutcome.InValidHedisDataset:
                case ValidationOutcome.MissingPhysicianReport:
                case ValidationOutcome.RealtimePhysicianDataCannotHaveSubReports:
                case ValidationOutcome.ObsoleteQualityIndicators:
                case ValidationOutcome.Pqi13HasNoData:
                    return ValidationLevel.Warning;
                case ValidationOutcome.DatasetsMissing:
                case ValidationOutcome.MeasuresMissing:
                case ValidationOutcome.OutputFolder:
                case ValidationOutcome.ReportsMissing:
                case ValidationOutcome.StateNotSelected:
                case ValidationOutcome.NameMissing:
                case ValidationOutcome.EmergencyDepartmentTreatAndReleaseReportEDServices:
                case ValidationOutcome.EmergencyDepartmentTreatAndReleaseReportIPDataSet:
                case ValidationOutcome.MissingCustomRegionAndPopulationMapping:
                case ValidationOutcome.CustomRegionOrCustomPopulationNotDefined:
                    // Make these warnings for now as per Megan / QA team
                    //case ValidationOutcome.HospitalProfileMissingMedicareDataset: 
                    //case ValidationOutcome.HospitalProfileMissingIPDataset:
                    return ValidationLevel.Error;
            }
            return ValidationLevel.Success;
        }

        public static WebsiteTabViewModels OutcomeArea(ValidationOutcome outcome)
        {
            switch (outcome)
            {
                case ValidationOutcome.AboutUsContent:
                case ValidationOutcome.OutputFolder:
                case ValidationOutcome.StateNotSelected:
                case ValidationOutcome.CostToChargeUndefined:
                case ValidationOutcome.AhrqQiDbConnection:
                case ValidationOutcome.HospitalProfileMissingLocalHospIdOrProviderId:
                case ValidationOutcome.CostQualityQiDbConnection:
                    return WebsiteTabViewModels.Settings;
                case ValidationOutcome.StaleBaseData:
                case ValidationOutcome.CmsProviderIdUnmapped:
                case ValidationOutcome.ReportsMissing:
                case ValidationOutcome.MissingCustomRegionAndPopulationMapping:
                case ValidationOutcome.PopulationMissingRegions:
                //case ValidationOutcome.MissingMedicareDataset:
                case ValidationOutcome.HospitalProfileMissingMedicareDataset:
                case ValidationOutcome.HospitalProfileMissingIPDataset:
                case ValidationOutcome.CustomRegionOrCustomPopulationNotDefined:
                case ValidationOutcome.CustomRegionAndIPCustomRegionNotMatched:
                case ValidationOutcome.TrendingQuarters:
                    return WebsiteTabViewModels.Reports;
                case ValidationOutcome.MeasuresMissing:
                case ValidationOutcome.NoMeasuresForTheDataSet:
                case ValidationOutcome.CostQualityAllFamilySelected:
                case ValidationOutcome.ObsoleteQualityIndicators:
                case ValidationOutcome.Pqi13HasNoData:
                    return WebsiteTabViewModels.Measures;
                case ValidationOutcome.NameMissing:
                    return WebsiteTabViewModels.Details;
                case ValidationOutcome.HospitalsMissingRegions:
                case ValidationOutcome.DatasetsMissing:
                case ValidationOutcome.DataYearMismatchForDataSetFile:
                case ValidationOutcome.DataYearMismatchForTrendingDataSetFile:
                case ValidationOutcome.CheckDatasetMappingForCounty:
                case ValidationOutcome.EmergencyDepartmentTreatAndReleaseReportEDServices:
                case ValidationOutcome.EmergencyDepartmentTreatAndReleaseReportIPDataSet:
                case ValidationOutcome.CustomRegionMissingPopulationCount:
                case ValidationOutcome.MissgingCustomRegion:
                case ValidationOutcome.CustomRegionNotDefined:
                case ValidationOutcome.QIUnMappedHospitalLocalId:
                case ValidationOutcome.UnMappedHospitalLocalId:
                case ValidationOutcome.UnMappedHospitalProfileLocalId:
                case ValidationOutcome.CostQualityIpFileNotImported:
                case ValidationOutcome.CostQualityQiFileNotImported:
                case ValidationOutcome.MeasuresDoNotSupportCost:
                case ValidationOutcome.CustomHeatMap:
                case ValidationOutcome.UnMappdedHospitals:
                case ValidationOutcome.InvalidCountyFips:
                case ValidationOutcome.InvalidRegionId:
                case ValidationOutcome.MissingDischargeQuarter:
                case ValidationOutcome.InValidHedisDataset:
                case ValidationOutcome.MissingPhysicianReport:
                case ValidationOutcome.RealtimePhysicianDataCannotHaveSubReports:
                    return WebsiteTabViewModels.Datasets;
                default:
                    return WebsiteTabViewModels.Manage;

            }
            throw new InvalidOperationException("Unsupported Validation Area");
        }

    }
}