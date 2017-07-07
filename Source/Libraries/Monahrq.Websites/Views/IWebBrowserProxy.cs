
namespace Monahrq.Websites.Views
{
	public interface IWebBrowserProxy
	{
		void NavigateTo(string url);
		void GoBack();
		void GoForward();
		void Refresh();
	}

	public interface IWebBrowserProxy<E>
	{
		void NavigateTo(E browserToken, string url);
		void GoBack(E browserToken);
		void GoForward(E browserToken);
		void Refresh(E browserToken);
	}
	public interface IPreviewSettings
	{
		void ResetSettings();
	}
}
