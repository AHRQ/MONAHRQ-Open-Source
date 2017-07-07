using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Sdk.DataProvider
{
    public static class IDataProviderControllerExtensions
    {
        public static IDataProviderControllerExportAttribute GetExportAttribute(this IDataProviderController controller)
        {
            var export = typeof(DataProviderControllerExportAttribute);
            return controller.GetType().GetCustomAttributes(export, true).First() as IDataProviderControllerExportAttribute;
        }

        public static T GetProviderFactory<T>(this IDataProviderController<T> controller)
        where T : DbProviderFactory
        {
            var export = controller.GetExportAttribute();
            var factory = export.DbProviderFactoryType;
            var providerRow = DbProviderFactories.GetFactoryClasses()
                .Rows.OfType<DataRow>()
                .Where(row=>row["AssemblyQualifiedName"].ToString().Equals(factory.AssemblyQualifiedName))
                .First();
            return DbProviderFactories.GetFactory(providerRow) as T;
        }

    }
}
