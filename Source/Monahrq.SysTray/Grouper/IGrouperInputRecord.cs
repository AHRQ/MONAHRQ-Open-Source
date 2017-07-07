using System;
using System.Collections.Generic;
using Monahrq.Infrastructure.Exceptions;

namespace Monahrq.SysTray.Grouper
{
    /// <summary>
    /// Class for GrouperInputRecord
    /// </summary>
    public interface IGrouperInputRecord
    {
        // Properties
        string AccountNumber { get; set; }
        DateTime? AdmitDate { get; set; }
        string AdmitDiagnosis { get; set; }
        int? Age { get; set; }
        string ApplyHACLogic { get; set; }
        DateTime? BirthDate { get; set; }
        DateTime? DischargeDate { get; set; }
        int? DischargeStatus { get; set; }
        int? LOS { get; set; }
        string MedicalRecordNumber { get; set; }
        string OptionalInformation { get; set; }
        string PatientName { get; set; }
        string PrimaryDiagnosis { get; set; }
        int? PrimaryPayer { get; set; }
        string PrincipalProcedure { get; set; }
        string GetSecondaryDiagnoses(int key);
        void SetSecondaryDiagnoses(int key, string value);
        string GetSecondaryProcedures(int key);
        void SetSecondaryProcedures(int key, string value);
        DateTime? GetProcedureDate(int key);
        void SetProcedureDate(int key, DateTime? value);
        int? Sex { get; set; }

        IList<GrouperRecordException> Errors  { get; }

        // Methods
        bool IsValid();
        string ToString();
    }
}
