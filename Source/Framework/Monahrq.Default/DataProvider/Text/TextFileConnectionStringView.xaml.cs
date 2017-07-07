using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.DataProvider;
using Monahrq.Sdk.DataProvider.Builder;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Default.DataProvider.Text
{
    [ConnectionStringViewExport(DataProviderStrings.TextFileBuilderViewName, typeof(IFileConnectionStringView))]
    public partial class TextFileConnectionStringView : UserControl, IFileConnectionStringView
    {
        public TextFileConnectionStringView()
        {
            SchemaIni = new SchemaIniElementCollection();

            InitializeComponent();
            var model = new TextFileConnectionViewModel();
            provider = ServiceLocator.Current.GetAllInstances<IDataProviderController>().OfType<TextAceDataProviderController>().FirstOrDefault();
            model.Reset(provider);
            Model = model;
            Loaded += (o, e) => ViewModel.Load(CreateConnectionStringBuilder());

            Filename = "uninitialized";
        }

        TextAceDataProviderController provider { get; set; }

        ConnectionStringViewModel model;
        public ConnectionStringViewModel Model
        {
            get
            {
                var builder = CreateConnectionStringBuilder();
                model.Configuration = new NamedConnectionElement();
                model.Configuration.SchemaIniSettings = SchemaIni;
                model.Configuration.ConnectionString = builder.ConnectionString;
                model.Configuration.ControllerType = provider.GetType().AssemblyQualifiedName;

                // The select from string for csv files is just the filename...
                model.Configuration.SelectFrom = Path.GetFileName(Filename);
                model.Configuration.HasHeader = builder.ConnectionString.ContainsIgnoreCase("HDR=YES");
                return model;
            }
            private set
            {
                model = value;
                //if (model == null) return;

                //// update view model and UI controls from incoming model
                //SchemaIni = model.Configuration.SchemaIniSettings;
                //var element = SchemaIni["TextDelimiter"];
                //HasDoubleQuotes = element == null ? false : element.Value == @"""";
                //var builder = new OleDbConnectionStringBuilder(model.GetConnectionString());
                //var ext = builder["Extended Properties"].ToString().ToUpper();
                //HasHeader = ext.Contains("HDR=YES");
            }
        }

        //bool HasDoubleQuotes { get; set; }
        //bool HasHeader { get; set; }

        // for text files, the datasource is the path of the file, not the full filename
        // For example:
        //Provider=Microsoft.ACE.OLEDB.12.0;Data Source=c:\txtFilesFolder\;
        //Extended Properties="text;HDR=Yes;FMT=Delimited";
        private System.Data.Common.DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            var builder = provider.CreateConnectionStringBuilder();
            builder.Add("Data Source", string.Format("{0}{1}", Path.GetDirectoryName(this.Filename), Path.DirectorySeparatorChar));
            builder.Add("Extended Properties", string.Format("text;HDR={0};FMT=Delimited", ViewModel.HasHeader ? "Yes" : "No"));
            return builder;
        }

        [Import]
        public TextFileConnectionStringViewModel ViewModel
        {
            get 
            { 
                var temp = DataContext as TextFileConnectionStringViewModel;
                //temp.HasDoubleQuotes = HasDoubleQuotes;
                //temp.HasHeader = HasHeader;
                return temp;
            }
            set 
            {
                if (ViewModel != null)
                {
                    (ViewModel as INotifyPropertyChanged).PropertyChanged -= PushToConnectionElement;
                }
                DataContext = value;
                 if (ViewModel != null)
                {
                    (ViewModel as INotifyPropertyChanged).PropertyChanged += PushToConnectionElement;
                }
            }
        }

        public SchemaIniElementCollection SchemaIni
        {
            get;
            set;
        }

        // get the TextDelimeter value from the TextFileConnectionStringViewModel
        private void PushToConnectionElement(object sender, PropertyChangedEventArgs e)
        {
            SchemaIni = new SchemaIniElementCollection();

            // Per this documentation, the SchemaIni TextDelimeter is double-quote if not present, so we only need to set to none if user said none.
            // http://office.microsoft.com/en-us/access-help/initializing-the-text-data-source-driver-HP001032166.aspx
            if (!ViewModel.HasDoubleQuotes)
            {
                SchemaIni.Add(new SchemaIniElement("TextDelimiter", "none"));
            }
        }

        public string Filename
        {
            get;
            set;
        }
    }
}
