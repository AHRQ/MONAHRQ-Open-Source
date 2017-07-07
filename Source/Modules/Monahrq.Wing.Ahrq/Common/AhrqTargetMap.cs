using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;

namespace Monahrq.Wing.Ahrq.Common
{
    /// <summary>
    /// Class for Ahrq target.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecordMap{Monahrq.Wing.Ahrq.Common.AhrqTarget}" />
    [MappingProviderExport]
    public class AhrqTargetMap : DatasetRecordMap<AhrqTarget>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AhrqTargetMap"/> class.
        /// </summary>
        public AhrqTargetMap()
        {
            var indexPrefix = string.Format("IDX_{0}", typeof(AhrqTarget).Name).ToUpper();
            DiscriminateSubClassesOnColumn("TargetType")
                 .Index("IX_AHRQTARGET_TARGETTYPE")
                 .SqlType("nvarchar(20)")
                 .Not.Nullable();

            Map(m => m.MeasureCode)
                      .Length(32)
                      .Not.Nullable()
                      .Index(indexPrefix + "_MEASURE_CODE");

            Map(m => m.Stratification).Length(12);
            Map(m => m.LocalHospitalID).Length(12).Index(indexPrefix + "_LOCAL_HOSPITAL_ID");
            Map(m => m.CountyFIPS).Length(12).Index(indexPrefix + "_COUNTY_FIPS");

            Map(m => m.ObservedNumerator);
            Map(m => m.ObservedDenominator);
            Map(m => m.ObservedRate).Scale(9).Nullable();
            Map(m => m.ObservedCIHigh).Scale(9).Nullable();
            Map(m => m.ObservedCILow).Scale(9).Nullable();
            Map(m => m.RiskAdjustedRate).Scale(9).Nullable();
            Map(m => m.RiskAdjustedCIHigh).Scale(9).Nullable();
            Map(m => m.RiskAdjustedCILow).Scale(9).Nullable();
            Map(m => m.ExpectedRate).Scale(9).Nullable();
            Map(m => m.StandardErr).Scale(9).Nullable();
            Map(m => m.Threshold);
            Map(m => m.NatBenchmarkRate).Scale(7).Nullable();
            Map(m => m.NatRating).Length(30);
            Map(m => m.PeerBenchmarkRate).Scale(7).Nullable();
            Map(m => m.PeerRating).Length(20);
            Map(m => m.TotalCost).Scale(2).Nullable();

            //Map(m => m.Dataset.Id, "Dataset_Id")
            //References(m => m.Dataset, "Dataset_Id")
            //    .Cascade.All().Not.Insert().Not.Update().ReadOnly();
        }

        /// <summary>
        /// Overrides NameMap.
        /// </summary>
        /// <returns></returns>
        protected override PropertyPart NameMap()
        {
            return null;
        }
    }
}
