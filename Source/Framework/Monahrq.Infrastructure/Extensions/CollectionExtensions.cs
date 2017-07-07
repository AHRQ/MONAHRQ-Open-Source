using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Sdk.Extensions
{
    public static class CollectionHelper
    {
        public static ListCollectionView EmptyListCollectionView<T>()
        {
            return CollectionViewSource.GetDefaultView(new ObservableCollection<T>()) as ListCollectionView;
        }

        public static bool In(this object obj, IEnumerable enumerable)
        {
            return enumerable.Cast<object>().Contains(obj);
        }

        public static bool In(this string obj, IEnumerable<string> enumerable)
        {
            if(string.IsNullOrEmpty(obj) || enumerable == null )
                return false;

            var result = false;
            foreach (var item in enumerable)
            {
                if (item.EqualsIgnoreCase(obj))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Removes the duplicates.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static List<T> RemoveDuplicates<T>(this IEnumerable<T> input)
        {
            if (input == null)
                return new List<T>();

            var uniqueStore = new Dictionary<T, int>();
            var finalList = new List<T>();

            foreach (T currValue in input.Where(currValue => !uniqueStore.ContainsKey(currValue)))
            {
                uniqueStore.Add(currValue, 0);
                finalList.Add(currValue);
            }
            return finalList;
        }


        public static void Replace<T>(this ListCollectionView collectionView, T itemToRemove, T itemToAdd)
            where T : class, IEntity<int>
        {
            var itemIndex = collectionView.IndexOf(itemToRemove);

            if (itemIndex == -1) return;

            var items = collectionView.OfType<T>().ToList();
            if (items.Any(o => o.Id == itemToRemove.Id))
            {
                items.RemoveAt(itemIndex);
                items.Insert(itemIndex, itemToAdd);
                collectionView = new ListCollectionView(items);
            }
            else
            {
                collectionView.AddNewItem(itemToAdd);
                collectionView.CommitEdit();
            }
        }
    }
}
