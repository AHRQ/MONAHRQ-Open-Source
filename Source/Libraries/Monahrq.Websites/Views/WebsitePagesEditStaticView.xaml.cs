using Monahrq.Websites.ViewModels;
using System;
using System.Collections.Generic;
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
	/// Interaction logic for WebsitePagesEditStaticView.xaml
	/// </summary>
	public partial class WebsitePagesEditStaticView : UserControl, IWebsitePagesEditView, IWebBrowserProxy, IPreviewSettings
	{

		#region Properties.
		public WebsitePagesEditStaticViewModel ViewModel
		{
			get { return DataContext as WebsitePagesEditStaticViewModel; }
		}
		#endregion

		#region Methods.
		#region Consturctor Methods.
		public WebsitePagesEditStaticView()
		{
			InitializeComponent();
		}
		#endregion


		#region View Load Methods.
		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			ViewModel.WebBrowserProxy = this;
			ViewModel.WebsitePagesEditView = this;
			ViewModel.PreviewSettingsProxy = this;
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
			PreviewWebBrowser.Navigate(url);
		}
		public void Refresh()
		{
			PreviewWebBrowser.Refresh();
		}
		#endregion

		#region Editor Mehtods.
		public void HideEditors()
		{
			StaticHtmlEditor.Visibility = Visibility.Hidden;
		}
		public void ShowEditors()
		{
			StaticHtmlEditor.Visibility = Visibility.Visible;
		}
		public void AddStyleSheetLink(string cssHref)
		{
			StaticHtmlEditor.AddStyleSheetLink(cssHref);
		}
		public void SetBodyClasses(string classes)
		{
			StaticHtmlEditor.SetBodyClasses(classes);
		}
		#endregion

		#region IPreviewSettings Methods.
		public void ResetSettings()
		{
			PreviewSettings.SelectedIndex = 0;
		}
		#endregion

		#endregion
	}
}
