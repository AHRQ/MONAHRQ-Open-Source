using System;
using PropertyChanged;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Entities.Domain.WebsiteContentManagement;
using System.Linq;
using Monahrq.Infrastructure.Extensions;
using System.Collections.ObjectModel;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using System.Windows.Data;
using Monahrq.Infrastructure.Types;
using System.Windows.Input;
using Monahrq.Infrastructure;
using Monahrq.Sdk.Services.Generators;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Websites.Views;
using System.IO;
using System.Collections.Generic;

namespace Monahrq.Websites.ViewModels
{
    [Export(typeof(WebsitePagesEditStaticViewModel))]
	//[PartCreationPolicy(CreationPolicy.Shared)]
	[ImplementPropertyChanged]
	public class WebsitePagesEditStaticViewModel : WebsiteTabViewModel
	{
		#region Types.
		public class WebsitePageZoneContent
		{
			public WebsitePageZone Zone { get; set; }
			public String Contents { get; set; }
		}

		#endregion

		#region Properites.
		private WebsitePagesViewModel ParentModel
		{
			get
			{
				return ParentViewModel as WebsitePagesViewModel;
				//return ManageViewModel.WebsiteSettingsViewModel.WebsitePagesViewModel;
			}
		}
		public WebsitePageModel ActiveWebsitePageModel { get; set; }
		public WebsitePage ActiveWebsitePage
		{
			get
			{
				return ActiveWebsitePageModel.WebsitePages.FirstOrDefault(wp => wp.Audience == SelectedEditAudienceType);
			}
		}
		public IWebBrowserProxy WebBrowserProxy { get; internal set; }
		public IWebsitePagesEditView WebsitePagesEditView { get; set; }
		public IPreviewSettings PreviewSettingsProxy { get; internal set; }

		#region Zone Properties.
		public ObservableCollection<WebsitePageZoneContent> ZoneContents { get; set; }
		public WebsitePageZoneContent CurrentZoneContent
		{
			get
			{
				return ZoneContents.FirstOrDefault(cc => cc.Zone.Audience == SelectedEditAudienceType);
			}
			//set { }
		}
		#endregion

		#region Audience Properties.
		private Audience _selectedEditAudienceType;
		public ListCollectionView EditAudienceTypes { get; set; }
		public Audience SelectedEditAudienceType
		{
			get { return _selectedEditAudienceType; }
			set
			{
				if (value != _selectedEditAudienceType)
				{
					_selectedEditAudienceType = value;

					switch (value)
					{
						case Audience.Consumers: WebsitePagesEditView.SetBodyClasses("consumer"); break;
						case Audience.Professionals: WebsitePagesEditView.SetBodyClasses("professional"); break;
					}

					RaisePropertyChanged(() => CurrentZoneContent);
					RaisePropertyChanged(() => ZoneContents);
					RaisePropertyChanged(() => ActiveWebsitePage);


				}
			}
		}
		#endregion

		#region Preview Settings Properties.
		public bool IsPreviewSettingsOpen { get; set; }
		public List<WebsitePage> AvailablePreviewWebsitePages
		{
			get
			{
				if (ActiveWebsitePageModel == null) return null;
				return ActiveWebsitePageModel.WebsitePages.Where(wp => wp.Audience == SelectedEditAudienceType).ToList();
			}
		}
		public WebsitePage SelectedPreviewWebsitePage { get; set; }
		#endregion

		#region Preview Properties.
		public bool IsPreviewWebBrowserOpen { get; private set; }
		[Import]
		public WebsiteGenerator Generator { get; set; }
		public bool CanPreviewWebsitePage
		{
			get
			{
				if (ActiveWebsitePage == null) return false;
				var filePath = Path.Combine(CurrentWebsite.OutPutDirectory, ActiveWebsitePage.TemplateRelativePath);
				return File.Exists(filePath);
			}
		}
		#endregion

		#endregion

		#region Methods.
		#region Constructor Methods.
		public WebsitePagesEditStaticViewModel()
		{
			ZoneContents = new ObservableCollection<WebsitePageZoneContent>();
		}
		private void InitializeControls()
		{
			SetupHtmlEditors();
			EditAudienceTypes = CollectionViewSource.GetDefaultView(
				ActiveWebsitePageModel.Audiences) as ListCollectionView;
			SelectedEditAudienceType = ActiveWebsitePageModel.Audiences[0];//.GetDescription();
			RaisePropertyChanged(() => SelectedEditAudienceType);
		}
		#endregion

		#region State Methods.
		public override void Continue()
		{
			//throw new NotImplementedException();
		}
		public override void Save()
		{
			ParentModel.Save();
		}
		private void CommitToCurrentWebsite()
		{
			//	Copy each Contents to Active...Model
			ZoneContents.ForEach(cc => { cc.Zone.Contents = cc.Contents; });

			//	Mark Model as modified.
			ActiveWebsitePageModel.IsModified = true;

			//	ActiveWebsitePageModel (or it's WebsitePage, or it's Zones) may not be a part of the
			//	CurrentWebsite object; it could be a 'local' member of the DataGrid list of the ListView screen.
			//	In such a case, it is required that the ActiveWebsitePageModel.*WebsitePage.*Zones be copied
			//	to the CurrentWebsite->WebsitePages list.
			//	1)	(if dbWS==null)	Reuse the exact object in the ListView Screen.  (It is already created, and unattached/non-conflicted
			//						to anything DB related)
			//	2)	(else if)		Not same object.  Some how we have different objects, just reassign the data.  [Probably should never happen]
			//	3)	(else)			ActiveWebsitePageModel is the exact object in CurrentWebsite.WebsitePages....  So
			//						we have already assigned everything we need to.

			ActiveWebsitePageModel.WebsitePages.ForEach(awp =>
			{
				var cwp = CurrentWebsite.WebsitePages.FirstOrDefault(wp => wp.Name == awp.Name && wp.Audience == awp.Audience);

				if (cwp == null)
					CurrentWebsite.WebsitePages.Add(awp);
				else if (cwp != awp)
					cwp.DeepAssignmentFrom(awp);
			});
		}
		#endregion

		#region Actions & Navigation Methods.
		public void OnViewLoaded()
		{
			ManageViewModel.WebsiteManageViewActionsVisibility = System.Windows.Visibility.Collapsed;
			InitializeControls();

			//	Copy contents of Zones to temp/local list.
			//ActiveWebsitePageModel = ReferenceWebsitePageModel.DeepClone();
			ZoneContents.Clear();
			ActiveWebsitePageModel.WebsitePages.ForEach(wp =>
			{ 
				wp.Zones.ForEach(z =>
				{
					ZoneContents.Add(new WebsitePageZoneContent
					{
						Zone = z,
						Contents = z.Contents
					});
				});
			});
			RaisePropertyChanged(() => ZoneContents);
			RaisePropertyChanged(() => CurrentZoneContent);
		}
		public void OnViewUnloaded()
		{
			//	Reset the Screen View; Navigate to WebsitePagesListView.xaml.
			ParentModel.WebsitePageState = WebsitePageStateEnum.List;
			ManageViewModel.WebsiteManageViewActionsVisibility = System.Windows.Visibility.Visible;
			
			//	Clear local stuff.
			ActiveWebsitePageModel = null;
			ZoneContents.Clear();
		}
		public void OnEditCancel()
		{
			OnViewUnloaded();
		}
		public void OnEditSave()
		{
			CommitToCurrentWebsite();

			//	Navigate Away.
			OnViewUnloaded();
		}

		#region Preview Action Methods.
		public void OnPreviewWebsitePage()
		{
			if (AvailablePreviewWebsitePages == null) return;
			if (AvailablePreviewWebsitePages.Count == 0) return;

			SelectedPreviewWebsitePage = AvailablePreviewWebsitePages[0];

			PreviewSettingsProxy.ResetSettings();
			if (AvailablePreviewWebsitePages.Count > 1 || true)
			{
				RaisePropertyChanged(() => SelectedPreviewWebsitePage);
				IsPreviewSettingsOpen = true;
				WebsitePagesEditView.HideEditors();
			}
			else
			{
				InternalPreviewWebsitePage(SelectedPreviewWebsitePage);
			}
		}
		public void OnPreviewSelectedWebsitePage()
		{
			IsPreviewSettingsOpen = false;
			InternalPreviewWebsitePage(SelectedPreviewWebsitePage);
		}
		private void InternalPreviewWebsitePage(WebsitePage previewWebsitePage)
		{
			try
			{
				//	Save data to DB so WebsiteGenerator can pick up changes.
				CommitToCurrentWebsite();
				Save();

				//	Generate CMSZoneEntities.js file; also disable navigation.
				using (ApplicationCursor.SetCursor(Cursors.Wait))
				{
					EventAggregator.GetEvent<DisableNavigationEvent>()
								   .Publish(new DisableNavigationEvent { DisableUIElements = true });
					Generator.Publish(CurrentWebsite, PublishTask.BaseCMSZoneOnly);
					EventAggregator.GetEvent<DisableNavigationEvent>()
								   .Publish(new DisableNavigationEvent { DisableUIElements = false });
				}

				//	Compute url to navigate to.
				var urlAudience = SelectedEditAudienceType == Audience.Consumers ? "consumer" : "professional";
				var urlPage = previewWebsitePage.Url.StartsWith("/") ?
					previewWebsitePage.Url.Substring(1) :
					previewWebsitePage.Url;
				var previewUrl =    //"http://www.google.com"
					String.Format(
						"file:///{0}/index.html#/{1}/{2}",
							CurrentWebsite.OutPutDirectory.Replace("\\", "/").Trim(),
							urlAudience,
							urlPage);

				//	Show WebBrowser window and navigate to the correct page.
				WebBrowserProxy.NavigateTo(previewUrl);
				WebsitePagesEditView.HideEditors();
				IsPreviewWebBrowserOpen = true;
				WebBrowserProxy.Refresh();
			}
			catch (Exception exc)
			{
				Logger.Log(exc.Message, Category.Exception, Priority.High);
			}
		}
		public void OnClosePreviewSettings()
		{
			WebsitePagesEditView.ShowEditors();
			IsPreviewSettingsOpen = false;
		}
		public void OnClosePreviewWebBrowser()
		{
			//	Show WebBrowser window and navigate to the correct page.
			WebsitePagesEditView.ShowEditors();
			IsPreviewWebBrowserOpen = false;
		}
		#endregion

		private void SetupHtmlEditors()
		{
			var websiteDir = CurrentWebsite.OutPutDirectory.Replace("\\", "/").Trim();

			var cssHrefs = new List<String>();

			cssHrefs.Add("https://fonts.googleapis.com/css?family=Droid+Sans:400,700,400italic");
			cssHrefs.Add("https://fonts.googleapis.com/css?family=Source+Sans+Pro:400,600,700,900,400italic,600italic");
			cssHrefs.Add("https://fonts.googleapis.com/css?family=Open+Sans:600italic,700italic,700,600,400");
			cssHrefs.Add(websiteDir + "/" + "app/vendor/bower_components/jquery-ui/themes/ui-lightness/jquery-ui.css");
			cssHrefs.Add(websiteDir + "/" + "themes/base/css/bootstrap.css");
			cssHrefs.Add(websiteDir + "/" + "themes/base/css/bootstrap-theme.css");
			cssHrefs.Add(websiteDir + "/" + "themes/professional/css/professional.css");
			cssHrefs.Add(websiteDir + "/" + "themes/professional/css/user-settings.css");
			cssHrefs.Add(websiteDir + "/" + "themes/consumer/css/consumer.css");

			foreach (var cssHref in cssHrefs)
			{
				WebsitePagesEditView.AddStyleSheetLink(cssHref);
			}
		}
		#endregion
		#endregion
	}
}