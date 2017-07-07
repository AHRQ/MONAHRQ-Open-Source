using System;
using FluentNHibernate.Mapping;

namespace Monahrq.Infrastructure.Entities.Data.Strategies
{
    public interface IKeyStrategy
    {
        void Apply(IdentityPart identityPart);
    }
}
