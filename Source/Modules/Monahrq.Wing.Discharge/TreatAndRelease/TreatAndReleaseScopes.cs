//using Monahrq.Sdk.Attributes.Wings;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Monahrq.Infrastructure.Core.Attributes;

//namespace Monahrq.Wing.Discharge.TreatAndRelease
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

//    [WingScope("Race", typeof(Race))]
//    public enum Race
//    {
//        [WingScopeValue("Exclude", -1, "Exclude from Dataset")]
//        Exclude = -1,
//        [WingScopeValue("Missing", 0, "Missing")]
//        Missing = 0,
//        [WingScopeValue("White", 1, "White")]
//        White = 1,
//        [WingScopeValue("Black", 2, "Black")]
//        Black = 2,
//        [WingScopeValue("Hispanic", 3, "Hispanic")]
//        Hispanic = 3,
//        [WingScopeValue("AsianPacificIsland", 4, "Asian or Pacific Island")]
//        AsianPacificIsland = 4,
//        [WingScopeValue("NativeAmerican", 5, "Native American")]
//        NativeAmerican = 5,
//        [WingScopeValue("Other", 6, "Other")]
//        Other = 6,
//        [WingScopeValue("Retain", 99, "Retain Value")]
//        Retain = 99,
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


//    [WingScope("Hospital Trauma Level", typeof(HospitalTraumaLevel))]
//    public enum HospitalTraumaLevel
//    {
//        [WingScopeValue("Exclude", -1, "Exclude from Dataset")]
//        Exclude = -1,
//        [WingScopeValue("NotTraumaCenter", 0, "Not a trauma center ")]
//        NotTraumaCenter = 0,
//        [WingScopeValue("Level_1", 1, "Trauma center level I ")]
//        Level_1 = 1,
//        [WingScopeValue("Level_2", 2, "Trauma center level II ")]
//        Level_2 = 2,
//        [WingScopeValue("Level_3", 3, "Trauma center level III ")]
//        Level_3 = 3,
//        [WingScopeValue("Level_4", 4, "Trauma center level IV ")]
//        Level_4 = 4,
//        [WingScopeValue("Level_1_2", 8, "Trauma center level I or II ")]
//        Level_1_2 = 8,
//        [WingScopeValue("Level_1_2_3", 9, "Trauma center level I, II, or III ")]
//        Level_1_2_3 = 9,
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
//        [WingScopeValue("DischargedAliveDestUnknown", 99, "Discharged alive, destination unknown")]
//        DischargedAliveDestUnknown = 99,
//    }

//}
