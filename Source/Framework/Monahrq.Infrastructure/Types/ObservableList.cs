using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace Monahrq.Infrastructure.Types
{
    public class ObservableList<T> : IList<T>, INotifyCollectionChanged
    {
        #region INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void NotifyCollectionChanged(NotifyCollectionChangedAction pAction, object pItem)
        {
            if (CollectionChanged != null)
            {
                if (Thread.CurrentThread == _dispatcher.Thread)
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(pAction, pItem));
                else
                    _dispatcher.Invoke(() => CollectionChanged(this, new NotifyCollectionChangedEventArgs(pAction, pItem)));
            }
        }

        private void NotifyCollectionChanged(NotifyCollectionChangedAction pAction, object pItem, int piIndex)
        {
            if (CollectionChanged != null)
                if (_dispatcher == null || Thread.CurrentThread == _dispatcher.Thread)
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(pAction, pItem, piIndex));
                else
                    _dispatcher.Invoke(() => CollectionChanged(this, new NotifyCollectionChangedEventArgs(pAction, pItem, piIndex)));

        }

        private void NotifyCollectionChanged(NotifyCollectionChangedAction pAction, IList<T> pList)
        {
            if (CollectionChanged != null)
                if (_dispatcher == null || Thread.CurrentThread == _dispatcher.Thread)
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(pAction, pList));
                else
                    _dispatcher.Invoke(() => CollectionChanged(this, new NotifyCollectionChangedEventArgs(pAction, pList)));

        }

        private void NotifyCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                if (_dispatcher == null || Thread.CurrentThread == _dispatcher.Thread)
                    CollectionChanged(this, args);
                else
                    _dispatcher.Invoke(() => CollectionChanged(this, args));

        }

        private void NotifyCollectionChanged(NotifyCollectionChangedAction pAction)
        {
            if (CollectionChanged != null)
                if (_dispatcher == null || Thread.CurrentThread == _dispatcher.Thread)
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(pAction));
                else
                    _dispatcher.Invoke(() => CollectionChanged(this, new NotifyCollectionChangedEventArgs(pAction)));
        }

        #endregion

        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return _list.TakeWhile(item => item != null).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Class Declarations

        private readonly List<T> _list = new List<T>();
        private readonly Dispatcher _dispatcher = null;

        #endregion

        #region Constructor

        public ObservableList()
        {
            //_dispatcher = Thread.CurrentThread.;
        }

        public ObservableList(IEnumerable<T> pList)
        {
            _list = pList.ToList();
        }

        public ObservableList(Dispatcher pDispatcher)
        {
            _dispatcher = pDispatcher;
        }

        public ObservableList(Dispatcher pDispatcher, IEnumerable<T> pList)
        {
            _dispatcher = pDispatcher;
            _list = pList.ToList();
        }

        #endregion

        #region IList Members

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        public void RemoveAt(int index)
        {
            if (index > -1)
            {
                _list.RemoveAt(index);
                NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, _list[index], index);
            }
        }

        public T this[int index]
        {
            get { return index > -1 && _list.Count > 0 ? _list.ElementAt(index) : default(T); }
            set
            {
                if (index > -1)
                {
                    if (index > _list.Count)
                    {
                        _list.Add(value);
                        NotifyCollectionChanged(NotifyCollectionChangedAction.Add, value);
                    }
                    else
                    {
                        _list[index] = value;
                        NotifyCollectionChanged(NotifyCollectionChangedAction.Replace, value, index);
                    }
                }
            }
        }

        public void Add(T item)
        {
            if (item != null)
            {
                _list.Add(item);
                NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item);
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items != null)
            {
                _list.AddRange(items);
                NotifyCollectionChanged(NotifyCollectionChangedAction.Add, items);
            }
        }

        public void Clear()
        {
            _list.Clear();
            NotifyCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            var bReturn = false;

            if (item != null)
            {
                var index = _list.IndexOf(item);

                if (index > -1)
                {
                    bReturn = _list.Remove(item);

                    if (bReturn)
                        NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
                }
            }

            return bReturn;
        }

        #endregion

        #region Conversions

        public static implicit operator ObservableList<T>(List<T> list)
        {
            var lReturn = new ObservableList<T>();

            foreach (var item in list)
            {
                lReturn.Add(item);
            }

            return lReturn;
        }

        #endregion
    }
}
