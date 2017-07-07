using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Monahrq.Infrastructure.Entities.Events;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.Infrastructure.Entities.Core.Import
{
    public partial class CSVDataReaderDictionary  : IDataReaderDictionary
    {
        IImportAuditor _auditor;

        [Import]
        public IImportAuditor Auditor
        {
            get
            {
                return _auditor;
            }
            set
            {
                _auditor = value;
            }
        }

        public event EventHandler<ExtendedEventArgs<bool>> RequestingWasExecuted = delegate { };

        protected virtual void OnInitialized()
        {
            if(DesignMode) return;
            InitializeConnection();
        }
         
        private void InitializeConnection()
        {
            TheDataConnection.Disposed += delegate
            {
                Directory.GetFiles(DataFolder).ToList().ForEach(File.Delete);
                Directory.Delete(DataFolder);
            };
            if (!Directory.Exists(DataFolder)) Directory.CreateDirectory(DataFolder);
            Disposed += delegate { TheDataConnection.Dispose(); };
            TheDataConnection.ConnectionString = CsvConnectionString;
            TheDataConnection.Open();
        }

        public string DataFolder
        {
            get
            {
                var assy = Assembly.GetExecutingAssembly();
                var AssemblyCompany = assy.GetCustomAttribute<AssemblyCompanyAttribute>()
                    ?? new AssemblyCompanyAttribute("{39049C7F-E216-4404-8FA7-4A213365DD19}");
                //var company = string.IsNullOrWhiteSpace(AssemblyCompany.Company) ?
                //                "{39049C7F-E216-4404-8FA7-4A213365DD19}"
                //                : AssemblyCompany.Company;
                var AssemblyProduct = assy.GetCustomAttribute<AssemblyProductAttribute>()
                        ?? new AssemblyProductAttribute("{9022C9CA-A2E7-46CD-9FBE-AE7A77C44F53}");
                //var product = string.IsNullOrWhiteSpace(AssemblyProduct.Product) ?
                //                "{9022C9CA-A2E7-46CD-9FBE-AE7A77C44F53}"
                //                : AssemblyProduct.Product;
                var contractName = VersionAttribute == null
                    || string.IsNullOrWhiteSpace(VersionAttribute.ContractName) ? "Default" : VersionAttribute.ContractName;
                var version = VersionAttribute == null
                                   ? 0.0M : VersionAttribute.Version;

                var path = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                            , AssemblyCompany.Company
                            , AssemblyProduct.Product
                            , contractName
                            , string.Format("{0}", version));

                return path;

                //var contractName = VersionAttribute == null || string.IsNullOrWhiteSpace(VersionAttribute.ContractName) 
                //                                            ? "Default" 
                //                                            : VersionAttribute.ContractName;

                //var version = VersionAttribute == null
                //                   ? 0.0M : VersionAttribute.Version;

                //return Path.Combine(
                //            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                //            , contractName
                //            , string.Format("{0}", version));
            }
        }

        public VersionedComponentExportAttribute VersionAttribute
        {
            get
            {
                return this.GetType().GetCustomAttribute<VersionedComponentExportAttribute>();
            }
        }

        public System.Data.IDataReader this[string readerName]
        {
            get
            {
                return this.GetType().GetProperty(readerName).GetValue(this) as IDataReader;
            }
        }

        protected string CsvConnectionString
        {
            get
            {
                OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder();
                builder.Provider = "Microsoft.ACE.OLEDB.12.0";
                builder.DataSource = DataFolder;
                builder["Extended Properties"] = "text;HDR=YES;FMT=Delimited";
                return builder.ConnectionString;
            }
        }

        private System.Data.IDbCommand GetCommand(PropertyInfo prop)
        {
            AssertCsvFile(prop);
            var sql = string.Format("SELECT * FROM [{0}.csv]", prop.Name);
            var result = TheDataConnection.CreateCommand();
            result.CommandText = sql;
            return result;
        }

        private void AssertCsvFile(PropertyInfo prop)
        {
            if (DesignMode) return;
            File.WriteAllText(Path.Combine(DataFolder, string.Format("{0}.csv", prop.Name)), prop.GetValue(null).ToString());
        }

        private IDbCommand GetCommand(Expression<Func<object>> expr)
        {
            var prop = expr.StaticProperty();
            return GetCommand(prop);
        }

        protected IDataReader GetReader(Expression<Func<object>> expr)
        {
           // if (Auditor.WasExecuted(this, expr)) return EmptyReader.Instance;
            using (var cmd = GetCommand(expr) as OleDbCommand)
            {
                return cmd.ExecuteReaderAsync().Result;
            }
        }

        public virtual IEnumerable<IProgrammability> Programmabilities
        {
            get;
            protected set;
        }
    }
}
