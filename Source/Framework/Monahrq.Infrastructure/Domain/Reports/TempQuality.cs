using System;
using System.Text;
using System.Collections.Generic;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.Reports
{
    [Serializable]
	[ImplementPropertyChanged]
	public class TempQuality	 : Entity<int>
	{
		public virtual System.Guid ReportId { get; set; }
		public virtual int MeasureId { get; set; }
		public virtual int? HospitalId { get; set; }
		public virtual int? CountyId { get; set; }
		public virtual int? RegionId { get; set; }
		public virtual string ZipCode { get; set; }
		public virtual string HospitalType { get; set; }
		public virtual string RateAndCI { get; set; }
		public virtual string NatRating { get; set; }
		public virtual string NatFilled { get; set; }	// int?
		public virtual string PeerRating { get; set; }
		public virtual string PeerFilled { get; set; }	// int?
		public virtual string Col1 { get; set; }
		public virtual string Col2 { get; set; }
		public virtual string Col3 { get; set; }
		public virtual string Col4 { get; set; }
		public virtual string Col5 { get; set; }
		public virtual string Col6 { get; set; }
		public virtual string Col7 { get; set; }
		public virtual string Col8 { get; set; }
		public virtual string Col9 { get; set; }
		public virtual string Col10 { get; set; }

		[System.ComponentModel.DataAnnotations.Schema.NotMapped]
		public override int Id { get; set; }
		[System.ComponentModel.DataAnnotations.Schema.NotMapped]
		public override String Name { get; set; }
	}

}
