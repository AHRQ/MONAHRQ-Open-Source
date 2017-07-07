using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace Monahrq.Infrastructure.Data.CustomTypes
{

	//public class ObservableListType<T> : CollectionType, IUserCollectionType
	//{
	//    public ObservableListType(string role, string foreignKeyPropertyName, bool isEmbeddedInXml)
	//        : base(role, foreignKeyPropertyName, isEmbeddedInXml)
	//    {}

	//    public ObservableListType()
	//        : base(string.Empty, string.Empty, false)
	//    {}

	//    public IPersistentCollection Instantiate(ISessionImplementor session, ICollectionPersister persister)
	//    {
	//        return new PersistentObservableGenericList<T>(session);
	//    }

	//    public override IPersistentCollection Instantiate(ISessionImplementor session, ICollectionPersister persister, object key)
	//    {
	//        return new PersistentObservableGenericList<T>(session);
	//    }

	//    public override IPersistentCollection Wrap(ISessionImplementor session, object collection)
	//    {
	//        return new PersistentObservableGenericList<T>(session, (IList<T>)collection);
	//    }

	//    public IEnumerable GetElements(object collection)
	//    {
	//        return ((IEnumerable)collection);
	//    }

	//    public bool Contains(object collection, object entity)
	//    {
	//        return ((ICollection<T>)collection).Contains((T)entity);
	//    }


	//    protected override void Clear(object collection)
	//    {
	//        ((IList)collection).Clear();
	//    }

	//    public object ReplaceElements(object original, object target, ICollectionPersister persister, object owner, IDictionary copyCache, ISessionImplementor session)
	//    {
	//        var result = (ICollection<T>)target;
	//        result.Clear();
	//        foreach (var item in ((IEnumerable)original))
	//        {
	//            if (copyCache.Contains(item))
	//                result.Add((T)copyCache[item]);
	//            else
	//                result.Add((T)item);
	//        }
	//        return result;
	//    }

	//    public override object Instantiate(int anticipatedSize)
	//    {
	//        return new ObservableCollection<T>();
	//    }

	//    public override Type ReturnedClass
	//    {
	//        get
	//        {
	//            return typeof(ObservableCollection<T>);
	//        }
	//    }
	//}

	//public class PersistentObservableGenericList<T> : PersistentGenericList<T>, INotifyCollectionChanged,
	//                                                  INotifyPropertyChanged
	//{
	//    private NotifyCollectionChangedEventHandler _collectionChanged;
	//    private PropertyChangedEventHandler _propertyChanged;

	//    public PersistentObservableGenericList(ISessionImplementor sessionImplementor)
	//        : base(sessionImplementor)
	//    {
	//    }

	//    public PersistentObservableGenericList(ISessionImplementor sessionImplementor, IList<T> list)
	//        : base(sessionImplementor, list)
	//    {
	//        CaptureEventHandlers(list);
	//    }

	//    public PersistentObservableGenericList()
	//    {
	//    }

	//    #region INotifyCollectionChanged Members

	//    public event NotifyCollectionChangedEventHandler CollectionChanged
	//    {
	//        add
	//        {
	//            Initialize(false);
	//            _collectionChanged += value;
	//        }
	//        remove { _collectionChanged -= value; }
	//    }

	//    #endregion

	//    #region INotifyPropertyChanged Members

	//    public event PropertyChangedEventHandler PropertyChanged
	//    {
	//        add
	//        {
	//            Initialize(false);
	//            _propertyChanged += value;
	//        }
	//        remove { _propertyChanged += value; }
	//    }

	//    #endregion

	//    public override void BeforeInitialize(ICollectionPersister persister, int anticipatedSize)
	//    {
	//        base.BeforeInitialize(persister, anticipatedSize);
	//        CaptureEventHandlers((ICollection<T>)list);
	//    }

	//    private void CaptureEventHandlers(ICollection<T> coll)
	//    {
	//        var notificableCollection = coll as INotifyCollectionChanged;
	//        var propertyNotificableColl = coll as INotifyPropertyChanged;

	//        if (notificableCollection != null)
	//            notificableCollection.CollectionChanged += OnCollectionChanged;

	//        if (propertyNotificableColl != null)
	//            propertyNotificableColl.PropertyChanged += OnPropertyChanged;
	//    }

	//    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
	//    {
	//        PropertyChangedEventHandler changed = _propertyChanged;
	//        if (changed != null) changed(this, e);
	//    }

	//    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	//    {
	//        NotifyCollectionChangedEventHandler changed = _collectionChanged;
	//        if (changed != null) changed(this, e);
	//    }
	//}

}