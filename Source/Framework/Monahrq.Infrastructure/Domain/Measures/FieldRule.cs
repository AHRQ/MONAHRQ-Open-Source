using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Measures;

namespace Monahrq.Infrastructure.Domain.Measures
{
    public class FieldRule : OwnedEntity<Measure, int, int>
    {
        public virtual string Message { get; set; }
        public virtual string Field { get; set; }

        public FieldRule(Measure measure, string field) : base(measure)
        {
            Field = field;
        }
        protected FieldRule() : base() { }

    }
}
