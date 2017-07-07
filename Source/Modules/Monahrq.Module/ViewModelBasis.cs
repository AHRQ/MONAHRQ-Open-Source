using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;

namespace Monahrq.Module
{

    [ImplementPropertyChanged]
    public class ViewModelBasis<TModel> 
        :  INotifyPropertyChanged
        , INotifyPropertyChanging
        where TModel: IEntity
    {
        public ViewModelBasis()
        {
            PropertyChanged += (o, e) =>
                {
                    if (e.PropertyName == "Model")
                    {
                        if (Model != null)
                        {
                            Model.PropertyChanged += ModelChanged;
                        }
                    }
                };

            PropertyChanging += (o, e) =>
                {
                    if (e.PropertyName == "Model" && Model != null)
                    {
                        Model.PropertyChanged -= ModelChanged;
                    }
                };
        }

        private void ModelChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(e.PropertyName));
        }

        public TModel Model { get; set; }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public event PropertyChangingEventHandler PropertyChanging = delegate { };
    }

    [ImplementPropertyChanged]
    public class ViewModelCollectionBasis<TModel> :
         IList<ViewModelBasis<TModel>>
        , IEnumerable<ViewModelBasis<TModel>>
        , ICollection<ViewModelBasis<TModel>>
        , INotifyCollectionChanged
        where TModel: IEntity
    {
        IList<ViewModelBasis<TModel>> ViewModels { get; set; }
        public IEnumerator<ViewModelBasis<TModel>> GetEnumerator()
        {
            return ViewModels.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(ViewModelBasis<TModel> item)
        {
            ViewModels.Add(item);
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void Clear()
        {
            ViewModels.Clear();
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(ViewModelBasis<TModel> item)
        {
            return ViewModels.Contains(item);
        }

        public void CopyTo(ViewModelBasis<TModel>[] array, int arrayIndex)
        {
            ViewModels.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return ViewModels.Count; }
        }

        public bool IsReadOnly
        {
            get { return ViewModels.IsReadOnly; }
        }

        public bool Remove(ViewModelBasis<TModel> item)
        {
            var index = ViewModels.IndexOf(item);
            if (index >= 0)
            {
                try
                {
                    return ViewModels.Remove(item);
                }
                finally
                {
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                }
            }
            return false;
        }

        public int IndexOf(ViewModelBasis<TModel> item)
        {
            return ViewModels.IndexOf(item);
        }

        public void Insert(int index, ViewModelBasis<TModel> item)
        {
            ViewModels.Insert(index, item);
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public void RemoveAt(int index)
        {
            var obj = ViewModels[index];
            ViewModels.RemoveAt(index);
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, obj, index));
        }

        public ViewModelBasis<TModel> this[int index]
        {
            get
            {
                return ViewModels[index];
            }
            set
            {
                ViewModels[index] = value;
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };
    }


}
