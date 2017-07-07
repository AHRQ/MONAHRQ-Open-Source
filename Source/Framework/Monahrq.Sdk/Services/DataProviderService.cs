using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Monahrq.Sdk.DataProvider;

namespace Monahrq.Sdk.Services
{
    [Export(typeof(IDataProviderService))]
    public class DataProviderService : IDataProviderService
    {
        [ImportMany]
        public IEnumerable<IDataProviderController> Controllers
        {
            get;
            private set;
        }

        public IDataProviderController GetController(string controllerName)
        {
            return Controllers.FirstOrDefault(ctrl => ctrl.GetExportAttribute().ControllerName == controllerName);
        }

        public IEnumerable<IDataProviderControllerExportAttribute> GetRegisteredProviderExports()
        {
            return Controllers.Select(ctrl => ctrl.GetExportAttribute());
        }
        
    }
}
