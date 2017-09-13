using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Sdk.DataProvider.Builder;
using Monahrq.Sdk.Regions;

namespace Monahrq.Default.DataProvider.Administration
{
    /// <summary>
    /// Interaction logic for DataProviderAdministratorView.xaml
    /// </summary>
    [Export(typeof(DataProviderAdministratorView))]
    public partial class DataProviderAdministratorView : UserControl
    {
        public DataProviderAdministratorView()
        {
            InitializeComponent();
        }

        IDataProviderAdministratorController Controller
        {
            get
            {
                return DataContext as IDataProviderAdministratorController;
            }
            set
            {
                DataContext = value;
            }
        }
      

        private IEventAggregator Events => ServiceLocator.Current.GetInstance<IEventAggregator>();
        private IRegion DialogRegion => ServiceLocator.Current.GetInstance<IRegionManager>().Regions[RegionNames.Modal];

        [Import]
        public ILogWriter Logger { get; set; }

        [ImportingConstructor]
        public DataProviderAdministratorView(IServiceLocator serviceLocator,
            IDataProviderAdministratorController controller)
        {
            InitializeComponent();
            Controller = controller;
            Events.GetEvent<DataProviderAdministratorController.ModifyOrNewEvent>()
                .Subscribe(evnt => LoadBuilderDialog(evnt.Configuration));

            Events.GetEvent<DataProviderAdministratorController.DeletingEvent>()
               .Subscribe(PromptForDelete);
        }

        private void PromptForDelete(DeletingConnectionEventArgs evnt)
        {
            var result = MessageBox.Show(
                Application.Current.MainWindow,
                    string.Format(@"Delete ""{0}"" ?", evnt.Connection.Name),
                    "Delete Connection?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question, MessageBoxResult.No);

            this.Logger.Debug($"User responded '{result}' to prompt \"Delete {evnt.Connection.Name}\"");

            evnt.Cancel = result == MessageBoxResult.No;
        }

        private void LoadBuilderDialog(Infrastructure.Configuration.NamedConnectionElement connection)
        {
            var region = DialogRegion;
            region.ActiveViews.ToList().ForEach((v) =>
            {
                region.Deactivate(v);
                region.Remove(v);
            });
            var builderView = ServiceLocator.Current.GetInstance<ConnectionStringBuilderView>();
            builderView.Model.InitialConnection = connection;
            region.Add(builderView);
            region.Activate(builderView);
        }

    }
}
