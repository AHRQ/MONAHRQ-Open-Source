using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Core.Attributes;
using NHibernate.Type;

namespace Monahrq.Wing.HospitalCompare
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecord" />
	[Serializable]
    [EntityTableName("Targets_HospitalCompareTargets")]
    [WingTarget("Hospital Compare Data", "f74f81e4-be20-41ca-abda-d8bada166fed", "Mapping target for Hospital Compare Data", false, 5,
        PublisherName = "Agency for Healthcare Research and Quality (AHRQ)",
        PublisherEmail = "moanhrq@ahrq.gov",
        PublisherWebsite = "http://monahrq.ahrq.gov/")]
    public class HospitalCompareTarget : DatasetRecord
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="HospitalCompareTarget"/> class.
		/// </summary>
		public HospitalCompareTarget()
        {}

		/// <summary>
		/// Gets or sets the CMS provider identifier.
		/// </summary>
		/// <value>
		/// The CMS provider identifier.
		/// </value>
		[StringLength(12), WingTargetElement("CMSProviderID", false, 11)]
        public virtual string CMSProviderID { get; set; }

		//[WingTargetElement("Provider", false, 1)]
		//public virtual HospitalCompareHospital Provider { get; set; }

		/// <summary>
		/// Gets or sets the condition code.
		/// </summary>
		/// <value>
		/// The condition code.
		/// </value>
		[WingTargetElement("ConditionCode", false, 2)]
        public virtual int? ConditionCode { get; set; }

		/// <summary>
		/// Gets or sets the measure code.
		/// </summary>
		/// <value>
		/// The measure code.
		/// </value>
		[StringLength(25, MinimumLength = 1), WingTargetElement("MeasureCode", false, 3)]
        public virtual string MeasureCode { get; set; }

		/// <summary>
		/// Gets or sets the category code.
		/// </summary>
		/// <value>
		/// The category code.
		/// </value>
		[WingTargetElement("CategoryCode", false, 4)]
        public virtual int? CategoryCode { get; set; }

		/// <summary>
		/// Gets or sets the rate.
		/// </summary>
		/// <value>
		/// The rate.
		/// </value>
		[WingTargetElement("Rate", false, 5)]
        public virtual decimal? Rate { get; set; }
		// public virtual double? Rate { get; set; }

		/// <summary>
		/// Gets or sets the sample.
		/// </summary>
		/// <value>
		/// The sample.
		/// </value>
		[WingTargetElement("Sample", false, 6)]
        public virtual int? Sample { get; set; }

		/// <summary>
		/// Gets or sets the lower.
		/// </summary>
		/// <value>
		/// The lower.
		/// </value>
		[WingTargetElement("Lower", false, 7)]
        public virtual decimal? Lower { get; set; }
		//public virtual double? Lower { get; set; }

		/// <summary>
		/// Gets or sets the upper.
		/// </summary>
		/// <value>
		/// The upper.
		/// </value>
		[WingTargetElement("Upper", false, 8)]
        public virtual decimal? Upper { get; set; }
		// public virtual double? Upper { get; set; }

		/// <summary>
		/// Gets or sets the footnote.
		/// </summary>
		/// <value>
		/// The footnote.
		/// </value>
		[WingTargetElement("Footnote", false, 9)]
        public virtual HospitalCompareFootnote Footnote { get; set; }

		/// <summary>
		/// Gets or sets the note.
		/// </summary>
		/// <value>
		/// The note.
		/// </value>
		[StringLength(5), WingTargetElement("Note", false, 10)]
        public virtual string Note { get; set; }

		/// <summary>
		/// Gets or sets the benchmark identifier.
		/// </summary>
		/// <value>
		/// The benchmark identifier.
		/// </value>
		[StringLength(15), WingTargetElement("BenchmarkID", false, 11)]
        public virtual string BenchmarkID { get; set; }
    }

	/// <summary>
	/// Fluent NHibernate mapper for HopsitalCompareTarget table.
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecordMap{Monahrq.Wing.HospitalCompare.HospitalCompareTarget}" />
	public class HospitalCompareTargetMap : DatasetRecordMap<HospitalCompareTarget>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="HospitalCompareTargetMap"/> class.
		/// </summary>
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

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecord" />
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

	/// <summary>
	/// Fluent NHibernate mapper for  HospitalCompareFootnote table.
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecordMap{Monahrq.Wing.HospitalCompare.HospitalCompareFootnote}" />
	public class HospitalCompareFootnoteMap : DatasetRecordMap<HospitalCompareFootnote>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="HospitalCompareFootnoteMap" /> class.
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
