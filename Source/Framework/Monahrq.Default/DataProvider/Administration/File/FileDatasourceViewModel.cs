using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.DataProvider;
using Monahrq.Sdk.DataProvider.Builder;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.Prism.Events;

namespace Monahrq.Default.DataProvider.Administration.File
{
    /// <summary>
    /// Class for file related operations
    /// </summary>
    /// <seealso cref="Monahrq.Default.DataProvider.Administration.File.IFileDatasourceViewModel" />
    [ImplementPropertyChanged]
    [BuilderExport(typeof(IFileDatasourceViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.NonShared)]
    public class FileDatasourceViewModel : IFileDatasourceViewModel
    {
        /// <summary>
        /// The provider lookup for file types
        /// </summary>
        readonly Dictionary<string, Type> _providerLookup = new Dictionary<string, Type>()
        {
            {"csv", typeof(Text.TextAceDataProviderController)}
            ,{"xls", typeof(MsExcel.MsExcelAceDataProviderController)}
            ,{"xlsx", typeof(MsExcel.MsExcelAceDataProviderController)}
            ,{"mdb", typeof(MsAccess.MsAccessAceDataProviderController)}
            ,{"accdb", typeof(MsAccess.MsAccessAceDataProviderController)}
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDatasourceViewModel"/> class.
        /// </summary>
        public FileDatasourceViewModel()
        {
            CurrentFile = string.Empty;
            OleDbProviders = new Lazy<IEnumerable<IDataProviderController>>(() =>
            {
                var temp = new Type[] {
                        typeof(MsAccess.MsAccessAceDataProviderController)
                        ,typeof(MsExcel.MsExcelAceDataProviderController)
                        ,typeof(Text.TextAceDataProviderController)
                    };

                return ServiceLocator.Current.GetAllInstances<IDataProviderController>()
                    .Where(ctrl => temp.Contains(ctrl.GetType()));
            });

            ProviderViews = new Lazy<IDictionary<IDataProviderController, IFileConnectionStringView>>(
                    () =>
                    {
                        Func<Type, IFileConnectionStringView> func = (type) =>
                        {
                            var attr = type.GetCustomAttribute<DataProviderControllerExportAttribute>();
                            var viewtype = attr.ViewType;
                            var viewattr = viewtype.GetCustomAttribute<ConnectionStringViewExportAttribute>();
                            var viewname = viewattr.ContractName;
                            var result = ServiceLocator.Current.GetInstance<IFileConnectionStringView>(viewname);
                            return result;
                        };

                        var temp = OleDbProviders.Value.ToList();
                        var temp2 = temp.ToDictionary(
                            k => k,
                            v => func(v.GetType()));
                        return temp2;
                    });

#if false   // unused
            CancelCommand = new DelegateCommand(() => Controller.Cancel());
            TestConnectionCommand = new DelegateCommand(() =>
                    {
                        try
                        {
                            Controller.TestConnection(View.Model.Configuration);
                        }
                        catch
                        {
                            ///To do error handler
                        }
                    });
            SaveConnectionCommand = new DelegateCommand(() => Controller.SaveConnection(View.Model.Configuration));
#endif
            SelectFileCommand = new DelegateCommand(SelectFile);
        }

        // Let the user browse for the file to import
        /// <summary>
        /// To let the user browse the file from the explorer
        /// </summary>
        private void SelectFile()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            
            // open in the same folder as last time, or the user's document folder by default
            dlg.InitialDirectory = LastFolder;

            // Set filter and file extension for the document types we can read
            dlg.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";//only support .csv  ticket# 3705    "|Excel Files (*.xlsx)|*.xlsx|Access Databases (*.mdb)|*.mdb";
            dlg.FilterIndex = 0;

            if (dlg.ShowDialog() == true)
            {
                CurrentFile = dlg.FileName;

                // update LastFolder if the path exists
                var path = Path.GetDirectoryName(CurrentFile);
                if (Directory.Exists(path)) LastFolder = path;
            }
        }

        /// <summary>
        /// Gets or sets the provider views.
        /// </summary>
        /// <value>
        /// The provider views.
        /// </value>
        Lazy<IDictionary<IDataProviderController, IFileConnectionStringView>> ProviderViews { get; set; }

        /// <summary>
        /// Gets or sets the OLE database providers.
        /// </summary>
        /// <value>
        /// The OLE database providers.
        /// </value>
        Lazy<IEnumerable<IDataProviderController>> OleDbProviders { get; set; }

        /// <summary>
        /// Gets or sets the internal current file.
        /// </summary>
        /// <value>
        /// The internal current file.
        /// </value>
        string InternalCurrentFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current file.
        /// </summary>
        /// <value>
        /// The current file.
        /// </value>
        public string CurrentFile
        {
            get
            {
                return InternalCurrentFile;
            }
            set
            {
                if (value == InternalCurrentFile)
                    return;
                InternalCurrentFile = value;
                ReconcileProvider();
            }
        }

        /// <summary>
        /// Reconciles the provider.
        /// </summary>
        private void ReconcileProvider()
        {
            var ext = Path.GetExtension(InternalCurrentFile).TrimStart('.');
            Type type;
            if (_providerLookup.TryGetValue(ext.ToLower(), out type))
            {
                CurrentDataProvider = OleDbProviders.Value.OfType<OleDbDataProviderController>().First(prov => prov.GetType() == type);
                CurrentView = ProviderViews.Value[CurrentDataProvider];
                CurrentView.Model.Reset(CurrentDataProvider);

                // Display the provider-specific UI pane
                LoadConnectionStringViewForProvider();
            }
            else
            {
                CurrentDataProvider = null;
                CurrentView = null;
            }
        }

        /// <summary>
        /// Gets the current data provider.
        /// </summary>
        /// <value>
        /// The current data provider.
        /// </value>
        public OleDbDataProviderController CurrentDataProvider
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the connection element.
        /// </summary>
        /// <value>
        /// The connection element.
        /// </value>
        public NamedConnectionElement ConnectionElement
        {
            get
            {
                if (CurrentView == null)
                {
                    return null;
                }
                var element = new NamedConnectionElement();
                var temp = CurrentView.Model.Configuration;
                temp.CopyTo(element);
                element.ControllerType = CurrentDataProvider == null ? string.Empty : CurrentDataProvider.GetType().AssemblyQualifiedName;
                return element;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has header.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has header; otherwise, <c>false</c>.
        /// </value>
        public bool HasHeader { get; set; }

        /// <summary>
        /// Gets the select file command.
        /// </summary>
        /// <value>
        /// The select file command.
        /// </value>
        public DelegateCommand SelectFileCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current view.
        /// </summary>
        /// <value>
        /// The current view.
        /// </value>
        public IFileConnectionStringView CurrentView
        {
            get;
            private set;
        }

        /// <summary>
        /// Loads the connection string view for provider.
        /// </summary>
        private void LoadConnectionStringViewForProvider()
        {

            var fileview = CurrentView as IFileConnectionStringView;
            if (fileview != null)
                fileview.Filename = CurrentFile;

        }

        /// <summary>
        /// Gets or sets the last folder.
        /// </summary>
        /// <value>
        /// The last folder.
        /// </value>
        private string LastFolder
        {
            get
            {
                var temp = MonahrqConfiguration.SettingsGroup.MonahrqSettings().LastFolder;
                return (string.IsNullOrEmpty(temp) || !Directory.Exists(temp)) ? System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) : temp;
            }
            set
            {
                var settings = MonahrqConfiguration.SettingsGroup.MonahrqSettings();
                settings.LastFolder = value;
                MonahrqConfiguration.Save(settings);
            }
        }

#if false
        [Import]
        IFileDatasourceController Controller
        {
            get;
            set;
        }

        public DelegateCommand CancelCommand
        {
            get;
            private set;
        }

        public DelegateCommand SaveConnectionCommand
        {
            get;
            private set;
        }

        public DelegateCommand TestConnectionCommand
        {
            get;
            private set;
        }
#endif

    }

    /// <summary>
    /// Enum for Jet Version
    /// </summary>
    enum JetVersion
    {
        Unknown, Ace,
    }

    /// <summary>
    /// Enum for file type
    /// </summary>
    enum Src
    {
        Unknown, Access, Excel, Text,
    }
}
