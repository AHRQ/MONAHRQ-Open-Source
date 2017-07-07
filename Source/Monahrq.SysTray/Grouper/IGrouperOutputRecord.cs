using System;
using System.Collections.Generic;

namespace Monahrq.SysTray.Grouper
{
    /// <summary>
    /// Class for GrouperOutputRecord
    /// </summary>
    public interface IGrouperOutputRecord
    {
        string AccountNumber { get; }
        DateTime? AdmitDate { get; }
        string AdmitDiagnosis { get; }
        int? Age { get; }
        string ApplyHACLogic { get; }
        DateTime? BirthDate { get; }
        float? CostWeight { get; }
        int? DiagnosticCodeCount { get; }
        DateTime? DischargeDate { get; }
        int? DischargeStatus { get; }
        int? DRGReturnCode { get; }
        int? Final4DigitDRG { get; }
        int? FinalDRG { get; }
        int? FinalDrgCcMccUsage { get; }
        int? FinalMDC { get; }
        int? FinalMedicalSurgicalIndicator { get; }
        DateTime? GetProcedureDate(int key);
        string GetSecondaryDiagnoses(int key);
        int? GetSecondaryDiagnosesReturnFlag1(int key);
        //int? GetSecondaryDiagnosesReturnFlag2(int key);
        //int? GetSecondaryDiagnosesReturnFlag3(int key);
        //int? GetSecondaryDiagnosesReturnFlag4(int key);
        int? GetSecondaryDiagnosesHAC(int key);
        int? GetSecondaryDiagnosesHACAssigned(int key);
        string GetSecondaryProcedures(int key);
        int? GetProcedureReturnFlag1(int key);
        //int? GetProcedureReturnFlag2(int key);
        //int? GetProcedureReturnFlag3(int key);
        //int? GetProcedureReturnFlag4(int key);
        int? GetProcedureHACAssigned(int key);
        int? HACStatus { get; }
        int? Initial4DigitDRG { get; }
        int? InitialDRG { get; }
        int? InitialDrgCcMccUsage { get; }
        int? InitialMedicalSurgicalIndicator { get; }
        int? LOS { get; }
        string MedicalRecordNumber { get; }
        int? MsgMceEditReturnCode { get; }
        int? MsgMceVersionUsed { get; }
        int? NumberOfUniqueHACMet { get; }
        string OptionalInformation { get; }
        string PatientName { get; }
        string PrimaryDiagnosis { get; }
        int? PrimaryPayer { get; }
        int? PrincipalDiagnosisEditReturnFlag1 { get; }
       // int? PrincipalDiagnosisEditReturnFlag2 { get; }
        //int? PrincipalDiagnosisEditReturnFlag3 { get; }
        //int? PrincipalDiagnosisEditReturnFlag4 { get; }
        int? PrincipalDiagnosisHAC { get; }
        int? PrincipalDiagnosisHACAssigned { get; }
        string PrincipalProcedure { get; }
        int? ProcedureCodeCount { get; }
        int? Sex { get; }

        string OutputRecord { get; set; }
        bool OutputRecordValid { get; }
        void ClearData();

    }
}
