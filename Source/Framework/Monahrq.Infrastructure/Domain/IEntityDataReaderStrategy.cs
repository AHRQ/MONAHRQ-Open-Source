using System.Data;
using Monahrq.Infrastructure.Utility;
using NHibernate;

namespace Monahrq.Infrastructure.Entities.Domain
{
    public interface IEntityDataReaderStrategy<T, TKey> where T: IEntity<TKey>
    {
        T LoadFromReader(IDataReader rdr);
        IStatelessSession CurrentSession { get; set; }
    }

    public abstract class EntityDataReaderStrategy<T, TKey> : IEntityDataReaderStrategy<T, TKey>
        where T : IEntity<TKey>
    {
        public abstract T LoadFromReader(IDataReader rdr);
        public abstract IStatelessSession CurrentSession { get; set; }

        public virtual string DatabaseTableName
        {
            get { return typeof (T).EntityTableName(); }
        }
    }
}
