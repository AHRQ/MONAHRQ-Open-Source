using System;
using Monahrq.Infrastructure.Domain.Common;
using PropertyChanged;

namespace Monahrq.Infrastructure.Domain.Physicians
{
    [Serializable,
     ImplementPropertyChanged]
    public class MedicalPracticeAddress : Address
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MedicalPracticeAddress"/> class.
        /// </summary>
        public MedicalPracticeAddress() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MedicalPracticeAddress"/> class.
        /// </summary>
        /// <param name="medicalPractice">The medical practice.</param>
        public MedicalPracticeAddress(MedicalPractice medicalPractice)
        {
            MedicalPractice = medicalPractice;
        }

        /// <summary>
        /// Gets or sets the medical practice.
        /// </summary>
        /// <value>
        /// The medical practice.
        /// </value>
        public virtual MedicalPractice MedicalPractice { get; set; }
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public virtual long Version { get; set; }
    }
}