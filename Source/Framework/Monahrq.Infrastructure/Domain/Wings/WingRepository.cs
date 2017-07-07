using System;

namespace Monahrq.Infrastructure.Entities.Domain.Wings.Repository
{
    public partial class WingRepository
    {
        public static Wing New(string name)
        {
            return new Wing(name);
        }
    }

    public partial class TargetRepository
    {
        public static Target New(Wing wing, Guid guid, string name)
        {
            return new Target(wing, guid, name);
        }
    }

    public partial class ScopeRepository
    {
        public static Scope New(Target target, string name, Type scopeType)
        {
            return new Scope(target, name, scopeType);
        }
    }

    public partial class ElementRepository
    {
        public static Element New(Target target, string name)
        {
            return new Element(target, name);
        }
    }

    public partial class ScopeValueRepository
    {
        public static ScopeValue New(Scope scope, string name)
        {
            return new ScopeValue(scope, name);
        }
    }


}
