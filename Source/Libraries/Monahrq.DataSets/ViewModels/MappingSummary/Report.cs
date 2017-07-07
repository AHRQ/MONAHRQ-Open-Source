using Monahrq.DataSets.Model;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace Monahrq.DataSets.ViewModels.MappingSummary
{
    /// <summary>
    /// The dataset wizard summary report view model.
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{Monahrq.DataSets.Model.DatasetContext}" />
    public class Report : WizardStepViewModelBase<DatasetContext>
    {
        /// <summary>
        /// The document name
        /// </summary>
        private readonly XName DocumentName = XName.Get("Report");
        /// <summary>
        /// The summary of variables name
        /// </summary>
        private readonly XName SummaryOfVariablesName = XName.Get("SummaryOfVariables");
        /// <summary>
        /// The file statistics name
        /// </summary>
        private readonly XName FileStatisticsName = XName.Get("FileStatisticsName");

        /// <summary>
        /// Initializes a new instance of the <see cref="Report"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public Report(DatasetContext context)
            : base(context)
        {
            Statistics = (Enum.GetValues(typeof(MappingStatistic)) as MappingStatistic[]).ToDictionary(k => k, v => 0);
        }

        /// <summary>
        /// Gets or sets the report description text.
        /// </summary>
        /// <value>
        /// The report description text.
        /// </value>
        private string ReportDescriptionText { get; set; }
        /// <summary>
        /// Gets or sets the statistics.
        /// </summary>
        /// <value>
        /// The statistics.
        /// </value>
        IDictionary<MappingStatistic, int> Statistics { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="System.Int32"/> with the specified stat.
        /// </summary>
        /// <value>
        /// The <see cref="System.Int32"/>.
        /// </value>
        /// <param name="stat">The stat.</param>
        /// <returns></returns>
        private int this[MappingStatistic stat]
        {
            get { return Statistics[stat]; }
            set { Statistics[stat] = value; }
        }

        /// <summary>
        /// The mapping statistic enumeration.
        /// </summary>
        public enum MappingStatistic
        {
            /// <summary>
            /// The variables in input file
            /// </summary>
            [Description("Variables In Input File")]
            VariablesInInputFile,
            /// <summary>
            /// The input variables mappedto monahrq variables
            /// </summary>
            [Description("Input Variables Mapped to MONAHRQ Variables")]
            InputVariablesMappedtoMONAHRQVariables,
            /// <summary>
            /// The unused input variables
            /// </summary>
            [Description("Unused Input Variables")]
            UnusedInputVariables,
            /// <summary>
            /// The unmapped required monahrq variables
            /// </summary>
            [Description("Unmapped Required MONAHRQ Variables")]
            UnmappedRequiredMONAHRQVariables,
            /// <summary>
            /// The unmapped monahrq variable warnings
            /// </summary>
            [Description("Unmapped MONAHRQ Variable Warning")]
            UnmappedMONAHRQVariableWarnings
        }

        /// <summary>
        /// HTMLs the element.
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        XElement HtmlElement(string elementName, string className, string content)
        {
            var items = new List<object>();
            if (!string.IsNullOrEmpty(className)) items.Add(new XAttribute("class", className));
            if (string.IsNullOrEmpty(content)) return new XElement(elementName, items.ToArray());
            return new XElement(elementName, items.ToArray()) { Value = content };
        }

        /// <summary>
        /// Gets the content of the document.
        /// </summary>
        /// <value>
        /// The content of the document.
        /// </value>
        public string DocumentContent
        {
            get
            {
                var body = HtmlElement("body", null, null);
                body.Add(HtmlElement("style", null, ReportConstants.Css));
                body.Add(HtmlElement("div", "title", ReportConstants.ScreenTitle));
                body.Add(HtmlElement("div", "description", ReportConstants.ReportDescription));
                BuildStats();
                body.Add(StatisticsTable);
                body.Add(VariablesTable);
                return body.ToString();
            }
        }

        /// <summary>
        /// Builds the stats.
        /// </summary>
        private void BuildStats()
        {
            Statistics[MappingStatistic.VariablesInInputFile] = DataContextObject.Histogram.FieldCount;

            Statistics[MappingStatistic.InputVariablesMappedtoMONAHRQVariables] =
                    DataContextObject.RequiredMappings.Concat(DataContextObject.OptionalMappings)
                    .Count(mapping=>!string.IsNullOrEmpty(mapping.Value));

            Statistics[MappingStatistic.UnusedInputVariables] = Statistics[MappingStatistic.VariablesInInputFile] 
                                    -  Statistics[MappingStatistic.InputVariablesMappedtoMONAHRQVariables];
            Statistics[MappingStatistic.UnmappedRequiredMONAHRQVariables] = 0;
            Statistics[MappingStatistic.UnmappedMONAHRQVariableWarnings] = DataContextObject.TargetElements.Count
                                    - Statistics[MappingStatistic.InputVariablesMappedtoMONAHRQVariables];
        }
        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public override string DisplayName
        {
            get { return ReportConstants.ScreenTitle; }
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return true;
        }

        /// <summary>
        /// The variable view model.
        /// </summary>
        /// <seealso cref="System.IComparable{Monahrq.DataSets.ViewModels.MappingSummary.Report.VariableViewModel}" />
        public class VariableViewModel: IComparable<VariableViewModel>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VariableViewModel"/> class.
            /// </summary>
            /// <param name="element">The element.</param>
            /// <param name="column">The column.</param>
            public VariableViewModel(Element element, DataColumn column)
            {
                Element = element;
                Source = column;
            }
            /// <summary>
            /// Gets or sets the element.
            /// </summary>
            /// <value>
            /// The element.
            /// </value>
            public Element Element
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the source.
            /// </summary>
            /// <value>
            /// The source.
            /// </value>
            public DataColumn Source
            {
                get;
                set;
            }

            /// <summary>
            /// Gets as HTML row.
            /// </summary>
            /// <value>
            /// As HTML row.
            /// </value>
            public XElement AsHtmlRow
            {
                get
                {
                    return new XElement("tr", new XAttribute("class", "variable-row" + (Source == null ? string.Empty : " unmapped")),
                        new XElement("td", 
                                new XAttribute("class", "element" + (Element.IsRequired ? " required" : string.Empty)))
                                { Value = Element == null ? string.Empty : Element.Description },
                        new XElement("td", new XAttribute("class", "source"))
                            { Value = Source == null ? "No Input Variable" : Source.ColumnName },
                        new XElement("td", new XAttribute("class", "ordinal")) 
                        { Value = Source == null ? "Unknown" : Source.Ordinal.ToString() });
                }
            }

            /// <summary>
            /// Gets the header.
            /// </summary>
            /// <value>
            /// The header.
            /// </value>
            public static XElement Header
            {
                get
                {
                    return new XElement("thead",
                        new XElement("tr",
                                new XElement("th", new XAttribute("class", "element")) { Value = ReportConstants.ElementHeader }
                                , new XElement("th", new XAttribute("class", "source")) { Value = ReportConstants.SourceHeader }
                                , new XElement("th", new XAttribute("class", "ordinal")) { Value = ReportConstants.OrdinalHeader }));
                }
            }

            /// <summary>
            /// Compares the current instance with another object of the same type and returns an integer that 
            /// indicates whether the current instance precedes, follows, or occurs in the same position in the
            /// sort order as the other object.
            /// </summary>
            /// <param name="other">An object to compare with this instance.</param>
            /// <returns>
            /// A value that indicates the relative order of the objects being compared. The return value has these meanings: 
            /// Value Meaning Less than zero This instance precedes <paramref name="other" /> in the sort order.  
            /// Zero This instance occurs in the same position in the sort order as <paramref name="other" />.
            /// Greater than zero This instance follows <paramref name="other" /> in the sort order.
            /// </returns>
            public int CompareTo(VariableViewModel other)
            {
                if (other == null) return -1;
                if (other.Element == null) return -1;
                if (this.Element == null) return 1;
                if (this.Element.IsRequired && !other.Element.IsRequired) return -1;
                if (!this.Element.IsRequired && other.Element.IsRequired) return 1;
                if (this.Element.IsRequired && other.Element.IsRequired)
                {
                    return this.Element.Ordinal.CompareTo(other.Element.Ordinal);
                }
                if(this.Source == null && other.Source == null )
                {
                    return this.Element.Ordinal.CompareTo(other.Element.Ordinal);
                }
                if (this.Source == null && other.Source != null) return 1;
                if (this.Source != null && other.Source == null) return -1;

                return this.Source.Ordinal.CompareTo(other.Source.Ordinal);
            }
        }

        /// <summary>
        /// Gets the statistics table.
        /// </summary>
        /// <value>
        /// The statistics table.
        /// </value>
        public XElement StatisticsTable
        {
            get
            {
                var table = HtmlElement("table", "statistics", string.Empty);
                var tbody = HtmlElement("tbody", null, string.Empty);
                table.Add(tbody);
                foreach (var stat in Enum.GetValues(typeof(MappingStatistic)).OfType<MappingStatistic>())
                {
                    var row = HtmlElement("tr", null, null);
                    var td = HtmlElement("td", "category", EnumHelper<MappingStatistic>.GetEnumDescription(stat));
                    row.Add(td);
                    row.Add(HtmlElement("td", "value", Statistics[stat].ToString("N0")));
                    tbody.Add(row);
                }
                return table;
            }
        }

        /// <summary>
        /// Gets the variables table.
        /// </summary>
        /// <value>
        /// The variables table.
        /// </value>
        public XElement VariablesTable
        {
            get
            {
                var elements = DataContextObject.TargetElements;
                var columns = DataContextObject.Histogram.FieldEntries.ToDictionary(k => k.Column.ColumnName.ToUpper(), v => v.Column);
                var mappings = DataContextObject.RequiredMappings.Concat(DataContextObject.OptionalMappings).ToDictionary(k=>k.Key.ToUpper(), v=>v.Value);
                var variables = elements.Select((candidate) =>
                        {
                            var temp = new VariableViewModel(candidate, null);
                            string columnName;
                            var foundColumn = mappings.TryGetValue(candidate.Name.ToUpper(), out columnName);
                            if (!foundColumn )
                            {
                                return temp;
                            }
                            DataColumn col;
                            foundColumn = columns.TryGetValue(columnName.ToUpper(), out col);
                            temp.Source = foundColumn ? col : null;
                            return temp;
                        });
                return new XElement("table", new XAttribute("class", "variables"),
                    VariableViewModel.Header,
                    variables.Where(item=>item.Element!=null)
                    .OrderBy(item => item).Select(variable => variable.AsHtmlRow));
            }
        }
    }

    /// <summary>
    /// The variable model.
    /// </summary>
    class VariableModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableModel"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="source">The source.</param>
        public VariableModel(Element element, FieldEntry source)
        {
            Element = element;
            Source = source;
        }

        /// <summary>
        /// Gets or sets the element.
        /// </summary>
        /// <value>
        /// The element.
        /// </value>
        public Element Element { get; set; }
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public FieldEntry Source {get;set;}
        /// <summary>
        /// Gets the ordinal.
        /// </summary>
        /// <value>
        /// The ordinal.
        /// </value>
        public int? Ordinal
        {
            get
            {
                return Element == null || Source == null ? null : (int?)Source.Column.Ordinal;
            }
        }

    }

    /// <summary>
    /// The local enumeration (enum) utility/helper class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    static class EnumHelper<T>
    {
        /// <summary>
        /// Gets the enum description.
        /// </summary>
        /// <param name="anEnum">An enum.</param>
        /// <returns></returns>
        public static string GetEnumDescription(object anEnum)
        {
            Type type = typeof(T);
            var field = type.GetField(anEnum.ToString());
            if (field == null) return string.Empty;
            var customAttribute = field.GetCustomAttribute<DescriptionAttribute>(false);
            return customAttribute == null ? anEnum.ToString() : customAttribute.Description;
        }
    }
}
