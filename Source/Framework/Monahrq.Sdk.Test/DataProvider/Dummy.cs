using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Sdk.DataProvider;
using Monahrq.Sdk.DataProvider.Builder;
using System.Data.SqlClient;

namespace Monahrq.Sdk.Test.DataProvider
{

    static class DummyNames
    {
        public const string ProviderName =      "DummyDataProvider";
        public const string ControllerName =    "dummycontroller";
        public const string ViewName =     "dummyView";
    }

    [DataProviderControllerExport(DummyNames.ControllerName,
        typeof(DummyProviderController),
        typeof(DummyStringView),
        SupportedPlatforms.All, "")]
    public class DummyProviderController : DataProviderController<OleDbFactory>
    {
    }

    public class DummyViewBase : IConnectionStringView
    {
        public DummyViewBase()
        {
            Model = new ConnectionStringViewModel();
        }

        public ConnectionStringViewModel Model
        {
            get;
            private set;
        }
    }

    [Export(DummyNames.ViewName, typeof(IConnectionStringView))]
    public class DummyStringView : DummyViewBase
    {
    }

    static class AnotherDummyNames
    {
        public const string ProviderName =     "AnotherDummyDataProvider";
        public const string ControllerName =   "anotherdummycontroller";
        public const string ViewName =    "anotherdummyView";
    }

    [DataProviderControllerExport(AnotherDummyNames.ControllerName,
        typeof(AnotherDummyProviderController),
        typeof(AnotherDummyStringView),
        SupportedPlatforms.All, "")]
    public class AnotherDummyProviderController : DataProviderController<OleDbFactory>
    {
    }


    [Export(AnotherDummyNames.ViewName, typeof(IConnectionStringView))]
    public class AnotherDummyStringView : DummyViewBase
    {
    }

}
