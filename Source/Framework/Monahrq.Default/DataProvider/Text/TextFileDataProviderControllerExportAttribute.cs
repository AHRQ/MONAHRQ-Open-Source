using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monahrq.Sdk.DataProvider;
using System.Data.OleDb;

namespace Monahrq.Default.DataProvider.Text
{
    /// <summary>
    /// class for text file data provider controller
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.DataProvider.OleDbDataProviderController" />
    [DataProviderControllerExport(
        DataProviderStrings.TextFileAceProviderControllerName
        , typeof(TextAceDataProviderController)
        , typeof(TextFileConnectionStringView)
        , SupportedPlatforms.All
        , "Provider=Microsoft.ACE.OLEDB.12.0")]
    public class TextAceDataProviderController : OleDbDataProviderController
    {
    }
}
