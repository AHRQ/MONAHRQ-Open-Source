using System.ComponentModel;

namespace Monahrq.Websites.ViewModels.Publish
{
    public enum ValidationOutcome
    {
        [Description("Warning 1026")]
        AboutUsContent,
        [Description("Warning 1022")]
        CmsProviderIdUnmapped,
        [Description("Warning 1009")]
        CostToChargeUndefined,
        [Description("Error 1027")]
        DatasetsMissing,
        [Description("Warning 1000")]
        HospitalsMissingRegions,
        [Description("Error 1030")]
        MeasuresMissing,
        [Description("")]
        NameMissing,
        [Description("Error 1025")]
        OutputFolder,
        [Description("Error 1032")]
        ReportsMissing,
        [Description("")]
        StaleBaseData,
        [Description("Error 1024")]
        StateNotSelected,
        [Description("")]
        Success,
        [Description("Warning 1034")]
        DataYearMismatchForDataSetFile,
        [Description("Warning 1033")]
        DataYearMismatchForTrendingDataSetFile,
        [Description("Warning 1001")]
        CheckDatasetMappingForCounty,
        [Description("Error 1002")]
        EmergencyDepartmentTreatAndReleaseReportEDServices,
        [Description("Error 1028")]
        EmergencyDepartmentTreatAndReleaseReportIPDataSet,
        [Description("Warning 1029")]
        NoMeasuresForTheDataSet,
        [Description("Warning 1006")]
        CustomRegionMissingPopulationCount,
        [Description("Warning 1049")]
        CustomRegionOrCustomPopulationNotDefined,
        [Description("Warning 1042")]
        CustomRegionAndIPCustomRegionNotMatched, // Need to add new Error for this.
        [Description("Warning 1049")]
        MissingCustomRegionAndPopulationMapping,
        [Description("Warning 1007")]
        MissgingCustomRegion,
        [Description("Warning 1008")]
        PopulationMissingRegions,
        [Description("Warning 1043")]
        CustomRegionNotDefined,
        [Description("Warning 1023")]
        AhrqQiDbConnection,
        [Description("Warning 1035")]
        MissingMedicareDataset,
        [Description("Warning 1022")]
        QIUnMappedHospitalLocalId,
        [Description("Warning 1022")]
        UnMappedHospitalLocalId,
        [Description("Warning 1022")]
        UnMappedHospitalProfileLocalId,
        [Description("Warning 1035")]
        HospitalProfileMissingMedicareDataset,
        [Description("Warning 1035")]
        HospitalProfileMissingIPDataset,
        [Description("Warning 1003")]
        CostQualityIpFileNotImported,
        [Description("Warning 1004")]
        CostQualityQiFileNotImported,
        [Description("Warning 1031")]
        MeasuresDoNotSupportCost,
        [Description("Warning 1022")]
        HospitalProfileMissingLocalHospIdOrProviderId,
        [Description("Warning 1036")]
        CustomHeatMap,
        [Description("Warning 1024")]
        UnMappdedHospitals,
        [Description("Warning 1040")]
        InvalidCountyFips,
        [Description("Warning 1041")]
        InvalidRegionId,
        [Description("Warning 1042")]
        MissingDischargeQuarter,
        [Description("Error 1040")]
        TrendingQuarters,
        [Description("Warning 1045")]
        CostQualityQiDbConnection,
        [Description("Warning 1046")]
        CostQualityAllFamilySelected,
        [Description("Warning 1044")]
        InValidHedisDataset,
        [Description("Warning 1047")]
        MissingPhysicianReport,
        [Description("Warning 1048")]
        RealtimePhysicianDataCannotHaveSubReports,
        [Description("Warning 1050")]
        ObsoleteQualityIndicators, // moved to QI module
        [Description("Warning 1051")]
        Pqi13HasNoData,// moved to QI module

    }
}