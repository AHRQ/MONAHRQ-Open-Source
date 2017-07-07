using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Physicians
{
    [Serializable, Auditable(typeof(PhysicianAuditLog)),
     ImplementPropertyChanged]
    public class PhysicianMedicalPractice : Entity<int>, ISoftDeletableOnly
    {
        #region Properties
		/// <summary>
		/// Gets or sets the physician.
		/// </summary>
		public virtual Physician Physician { get; set; }
        /// <summary>
        /// Gets or sets the medical practice.
        /// </summary>
        /// <value>
        /// The medical practice.
        /// </value>
        public virtual MedicalPractice MedicalPractice { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public virtual IList<int> AssociatedPMPAddresses
		{
            get; set;
        }
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public long Version { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public virtual int Index { get; set; }
        #endregion
    }
}