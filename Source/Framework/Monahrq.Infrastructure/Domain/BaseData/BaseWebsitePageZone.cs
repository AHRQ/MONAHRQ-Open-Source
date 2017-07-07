using System;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using System.Collections.Generic;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{

    [Serializable]
	[ImplementPropertyChanged]
    [EntityTableName("Base_WebsitePageZones")]
    public class BaseWebsitePageZone : Entity<int>
    {
		#region Methods.
		public BaseWebsitePageZone()
		{
			//Audiences = new List<Audience>();
		}
		#endregion

		#region Properties.
		/// <summary>
		/// The name of the WebsitePage this WebsitePageZone is hosted on.
		/// </summary>
		public virtual String WebsitePageName { get; set; }
		/// <summary>
		/// The name of this WebsitePageZone.
		/// </summary>
		//public virtual String Name { get; set; }
		/// <summary>
		/// The 'Code' path depicting the location of this WebsitePageZone
		/// within it's hosting WebsitePage.
		/// </summary>
		public virtual String CodePath { get; set; }
		/// <summary>
		/// ...
		/// </summary>
		public virtual WebsitePageZoneTypeEnum ZoneType { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public virtual Audience Audience { get; set; }
		#endregion
	}
	
	public enum WebsitePageZoneTypeEnum
	{
		Zone,
		Page
	}
}
