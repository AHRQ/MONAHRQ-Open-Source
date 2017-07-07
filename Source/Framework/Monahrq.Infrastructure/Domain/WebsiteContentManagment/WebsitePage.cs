using System;
using Monahrq.Infrastructure.Data.Conventions;
using PropertyChanged;
using Monahrq.Infrastructure.Domain.Websites;
using System.Collections.Generic;
using BDWebsitePage = Monahrq.Infrastructure.Entities.Domain.BaseData.BaseWebsitePage;
using Monahrq.Infrastructure.Extensions;
using System.Linq;
using Monahrq.Infrastructure.Types;

namespace Monahrq.Infrastructure.Entities.Domain.WebsiteContentManagement
{
	[Serializable]
	[ImplementPropertyChanged]
	[EntityTableName("Websites_WebsitePages")]
	public class WebsitePage : Entity<int>, IDeepAssignable<WebsitePage>, IDeepCloneable<WebsitePage>
//	public class WebsitePage : OwnedEntity<Website,int,int>, IDeepAssignable<WebsitePage>, IDeepCloneable<WebsitePage>
	{
		#region Properties.

		#region Mapped Properties.
		///// <summary>
		///// Gets or sets the Website this WebsitePage belongs to.
		///// </summary>
		//public virtual Website Website { get; set; }

		/// <summary>
		/// Gets or sets the WebsitePageZones belonging to this WebsitePage.
		/// </summary>
		public virtual IList<WebsitePageZone> Zones { get; set; }
		/// <summary>
		/// Gets or sets the PageType.
		/// </summary>
		public virtual BaseData.WebsitePageTypeEnum PageType { get; set; }
		/// <summary>
		/// Gets or sets the ReportName.
		/// </summary>
		public virtual String ReportName { get; set; }
		/// <summary>
		/// Gets or sets the Audience.
		/// </summary>
		public virtual Reports.Audience Audience { get; set; }
		/// <summary>
		/// Gets or sets the IsEditable.
		/// </summary>
		public virtual bool IsEditable { get; set; }
		#endregion

		#region Unmapped Properties.
		/// <summary>
		/// Not Mapped.
		/// </summary>
		public virtual String TemplateRelativePath { get; set; }
		/// <summary>
		/// Not Mapped.
		/// </summary>
		public virtual String PublishRelativePath { get; set; }
		/// <summary>
		/// Not Mapped.
		/// Gets or sets the Url.
		/// </summary>
		public virtual String Url { get; set; }
		#endregion
		#endregion

		#region Methods.
		#region Constructors.
		public WebsitePage()
		{
			Zones = new List<WebsitePageZone>();
		}
		public WebsitePage(
			BaseData.WebsitePageTypeEnum pageType,
			string pageName,
			string reportName,
			Reports.Audience audience,
			string templateRelativePath="",
			string publishRelativePath="",
			string url="",
			bool isEditable=true)
		{
			Name = pageName;
			PageType = pageType;
			ReportName = reportName;
			Zones = new List<WebsitePageZone>();
			Audience = audience;
			TemplateRelativePath = templateRelativePath;
			PublishRelativePath = publishRelativePath;
			Url = url;
			IsEditable = isEditable;
		}
		public WebsitePage(WebsitePage other)
		{
			Name = other.Name;
			PageType = other.PageType;
			ReportName = other.ReportName;
			Audience = other.Audience;
			Zones = new List<WebsitePageZone>(other.Zones);
			TemplateRelativePath = other.TemplateRelativePath;
			PublishRelativePath = other.PublishRelativePath;
			Url = other.Url;
			IsEditable = other.IsEditable;
		}
		public WebsitePage(BDWebsitePage basePage)
		{
			Name = basePage.Name;
			PageType = basePage.PageType;
			ReportName = basePage.ReportName;
			Audience = basePage.Audience;
			Zones = new List<WebsitePageZone>();
			TemplateRelativePath = basePage.TemplateRelativePath;
			PublishRelativePath = basePage.PublishRelativePath;
			Url = basePage.Url;
			IsEditable = basePage.IsEditable;
		}
		#endregion

		#region Copy Methods.
		public void DeepAssignmentFrom(WebsitePage other)
		{
			Name = other.Name;
			PageType = other.PageType;
			ReportName = other.ReportName;
			Audience = other.Audience;
			Zones.DeepAssignmentFrom(
				other.Zones,
				(c,oc) => c.Name == oc.Name);
			TemplateRelativePath = other.TemplateRelativePath;
			PublishRelativePath = other.PublishRelativePath;
			Url = other.Url;
			IsEditable = other.IsEditable;
		}
		public WebsitePage DeepClone()
		{
			var clone = new WebsitePage();
			
			clone.Id = Id;
			clone.Name = Name;
			clone.PageType = PageType;
			clone.ReportName = ReportName;
			clone.Audience = Audience;
			clone.Zones = Zones.DeepClone();
			clone.TemplateRelativePath = TemplateRelativePath;
			clone.PublishRelativePath = PublishRelativePath;
			clone.Url = Url;
			clone.IsEditable = IsEditable;

			return clone;
		}
		#endregion
		#endregion
	}
}