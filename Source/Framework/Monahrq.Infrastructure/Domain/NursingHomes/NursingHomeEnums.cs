using System;
using System.ComponentModel;

namespace Monahrq.Infrastructure.Domain.NursingHomes
{
    [Serializable]
    public enum ResFamCouncilEnum
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,
        /// <summary>
        /// The resident
        /// </summary>
        Resident = 1,
        /// <summary>
        /// The family
        /// </summary>
        Family = 2,
        /// <summary>
        /// The both
        /// </summary>
        Both = 3
    }

    [Serializable]
    public enum SprinklerStatusEnum
    {
        /// <summary>
        /// The yes
        /// </summary>
        Yes = 0,
        /// <summary>
        /// The partial
        /// </summary>
        Partial = 1,
        /// <summary>
        /// The no
        /// </summary>
        No = 2,
        /// <summary>
        /// The data not avaiable
        /// </summary>
        [Description("Data Not Available")]
        DataNotAvailable = 3
    }

    [Serializable]
    public enum FacilityTypeEnum
    {
        //[Description("Unknown")]
        //Unknown = -1,
        [Description("SHORT STAY")]
        ShortStay = 0,
        [Description("LONG STAY")]
        LongStay = 1,
        [Description("LONG/SHORT STAY")]
        LongShortStay = 2
    }

    [Serializable]
    public enum ResSizeEnum
    {
        //[Description("Unknown")]
        //Unknown = -1,
        [Description("1-80")]
        Lte80 = 0,
        [Description(">80")]
        Gt80 = 1
    }

    [Serializable]
    public enum BedSizeEnum
    {
        //[Description("Unknown")]
        //Unknown = -1,
        [Description("1-100")]
        Lte100 = 0,
        [Description(">100")]
        Gt100 = 1
    }

    [Serializable]
    public enum FunctionEnum
    {
        [Description("Data Not Available")]
        DataNotAvailable,
        [Description("Too New to Rate")]
        TooNewToRate
    }
}
