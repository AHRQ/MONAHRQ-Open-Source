using System;
using System.ComponentModel;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Physicians
{
    [Serializable,
     ImplementPropertyChanged,
     EntityTableName("MedicareAssignments")]
    public class MedicareAssignment : OwnedEntity<Physician, int, int>
    {
        /// <summary>
        /// Gets or sets the accepts medicare assignment.
        /// </summary>
        /// <value>
        /// The accepts medicare assignment.
        /// </value>
        public virtual MedicalAssignmentEnum? AcceptsMedicareAssignment { get; set; }
        /// <summary>
        /// Gets or sets the participates in erx.
        /// </summary>
        /// <value>
        /// The participates in erx.
        /// </value>
        public virtual bool? ParticipatesInERX { get; set; }
        /// <summary>
        /// Gets or sets the participates in PQRS.
        /// </summary>
        /// <value>
        /// The participates in PQRS.
        /// </value>
        public virtual bool? ParticipatesInPQRS { get; set; }
        /// <summary>
        /// Gets or sets the participates in ehr.
        /// </summary>
        /// <value>
        /// The participates in ehr.
        /// </value>
        public virtual bool? ParticipatesInEHR { get; set; }
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public long Version { get; set; }
    }

    [Serializable]
    public enum MedicalAssignmentEnum
    {
        [Description("Physician(s) accepts Medicare approved amount as payment in full")]
        Y = 0,
        [Description("Physician(s) may accept Medicare Assignment")]
        M = 1
    }

    [Serializable,
     ImplementPropertyChanged]
    public class PhysicianMedicareAssignment : Entity<int>
    {
        public PhysicianMedicareAssignment()
        {}

        /// <summary>
        /// Gets or sets the assignment.
        /// </summary>
        /// <value>
        /// The assignment.
        /// </value>
        public virtual MedicareAssignment Assignment { get; set; }
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public long Version { get; set; }
        public virtual int Index { get; set; }
    }
}