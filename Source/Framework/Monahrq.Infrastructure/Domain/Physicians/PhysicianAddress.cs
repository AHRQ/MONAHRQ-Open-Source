using System;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Domain.Audits;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.NursingHomes;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Physicians
{
    [Serializable, Auditable(typeof(PhysicianAuditLog)),
     ImplementPropertyChanged]
    public class PhysicianAddress : Address
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicianAddress"/> class.
        /// </summary>
        public PhysicianAddress() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicianAddress"/> class.
        /// </summary>
        /// <param name="physician">The physician.</param>
        public PhysicianAddress(Physician physician)
        {
            Physician = physician;
        }

        /// <summary>
        /// Gets or sets the physician.
        /// </summary>
        /// <value>
        /// The physician.
        /// </value>
        public virtual Physician Physician { get; set; }
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public virtual long Version { get; set; }
    }
}