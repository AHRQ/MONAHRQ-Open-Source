using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Websites.ViewModels;

namespace Monahrq.Websites.Events
{
    public class UpdateTabContextEventArgs
    {
        public WebsiteViewModel WebsiteViewModel { get; set; }
        public WebsiteTabViewModels ExecuteViewModel { get; set; }
    }
}