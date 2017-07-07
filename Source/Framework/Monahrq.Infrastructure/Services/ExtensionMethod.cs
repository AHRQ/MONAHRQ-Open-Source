using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Types;

namespace Monahrq.Infrastructure.Services
{
    /// <summary>
    /// The collection utility extension methods class
    /// </summary>
    public static class CollectionUtils
    {
        /// <summary>
        /// To the observable collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> items)
        {
			return (items == null) ?	new ObservableCollection<T>() :
										new ObservableCollection<T>(items);
			//return new ObservableCollection<T>(items.ToList());
        }
        /// <summary>
        /// Ases the observable collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static ObservableCollection<T> AsObservableCollection<T>(this IEnumerable<T> items)
		{
			return new ObservableCollection<T>(items.AsEnumerable());
		}

        /// <summary>
        /// To the list collection view.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static ListCollectionView ToListCollectionView<T>(this IEnumerable<T> items)
        {
            return CollectionViewSource.GetDefaultView(items.ToObservableCollection()) as ListCollectionView;
        }

        /// <summary>
        /// To the multi select list collection view.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static MultiSelectCollectionView<T> ToMultiSelectListCollectionView<T>(this IEnumerable<T> items)
            where T : class, ISelectable
        {
            return new MultiSelectCollectionView<T>(items.ToObservableCollection());
        }
    }

    /// <summary>
    /// A custom Cache that works asynchronously.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class AsyncCache<TKey, TValue>
    {
        /// <summary>
        /// The value factory
        /// </summary>
        private readonly Func<TKey, Task<TValue>> _valueFactory;
        /// <summary>
        /// The map
        /// </summary>
        private readonly ConcurrentDictionary<TKey, Lazy<Task<TValue>>> _map;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCache{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="valueFactory">The value factory.</param>
        /// <exception cref="ArgumentNullException">valueFactory</exception>
        public AsyncCache(Func<TKey, Task<TValue>> valueFactory)
        {
            if (valueFactory == null) throw new ArgumentNullException("valueFactory");
            _valueFactory = valueFactory;
            _map = new ConcurrentDictionary<TKey, Lazy<Task<TValue>>>();
        }

        /// <summary>
        /// Gets the <see cref="Task{TValue}"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="Task{TValue}"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">key</exception>
        public Task<TValue> this[TKey key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException("key");
                return _map.GetOrAdd(key, toAdd =>
                    new Lazy<Task<TValue>>(() => _valueFactory(toAdd))).Value;
            }
        }
    }
}
