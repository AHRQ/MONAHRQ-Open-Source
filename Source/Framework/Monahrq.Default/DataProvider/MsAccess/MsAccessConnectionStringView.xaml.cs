using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.DataProvider;
using Monahrq.Sdk.DataProvider.Builder;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;

namespace Monahrq.Default.DataProvider.MsAccess
{
    /// <summary>
    /// Interaction logic for MsAccessConnectionStringBuilderView.xaml
    /// </summary>
    [ConnectionStringViewExport(DataProviderStrings.MsAccessBuilderViewName, typeof(IFileConnectionStringView))]
    public partial class MsAccessConnectionStringView : UserControl, IFileConnectionStringView
    {
        public MsAccessConnectionStringView()
        {
            InitializeComponent();
            var model = new AccessConnectionViewModel();
            provider = ServiceLocator.Current.GetAllInstances<IDataProviderController>().OfType<MsAccessAceDataProviderController>().FirstOrDefault();
            model.Reset(provider);
            Model = model;
            Loaded += (o, e) => ViewModel.Load(CreateConnectionStringBuilder());

            Filename = "uninitialized";
        }

        ConnectionStringViewModel model;
        public ConnectionStringViewModel Model
        {
            get
            {
                var builder = CreateConnectionStringBuilder();

                model.Configuration = new NamedConnectionElement();
                model.Configuration.ConnectionString = builder.ConnectionString;
                model.Configuration.ControllerType = provider.GetType().AssemblyQualifiedName;
                model.Configuration.SelectFrom = (this.ViewModel.Tables.CurrentItem ?? string.Empty).ToString();
                return model;
            }
            private set
            {
                model = value;
            }
        }

        MsAccessAceDataProviderController provider { get; set; }

        // For example:
        // Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\myFolder\myAccess2007file.accdb;
        // Persist Security Info=False;
        private System.Data.Common.DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            var builder = provider.CreateConnectionStringBuilder();
            builder.Add("Data Source", this.Filename);
            builder.Add("Persist Security Info", "False");
            return builder;
        }

        [Import]
        public AccessConnectionStringViewModel ViewModel
        {
            get { return DataContext as AccessConnectionStringViewModel; }
            set { DataContext = value; }
        }

        public string Filename
        {
            get;
            set;
        }
    }
}
