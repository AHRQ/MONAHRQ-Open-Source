using Microsoft.Practices.Prism.Events;
using System;
using Monahrq.Websites.ViewModels;

namespace Monahrq.Websites.Events
{

    public class StatusNotificationEvent : CompositePresentationEvent<string> { }
    public class MeasureDetailsNavigateEvent:CompositePresentationEvent<MeasureModel>{}
    public class SelectedMeasureEditEvent : CompositePresentationEvent<bool> { }
    public class TopicsUpdatedEvent : CompositePresentationEvent<int> { }
    public class TopicFilterApplied : CompositePresentationEvent<EventArgs> { }
    public class MeasureFilterApplied : CompositePresentationEvent<EventArgs> { }
    public class TopicSelectedChanged : CompositePresentationEvent<object> { }


    /*Class to hold basic enitity information in string
     Used for error notifications and logging
     in Some event we might not have entity ID
     */
}
