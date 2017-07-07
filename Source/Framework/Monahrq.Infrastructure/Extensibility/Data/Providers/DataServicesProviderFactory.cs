using System.ComponentModel.Composition;
using Monahrq.Infrastructure.FileSystem;

namespace Monahrq.Sdk.Extensibility.Data.Providers
{
    public delegate IDataServicesProvider CreateDataServicesProvider(string dataFolder, string connectionString);

    public class DataServicesProviderFactory : IDataServicesProviderFactory
    {
        private readonly CreateDataServicesProvider _provider;

        public DataServicesProviderFactory(CreateDataServicesProvider provider)
        {
            _provider = provider;
        }

        public IDataServicesProvider CreateProvider(DataServiceParameters parameters)
        {
            return _provider(parameters.DataFolder, parameters.ConnectionString);
        }
       
    }


    [Export(typeof(IDataServicesProviderFactory))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MonahrqDataServicesProviderFactory : IDataServicesProviderFactory
    {
        [Import]
        public IUserFolder UserFolder { get; set; }

        public IDataServicesProvider CreateProvider(DataServiceParameters sessionFactoryParameters)
        {
            return new SqlServerDataServicesProvider(sessionFactoryParameters.DataFolder,
                        sessionFactoryParameters.ConnectionString, UserFolder);
        }
    }
   
}
