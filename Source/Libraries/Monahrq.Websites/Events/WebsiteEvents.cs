using System;
using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Events;
using Monahrq.Sdk.Events;
using Monahrq.Websites.ViewModels;

namespace Monahrq.Websites.Events
{
	public class WebsiteCreatedOrUpdatedEvent : CompositePresentationEvent<ExtendedEventArgs<GenericWebsiteEventArgs>> { }
    public class WebsiteDeletedEvent : CompositePresentationEvent<WebsiteViewModel> { }
    //public class WebsiteUpdatedEvent : CompositePresentationEvent<WebsiteViewModel> { }

    public class WebsiteSelectedEvent : CompositePresentationEvent<WebsiteViewModel> { }
    public class RequestSelectedWebsiteEvent : CompositePresentationEvent<ExtendedEventArgs<WebsiteViewModel>> { }

    public class WebsiteNextTabEvent : CompositePresentationEvent<WebsiteViewModel> { }

    //public class UpdateWebsiteTabContextEvent : CompositePresentationEvent<UpdateTabContextEventArgs> { }
    public class WebsiteJumpToTabEvent : CompositePresentationEvent<WebsiteJumpToTabEventArgs> { }


    public class WebsiteJumpToTabEventArgs
    {
        public WebsiteViewModel WebsiteVm { get; set; }
        public WebsiteTabViewModels TabVm { get; set; }
    }

    public class GenericWebsiteEventArgs
    {
        public WebsiteViewModel Website { get; set; }
        public string Message { get; set; }
		public ENotificationType NotificationType { get; set; }
		public GenericWebsiteEventArgs() { NotificationType = ENotificationType.Info; }
    }

    public class UpdateTabContextEventArgs
    {
        public WebsiteViewModel WebsiteViewModel { get; set; }
        public WebsiteTabViewModels ExecuteViewModel { get; set; }
    }

    // public class WebSiteGenerationLogEvent : CompositePresentationEvent<Tuple<string, DateTime>> { }
    // public class WebsiteGenerationResult: CompositePresentationEvent<bool> { }

    public enum WebsiteTabViewModels
    {
        Details,
        Datasets,
        Manage,
        Measures,
        Reports,
        ReportsPublish,
        Settings,
        Topics
    }

    public class SelectedTopicAssigmentEvent : CompositePresentationEvent<Empty> { }

    public class SelectedItemsForNewlyAddedDatasets : CompositePresentationEvent<bool> { }

    public class ItemsForNewlyAddedDatasetsEventArgs
    {
        public string DatasetType { get; set; }
        public bool Refresh { get; set; }
    }

    //public class ForceTabSaveEvent : CompositePresentationEvent<bool> { }
}
