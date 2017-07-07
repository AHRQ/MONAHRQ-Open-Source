using System.Xml.Linq;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Modules.Wings;
using System.Linq;
using NHibernate.Linq;

namespace Monahrq.Wing.Discharge.Inpatient
{
	/// <summary>
	///  Supplies dataset hints for mapping of dischrage Inpaticent data.
	/// </summary>
	/// <seealso cref="Monahrq.Wing.Discharge.DischargeTargetedModule{Monahrq.Wing.Discharge.Inpatient.InpatientTarget}" />
	[WingModule(typeof(InpatientModule),
      Constants.WingGuid,
      "Inpatient Data",
      "Provides Services for Inpatient Statistics", DisplayOrder = 0)]
    public class InpatientModule : DischargeTargetedModule<InpatientTarget>
    {
		/// <summary>
		/// Called when [apply dataset hints].
		/// </summary>
		protected override void OnApplyDatasetHints()
        {
            Target<InpatientTarget>(target => target.AdmissionDate)
            .ApplyMappingHints("ADMIT_DATE", "ADMIND", "ADATE", "ADMTDATE");

            Target<InpatientTarget>(target => target.AdmissionSource)
            .ApplyMappingHints("ASOURCE", "ADMISSIONSOURCE", "ADMISSION_SOURCE");

            Target<InpatientTarget>(target => target.AdmissionType)
            .ApplyMappingHints("ATYPE", "ADMISSIONTYPE", "ADMISSION_TYPE", "ADTYPE", "ADMISTYPE");

            Target<InpatientTarget>(target => target.Age)
            .ApplyMappingHints("AGE");

            Target<InpatientTarget>(target => target.AgeInDays)
            .ApplyMappingHints("AGEDAY");

            Target<InpatientTarget>(target => target.BirthDate)
            .ApplyMappingHints("BIRTH_DATE", "DOB");

            Target<InpatientTarget>(target => target.BirthWeightGrams)
            .ApplyMappingHints("BIRTHWEIGHT", "BIRTHWT", "BWT");

            Target<InpatientTarget>(target => target.CustomStratifier1)
            .ApplyMappingHints("CUSTOM1");

            Target<InpatientTarget>(target => target.CustomStratifier2)
            .ApplyMappingHints("CUSTOM2");

            Target<InpatientTarget>(target => target.CustomStratifier3)
            .ApplyMappingHints("CUSTOM3");

            Target<InpatientTarget>(target => target.DaysOnMechVentilator)
            .ApplyMappingHints("DMV");

            #region unsed for this version - May come back in future versions
            //Target<InpatientTarget>(target => target.DaysToProcedure1)
            //.ApplyMappingHints("PRDAY1");

            //Target<InpatientTarget>(target => target.DaysToProcedure10)
            //.ApplyMappingHints("PRDAY10");

            //Target<InpatientTarget>(target => target.DaysToProcedure11)
            //.ApplyMappingHints("PRDAY11");

            //Target<InpatientTarget>(target => target.DaysToProcedure12)
            //.ApplyMappingHints("PRDAY12");

            //Target<InpatientTarget>(target => target.DaysToProcedure13)
            //.ApplyMappingHints("PRDAY13");

            //Target<InpatientTarget>(target => target.DaysToProcedure14)
            //.ApplyMappingHints("PRDAY14");

            //Target<InpatientTarget>(target => target.DaysToProcedure15)
            //.ApplyMappingHints("PRDAY15");

            //Target<InpatientTarget>(target => target.DaysToProcedure16)
            //.ApplyMappingHints("PRDAY16");

            //Target<InpatientTarget>(target => target.DaysToProcedure17)
            //.ApplyMappingHints("PRDAY17");

            //Target<InpatientTarget>(target => target.DaysToProcedure18)
            //.ApplyMappingHints("PRDAY18");

            //Target<InpatientTarget>(target => target.DaysToProcedure19)
            //.ApplyMappingHints("PRDAY19");

            //Target<InpatientTarget>(target => target.DaysToProcedure2)
            //.ApplyMappingHints("PRDAY2");

            //Target<InpatientTarget>(target => target.DaysToProcedure20)
            //.ApplyMappingHints("PRDAY20");

            //Target<InpatientTarget>(target => target.DaysToProcedure21)
            //.ApplyMappingHints("PRDAY21");

            //Target<InpatientTarget>(target => target.DaysToProcedure22)
            //.ApplyMappingHints("PRDAY22");

            //Target<InpatientTarget>(target => target.DaysToProcedure23)
            //.ApplyMappingHints("PRDAY23");

            //Target<InpatientTarget>(target => target.DaysToProcedure24)
            //.ApplyMappingHints("PRDAY24");

            //Target<InpatientTarget>(target => target.DaysToProcedure25)
            //.ApplyMappingHints("PRDAY25");

            //Target<InpatientTarget>(target => target.DaysToProcedure26)
            //.ApplyMappingHints("PRDAY26");

            //Target<InpatientTarget>(target => target.DaysToProcedure27)
            //.ApplyMappingHints("PRDAY27");

            //Target<InpatientTarget>(target => target.DaysToProcedure28)
            //.ApplyMappingHints("PRDAY28");

            //Target<InpatientTarget>(target => target.DaysToProcedure29)
            //.ApplyMappingHints("PRDAY29");

            //Target<InpatientTarget>(target => target.DaysToProcedure3)
            //.ApplyMappingHints("PRDAY3");

            //Target<InpatientTarget>(target => target.DaysToProcedure30)
            //.ApplyMappingHints("PRDAY30");

            //Target<InpatientTarget>(target => target.DaysToProcedure4)
            //.ApplyMappingHints("PRDAY4");

            //Target<InpatientTarget>(target => target.DaysToProcedure5)
            //.ApplyMappingHints("PRDAY5");

            //Target<InpatientTarget>(target => target.DaysToProcedure6)
            //.ApplyMappingHints("PRDAY6");

            //Target<InpatientTarget>(target => target.DaysToProcedure7)
            //.ApplyMappingHints("PRDAY7");

            //Target<InpatientTarget>(target => target.DaysToProcedure8)
            //.ApplyMappingHints("PRDAY8");

            //Target<InpatientTarget>(target => target.DaysToProcedure9)
            //.ApplyMappingHints("PRDAY9");
            #endregion

            Target<InpatientTarget>(target => target.DiagnosisCode10)
            .ApplyMappingHints("DX10");

            Target<InpatientTarget>(target => target.DiagnosisCode11)
            .ApplyMappingHints("DX11");

            Target<InpatientTarget>(target => target.DiagnosisCode12)
            .ApplyMappingHints("DX12");

            Target<InpatientTarget>(target => target.DiagnosisCode13)
            .ApplyMappingHints("DX13");

            Target<InpatientTarget>(target => target.DiagnosisCode14)
            .ApplyMappingHints("DX14");

            Target<InpatientTarget>(target => target.DiagnosisCode15)
            .ApplyMappingHints("DX15");

            Target<InpatientTarget>(target => target.DiagnosisCode16)
            .ApplyMappingHints("DX16");

            Target<InpatientTarget>(target => target.DiagnosisCode17)
            .ApplyMappingHints("DX17");

            Target<InpatientTarget>(target => target.DiagnosisCode18)
            .ApplyMappingHints("DX18");

            Target<InpatientTarget>(target => target.DiagnosisCode19)
            .ApplyMappingHints("DX19");

            Target<InpatientTarget>(target => target.DiagnosisCode2)
            .ApplyMappingHints("DX2");

            Target<InpatientTarget>(target => target.DiagnosisCode20)
            .ApplyMappingHints("DX20");

            Target<InpatientTarget>(target => target.DiagnosisCode21)
            .ApplyMappingHints("DX21");

            Target<InpatientTarget>(target => target.DiagnosisCode22)
            .ApplyMappingHints("DX22");

            Target<InpatientTarget>(target => target.DiagnosisCode23)
            .ApplyMappingHints("DX23");

            Target<InpatientTarget>(target => target.DiagnosisCode24)
            .ApplyMappingHints("DX24");

            #region unsed for this version - May come back in future versions
            //Target<InpatientTarget>(target => target.DiagnosisCode25)
            //.ApplyMappingHints("DX25");

            //Target<InpatientTarget>(target => target.DiagnosisCode26)
            //.ApplyMappingHints("DX26");

            //Target<InpatientTarget>(target => target.DiagnosisCode27)
            //.ApplyMappingHints("DX27");

            //Target<InpatientTarget>(target => target.DiagnosisCode28)
            //.ApplyMappingHints("DX28");

            //Target<InpatientTarget>(target => target.DiagnosisCode29)
            //.ApplyMappingHints("DX29");
            #endregion

            Target<InpatientTarget>(target => target.DiagnosisCode3)
            .ApplyMappingHints("DX3");

            #region unsed for this version - May come back in future versions
            //Target<InpatientTarget>(target => target.DiagnosisCode30)
            //.ApplyMappingHints("DX30");

            //Target<InpatientTarget>(target => target.DiagnosisCode31)
            //.ApplyMappingHints("DX31", "ECODE1");

            //Target<InpatientTarget>(target => target.DiagnosisCode32)
            //.ApplyMappingHints("ECODE2", "DX32");

            //Target<InpatientTarget>(target => target.DiagnosisCode33)
            //.ApplyMappingHints("DX33", "ECODE3");

            //Target<InpatientTarget>(target => target.DiagnosisCode34)
            //.ApplyMappingHints("ECODE4", "DX34");

            //Target<InpatientTarget>(target => target.DiagnosisCode35)
            //.ApplyMappingHints("DX35", "ECODE5");
            #endregion

            Target<InpatientTarget>(target => target.DiagnosisCode4)
            .ApplyMappingHints("DX4");

            Target<InpatientTarget>(target => target.DiagnosisCode5)
            .ApplyMappingHints("DX5");

            Target<InpatientTarget>(target => target.DiagnosisCode6)
            .ApplyMappingHints("DX6");

            Target<InpatientTarget>(target => target.DiagnosisCode7)
            .ApplyMappingHints("DX7");

            Target<InpatientTarget>(target => target.DiagnosisCode8)
            .ApplyMappingHints("DX8");

            Target<InpatientTarget>(target => target.DiagnosisCode9)
            .ApplyMappingHints("DX9");

            Target<InpatientTarget>(target => target.DRG)
            .ApplyMappingHints("DRG");

            Target<InpatientTarget>(target => target.DischargeDate)
            .ApplyMappingHints("DISCHARGE_DATE", "DISCD", "DISCDATE", "DISCHARGEDATE", "DISCHARGED", "DDATE");

            Target<InpatientTarget>(target => target.DischargeDisposition)
            .ApplyMappingHints("DISP", "DISPUNIFORM", "DISPUB04");

            Target<InpatientTarget>(target => target.DischargeQuarter)
            .ApplyMappingHints("DQTR", "DISCHARGEQUARTER", "QUARTER");

            Target<InpatientTarget>(target => target.DischargeYear)
            .ApplyMappingHints("YEAR", "DYEAR", "DYR");

            #region unsed for this version - May come back in future versions
            //Target<InpatientTarget>(target => target.DRG)
            //.ApplyMappingHints("IMPORT_CMS_DRG");

            //Target<InpatientTarget>(target => target.DRGVersion)
            //.ApplyMappingHints("DRG_VER");

            //Target<InpatientTarget>(target => target.ICDVER)
            //.ApplyMappingHints("ICDVER");

            //Target<InpatientTarget>(target => target.MDC)
            //.ApplyMappingHints("IMPORT_CMS_MDC");

            //Target<InpatientTarget>(target => target.DRG)
            //.ApplyMappingHints("MS_DRG");
            #endregion

            Target<InpatientTarget>(target => target.LocalHospitalID)
            .ApplyMappingHints("HOSPID", "DHOSP", "LOCALHOSP", "HOSP", "LOCALHOSPITALID", "DSHOSPID", "HOSPITALID", "LOCAL_HOSPITAL_ID", "LOCAL_HOSPITALID", "LOCALHOSPITAL_ID", "HOSPITAL_ID", "LHID", "HID");

            Target<InpatientTarget>(target => target.Key)
            .ApplyMappingHints("KEY");

            Target<InpatientTarget>(target => target.LengthOfStay)
            .ApplyMappingHints("LOS");

            Target<InpatientTarget>(target => target.MDC)
                .ApplyMappingHints("MDC");

            Target<InpatientTarget>(target => target.PatientID)
            .ApplyMappingHints("PATIENT_ID", "ID", "PAT_ID", "PATIENTID", "PATID", "PID");

            Target<InpatientTarget>(target => target.PatientStateCountyCode)
            .ApplyMappingHints("HOSPITAL STATE/COUNTY CODE", "HOSPSTCO", "PSTCO", @"HOSPSTCO/PSTCO", "COUNTY", "PSTCO2", "PATIENTSTATECOUNTYCODE");

            Target<InpatientTarget>(target => target.PointOfOrigin)
            .ApplyMappingHints("POINTOFORIGIN04", "POINTOFORIGINUB04", "POINTOFORIGIN", "POO", "PORIGIN");

            #region unsed for this version - May come back in future versions
            //Target<InpatientTarget>(target => target.PresentOnAdmission1)
            //.ApplyMappingHints("POA 1", "DXATADMIT1");

            //Target<InpatientTarget>(target => target.PresentOnAdmission10)
            //.ApplyMappingHints("DXATADMIT10", "POA 10");

            //Target<InpatientTarget>(target => target.PresentOnAdmission11)
            //.ApplyMappingHints("POA 11", "DXATADMIT11");

            //Target<InpatientTarget>(target => target.PresentOnAdmission12)
            //.ApplyMappingHints("DXATADMIT12", "POA 12");

            //Target<InpatientTarget>(target => target.PresentOnAdmission13)
            //.ApplyMappingHints("POA 13", "DXATADMIT13");

            //Target<InpatientTarget>(target => target.PresentOnAdmission14)
            //.ApplyMappingHints("DXATADMIT14", "POA 14");

            //Target<InpatientTarget>(target => target.PresentOnAdmission15)
            //.ApplyMappingHints("POA 15", "DXATADMIT15");

            //Target<InpatientTarget>(target => target.PresentOnAdmission16)
            //.ApplyMappingHints("DXATADMIT16", "POA 16");

            //Target<InpatientTarget>(target => target.PresentOnAdmission17)
            //.ApplyMappingHints("POA 17", "DXATADMIT17");

            //Target<InpatientTarget>(target => target.PresentOnAdmission18)
            //.ApplyMappingHints("DXATADMIT18", "POA 18");

            //Target<InpatientTarget>(target => target.PresentOnAdmission19)
            //.ApplyMappingHints("POA 19", "DXATADMIT19");

            //Target<InpatientTarget>(target => target.PresentOnAdmission2)
            //.ApplyMappingHints("DXATADMIT2", "POA 2");

            //Target<InpatientTarget>(target => target.PresentOnAdmission20)
            //.ApplyMappingHints("POA 20", "DXATADMIT20");

            //Target<InpatientTarget>(target => target.PresentOnAdmission21)
            //.ApplyMappingHints("DXATADMIT21", "POA 21");

            //Target<InpatientTarget>(target => target.PresentOnAdmission22)
            //.ApplyMappingHints("POA 22", "DXATADMIT22");

            //Target<InpatientTarget>(target => target.PresentOnAdmission23)
            //.ApplyMappingHints("DXATADMIT23", "POA 23");

            //Target<InpatientTarget>(target => target.PresentOnAdmission24)
            //.ApplyMappingHints("POA 24", "DXATADMIT24");

            //Target<InpatientTarget>(target => target.PresentOnAdmission25)
            //.ApplyMappingHints("DXATADMIT25", "POA 25");

            //Target<InpatientTarget>(target => target.PresentOnAdmission26)
            //.ApplyMappingHints("POA 26", "DXATADMIT26");

            //Target<InpatientTarget>(target => target.PresentOnAdmission27)
            //.ApplyMappingHints("DXATADMIT27", "POA 27");

            //Target<InpatientTarget>(target => target.PresentOnAdmission28)
            //.ApplyMappingHints("POA 28", "DXATADMIT28");

            //Target<InpatientTarget>(target => target.PresentOnAdmission29)
            //.ApplyMappingHints("DXATADMIT29", "POA 29");

            //Target<InpatientTarget>(target => target.PresentOnAdmission3)
            //.ApplyMappingHints("POA 3", "DXATADMIT3");

            //Target<InpatientTarget>(target => target.PresentOnAdmission30)
            //.ApplyMappingHints("DXATADMIT30", "POA 30");

            //Target<InpatientTarget>(target => target.PresentOnAdmission31)
            //.ApplyMappingHints("POA 31", "DXATADMIT31");

            //Target<InpatientTarget>(target => target.PresentOnAdmission32)
            //.ApplyMappingHints("DXATADMIT32", "POA 32");

            //Target<InpatientTarget>(target => target.PresentOnAdmission33)
            //.ApplyMappingHints("POA 33", "DXATADMIT33");

            //Target<InpatientTarget>(target => target.PresentOnAdmission34)
            //.ApplyMappingHints("DXATADMIT34", "POA 34");

            //Target<InpatientTarget>(target => target.PresentOnAdmission35)
            //.ApplyMappingHints("POA 35", "DXATADMIT35");

            //Target<InpatientTarget>(target => target.PresentOnAdmission4)
            //.ApplyMappingHints("DXATADMIT4", "POA 4");

            //Target<InpatientTarget>(target => target.PresentOnAdmission5)
            //.ApplyMappingHints("POA 5", "DXATADMIT5");

            //Target<InpatientTarget>(target => target.PresentOnAdmission6)
            //.ApplyMappingHints("DXATADMIT6", "POA 6");

            //Target<InpatientTarget>(target => target.PresentOnAdmission7)
            //.ApplyMappingHints("POA 7", "DXATADMIT7");

            //Target<InpatientTarget>(target => target.PresentOnAdmission8)
            //.ApplyMappingHints("DXATADMIT8", "POA 8");

            //Target<InpatientTarget>(target => target.PresentOnAdmission9)
            //.ApplyMappingHints("POA 9", "DXATADMIT9");
            #endregion

            Target<InpatientTarget>(target => target.PrimaryPayer)
            .ApplyMappingHints("PAY1", "PRIMARYPAYER", "PRIMARY_PAYER", "PPAYER", "PAY");

            Target<InpatientTarget>(target => target.PrincipalDiagnosis)
            .ApplyMappingHints("DX1");

            Target<InpatientTarget>(target => target.PrincipalProcedure)
            .ApplyMappingHints("PR1");

            Target<InpatientTarget>(target => target.ProcedureCode10)
            .ApplyMappingHints("PR10");

            Target<InpatientTarget>(target => target.ProcedureCode11)
            .ApplyMappingHints("PR11");

            Target<InpatientTarget>(target => target.ProcedureCode12)
            .ApplyMappingHints("PR12");

            Target<InpatientTarget>(target => target.ProcedureCode13)
            .ApplyMappingHints("PR13");

            Target<InpatientTarget>(target => target.ProcedureCode14)
            .ApplyMappingHints("PR14");

            Target<InpatientTarget>(target => target.ProcedureCode15)
            .ApplyMappingHints("PR15");

            Target<InpatientTarget>(target => target.ProcedureCode16)
            .ApplyMappingHints("PR16");

            Target<InpatientTarget>(target => target.ProcedureCode17)
            .ApplyMappingHints("PR17");

            Target<InpatientTarget>(target => target.ProcedureCode18)
            .ApplyMappingHints("PR18");

            Target<InpatientTarget>(target => target.ProcedureCode19)
            .ApplyMappingHints("PR19");

            Target<InpatientTarget>(target => target.ProcedureCode2)
            .ApplyMappingHints("PR2");

            Target<InpatientTarget>(target => target.ProcedureCode20)
            .ApplyMappingHints("PR20");

            Target<InpatientTarget>(target => target.ProcedureCode21)
            .ApplyMappingHints("PR21");

            Target<InpatientTarget>(target => target.ProcedureCode22)
            .ApplyMappingHints("PR22");

            Target<InpatientTarget>(target => target.ProcedureCode23)
            .ApplyMappingHints("PR23");

            Target<InpatientTarget>(target => target.ProcedureCode24)
            .ApplyMappingHints("PR24");

            #region unsed for this version - May come back in future versions
            //Target<InpatientTarget>(target => target.ProcedureCode25)
            //.ApplyMappingHints("PR25");

            //Target<InpatientTarget>(target => target.ProcedureCode26)
            //.ApplyMappingHints("PR26");

            //Target<InpatientTarget>(target => target.ProcedureCode27)
            //.ApplyMappingHints("PR27");

            //Target<InpatientTarget>(target => target.ProcedureCode28)
            //.ApplyMappingHints("PR28");

            //Target<InpatientTarget>(target => target.ProcedureCode29)
            //.ApplyMappingHints("PR29");

            //Target<InpatientTarget>(target => target.ProcedureCode30)
            //.ApplyMappingHints("PR30");

            //Target<InpatientTarget>(target => target.DRG)
            //.ApplyMappingHints("RATES_DRG");
            #endregion

            Target<InpatientTarget>(target => target.ProcedureCode3)
            .ApplyMappingHints("PR3");

            Target<InpatientTarget>(target => target.ProcedureCode4)
            .ApplyMappingHints("PR4");

            Target<InpatientTarget>(target => target.ProcedureCode5)
            .ApplyMappingHints("PR5");

            Target<InpatientTarget>(target => target.ProcedureCode6)
            .ApplyMappingHints("PR6");

            Target<InpatientTarget>(target => target.ProcedureCode7)
            .ApplyMappingHints("PR7");

            Target<InpatientTarget>(target => target.ProcedureCode8)
            .ApplyMappingHints("PR8");

            Target<InpatientTarget>(target => target.ProcedureCode9)
            .ApplyMappingHints("PR9");

            Target<InpatientTarget>(target => target.Race)
            .ApplyMappingHints("RACE");

            Target<InpatientTarget>(target => target.Sex)
            .ApplyMappingHints("GENDER", "SEX", "FEMALE");

            Target<InpatientTarget>(target => target.TotalCharge)
            .ApplyMappingHints("TOTCHG", "TOTCHGS", "CHRGS", "TCHRGS", "TOTALCHARGE", "TOTAL_CHARGE", "TOTALCHARGES", "TOTAL_CHARGES");

            Target<InpatientTarget>(t => t.HRRRegionID)
            .ApplyMappingHints("HRR", "HRR_R", "HRRREGION", "HRR_REGION");

            Target<InpatientTarget>(t => t.HSARegionID)
            .ApplyMappingHints("HSA", "HSA_R", "HSAREGION", "HSA_REGION");

            Target<InpatientTarget>(t => t.CustomRegionID)
            .ApplyMappingHints("CUSTOMREGIONID", "REGION", "CUSTOM_REGION", "CUSTOM", "CUSTOMREGION");

            Target<InpatientTarget>(t => t.PatientZipCode)
            .ApplyMappingHints("ZIP", "PZIP", "PATIENTZIP", "PATIENT_ZIP");

            Target<InpatientTarget>(t => t.EDServices)
           .ApplyMappingHints("ED", "EDSVC", "ED_SERVICES", "EDSERVICES", "ESERV", "ED_IND", "EDIND", "ED_INDICATOR", "EDINDICATOR");
        }

		/// <summary>
		/// Gets the measure prefix.
		/// </summary>
		/// <value>
		/// The measure prefix.
		/// </value>
		protected override string MeasurePrefix
        {
            get { return "IP"; }
        }

        //protected override XDocument MeasuresXml
        //{
        //    get
        //    {
        //        using (var str = GetType().Assembly.GetManifestResourceStream(GetType(), "Measures-No-Topics.xml"))
        //        {
        //            return XDocument.Load(str);
        //        }
        //    }
        //}

        //protected override int CodeN
        //{
        //    get
        //    {
        //        return 8;
        //    }
        //}

        //protected override void ImportMeasures()
        //{
        //    base.ImportMeasures();
        //}
    }
}
