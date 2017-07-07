using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace Monahrq.Infrastructure.Data.CustomTypes
{
	#region ObservableCollection
	///// <summary>
	///// Outlines the behavior that the user and persistent notifying list collections 
	///// will exhibit.
	///// </summary>
	///// <remarks>
	///// When using this collection type in an application with an NHibernate back-end, this 
	///// interface should be used as the declaring type for any class members in which the 
	///// desired type is <see cref="ObservableList<T>"/>.  NHibernate will provide an instance
	///// of the <see cref="PersistentGenericObservableBag[T]"/> persistent wrapper class if the
	///// collection has been persisted.
	///// </remarks>
	///// <typeparam name="T">Type of item to be stored in the list.</typeparam>
	//public interface IObservableList<T> : IList<T>, INotifyCollectionChanged
	//{}

	///// <summary>
	///// Represents a list collection that fires events when the collection's contents have 
	///// changed.
	///// </summary>
	///// <typeparam name="T">Type of item to be stored in the list.</typeparam>
	//public class ObservableList2<T> : List<T>, IObservableList<T>, IUserCollectionType
	//    // where T : class
	//{
	//    #region Contructors
	//    public ObservableList2()
	//        : base()
	//    {}

	//    public ObservableList2(int compasicty)
	//        : base(compasicty)
	//    { }

	//    public ObservableList2(IEnumerable<T> items)
	//        : base(items)
	//    { }
	//    #endregion

	//    public new void Add(T item)
	//    {
	//        base.Add(item);
	//        this.OnItemAdded(item);
	//    }

	//    public new void Clear()
	//    {
	//        base.Clear();
	//        this.OnCollectionReset();
	//    }

	//    public new void Insert(int index, T item)
	//    {
	//        base.Insert(index, item);
	//        this.OnItemInserted(index, item);
	//    }

	//    public new bool Remove(T item)
	//    {
	//        int index = this.IndexOf(item);

	//        bool result = base.Remove(item);
	//        this.OnItemRemoved(item, index);

	//        return result;
	//    }

	//    public new void RemoveAt(int index)
	//    {
	//        T item = this[index];

	//        base.RemoveAt(index);
	//        this.OnItemRemoved(item, index);
	//    }

	//    #region INotifyCollectionChanged Members

	//    public event NotifyCollectionChangedEventHandler CollectionChanged;

	//    /// <summary>
	//    /// Fires the <see cref="CollectionChanged"/> event to indicate an item has been 
	//    /// added to the end of the collection.
	//    /// </summary>
	//    /// <param name="item">Item added to the collection.</param>
	//    protected void OnItemAdded(T item)
	//    {
	//        if (this.CollectionChanged != null)
	//        {
	//            this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(
	//                NotifyCollectionChangedAction.Add, item, this.Count - 1));
	//        }
	//    }

	//    /// <summary>
	//    /// Fires the <see cref="CollectionChanged"/> event to indicate the collection 
	//    /// has been reset.  This is used when the collection has been cleared or 
	//    /// entirely replaced.
	//    /// </summary>
	//    protected void OnCollectionReset()
	//    {
	//        if (this.CollectionChanged != null)
	//        {
	//            this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(
	//                NotifyCollectionChangedAction.Reset));
	//        }
	//    }

	//    /// <summary>
	//    /// Fires the <see cref="CollectionChanged"/> event to indicate an item has 
	//    /// been inserted into the collection at the specified index.
	//    /// </summary>
	//    /// <param name="index">Index the item has been inserted at.</param>
	//    /// <param name="item">Item inserted into the collection.</param>
	//    protected void OnItemInserted(int index, T item)
	//    {
	//        if (this.CollectionChanged != null)
	//        {
	//            this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(
	//                NotifyCollectionChangedAction.Add, item, index));
	//        }
	//    }

	//    /// <summary>
	//    /// Fires the <see cref="CollectionChanged"/> event to indicate an item has
	//    /// been removed from the collection at the specified index.
	//    /// </summary>
	//    /// <param name="item">Item removed from the collection.</param>
	//    /// <param name="index">Index the item has been removed from.</param>
	//    protected void OnItemRemoved(T item, int index)
	//    {
	//        if (this.CollectionChanged != null)
	//        {
	//            this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(
	//                NotifyCollectionChangedAction.Remove, item, index));
	//        }
	//    }

	//    #endregion

	//    #region IUserCollectionType Members

	//    public bool Contains(object collection, object entity)
	//    {
	//        return ((IObservableList<T>)collection).Contains((T)entity);
	//    }

	//    public new IEnumerable GetElements(object collection)
	//    {
	//        return (IEnumerable)collection;
	//    }

	//    public object IndexOf(object collection, object entity)
	//    {
	//        return ((IObservableList<T>)collection).IndexOf((T)entity);
	//    }

	//    public object Instantiate()
	//    {
	//        return new ObservableList2<T>();
	//    }

	//    public object Instantiate(int anticipatedSize)
	//    {
	//        return anticipatedSize > 0 ? new ObservableList2<T>(anticipatedSize) : new ObservableList2<T>();
	//    }

	//    public IPersistentCollection Instantiate(ISessionImplementor session, ICollectionPersister persister)
	//    {
	//        return new PersistentGenericObservableBag<T>(session);
	//    }

	//    public object ReplaceElements(object original, object target, ICollectionPersister persister, object owner, System.Collections.IDictionary copyCache, ISessionImplementor session)
	//    {
	//        var result = (IObservableList<T>)target;

	//        result.Clear();
	//        foreach (object item in ((IEnumerable)original))
	//        {
	//            result.Add((T)item);
	//        }
	//        this.OnCollectionReset();

	//        return result;
	//    }


	//    public IPersistentCollection Wrap(ISessionImplementor session, object collection)
	//    {
	//        return new PersistentGenericObservableBag<T>(session, (ObservableList2<T>)collection);
	//    }

	//    #endregion
	//}

	///// <summary>
	///// Represents a persistent wrapper for a generic bag collection that fires events when the
	///// collection's contents have changed.
	///// </summary>
	///// <typeparam name="T">Type of item to be stored in the list.</typeparam>
	//public class PersistentGenericObservableBag<T> : PersistentGenericBag<T>, IObservableList<T>
	//    //where T : object
	//{
	//    public PersistentGenericObservableBag(ISessionImplementor session, ObservableList2<T> list)
	//        : base(session, list)
	//    {
	//    }

	//    public PersistentGenericObservableBag(ISessionImplementor session)
	//        : base(session)
	//    {
	//    }

	//    public void Add(T item)
	//    {
	//        base.Add(item);
	//        OnItemAdded(item);
	//    }

	//    public new void Clear()
	//    {
	//        base.Clear();
	//        OnCollectionReset();
	//    }

	//    public void Insert(int index, T item)
	//    {
	//        base.Insert(index, item);
	//        OnItemInserted(index, item);
	//    }

	//    public bool Remove(T item)
	//    {
	//        var index = IndexOf(item);

	//        base.Remove(item);
	//        OnItemRemoved(item, index);

	//        return true;
	//    }

	//    public new void RemoveAt(int index)
	//    {
	//        var item = this[index];// as T;

	//        base.RemoveAt(index);
	//        OnItemRemoved((T) item, index);
	//    }

	//    #region INotifyCollectionChanged Members

	//    public event NotifyCollectionChangedEventHandler CollectionChanged;

	//    /// <summary>
	//    /// Fires the <see cref="CollectionChanged"/> event to indicate an item has been 
	//    /// added to the end of the collection.
	//    /// </summary>
	//    /// <param name="item">Item added to the collection.</param>
	//    protected void OnItemAdded(T item)
	//    {
	//        if (CollectionChanged != null)
	//        {
	//            CollectionChanged(this, new NotifyCollectionChangedEventArgs(
	//                NotifyCollectionChangedAction.Add, item, this.Count - 1));
	//        }
	//    }

	//    /// <summary>
	//    /// Fires the <see cref="CollectionChanged"/> event to indicate the collection 
	//    /// has been reset.  This is used when the collection has been cleared or 
	//    /// entirely replaced.
	//    /// </summary>
	//    protected void OnCollectionReset()
	//    {
	//        if (CollectionChanged != null)
	//        {
	//            CollectionChanged(this, new NotifyCollectionChangedEventArgs(
	//                NotifyCollectionChangedAction.Reset));
	//        }
	//    }

	//    /// <summary>
	//    /// Fires the <see cref="CollectionChanged"/> event to indicate an item has 
	//    /// been inserted into the collection at the specified index.
	//    /// </summary>
	//    /// <param name="index">Index the item has been inserted at.</param>
	//    /// <param name="item">Item inserted into the collection.</param>
	//    protected void OnItemInserted(int index, T item)
	//    {
	//        if (CollectionChanged != null)
	//        {
	//            CollectionChanged(this, new NotifyCollectionChangedEventArgs(
	//                NotifyCollectionChangedAction.Add, item, index));
	//        }
	//    }

	//    /// <summary>
	//    /// Fires the <see cref="CollectionChanged"/> event to indicate an item has
	//    /// been removed from the collection at the specified index.
	//    /// </summary>
	//    /// <param name="item">Item removed from the collection.</param>
	//    /// <param name="index">Index the item has been removed from.</param>
	//    protected void OnItemRemoved(T item, int index)
	//    {
	//        if (CollectionChanged != null)
	//        {
	//            CollectionChanged(this, new NotifyCollectionChangedEventArgs(
	//                NotifyCollectionChangedAction.Remove, item, index));
	//        }
	//    }

	//    #endregion
	//}

	#endregion

}