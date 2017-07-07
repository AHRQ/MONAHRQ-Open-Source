using System;
using System.Collections.Generic;
using System.ComponentModel;
using Monahrq.Infrastructure.Domain.Common;
using PropertyChanged;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Infrastructure.Entities.Domain.Measures
{
    [Serializable]
	[ImplementPropertyChanged]
    public class MeasureTopic : Entity<int>, ISoftDeletableOnly
	{
		#region Properties.
		public virtual Measure Measure { get; set; }
		public virtual Topic Topic { get; set; }
		public virtual bool UsedForInfographic { get; set; }
		public virtual List<Audience> Audiences { get; set; }
		#endregion

		#region Methods.
		#region Constructors.
		public MeasureTopic(): base()
        {
			Measure = null;
			Topic = null;
			UsedForInfographic = false;
			if (Audiences == null) Audiences = new List<Audience>();
		}
		public MeasureTopic(Measure measure,Topic topic,bool usedForInfographic = false) : base()
		{
			Measure = measure;
			Topic = topic;
			UsedForInfographic = usedForInfographic;
			if (Audiences == null) Audiences = new List<Audience>();
		}
		protected override void Initialize()
        {
            base.Initialize();
        }

		public void AddAudience(Audience audience)
		{
			Audiences.AddIfUnique(audience);
		}
		#endregion
		#endregion
	}
	
}


