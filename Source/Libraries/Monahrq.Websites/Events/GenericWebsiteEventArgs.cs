using Monahrq.Sdk.Events;
using Monahrq.Websites.ViewModels;

namespace Monahrq.Websites.Events
{
    public class GenericWebsiteEventArgs
    {
        public WebsiteViewModel Website { get; set; }
        public string Message { get; set; }
        public ENotificationType NotificationType { get; set; }
        public GenericWebsiteEventArgs() { NotificationType = ENotificationType.Info; }
    }
}