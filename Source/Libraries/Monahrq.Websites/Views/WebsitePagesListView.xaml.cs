using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using Monahrq.Websites.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Monahrq.Websites.Views
{
	/// <summary>
	/// Interaction logic for WebsitePagesListView.xaml
	/// </summary>
	//[Export(typeof(WebsitePagesListView))]
	[ViewExport(typeof(WebsitePagesListView), RegionName = RegionNames.WebsitePagesListRegion)]
	public partial class WebsitePagesListView : UserControl, IWebBrowserProxy, IPreviewSettings
	{

		#region Properties.
		[Import(RequiredCreationPolicy = CreationPolicy.Shared)]
		public WebsitePagesListViewModel ViewModel
		{
			get
			{
				return DataContext as WebsitePagesListViewModel;
			}
			set
			{
				DataContext = value;
			}
		}
		#endregion
		
		#region Methods.
		#region Constructor Methods.
		public WebsitePagesListView()
		{
			InitializeComponent();
			Loaded += WebsitePagesListView_Loaded;
			Unloaded += WebsitePagesListView_Unloaded;
		}
		private void WebsitePagesListView_Loaded(object sender, RoutedEventArgs e)
		{
			ViewModel.WebBrowserProxy = this;
			ViewModel.PreviewSettingsProxy = this;
		}
		private void WebsitePagesListView_Unloaded(object sender, RoutedEventArgs e)
		{
		}
		#endregion
		
		#region Browser Navigation.
		public void GoBack()
		{
			throw new NotImplementedException();
		}
		public void GoForward()
		{
			throw new NotImplementedException();
		}
		public void NavigateTo(string url)
		{
			DemoPreviewWebBrowser.Navigate(url);
		}
		public void Refresh()
		{
			DemoPreviewWebBrowser.Refresh();
		}
		#endregion

		public void ResetSettings()
		{
			PreviewSettings.SelectedIndex = 0;
			//rbIsSelected.
		}
		#endregion

	}
}
