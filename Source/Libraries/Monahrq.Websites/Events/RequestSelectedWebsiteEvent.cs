using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Websites.ViewModels;

namespace Monahrq.Websites.Events
{
    public class RequestSelectedWebsiteEvent : CompositePresentationEvent<ExtendedEventArgs<WebsiteViewModel>> { }
}