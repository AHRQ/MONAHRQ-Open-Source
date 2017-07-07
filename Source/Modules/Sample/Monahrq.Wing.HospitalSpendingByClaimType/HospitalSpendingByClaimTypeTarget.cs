using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Validation;

namespace Monahrq.Wing.HospitalSpendingByClaimType
{
    /// <summary>
    /// Sample definition for a custom data set in Monahrq
    /// </summary>
    [WingTarget("Hospital Spending by Claim Type",
        HospitalSpendingByClaimTypeTarget.GuidString,
        "Hospital Spending by Claim Type Sample Target",
        isReferenceTarget: false, 
        isTrendingEnabled: false,
        displayOrder: 0,
        PublisherName = "Agency for Healthcare Research and Quality (AHRQ)",
        PublisherEmail = "moanhrq@ahrq.gov",
        PublisherWebsite = "http://monahrq.ahrq.gov/")]

    [EntityTableName("Targets_HospitalSpendingByClaimTypeTargets")] // possible bug: "Targets" suffix comes from type name, as defined by DatasetRecordMap

    // exclude entire row if either 'period' or 'claim type' is marked "exclude"
    [RejectIfAnyPropertyHasValue(typeof(HospitalSpendingByClaimTypeTarget), HospitalSpendingByClaimType.Period.Exclude, HospitalSpendingByClaimType.ClaimType.Exclude)]

    public class HospitalSpendingByClaimTypeTarget : DatasetRecord
    {
        // the GUID needs to be shared between several components, so put it in one place
        internal const string GuidString = "71a308bb-05f8-4e18-84e6-8db6a7ecc7dc";
        internal static readonly System.Guid Guid = System.Guid.Parse(GuidString);

        #region Primary Key
        [Required, StringLength(12)]
        // case matters! reflection is used to map WingTargetElement.Name to PropertyInfo.Name
        [WingTargetElement("CmsProviderId", "Provider ID", true, 1)] 
        public string CmsProviderId { get; set; }

        [Required, WingTargetElement("Period", "Period", true, 2)]
        public Period? Period { get; set; }

        [Required, WingTargetElement("ClaimType", "Claim Type", true, 3)]
        public ClaimType? ClaimType { get; set; }
        #endregion

        #region Data Fields
        [Required]
        [WingTargetElement("AverageSpendingPerEpisodeHospital", 
            description: "Average Spending per Episode - Hospital",
            isRequired: true,
            ordinal: 4,
            longDecription: "The average amount of money spent per admission at this hospital")]
        public double AverageSpendingPerEpisodeHospital { get; set; }

        [Required, WingTargetElement("AverageSpendingPerEpisodeState", "Average Spending per Episode - State", true, 5)]
        public double AverageSpendingPerEpisodeState { get; set; }

        [Required, WingTargetElement("AverageSpendingPerEpisodeNational", "Average Spending per Episode - National", true, 6)]
        public double AverageSpendingPerEpisodeNational { get; set; }

        // require percentages to be 0.0-1.0
        [Required, Range(0.0d, 1.0d), WingTargetElement("PercentSpendingPerEpisodeHospital", "Percent Spending per Episode - Hospital", true, 7)]
        public double PercentSpendingPerEpisodeHospital { get; set; }

        [Required, Range(0.0d, 1.0d), WingTargetElement("PercentSpendingPerEpisodeState", "Percent Spending per Episode - State", true, 8)]
        public double PercentSpendingPerEpisodeState { get; set; }

        [Required, Range(0.0d, 1.0d), WingTargetElement("PercentSpendingPerEpisodeNational", "Percent Spending per Episode - National", true, 9)]
        public double PercentSpendingPerEpisodeNational { get; set; }
        #endregion
    }
}
