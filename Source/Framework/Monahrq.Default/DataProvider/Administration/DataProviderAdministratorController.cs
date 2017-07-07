using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.DataProvider.Builder;
using Monahrq.Sdk.Regions;
using Monahrq.Sdk.Services;

namespace Monahrq.Default.DataProvider.Administration
{
    /// <summary>
    /// class for data provider controller
    /// </summary>
    /// <seealso cref="Monahrq.Default.DataProvider.Administration.IDataProviderAdministratorController" />
    [Export(typeof(IDataProviderAdministratorController))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DataProviderAdministratorController : Monahrq.Default.DataProvider.Administration.IDataProviderAdministratorController
    {
        /// <summary>
        /// Class for select event
        /// </summary>
        /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Default.DataProvider.Administration.NamedConnectionModel}" />
        public class SelectedEvent : CompositePresentationEvent<NamedConnectionModel>{}
        /// <summary>
        /// class for deleting event
        /// </summary>
        /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Default.DataProvider.Administration.DeletingConnectionEventArgs}" />
        public class DeletingEvent : CompositePresentationEvent<DeletingConnectionEventArgs>{}
        /// <summary>
        /// class for deleted event
        /// </summary>
        /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{System.EventArgs}" />
        public class DeletedEvent : CompositePresentationEvent<EventArgs> { }
        /// <summary>
        /// class for modify or new event
        /// </summary>
        /// <seealso cref="Microsoft.Practices.Prism.Events.CompositePresentationEvent{Monahrq.Default.DataProvider.Administration.NamedConnectionModel}" />
        public class ModifyOrNewEvent : CompositePresentationEvent<NamedConnectionModel> { }

        /// <summary>
        /// Gets the modify command.
        /// </summary>
        /// <value>
        /// The modify command.
        /// </value>
        public DelegateCommand ModifyCommand { get; private set; }
        /// <summary>
        /// Gets the new command.
        /// </summary>
        /// <value>
        /// The new command.
        /// </value>
        public DelegateCommand NewCommand { get; private set; }
        /// <summary>
        /// Gets the delete command.
        /// </summary>
        /// <value>
        /// The delete command.
        /// </value>
        public DelegateCommand DeleteCommand { get; private set; }
        /// <summary>
        /// Gets the select command.
        /// </summary>
        /// <value>
        /// The select command.
        /// </value>
        public DelegateCommand SelectCommand { get; private set; }

        /// <summary>
        /// Gets the events.
        /// </summary>
        /// <value>
        /// The events.
        /// </value>
        private IEventAggregator Events 
        {
            get
            {
                return ServiceLocator.GetInstance<IEventAggregator>();
            }
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        public IDataProviderAdministratorViewModel ViewModel { get; private set; }

        /// <summary>
        /// Gets or sets the service locator.
        /// </summary>
        /// <value>
        /// The service locator.
        /// </value>
        IServiceLocator ServiceLocator {get;set;}

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProviderAdministratorController"/> class.
        /// </summary>
        /// <param name="serviceLocator">The service locator of type <see cref="IServiceLocator"/></param>
        /// <param name="viewModel">The view model of type<see cref="IDataProviderAdministratorViewModel"/>.</param>
        [ImportingConstructor]
        public DataProviderAdministratorController(IServiceLocator serviceLocator, 
            IDataProviderAdministratorViewModel viewModel)
        {
            ServiceLocator = serviceLocator;
            ModifyCommand = new DelegateCommand(() => Modify(), () => ViewModel.Current != null);
            NewCommand = new DelegateCommand(() => New());
            DeleteCommand = new DelegateCommand(() => Delete(), () => ViewModel.Current != null);
            SelectCommand = new DelegateCommand(() => Select(), () => ViewModel.Current != null);
            ViewModel = viewModel;
            LoadViewModel();
            ViewModel.Connections.CurrentChanged += (o, e) =>
            {
                ViewModel.Current = ViewModel.Connections.CurrentItem as NamedConnectionModel;
            };
            ViewModel.Current = ViewModel.Connections.OfType<NamedConnectionModel>().FirstOrDefault();
        }

        /// <summary>
        /// Selects this instance.
        /// </summary>
        private void Select()
        {
            Events.GetEvent<SelectedEvent>().Publish(ViewModel.Current);
        }

        /// <summary>
        /// Loads the view model.
        /// </summary>
        private void LoadViewModel()
        {
            var connections = MonahrqConfiguration.SettingsGroup.MonahrqSettings().NamedConnections;
            ViewModel.Connections = new ListCollectionView(new ObservableCollection<NamedConnectionModel>());
            connections.OfType<NamedConnectionElement>()
            .Select(element => new NamedConnectionModel(element));
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        private void Delete()
        {
            var canceler = new DeletingConnectionEventArgs(ViewModel.Current.Configuration);
            Events.GetEvent<DeletingEvent>().Publish(canceler);
            if (!canceler.Cancel)
            {
                (ViewModel.Connections as ListCollectionView).Remove(ViewModel.Current);
                MonahrqConfiguration.Delete(ViewModel.Current.Configuration);
            }
            Events.GetEvent<DeletedEvent>().Publish(EventArgs.Empty);
        }

        /// <summary>
        /// To load the builder
        /// </summary>
        private void New()
        {
            LoadBuilder();
        }

        /// <summary>
        /// Loads the builder.
        /// </summary>
        private void LoadBuilder()
        {
            LoadBuilder(new NamedConnectionModel(new NamedConnectionElement()));
        }

        /// <summary>
        /// Modifies this instance.
        /// </summary>
        private void Modify()
        {
            LoadBuilder(ViewModel.Current);
        }

        /// <summary>
        /// Loads the builder.
        /// </summary>
        /// <param name="item">The item.</param>
        private void LoadBuilder(NamedConnectionModel item)
        {
            Events.GetEvent<ModifyOrNewEvent>().Publish(item);
        }
    }

    /// <summary>
    /// class to delete the connection
    /// </summary>
    /// <seealso cref="System.ComponentModel.CancelEventArgs" />
    public class DeletingConnectionEventArgs: CancelEventArgs
    {
        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        public NamedConnectionElement Connection { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="DeletingConnectionEventArgs"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public DeletingConnectionEventArgs(NamedConnectionElement connection)
        {
            Connection = connection;
        }
    }
}
