using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Configuration;

namespace Monahrq.Sdk.DataProvider.Builder
{
    public static class NamedConnectionElementExtension
    {
        public static Type GetDataProviderType (this NamedConnectionElement element)
        {
            return Type.GetType(element.ControllerType);
        }

        public static IDataProviderController GetDataProviderController(this NamedConnectionElement element)
        {
            return Activator.CreateInstance(element.GetDataProviderType()) as IDataProviderController;
        }

        public static IDataProviderControllerExportAttribute GetExportAttribute(this NamedConnectionElement element)
        {
            try
            {
                if (string.IsNullOrEmpty(element.ControllerType)) return null; 
                var provType = element.GetDataProviderType();
                return element.GetDataProviderType().GetCustomAttributes(typeof(IDataProviderControllerExportAttribute), true)
                    .FirstOrDefault() as IDataProviderControllerExportAttribute;
            }
            catch
            {
                return null;
            }
        }

        public static DbConnectionStringBuilder CreateConnectionStringBuilder(this NamedConnectionElement element)
        {
            return element.GetExportAttribute().CreateConnectionStringBuilder();
        }
    }
}
