using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Websites 
{
    [ImplementPropertyChanged, EntityTableName("Websites_ActivityLogEntries")]
    public class ActivityLogEntry: Entity<int>
    {
        public virtual string Description { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual int Index { get; set; }

        protected ActivityLogEntry() : base() { }

        public ActivityLogEntry(string description, DateTime date) : this()
        {
            Description = description;
            Date = date;
        }
    }
}
