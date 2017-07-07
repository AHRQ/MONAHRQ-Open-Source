using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.Domain.BaseData.ViewModel
{
    public class EntityViewModel<TEntity, TKey> : IComparable
        where TEntity : Entity<TKey>
        where TKey : struct
    {

        public TEntity Data { get; private set; }

        public EntityViewModel(TEntity entity)
            : this(new DefaultTextPresentationStrategy(), entity)
        {

        }

        public EntityViewModel(ITextPresentationStrategy strategy, TEntity entity)
            : this(new DefaultLookupComparer<TEntity, TKey>(), strategy, entity)
        {
        }

        public EntityViewModel(IComparer<EntityViewModel<TEntity, TKey>> comparer
                , ITextPresentationStrategy strategy
                    , TEntity entity) 
        {
            // TODO: Complete member initialization
            this.CompareStrategy = comparer;
            this.TextStrategy = strategy;
            this.Data = entity;
        }

        public EntityViewModel(IComparer<EntityViewModel<TEntity, TKey>> comparer, TEntity entity)
            : this(comparer, new DefaultTextPresentationStrategy(), entity)
        {
        }

        ITextPresentationStrategy InternalTextStrategy
        {
            get;
            set;
        }

        ITextPresentationStrategy TextStrategy
        {
            get
            {
                return InternalTextStrategy ?? new DefaultTextPresentationStrategy();
            }
            set
            {
                InternalTextStrategy = value ?? new DefaultTextPresentationStrategy();
            }
        }

        private IComparer<EntityViewModel<TEntity, TKey>> CompareStrategy { get; set; }


        public Nullable<TKey> Id
        {
            get
            {
                return Data == null ? null : new Nullable<TKey>(Data.Id);
            }
        }

        public string Text
        {
            get
            {
                return TextStrategy.Present(Data);
            }
        }

        public int CompareTo(object obj)
        {
            return this.CompareStrategy.Compare(this, obj as EntityViewModel<TEntity, TKey>);
        }

        public override string ToString()
        {
            return Text;
        }
    }

    //public class NamedEntityViewModel<TEntity, TKey> : IComparable
    //    where TEntity: NamedEntity<TKey>
    //    where TKey: struct
    //{

    //    public TEntity Data { get; private set; }

    //    public NamedEntityViewModel(TEntity entity)
    //        : this(new DefaultTextPresentationStrategy(), entity)
    //    {
            
    //    }

    //    public NamedEntityViewModel(ITextPresentationStrategy strategy, TEntity entity)
    //        : this(new DefaultLookupComparer<TEntity, TKey>(), strategy, entity)
    //    {
    //    }

    //    public NamedEntityViewModel(IComparer<NamedEntityViewModel<TEntity, TKey>> comparer
    //            , ITextPresentationStrategy strategy
    //                , TEntity entity)
    //    {
    //        // TODO: Complete member initialization
    //        this.CompareStrategy = comparer;
    //        this.TextStrategy = strategy;
    //        this.Data = entity;
    //    }

    //    public NamedEntityViewModel(IComparer<NamedEntityViewModel<TEntity, TKey>> comparer, TEntity entity)
    //        : this(comparer, new DefaultTextPresentationStrategy(), entity)
    //    {
    //    }

    //    ITextPresentationStrategy InternalTextStrategy
    //    {
    //        get;
    //        set;
    //    }

    //    ITextPresentationStrategy TextStrategy 
    //    {
    //        get 
    //        { 
    //            return InternalTextStrategy ?? new DefaultTextPresentationStrategy(); 
    //        }
    //        set
    //        {
    //            InternalTextStrategy = value ??  new DefaultTextPresentationStrategy();  
    //        }
    //    }

    //    private IComparer<NamedEntityViewModel<TEntity, TKey>> CompareStrategy { get; set; }


    //    public Nullable<TKey> Id 
    //    {
    //        get
    //        {
    //            return Data == null ? null : new Nullable<TKey>(Data.Id);
    //        }
    //    }

    //    public string Text
    //    {
    //        get
    //        {
    //            return TextStrategy.Present(Data);
    //        }
    //    }

    //    public int CompareTo(object obj)
    //    {
    //        return this.CompareStrategy.Compare(this, obj as NamedEntityViewModel<TEntity, TKey>);
    //    }

    //    public override string ToString()
    //    {
    //        return Text;
    //    }
    //}


    public class DefaultLookupComparer<TEntity, TKey> 
        : Comparer<EntityViewModel<TEntity, TKey>>
        where TEntity : Entity<TKey>
        where TKey : struct
    {

        public override int Compare(EntityViewModel<TEntity, TKey> x, EntityViewModel<TEntity, TKey> y)
        {
            return x == null ? -1
                : y == null ? 1
                : x.Data == null ? -1
                : y.Data == null ? 1
                : (x.Text ?? string.Empty).CompareTo(y.Text ?? string.Empty);
        }
    }

    public class ComparableKeyCompareStrategy<TEntity, TKey>
      : Comparer<EntityViewModel<TEntity, TKey>>
        where TEntity : Entity<TKey>
        where TKey : struct
    {

        public override int Compare(EntityViewModel<TEntity, TKey> x, EntityViewModel<TEntity, TKey> y)
        {
            return x == null ? -1
                : y == null ? 1
                : x.Data == null ? -1
                : y.Data == null ? 1
                : (x.Id as IComparable).CompareTo(y.Id);
        }
    }

}
