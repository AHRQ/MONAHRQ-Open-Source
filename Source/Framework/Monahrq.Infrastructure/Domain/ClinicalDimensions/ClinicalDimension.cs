using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.Domain.ClinicalDimensions
{
    public abstract class ClinicalDimension: Entity<int>
    {
        public virtual string Description { get; set; }
        public virtual int FirstYear { get; set; }
        public virtual int LastYear { get; set; }
        public decimal Version { get; set; }
    }
}
