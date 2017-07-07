using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;

using Monahrq.Sdk.DataProvider;
using PropertyChanged;

namespace Monahrq.Sdk.DataProvider.Builder
{
    [ImplementPropertyChanged]
    public class ConnectionStringViewModel
    {
        //public bool IsChanged { get; set; }

        protected virtual DbConnectionStringBuilder Builder { get; set; }
        
        private NamedConnectionElement InternalConfiguration { get; set; }

        public NamedConnectionElement Configuration
        {
            get
            {
                return InternalConfiguration;
            }
            set
            {
                InternalConfiguration = value;
                if (!string.IsNullOrEmpty(value.ConnectionString))
                {
                    Builder.ConnectionString = value.ConnectionString;
                }
                else
                {
                    value.ConnectionString = Builder.ConnectionString;
                }
            }
        }
        public ConnectionStringViewModel()
        {
        }

        public virtual string GetConnectionString()
        {
            return Builder.ConnectionString;
        }

        public void Reset(IDataProviderController provider)
        {
            Builder = provider.GetExportAttribute().CreateConnectionStringBuilder();
            Configuration = new NamedConnectionElement() { ControllerType = provider.GetType().AssemblyQualifiedName };
        }

        public bool IsValid
        {
            get
            {
                return InternalConfiguration.IsValid;
            }
        }
    }
}
