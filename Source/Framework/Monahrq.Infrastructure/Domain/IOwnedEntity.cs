using System;
namespace Monahrq.Infrastructure.Entities.Domain
{
    public interface IOwnedEntity<TOwner, TOwnerId, TId>:  IEntity<TId>
        where TOwner : Entity<TOwnerId>
    {
        TOwner Owner { get; set; }
    }
}
