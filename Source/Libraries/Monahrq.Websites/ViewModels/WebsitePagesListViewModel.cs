using PropertyChanged;
using System.ComponentModel.Composition;
using System;
using Monahrq.Infrastructure.Entities.Domain.WebsiteContentManagement;
using System.Collections.Generic;
using System.Linq.Expressions;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using System.Linq;
using Microsoft.Practices.Prism.Regions;
using System.Windows.Data;
using System.Collections.ObjectModel;
using Monahrq.Infrastructure.Services;
using Monahrq.Infrastructure.Extensions;
using System.IO;
using System.Windows;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Websites.Views;
using System.Globalization;

namespace Monahrq.Websites.ViewModels
{
	[Export(typeof(WebsitePagesListViewModel))]
	//[PartCreationPolicy(CreationPolicy.Shared)]
	[ImplementPropertyChanged]
	public class WebsitePagesListViewModel : WebsiteTabViewModel
	{
		#region Properties.
		public IWebBrowserProxy WebBrowserProxy { get; set; }
		public IPreviewSettings PreviewSettingsProxy { get; internal set; }

		#region WebsitePage Properties.
		/// <summary>
		/// WebsitePages -	Holds the entire list of editable Website Pages (for this ViewModel).  Includes
		///					custom (CurrentWebsite.WebsitePages) and non-Custom (Base) data.
		/// WebsitePagesView - Proxy around WebsitePages that is used on the actual display (.xaml).
		/// CurrentWebsite.WebsitePages - The collection of WebsitePages that have actual custom data.
		/// </summary>
		public List<WebsitePageModel> WebsitePageModels { get; set; }
		public ListCollectionView WebsitePageModelsView { get; set; }
		public bool CanPreviewWebsitePage { get { return true; } }
		#endregion

		#region Query Properties.
		private Expression<Func<WebsitePage, bool>> WebsitePageQueryExpression
		{
			get
			{
				Expression<Func<WebsitePage, bool>> criteria = wp => true;// wp == CurrentWebsite.Id;
			//	Expression<Func<WebsitePage, bool>> criteria = wp => wp.Owner.Id == CurrentWebsite.Id;
				return criteria;
			}
		}
		private Expression<Func<BaseWebsitePage, bool>> BDWebsitePageQueryExpression
		{
			get
			{
				Expression<Func<BaseWebsitePage, bool>> criteria = bdwp => true;
				return criteria;
			}
		}
		#endregion

		#region Filter Properties.
		private string _propertyFilterInputText;
		private string _selectedPropertyFilter;
		private string _selectedPageTypeFilter;
		private string _selectedAudienceFilter;

		public ListCollectionView PropertyFilterTypes { get; set; }
		public ListCollectionView PageTypeFilterTypes { get; set; }
		public ListCollectionView AudienceFilterTypes { get; set; }
		public string SelectedPropertyFilter
		{
			get { return _selectedPropertyFilter; }
			set
			{
				if (value != _selectedPropertyFilter)
				{
					_selectedPropertyFilter = value;
					OnFilterSearch();
				}
			}
		}
		public string SelectedPageTypeFilter
		{
			get { return _selectedPageTypeFilter; }
			set
			{
				if (value != _selectedPageTypeFilter)
				{
					_selectedPageTypeFilter = value;
					OnFilterSearch();
				}
			}
		}
		public string SelectedAudienceFilter
		{
			get { return _selectedAudienceFilter; }
			set
			{
				if (value != _selectedAudienceFilter)
				{
					_selectedAudienceFilter = value;
					OnFilterSearch();
				}
			}
		}
		public string PropertyFilterInputText
		{
			get { return _propertyFilterInputText; }
			set
			{
				if (value != _propertyFilterInputText)
				{
					_propertyFilterInputText = value;
					OnFilterSearch();
				}
			}
		}
		#endregion

		#region Preview Properties.
		public bool IsPreviewOpen { get; private set; }
		private ReportViewModel previewReport;
		public ReportViewModel PreviewReport
		{
			get { return previewReport; }
			private set
			{
				previewReport = value;
				if (previewReport == null) return;

				ActivePreviewImageTabIndex =
					previewReport.Report.HasProfessionalsAudience ? 0 :
					previewReport.Report.HasConsumersAudience ? 1 : 0;
			}
		}
		public int ActivePreviewImageTabIndex { get; set; }

		public string PreviewDemoUrl { get; set; }
		public bool IsPreviewDemoOpen { get; private set; }
		public bool IsPreviewDemoSettingsOpen { get; private set; }
		public WebsitePageModel ActivePreviewWebsitePageModel { get; set; }

		//public WebsitePage SelectedPreviewWebsitePage { get; set; }
		private WebsitePage _selectedPreviewWebsitePage;
		public WebsitePage SelectedPreviewWebsitePage
		{
			get { return _selectedPreviewWebsitePage; }
			set { _selectedPreviewWebsitePage = value; }
		}
		#endregion

		private WebsitePagesViewModel ParentModel
		{
			get
			{
				return ParentViewModel as WebsitePagesViewModel;
				//return ManageViewModel.WebsiteSettingsViewModel.WebsitePagesViewModel;
			}
		}

		#endregion

		#region Methods.
		#region Constructors.
		public WebsitePagesListViewModel()
		{
			WebsitePageModels = new List<WebsitePageModel>();
			WebsitePageModelsView = new ListCollectionView(WebsitePageModels);
		}
		#endregion

		#region State Methods.
		public override void Continue()
		{
			throw new NotImplementedException();
		}

		public override void Save()
		{
			// Try this...
			// TODO: Fix if doesn't work, copy from WebsitePageViewModel page.
			ManageViewModel.ExecuteSaveCommand();


			//	string message = string.Empty;
			//	if (!WebsiteDataService.SaveOrUpdateWebsite(CurrentWebsite))
			//	{
			//		message = CurrentWebsite.IsPersisted
			//						? String.Format("Website {0} has been updated", CurrentWebsite.Name)
			//						: String.Format("Website {0} has been added", CurrentWebsite.Name);
			//	}
			//	
			//	if (!string.IsNullOrEmpty(message))
			//	{
			//		var eventArgs = new ExtendedEventArgs<GenericWebsiteEventArgs>
			//		{
			//			Data = new GenericWebsiteEventArgs { Website = ManageViewModel.WebsiteViewModel, Message = message }
			//		};
			//	
			//		EventAggregator.GetEvent<WebsiteCreatedOrUpdatedEvent>().Publish(eventArgs);
			//	}
		}
		public override void Refresh()
		{
			if (ManageViewModel == null || ManageViewModel.CurrentWebsite == null)
				return;
		}
		#endregion

		#region Initialize Methods.
		private void InitializeWebsitePageView()
		{
			WebsitePageModelsView = WebsitePageModels.ToListCollectionView();
			WebsitePageModelsView.Filter = (item) =>
			{
				var wpm = item as WebsitePageModel;
				return
					DoPropertyFilter(wpm) &&
					DoPageTypeFilter(wpm) &&
					DoAudienceFilter(wpm);
			};
		}
		protected void InitializeControls()
		{
			PropertyFilterTypes = CollectionViewSource.GetDefaultView(
									new ObservableCollection<string> {
									//	WebsitePageModelPropertyFilterValues.NONE,
										WebsitePageModelPropertyFilterValues.PAGE_NAME,
								}) as ListCollectionView;
			PageTypeFilterTypes = CollectionViewSource.GetDefaultView(
									new ObservableCollection<string> {
										WebsitePageModelPageTypeFilterValues.ALL_PAGES,
										WebsitePageModelPageTypeFilterValues.STATIC_PAGES,
										WebsitePageModelPageTypeFilterValues.REPORT_PAGES,
								}) as ListCollectionView;
			AudienceFilterTypes = CollectionViewSource.GetDefaultView(
									new ObservableCollection<string> {
										WebsitePageModelAudienceFilterValues.ALL_AUDIENCES,
										WebsitePageModelAudienceFilterValues.CONSUMER_ONLY,
										WebsitePageModelAudienceFilterValues.PROFESSIONAL_ONLY,
								}) as ListCollectionView;
			SelectedPropertyFilter = WebsitePageModelPropertyFilterValues.PAGE_NAME;
			SelectedPageTypeFilter = WebsitePageModelPageTypeFilterValues.ALL_PAGES;
			SelectedAudienceFilter = WebsitePageModelAudienceFilterValues.ALL_AUDIENCES;
		}
		#endregion

		#region Filter Methods.
		private bool DoPropertyFilter(WebsitePageModel model)
		{
			switch (SelectedPropertyFilter)
			{
			//	case WebsitePageModelPropertyFilterValues.NONE:
			//		return true;
				case WebsitePageModelPropertyFilterValues.PAGE_NAME:
					if (string.IsNullOrEmpty(PropertyFilterInputText)) return true;
					else return model.WebsitePages[0].Name.ToLower().Contains(PropertyFilterInputText.ToLower());
			};
			return true;
		}
		private bool DoPageTypeFilter(WebsitePageModel model)
		{
			switch (SelectedPageTypeFilter)
			{
				case WebsitePageModelPageTypeFilterValues.ALL_PAGES:
					return true;
				case WebsitePageModelPageTypeFilterValues.STATIC_PAGES:
					return model.WebsitePages[0].PageType == WebsitePageTypeEnum.Static;
				case WebsitePageModelPageTypeFilterValues.REPORT_PAGES:
					return model.WebsitePages[0].PageType == WebsitePageTypeEnum.Report;
			};
			return true;
		}
		private bool DoAudienceFilter(WebsitePageModel model)
		{
			switch (SelectedAudienceFilter)
			{
				case WebsitePageModelAudienceFilterValues.ALL_AUDIENCES:
					return true;
				case WebsitePageModelAudienceFilterValues.CONSUMER_ONLY:
					return model.Audiences.Contains(Infrastructure.Entities.Domain.Reports.Audience.Consumers);
				case WebsitePageModelAudienceFilterValues.PROFESSIONAL_ONLY:
					return model.Audiences.Contains(Infrastructure.Entities.Domain.Reports.Audience.Professionals);
			};
			return true;
		}
		#endregion

		#region Load Methods.
		private void LoadWebsitePagesFromWebsiteData()
		{
			//	These pages are already loaded from NHibernate.
			//CurrentWebsite.WebsitePages = WebsiteDataService.GetWebsitePages(WebsitePageQueryExpression).ToList();
		}
		private void LoadAvailableWebsitePagesFromBaseData()
		{
			var baseWebsitePages = WebsiteDataService.GetBaseDataWebsitePages(BDWebsitePageQueryExpression);
			var baseWebsitePageZones = WebsiteDataService.GetBaseDataWebsitePageZones(_ => true);

			foreach (var bWebsitePage in baseWebsitePages)
			{
				//	Find the baseWebsitePage's WebsitePage in AvailableWebsitePages; if cannot find, add it to the AvailableWebsitePages list.
				var aWebsitePageModel = ManageViewModel.AllAvailableWebsitePages.FirstOrDefault(wpm => wpm.WebsitePageName == bWebsitePage.Name);
				if (aWebsitePageModel == null)
				{
					aWebsitePageModel = new WebsitePageModel(new WebsitePage(bWebsitePage),false);
					ManageViewModel.AllAvailableWebsitePages.Add(aWebsitePageModel);
				}
				else
				{
					aWebsitePageModel.WebsitePages.AddIfUnique(new WebsitePage(bWebsitePage), (a, b) => a.Audience == b.Audience);
				}

				//	Find the WebsitePage and BaseWebsitePageZones (using Name[from above] and Audience[below])
				var aWebsitePage = aWebsitePageModel.WebsitePages.First(wp => wp.Audience == bWebsitePage.Audience);
				var bZones = baseWebsitePageZones.Where(bZone => bZone.WebsitePageName == aWebsitePage.Name && bZone.Audience == aWebsitePage.Audience);

				//	Add BaseZones to their respective WebsitePage in AllAvailableWebsitePages.
				foreach (var bZone in bZones)
				{
					var aZone = aWebsitePage.Zones.FirstOrDefault(z => z.Name == bZone.Name);
					if (aZone == null)
					{
						aZone = new WebsitePageZone(bZone);
						aWebsitePage.Zones.Add(aZone);
					}
				}
			}
		}
		private void LoadAvailableWebsitePagesFromReports()
		{
			foreach (var reportModel in ManageViewModel.AllAvailableReports)
			{
				foreach (var rPage in reportModel.Report.WebsitePages)
				{
					//var aWebsitePageModel = ManageViewModel.AllAvailableWebsitePages.FirstOrDefault(wpm => wpm.WebsitePageName == rPage.Name);
					var aWebsitePageModel = ManageViewModel.AllAvailableWebsitePages.FirstOrDefault(wpm =>
					{
						if (wpm.PageType == WebsitePageTypeEnum.Static) return wpm.WebsitePageName == rPage.Name;
						else if (wpm.PageType == WebsitePageTypeEnum.Report) return wpm.ReportName == reportModel.Report.Name;		// rPage.Report.Name;	// For whatever reason, this link is broken. I.e. the ReportPage does not have a link back up to the Report.
						else return false;
					});

					if (aWebsitePageModel == null)
					{
						aWebsitePageModel = new WebsitePageModel();
						ManageViewModel.AllAvailableWebsitePages.Add(aWebsitePageModel);
					}

					aWebsitePageModel.WebsitePages.AddIfUnique(
						new WebsitePage(
								WebsitePageTypeEnum.Report,
								rPage.Name,
								reportModel.Name,
								rPage.Audience,
								rPage.Path,
								rPage.Path,
								rPage.Url,
								rPage.IsEditable),
						(wp, rwp) => wp.Audience == rwp.Audience);

					var aWebsitePage = aWebsitePageModel.WebsitePages.FirstOrDefault(wp => wp.Audience == rPage.Audience);

					foreach (var rZone in rPage.WebsitePageZones)
					{
						var aZone = aWebsitePage.Zones.FirstOrDefault(z => z.Name == rZone.Name);
						if (aZone == null)
						{
							aZone = new WebsitePageZone(
								WebsitePageZoneTypeEnum.Zone,
								rZone.Name,
								rZone.CodePath,
								rPage.Audience);

							aWebsitePage.Zones.Add(aZone);
						}
					}
				}
			}
		}
		private void PopulateUnmappedWebsitePagesData()
		{
			foreach (var cWebsitePage in CurrentWebsite.WebsitePages)
			{
				var aWebsitePageModel = ManageViewModel.AllAvailableWebsitePages.FirstOrDefault(wpm => wpm.WebsitePages.Any(wp => wp.Name == cWebsitePage.Name));
			//	var aWebsitePageModel = ManageViewModel.AllAvailableWebsitePages.FirstOrDefault(wpm => wpm.WebsitePageName == cWebsitePage.Name);
				if (aWebsitePageModel == null) continue;
				var aWebsitePage = aWebsitePageModel.WebsitePages.FirstOrDefault(wp => wp.Audience == cWebsitePage.Audience);
				if (aWebsitePage == null) continue;

				cWebsitePage.TemplateRelativePath = aWebsitePage.TemplateRelativePath;
				cWebsitePage.PublishRelativePath = aWebsitePage.PublishRelativePath;
				cWebsitePage.Url = aWebsitePage.Url;
			//	cWebsitePage.IsEditable = aWebsitePage.IsEditable;

				foreach (var cZone in cWebsitePage.Zones)
				{
					var aZone = aWebsitePage.Zones.FirstOrDefault(z => z.Name == cZone.Name);
					if (aZone == null) continue;
					cZone.CodePath = aZone.CodePath;
				}
			}
		}
		
		private void ComputeWebsitePageList()
		{
			if (CurrentWebsite == null) return;
			//	Start with the loaded 'modified' WebsitePages.
			//	- Care has to be taken that:
			//		A) (for Static Pages,) Pages with the same name get added to the same WebsitePageModel. or 
			//		B) (for Report Pages,) Pages with the same Report get added to the same WebsitePageModel
			//	- ^^ (A) happens because in DB, pages are considered unique per Audience.
			//	- ^^ (B) happens because we currently allow Headers/Footers, to be edited at the Report (per Audience) level.
			WebsitePageModels.Clear();
			CurrentWebsite.WebsitePages.ForEach(wp =>
			{
				//var model = WebsitePageModels.FirstOrDefault(wpm => wpm.WebsitePageName == wp.Name);
				var model = WebsitePageModels.FirstOrDefault(wpm =>
				{
					if (wpm.PageType == WebsitePageTypeEnum.Static)			return wpm.WebsitePageName == wp.Name;
					else if (wpm.PageType == WebsitePageTypeEnum.Report)	return wpm.ReportName == wp.ReportName;
					else													return false;
				});
				if (model == null) { model = new WebsitePageModel(); WebsitePageModels.Add(model); }

				model.WebsitePages.Add(new WebsitePage(wp));
				model.IsModified = true;
			});

			//	Insert any missing/unmodified WebsitePages from Available List (BaseData).
			foreach (var aWebsitePageModel in ManageViewModel.AllAvailableWebsitePages)
			{

				//var oWebsitePageModel = WebsitePageModels.FirstOrDefault(wpm => wpm.WebsitePageName == aWebsitePageModel.WebsitePageName);
				var oWebsitePageModel = WebsitePageModels.FirstOrDefault(wpm =>
				{
					if (wpm.PageType == WebsitePageTypeEnum.Static)			return wpm.WebsitePageName == aWebsitePageModel.WebsitePageName;
					else if (wpm.PageType == WebsitePageTypeEnum.Report)	return wpm.ReportName == aWebsitePageModel.ReportName;
					else													return false;
				});
				if (oWebsitePageModel == null)
				{
					oWebsitePageModel = new WebsitePageModel(aWebsitePageModel);
					WebsitePageModels.Add(oWebsitePageModel);
					continue;	//  We're adding the entire PageModel from BaseData, thus we have all the Pages(by audience) and their zones also.
				}

				foreach (var aPage in aWebsitePageModel.WebsitePages)
				{
					var oWebsitePage = oWebsitePageModel.WebsitePages.FirstOrDefault(wp => wp.Audience == aPage.Audience && wp.Name == aPage.Name);

					if (oWebsitePage == null)
					{
						oWebsitePage = new WebsitePage(aPage);
						oWebsitePageModel.WebsitePages.Add(oWebsitePage);
					}

					foreach (var aZone in aPage.Zones)
					{
						var oZone = oWebsitePage.Zones.FirstOrDefault(c => c.Name == aZone.Name);
						if (oZone == null)
						{
							oZone = new WebsitePageZone(aZone);
							oWebsitePage.Zones.Add(oZone);
						}
					}
				}
			}

			//	Filter out:
			//		Any Pages that are not associated with one of this websites selected Audiences.
			//		Any (Report) Pages that are not associated with one of the websites selected Reports.
			//		Any Model that does not have at least one Editable Page.
			WebsitePageModels = WebsitePageModels.Where(wpm =>
			{
				return
					CurrentWebsite.Audiences.ContainsAny(wpm.Audiences) &&
					(	wpm.WebsitePages[0].PageType == WebsitePageTypeEnum.Static ||
						wpm.WebsitePages[0].PageType == WebsitePageTypeEnum.Report &&
						CurrentWebsite.Reports.Any(r =>
							{
								//	We are forced to check Report.Datasets here because CurrentWebsite.Reports are not kept up to date;
								//	 particularly on Dataset delete.
								return
									wpm.WebsitePages[0].ReportName == r.Report.Name &&
									CurrentWebsite.Datasets.AnyIn(r.Report.Datasets, (cwd, rd) => cwd.Dataset.ContentType.Name == rd);
							})
					) &&
					wpm.WebsitePages.Any(wp => wp.IsEditable);

				//return
				//	CurrentWebsite.Audiences.Any(a => wpm.WebsitePage.Audiences.Contains(a)) &&
				//	CurrentWebsite.Reports.Any(r =>
				//	{
				//		return
				//							wpm.WebsitePage.PageType == WebsitePageTypeEnum.Static ||
				//							wpm.WebsitePage.ReportName == r.Report.Name;
				//	});
			}).ToList();
			
			InitializeWebsitePageView();
		}
		private void LoadWebsitePageDataFromTemplateFiles(WebsitePageModel websitePageModel)
		{
			if (websitePageModel.IsModified) return;

			websitePageModel.WebsitePages.ForEach(websitePage =>
			{
				websitePage.Zones.ForEach(zone =>
				{
					if (!zone.Contents.IsNullOrEmpty()) return;

					switch (zone.ZoneType)
					{
						case WebsitePageZoneTypeEnum.Page:

							var baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
							var templateDirectoryPath = Path.Combine(baseDirectoryPath, "resources\\templates\\site\\");
							var filePath = Path.Combine(templateDirectoryPath, websitePage.TemplateRelativePath);
						//	var filePath = Path.Combine(CurrentWebsite.OutPutDirectory, websitePage.TemplateRelativePath);

							if (!File.Exists(filePath)) return;
							zone.Contents = File.ReadAllText(filePath);
							break;
						case WebsitePageZoneTypeEnum.Zone:
							//throw new NotImplementedException("Not currently possible.");
							break;
					}
				});
			});
		}
		#endregion

		#region Navigation Methods.
		public override void OnNavigatedTo(NavigationContext navigationContext)
		{
			base.OnNavigatedTo(navigationContext);
		}		
		public override void OnHostNavigatedTo(NavigationContext navigationContext)
		{
			base.OnHostNavigatedTo(navigationContext);

			//  Loads WebsitePages from WebsiteData, Reports, and BaseData (In this explicit order.
			//	Reports only loads pages that haven't been loaded thus far.
			//	BaseData only loads pages that haven't been loaded thus far.

			if (ManageViewModel.AllAvailableWebsitePages == null)
				ManageViewModel.AllAvailableWebsitePages = new ObservableCollection<WebsitePageModel>();
			ManageViewModel.AllAvailableWebsitePages.Clear();

		//	LoadWebsitePagesFromWebsiteData();
			LoadAvailableWebsitePagesFromReports();
			LoadAvailableWebsitePagesFromBaseData();
			PopulateUnmappedWebsitePagesData();
			InitializeControls();

		}
		
		public void OnViewLoaded()
		{
			ComputeWebsitePageList();
		}
		public void OnViewUnloaded()
		{
		}
		#endregion

		#region Action Methods.
		public void EditContent(WebsitePageModel activeWebsitePageModel)
		{
			switch (activeWebsitePageModel.PageType)
			{
				case WebsitePageTypeEnum.Static:
					ParentModel.WebsitePagesEditStaticViewModel.ActiveWebsitePageModel = activeWebsitePageModel;
					ParentModel.WebsitePageState = WebsitePageStateEnum.EditStatic;
					LoadWebsitePageDataFromTemplateFiles(activeWebsitePageModel);
					break;
				case WebsitePageTypeEnum.Report:
					ParentModel.WebsitePagesEditReportViewModel.ActiveWebsitePageModel = activeWebsitePageModel;
					ParentModel.WebsitePageState = WebsitePageStateEnum.EditReport;
					LoadWebsitePageDataFromTemplateFiles(activeWebsitePageModel);
					break;
			}
		}

		public void DeleteModifiedWebsitePage(WebsitePageModel activeWebsitePageModel)
		{
			//	Confirmation of deletion.
			//var warningMessage = string.Format("All custom changes for Website Page \"{0}\" will be lost.  Are you sure you want to proceed?", activeWebsitePageModel.WebsitePageName);
			var warningMessage = string.Format("All modifications will be lost.  Are you sure you want to proceed?", activeWebsitePageModel.WebsitePageName);
			var result = MessageBox.Show(warningMessage, "Custom WebsitePage Deletion Confirmation", MessageBoxButton.YesNo);

			if (result == MessageBoxResult.No ||
				result == MessageBoxResult.Cancel ||
				result == MessageBoxResult.None)
				return;

			// Perform Deletion.
			activeWebsitePageModel.WebsitePages.ForEach(wp =>
			{
				CurrentWebsite.WebsitePages.Remove(wp);
				CurrentWebsite.WebsitePages.RemoveAll(cwp => cwp.Name == wp.Name);
				wp.Id = default(int);
				wp.Zones.ForEach(z =>
				{
					z.Id = default(int);
					z.Contents = "";
				});
			});
			activeWebsitePageModel.IsModified = false;
			WebsitePageModelsView.Refresh();
		}
		public void OnPreviewContent(WebsitePageModel activeWebsitePageModel)
		{
			if (activeWebsitePageModel == null) return;
			if (activeWebsitePageModel.WebsitePages == null) return;
			if (activeWebsitePageModel.WebsitePages.Count == 0) return;
			if (activeWebsitePageModel.WebsitePages[0].ReportName.IsNullOrEmpty()) return;

			PreviewReport = ManageViewModel.AllAvailableReports.SingleOrDefault(r => r.Name == activeWebsitePageModel.WebsitePages[0].ReportName);
			IsPreviewOpen = true;
		}
		public void OnClosePreviewContent()
		{
			IsPreviewOpen = false;
			PreviewReport = null;
		}
		public void OnFilterSearch()
		{
			WebsitePageModelsView.Refresh();
		}
		public void OnFilterReset()
		{
			PropertyFilterInputText = string.Empty;
			SelectedPropertyFilter = PropertyFilterTypes.SourceCollection.OfType<string>().FirstOrDefault();
			SelectedPageTypeFilter = PageTypeFilterTypes.SourceCollection.OfType<string>().FirstOrDefault();
			SelectedAudienceFilter = AudienceFilterTypes.SourceCollection.OfType<string>().FirstOrDefault();
		}


		public void OnPreviewWebsitePage(WebsitePageModel activeWebsitePageModel)
		{
			if (activeWebsitePageModel == null) return;
			if (activeWebsitePageModel.WebsitePages == null) return;
			if (activeWebsitePageModel.WebsitePages.Count == 0) return;
			//if (activeWebsitePageModel.WebsitePages[0].ReportName.IsNullOrEmpty()) return;


			ActivePreviewWebsitePageModel = activeWebsitePageModel;
			SelectedPreviewWebsitePage = ActivePreviewWebsitePageModel.WebsitePages[0];
			PreviewSettingsProxy.ResetSettings();

			if (ActivePreviewWebsitePageModel.WebsitePages.Count > 1 || true)
			{
				RaisePropertyChanged(() => SelectedPreviewWebsitePage);
				IsPreviewDemoSettingsOpen = true;
			}
			else
			{
				InternalPreviewWebsitePage(activeWebsitePageModel.WebsitePages[0]);
			}
		}
		public void OnPreviewSelectedWebsitePage()
		{
			IsPreviewDemoSettingsOpen = false;
			InternalPreviewWebsitePage(SelectedPreviewWebsitePage);
		}
		private void InternalPreviewWebsitePage(WebsitePage activeWebsitePage)
		{
			if (activeWebsitePage == null) return;
			
			var ahrqMonahrqDemoSiteUrl = MonahrqConfiguration.SettingsGroup.MonahrqSettings().MonahrqDemoSiteUrl;
			var urlAudience = activeWebsitePage.Audience == Audience.Consumers ? "consumer" : "professional";
			var urlPage = activeWebsitePage.Url.StartsWith("/") ? activeWebsitePage.Url.Substring(1) : activeWebsitePage.Url;
			PreviewDemoUrl = String.Format("{0}/{1}/{2}",
				ahrqMonahrqDemoSiteUrl,
				urlAudience,
				urlPage);

			IsPreviewDemoOpen = true;
		}

		public void OnClosePreviewDemoWebBrowser()
		{
			IsPreviewDemoOpen = false;
		}
		public void OnClosePreviewDemoSettings()
		{
			IsPreviewDemoSettingsOpen = false;
		}
		#endregion
		#endregion
	}


	#region Class Enums.
	//
	public static class WebsitePageModelPropertyFilterValues
	{
	//	public const string NONE = "None";
		public const string PAGE_NAME = "Page Name";
	}
	public static class WebsitePageModelPageTypeFilterValues
	{
		public const string ALL_PAGES = "All Pages";
		public const string REPORT_PAGES = "Pages with Reports";	//"Report Pages";
		public const string STATIC_PAGES = "Static Content Page";   //"Static Pages";
	}
	public static class WebsitePageModelAudienceFilterValues
	{
		public const string ALL_AUDIENCES = "All Pages";
		public const string CONSUMER_ONLY = "Consumer Pages";
		public const string PROFESSIONAL_ONLY = "Healthcare Professional Pages";
	}
	#endregion

	#region Converters.
	public class WebsitePagePreviewDisplayNameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var websitePage = value as WebsitePage;
			if (websitePage == null) return null;

			return String.Format("{0} - [{1}]",
				websitePage.Name,
				websitePage.Audience.GetEnumDescription());
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return DependencyProperty.UnsetValue;
		}
	}
	#endregion
}