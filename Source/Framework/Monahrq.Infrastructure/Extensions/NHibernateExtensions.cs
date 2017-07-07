using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Monahrq.Infrastructure.Types;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using Expression = System.Linq.Expressions.Expression;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;
using NHibernate.Engine;

namespace Monahrq.Infrastructure.Extensions
{
    public static class NHibernateExtensions
    {
        /// <summary>
        /// Clears all nhibernate caches.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public static void ClearAllNhibernateCaches(this ISessionFactory factory)
        {
            factory.EvictQueries();
            foreach (var collectionMetadata in factory.GetAllCollectionMetadata())
                factory.EvictCollection(collectionMetadata.Key);
            foreach (var classMetadata in factory.GetAllClassMetadata())
                factory.EvictEntity(classMetadata.Key);
        }

        /// <summary>
        /// Clears the nhibernate query caches.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="queryKeyName">Name of the query key.</param>
        public static void ClearNhibernateQueryCaches(this ISessionFactory factory, string queryKeyName = null)
        {
            if (!string.IsNullOrEmpty(queryKeyName)) 
                factory.EvictQueries(queryKeyName);
            else 
                factory.EvictQueries();
        }
        
		/// <summary>
		/// 
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public static IList<dynamic> DynamicList(this IQuery query)
		{
			return query.SetResultTransformer(ExpandoObjectTransformer.ExpandoObject)
						.List<dynamic>();
		}
	}
}
