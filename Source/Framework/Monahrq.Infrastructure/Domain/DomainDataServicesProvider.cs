using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Data;


namespace Monahrq.Infrastructure.Entities.Domain
{

    [Export(typeof(IDataServicesProvider))]
    public class DomainDataServicesProvider: SqlServerDataServicesProvider<DomainDataServicesProvider>
    {

    }
}
