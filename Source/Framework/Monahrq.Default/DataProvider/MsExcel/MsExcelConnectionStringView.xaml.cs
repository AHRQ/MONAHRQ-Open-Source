using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.DataProvider;
using Monahrq.Sdk.DataProvider.Builder;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;

namespace Monahrq.Default.DataProvider.MsExcel
{
    /// <summary>
    /// Interaction logic for MsExcelConnectionStringView.xaml
    /// </summary>
    [ConnectionStringViewExport(DataProviderStrings.MsExcelBuilderViewName, typeof(IFileConnectionStringView))]
    public partial class MsExcelConnectionStringView : UserControl, IFileConnectionStringView
    {
        public MsExcelConnectionStringView()
        {
            InitializeComponent();
            var model = new ExcelConnectionViewModel();
            provider = ServiceLocator.Current.GetAllInstances<IDataProviderController>().OfType<MsExcelAceDataProviderController>().FirstOrDefault();
            model.Reset(provider);
            Model = model;
            Loaded += delegate
            {
                ViewModel.Load(CreateConnectionStringBuilder());
                var temp = ViewModel.Tables;
            };

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

        MsExcelAceDataProviderController provider { get; set; }

        // For example:
        // Provider=Microsoft.ACE.OLEDB.12.0;Data Source=c:\myFolder\myOldExcelFile.xls;
        // Extended Properties="Excel 8.0;HDR=YES";
        private System.Data.Common.DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            var builder = provider.CreateConnectionStringBuilder();
            builder.Add("Data Source", this.Filename);
            builder.Add("Extended Properties", string.Format("Excel 8.0;HDR={0}", ViewModel.HasHeader ? "YES" : "NO"));
            return builder;
        }

        [Import]
        public ExcelConnectionStringViewModel ViewModel
        {
            get { return DataContext as ExcelConnectionStringViewModel; }
            set
            {
                DataContext = value;
            }
        }

        public string Filename
        {
            get;
            set;
        }
    }
}
