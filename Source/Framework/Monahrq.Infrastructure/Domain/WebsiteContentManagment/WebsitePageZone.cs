using System;
using Monahrq.Infrastructure.Data.Conventions;
using PropertyChanged;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Domain.Reports.Validators;
using System.Collections.Generic;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Types;

namespace Monahrq.Infrastructure.Entities.Domain.WebsiteContentManagement
{
	[Serializable]
	[ImplementPropertyChanged]
	[EntityTableName("Websites_WebsitePageZones")]
	public class WebsitePageZone : Entity<int>, IDeepAssignable<WebsitePageZone>, IDeepCloneable<WebsitePageZone>
	{
		#region Properties.
		#region Mapped Properties.
		public Audience Audience { get; set; }
		public virtual String Contents { get; set; }
		public virtual WebsitePageZoneTypeEnum ZoneType { get; set; }
		#endregion

		#region Unmapped Properties.
		public virtual String CodePath { get; set; }
		#endregion
		#endregion

		#region Methods.
		#region Constructors.
		public WebsitePageZone()
		{
			Contents = "";
			Audience = Audience.None;
			ZoneType = WebsitePageZoneTypeEnum.Page;
			CodePath = "";
		}
		public WebsitePageZone(WebsitePageZone other)
		{
			Name = other.Name;
			Contents = other.Contents;
			Audience = other.Audience;
			ZoneType = other.ZoneType;
			CodePath = other.CodePath;
		}
		public WebsitePageZone(BaseWebsitePageZone baseZone)
		{
			Name = baseZone.Name;
			Contents = "";
			Audience = baseZone.Audience;
			ZoneType = baseZone.ZoneType;
			CodePath = baseZone.CodePath;
		}
		public WebsitePageZone(BaseWebsitePageZone baseZone, String contents)
		{
			Name = baseZone.Name;
			Contents = contents;
			Audience = baseZone.Audience;
			ZoneType = baseZone.ZoneType;
			CodePath = baseZone.CodePath;
		}

		public WebsitePageZone(WebsitePageZoneTypeEnum type, string pageZoneName, string codePath, Audience audience)
		{
			Name = pageZoneName;
			Contents = "";
			CodePath = codePath;
			Audience = audience;
			ZoneType = type;
		}

		#endregion

		#region Copy Methods.
		/// <summary>
		/// - Explicitly does not copy the Id.
		/// </summary>
		/// <param name="other"></param>
		public void DeepAssignmentFrom(WebsitePageZone other)
		{
			Name = other.Name;
			Contents = other.Contents;
			Audience = other.Audience;
			ZoneType = other.ZoneType;
			CodePath = other.CodePath;
		}

		/// <summary>
		/// - Not sure if should copy Id yet.
		/// </summary>
		/// <returns></returns>
		public WebsitePageZone DeepClone()
		{
			var clone = new WebsitePageZone();

			clone.Id = Id;
			clone.Name = Name;
			clone.Contents = Contents;
			clone.Audience = Audience;
			clone.ZoneType = ZoneType;
			clone.CodePath = CodePath;

			return clone;
		}
		#endregion
		#endregion
	}
}