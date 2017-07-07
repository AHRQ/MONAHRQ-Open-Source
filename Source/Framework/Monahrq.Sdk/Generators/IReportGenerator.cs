using System.Collections.Generic;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Sdk.Services.Generators;
using Monahrq.Infrastructure.Entities.Domain.Reports;

namespace Monahrq.Sdk.Generators
{
    /// <summary>
    /// The interface that all first party Monahrq report generators implement to be imported into Monahrq during application iniialization.
    /// </summary>
    public interface IReportGenerator
    {
        /// <summary>
        /// Gets or sets the <see cref="Website"/> for which a report is being generated
        /// </summary>
        Website CurrentWebsite { get; set; }

        /// <summary>
        /// Gets the string representation of report GUIDs for which this <see cref="IReportGenerator"/> can generate reports
        /// </summary>
        string[] ReportIds { get; }

        /// <summary>
        /// Gets or sets the <see cref="Report"/> currently being generated
        /// </summary>
        Report ActiveReport { get; set; }

        /// <summary>
        /// Gets the execution order.
        /// </summary>
        /// <value>
        /// The execution order.
        /// </value>
        int ExecutionOrder { get; }

        /// <summary>
        /// Checks if can run.
        /// </summary>
        /// <returns></returns>
        bool CheckIfCanRun();
        
        /// <summary>
        /// Prepares the generator for use. This method is executed during startup or when switching/initializing databases.
        /// </summary>
        void InitGenerator();

        /// <summary>
        /// Generates the report.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="publishTask">The publish task.</param>
        void GenerateReport(Website website, PublishTask publishTask);

        /// <summary>
        /// Validate report dependencies
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="validationResults">The validation results.</param>
        /// <returns>A boolean value indicating whether report generation should continue</returns>
        bool ValidateDependencies(Website website, IList<ValidationResult> validationResults);
    }

    /// <summary>
    /// The Sql Data Object Type Enum
    /// </summary>
    public enum DataObjectTypeEnum
    {
        StoredProcedure,
        Table,
        Type,
        UserDefinedFunction,
        UserDefinedView
    }

}
