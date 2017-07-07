using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monahrq.Infrastructure.Domain.NursingHomes;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Websites
{
    [ImplementPropertyChanged]
    public class WebsiteNursingHome : Entity<int>
    {
        public virtual NursingHome NursingHome { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public virtual int Index { get; set; }
    }
}
