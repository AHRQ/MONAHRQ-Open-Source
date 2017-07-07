using Monahrq.Sdk.Attributes.Wings;
using Monahrq.Sdk.Modules.Wings;

namespace Monahrq.Wing.Discharge.TreatAndRelease
{
    [WingModule(typeof(TreatAndReleaseModule),
     Constants.EDWingGuid,
     "Treat and Release Discharge Data",
     "Provides Services for Treat and Release Discharge Statistics", DisplayOrder = 1)]
    public partial class TreatAndReleaseModule : DischargeTargetedModule<TreatAndReleaseTarget>
    {
        protected override int CodeN
        {
            get
            {
                return 1;
            }
        }

        protected override string MeasurePrefix
        {
            get { return "ED"; }
        }

        protected override void OnApplyDatasetHints()
        {
            Target<TreatAndReleaseTarget>(target => target.Age)
            .ApplyMappingHints("AGE");

            Target<TreatAndReleaseTarget>(target => target.PatientStateCountyCode)
            .ApplyMappingHints("COUNTY", "HOSPITAL STATE/COUNTY CODE", "HOSPSTCO", "PSTCO", @"HOSPSTCO/PSTCO", "PATIENTSTATECOUNTYCODE", "PSTCO2");

            Target<TreatAndReleaseTarget>(target => target.DischargeDisposition)
            .ApplyMappingHints("DISP_ED", "DISP", "DISPUNIFORM", "DISPUB04", "DISCHARGEDISPOSITION");

            Target<TreatAndReleaseTarget>(target => target.PrimaryDiagnosis)
            .ApplyMappingHints("DX1", "DXCSS1");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode10)
            .ApplyMappingHints("DX10", "DXCSS10");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode11)
            .ApplyMappingHints("DX11", "DXCSS11");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode12)
            .ApplyMappingHints("DX12", "DXCSS12");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode13)
            .ApplyMappingHints("DX13", "DXCSS13");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode14)
            .ApplyMappingHints("DX14", "DXCSS14");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode15)
            .ApplyMappingHints("DX15", "DXCSS15");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode16)
            .ApplyMappingHints("DX16", "DXCSS16");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode17)
            .ApplyMappingHints("DX17", "DXCSS17");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode18)
            .ApplyMappingHints("DX18", "DXCSS18");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode19)
            .ApplyMappingHints("DX19", "DXCSS19");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode2)
            .ApplyMappingHints("DX2", "DXCSS2");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode20)
            .ApplyMappingHints("DX20", "DXCSS20");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode3)
            .ApplyMappingHints("DX3", "DXCSS3");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode4)
            .ApplyMappingHints("DX4", "DXCSS4");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode5)
            .ApplyMappingHints("DX5", "DXCSS5");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode6)
            .ApplyMappingHints("DX6", "DXCSS6");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode7)
            .ApplyMappingHints("DX7", "DXCSS7");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode8)
            .ApplyMappingHints("DX8", "DXCSS8");

            Target<TreatAndReleaseTarget>(target => target.DiagnosisCode9)
            .ApplyMappingHints("DX9", "DXCSS9");

            Target<TreatAndReleaseTarget>(target => target.Sex)
            .ApplyMappingHints("FEMALE", "GENDER", "SEX");

            Target<TreatAndReleaseTarget>(target => target.HospitalTraumaLevel)
            .ApplyMappingHints("HOSP_TRAUMA", "HTL", "HOSPITALTRAUMALEVEL", "TRAUMALEVEL");

            Target<TreatAndReleaseTarget>(target => target.LocalHospitalID)
            .ApplyMappingHints("HOSPID", "DHOSP", "LOCALHOSP", "HOSP", "LOCALHOSPITALID", "DSHOSPID", "HOSPITALID", "LOCAL_HOSPITAL_ID", "LOCAL_HOSPITALID", "LOCALHOSPITAL_ID", "HOSPITAL_ID", "LHID", "HID");

            Target<TreatAndReleaseTarget>(target => target.Key)
            .ApplyMappingHints("KEY_ED");

            Target<TreatAndReleaseTarget>(target => target.NumberofDiagnoses)
            .ApplyMappingHints("NDX");

            Target<TreatAndReleaseTarget>(target => target.PrimaryPayer)
            .ApplyMappingHints("PAY1", "PRIMARYPAYER", "PRIMARY_PAYER", "PPAYER", "PAY");

            Target<TreatAndReleaseTarget>(target => target.Race)
            .ApplyMappingHints("RACE", "ETHNIC");

            Target<TreatAndReleaseTarget>(target => target.DischargeQuarter)
            .ApplyMappingHints("DQTR", "DISCHARGEQUARTER", "QUARTER");

            Target<TreatAndReleaseTarget>(target => target.DischargeYear)
            .ApplyMappingHints("YEAR", "DYEAR", "DISCHARGEYR", "DISCHARGEYEAR");
        }
    }
}
