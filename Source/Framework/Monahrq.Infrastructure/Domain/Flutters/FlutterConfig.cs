using System.Runtime.Serialization;

namespace Monahrq.Infrastructure.Domain.Flutters
{
    public class FlutterAssets
    {
        [DataMember(Name="scripts")]
        public string[] Scripts { get; set; }

        [DataMember(Name="styles")]
        public string[] Styles { get; set; }

        [DataMember(Name="templates")]
        public string[] Templates { get; set; }
    }

    public class FlutterRoute
    {
        [DataMember(Name="name")]
        public string Name { get; set; }

        [DataMember(Name="params")]
        public object Params { get; set; }
    }

    public class FlutterMenuItem
    {
        [DataMember(Name="menu")]
        public string Menu { get; set; }

        [DataMember(Name="id")]
        public string Id { get; set; }

        [DataMember(Name="reportId")]
        public string ReportId { get; set; }

        [DataMember(Name="label")]
        public string Label { get; set; }

        [DataMember(Name="priority")]
        public int Priority { get; set; }

        [DataMember(Name="primary")]
        public bool Primary { get; set; }

        [DataMember(Name="classes")]
        public object[] Classes { get; set; }

        [DataMember(Name="route")]
        public FlutterRoute Route { get; set; }
    }

    public class FlutterPage
    {
        [DataMember(Name="title")]
        public string Title { get; set; }

        [DataMember(Name="header")]
        public string Header { get; set; }

        [DataMember(Name="footer")]
        public string Footer { get; set; }
    }

    public class FlutterColumn
    {
        [DataMember(Name="name")]
        public string Name { get; set; }

        [DataMember(Name="label")]
        public string Label { get; set; }

        [DataMember(Name="format")]
        public string Format { get; set; }

        [DataMember(Name="formatOptions")]
        public int[] FormatOptions { get; set; }
    }

    public class FlutterTable
    {
        [DataMember(Name="hasGlobalSearch")]
        public bool HasGlobalSearch { get; set; }

        [DataMember(Name="hasPager")]
        public bool HasPager { get; set; }

        [DataMember(Name="columns")]
        public FlutterColumn[] Columns { get; set; }
    }

    public class FlutterDetails
    {
        [DataMember(Name="rootObj")]
        public string RootObj { get; set; }

        [DataMember(Name="reportName")]
        public string ReportName { get; set; }

        [DataMember(Name="reportDir")]
        public string ReportDir { get; set; }

        [DataMember(Name="filePrefix")]
        public string FilePrefix { get; set; }
    }

    public class Custom
    {
        [DataMember(Name="table")]
        public FlutterTable Table { get; set; }

        [DataMember(Name="details")]
        public FlutterDetails Details { get; set; }
    }

    public class FlutterReport
    {
        [DataMember(Name="id")]
        public string Id { get; set; }

        [DataMember(Name = "type", IsRequired = false)]
        public string Type { get; set; }
        
        [DataMember(Name="displayName")]
        public string DisplayName { get; set; }

        [DataMember(Name="page")]
        public FlutterPage Page { get; set; }

        [DataMember(Name="custom")]
        public Custom Custom { get; set; }
    }

    public class FlutterConfig
    {
        [DataMember(Name="id")]
        public string Id { get; set; }

        [DataMember(Name="displayName")]
        public string DisplayName { get; set; }

        [DataMember(Name="moduleName")]
        public string ModuleName { get; set; }

        [DataMember(Name="assets")]
        public FlutterAssets Assets { get; set; }

        [DataMember(Name="menuItems")]
        public FlutterMenuItem[] MenuItems { get; set; }

        [DataMember(Name="reports")]
        public FlutterReport[] Reports { get; set; }
    }

}
