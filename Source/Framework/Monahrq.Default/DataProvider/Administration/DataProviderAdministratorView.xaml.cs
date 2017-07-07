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
      

        IEventAggregator Events
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IEventAggregator>();
            }
        }

        IRegion DialogRegion
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IRegionManager>().Regions[RegionNames.Modal];
            }
        }

        [ImportingConstructor]
        public DataProviderAdministratorView(IServiceLocator serviceLocator,
            IDataProviderAdministratorController controller)
        {
            InitializeComponent();
            Controller = controller;
            Events.GetEvent<DataProviderAdministratorController.ModifyOrNewEvent>()
                .Subscribe(evnt => LoadBuilderDialog(evnt.Configuration));

            Events.GetEvent<DataProviderAdministratorController.DeletingEvent>()
               .Subscribe(evnt => PromptForDelete(evnt));
        }

        private void PromptForDelete(DeletingConnectionEventArgs evnt)
        {
            evnt.Cancel =
                MessageBox.Show(Application.Current.MainWindow,
                    string.Format(@"Delete ""{0}"" ?", evnt.Connection.Name),
                    "Delete Connection?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No;
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
