using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Monahrq.Infrastructure.Entities.Events;

namespace Monahrq.Infrastructure.Entities.Core.Import
{
    /// <summary>
    /// The monahrq data loader interface.
    /// </summary>
    public interface IDataLoader
    {
        /// <summary>
        /// Loads the data.
        /// </summary>
        void LoadData();
        /// <summary>
        /// Gets the reader.
        /// </summary>
        /// <value>
        /// The reader.
        /// </value>
        ReaderDefinition Reader { get; }
        /// <summary>
        /// Gets the data provider.
        /// </summary>
        /// <value>
        /// The data provider.
        /// </value>
        IDataReaderDictionary DataProvider { get; }
        /// <summary>
        /// Occurs when [on feedback].
        /// </summary>
        event EventHandler<ExtendedEventArgs<string>> OnFeedback;
        /// <summary>
        /// Gets a value indicating whether this instance is background.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is background; otherwise, <c>false</c>.
        /// </value>
        bool IsBackground { get; }
    }

    /// <summary>
    /// Custom data loader extension methods class.
    /// </summary>
    public static class IDataLoaderExtension
    {
        /// <summary>
        /// Prioritizes the specified loaders.
        /// </summary>
        /// <param name="loaders">The loaders.</param>
        /// <returns></returns>
        public static IEnumerable<IDataLoader> Prioritize(this IEnumerable<IDataLoader> loaders)
        {
            return loaders.OrderBy(l => l.Priority()).Reverse();
        }

        /// <summary>
        /// Priorities the specified loader.
        /// </summary>
        /// <param name="loader">The loader.</param>
        /// <returns></returns>
        public static  int Priority(this IDataLoader loader)
        { 
             var attr = loader.GetType().GetCustomAttribute<VersionedComponentExportAttribute>();
             return attr == null ? 0 : attr.Priority;
        }
    }

    /// <summary>
    /// Custom monahrq data loader MEF export attribute.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Core.VersionedComponentExportAttribute" />
    public abstract class DataLoaderExportAttribute : VersionedComponentExportAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataLoaderExportAttribute"/> class.
        /// </summary>
        /// <param name="contractName">Name of the contract.</param>
        /// <param name="major">The major.</param>
        /// <param name="minor">The minor.</param>
        public DataLoaderExportAttribute(string contractName, sbyte major, sbyte minor)
            : base(contractName, typeof(IDataLoader), major, minor)
        {
        }
    }

    /// <summary>
    /// The custom clinical dimension loader MEF export attribute.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Core.Import.DataLoaderExportAttribute" />
    [Obsolete("The ClinicalDimensionLoaderExportAttribute has been deprecated as of Monahrq version 6.0 Build 2.")]
    public class ClinicalDimensionLoaderExportAttribute : DataLoaderExportAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClinicalDimensionLoaderExportAttribute"/> class.
        /// </summary>
        /// <param name="major">The major.</param>
        /// <param name="minor">The minor.</param>
        public ClinicalDimensionLoaderExportAttribute(sbyte major, sbyte minor)
            : base(DataImportContracts.ClinicalDimensions, major, minor)
        {

        }
    }

    /// <summary>
    /// The base data loader MEf export attribute.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Core.Import.DataLoaderExportAttribute" />
    [Obsolete("The BaseDataLoaderExportAttribute has been deprecated as of Monahrq version 6.0 Build 2.")]
    public class BaseDataLoaderExportAttribute : DataLoaderExportAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDataLoaderExportAttribute"/> class.
        /// </summary>
        /// <param name="major">The major.</param>
        /// <param name="minor">The minor.</param>
        public BaseDataLoaderExportAttribute(sbyte major, sbyte minor)
            : base(DataImportContracts.BaseData, major, minor)
        {

        }
    }

    /// <summary>
    /// The measure topics data loader MEf export attribute.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Core.Import.DataLoaderExportAttribute" />
    [Obsolete("The TopicsDataLoaderExportAttribute has been deprecated as of Monahrq version 6.0 Build 2.")]
    public class TopicsDataLoaderExportAttribute : DataLoaderExportAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TopicsDataLoaderExportAttribute"/> class.
        /// </summary>
        /// <param name="major">The major.</param>
        /// <param name="minor">The minor.</param>
        public TopicsDataLoaderExportAttribute(sbyte major, sbyte minor)
            : base(DataImportContracts.Topics, major, minor)
        {

        }
    }

    /// <summary>
    /// The hospital data loader MEf export attribute.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Entities.Core.Import.DataLoaderExportAttribute" />
    [Obsolete("The HospitalsDataLoaderExportAttribute has been deprecated as of Monahrq version 6.0 Build 2.")]
    public class HospitalsDataLoaderExportAttribute : DataLoaderExportAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalsDataLoaderExportAttribute"/> class.
        /// </summary>
        /// <param name="major">The major.</param>
        /// <param name="minor">The minor.</param>
        public HospitalsDataLoaderExportAttribute(sbyte major, sbyte minor)
            : base(DataImportContracts.Hospitals, major, minor)
        {

        }
    }

}
