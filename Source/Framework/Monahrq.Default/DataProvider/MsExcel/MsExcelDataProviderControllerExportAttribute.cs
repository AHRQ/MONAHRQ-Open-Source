using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monahrq.Sdk.DataProvider;
using System.Data.OleDb;

namespace Monahrq.Default.DataProvider.MsExcel
{
    /// <summary>
    /// Ms-Excel data provider controller
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.DataProvider.OleDbDataProviderController" />
    [DataProviderControllerExport(
        DataProviderStrings.MsExcelAceProviderControllerName
        , typeof(MsExcelAceDataProviderController)
        , typeof(MsExcelConnectionStringView)
        , SupportedPlatforms.All
        , "Provider=Microsoft.ACE.OLEDB.12.0")]
    public class MsExcelAceDataProviderController : OleDbDataProviderController
    {
    }
}
