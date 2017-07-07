using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Measures.Fields;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Infrastructure.Entities.Domain.Measures
{
	[Serializable, EntityTableName("Measures")]
	public class Measure : OwnedEntity<Target, int, int>
	{
		public Measure()
		{ }

		public Measure(Target target, string measureCode)
			: base(target)
		{
			target.Measures.Add(this);
			Name = measureCode;
		}

		public virtual string MeasureCode
		{
			get { return Name; }
		}

		// e.g. PQI1
		public virtual string MeasureType { get; set; }
		public virtual bool IsOverride { get; set; }
		public virtual bool IsLibEdit { get; set; }

		//[XmlIgnore]
		//public virtual IList<Topic> Topics { get; set; }
		[XmlIgnore]
		public IReadOnlyCollection<Topic> Topics
		{
			get
			{
				if (MeasureTopics == null) return null;
				return MeasureTopics.Select(mt => mt.Topic).ToList().AsReadOnly();
			}
		}
		[XmlIgnore]
		public virtual IList<MeasureTopic> MeasureTopics { get; set;}
        public virtual List<string> WebsitesForMeasureDisplay { get; set; }

        public virtual string TopicsForMeasureDisplay { get; set; }

        #region MEASURE_FIELDS      // all measure fields are nullable

        public virtual string Source { get; set; }
        public virtual string Description { get; set; }
        public virtual string Footnotes { get; set; }
        public virtual bool HigherScoresAreBetter { get; set; }
        public virtual decimal? UpperBound { get; set; }
        public virtual decimal? LowerBound { get; set; }
        public virtual MeasureTitle MeasureTitle { get; set; }
        public virtual string MoreInformation { get; set; }
        public virtual decimal? NationalBenchmark { get; set; }
        public virtual StatePeerBenchmark StatePeerBenchmark { get; set; }

        public virtual decimal? SuppressionDenominator { get; set; }
        public virtual decimal? SuppressionNumerator { get; set; }
        public virtual bool PerformMarginSuppression { get; set; }
        public virtual string Url { get; set; }
        public virtual string UrlTitle { get; set; }
        public virtual decimal? ScaleBy { get; set; }
        public virtual string ScaleTarget { get; set; }
        public virtual string RiskAdjustedMethod { get; set; }
        public virtual string RateLabel { get; set; }
        public virtual bool NQFEndorsed { get; set; }
        public virtual string NQFID { get; set; }

        public virtual bool UsedInCalculations { get; set; }
        [XmlIgnore]
        public virtual EditRule EditRule { get; set; }
        [XmlIgnore]
        public virtual ValidationRule ValidationRule { get; set; }
        public virtual bool SupportsCost { get; set; }
        public virtual string ConsumerDescription { get; set; }
        public virtual string ConsumerPlainTitle { get; set; }

        #endregion

        protected override void Initialize()
        {
            MeasureTopics = new List<MeasureTopic>();
            MeasureTitle = new MeasureTitle();
            StatePeerBenchmark = new StatePeerBenchmark();
            WebsitesForMeasureDisplay = new List<string>();
        }

        public Measure Clone(bool cloneForOverride = false)
        {
            var clonedMeasure = this.Clone<Measure, int>(false);

            clonedMeasure.MeasureTopics = new List<MeasureTopic>();
            foreach (var measureTopic in MeasureTopics.ToList())
            {
                var t = new MeasureTopic()
                {
					Name = measureTopic.Name,
					Measure = measureTopic.Measure,
					Topic = measureTopic.Topic,
					Id = measureTopic.Id		// Want this?					
                };

                clonedMeasure.MeasureTopics.Add(t);
            }

            clonedMeasure.IsOverride = cloneForOverride;

            return clonedMeasure;
        }

        public static Measure CreateMeasure(Type measureType, Target target, string measureCode)
        {
            //var measure = new Measure(target, measureCode);

            return (Measure)Activator.CreateInstance(measureType, target, measureCode);
        }

		public void AddTopic(Topic topic,bool usedForInfographic=false)
		{
			var measureTopic = MeasureTopics.FirstOrDefault(mt => mt.Topic.Name == topic.Name);

			if (measureTopic != null)
			{
				measureTopic.UsedForInfographic = usedForInfographic;
				measureTopic.Topic = topic;
				return;
			}

			MeasureTopics.Add(new MeasureTopic(this, topic, usedForInfographic));
		}
		public void RemoveTopic(Topic topic)
		{
			MeasureTopics.RemoveAll(mt => mt.Topic == topic);
		}
		public void ClearTopics()
		{
			MeasureTopics.ForEach(mt =>
			{
				mt.Topic.Measures.Remove(this);
				mt.Measure = null;
				mt.Topic = null;
			});
			MeasureTopics.Clear();
		}
    }

    [Serializable]
    public enum SelectedMeasuretitleEnum
    {
        Plain = 0,
        Clinical
    }

    [Serializable]
    public class MeasureTitle
    {
        public virtual string Plain { get; set; }
        public virtual string Policy { get; set; }
        public virtual string Clinical { get; set; }
        public virtual SelectedMeasuretitleEnum Selected { get; set; }
    }

    [Serializable]
    public enum StatePeerBenchmarkCalculationMethod { Calculated_Mean, Calculated_Median, Provided };

    [Serializable]
    public class StatePeerBenchmark
    {
        public virtual decimal? ProvidedBenchmark { get; set; }
        public virtual StatePeerBenchmarkCalculationMethod CalculationMethod { get; set; }
    }
}
