using System;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Measures;

namespace Monahrq.Infrastructure.Domain.Measures
{
    public enum Editability
    {
        Open, Closed, EditWithWarning
    };

    [Serializable]
    public class EditRule : OwnedEntity<Measure, int, int>
    {
        protected EditRule() : base() { }
        public EditRule(Measure measure) : base(measure)
        {
            Name = "EditRule for measure: " + measure.Name;
        }
        public virtual Editability Editability { get; set; }

    }
}
