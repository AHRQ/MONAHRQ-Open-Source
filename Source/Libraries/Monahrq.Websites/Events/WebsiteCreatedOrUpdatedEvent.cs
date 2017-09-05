using System;
using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Events;

namespace Monahrq.Websites.Events
{
	public class WebsiteCreatedOrUpdatedEvent : CompositePresentationEvent<ExtendedEventArgs<GenericWebsiteEventArgs>> { }

    //public class WebsiteUpdatedEvent : CompositePresentationEvent<WebsiteViewModel> { }

    //public class UpdateWebsiteTabContextEvent : CompositePresentationEvent<UpdateTabContextEventArgs> { }


    // public class WebSiteGenerationLogEvent : CompositePresentationEvent<Tuple<string, DateTime>> { }
    // public class WebsiteGenerationResult: CompositePresentationEvent<bool> { }

    //public class ForceTabSaveEvent : CompositePresentationEvent<bool> { }
}
