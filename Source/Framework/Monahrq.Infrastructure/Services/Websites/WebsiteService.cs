using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using NHibernate.Linq;

namespace Monahrq.Infrastructure.Services.Websites
{
    /// <summary>
    /// The Monahrq website domain service
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Services.Websites.IWebsiteService" />
    [Export(typeof(IWebsiteService))]
    public partial class WebsiteService : IWebsiteService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteService"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        [ImportingConstructor]
        public WebsiteService(IDomainSessionFactoryProvider provider)
        {
            Provider = provider;
        }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        IDomainSessionFactoryProvider Provider
        {
            get;
            set;
        }

        /// <summary>
        /// Creates the website.
        /// </summary>
        /// <returns></returns>
        public Domain.Websites.Website CreateWebsite()
        {
            return new Website();
        }

        /// <summary>
        /// Saves the specified website.
        /// </summary>
        /// <param name="website">The website.</param>
        public void Save(Website website)
        {
            if (website.CurrentStatus == WebsiteState.Generating) return;
            var current = website.CurrentStatus;
            website.CurrentStatus =
                website.CurrentStatus == null
                ? WebsiteState.Initialized
                : (WebsiteState)((int)website.CurrentStatus + 1);
            using (var session = Provider.SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    try
                    {
                        session.Save(website);
                        tx.Commit();
                        website.IsChanged = false;
                    }
                    catch
                    {
                        website.CurrentStatus = current;
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Searches the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public IEnumerable<Website> Search(System.Linq.Expressions.Expression<Func<Website, bool>> criteria)
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                return session.Query<Website>()
                            .Where(criteria).ToList();
            }
        }


        /// <summary>
        /// Deletes the specified to delete.
        /// </summary>
        /// <param name="toDelete">To delete.</param>
        public void Delete(Website toDelete)
        {
            using (var session = Provider.SessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    try
                    {
                        session.Delete(toDelete);
                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
