using FluentNHibernate.Mapping;

namespace Monahrq.Infrastructure.Entities.Data.Strategies
{
    public abstract class KeyStrategy : IKeyStrategy
    {
        /// <summary>
        /// Applies the specified identity part.
        /// </summary>
        /// <param name="identityPart">The identity part.</param>
        public abstract void Apply(IdentityPart identityPart);
    }

    public class IdentityGeneratedKeyStrategy : KeyStrategy
    {
        /// <summary>
        /// Applies the specified identity part.
        /// </summary>
        /// <param name="identityPart">The identity part.</param>
        public override void Apply(IdentityPart identityPart)
        {
            identityPart.GeneratedBy.Identity();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HiLowIdentityGeneratedKeyStrategy : KeyStrategy
    {
        /// <summary>
        /// Applies the specified identity part.
        /// </summary>
        /// <param name="identityPart">The identity part.</param>
        public override void Apply(IdentityPart identityPart)
        {
            identityPart.GeneratedBy.HiLo("100", "Id", "100000");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GuidGeneratedKeyStrategy : KeyStrategy
    {
        /// <summary>
        /// Applies the specified identity part.
        /// </summary>
        /// <param name="identityPart">The identity part.</param>
        public override void Apply(IdentityPart identityPart)
        {
            identityPart.GeneratedBy.GuidComb();
        }
    }

    public class AssignedValueKeyStrategy : KeyStrategy
    {
        /// <summary>
        /// Applies the specified identity part.
        /// </summary>
        /// <param name="identityPart">The identity part.</param>
        public override void Apply(IdentityPart identityPart)
        {
            identityPart.GeneratedBy.Assigned();
        }
    }

}
