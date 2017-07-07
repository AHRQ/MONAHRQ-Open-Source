using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using Monahrq.Default.ViewModels;
using PropertyChanged;
using Microsoft.Practices.Prism.Regions;

namespace Monahrq.DataSets.HospitalRegionMapping
{
    /// <summary>
    /// The hospital region mapping view model interface.
    /// </summary>
    public interface IHospitalRegionMappingViewModel { }

    /// <summary>
    /// The hospital region mapping view model.
    /// </summary>
    /// <seealso cref="Monahrq.Default.ViewModels.BaseViewModel" />
    /// <seealso cref="Microsoft.Practices.Prism.Regions.INavigationAware" />
    /// <seealso cref="Monahrq.DataSets.HospitalRegionMapping.IHospitalRegionMappingViewModel" />
    [ImplementPropertyChanged]
    public abstract class HospitalRegionMappingViewModel : BaseViewModel, INavigationAware, IHospitalRegionMappingViewModel
    {
        /// <summary>
        /// Called when the implementer has been navigated to.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public abstract void OnNavigatedTo(NavigationContext navigationContext);

        /// <summary>
        /// Called to determine if this instance can handle the navigation request.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        /// <returns>
        /// <see langword="true" /> if this instance accepts the navigation request; otherwise, <see langword="false" />.
        /// </returns>
        public abstract bool IsNavigationTarget(NavigationContext navigationContext);

        /// <summary>
        /// Called when the implementer is being navigated away from.
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public abstract void OnNavigatedFrom(NavigationContext navigationContext);
    }


    /// <summary>
    /// The hospital regionMapping collecton view model.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.Generic.ICollection{T}" />
    /// <seealso cref="Monahrq.DataSets.HospitalRegionMapping.IHospitalRegionMappingViewModel" />
    public interface IHospitalRegionMappingCollectonViewModel<T> : ICollection<T>, IHospitalRegionMappingViewModel { }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Monahrq.DataSets.HospitalRegionMapping.HospitalRegionMappingViewModel" />
    /// <seealso cref="Monahrq.DataSets.HospitalRegionMapping.IHospitalRegionMappingCollectonViewModel{T}" />
    [ImplementPropertyChanged]
    public abstract class HospitalRegionMappingCollectionViewModel<T> : HospitalRegionMappingViewModel, IHospitalRegionMappingCollectonViewModel<T>
        where T: class
    {
        /// <summary>
        /// Called when [navigated from].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {}

        /// <summary>
        /// Called when [navigated to].
        /// </summary>
        /// <param name="navigationContext">The navigation context.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalRegionMappingCollectionViewModel{T}"/> class.
        /// </summary>
        protected HospitalRegionMappingCollectionViewModel()
        {
            Items = new ObservableCollection<T>();
            ItemsView = new ListCollectionView(Items);
        }

        /// <summary>
        /// Gets or sets the items view.
        /// </summary>
        /// <value>
        /// The items view.
        /// </value>
        public ListCollectionView ItemsView
        {
            get { return _itemsView; }
            set { _itemsView = value; }
        }

        private ObservableCollection<T> _items;
        private ListCollectionView _itemsView;

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public ObservableCollection<T> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count
        {
            get
            {
                return Items.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return (Items as ICollection<T>).IsReadOnly;
            }
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(T item)
        {
            Items.Add(item);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            Items.Clear();
        }

        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(T item)
        {
            return Items.Contains(item);
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            if (item == null)
                return true;

            return Items.Remove(item);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        /// <summary>
        /// Resets the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="properties">The properties.</param>
        protected virtual void Reset(ObservableCollection<T> source, params string[] properties)
        {
            properties.ToList().ForEach(ClearErrors);
            Committed = true;
            Items = source;
            ItemsView = new ListCollectionView(Items);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets or sets the current item.
        /// </summary>
        /// <value>
        /// The current item.
        /// </value>
        public T CurrentItem
        {
            get
            {
                return ItemsView.CurrentItem as T;
            }
            set
            {
                if (value == ItemsView.CurrentItem) return;
                ItemsView.MoveCurrentTo(value);
            }
        }
    }
}
