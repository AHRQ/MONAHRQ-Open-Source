using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Core.Attributes;
using NHibernate.Type;

namespace Monahrq.Wing.HospitalCompare
{
    [Serializable, EntityTableName("Targets_NursingHomeCompareTargets")]
    [WingTarget("Nursing Home Compare Data", "3A005AB9-847C-4515-B8DB-EF6D98252CB5", "Mapping target for Nursing Home Compare Data", false, 5)]
    
    public class HospitalCompareTarget : DatasetRecord
    {
        public HospitalCompareTarget()
        {}

        [StringLength(12), WingTargetElement("CMSProviderID", false, 11)]
        public virtual string CMSProviderID { get; set; }

        //[WingTargetElement("Provider", false, 1)]
        //public virtual HospitalCompareHospital Provider { get; set; }

        [WingTargetElement("ConditionCode", false, 2)]
        public virtual int? ConditionCode { get; set; }

        [StringLength(25, MinimumLength = 1), WingTargetElement("MeasureCode", false, 3)]
        public virtual string MeasureCode { get; set; }

        [WingTargetElement("CategoryCode", false, 4)]
        public virtual int? CategoryCode { get; set; }

        [WingTargetElement("Rate", false, 5)]
        public virtual decimal? Rate { get; set; }
        // public virtual double? Rate { get; set; }

        [WingTargetElement("Sample", false, 6)]
        public virtual int? Sample { get; set; }

        [WingTargetElement("Lower", false, 7)]
        public virtual decimal? Lower { get; set; }
        //public virtual double? Lower { get; set; }

        [WingTargetElement("Upper", false, 8)]
        public virtual decimal? Upper { get; set; }
        // public virtual double? Upper { get; set; }

        [WingTargetElement("Footnote", false, 9)]
        public virtual HospitalCompareFootnote Footnote { get; set; }

        [StringLength(5), WingTargetElement("Note", false, 10)]
        public virtual string Note { get; set; }

        [StringLength(15), WingTargetElement("BenchmarkID", false, 11)]
        public virtual string BenchmarkID { get; set; }
    }

    public class HospitalCompareTargetMap : DatasetRecordMap<HospitalCompareTarget>
    {
        public HospitalCompareTargetMap()
        {
            Map(m => m.CMSProviderID).Length(12).Index("IDX_CMS_PROVIDER_ID");
            Map(m => m.ConditionCode).Nullable();
            Map(m => m.MeasureCode).Length(25).Index("IDX_MEASURE_CODE");
            Map(m => m.CategoryCode).Nullable();
            Map(m => m.Rate).Scale(3).Nullable();
            Map(m => m.Sample).Nullable();
            Map(m => m.Lower).Scale(2).Nullable();
            Map(m => m.Upper).Scale(2).Nullable();
            Map(m => m.Note).Length(5);
            Map(m => m.BenchmarkID).Length(15);

            //References(m => m.Provider, "Provider_Id")
            //    .Cascade.All()
            //    .Not.LazyLoad();

            References(m => m.Footnote, "Footnote_Id")
                .Cascade.All()
                .Not.LazyLoad();
        }
    }

    //[Serializable, EntityTableName("Targets_HospitalCompareHospitals")]
    //public class HospitalCompareHospital : ContentPartRecord
    //{
    //    /// <summary>
    //    /// Gets or sets the provider number.
    //    /// </summary>
    //    /// <value>
    //    /// The provider number.
    //    /// </value>
    //    [StringLength(12, MinimumLength = 1), WingTargetElement("ProviderNumber", false, 1)]
    //    public virtual string ProviderNumber { get; set; }
    //    /// <summary>
    //    /// Gets or sets the name.
    //    /// </summary>
    //    /// <value>
    //    /// The name.
    //    /// </value>
    //    [StringLength(80, MinimumLength = 1), WingTargetElement("Name", false, 1)]
    //    public override string Name { get; set; }
    //    /// <summary>
    //    /// Gets or sets the city.
    //    /// </summary>
    //    /// <value>
    //    /// The city.
    //    /// </value>
    //    [StringLength(35, MinimumLength = 1), WingTargetElement("City", false, 1)]
    //    public virtual string City { get; set; }
    //    /// <summary>
    //    /// Gets or sets the state.
    //    /// </summary>
    //    /// <value>
    //    /// The state.
    //    /// </value>
    //    [StringLength(2, MinimumLength = 1), WingTargetElement("State", false, 1)]
    //    public virtual string State { get; set; }
    //    /// <summary>
    //    /// Gets or sets the zip.
    //    /// </summary>
    //    /// <value>
    //    /// The zip.
    //    /// </value>
    //    [StringLength(5, MinimumLength = 1), WingTargetElement("Zip", false, 1)]
    //    public virtual string Zip { get; set; }
    //    /// <summary>
    //    /// Gets or sets the county.
    //    /// </summary>
    //    /// <value>
    //    /// The county.
    //    /// </value>
    //    [StringLength(35, MinimumLength = 1), WingTargetElement("County", false, 1)]
    //    public virtual string County { get; set; }
    //    /// <summary>
    //    /// Gets or sets the fips.
    //    /// </summary>
    //    /// <value>
    //    /// The fips.
    //    /// </value>
    //    [StringLength(5, MinimumLength = 1), WingTargetElement("FIPS", false, 1)]
    //    public virtual string FIPS { get; set; }
    //}

    //public class HospitalCompareHospitalMap : ContentPartRecordMap<HospitalCompareHospital>
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="HospitalCompareHospitalMap"/> class.
    //    /// </summary>
    //    public HospitalCompareHospitalMap()
    //    {
    //        Map(m => m.ProviderNumber).Length(12);
    //        Map(m => m.Name).Length(80);
    //        Map(m => m.City).Length(35);
    //        Map(m => m.State, "[State]").Length(2);
    //        Map(m => m.Zip).Length(5);
    //        Map(m => m.County).Length(35);
    //        Map(m => m.FIPS).Length(5);
    //    }
    //}

    [Serializable, EntityTableName("Targets_HospitalCompareFootnotes")]
    public class HospitalCompareFootnote : DatasetRecord
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [StringLength(12, MinimumLength = 1), WingTargetElement("Name", false, 1)]
        public override string Name { get; set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [StringLength(10000, MinimumLength = 1), WingTargetElement("Description", false, 1)]
        public virtual string Description { get; set; }
        /// <summary>
        /// Gets the hospital compare data target.
        /// </summary>
        /// <value>
        /// The hospital compare data target.
        /// </value>
        [WingTargetElement("HospitalCompareDataTarget", false, 1)]
        public virtual IList<HospitalCompareTarget> HospitalCompareDataTarget
        {
            get;
            private set;
        }
    }

    public class HospitalCompareFootnoteMap : DatasetRecordMap<HospitalCompareFootnote>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalCompareFootnoteMap"/> class.
        /// </summary>
        public HospitalCompareFootnoteMap()
        {
            Map(m => m.Name).Length(12).Index("IDX_NAME");
            Map(m => m.Description).CustomSqlType("nvarchar(max)");

            HasMany(m => m.HospitalCompareDataTarget)
                .KeyColumn("Footnote_Id")
                .ReadOnly()
                .AsBag();
        }
    }
}
