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
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Sdk.Attributes;
using Monahrq.Sdk.DataProvider.Builder;
using Monahrq.Sdk.Regions;
using System.ComponentModel;

namespace Monahrq.Default.DataProvider.Administration.File
{
    /// <summary>
    /// Interaction logic for ConnectinStringBuilderView.xaml
    /// </summary>
    [ViewExport(typeof(IFileDatasourceView))]
    public partial class FileDatasourceView : UserControl, IFileDatasourceView
    {
        [Import(LogNames.Operations)]
        ILogWriter Logger
        {
            get;
            set;
        }

        public IRegionManager Manager
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IRegionManager>();
            }
        }

        public IFileDatasourceViewModel Model
        {
            get { return DataContext as IFileDatasourceViewModel; }
            set
            {
                DataContext = value;
            }
        }

        public FileDatasourceView()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            var regions = Manager.Regions;
            try
            {
                if (!regions.ContainsRegionWithName(DialogRegionNames.ConnectionStringViewRegion))
                {
                    RegionManager.SetRegionManager(ViewContent, Manager);
                    RegionManager.UpdateRegions();
                }
            }
            catch (UpdateRegionsException ex)
            {
                Logger.Write(ex);
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
    }
}
