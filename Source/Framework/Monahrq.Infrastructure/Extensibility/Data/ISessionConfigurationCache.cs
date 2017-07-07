using System;
using Monahrq.Infrastructure;


namespace Monahrq.Sdk.Extensibility.Data
{
    using NHibernate.Cfg;
    public interface ISessionConfigurationCache
    {
        Configuration GetConfiguration(Func<Configuration> builder);
        void Reset();
    }
}