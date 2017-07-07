using System;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Wing.NursingHomeCompare.NHCAHPS;

namespace Monahrq.Wing.NursingHomeCompare.Deficiency
{
	//[Serializable, EntityTableName("Targets_NHDeficiencyTargets")]
	//[WingTarget("Nursing Home Deficiency Matrix Data", NHCAHPS.Constants.WING_GUID, "Mapping target for Nursing Home Deficiency Matrix Data", false, false, 9)]
	/// <summary>
	/// DataMode for NHDeficiencyTarget.
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecord" />
	public class NHDeficiencyTarget : DatasetRecord
    {
		#region Properties
		// Required
		/// <summary>
		/// Nursing Home Provider ID. This should match the provider  ID’s for the Medical Practices contained in the Nursing Home  Compare Database
		/// </summary>
		/// <value>
		/// The provider identifier.
		/// </value>
		public string ProviderId { get; set; }

		// Non Required
		/// <summary>
		/// Gets or sets the deficiency care.
		/// </summary>
		/// <value>
		/// The deficiency care.
		/// </value>
		public int? DeficiencyCare { get; set; }

		/// <summary>
		/// Gets or sets the deficiency facility.
		/// </summary>
		/// <value>
		/// The deficiency facility.
		/// </value>
		public int? DeficiencyFacility { get; set; }

		/// <summary>
		/// Gets or sets the deficiency life.
		/// </summary>
		/// <value>
		/// The deficiency life.
		/// </value>
		public int? DeficiencyLife { get; set; }

		/// <summary>
		/// Gets or sets the is abuse.
		/// </summary>
		/// <value>
		/// The is abuse.
		/// </value>
		public bool? IsAbuse { get; set; }

		/// <summary>
		/// Gets or sets the is neglect.
		/// </summary>
		/// <value>
		/// The is neglect.
		/// </value>
		public bool? IsNeglect { get; set; }

		/// <summary>
		/// Gets or sets the is immediate jeopardy.
		/// </summary>
		/// <value>
		/// The is immediate jeopardy.
		/// </value>
		public bool? IsImmediateJeopardy { get; set; }
        #endregion
    }

	/// <summary>
	/// NHibernate mapping file for NHDeficiencyTarget
	/// </summary>
	/// <seealso cref="Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.DatasetRecordMap{Monahrq.Wing.NursingHomeCompare.Deficiency.NHDeficiencyTarget}" />
	public class NHDeficiencyTargetMap : DatasetRecordMap<NHDeficiencyTarget>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="NHDeficiencyTargetMap"/> class.
		/// </summary>
		public NHDeficiencyTargetMap()
        {
            var indexName = string.Format("IDX_{0}", EntityTableName);

            Map(x => x.ProviderId).Length(12).Not.Nullable().Index(indexName);
            Map(x => x.DeficiencyCare).Nullable().Index(indexName);
            Map(x => x.DeficiencyFacility).Nullable();
            Map(x => x.DeficiencyLife).Nullable();
            Map(x => x.IsAbuse).Nullable();
            Map(x => x.IsNeglect).Nullable();
            Map(x => x.IsImmediateJeopardy).Nullable();
        }
    }
}