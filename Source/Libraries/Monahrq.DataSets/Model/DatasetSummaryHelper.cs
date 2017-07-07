using Monahrq.Theme.Controls.Wizard.Models;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Monahrq.DataSets.Model
{
    /// <summary>
    /// The dataset summary helper/utility class.
    /// </summary>
    public class DatasetSummaryHelper
    {
        /// <summary>
        /// Gets or sets the progress.
        /// </summary>
        /// <value>
        /// The progress.
        /// </value>
        ProgressState Progress 
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the mappings helper.
        /// </summary>
        /// <value>
        /// The mappings helper.
        /// </value>
        MappingsHelper MappingsHelper { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetSummaryHelper"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="progress">The progress.</param>
        /// <param name="required">The required.</param>
        /// <param name="optional">The optional.</param>
        public DatasetSummaryHelper(
            DatasetContext context,
            ProgressState progress,
            MappingDictionary required,
            MappingDictionary optional)
        {
            Progress = progress ?? new ProgressState(0, 1);
            MappingsHelper = new MappingsHelper(required, optional);
            var attrs = new List<XAttribute>();

            if (!string.IsNullOrEmpty(context.Year))
                attrs.Add(new XAttribute("Year", context.Year ?? string.Empty));

            if(context.Quarter.HasValue)
            {
                attrs.Add( new XAttribute("Quarter", context.Quarter.GetValueOrDefault()));
            }
            ReportingPeriod = new XElement("ReportingPeriod", attrs.ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasetSummaryHelper"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        public DatasetSummaryHelper(XElement element)
        {
            var progElement = element.Element(progessName);
            Progress = new ProgressState(
                    int.Parse(progElement.Attribute(countName).Value),
                    int.Parse(progElement.Attribute(totalName).Value));
            MappingsHelper = new MappingsHelper(element);
        }

        /// <summary>
        /// The summary name
        /// </summary>
        const string summaryName = "summary";
        /// <summary>
        /// The progess name
        /// </summary>
        const string progessName = "progress";
        /// <summary>
        /// The total name
        /// </summary>
        const string totalName = "total";
        /// <summary>
        /// The count name
        /// </summary>
        const string countName = "current";

        /// <summary>
        /// Gets the element as a <see cref="XElement"/>.
        /// </summary>
        /// <value>
        /// As element.
        /// </value>
        public XElement AsElement
        {
            get
            {
                return new XElement(summaryName,
                    ReportingPeriod,
                    new XElement(progessName, 
                        new XAttribute(totalName, Progress.Total),
                        new XAttribute(countName, Progress.Current)),
                        MappingsHelper.AsXElement);
            }
        }

        /// <summary>
        /// Gets or sets the reporting period.
        /// </summary>
        /// <value>
        /// The reporting period.
        /// </value>
        public XElement ReportingPeriod { get; set; }
    }

    /// <summary>
    /// The dataset wizard mappings helper/utility ckass.
    /// </summary>
    public class MappingsHelper
    {
        const string mappingsName = "mappings";
        const string optionalName = "optional";
        const string requiredName = "required";
        const string mappingName = "mapping";
        const string keyName = "key";
        const string valueName = "value";

        /// <summary>
        /// Gets or sets the required.
        /// </summary>
        /// <value>
        /// The required.
        /// </value>
        MappingDictionary Required
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the optional.
        /// </summary>
        /// <value>
        /// The optional.
        /// </value>
        MappingDictionary Optional
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingsHelper"/> class.
        /// </summary>
        /// <param name="required">The required.</param>
        /// <param name="optional">The optional.</param>
        public MappingsHelper(MappingDictionary required,
            MappingDictionary optional)
        {
            Required = required ?? new MappingDictionary();
            Optional = optional ?? new MappingDictionary();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingsHelper"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        public MappingsHelper(XElement element): this (null, null)
        {
            LoadFromElement(element.Descendants(requiredName).First(), Required);
            LoadFromElement(element.Descendants(optionalName).First(), Optional);
        }

        /// <summary>
        /// Loads from element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="optional">The optional.</param>
        private void LoadFromElement(XElement element, MappingDictionary optional)
        {
            foreach (var mapping in element.Elements(mappingName))
            {
                Required[mapping.Attribute(keyName).Value] = mapping.Attribute(valueName).Value;
            }
        }

        /// <summary>
        /// Gets as x element.
        /// </summary>
        /// <value>
        /// As x element.
        /// </value>
        public XElement AsXElement
        {
            get
            {
                return new XElement(mappingsName,
                    CreateMappingElement(requiredName, Required),
                    CreateMappingElement(optionalName, Optional));
            }
        }

        /// <summary>
        /// Creates the mapping element.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public XElement CreateMappingElement(string name, MappingDictionary items)
        {
            return new XElement(name,
                        items.Select(kvp =>
                            new XElement(mappingName,
                                new XAttribute(keyName, kvp.Key),
                                new XAttribute(valueName, kvp.Value))));
        }
    }

}
