using System;
using System.Linq.Expressions;

namespace Monahrq.Infrastructure.Entities.Core.Import
{
    /// <summary>
    /// The custom data set import auditor.
    /// </summary>
    public interface IImportAuditor
    {
        /// <summary>
        /// Wases the executed.
        /// </summary>
        /// <param name="dataReaders">The data readers.</param>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        bool WasExecuted(IDataReaderDictionary dataReaders, Expression<Func<object>> expr);
    }
}
