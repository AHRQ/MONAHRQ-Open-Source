using Monahrq.Websites.ViewModels;

namespace Monahrq.Websites.Views
{
    public interface IWebsitePagesEditView
	{
		void HideEditors();
		void ShowEditors();
		void AddStyleSheetLink(string cssHref);
		void SetBodyClasses(string classes);
	}
}
