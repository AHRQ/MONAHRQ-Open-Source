using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Wing.MedicareProviderCharge
{
    /// <summary>
    /// Model class for Medicare provider charge
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecord" />
    [Serializable, EntityTableName("Targets_MedicareProviderChargeTargets")]
    [WingTarget("Medicare Provider Charge Data", Constants.WING_TARGET_GUID, "Mapping target for Medicare Provider Charge Data", false, 6,
        PublisherName = "Agency for Healthcare Research and Quality (AHRQ)",
        PublisherEmail = "moanhrq@ahrq.gov",
        PublisherWebsite = "http://monahrq.ahrq.gov/")]
    public class MedicareProviderChargeTarget : DatasetRecord
    {
        /// <summary>
        /// Gets or sets the DRG identifier.
        /// </summary>
        /// <value>
        /// The DRG identifier.
        /// </value>
        public virtual int DRG_Id { get; set; }
        /// <summary>
        /// Gets or sets the DRG.
        /// </summary>
        /// <value>
        /// The DRG.
        /// </value>
        [Required, WingTargetElement("DRG Definition", true, 0)]
        public virtual string DRG { get; set; }
        /// <summary>
        /// Gets or sets the provider identifier.
        /// </summary>
        /// <value>
        /// The provider identifier.
        /// </value>
        [Required, WingTargetElement("Provider Id", true, 1)]
        public virtual string ProviderId { get; set; }
        //public virtual string ProviderStateAbbrev { get; set; }
        /// <summary>
        /// Gets or sets the total discharges.
        /// </summary>
        /// <value>
        /// The total discharges.
        /// </value>
        [Required, WingTargetElement("Total Discharges", true, 8)]
        public virtual int? TotalDischarges { get; set; }
        /// <summary>
        /// Gets or sets the average covered charges.
        /// </summary>
        /// <value>
        /// The average covered charges.
        /// </value>
        [Required, WingTargetElement("Average Covered Charges", true, 9)]
        public virtual double? AverageCoveredCharges { get; set; }
        /// <summary>
        /// Gets or sets the average total payments.
        /// </summary>
        /// <value>
        /// The average total payments.
        /// </value>
        [Required, WingTargetElement("Average Total Payments", true, 10)]
        public virtual double? AverageTotalPayments { get; set; }

        #region overrides

        /// <summary>
        /// The name
        /// </summary>
        private string _name;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name
        {
            get
            {
                if (_name.IsNullOrEmpty())
                {
                    _name = this.DRG;
                }
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        #endregion
    }

    
    static class XHelpers
    {
        /// <summary>
        /// To the measure.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="target">The target.</param>
        /// <param name="code">The code.</param>
        /// <param name="measureType">Type of the measure.</param>
        /// <returns></returns>
        public static Measure ToMeasure(this XElement element, Target target, string code, Type measureType)
        {
            var result = Measure.CreateMeasure(measureType, target, code); // new Measure(target, code);

            if (result.MeasureTitle == null) result.MeasureTitle = new MeasureTitle();

            result.MeasureTitle.Plain = element.Attribute("name").Value;
            result.MeasureTitle.Clinical = element.Attribute("name").Value;
            result.MeasureTitle.Policy = element.Attribute("name").Value;
            result.Description = element.Attribute("name").Value;

            return result;
        }
    }

    /// <summary>
    /// Class for Medicare provider charge
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecordMap{Monahrq.Wing.MedicareProviderCharge.MedicareProviderChargeTarget}" />
    public class MedicareProviderChargeTargetMap : DatasetRecordMap<MedicareProviderChargeTarget>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MedicareProviderChargeTargetMap"/> class.
        /// </summary>
        public MedicareProviderChargeTargetMap()
        {
            Map(x => x.DRG_Id).Not.Nullable().Index("IX_MEDICARE_PROVIDER_DRG_ID"); ;
            Map(x => x.DRG).Not.Nullable().Index("IX_MEDICARE_PROVIDER_DRG");
            Map(x => x.ProviderId, "Provider_Id").Not.Nullable().Index("IX_MEDICARE_PROVIDER_ID");
            //Map(x => x.ProviderStateAbbrev, "Provider_StateAbbrev").Nullable().Index("IX_MEDICARE_PROVIDER_STATEABBREV");
            Map(x => x.TotalDischarges).Nullable().Index("IX_MEDICARE_TOTALDISCHARGES");
            Map(x => x.AverageCoveredCharges).CustomSqlType("decimal");
            Map(x => x.AverageTotalPayments).CustomSqlType("decimal");
        }

        /// <summary>
        /// Names the map.
        /// </summary>
        /// <returns></returns>
        protected override FluentNHibernate.Mapping.PropertyPart NameMap()
        {
            return null;
        }
    }
}