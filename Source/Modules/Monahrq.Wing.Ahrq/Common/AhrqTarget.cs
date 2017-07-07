using System.ComponentModel.DataAnnotations;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Wing.Ahrq.Common
{
    /// <summary>
    /// Model class for Ahrq target.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecord" />
    [EntityTableName("Targets_AhrqTargets")]
    public abstract class AhrqTarget : DatasetRecord
    {
        /// <summary>
        /// Gets or sets the measure code.
        /// </summary>
        [StringLength(30, MinimumLength = 1), WingTargetElement("MeasureCode", false, 1)]
        public virtual string MeasureCode { get; set; }

        /// <summary>
        /// Gets or sets the stratification.
        /// </summary>
        [StringLength(12, MinimumLength = 1), WingTargetElement("Stratification", false, 2)]
        public virtual string Stratification { get; set; }
        /// <summary>
        // Gets or set the Local hospital ID
        /// </summary>
        [StringLength(12, MinimumLength = 1), WingTargetElement("LocalHospitalID", false, 3)]
        public virtual string LocalHospitalID { get; set; }

        /// <summary>
        /// Gets or sets the county fips.
        /// </summary>
        /// <value>
        /// The county fips.
        /// </value>
        [StringLength(12, MinimumLength = 1), WingTargetElement("County", false, 4)]
        public virtual string CountyFIPS { get; set; }

        /// <summary>
        /// Gets or sets the observed numerator.
        /// </summary>
        /// <value>
        /// The observed numerator.
        /// </value>
        [WingTargetElement("ObservedNumerator", false, 4)]
        public virtual int? ObservedNumerator { get; set; }

        /// <summary>
        /// Gets or sets the observed denominator.
        /// </summary>
        /// <value>
        /// The observed denominator.
        /// </value>
        [WingTargetElement("ObservedDenominator", false, 4)]
        public virtual int? ObservedDenominator { get; set; }

        /// <summary>
        /// Gets or sets the observed rate.
        /// </summary>
        /// <value>
        /// The observed rate.
        /// </value>
        [WingTargetElement("ObservedRate", false, 4)]
        public virtual decimal? ObservedRate { get; set; }

        /// <summary>
        /// Gets or sets the observed ci high.
        /// </summary>
        /// <value>
        /// The observed ci high.
        /// </value>
        [WingTargetElement("ObservedCIHigh", false, 4)]
        public virtual decimal? ObservedCIHigh { get; set; }

        /// <summary>
        /// Gets or sets the observed ci low.
        /// </summary>
        /// <value>
        /// The observed ci low.
        /// </value>
        [WingTargetElement("ObservedCILow", false, 4)]
        public virtual decimal? ObservedCILow { get; set; }

        /// <summary>
        /// Gets or sets the risk adjusted rate.
        /// </summary>
        /// <value>
        /// The risk adjusted rate.
        /// </value>
        [WingTargetElement("RiskAdjustedRate", false, 4)]
        public virtual decimal? RiskAdjustedRate { get; set; }

        /// <summary>
        /// Gets or sets the risk adjusted ci high.
        /// </summary>
        /// <value>
        /// The risk adjusted ci high.
        /// </value>
        [WingTargetElement("RiskAdjustedCIHigh", false, 4)]
        public virtual decimal? RiskAdjustedCIHigh { get; set; }

        /// <summary>
        /// Gets or sets the risk adjusted ci low.
        /// </summary>
        /// <value>
        /// The risk adjusted ci low.
        /// </value>
        [WingTargetElement("RiskAdjustedCILow", false, 4)]
        public virtual decimal? RiskAdjustedCILow { get; set; }

        /// <summary>
        /// Gets or sets the expected rate.
        /// </summary>
        /// <value>
        /// The expected rate.
        /// </value>
        [WingTargetElement("ExpectedRate", false, 4)]
        public virtual decimal? ExpectedRate { get; set; }

        /// <summary>
        /// Gets or sets the standard error.
        /// </summary>
        /// <value>
        /// The standard error.
        /// </value>
        [WingTargetElement("StandardErr", false, 4)]
        public virtual decimal? StandardErr { get; set; }

        /// <summary>
        /// Gets or sets the threshold.
        /// </summary>
        /// <value>
        /// The threshold.
        /// </value>
        [WingTargetElement("Threshold", false, 4)]
        public virtual int? Threshold { get; set; }

        /// <summary>
        /// Gets or sets the nat benchmark rate.
        /// </summary>
        /// <value>
        /// The nat benchmark rate.
        /// </value>
        [WingTargetElement("NatBenchmarkRate", false, 4)]
        public virtual decimal? NatBenchmarkRate { get; set; }

        /// <summary>
        /// Gets or sets the nat rating.
        /// </summary>
        /// <value>
        /// The nat rating.
        /// </value>
        [StringLength(30, MinimumLength = 1), WingTargetElement("NatRating", false, 1)]
        public virtual string NatRating { get; set; }

        /// <summary>
        /// Gets or sets the peer benchmark rate.
        /// </summary>
        /// <value>
        /// The peer benchmark rate.
        /// </value>
        [WingTargetElement("PeerBenchmarkRate", false, 4)]
        public virtual decimal? PeerBenchmarkRate { get; set; }

        /// <summary>
        /// Gets or sets the peer rating.
        /// </summary>
        /// <value>
        /// The peer rating.
        /// </value>
        [StringLength(20, MinimumLength = 1), WingTargetElement("PeerRating", false, 1)]
        public virtual string PeerRating { get; set; }

        /// <summary>
        /// Gets or sets the total cost.
        /// </summary>
        /// <value>
        /// The total cost.
        /// </value>
        [WingTargetElement("TotalCost", false, 4)]
        public virtual decimal? TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the dataset.
        /// </summary>
        /// <value>
        /// The dataset.
        /// </value>
        public override Dataset Dataset { get; set; }

        /// <summary>
        /// Creates the bulk insert mapper.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataTable">The data table.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public override IBulkMapper CreateBulkInsertMapper<T>(System.Data.DataTable dataTable, T instance = default(T), Monahrq.Infrastructure.Entities.Domain.Wings.Target target = null)
        {
            return new DatasetRecordBulkInsertMapper<T>(dataTable);
        }
    }
}
