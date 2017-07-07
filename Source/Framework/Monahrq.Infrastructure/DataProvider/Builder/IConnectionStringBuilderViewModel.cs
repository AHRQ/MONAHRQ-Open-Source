using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.Services;
using PropertyChanged;

namespace Monahrq.Sdk.DataProvider.Builder
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BuilderExportAttribute : ExportAttribute
    {
        public BuilderExportAttribute(Type contractType)
            : base(contractType)
        {
        }
    }
        

    public interface IConnectionStringBuilderViewModel
    {
        ICollectionView ProviderExports { get; }
        NamedConnectionElement InitialConnection { get; set; }
        DelegateCommand CancelCommand { get;  }
        DelegateCommand SaveConnectionCommand { get;  }
        DelegateCommand TestConnectionCommand { get; }
        IConnectionStringView View { get; }
        IDataProviderControllerExportAttribute CurrentProviderExport { get; set; }
    }

    [ImplementPropertyChanged]
    [BuilderExportAttribute(typeof(IConnectionStringBuilderViewModel))]
    public class ConnectionStringBuilderViewModel
        : IConnectionStringBuilderViewModel
    {

        public ICollectionView ProviderExports { get; private set; }
        private NamedConnectionElement _initial = new NamedConnectionElement();
        public NamedConnectionElement InitialConnection
        {
            get
            {
                return _initial;
            }
            set
            {
                value.CopyTo(_initial);
            }
        }

        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand SaveConnectionCommand { get; private set; }
        public DelegateCommand TestConnectionCommand { get; private set; }

        protected IEventAggregator Events
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IEventAggregator>();
            }
        }

        IRegion TargetRegion 
        {
            get
            {
                return Manager.Regions[DialogRegionNames.ConnectionStringViewRegion];
            }
        }

        IDataProviderService DataProviderService
        {
            get;
            set;
        }

        IConnectionStringBuilderController Controller
        {
            get;
            set;
        }

        [ImportingConstructor]
        public ConnectionStringBuilderViewModel(
            IConnectionStringBuilderController controller
            , IDataProviderService dataProviderService)
        {
            Controller = controller;
            DataProviderService = dataProviderService;
            Initialize();
            AttachCommands();
        }


        private void RegisterViews()
        {
            var providers = DataProviderService.GetRegisteredProviderExports();
            foreach(var view in providers.Select(prov=>prov.ViewType))
            {
                Manager.RegisterViewWithRegion(DialogRegionNames.ConnectionStringViewRegion, view);
            }
        }

        private void AttachCommands()
        {
            CancelCommand = new DelegateCommand(Cancel);
            TestConnectionCommand = new DelegateCommand(TestConnection);
            SaveConnectionCommand = new DelegateCommand(SaveConnection, () => View != null && View.Model.IsValid);
        }

        private void SaveConnection()
        {
            Controller.SaveConnection(View.Model.Configuration);
        }

        private void TestConnection()
        {
            Controller.Provider = CurrentDataProvider;
            Controller.TestConnection(View.Model.GetConnectionString());
        }

        private void Cancel()
        {
            Controller.Cancel();
        }

        private void LoadConnectionStringViewForProvider()
        {
            TargetRegion.ActiveViews.ToList().ForEach(view =>
                {
                    TargetRegion.Deactivate(view);
                    TargetRegion.Remove(view);
                });
            if (CurrentProviderExport == null) return;
            View = ServiceLocator.Current.GetInstance<IConnectionStringView>(CurrentProviderExport.ViewName);

            if ( InitialControllerName == CurrentProviderExport.ControllerName)
            {
                View.Model.Configuration = InitialConnection;
            }
            else
            {
                View.Model.Reset(CurrentDataProvider);
            }
            TargetRegion.Add(View);
            TargetRegion.Activate(View);
        }

        public IConnectionStringView View
        {
            get;
            set;
        }

        public IDataProviderControllerExportAttribute CurrentProviderExport
        {
            get { return ProviderExports.CurrentItem as IDataProviderControllerExportAttribute; }
            set 
            { 
                ProviderExports.MoveCurrentTo(value); 
            }
        }

        public string InitialControllerName
        {
            get
            {
                if (InitialConnection == null) return string.Empty;
                var export = InitialConnection.GetExportAttribute();
                if (export == null) return string.Empty;
                return export.ControllerName;
            }
        }

        public IDataProviderController CurrentDataProvider
        {
            get
            {
                return DataProviderService.GetController(CurrentProviderExport.ControllerName);
            }
        }

        public bool IsChanged { get; set; }


        public IRegionManager Manager 
        { 
            get
            {
                return ServiceLocator.Current.GetInstance<IRegionManager>();
            }
        }


        private void Initialize()
        {
            var items = DataProviderService.GetRegisteredProviderExports();
            var observable = new ObservableCollection<IDataProviderControllerExportAttribute>(items);
            var providers = new ListCollectionView(new ObservableCollection<IDataProviderControllerExportAttribute>(observable));
            ProviderExports = providers;
            CurrentProviderExport = null;
            providers.CurrentChanged += (o, e) =>
            {
                LoadConnectionStringViewForProvider();
            };
        }
    }

   

}
