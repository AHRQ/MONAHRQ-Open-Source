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
     ImplementPropertyChanged,
     EntityTableName("MedicalPractices")]
    public class MedicalPractice : Entity<int>, ISoftDeletableOnly
    {
        #region Properties
        /// <summary>
        /// Gets or sets the group practice PAC identifier.
        /// </summary>
        /// <value>
        /// The group practice PAC identifier.
        /// </value>
        public virtual string GroupPracticePacId { get; set; }

        /// <summary>
        /// Gets or sets the name of the DBA.
        /// </summary>
        /// <value>
        /// The name of the DBA.
        /// </value>
        public virtual string DBAName { get; set; }

        /// <summary>
        /// Gets or sets the numberOf group practice members.
        /// </summary>
        /// <value>
        /// The numberOf group practice members.
        /// </value>
        public virtual int? NumberofGroupPracticeMembers { get; set; }

        /// <summary>
        /// Gets or sets the addresses.
        /// </summary>
        /// <value>
        /// The addresses.
        /// </value>
        public virtual IList<MedicalPracticeAddress> Addresses { get; set; }

        /// <summary>
        /// Gets or sets the marker of address line2 suppression.
        /// </summary>
        /// <value>
        /// The marker of address line2 suppression.
        /// </value>
        public virtual bool? MarkerofAdressLine2Suppression { get; set; }

        /// <summary>
        /// Gets or sets the address count for display.
        /// </summary>
        /// <value>
        /// The address count for display.
        /// </value>
        public int AddressCountForDisplay { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public virtual string State { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public long Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is edited.
        /// </summary>
        /// <value>
        /// {D255958A-8513-4226-94B9-080D98F904A1}  <c>true</c> if this instance is edited; otherwise, <c>false</c>.
        /// </value>
        public bool IsEdited { get; set; }
        #endregion

        #region Methods

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            Addresses = new List<MedicalPracticeAddress>();
        }

        /// <summary>
        /// Cleans the before save.
        /// </summary>
        public override void CleanBeforeSave()
        {
            base.CleanBeforeSave();

            if(string.IsNullOrEmpty(this.State) && this.Addresses.Any())
            {
                this.State = this.Addresses[0].State;
            }
        }

        /// <summary>
        /// Determines whether this instance has addresses.
        /// </summary>
        /// <returns></returns>
        public bool HasAddresses()
        {
            return Addresses != null && Addresses.Any();
        }

        #endregion
    }
}