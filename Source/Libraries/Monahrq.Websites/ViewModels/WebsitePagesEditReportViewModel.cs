
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
using Monahrq.Infrastructure.Extensions;
using System.Collections.ObjectModel;
using Monahrq.Websites.Views;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Domain.Websites;
using System.Windows.Data;
using System.Windows.Input;
using Monahrq.Infrastructure.Types;
using Monahrq.Sdk.Services.Generators;
using System.IO;
using Monahrq.Infrastructure;
using Microsoft.Practices.Prism.Logging;

namespace Monahrq.Websites.ViewModels
{
	//[PartCreationPolicy(CreationPolicy.Shared)]
	[ImplementPropertyChanged]
	[Export(typeof(WebsitePagesEditReportViewModel))]
	public class WebsitePagesEditReportViewModel : WebsiteTabViewModel
	{
		#region Types.
		public class WebsitePageZoneContent
		{
			public WebsitePageZone Zone { get; set; }
			public string Contents { get; set; }
		}
		[ImplementPropertyChanged]
		public class MeasureColumn: Default.ViewModels.BaseViewModel
		{
			public ReportColumn ReportColumn { get; set; }
			public WebsiteMeasure WebsiteMeasure { get; set; }

			public string Code
			{
				get
				{
					if (ReportColumn.IsMeasure)
						return WebsiteMeasure.OriginalMeasure.MeasureCode;
					else
						return "";	// ReportColumn.MeasureCode;
				}
			}
			public string Title
			{
				get
				{
					if (ReportColumn.IsMeasure)
					{
						if (WebsiteMeasure.OverrideMeasure != null)
							return WebsiteMeasure.OverrideMeasure.MeasureTitle.Clinical
								.Replace("\r\n", " ")
								.Replace("\r", " ")
								.Replace("\n", " ");
						else
							return WebsiteMeasure.OriginalMeasure.MeasureTitle.Clinical
								.Replace("\r\n", " ")
								.Replace("\r", " ")
								.Replace("\n", " ");
					}
					else
						return ReportColumn.Name;
				}
				set { }
			}
			public bool IsModified
			{
				get
				{
					if (ReportColumn.IsMeasure && WebsiteMeasure.OverrideMeasure != null)
						return
							WebsiteMeasure.OriginalMeasure.MeasureTitle.Plain != WebsiteMeasure.OverrideMeasure.MeasureTitle.Plain ||
							WebsiteMeasure.OriginalMeasure.ConsumerPlainTitle != WebsiteMeasure.OverrideMeasure.ConsumerPlainTitle;
					return false;
				}
				set { }
			}
			public void SubmitPropertyChanged()
			{
				RaisePropertyChanged(() => IsModified);
			}
		}
		#endregion

		#region Properites.
		private WebsitePagesViewModel ParentModel
		{
			get
			{
				return ManageViewModel.WebsiteSettingsViewModel.WebsitePagesViewModel;
			}
		}
		public WebsitePageModel ActiveWebsitePageModel { get; set; }
		public WebsitePage ActiveWebsitePage
		{
			get
			{
				if (ActiveWebsitePageModel == null) return null;
				return ActiveWebsitePageModel.WebsitePages.FirstOrDefault(wp => wp.Audience == SelectedEditAudienceType);
			}
		}


		public IWebBrowserProxy WebBrowserProxy { get; set; }
		public IWebsitePagesEditView WebsitePagesEditView { get; set; }
		public IPreviewSettings PreviewSettingsProxy { get; internal set; }

		#region Zone Properties.
		public ObservableCollection<WebsitePageZoneContent> ZoneContents { get; set; }
		public WebsitePageZoneContent HeaderZone
		{
			get { return ZoneContents.FirstOrDefault(cc => cc.Zone.Name == "Header" && 
					                                       cc.Zone.Audience == SelectedEditAudienceType); }
			set { }
		}
		public WebsitePageZoneContent FooterZone
		{
            get
            {
                return ZoneContents.FirstOrDefault(cc => cc.Zone.Name == "Footer" &&
                                                         cc.Zone.Audience == SelectedEditAudienceType);
			}
			set { }
		}
		#endregion

		#region Column Properties.
		public ObservableCollection<MeasureColumn> MeasureColumns { get; set; }
		public int EditedMeasureColumnsCount
		{
			get { return MeasureColumns.Count(mc => mc.IsModified); }
		}
		private MeasureColumn activeMeasureColumn;
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
					RaisePropertyChanged(() => HeaderZone);
					RaisePropertyChanged(() => FooterZone);
				}
			}
		}
		#endregion

		#region Measure Edit Properties.
		public bool IsEditMeasureWindowOpen { get; set; }
		[Import(RequiredCreationPolicy = CreationPolicy.Shared)]
		public WebsiteEditMeasureViewModel WebsiteEditMeasureViewModel { get; set; }
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
		public WebsitePagesEditReportViewModel()
		{
			ZoneContents = new ObservableCollection<WebsitePageZoneContent>();
			MeasureColumns = new ObservableCollection<MeasureColumn>();
		}
		public override void OnImportsSatisfied()
		{
			base.OnImportsSatisfied();
			WebsiteEditMeasureViewModel.ManageViewModel = ManageViewModel;
			AddSubListTabViewModel(new SubListTabViewModel(WebsiteEditMeasureViewModel) { SyncStateActions = true });
		}
		private void InitializeControls()
		{
			EditAudienceTypes = CollectionViewSource.GetDefaultView(
				ActiveWebsitePageModel.Audiences) as ListCollectionView;
			SelectedEditAudienceType = ActiveWebsitePageModel.Audiences[0];	//.GetDescription();
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
				if (!awp.IsEditable) return;

				var cwp = CurrentWebsite.WebsitePages.FirstOrDefault(wp => wp.Name == awp.Name && wp.Audience == awp.Audience);

				if (cwp == null)
					CurrentWebsite.WebsitePages.Add(awp);
				else if (cwp != awp)
					cwp.DeepAssignmentFrom(awp);
			});
		}
		#endregion

		#region Navigation Methods.
		public void OnViewLoaded()
		{
			if (ActiveWebsitePageModel == null) return;

			ManageViewModel.WebsiteManageViewActionsVisibility = System.Windows.Visibility.Collapsed;
			InitializeControls();

			//	Copy contents of Zones to temp/local list.
			ZoneContents.Clear();
			ActiveWebsitePageModel.WebsitePages.ForEach(wp =>
			{
				wp.Zones.ForEach(c =>
				{
					ZoneContents.Add(new WebsitePageZoneContent()
					{
						Zone = c,
						Contents = c.Contents
					});
				});
			});
			RaisePropertyChanged(() => HeaderZone);
			RaisePropertyChanged(() => FooterZone);

			//	Load Measures associated with current 'ActiveWebsitePageModel'.
			MeasureColumns.Clear();

			if (ActiveWebsitePageModel.PageType == Infrastructure.Entities.Domain.BaseData.WebsitePageTypeEnum.Report &&
				!ActiveWebsitePageModel.ReportName.IsNullOrEmpty())
			{
				var websiteReport = CurrentWebsite.Reports.FirstOrDefault(r => r.Report.Name == ActiveWebsitePageModel.ReportName);
				if (websiteReport != null)
				{
					var allReportColumns = websiteReport.Report.Columns;	//.Where(c => c.IsMeasure);

					var currentReportMeasures =
						allReportColumns.LeftJoin(
							CurrentWebsite.Measures.Where(wm => wm.IsSelected),
							rc => rc.MeasureCode,
							wm => wm.OriginalMeasure.Name,
							(rc,wm) => new MeasureColumn() { WebsiteMeasure = wm, ReportColumn = rc });

					currentReportMeasures = currentReportMeasures.Where(crm => !crm.ReportColumn.IsMeasure || crm.WebsiteMeasure != null);
					currentReportMeasures.ForEach(crm => MeasureColumns.Add(crm));

					RaisePropertyChanged(() => MeasureColumns);
					RaisePropertyChanged(() => EditedMeasureColumnsCount);
				}
			}
		}
		public void OnViewUnloaded()
		{
			try
			{
				//	Reset the Screen View; Navigate to WebsitePagesListView.xaml.
				ParentModel.WebsitePageState = WebsitePageStateEnum.List;
				ManageViewModel.WebsiteManageViewActionsVisibility = System.Windows.Visibility.Visible;

				//	Clear local stuff.
				ActiveWebsitePageModel = null;
				ZoneContents.Clear();
			}
			catch (Exception ex)
			{
				ex.GetType();
			}
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
		#endregion

		#region Action Methods.
		public void EditColumn(MeasureColumn measureColumn)
		{
			IsEditMeasureWindowOpen = true;
			activeMeasureColumn = measureColumn;
			WebsiteEditMeasureViewModel.AssignSelectedMeasure(measureColumn.WebsiteMeasure);
			WebsiteEditMeasureViewModel.MeasureSaved -= OnEditMeasureSaveHandler;
			WebsiteEditMeasureViewModel.MeasureEditCancelled -= OnEditMeasureCancelHandler;
			WebsiteEditMeasureViewModel.MeasureSaved += OnEditMeasureSaveHandler;
			WebsiteEditMeasureViewModel.MeasureEditCancelled += OnEditMeasureCancelHandler;
			WebsiteEditMeasureViewModel.SetViewMode(WebsiteEditMeasureViewModeEnum.TitleOnly);

			WebsitePagesEditView.HideEditors();
		}
		private void OnEditMeasureSaveHandler()
		{
			//MeasureColumns.FirstOrDefault(mc => mc.WebsiteMeasure.ReportMeasure.Name == WebsiteEditMeasureViewModel.SelectedMeasure.Measure.Name).SubmitPropertyChanged();
			activeMeasureColumn.SubmitPropertyChanged();
			activeMeasureColumn = null;

			IsEditMeasureWindowOpen = false;
			WebsiteEditMeasureViewModel.MeasureSaved -= OnEditMeasureSaveHandler;
			WebsiteEditMeasureViewModel.MeasureEditCancelled -= OnEditMeasureCancelHandler;
			WebsiteEditMeasureViewModel.SetViewMode(WebsiteEditMeasureViewModeEnum.Normal);
			WebsitePagesEditView.ShowEditors();
		}
		private void OnEditMeasureCancelHandler()
		{
			IsEditMeasureWindowOpen = false;
			WebsiteEditMeasureViewModel.MeasureSaved -= OnEditMeasureSaveHandler;
			WebsiteEditMeasureViewModel.MeasureEditCancelled -= OnEditMeasureCancelHandler;
			WebsiteEditMeasureViewModel.SetViewMode(WebsiteEditMeasureViewModeEnum.Normal);
			WebsitePagesEditView.ShowEditors();
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
				var urlPage = previewWebsitePage.Url.StartsWith("/") ? previewWebsitePage.Url.Substring(1) : previewWebsitePage.Url;
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
			catch (System.Runtime.InteropServices.COMException comEx)
			{
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
		#endregion
		#endregion
	}
}