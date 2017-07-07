using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

namespace Monahrq.Sdk.Extensibility.Data.Providers
{
    public interface IDataServicesProviderFactory 
    {
        IDataServicesProvider CreateProvider(DataServiceParameters sessionFactoryParameters);
    }
}
