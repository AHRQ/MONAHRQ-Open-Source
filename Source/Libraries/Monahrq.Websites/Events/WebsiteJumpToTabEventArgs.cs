using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Websites.ViewModels;

namespace Monahrq.Websites.Events
{
    public class WebsiteJumpToTabEventArgs
    {
        public WebsiteViewModel WebsiteVm { get; set; }
        public WebsiteTabViewModels TabVm { get; set; }
    }
}