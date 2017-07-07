using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Types;

namespace Monahrq.Infrastructure.Extensions
{
    /// <summary>
    /// A set of useful extensions for the List interface.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Some of these functions need random numbers - so keep a single
        /// Random instance.
        /// </summary>
        private static readonly Random _rng = new Random();

        /// <summary>
        /// Shuffles the specified list.
        /// </summary>
        /// <typeparam name="T">The list type.</typeparam>
        /// <param name="list">The list.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = _rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Returns a random element from the list.
        /// </summary>
        /// <typeparam name="T">The list type.</typeparam>
        /// <param name="list">The list.</param>
        /// <returns>A random element from the list.</returns>
        public static T RandomElement<T>(this IList<T> list)
        {
            return list.ElementAt(_rng.Next(0, list.Count));
        }

        /// <summary>
        /// Distincts the by.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
                                                                     Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element))).ToList();
        }

        /// <summary>
        /// Removes the null values.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static IList<TSource> RemoveNullValues<TSource>(this IList<TSource> source)
        {
            return source.All(item => !Equals(item, default(TSource))) 
                         ? source 
                         : source.Where(item => !Equals(item, default(TSource))).ToList();
        }

        /// <summary>
        /// Fors the each.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="action">The action.</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
		}
		public static void ForEach<T>(this IEnumerable<T> collection, Action<T,int> action)
		{
			int index = 0;
			foreach (var item in collection)
			{
				action(item, index++);
			}
		}


		public static int RemoveAll<T>(this IList<T> list, Predicate<T> match)
		{
			int count = 0;

			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (match(list[i]))
				{
					++count;
					list.RemoveAt(i);
				}
			}
			return count;
		}

		/// <summary>
		/// To the hash set.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection">The collection.</param>
		/// <returns></returns>
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection)
        {
            return collection == null
                       ? new HashSet<T>()
                       : new HashSet<T>(collection);
        }

        /// <summary>
        /// Exists the specified list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="match">The match.</param>
        /// <returns></returns>
        public static bool Exists<T>(this ObservableList<T> list, Predicate<T> match)
        {
            var bReturn = false;

            Parallel.ForEach(list, item =>
                {
                    if (!match(item)) return;

                    bReturn = true;
                });

            return bReturn;
		}
		public static bool ContainsAll<T>(this IEnumerable<T> listA, params T[] listB)
		{
			return !listB.Except(listA).Any();
		}
		public static bool ContainsAll<T>(this IEnumerable<T> listA, IEnumerable<T> listB)
		{
			return !listB.Except(listA).Any();
		}
		public static bool ContainsAny<T>(this IEnumerable<T> listA, params T[] listB)
		{
			return listA.Any(a => listB.Contains(a));
		}
		public static bool ContainsAny<T>(this IEnumerable<T> listA, IEnumerable<T> listB)
		{
			return listA.Any(a => listB.Contains(a));
		}
		public static bool AnyIn<T,U>(this IEnumerable<T> listA, IEnumerable<U> listB, Func<T,U,bool> match)
		{
			return listA.Any(a => listB.Any(b => match(a, b)));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="item"></param>
		/// <param name="matchPred"></param>
		/// <returns></returns>
		public static bool AddIfUnique<T>(this ICollection<T> list,T item, Func<T,T,bool> matchPred=null)
		{
			Func<T, T, bool> tempPred = (a, b) => { return EqualityComparer<T>.Default.Equals(a, b); };
			matchPred = matchPred ?? tempPred;
			var isFound = list.Any(i => matchPred(i, item));
			if (!isFound) list.Add(item);
			return !isFound;
		}

		public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
		{
			if (list == null) throw new ArgumentNullException("list");
			if (items == null) throw new ArgumentNullException("items");

			if (list is List<T>)
			{
				((List<T>)list).AddRange(items);
			}
			else
			{
				foreach (var item in items)
				{
					list.Add(item);
				}
			}
		}

		/// <summary>
		/// To the observable list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list.</param>
		/// <returns></returns>
		public static ObservableList<T> ToObservableList<T>(this IEnumerable<T> list)
        {
            return new ObservableList<T>(list ?? Enumerable.Empty<T>());
        }

		/// <summary>
		/// Gets the unanimous value of a Property/selector in a group of items.  If even one item has a 
		/// different value for the Property the 'elseValue' is returned.
		/// If all values equal the default(VT) value, allDefaultValue is returned.
		/// </summary>
		/// <typeparam name="TT"></typeparam>
		/// <typeparam name="VT"></typeparam>
		/// <param name="list"></param>
		/// <param name="selector"></param>
		/// <param name="elseValue"></param>
		/// <returns></returns>
		public static VT GetCollectiveValue<TT,VT>(this IList<TT> list, Func<TT, int, VT> selector,VT elseValue,VT allDefaultValue=default(VT))
		{
			VT firstValue = selector(list[0],0);
			VT defaultValue = default(VT);
			bool isAllDefault = true;

			int index = 0;
			foreach (var element in list)
			{
				var curValue = selector(element,index);
				if (!EqualityComparer<VT>.Default.Equals(curValue, firstValue))
					return elseValue;

				if (isAllDefault && !EqualityComparer<VT>.Default.Equals(curValue, defaultValue))
					isAllDefault = false;
				index++;
			}
			return isAllDefault ? allDefaultValue : firstValue;
		}
		/// <summary>
		/// Gets the unanimous value of a Property/selector in a group of items.  If even one item has a  
		/// different value for the Property the 'elseValue' is returned.
		/// If all values equal the default(VT) value, allDefaultValue is returned.
		/// </summary>
		/// <typeparam name="TT"></typeparam>
		/// <typeparam name="VT"></typeparam>
		/// <param name="list"></param>
		/// <param name="selector"></param>
		/// <param name="elseValue"></param>
		/// <returns></returns>
		public static VT GetCollectiveValue<TT, VT>(this IList<TT> list, Func<TT, VT> selector, VT elseValue, VT allDefaultValue = default(VT))
		{
			if (list == null || list.Count == 0)
				return allDefaultValue;

			VT firstValue = selector(list[0]);
			VT defaultValue = default(VT);
			bool isAllDefault = true;

			foreach (var element in list)
			{
				var curValue = selector(element);
				if (!EqualityComparer<VT>.Default.Equals(curValue, firstValue))
					return elseValue;

				if (isAllDefault && !EqualityComparer<VT>.Default.Equals(curValue, defaultValue))
					isAllDefault = false;
			}
			return isAllDefault ? allDefaultValue : firstValue;
		}

        public static string DisplayAsCommaDelimitedList(this IEnumerable<string> list)
        {
            return list == null ? null : string.Join(", ", list);
        }


		public static void DeepAssignmentFrom<T>(this IList<T> dest, IList<T> src, Func<T,T,bool> matchFunc, bool addMissing=true)
			where T : IDeepAssignable<T>, IDeepCloneable<T>
		{
			src.ForEach(oc =>
			{
				var comp = dest.FirstOrDefault(c => matchFunc(c, oc));
				if (comp != null) comp.DeepAssignmentFrom(oc);
				else if (addMissing) dest.Add(oc.DeepClone());
			});
		}
		public static IList<T> DeepClone<T>(this IList<T> src)
			where T : IDeepCloneable<T>
		{
			var clone = new List<T>();
			src.ForEach(item => { clone.Add(item.DeepClone()); });
			return clone;
		}

        public static List<ExpandoObject> ToExpandoObjectList<T>(this IEnumerable<T> items)
        {
            return items.Select(i => i.ToExpandoObject()).ToList();
        }

        public static List<dynamic> ToDynamicList<T>(this IEnumerable<T> items)
        {
            return items.Select(i => i.ToDynamic()).ToList();
        }

        public static IList<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ReadOnlyCollection<T>(enumerable.ToList());
        }


		public static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
			this IEnumerable<TOuter> outer,
			IEnumerable<TInner> inner,
			Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector,
			Func<TOuter, TInner, TResult> resultSelector)
		{
			return LeftJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TKey>.Default );
		}
		public static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
			this IEnumerable<TOuter> outer,
			IEnumerable<TInner> inner,
			Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector,
			Func<TOuter, TInner, TResult> resultSelector,
			IEqualityComparer<TKey> comparer)
		{
			return
				outer.SelectMany(oItem =>
				{
					var oKey = outerKeySelector(oItem);
					var iItems = inner.Where(i => comparer.Equals(oKey, innerKeySelector(i)));
					return iItems.Count() == 0
						? new List<TResult> { resultSelector(oItem, default(TInner)) }
						: iItems.Select<TInner,TResult>(iItem => resultSelector(oItem, iItem));
				});
		}
	}
}
