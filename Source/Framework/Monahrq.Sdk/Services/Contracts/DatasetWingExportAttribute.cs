using System;
using System.ComponentModel.Composition;

namespace Monahrq.Sdk.Services.Contracts
{
    /// <summary>
    /// Specifies that a class exports a <see cref="IDatasetWing"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DatasetWingExportAttribute : ExportAttribute
    {
        public DatasetWingExportAttribute()
            : base(typeof(IDatasetWing))
        {
        }
    }
}