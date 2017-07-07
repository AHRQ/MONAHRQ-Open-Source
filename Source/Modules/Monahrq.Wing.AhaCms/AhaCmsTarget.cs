using System;
using System.ComponentModel.DataAnnotations;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;

namespace Monahrq.Wing.AhaCms
{
    [Serializable]
    [WingTarget("AHA Annual Survey Database", "53ccbd64-c36f-4c44-b8e2-fcd057a39633", "Mapping target for AHA Annual Survey CMS Database", false, 7)]
    public class AhaCmsTarget : ContentPartRecord
    {
        [Required]
        [WingTargetElement("Name", "Name", true, 1, "Hospital Name")]
        public new string Name { get; set; }
        [WingTargetElement("Address", "Address", false, 2, "Hospital Address")]
        public string Address { get; set; }
        [WingTargetElement("Telephone", "Telephone", false, 3, "Hospital Telephone")]
        public string Telephone { get; set; }
        [WingTargetElement("HospitalWebsite", "HospitalWebsite", false, 4, "Hospital Website")]
        public string HospitalWebsite { get; set; }
        [WingTargetElement("CMSCertificationNumber", "CMS Certification#", false, 4, "CMS Certification#")]
        public string CMSCertificationNumber { get; set; }
        [WingTargetElement("TypeOfFacility", "Type of Facility", false, 6, "Type of Facility")]
        public string TypeOfFacility { get; set; }
        [WingTargetElement("TypeOfControl", "Type of Control", false, 7, "Type of Control")]
        public string TypeOfControl { get; set; }
        [WingTargetElement("TotalStaffedBeds", "Total Staffed Beds", false, 8, "Total Staffed Beds")]
        public int TotalStaffedBeds { get; set; }
        [WingTargetElement("TotalPatientRevenue", "Total Patient Revenue", false, 9, "Total Patient Revenue")]
        public int TotalPatientRevenue { get; set; }
        [WingTargetElement("TotalDischarges", "Total Discharges", false, 10, "Total Discharges")]
        public int TotalDischarges { get; set; }
        [WingTargetElement("TotalPatientDays", "Total Patient Days", false, 11, "Total Patient Days")]
        public int TotalPatientDays { get; set; }
    }
}