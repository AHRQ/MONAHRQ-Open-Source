using System;
using System.ComponentModel;

namespace Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records
{
    /// <summary>
    /// The DRG/MDC mapping status enumeration. This status is utlized with in the MS-DRG grouper functionality.
    /// </summary>
    [Serializable]
    public enum DrgMdcMappingStatusEnum
    {
        /// <summary>
        /// The not processed
        /// </summary>
        [Description("Not Processed")]
        NotProcessed,
        /// <summary>
        /// The intializing
        /// </summary>
        Intializing,
        /// <summary>
        /// The in progress
        /// </summary>
        [Description("In Progress")]
        InProgress,
        /// <summary>
        /// The completed
        /// </summary>
        Completed,
        /// <summary>
        /// The error
        /// </summary>
        Error,
        /// <summary>
        /// The pending
        /// </summary>
        [Description("Pending - Please click the \"DRC/MDC mapping process\" button.")]
        Pending,
        /// <summary>
        /// The unknown
        /// </summary>
        Unknown
    }
}
