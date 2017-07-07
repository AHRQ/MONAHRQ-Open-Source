using System;
using Monahrq.Infrastructure.Core.Attributes;

namespace Monahrq.Infrastructure.Domain.Wings
{
    [WingScope("Sex", typeof (Sex))]
    public enum Sex
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
        [WingScopeValue("Missing", 0, "Missing")] Missing = 0,
        [WingScopeValue("Male", 1, "Male")] Male = 1,
        [WingScopeValue("Female", 2, "Female")] Female = 2,
    }

    [WingScope("Admission Source", typeof (AdmissionSource))]
    public enum AdmissionSource
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
        [WingScopeValue("Missing", 0, "Missing")] Missing = 0,
        [WingScopeValue("ER", 1, "ER")] ER = 1,
        [WingScopeValue("OtherHospital", 2, "Another hospital")] OtherHospital = 2,
        [WingScopeValue("OtherFacility", 3, "Another fac. incl. LTC")] OtherFacility = 3,
        [WingScopeValue("LegalSystem", 4, "Court/Law enforcement")] LegalSystem = 4,
        [WingScopeValue("Routine", 5, "Routine/Birth/Other")] Routine = 5,
    }

    [WingScope("Admission Type", typeof (AdmissionType))]
    public enum AdmissionType
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
        [WingScopeValue("Missing", 0, "Missing")] Missing = 0,
        [WingScopeValue("Emergency", 1, "Emergency")] Emergency = 1,
        [WingScopeValue("Urgent", 2, "Urgent")] Urgent = 2,
        [WingScopeValue("Elective", 3, "Elective")] Elective = 3,
        [WingScopeValue("Newborn", 4, "Newborn")] Newborn = 4,
        [WingScopeValue("Trauma", 5, "Trauma Center")] Trauma = 5,
        [WingScopeValue("Other", 6, "Other")] Other = 6,
    }

    [WingScope("Discharge Disposition", typeof (DischargeDisposition))]
    public enum DischargeDisposition
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
        [WingScopeValue("Missing", 0, "Missing")] Missing = 0,
        [WingScopeValue("Routine", 1, "Routine")] Routine = 1,
        [WingScopeValue("ShortTerm", 2, "Short-term hospital")] ShortTerm = 2,
        [WingScopeValue("NursingFacility", 3, "Skilled nursing facility")] NursingFacility = 3,
        [WingScopeValue("IntermediateCare", 4, "Intermediate care")] IntermediateCare = 4,
        [WingScopeValue("OtherFacility", 5, "Another type of facility")] OtherFacility = 5,
        [WingScopeValue("HomeHealthCare", 6, "Home health care")] HomeHealthCare = 6,
        [WingScopeValue("AMA", 7, "Against medical advice")] AMA = 7,
        [WingScopeValue("Deceased", 20, "Died")] Deceased = 20,
        [WingScopeValue("DischargedAliveDestUnknown", 99, "Discharged alive, destination unknown")] DischargedAliveDestUnknown = 99

        /*   
        21 = Disch/trans to court/law enforcement (added 10/01/09)  
        30 = Still a patient 
        43 = Fed hospital (added 10/01/03) 
        50 = Hospice-home 
        51 = Hospice-medical facility 
        61 = Swing Bed (added 10/01/2001) 
        62 = Rehab fac/unit (added 10/01/2001) 
        63 = LTC hospital (added 10/01/2001) 
        64 = Nursing facility–Medicaid certified (added 10/01/02) 
        65 = Psych hosp/unit (added 10/01/03) 
        66 = Critical access hospital (added  10/01/05) 
        69 = Designated Disaster Alternative Care Site 
        70 = Oth institution (added 04/01/08) 
        71 = OP services-other facility  (10/01/01– 09/30/03 only) 
        72 = OP services-this facility  (10/01/01– 09/30/03 only) 
        81 = Home-Self care w Planned Readmission 
        82 = Short Term Hospital w Planned Readmission 
        83 = SNF w Planned Readmission 
        84 = Cust/supp care w Planned Readmission 
        85 = Canc/child hosp w Planned Readmission 
        86 = Home Health Service w Planned Readmission 
        87 = Court/law enfrc w Planned Readmission 
        88 = Federal Hospital w Planned Readmission 
        89 = Swing Bed w Planned Readmission 
        90 = Rehab Facility/ Unit w Planned Readmission 
        91 = LTCH w Planned Readmission 
        92 = Nursg Fac-Medicaid Cert w Planned Readmiss 
        93 = Psych Hosp/Unit w Planned Readmission 
        94 = Crit Acc Hosp w Planned Readmission 
        95 = Oth Institution w Planned Readmission
        */
    }

    [WingScope("ED Services", typeof (EDServices))]
    public enum EDServices
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
        [WingScopeValue("NoEdReported", 0, "No ED Services Reported")] NoEdReported = 0,
        [WingScopeValue("EdReported", 1, "ED Services Reported")] EdReported = 1,
    }

    [WingScope("Race", typeof (Race))]
    public enum Race
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
        [WingScopeValue("Missing", 0, "Missing")] Missing = 0,
        [WingScopeValue("White", 1, "White")] White = 1,
        [WingScopeValue("Black", 2, "Black")] Black = 2,
        [WingScopeValue("Hispanic", 3, "Hispanic")] Hispanic = 3,
        [WingScopeValue("AsianPacificIsland", 4, "Asian or Pacific Island")] AsianPacificIsland = 4,
        [WingScopeValue("NativeAmerican", 5, "Native American")] NativeAmerican = 5,
        [WingScopeValue("Other", 6, "Other")] Other = 6,
        [WingScopeValue("Retain", 99, "Retain Value")] Retain = 99
    }

    [WingScope("Primary Payer", typeof (PrimaryPayer))]
    public enum PrimaryPayer
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
        [WingScopeValue("Missing", 0, "Missing")] Missing = 0,
        [WingScopeValue("Medicare", 1, "Medicare")] Medicare = 1,
        [WingScopeValue("Medicaid", 2, "Medicaid")] Medicaid = 2,
        [WingScopeValue("Private", 3, "Private including HMO")] Private = 3,
        [WingScopeValue("SelfPay", 4, "Self-pay")] SelfPay = 4,
        [WingScopeValue("NoCharge", 5, "No Charge")] NoCharge = 5,
        [WingScopeValue("Other", 6, "Other")] Other = 6,
        [WingScopeValue("Retain", 99, "Retain Value")] Retain = 99
    }

    [WingScope("Point of Origin", typeof (PointOfOrigin))]
    public enum PointOfOrigin
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
        [WingScopeValue("Missing", 0, "Missing")] Missing = 0,
        [WingScopeValue("NonHealthCare", 1, "Non-health care facility point of origin")] NonHealthCare = 1,
        [WingScopeValue("Clinic", 2, "Clinic")] Clinic = 2,
        [WingScopeValue("TransferFromOther", 4, "Transfer from a hospital (different facility)")] TransferFromOther = 4,
        [WingScopeValue("TransferInternal", 5, "Transfer from nursing facility OR (w/admin type = newborn) born in this hospital")] TransferInternal = 5,
        [WingScopeValue("TransferExternal", 6, "Transfer from another health care facility OR (w/admin type = newborn) born outside this hospital")] TransferExternal = 6,
        [WingScopeValue("ER", 7, "Emergency room")] ER = 7,
        [WingScopeValue("LegalSystem", 8, "Court/law enforcement")] LegalSystem = 8,
        [WingScopeValue("OtherHealthAgency", 11, "Transfer from another Home Health Agency")] OtherHealthAgency = 11,
        [WingScopeValue("ReadminFromSame", 12, "Readmission to Same Home Health Agency")] ReadminFromSame = 12,
        [WingScopeValue("TransferDistrict", 13, "Transfer from one distinct unit of the hospital to another distinct unit of the same hospital")] TransferDistrict = 13,
        [WingScopeValue("TransferAmbulatory", 14, "Transfer from ambulatory surgery center")] TransferAmbulatory = 14,
        [WingScopeValue("TransferHospice", 15, "Transfer from hospice and is under a hospice plan of care or enrolled in a hospice program")] TransferHospice = 15
    }

    [WingScope("Hospital Trauma Level", typeof (HospitalTraumaLevel))]
    public enum HospitalTraumaLevel
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
        [WingScopeValue("NotTraumaCenter", 0, "Not a trauma center ")] NotTraumaCenter = 0,
        [WingScopeValue("Level_1", 1, "Trauma center level I ")] Level_1 = 1,
        [WingScopeValue("Level_2", 2, "Trauma center level II ")] Level_2 = 2,
        [WingScopeValue("Level_3", 3, "Trauma center level III ")] Level_3 = 3,
        [WingScopeValue("Level_4", 4, "Trauma center level IV ")] Level_4 = 4,
        [WingScopeValue("Level_1_2", 8, "Trauma center level I or II ")] Level_1_2 = 8,
        [WingScopeValue("Level_1_2_3", 9, "Trauma center level I, II, or III ")] Level_1_2_3 = 9
    }

    [Flags]
    public enum ICDCodeTypeEnum
    {
        ICD9 = 9,
        ICD10 = 10
    }

    // [Flags]
    [WingScope("Yes or No", typeof (YesNoEnum))]
    public enum YesNoEnum 
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
        [WingScopeValue("Missing", 0, "Missing")] Missing = 0,
        [WingScopeValue("Yes", 1, "Yes")] Yes = 1,
        [WingScopeValue("No", 2, "No")] No = 2,
        [WingScopeValue("Inapplicable", 98, "Inapplicable")] Inapplicable = 98,
        [WingScopeValue("NoAnswerGiven", 99, "No Answer Given")] NoAnswerGiven = 99
    }

    //[WingScope("Yes or No1", typeof (YesNo1Enum))]
    //public enum YesNo1Enum : int
    //{
    //    [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
    //    //[WingScopeValue("Missing", 0, "Missing")] Missing = 0,
    //    [WingScopeValue("Yes", 1, "Yes")] Yes = 1,
    //    [WingScopeValue("No", 0, "No")] No = 0,
    //    [WingScopeValue("Inapplicable", 98, "Inapplicable")] Inapplicable = 98,
    //    [WingScopeValue("No Answer Given", 99, "No Answer Given")] NoAnswerGiven = 98
    //}

    //[WingScope("Yes or No2", typeof (YesNo2Enum))]
    //public enum YesNo2Enum : int
    //{
    //    [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
    //    [WingScopeValue("Missing", 0, "Missing")] Missing = 0,
    //    [WingScopeValue("Yes", 1, "Yes")] Yes = 1,
    //    [WingScopeValue("No", 2, "No")] No = 2,
    //    [WingScopeValue("Inapplicable", 98, "Inapplicable")] Inapplicable = 98,
    //    [WingScopeValue("No Answer Given", 99, "No Answer Given")] NoAnswerGiven = 99
    //}

    // [Flags]
    [WingScope("How Often", typeof (HowOftenEnum))]
    public enum HowOftenEnum 
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
        [WingScopeValue("Missing", 0, "Missing")] Missing = 0,
        [WingScopeValue("Never", 1, "Never")] Never = 1,
        [WingScopeValue("Sometimes", 2, "Sometimes")] Sometimes = 2,
        [WingScopeValue("Usually", 3, "Usually")] Usually = 3,
        [WingScopeValue("Always", 4, "Always")] Always = 4,
        [WingScopeValue("Inapplicable", 98, "Inapplicable")] Inapplicable = 98,
        [WingScopeValue("NoAnswerGiven", 99, "No Answer Given")] NoAnswerGiven = 99
    }

    [WingScope("How Often2", typeof (HowOften2Enum))]
    public enum HowOften2Enum 
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
        [WingScopeValue("Missing", 0, "Missing")] Missing = 0,
        [WingScopeValue("Never", 1, "Never")] Never = 1,
        [WingScopeValue("Sometimes", 2, "Sometimes")] Sometimes = 2,
        [WingScopeValue("Usually", 3, "Usually")] Usually = 3,
        [WingScopeValue("Always", 4, "Always")] Always = 4,
        [WingScopeValue("Inapplicable", 98, "Inapplicable")] Inapplicable = 98,
        [WingScopeValue("NoAnswerGiven", 99, "No Answer Given")] NoAnswerGiven = 99
    }

    // [Flags]
    [WingScope("Definite", typeof(DefiniteEnum))]
    public enum DefiniteEnum
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")]Exclude = -1,
        [WingScopeValue("Missing", 0, "Missing")] Missing = 0,
        [WingScopeValue("YesDefinitely", 1, "Yes Definitely")] YesDefinitely = 1,
        [WingScopeValue("YesSomewhat", 2, "Yes Somewhat")] YesSomewhat = 2,
        [WingScopeValue("No", 3, "No")] No = 3
    }

    // [Flags]
    [WingScope("Rate Provider", typeof(RateProviderEnum))]
    public enum RateProviderEnum 
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")]Exclude = -1,
        [WingScopeValue("Zero", 0, "WORST PROVIDER POSSIBLE")] Zero = 0,
        [WingScopeValue("One", 1, "ONE")] One = 1,
        [WingScopeValue("Two", 2, "TWO")] Two = 2,
        [WingScopeValue("Three", 3, "THREE")] Three = 3,
        [WingScopeValue("Four", 4, "FOUR")] Four = 4,
        [WingScopeValue("Five", 5, "FIVE")] Five = 5,
        [WingScopeValue("Six", 6, "SIX")] Six = 6,
        [WingScopeValue("Seven", 7, "SEVEN")] Seven = 7,
        [WingScopeValue("Eight", 8, "EIGHT")] Eight = 8,
        [WingScopeValue("Nine", 9, "NINE")] Nine = 9,
        [WingScopeValue("Ten", 10, "BEST PROVIDER POSSIBLE")] Ten = 10
    }

    // [Flags]
    [WingScope("Gender", typeof(GenderEnum))]
    public enum GenderEnum 
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")]Exclude = -1,
        [WingScopeValue("Missing", 0, "Missing")] Missing = 0, 
        [WingScopeValue("Male", 1, "Male")] Male = 1,
        [WingScopeValue("Male", 2, "Female")] Female = 2
    }

    [WingScope("NumberOfTimes", typeof (NumberOfTimesEnum))]
    public enum NumberOfTimesEnum 
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
        [WingScopeValue("Missing", 0, "Missing")] Missing = 0,
        [WingScopeValue("Never", 1, "Never")] Never = 1,
        [WingScopeValue("Once", 2, "Once")] Once = 2,
        [WingScopeValue("Two or more times", 3, "Two or more times")] ThreeTimes = 3,
        [WingScopeValue("Inapplicable", 98, "Inapplicable")] Inapplicable = 98,
        [WingScopeValue("NoAnswerGiven", 99, "No Answer Given")] NoAnswerGiven = 99,
        //[WingScopeValue("4 times", 4, "4 times")]
        //FourTimes = 4,
        //[WingScopeValue("5 to 9 times", 5, "5 to 9 times")]
        //FiveToNineTimes = 5,
        //[WingScopeValue("10 or more times", 6, "10 or more times")]
        //TenOeMoreTimes = 6,
        //[WingScopeValue("Inapplicable", 98, "Inapplicable")]
        //Inapplicable = 98,
        //[WingScopeValue("No Answer Given", 99, "No Answer Given")]
        //NoAnswerGiven = 99,
        //[WingScopeValue("Missing", 999, "Missing")]
        //Missing = 999
    }


    [WingScope("NumberOfTimes2", typeof (NumberOfTimes2Enum))]
    public enum NumberOfTimes2Enum
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
        [WingScopeValue("Missing", 0, "Missing")] Missing = 0,
        [WingScopeValue("Never", 1, "Never")] Never = 1,
        [WingScopeValue("Once or twice", 2, "Once or twice")] TwoTimes = 2,
        [WingScopeValue("Three or more times", 3, "Three or more times")] ThreeTimes = 3,
        [WingScopeValue("Inapplicable", 98, "Inapplicable")] Inapplicable = 98,
        [WingScopeValue("NoAnswerGiven", 99, "No Answer Given")] NoAnswerGiven = 99,
        //[WingScopeValue("4 times", 4, "4 times")]
        //FourTimes = 4,
        //[WingScopeValue("5 to 9 times", 5, "5 to 9 times")]
        //FiveToNineTimes = 5,
        //[WingScopeValue("10 or more times", 6, "10 or more times")]
        //TenOeMoreTimes = 6,
        //[WingScopeValue("Inapplicable", 98, "Inapplicable")]
        //Inapplicable = 98,
        //[WingScopeValue("No Answer Given", 99, "No Answer Given")]
        //NoAnswerGiven = 99,
        //[WingScopeValue("Missing", 999, "Missing")]
        //Missing = 999
    }

    //[WingScope("Ratings", typeof(RatingsEnum))]
    //public enum RatingsEnum : int
    //{
    //    [WingScopeValue("Exclude", -1, "Exclude from Dataset")]
    //    Exclude = -1,
    //    [WingScopeValue("Missing", 0, "Missing")]
    //    Missing = 0,
    //    [WingScopeValue("Excellent", 1, "Excellent")]
    //    Excellent = 1,
    //    [WingScopeValue("VeryGood", 2, "Very Good")]
    //    VeryGood = 2,
    //    [WingScopeValue("Good", 3, "Good")]
    //    Good = 3,
    //    [WingScopeValue("Fair", 4, "Fair")]
    //    Fair = 4,
    //    [WingScopeValue("Poor", 5, "Poor")]
    //    Poor = 5,
    //    [WingScopeValue("Inapplicable", 98, "Inapplicable")]
    //    Inapplicable = 98,
    //    [WingScopeValue("NoAnswerGiven", 99, "No Answer Given")]
    //    NoAnswerGiven = 99
    //}

    [WingScope("Ratings2", typeof (Ratings2Enum))]
    public enum Ratings2Enum 
    {
        [WingScopeValue("Exclude", -1, "Exclude from Dataset")] Exclude = -1,
        [WingScopeValue("WorstCarePossible", 0, "0 Worst care possible")] WorstCarePossible = 0,
        [WingScopeValue("One", 1, "1")] One = 1,
        [WingScopeValue("Two", 2, "2")] Two = 2,
        [WingScopeValue("Three", 3, "3")] Three = 3,
        [WingScopeValue("4", 4, "4")] Four = 4,
        [WingScopeValue("Four", 5, "5")] Five = 5,
        [WingScopeValue("Five", 6, "6")] Six = 6,
        [WingScopeValue("Six", 7, "7")] Seven = 7,
        [WingScopeValue("Seven", 8, "8")] Eight = 8,
        [WingScopeValue("9", 9, "9")] Nine = 9,
        [WingScopeValue("Best care possible", 10, "10 Best care possible")] BestCarePossible = 10,
        [WingScopeValue("Inapplicable", 98, "Inapplicable")] Inapplicable = 98,
        [WingScopeValue("NoAnswerGiven", 99, "No Answer Given")] NoAnswerGiven = 99,
    }

    /*
      value times
         . = 'Missing         '
         0 = 'None            '
         1 = '1 time          '
         2 = '2 times         '
         3 = '3 times         '
         4 = '4 times         '
         5 = '5 to 9 times    '
         6 = '10 or more times'
        98 = 'Inapplicable    '
        99 = 'No Answer Given '
      ;
      value ratinge
             . = 'Missing         '
             1 = 'Excellent       '
             2 = 'Very good       '
             3 = 'Good            '
             4 = 'Fair            '
             5 = 'Poor            '
            98 = 'Inapplicable    '
            99 = 'No Answer Given '
      ;
    */
}
