using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Core.Attributes;

namespace Monahrq.Wing.HospitalSpendingByClaimType
{
    [WingScope("Period", typeof(Period))]
    public enum Period
    {
        [WingScopeValue("Exclude", -1)]
        Exclude = -1,

        [WingScopeValue("Unknown", 0)]
        Unknown = 0,

        [WingScopeValue("DuringHospitalAdmission", 1, "During Index Hospital Admission")]
        DuringHospitalAdmission = 1,

        [WingScopeValue("UpTo30DaysAfterDischarge", 2, "1 through 30 days After Discharge from Index Hospital Admission")]
        UpTo30DaysAfterDischarge = 2,

        [WingScopeValue("CompleteEpisode", 3, "Complete Episode")]
        CompleteEpisode = 3,

        [WingScopeValue("OneToThreeDaysPrior", 4, "1 to 3 days Prior to Index Hospital Admission")]
        OneToThreeDaysPrior = 4,
    }
}
