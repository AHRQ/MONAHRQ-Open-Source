using System;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;
using System.Collections.Generic;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using System.ComponentModel;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{

    [Serializable]
	[ImplementPropertyChanged]
    [EntityTableName("Base_WebsitePages")]
    public class BaseWebsitePage : Entity<int>
    {
		/// <summary>
		/// The IO path depicting the location of the 'sourcing' template file of this WebsitePage.
		/// </summary>
		public virtual String TemplateRelativePath { get; set; }
		/// <summary>
		/// The IO path depicting the location of the 'published' file of this WebsitePage.
		/// </summary>
		public virtual String PublishRelativePath { get; set; }
		/// <summary>
		/// ...
		/// </summary>
		public virtual WebsitePageTypeEnum PageType { get; set; }
		/// <summary>
		/// Gets or sets the ReportName.
		/// </summary>
		public virtual String ReportName { get; set; }
		/// <summary>
		/// Gets or sets the Audience.
		/// </summary>
		public virtual Audience Audience { get; set; }
		/// <summary>
		/// Gets or sets the Url.
		/// </summary>
		public virtual String Url { get; set; }
		/// <summary>
		/// Gets or sets the Url.
		/// </summary>
		public virtual bool IsEditable { get; set; }
	}
	
	public enum WebsitePageTypeEnum
	{
		[Description("None")]
		None,
		[Description("Pages with Reports")]
		Report,
		[Description("Static Content Page")]
		Static,
	}
}
