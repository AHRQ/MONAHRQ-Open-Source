using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monahrq.Sdk.Extensibility.Builders.Models;

namespace Monahrq.Sdk.Extensibility.Data.Providers 
{
    public class SessionFactoryParameters : DataServiceParameters
    {
        public IEnumerable<RecordBlueprint> RecordDescriptors { get; set; }
        public bool CreateDatabase { get; set; }
    }
}
