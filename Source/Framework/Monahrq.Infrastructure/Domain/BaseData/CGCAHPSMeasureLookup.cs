using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{

	[ImplementPropertyChanged]
	public class CGCAHPSMeasureLookup : Entity<int>
	{

		public virtual String MeasureId { get; set; }
		public virtual String MeasureType { get; set; }
		public virtual String CAHPSQuestionType { get; set; }
	}
}
