using PropertyChanged;
using Monahrq.Default.ViewModels;
using Monahrq.Infrastructure.Entities.Domain.WebsiteContentManagement;
using Monahrq.Infrastructure.Types;
using System;
using System.Collections.Generic;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System.Linq;

namespace Monahrq.Websites.ViewModels
{
	[ImplementPropertyChanged]
	public class WebsitePageModel : BaseNotify, IDeepAssignable<WebsitePageModel>, IDeepCloneable<WebsitePageModel>
	{
		#region Properties.
		public IList<WebsitePage> WebsitePages { get; set; }
		public bool IsModified { get; set; }
		public IList<Audience> Audiences
		{
			get
			{
				var audiences = new List<Audience>();
				WebsitePages.ForEach(wp => audiences.AddIfUnique(wp.Audience));
				audiences.Sort();
				return audiences;
			}
		}
		/// <summary>
		///		<strike>All added WebsitePages *must* have the same Name.</strike>
		/// Above, for now, not true.  For Reports, we are currently grouping Pages by Report(Id/Name) and Audience.
		/// The WebsitePageName (displayed to users for edit) for these will be comma delimited list of all pages.
		/// This is because Header/Footer edits are per Report for Report Pages; not per Page.
		/// </summary>
		public string WebsitePageName
		{
			get
			{
				if (PageType == WebsitePageTypeEnum.Report)
					return String.Join(", ", WebsitePages.Select(wp => wp.Name));
				else
					return WebsitePages[0].Name;
			}
		}
		/// <summary>
		/// All added WebsitePages *must* have the same PageType.
		/// </summary>
		public WebsitePageTypeEnum PageType { get { return WebsitePages[0].PageType; } }
		/// <summary>
		/// All added WebsitePages *must* have the same ReportName.
		/// </summary>
		public string ReportName { get { return WebsitePages[0].ReportName; } }
		#endregion

		#region Methods.
		#region Contstructor Methods.
		public WebsitePageModel()
		{
			WebsitePages = new List<WebsitePage>();
			IsModified = false;
		}
		public WebsitePageModel(WebsitePage websitePage, bool isModified)
		{
			WebsitePages = new List<WebsitePage>() { websitePage };
			IsModified = isModified;
		}
		public WebsitePageModel(WebsitePageModel other)
		{
			WebsitePages = new List<WebsitePage>(other.WebsitePages);
			IsModified = other.IsModified;
		}
		#endregion

		#region Deep Assign/Clone Methods.
		public void DeepAssignmentFrom(WebsitePageModel other)
		{
			WebsitePages.DeepAssignmentFrom(other.WebsitePages, (me,ot)=>me.Name == ot.Name && me.Audience == ot.Audience,true);
			IsModified = other.IsModified;
		}
		public WebsitePageModel DeepClone()
		{
			var clone = new WebsitePageModel();
			clone.WebsitePages = WebsitePages.DeepClone();
			clone.IsModified = IsModified;
			return clone;
		}
		#endregion
		#endregion

	}
}
