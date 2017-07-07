using System;

namespace Monahrq.Infrastructure.Domain.Wings
{
    [Serializable]
    public enum DynamicScopeEnum
    {
        /// <summary>
        /// The none
        /// </summary>
        None,
        /// <summary>
        /// The admission source
        /// </summary>
        AdmissionSource,
        /// <summary>
        /// The admission type
        /// </summary>
        AdmissionType,
        /// <summary>
        /// The discharge disposition
        /// </summary>
        DischargeDisposition,
        /// <summary>
        /// The ed services
        /// </summary>
        EDServices,
        /// <summary>
        /// The hospital trauma level
        /// </summary>
        HospitalTraumaLevel,
        /// <summary>
        /// The race
        /// </summary>
        Race,
        /// <summary>
        /// The point of origin
        /// </summary>
        PointOfOrigin,
        /// <summary>
        /// The primary payer
        /// </summary>
        PrimaryPayer,
        /// <summary>
        /// The sex
        /// </summary>
        Sex,
        /// <summary>
        /// The custom
        /// </summary>
        Custom
    }
}