
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Monahrq.Sdk.DataProvider.Builder
{
    public interface IConnectionStringView
    {
        ConnectionStringViewModel Model { get; }
    }

    public interface IFileConnectionStringView : IConnectionStringView
    {
        string Filename { get; set; }
    }
}