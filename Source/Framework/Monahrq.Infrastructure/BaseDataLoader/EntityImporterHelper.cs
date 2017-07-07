using System.Data;
using Monahrq.Infrastructure.Entities.Core.Import;

namespace Monahrq.Infrastructure.BaseDataLoader
{
    /// <summary>
    /// The Entity Importer Helper Extension Methods Class.
    /// </summary>
    static class EntityImporterHelper
    {
        /// <summary>
        /// Alreadies the imported.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        public static bool AlreadyImported(this IDataReader reader)
        {
            return reader == EmptyReader.Instance;
        }
    }
}
