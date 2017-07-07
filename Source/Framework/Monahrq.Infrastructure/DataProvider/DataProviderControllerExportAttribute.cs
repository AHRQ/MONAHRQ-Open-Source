using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Sdk.DataProvider.Builder;
using System.Reflection;
using System.Data;

namespace Monahrq.Sdk.DataProvider
{

    [Flags]
    public enum SupportedPlatforms
    {
        x86 = 1,
        x64 = 2,
        All = x86 | x64,
    }

    /// <summary>
    /// Used to export a connection string editor specific to a connection provider factory 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DataProviderControllerExportAttribute :
        ExportAttribute,
        IDataProviderControllerExportAttribute
    {
        public SupportedPlatforms Platforms
        {
            get;
            private set;
        }

        public bool IsProviderSupported
        {
            get
            {
                return System.Environment.Is64BitProcess ?
                    (Platforms & SupportedPlatforms.x64) == SupportedPlatforms.x64
                    : (Platforms & SupportedPlatforms.x86) == SupportedPlatforms.x86;
            }
        }

        /// <summary>
        /// Constructs an instance of DataProviderViewExportAttribute
        /// </summary>
        /// <param name="providertype">the type of provider for the connection string</param>
        /// <param name="controllerName">display caption</param>
        /// <exception cref="">providerType is not a DataProviderController</exception>
        public DataProviderControllerExportAttribute(
            string controllerName,
            Type controllerType,
            Type viewType,
            SupportedPlatforms platforms,
            string builderOptions
        )
            : base(typeof(IDataProviderController))
        {
            var genInterf = controllerType.GetInterfaces()
                    .Where(intf => typeof(IDataProviderController).IsAssignableFrom(intf))
                    .Where(intf=> intf.IsGenericType)
                    .Where(intf => intf.GetGenericArguments().Count() == 1)
                    .FirstOrDefault();
            if (genInterf == null)
            {
                throw new ArgumentException("Controller type does not implement IDataProviderController", "controllerType");
            }
            var factoryType = genInterf.GetGenericArguments().Where(arg => typeof(DbProviderFactory).IsAssignableFrom(arg)).FirstOrDefault();
            if (factoryType == null)
            {
                throw new ArgumentException("Controller type does not implement IDataProviderController", "controllerType");
            }
            ControllerName = controllerName;
            ControllerType = controllerType;
            DbProviderFactoryType = factoryType;
            Type viewTest = typeof(IConnectionStringView);

            if (!viewTest.IsAssignableFrom(viewType))
            {
                throw new ArgumentException("viewType not IConnectionStringView", "viewType");
            }
            ViewType = viewType;
            Platforms = platforms;
            Options = builderOptions;
        }

        /// <summary>
        /// When defined in a concrete implementation, this string must correspond with an installed data provider factory
        /// </summary>
        private DataRow ProviderFactoryRow
        {
            get
            {
                return 
                DbProviderFactories.GetFactoryClasses()
                    .Rows.OfType<DataRow>()
                    .Where( row=>
                            row["AssemblyQualifiedName"].ToString() 
                            == DbProviderFactoryType.AssemblyQualifiedName).FirstOrDefault();    
            }

        }

        public Type DbProviderFactoryType { get; private set; }

        public DbProviderFactory CreateDbProviderFactory()
        {
            return DbProviderFactories.GetFactory(ProviderFactoryRow);
        }

        public DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            var builder = CreateDbProviderFactory().CreateConnectionStringBuilder();
            builder.ConnectionString = Options;
            return builder;
        }

        public Type ControllerType
        {
            get;
            set;
        }

        public string ViewName
        {
            get
            {
                var export = ViewType.GetCustomAttributes(typeof(ExportAttribute), true).FirstOrDefault() as ExportAttribute;
                return export == null ? string.Empty : export.ContractName;
            }
        }

        public Type ViewType
        {
            get;
            private set;
        }

        public string ControllerName
        {
            get;
            private set;
        }

        public string Options { get; private set; }
    }


}
