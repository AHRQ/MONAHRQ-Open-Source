using Monahrq.Infrastructure.Entities.Domain.Reports;
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
	//public enum BrowserTokenEnum
	//{
	//	Professional,
	//	Consumer
	//}

	/// <summary>
	/// Interaction logic for WebsitePagesEditReportView.xaml
	/// </summary>
	public partial class WebsitePagesEditReportView : UserControl, IWebsitePagesEditView, IWebBrowserProxy, IPreviewSettings
	{
		#region Properties.
		public WebsitePagesEditReportViewModel ViewModel
		{
			get { return DataContext as WebsitePagesEditReportViewModel;  }
		}
		#endregion

		#region Methods.
		#region Constructor Methods.
		public WebsitePagesEditReportView()
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
			//ProfessionalPreviewWebBrowser.Visibility = Visibility.Hidden;
			//ConsumerPreviewWebBrowser.Visibility = Visibility.Hidden;
			HeaderHtmlEditor.Visibility = Visibility.Hidden;
			FooterHtmlEditor.Visibility = Visibility.Hidden;
		}
		public void ShowEditors()
		{
			//ProfessionalPreviewWebBrowser.Visibility = Visibility.Visible;
			//ConsumerPreviewWebBrowser.Visibility = Visibility.Visible;
			HeaderHtmlEditor.Visibility = Visibility.Visible;
			FooterHtmlEditor.Visibility = Visibility.Visible;
		}
		public void AddStyleSheetLink(string cssHref)
		{
			HeaderHtmlEditor.AddStyleSheetLink(cssHref);
			FooterHtmlEditor.AddStyleSheetLink(cssHref);
		}
		public void SetBodyClasses(string classes)
		{
			HeaderHtmlEditor.SetBodyClasses(classes);
			FooterHtmlEditor.SetBodyClasses(classes);
		}
		#endregion

		#region IPreviewSettings Methods.
		public void ResetSettings()
		{
			PreviewSettings.SelectedIndex = 0;
			//rbIsSelected.
		}
		#endregion
		#endregion
	}
}
