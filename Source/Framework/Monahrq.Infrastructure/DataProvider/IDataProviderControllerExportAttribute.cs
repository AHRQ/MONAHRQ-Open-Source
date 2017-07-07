using System;
using System.Data.Common;
namespace Monahrq.Sdk.DataProvider
{
    public interface IDataProviderControllerExportAttribute
    {
        System.Data.Common.DbConnectionStringBuilder CreateConnectionStringBuilder();
        bool IsProviderSupported
        {
            get;
        }

        /// <summary>
        /// When defined in a concrete implementation, this string must correspond with an installed data provider factory
        /// </summary>
        Type ViewType { get; }
        Type DbProviderFactoryType { get; }
        string ControllerName { get; }
        Type ControllerType { get; }
        string ViewName { get; }
        DbProviderFactory CreateDbProviderFactory();
        
    }
}
