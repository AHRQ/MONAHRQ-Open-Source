using System;
using System.Collections.Generic;
using Monahrq.Infrastructure.Domain.Websites;

namespace Monahrq.Infrastructure.Services.Websites
{
    /// <summary>
    /// The Monahrq website domain service interface/contract.
    /// </summary>
    public interface IWebsiteService
    {
        /// <summary>
        /// Creates the website.
        /// </summary>
        /// <returns></returns>
        Website CreateWebsite();
        /// <summary>
        /// Saves the specified website.
        /// </summary>
        /// <param name="website">The website.</param>
        void Save(Website website);
        /// <summary>
        /// Searches the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        IEnumerable<Website> Search(System.Linq.Expressions.Expression<Func<Website, bool>> criteria);
        /// <summary>
        /// Deletes the specified to delete.
        /// </summary>
        /// <param name="toDelete">To delete.</param>
        void Delete(Website toDelete);
    }
}
