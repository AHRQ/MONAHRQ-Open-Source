using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Theme.Controls.Wizard.Models;
using Monahrq.Wing.Dynamic.Models;

namespace Monahrq.Wing.Dynamic.ViewModels
{
    /// <summary>
    /// View model class for full wizard report
    /// </summary>
    /// <seealso cref="Monahrq.Theme.Controls.Wizard.Models.WizardStepViewModelBase{Monahrq.Wing.Dynamic.Models.WizardContext}" />
    public class FullWizrdReportViewModel : WizardStepViewModelBase<WizardContext>
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
        /// Initializes a new instance of the <see cref="FullWizrdReportViewModel"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public FullWizrdReportViewModel(WizardContext context)
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
        /// Gets or sets the <see cref="System.Int32"/> with the specified MappingStatistic.
        /// </summary>
        /// <value>
        /// The <see cref="System.Int32"/>.
        /// </value>
        /// <param name="stat">The MappingStatistic enum value.</param>
        /// <returns></returns>
        private int this[MappingStatistic stat]
        {
            get { return Statistics[stat]; }
            set { Statistics[stat] = value; }
        }

        /// <summary>
        /// Mapping Statistic enum
        /// </summary>
        public enum MappingStatistic
        {
            [Description("Variables In Input File")]
            VariablesInInputFile,
            [Description("Input Variables Mapped to MONAHRQ Variables")]
            InputVariablesMappedtoMONAHRQVariables,
            [Description("Unused Input Variables")]
            UnusedInputVariables,
            [Description("Unmapped Required MONAHRQ Variables")]
            UnmappedRequiredMONAHRQVariables,
            [Description("Unmapped MONAHRQ Variable Warning")]
            UnmappedMONAHRQVariableWarnings
        }

        /// <summary>
        /// To create and return the html element 
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        XElement HtmlElement(string elementName, string className, string content)
        {
            var items = new List<object>();
            if (!String.IsNullOrEmpty(className)) items.Add(new XAttribute("class", className));
            if (String.IsNullOrEmpty(content)) return new XElement(elementName, items.ToArray());
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
                body.Add(HtmlElement("style", null, CSS));
                body.Add(HtmlElement("div", "title", SCREEN_TITLE));
                body.Add(HtmlElement("div", "description", REPORT_DESCRIPTION));
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
                    .Count(mapping=>!String.IsNullOrEmpty(mapping.Value));

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
            get { return SCREEN_TITLE; }
        }

        /// <summary>
        /// Returns true if this instance is valid.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsValid()
        {
            return true;
        }

        /// <summary>
        /// View model class for  various variable names
        /// </summary>
        /// <seealso cref="System.IComparable{Monahrq.Wing.Dynamic.ViewModels.FullWizrdReportViewModel.VariableViewModel}" />
        public class VariableViewModel: IComparable<VariableViewModel>
        {
            /// <summary>
            /// The element header
            /// </summary>
            private const string ELEMENT_HEADER = @"MONAHRQ data element";
            /// <summary>
            /// The source header
            /// </summary>
            private const string SOURCE_HEADER = @"Input file data element";
            /// <summary>
            /// The ordinal header
            /// </summary>
            private const string ORDINAL_HEADER = @"Element position in your file";
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
            /// Gets a HTML row.
            /// </summary>
            /// <value>
            /// As HTML row.
            /// </value>
            public XElement AsHtmlRow
            {
                get
                {
                    return new XElement("tr", new XAttribute("class", "variable-row" + (Source == null ? String.Empty : " unmapped")),
                        new XElement("td", 
                                new XAttribute("class", "element" + (Element.IsRequired ? " required" : String.Empty)))
                                { Value = Element == null ? String.Empty : Element.Description },
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
                                new XElement("th", new XAttribute("class", "element")) { Value = ELEMENT_HEADER }
                                , new XElement("th", new XAttribute("class", "source")) { Value = SOURCE_HEADER }
                                , new XElement("th", new XAttribute("class", "ordinal")) { Value = ORDINAL_HEADER }));
                }
            }

            /// <summary>
            /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
            /// </summary>
            /// <param name="other">An object to compare with this instance.</param>
            /// <returns>
            /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="other" /> in the sort order.  Zero This instance occurs in the same position in the sort order as <paramref name="other" />. Greater than zero This instance follows <paramref name="other" /> in the sort order.
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
                var table = HtmlElement("table", "statistics", String.Empty);
                var tbody = HtmlElement("tbody", null, String.Empty);
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


        #region Constants

        private const string CSS = @"body {
    font-family: Arial, Helvetica, sans-serif;
    width: 7in;
    overflow: scroll;
}

div.title {
    font-size: 1.2em;
    font-weight: bold;
}

div.description {
    font-style: italic;
    font-size: .9em;
    color: blue;
}

table.statistics,
table.variables {
    position: relative;
    top: 10px;
}

table.statistics {
    margin-bottom: 10px;
}

    table.statistics tbody {
        font-size: .9em;
    }

 table.statistics tbody td{
        padding: 1px 5px 1px 0px;
    }

 table.statistics tbody td.category{
        font-weight: bold;
    }

 table.statistics tbody td.value{
        text-align: right;
    }


table.variables {
    position: relative;
    top: 10px;
}

    table.variables caption {
        text-align: left;
        font-weight: bold;
        font-size: 1.1em;
    }

    table.variables thead th,
    table.variables tbody td {
        text-align: left;
        padding: 0px 10px 0px 10px;
    }


    table.variables tbody {
        font-size: .9em;
    }

        table.variables tbody td.required {
            font-weight: bold;
        }

        table.variables tbody tr.unmapped {
            color: green;
        }";

        private const string SCREEN_TITLE = @"Mapping Summary";
        private const string REPORT_DESCRIPTION = @"This report summarizes the Data Mapping between the input file and the MONAHRQ Dataset that you have assigned on the previous screen. Certain variables are required to continue with the data analysis. See the Host User Guide for more detailed information.";

        #endregion
    }
}
