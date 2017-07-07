using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Core.Attributes;

namespace Monahrq.Wing.HospitalSpendingByClaimType
{
    [WingScope("Claim Type", typeof(ClaimType))]
    public enum ClaimType
    {
        [WingScopeValue("Exclude", -1)]
        Exclude = -1,

        [WingScopeValue("Unknown", 0)]
        Unknown = 0,

        [WingScopeValue("HomeHealthAgency", 1, "Home Health Agency")]
        HomeHealthAgency = 1,

        [WingScopeValue("Hospice", 2, "Hospice")]
        Hospice = 2,

        [WingScopeValue("Inpatient", 3, "Inpatient")]
        Inpatient = 3,

        [WingScopeValue("Outpatient", 4, "Outpatient")]
        Outpatient = 4,

        [WingScopeValue("SkilledNursingFacility", 5, "Skilled Nursing Facility")]
        SkilledNursingFacility = 5,

        [WingScopeValue("DurableMedicalEquipment", 6, "Durable Medical Equipment")]
        DurableMedicalEquipment = 6,

        [WingScopeValue("Carrier", 7, "Carrier")]
        Carrier = 7,

        [WingScopeValue("Total", 8, "Total")]
        Total = 8
    }
}
