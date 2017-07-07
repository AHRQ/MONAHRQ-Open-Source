using System;
using System.Linq;

namespace Monahrq.Sdk.Generators
{
    /// <summary>
    /// The ReportGeneratorAttribute designates that a report generator should be loaded into Monahrq during initialization.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ReportGeneratorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportGeneratorAttribute" /> class.
        /// </summary>
        /// <param name="reportIds">The report ids.</param>
        /// <param name="moduleDependencies">The module dependencies.</param>
        /// <param name="datasetTargetDependencies">The dataset target dependencies.</param>
        /// <param name="executionOder"></param>
        /// <param name="messageOverride"></param>
        public ReportGeneratorAttribute(string[] reportIds, string[] moduleDependencies, Type[] datasetTargetDependencies, int executionOder = 0, string messageOverride = null, string eventRegion = null) 
        {
            ReportIds = reportIds ?? new string[] { };
            ModuleDependencies = moduleDependencies ?? new string[] { };
            DatasetTargetDependencies = datasetTargetDependencies ?? new Type[] { };
            MessageOverride = messageOverride;
            EventRegion = eventRegion;
            ExecutionOrder = executionOder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportGeneratorAttribute"/> class.
        /// </summary>
        public ReportGeneratorAttribute()
        {
        }

        /// <summary>
        /// Gets the report ids associated with the report generator.
        /// </summary>
        /// <value>
        /// The report ids.
        /// </value>
        public string[] ReportIds { get; private set; }
        /// <summary>
        /// Gets the arry of any module dependencies the generator may have.
        /// </summary>
        /// <value>
        /// The module dependencies.
        /// </value>
        public string[] ModuleDependencies { get; private set; }
        /// <summary>
        /// Gets the dataset target dependencies the generator may have.
        /// </summary>
        /// <value>
        /// The dataset target dependencies.
        /// </value>
        public Type[] DatasetTargetDependencies { get; private set; }
        /// <summary>
        /// Gets the message override for the website generation/status log.
        /// </summary>
        /// <value>
        /// The message override.
        /// </value>
        public string MessageOverride { get; private set; }
        /// <summary>
        /// Gets the event region name for the website generation/status log.
        /// </summary>
        /// <value>
        /// The event region.
        /// </value>
        public string EventRegion { get; private set; }
        /// <summary>
        /// Gets or sets the execution order in which the report generator will be executed in.
        /// </summary>
        /// <value>
        /// The execution order.
        /// </value>
        public int ExecutionOrder { get; set; }
    }
}
