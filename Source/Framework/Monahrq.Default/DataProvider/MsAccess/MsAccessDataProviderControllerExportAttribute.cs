using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Sdk.DataProvider;
using Monahrq.Sdk.DataProvider.Builder;
using System.Data.OleDb;

namespace Monahrq.Default.DataProvider.MsAccess
{
    /// <summary>
    /// class for ms access data provider
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.DataProvider.OleDbDataProviderController" />
    [DataProviderControllerExport(
        DataProviderStrings.MsAccessAceProviderControllerName
        , typeof(MsAccessAceDataProviderController)
        , typeof(MsAccessConnectionStringView)
        , SupportedPlatforms.All
        , "Provider=Microsoft.ACE.OLEDB.12.0")]
    public class MsAccessAceDataProviderController : OleDbDataProviderController
    {
    }
}
