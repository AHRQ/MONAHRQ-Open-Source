using System;
using System.ComponentModel.Composition;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.Wings
{
    public interface IWingItem : IEntity
    {
        string Description { get; set; }
    }

    [Serializable]
    public class WingItem<T> : Entity<T>, IWingItem
    {
        protected WingItem() { }
        public WingItem(string name) { Name = name; }
        public virtual string Description { get; set; }
    }

    [Serializable]
    public class OwnedWingItem<TOwner, TOwnerKey, T> : OwnedEntity<TOwner, TOwnerKey, T>, IWingItem
       where TOwner : Entity<TOwnerKey>, IWingItem
    {
        protected OwnedWingItem() { }
        public OwnedWingItem(TOwner owner, string name) : base(owner) { Name = name; }
        public virtual string Description { get; set; }
    }

    [Serializable]
    public class TargetOwnedWingItem<T> : OwnedWingItem<Target, int, T>
    {
        protected TargetOwnedWingItem()  { }
        public TargetOwnedWingItem(Target owner, string name) : base(owner, name) { }
    }

    [Serializable]
    public class WingOwnedWingItem<T> : OwnedWingItem<Wing, int, T>
    {
        protected WingOwnedWingItem()  { }
        public WingOwnedWingItem(Wing owner, string name) : base(owner, name) { }
    }

    [Serializable]
    public class ElementOwnedWingItem<T> : OwnedWingItem<Element, int, T>
    {
        protected ElementOwnedWingItem() { }
        public ElementOwnedWingItem(Element owner, string name) : base(owner, name) { }
    }

    [Serializable]
    public class ScopedOwnedWingItem<T> : OwnedWingItem<Scope, int, T>
    {
        protected ScopedOwnedWingItem() { }
        public ScopedOwnedWingItem(Scope owner, string name) : base(owner, name) { }
    }

    [ImplementPropertyChanged]
    public partial class Wing : WingItem<int>
    {
    }

    public interface IWingRepository : IRepository<Wing, int>
    {
    }

    namespace Repository
    {
        [Export(typeof(IWingRepository))]
        public partial class WingRepository : RepositoryBase<Wing, int>,
                 IWingRepository
        {
            [Import(typeof(ISessionFactoryProvider))]
            protected override IDomainSessionFactoryProvider DomainSessionFactoryProvider { get; set; }
        }

    }

    [ImplementPropertyChanged]
    public partial class Scope : TargetOwnedWingItem<int>
    {
    }

    public interface IScopeRepository : IRepository<Scope, int>
    {
    }

    namespace Repository
    {
        [Export(typeof(IScopeRepository))]
        public partial class ScopeRepository : RepositoryBase<Scope, int>,
                 IScopeRepository
        {
            [Import(typeof(ISessionFactoryProvider))]
            protected override IDomainSessionFactoryProvider DomainSessionFactoryProvider { get; set; }
        }

    }

    namespace Maps
    {
        using Domain.Maps;


        public abstract class WingItemMap<TType, TKey, TKeyStrategy> : EntityMap<TType, TKey, TKeyStrategy>
            where TKeyStrategy : IKeyStrategy, new()
            where TType : Entity<TKey>, IWingItem
        {
            protected WingItemMap()
            {
                Map(x => x.Description);
            }
        }

        public abstract  class OwnedWingItemMap<TOwner, TOwnerKey, TItemType, TKey, TKeyStrategy>
            : WingItemMap<TItemType, TKey, TKeyStrategy>
            where TKeyStrategy : IKeyStrategy, new()
            where TItemType : OwnedWingItem<TOwner, TOwnerKey, TKey>
            where TOwner : Entity<TOwnerKey>, IWingItem
        {
            //protected OwnedWingItemMap()
            //    : base()
            //{

            //}
        }

        public abstract class ElementOwnedWingItemMap<TItemType, TKey, TKeyStrategy>
                        : OwnedWingItemMap<Element, int, TItemType, TKey, TKeyStrategy>
            where TKeyStrategy : IKeyStrategy, new()
            where TItemType : OwnedWingItem<Element, int, TKey>
        {
            //protected ElementOwnedWingItemMap()
            //    : base()
            //{
            //}
        }

        public abstract class TargetOwnedWingItemMap<TItemType, TKey, TKeyStrategy>
                : OwnedWingItemMap<Target, int, TItemType, TKey, TKeyStrategy>
            where TKeyStrategy : IKeyStrategy, new()
            where TItemType : OwnedWingItem<Target, int, TKey>
        {
            //protected TargetOwnedWingItemMap()
            //    : base()
            //{
            //}
        }

        public abstract class ScopedOwnedWingItemMap<TItemType, TKey, TKeyStrategy>
                : OwnedWingItemMap<Scope, int, TItemType, TKey, TKeyStrategy>
            where TKeyStrategy : IKeyStrategy, new()
            where TItemType : OwnedWingItem<Scope, int, TKey>
        {
            //protected ScopedOwnedWingItemMap()
            //    : base()
            //{
            //}
        }

        #region old code to delete during code clean up
        //public abstract partial class WingOwnedWingItemMap<TItemType, TKey, TKeyStrategy> : OwnedWingItemMap<Wing, int, TItemType, TKey, TKeyStrategy>
        //    where TKeyStrategy : IKeyStrategy, new()
        //    where TItemType : WingOwnedWingItem<TKey>
        //{
        //    //protected WingOwnedWingItemMap()
        //    //    : base()
        //    //{
        //    //}
        //}


        //public partial class FeatureMap : WingOwnedWingItemMap<Feature, int, IdentityGeneratedKeyStrategy>
        //{
        //    public FeatureMap()
        //    {
        //        Map(x => x.ClrType)
        //            .Not.Nullable();
        //        Map(x => x.Guid).Not.Nullable();

        //        Cache.ReadWrite().Region("Features");
        //    }

        //}

        //// this is the comment for not discrim
        //public partial class WingMap : WingItemMap<Wing, int, IdentityGeneratedKeyStrategy>
        //{

        //}
        //// this is the comment for discrim
        //public partial class TargetMap : FeatureSubclassMap<Target>
        //{

        //}
        //// this is the comment for discrim
        //public partial class GeneratorMap : FeatureSubclassMap<Generator>
        //{

        //}
        //// this is the comment for not discrim
        //public partial class ScopeMap : TargetOwnedWingItemMap<Scope, int, IdentityGeneratedKeyStrategy>
        //{

        //}
        //// this is the comment for not discrim
        //public partial class ElementMap : TargetOwnedWingItemMap<Element, int, IdentityGeneratedKeyStrategy>
        //{

        //}
        //// this is the comment for not discrim
        //public partial class ScopeValueMap
        //{

        //}
        #endregion
    }
}

