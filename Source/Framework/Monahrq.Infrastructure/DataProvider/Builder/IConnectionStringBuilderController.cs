using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Configuration;

namespace Monahrq.Sdk.DataProvider.Builder
{
    public interface IConnectionStringBuilderController
    {
        IDataProviderController Provider { get; set; }
        void TestConnection(string element);
        void SaveConnection(NamedConnectionElement element);
        void Cancel();
    }

    [Export(typeof(IConnectionStringBuilderController))]
    public class ConnectionStringBuilderController: IConnectionStringBuilderController
    {
        public class SavedEvent : CompositePresentationEvent<NamedConnectionElement> { }
        public class CanceledEvent : CompositePresentationEvent<EventArgs> { }
        public class TestSucceededEvent : CompositePresentationEvent<EventArgs> { }
        public class TestFailedEvent : CompositePresentationEvent<Exception> { }

        [Import]
        IEventAggregator Events {get;set;}
 

        public void TestConnection(string connectionString)
        {
            var factory = Provider.ProviderFactory;
            using (var db = factory.CreateConnection())
            {
                db.ConnectionString = connectionString;
                try
                {
                    db.Open();
                    Events.GetEvent<TestSucceededEvent>().Publish(EventArgs.Empty);
                }
                catch(Exception ex)
                {
                    Events.GetEvent<TestFailedEvent>().Publish(ex);
                }
            }
        }

        public void SaveConnection(NamedConnectionElement element)
        {
            var settings = Monahrq.Infrastructure.Configuration.MonahrqConfiguration.SettingsGroup.MonahrqSettings();
            var current = settings.NamedConnections[element.Name];
            if (current != null)
            {
                settings.NamedConnections.Remove(current);
            }
            settings.NamedConnections.Add(element);
            Monahrq.Infrastructure.Configuration.MonahrqConfiguration.Save(settings);
            Events.GetEvent<SavedEvent>().Publish(element);
        }

        public IDataProviderController Provider
        {
            get;
            set;
        }

        public void Cancel()
        {
            Events.GetEvent<CanceledEvent>().Publish(EventArgs.Empty);
        }
    }

}
