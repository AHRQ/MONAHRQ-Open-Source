//using Monahrq.Sdk.Attributes.Wings;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Monahrq.Infrastructure.Core.Attributes;

//namespace Monahrq.Wing.Discharge.Inpatient
//{
//    [WingScope("Sex", typeof(Sex))]
//    public enum Sex
//    {
//        [WingScopeValue("Exclude", -1, "Exclude from Dataset")]
//        Exclude = -1,
//        [WingScopeValue("Male", 1, "Male")]
//        Male = 1,
//        [WingScopeValue("Female", 2, "Female")]
//        Female = 2,
//    }

//    [WingScope("Admission Source", typeof(AdmissionSource))]
//    public enum AdmissionSource
//    {
//        [WingScopeValue("Exclude", -1, "Exclude from Dataset")]
//        Exclude = -1,
//        [WingScopeValue("Missing", 0, "Missing")]
//        Missing = 0,
//        [WingScopeValue("ER", 1, "ER")]
//        ER = 1,
//        [WingScopeValue("OtherHospital", 2, "Another hospital")]
//        OtherHospital = 2,
//        [WingScopeValue("OtherFacility", 3, "Another fac. incl. LTC")]
//        OtherFacility = 3,
//        [WingScopeValue("LegalSystem", 4, "Court/Law enforcement")]
//        LegalSystem = 4,
//        [WingScopeValue("Routine", 5, "Routine/Birth/Other")]
//        Routine = 5,
//    }

//    [WingScope("Admission Type", typeof(AdmissionType))]
//    public enum AdmissionType
//    {
//        [WingScopeValue("Exclude", -1, "Exclude from Dataset")]
//        Exclude = -1,
//        [WingScopeValue("Missing", 0, "Missing")]
//        Missing = 0,
//        [WingScopeValue("Emergency", 1, "Emergency")]
//        Emergency = 1,
//        [WingScopeValue("Urgent", 2, "Urgent")]
//        Urgent = 2,
//        [WingScopeValue("Elective", 3, "Elective")]
//        Elective = 3,
//        [WingScopeValue("Newborn", 4, "Newborn")]
//        Newborn = 4,
//        [WingScopeValue("Trauma", 5, "Trauma Center")]
//        Trauma = 5,
//        [WingScopeValue("Other", 6, "Other")]
//        Other = 6,
//    }

//    [WingScope("Discharge Disposition", typeof(DischargeDisposition))]
//    public enum DischargeDisposition
//    {
//        [WingScopeValue("Exclude", -1, "Exclude from Dataset")]
//        Exclude = -1,
//        [WingScopeValue("Missing", 0, "Missing")]
//        Missing = 0,
//        [WingScopeValue("Routine", 1, "Routine")]
//        Routine = 1,
//        [WingScopeValue("ShortTerm", 2, "Short-term hospital")]
//        ShortTerm = 2,
//        [WingScopeValue("NursingFacility", 3, "Skilled nursing facility")]
//        NursingFacility = 3,
//        [WingScopeValue("IntermediateCare", 4, "Intermediate care")]
//        IntermediateCare = 4,
//        [WingScopeValue("OtherFacility", 5, "Another type of facility")]
//        OtherFacility = 5,
//        [WingScopeValue("HomeHealthCare", 6, "Home health care")]
//        HomeHealthCare = 6,
//        [WingScopeValue("AMA", 7, "Against medical advice")]
//        AMA = 7,
//        [WingScopeValue("Deceased", 20, "Died")]
//        Deceased = 20,
//        [WingScopeValue("DischargedAliveDestUnknown", 99, "Discharged alive")] //"Discharged alive, destination unknown"
//        DischargedAliveDestUnknown = 99,
//    }

//    [WingScope("ED Services", typeof(EDServices))]
//    public enum EDServices
//    {
//        [WingScopeValue("Exclude", -1, "Exclude from Dataset")]
//        Exclude = -1,
//        [WingScopeValue("NoEdReported", 0, "No ED Services Reported")]
//        NoEdReported = 0,
//        [WingScopeValue("EdReported", 1, "ED Services Reported")]
//        EdReported = 1,
//    }

    

//    [WingScope("Primary Payer", typeof(PrimaryPayer))]
//    public enum PrimaryPayer
//    {
//        [WingScopeValue("Exclude", -1, "Exclude from Dataset")]
//        Exclude = -1,
//        [WingScopeValue("Missing", 0, "Missing")]
//        Missing = 0,
//        [WingScopeValue("Medicare", 1, "Medicare")]
//        Medicare = 1,
//        [WingScopeValue("Medicaid", 2, "Medicaid")]
//        Medicaid = 2,
//        [WingScopeValue("Private", 3, "Private including HMO")]
//        Private = 3,
//        [WingScopeValue("SelfPay", 4, "Self-pay")]
//        SelfPay = 4,
//        [WingScopeValue("NoCharge", 5, "No Charge")]
//        NoCharge = 5,
//        [WingScopeValue("Other", 6, "Other")]
//        Other = 6,
//        [WingScopeValue("Retain", 99, "Retain Value")]
//        Retain = 99,
//    }

//    [WingScope("Point of Origin", typeof(PointOfOrigin))]
//    public enum PointOfOrigin
//    {
//        [WingScopeValue("Exclude", -1, "Exclude from Dataset")]
//        Exclude = -1,
//        [WingScopeValue("Missing", 0, "Missing")]
//        Missing = 0,
//        [WingScopeValue("NonHealthCare", 1, "Non-health care facility point of origin")]
//        NonHealthCare = 1,
//        [WingScopeValue("Clinic", 2, "Clinic")]
//        Clinic = 2,
//        [WingScopeValue("TransferFromOther", 4, "Transfer from a hospital (different facility)")]
//        TransferFromOther = 4,
//        [WingScopeValue("TransferInternal", 5, "Transfer from nursing facility OR (w/admin type = newborn) born in this hospital")]
//        TransferInternal = 5,
//        [WingScopeValue("TransferExternal", 6, "Transfer from another health care facility OR (w/admin type = newborn) born outside this hospital")]
//        TransferExternal = 6,
//        [WingScopeValue("ER", 7, "Emergency room")]
//        ER = 7,
//        [WingScopeValue("LegalSystem", 8, "Court/law enforcement")]
//        LegalSystem = 8,
//        [WingScopeValue("OtherHealthAgency", 11, "Transfer from another Home Health Agency")]
//        OtherHealthAgency = 11,
//        [WingScopeValue("ReadminFromSame", 12, "Readmission to Same Home Health Agency")]
//        ReadminFromSame = 12,
//        [WingScopeValue("TransferDistrict", 13, "Transfer from one distinct unit of the hospital to another distinct unit of the same hospital")]
//        TransferDistrict = 13,
//        [WingScopeValue("TransferAmbulatory", 14, "Transfer from ambulatory surgery center")]
//        TransferAmbulatory = 14,
//        [WingScopeValue("TransferHospice", 15, "Transfer from hospice and is under a hospice plan of care or enrolled in a hospice program")]
//        TransferHospice = 15,
//    }


//}
