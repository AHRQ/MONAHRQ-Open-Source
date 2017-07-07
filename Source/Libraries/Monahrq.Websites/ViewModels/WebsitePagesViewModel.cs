
using System;
using PropertyChanged;
using System.ComponentModel.Composition;
using System.Linq.Expressions;
using Monahrq.Infrastructure.Entities.Domain.WebsiteContentManagement;
using BDWebsitePage = Monahrq.Infrastructure.Entities.Domain.BaseData.BaseWebsitePage;
using System.Collections.Generic;
using System.Linq;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Websites.Events;
using Microsoft.Practices.Prism.Regions;

namespace Monahrq.Websites.ViewModels
{
	[Export(typeof(WebsitePagesViewModel))]
    //[PartCreationPolicy(CreationPolicy.Shared)]
	[ImplementPropertyChanged]
	public class WebsitePagesViewModel : WebsiteTabViewModel
	{
		#region Methods.
		#region Constructor Methods.
		public WebsitePagesViewModel()
		{
			Initialize();
		}
		public override void OnImportsSatisfied()
		{
			base.OnImportsSatisfied();
			WebsitePagesListViewModel.ManageViewModel = ManageViewModel;
			WebsitePagesEditStaticViewModel.ManageViewModel = ManageViewModel;
			WebsitePagesEditReportViewModel.ManageViewModel = ManageViewModel;

			AddSubListTabViewModel(new SubListTabViewModel(WebsitePagesListViewModel) { SyncStateActions = true });
			AddSubListTabViewModel(new SubListTabViewModel(WebsitePagesEditStaticViewModel) { SyncStateActions = true });
			AddSubListTabViewModel(new SubListTabViewModel(WebsitePagesEditReportViewModel) { SyncStateActions = true });

		}
		public void Initialize()
		{
			WebsitePageState = WebsitePageStateEnum.List;
		}
		#endregion

		#region State Methods.
		public override void Continue()
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Saves the complete Website object to DB.
		/// </summary>
		public override void Save()
		{
			string message = string.Empty;
			if (!WebsiteDataService.SaveOrUpdateWebsite(CurrentWebsite))
			{
				message = CurrentWebsite.IsPersisted
								? String.Format("Website {0} has been updated", CurrentWebsite.Name)
								: String.Format("Website {0} has been added", CurrentWebsite.Name);
			}

			if (!string.IsNullOrEmpty(message))
			{
				var eventArgs = new ExtendedEventArgs<GenericWebsiteEventArgs>
				{
					Data = new GenericWebsiteEventArgs { Website = ManageViewModel.WebsiteViewModel, Message = message }
				};

				EventAggregator.GetEvent<WebsiteCreatedOrUpdatedEvent>().Publish(eventArgs);
			}
		}
		#endregion
		#endregion

		#region Properties.
		#region ViewModel Properties.
		[Import(RequiredCreationPolicy = CreationPolicy.Shared)]
		public WebsitePagesListViewModel WebsitePagesListViewModel { get; set; }
        [Import(RequiredCreationPolicy = CreationPolicy.Shared)]
		public WebsitePagesEditStaticViewModel WebsitePagesEditStaticViewModel { get; set; }
		[Import(RequiredCreationPolicy = CreationPolicy.Shared)]
		public WebsitePagesEditReportViewModel WebsitePagesEditReportViewModel { get; set; }
		#endregion

		#region ViewModel Properties.
		public WebsitePageStateEnum WebsitePageState { get; set; }
		#endregion
		#endregion
	}

	public enum WebsitePageStateEnum
	{
		List,
		EditStatic,
		EditReport,
	}
}
