using Monahrq.Sdk.Generators;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Wing.Physician.Reports
{
	/// <summary>
	/// Generates the report data/.json files for HEDIS reports, a sub report of the Physician Report.
	/// </summary>
	/// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
	[Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    [ReportGenerator(new[] { "4C5727B4-0E85-4F80-ADE9-418B49A1373E" },
     new string[] { }, new[] { typeof(HEDIS.HEDISTarget) }, 8, "Medical Practice HEDIS Data for Physician Profile")]
    public class HEDISReportGenerator : BaseReportGenerator
    {

		#region Fields and Constants

		/// <summary>
		/// The hedis measures folder
		/// </summary>
		private string _hedisMeasuresFolder;
		/// <summary>
		/// The measure description ns
		/// </summary>
		private const string _measureDescriptionNS = "$.monahrq.Physicians.Base.MeasureDescriptions";
		/// <summary>
		/// The measure topics ns
		/// </summary>
		private const string _measureTopicsNS = "$.monahrq.Physicians.Base.MeasureTopics";
		/// <summary>
		/// The measure descriptions
		/// </summary>
		private List<MeasureDescriptionStruct> _measureDescriptions;
		/// <summary>
		/// The included hedisps measures
		/// </summary>
		private List<Measure> _includedHEDISPSMeasures;
		/// <summary>
		/// The measure topics
		/// </summary>
		private List<MeasureTopicStruct> _measureTopics;

		#endregion

		#region Properties

		#endregion

		#region Constructor


		#endregion

		#region Methods

		/// <summary>
		/// Initializes the generator.
		/// </summary>
		public override void InitGenerator()
        {

        }

		/// <summary>
		/// Loads the report data.
		/// </summary>
		/// <returns></returns>
		protected override bool LoadReportData()
        {
            _hedisMeasuresFolder = Path.Combine(BaseDataDirectoryPath, "Base", "PhysicianMeasures");

            return true;
        }

		/// <summary>
		/// Outputs the data files.
		/// </summary>
		/// <returns></returns>
		protected override bool OutputDataFiles()
        {
            _measureDescriptions.ForEach(md =>
            {
                GenerateJsonFile(md, Path.Combine(_hedisMeasuresFolder, string.Format("MeasureDescription_{0}.js", md.MeasureId)), string.Format("{0}['{1}'] =", _measureDescriptionNS, md.MeasureId));
            });

            GenerateJsonFile(_measureTopics, Path.Combine(_hedisMeasuresFolder, "MeasureTopics.js"), string.Format("{0} =", _measureTopicsNS));

            return true;
        }

		/// <summary>
		/// Validates the dependencies.
		/// </summary>
		/// <param name="website">The website.</param>
		/// <param name="validationResults">The validation results.</param>
		/// <returns></returns>
		public override bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
        {
            if (base.ValidateDependencies(website, validationResults))
            {
                var isDatasetIncluded = CurrentWebsite.Datasets.Count(x => x.Dataset.ContentType.Name.ContainsIgnoreCase("Medical Practice HEDIS Measures Data") || x.Dataset.ContentType.Name.ContainsIgnoreCase("Physician Data")) > 1;
                if (!isDatasetIncluded)
                {
                    validationResults.Add(new ValidationResult("The medical practice HEDIS report data could not be generated since either the Medical Practice HEDIS Measures Data and/or Physician Data was not included in website. Skipping generation."));
                }
                else
                {
                    var reportAttributeSelected = CurrentWebsite.Reports.Any(x => x.Report.Filters.Any(f => f.Values.Any(v => v.Value && v.Name == "HEDIS Measures")));
                    if (!reportAttributeSelected) return false;

                    _includedHEDISPSMeasures = CurrentWebsite.Measures.Where(m => m.ReportMeasure.Source != null && m.ReportMeasure.Source.ContainsIgnoreCase("Physician HEDIS"))
                                                             .Select(x => x.ReportMeasure)
                                                             .ToList();

                    _measureDescriptions = _includedHEDISPSMeasures.Select(m => new MeasureDescriptionStruct(m)).ToList();

                    _measureTopics = new List<MeasureTopicStruct>
                    {
                        new MeasureTopicStruct
                        {
                            TopicId = 1,
                            Name = "hedis topic",
                            LongTitle = "hedis topic",
                            Description = "Lorem ipsum",
                            MeasureIDs = _includedHEDISPSMeasures.Select(x => x.Id).ToList()
                        }
                    };
                }


                if (!_measureDescriptions.Any()) validationResults.Add(new ValidationResult("You have not selected Medical Practice HEDIS Measures. Medical Practice HEDIS report could not be generated."));

            }

            return validationResults == null || validationResults.Count == 0;

        }

		/// <summary>
		/// Checks if can run.
		/// </summary>
		/// <returns></returns>
		public override bool CheckIfCanRun()
        {
            return CurrentWebsite != null && 
                   (CurrentWebsite.Datasets != null && 
                   CurrentWebsite.Datasets.Any(x => x.Dataset.ContentType.Name.ContainsIgnoreCase("Medical Practice HEDIS Measures Data")));
        }

        #endregion
    }

	/// <summary>
	/// 
	/// </summary>
	[DataContract]
    public struct MeasureDescriptionStruct
    {
		/// <summary>
		/// Gets or sets the measure identifier.
		/// </summary>
		/// <value>
		/// The measure identifier.
		/// </value>
		[DataMember(Name = "MeasureID")]
        public int MeasureId { get; set; }

		/// <summary>
		/// Gets or sets the name of the measures.
		/// </summary>
		/// <value>
		/// The name of the measures.
		/// </value>
		[DataMember(Name = "MeasuresName")]
        public string MeasuresName { get; set; }

		/// <summary>
		/// Gets or sets the measure source.
		/// </summary>
		/// <value>
		/// The measure source.
		/// </value>
		[DataMember(Name = "MeasureSource")]
        public string MeasureSource { get; set; }

		/// <summary>
		/// Gets or sets the type of the measure.
		/// </summary>
		/// <value>
		/// The type of the measure.
		/// </value>
		[DataMember(Name = "MeasureType")]
        public string MeasureType { get; set; }

		/// <summary>
		/// Gets or sets the selected title.
		/// </summary>
		/// <value>
		/// The selected title.
		/// </value>
		[DataMember(Name = "SelectedTitle")]
        public string SelectedTitle { get; set; }

		/// <summary>
		/// Gets or sets the plain title.
		/// </summary>
		/// <value>
		/// The plain title.
		/// </value>
		[DataMember(Name = "PlainTitle")]
        public string PlainTitle { get; set; }

		/// <summary>
		/// Gets or sets the clinical title.
		/// </summary>
		/// <value>
		/// The clinical title.
		/// </value>
		[DataMember(Name = "ClinicalTitle")]
        public string ClinicalTitle { get; set; }

		/// <summary>
		/// Gets or sets the measure description.
		/// </summary>
		/// <value>
		/// The measure description.
		/// </value>
		[DataMember(Name = "MeasureDescription")]
        public string MeasureDescription { get; set; }

		/// <summary>
		/// Gets or sets the selected title consumer.
		/// </summary>
		/// <value>
		/// The selected title consumer.
		/// </value>
		[DataMember(Name = "SelectedTitleConsumer")]
        public string SelectedTitleConsumer { get; set; }

		/// <summary>
		/// Gets or sets the plain title consumer.
		/// </summary>
		/// <value>
		/// The plain title consumer.
		/// </value>
		[DataMember(Name = "PlainTitleConsumer")]
        public string PlainTitleConsumer { get; set; }

		/// <summary>
		/// Gets or sets the measure description consumer.
		/// </summary>
		/// <value>
		/// The measure description consumer.
		/// </value>
		[DataMember(Name = "MeasureDescriptionConsumer")]
        public string MeasureDescriptionConsumer { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MeasureDescriptionStruct"/> struct.
		/// </summary>
		/// <param name="measure">The measure.</param>
		public MeasureDescriptionStruct(Measure measure) : this()
        {
            MeasureId = measure.Id;
            MeasuresName = measure.MeasureCode;
            MeasureSource = measure.Source;
            MeasureType = measure.MeasureType;
            SelectedTitle = measure.MeasureTitle.Selected == SelectedMeasuretitleEnum.Plain ? measure.MeasureTitle.Plain : measure.MeasureTitle.Clinical;
            PlainTitle = measure.MeasureTitle.Plain;
            ClinicalTitle = measure.MeasureTitle.Clinical;
            MeasureDescription = measure.Description;
            SelectedTitleConsumer = measure.ConsumerPlainTitle;
            PlainTitleConsumer = measure.ConsumerPlainTitle;
            MeasureDescriptionConsumer = measure.ConsumerDescription;
        }
    }

	/// <summary>
	/// 
	/// </summary>
	[DataContract]
    public struct MeasureTopicStruct
    {
		/// <summary>
		/// Gets or sets the topic identifier.
		/// </summary>
		/// <value>
		/// The topic identifier.
		/// </value>
		[DataMember(Name = "TopicID")]
        public int TopicId { get; set; }
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		[DataMember(Name = "Name")]
        public string Name { get; set; }
		/// <summary>
		/// Gets or sets the long title.
		/// </summary>
		/// <value>
		/// The long title.
		/// </value>
		[DataMember(Name = "LongTitle")]
        public string LongTitle { get; set; }
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
		[DataMember(Name = "Description")]
        public string Description { get; set; }
		/// <summary>
		/// Gets or sets the measure i ds.
		/// </summary>
		/// <value>
		/// The measure i ds.
		/// </value>
		[DataMember(Name = "MeasureIDs")]
        public List<int> MeasureIDs { get; set; }

    }
}
