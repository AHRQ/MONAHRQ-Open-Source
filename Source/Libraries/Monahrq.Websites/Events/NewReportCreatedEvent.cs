using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Websites.ViewModels;

namespace Monahrq.Websites.Events
{
    public class NewReportCreatedEvent : CompositePresentationEvent<ReportViewModel> { }
   public class ReportDeletedEvent : CompositePresentationEvent<ReportViewModel> { }
}
