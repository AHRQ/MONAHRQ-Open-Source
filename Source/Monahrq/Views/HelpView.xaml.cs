using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Events;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.Regions;
using Monahrq.ViewModels;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace Monahrq.Views
{
    /// <summary>
    /// Interaction logic for LeftNavigation.xaml
    /// </summary>
    [ViewExport(typeof(HelpView), RegionName = RegionNames.HelpContent)]
    public partial class HelpView : UserControl
    {
        [ImportingConstructor]
        public HelpView(HelpViewModel helpViewModel)
        {
            InitializeComponent();
            this.Loaded += HelpView_Loaded;
            
            if (helpViewModel != null)
                this.DataContext = helpViewModel;

            //return;

            //Events.GetEvent<HelpOpenCloseEvent>().Subscribe((HelpWindowIsOpen) =>
            //    {
            //        if (HelpWindowIsOpen)
            //        {
            //            WebContent.Visibility = Visibility.Visible;
            //            //VisualStateManager.GoToState(this, "Open", false);
            //        }
            //        else
            //        {
            //            WebContent.Visibility = Visibility.Hidden;
            //            //VisualStateManager.GoToState(this, "Closed", false);
            //        }
            //    });
        }

        private bool HelpWindowIsOpen { get; set; }

        void HelpView_Loaded(object sender, RoutedEventArgs e)
        {
            //VisualStateManager.GoToState(this, "Closed", false);
        }

        protected IEventAggregator Events
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IEventAggregator>();
            }
        }
    }
}
