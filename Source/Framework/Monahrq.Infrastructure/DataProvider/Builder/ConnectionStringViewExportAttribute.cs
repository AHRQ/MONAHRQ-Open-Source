using System;
using Monahrq.Sdk.Attributes;

namespace Monahrq.Sdk.DataProvider.Builder
{
    public class ConnectionStringViewExportAttribute
        : ViewExportAttribute
    {
        public ConnectionStringViewExportAttribute(string contractName, Type viewType)
            : base(contractName, viewType)
        {
        }
    }
}
