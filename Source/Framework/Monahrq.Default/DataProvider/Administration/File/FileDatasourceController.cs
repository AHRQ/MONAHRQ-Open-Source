using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Sdk.DataProvider;
using Microsoft.Practices.ServiceLocation;

namespace Monahrq.Default.DataProvider.Administration.File
{
#if false   // unused since we removed the Test button
    public interface IFileDatasourceController
    {
        void SaveConnection(NamedConnectionElement namedConnectionElement);
        void TestConnection(NamedConnectionElement connection);
        void Cancel();
    }

    [Export(typeof(IFileDatasourceController))]
    public class FileDatasourceController : IFileDatasourceController
    {
        public void SaveConnection(Infrastructure.Configuration.NamedConnectionElement namedConnectionElement)
        {
            ServiceLocator.Current.GetInstance<IConfigurationService>().Save(namedConnectionElement);
        }

        public void TestConnection(NamedConnectionElement p)
        {
            var prov = Activator.CreateInstance(Type.GetType(p.ControllerType)) as IDataProviderController;
            var conn = prov.ProviderFactory.CreateConnection();
            conn.ConnectionString = p.ConnectionString;
            conn.Open();
            if (!string.IsNullOrEmpty(p.SelectFrom))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("Select * from [{0}]", p.SelectFrom);
                cmd.ExecuteReader();
            }
        }

        public void Cancel()
        { 
        }
    }
#endif
}
