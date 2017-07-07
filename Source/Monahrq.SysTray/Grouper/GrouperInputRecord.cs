using Monahrq.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monahrq.SysTray.Grouper
{
    /// <summary>
    /// Class for Grouper Input
    /// </summary>
    /// <seealso cref="Monahrq.SysTray.Grouper.IGrouperInputRecord" />
    public class GrouperInputRecord : IGrouperInputRecord
    {
        /// <summary>
        /// The errors
        /// </summary>
        private IList<GrouperRecordException> _errors = new List<GrouperRecordException>();

        /// <summary>
        /// Gets the errors.
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        public IList<GrouperRecordException> Errors { get {  return _errors; } }

        #region Grouper Input File Properties

        /// <summary>
        /// The patient name
        /// </summary>
        private string _patientName;
        /// <summary>
        /// Gets or sets the name of the patient.
        /// </summary>
        /// <value>
        /// The name of the patient.
        /// </value>
        public string PatientName
        {
            // Position: 1      Length: 31      Occurences: 1
            // Alphanumeric. Left-justified, blank-filled. All blanks if no value is entered.
            get { return _patientName; }
            
            set
            {
                if (value.Length <= 31)
                {
                    _patientName = value;
                }
                else
                {
                    _patientName = string.Empty;
                    _errors.Add(new GrouperRecordException("Invalid string length for Patient Name.", "PatientName"));
                }
            }
        }

        /// <summary>
        /// The medical record number
        /// </summary>
        private string _medicalRecordNumber;
        /// <summary>
        /// Gets or sets the medical record number.
        /// </summary>
        /// <value>
        /// The medical record number.
        /// </value>
        public string MedicalRecordNumber
        {
            // Position: 32     Length: 13      Occurences: 1
            // Medical record number. Alphanumeric. Left-justified, blank-filled. All blanks if no value is entered.
            get { return _medicalRecordNumber; }

            set
            {
                if (value.Length <= 13)
                {
                    _medicalRecordNumber = value;
                }
                else
                {
                    _medicalRecordNumber = string.Empty;
                    _errors.Add(new GrouperRecordException("Invalid string length for Medical Record Number.", "MedicalRecordNumber"));
                }
            }
        }

        /// <summary>
        /// The account number
        /// </summary>
        private string _accountNumber;
        /// <summary>
        /// Gets or sets the account number.
        /// </summary>
        /// <value>
        /// The account number.
        /// </value>
        public string AccountNumber
        {
            // Position: 45     Length: 17      Occurences: 1       
            // Account number. Alphanumeric. Left-justified, blank-filled. All blanks if no value is entered.
            get { return _accountNumber; }

            set
            {
                if (value.Length <= 17)
                {
                    _accountNumber = value;
                }
                else
                {
                    _accountNumber = string.Empty;
                    _errors.Add(new GrouperRecordException("Invalid string length for Account Number.", "AccountNumber"));
                }
            }
        }

        /// <summary>
        /// The admit date
        /// </summary>
        DateTime? _admitDate;
        /// <summary>
        /// Gets or sets the admit date.
        /// </summary>
        /// <value>
        /// The admit date.
        /// </value>
        public DateTime? AdmitDate
        {
            // Position: 62     Length: 10      Occurences: 1       
            // Admit date. mm/dd/yyyy format. All blanks if no value is entered. Used in age and LOS calculations.
            get { return _admitDate; }
            set { _admitDate = value; }
        }

        /// <summary>
        /// The discharge date
        /// </summary>
        private DateTime? _dischargeDate;
        /// <summary>
        /// Gets or sets the discharge date.
        /// </summary>
        /// <value>
        /// The discharge date.
        /// </value>
        public DateTime? DischargeDate
        {
            // Position: 72     Length: 10      Occurences: 1       
            // Discharge date. mm/dd/yyyy. All blanks if no value is entered. Used in LOS calculation.
            get { return _dischargeDate; }
            set { _dischargeDate = value; }
        }

        /// <summary>
        /// The discharge status
        /// </summary>
        private int? _dischargeStatus;
        /// <summary>
        /// Gets or sets the discharge status.
        /// </summary>
        /// <value>
        /// The discharge status.
        /// </value>
        public int? DischargeStatus
        {
            // Position: 82     Length: 2       Occurences: 1
            // UB-04 discharge status. Right-justified, zero-filled. Valid values:
            // 01 = Home or self-care
            // 02 = Disch/trans to another short term hosp
            // 03 = Disch/trans to SNF
            // 04 = Disch/trans to ICF (valid until 09/30/09)
            // 04 = Custodial/supportive care (revised 10/01/09)
            // 05 = Disch/trans to another type of facility (valid until 03/31/08)
            // 05 = Canc/child hosp (revised 04/01/08)
            // 06 = Care of home health service
            // 07 = Left against medical advice
            // 08 = Home IV service (deleted 10/01/05)
            // 20 = Died
            // 21 = Disch/trans to court/law enforcement
            // 30 = Still a patient
            // 43 = Fed hospital (added 10/01/03)
            // 50 = Hospice-home
            // 51 = Hospice-medical facility
            // 61 = Swing Bed (added 10/01/2001)
            // 62 = Rehab fac/unit (added 10/01/2001)
            // 63 = LTC hospital (added 10/01/2001)
            // 64 = Nursing facility–Medicaid certified (added 10/01/02)
            // 65 = Psych hosp/unit (added 10/01/03)
            // 66 = Critical access hospital (added 10/01/05)
            // 70 = Oth institution (added 04/01/08)
            // 71 = OP services-other facility (10/01/01–09/30/03 only)
            get { return _dischargeStatus; }

            set
            {
                switch (value)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 20:
                    case 21:
                    case 30:
                    case 43:
                    case 50:
                    case 51:
                    case 61:
                    case 62:
                    case 63:
                    case 64:
                    case 65:
                    case 66:
                    case 70:
                    case 71:
                        _dischargeStatus = value;
                        break;
                    case 0:
                    case 99:
                    case null:
                        _dischargeStatus = null;
                        break;
                    default:
                        _dischargeStatus = null;
                        _errors.Add(new GrouperRecordException("Invalid value for Discharge Status.", "DischargeStatus"));
                        break;

                }
            }
        }
        /// <summary>
        /// Gets the discharge status as string.
        /// </summary>
        /// <value>
        /// The discharge status as string.
        /// </value>
        private string DischargeStatusAsString
        {
            get
            {
                if (_dischargeStatus == null)
                    return "  ";

                return String.Format("{0:00}", _dischargeStatus);
            }
        }

        /// <summary>
        /// The primary payer
        /// </summary>
        private int? _primaryPayer;
        /// <summary>
        /// Gets or sets the primary payer.
        /// </summary>
        /// <value>
        /// The primary payer.
        /// </value>
        public int? PrimaryPayer
        {
            // Position: 84     Lenght: 2       Occurences: 1
            // Primary pay source. Right-justified, zero-filled. Valid values:
            // 01 = Medicare
            // 02 = Medicaid
            // 03 = Title V
            // 04 = Other Govt
            // 05 = Work Comp
            // 06 = Blue Cross
            // 07 = Insur Co
            // 08 = Self Pay
            // 09 = Other
            // 10 = No Charge
            get { return _primaryPayer; }

            set
            {
                switch (value)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                        _primaryPayer = value;
                        break;
                    case null:
                        _primaryPayer = null;
                        break;
                    default:
                        _primaryPayer = null;
                        _errors.Add(new GrouperRecordException("Invalid value for Primary Payer.", "PrimaryPayer"));
                        break;
                }
            }
        }
        /// <summary>
        /// Gets the primary payer as string.
        /// </summary>
        /// <value>
        /// The primary payer as string.
        /// </value>
        private string PrimaryPayerAsString
        {
            get
            {
                if (_primaryPayer == null)
                    return "00";

                return string.Format("{0:00}", _primaryPayer);
            }
        }

        /// <summary>
        /// The los
        /// </summary>
        private int? _los;
        /// <summary>
        /// Gets or sets the los.
        /// </summary>
        /// <value>
        /// The los.
        /// </value>
        public int? LOS
        {
            // Position: 86     Length: 5       Occurences: 1
            // Length of stay. Right-justified, zero-filled. All blanks if no value is entered. Calculated LOS overrides entered LOS.
            get { return _los; }

            set
            {
                if ((value >= 0 && value <= 45291) || value == null)
                {
                    _los = value;
                }
                else
                {
                    _los = null;
                    _errors.Add(new GrouperRecordException("Invalid value for Length of Stay.","LOS"));
                }
            }
        }
        /// <summary>
        /// Gets the los as string.
        /// </summary>
        /// <value>
        /// The los as string.
        /// </value>
        private string LOSAsString
        {
            get
            {
                if (_los == null)
                    return "   ";
                else
                    return string.Format("{0:00000}", _los);
            }
        }

        /// <summary>
        /// The birth date
        /// </summary>
        private DateTime? _birthDate;
        /// <summary>
        /// Gets or sets the birth date.
        /// </summary>
        /// <value>
        /// The birth date.
        /// </value>
        public DateTime? BirthDate
        {
            // Position: 91     Length: 10      Occurences: 1       
            // Birth date. mm/dd/yyyy format. All blanks if no value is entered. Used in age calculation.
            get { return _birthDate; }
            set { _birthDate = value; }
        }

        /// <summary>
        /// The age
        /// </summary>
        private int? _age;
        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>
        /// The age.
        /// </value>
        public int? Age
        {
            // Position: 101     Length: 3       Occurences: 1       
            // Age. Right-justified, zero-filled. All blanks if no value is entered. Valid values: 0–124 years. Calculated age overrides entered age.
            get { return _age; }

            set
            {
                if ((value >= 0 && value <= 124) || value == null)
                {
                    _age = value;
                }
                else
                {
                    _age = null;
                    _errors.Add(new GrouperRecordException("Invalid value for Age.", "Age"));
                }
            }
        }
        /// <summary>
        /// Gets the age as string.
        /// </summary>
        /// <value>
        /// The age as string.
        /// </value>
        private string AgeAsString
        {
            get
            {
                if (_age == null)
                    return "   ";
                else
                    return String.Format("{0:000}", _age);
            }
        }

        /// <summary>
        /// The sex
        /// </summary>
        private int? _sex;
        /// <summary>
        /// Gets or sets the sex.
        /// </summary>
        /// <value>
        /// The sex.
        /// </value>
        public int? Sex
        {
            // Position: 104        Length: 1       Occurences: 1       
            // Sex. Numeric. Valid values:
            // 0 = Unknown
            // 1 = Male
            // 2 = Female
            get { return _sex; }

            set
            {
                if ((value >= 0 && value <= 2) || value == null)
                {
                    _sex = value;
                }
                else
                {
                    _sex = null;
                    _errors.Add(new GrouperRecordException("Invalid value for Sex.", "Sex"));
                }
            }
        }
        /// <summary>
        /// Gets the sex as string.
        /// </summary>
        /// <value>
        /// The sex as string.
        /// </value>
        private string SexAsString
        {
            get
            {
                if (_sex == null)
                    return "0";
                else
                    return String.Format("{0:0}", _sex);
            }
        }

        /// <summary>
        /// The admit diagnosis
        /// </summary>
        private string _admitDiagnosis;
        /// <summary>
        /// Gets or sets the admit diagnosis.
        /// </summary>
        /// <value>
        /// The admit diagnosis.
        /// </value>
        public string AdmitDiagnosis
        {
            // Position: 105        Length: 7       Occurences: 1       
            // Admit diagnosis. Left-justified, blank-filled. ICD-9-CM diagnosis code without decimal. All blanks if no value is entered.
            // Note: Only diagnosis codes of up to five digits are currently recognized as valid. When a code longer than five digits is
            // entered it will be blank filled through the seventh position.
            get { return _admitDiagnosis; }

            set
            {
                if (value.Length <= 7)
                {
                    _admitDiagnosis = value;
                }
                else
                {
                    _admitDiagnosis = null;
                    _errors.Add(new GrouperRecordException("Invalid value for Admit Diagnosis.", "AdmitDiagnosis"));
                }
            }
        }

        /// <summary>
        /// The primary diagnosis
        /// </summary>
        private string _primaryDiagnosis;
        /// <summary>
        /// Gets or sets the primary diagnosis.
        /// </summary>
        /// <value>
        /// The primary diagnosis.
        /// </value>
        public string PrimaryDiagnosis
        {
            // Position: 112        Lenght: 8       Occurences: 1       
            // Principal diagnosis. First 7 bytes left-justified, blank filled without decimals. Eighth byte represents POA indicator. Valid values:
            // Y = present at the time of inpatient admission
            // N = not present at the time of inpatient admission
            // U = the documentation is insufficient to determine if the condition was present at the time of inpatient admission
            // W = the provider is unable to clinically determine whether the condition was present at the time of inpatient admission or not
            // 1 = Unreported/Not used - Exempt from POA reporting
            // Blank = Exempt from POA reporting
            get { return _primaryDiagnosis; }

            set
            {
                if (value.Length <= 8)
                {
                    _primaryDiagnosis = value;
                }
                else
                {
                    _primaryDiagnosis = string.Empty;
                    _errors.Add( new GrouperRecordException("Invalid value for Primary Diagnosis.","PrimaryDiagnosis"));
                }
            }
        }

        /// <summary>
        /// The secondary diagnoses
        /// </summary>
        private Dictionary<int, string> _secondaryDiagnoses;
        /// <summary>
        /// Gets the secondary diagnoses.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetSecondaryDiagnoses(int key)
        {
            if (key >= 1 && key <= 24)
            {
                return _secondaryDiagnoses[key];
            }
            else
            {
                 _errors.Add(new GrouperRecordException("Invalid key for Secondary Diagnosis.", "Secondary Diagnosis"));
                 return null;
            }
        }

        /// <summary>
        /// Sets the secondary diagnoses.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetSecondaryDiagnoses(int key, string value)
        {
            // Position: 120        Length: 8       Occurences: 24      
            // Diagnoses. First 7 bytes left-justified, blank filled. Eighth byte represents POA indicator. Up to 24 ICD-9-CM diagnosis codes 
            // without decimals. Valid values:
            // Y = present at the time of inpatient admission
            // N = not present at the time of inpatient admission
            // U = the documentation is insufficient to determine if the condition was present at the time of inpatient admission
            // W = the provider is unable to clinically determine whether the condition was present at the time of inpatient admission or not
            // 1 = Unreported/Not used - Exempt from POA reporting
            // Blank = Exempt from POA reporting
            if (key >= 1 && key <= 24)
            {
                if (value.Length <= 8)
                {
                    _secondaryDiagnoses[key] = value;
                }
                else
                {
                     _errors.Add(new GrouperRecordException("Invalid value for Secondary Diagnosis.", "Secondary Diagnosis"));
                }
            }
            else
            {
                _errors.Add(new GrouperRecordException("Invalid key for Secondary Diagnosis.", "Secondary Diagnosis"));
            }
        }

        /// <summary>
        /// The principal procedure
        /// </summary>
        private string _principalProcedure;
        /// <summary>
        /// Gets or sets the principal procedure.
        /// </summary>
        /// <value>
        /// The principal procedure.
        /// </value>
        public string PrincipalProcedure
        {
            // Position: 312        Length: 7       Occurences: 1       
            // Procedure codes. Seven left-justified characters, blank-filled.
            // Note: Only procedure codes of up to four digits are currently recognized as valid. When a code longer than five digits is entered 
            // it will be blank filled through the seventh position.
            get { return _principalProcedure; }

            set
            {
                if (value.Length <= 7)
                {
                    _principalProcedure = value;
                }
                else
                {
                    _principalProcedure = string.Empty;
                    _errors.Add(new GrouperRecordException("Invalid value for Principal Procedure.","PrincipalProcedure"));
                }
            }
        }

        // Position: 319        Length: 7       Occurs: 24
        // Procedure codes. Seven left-justified characters, blank-filled. Up to 24 ICD-9-CM procedure codes without decimal.
        // See the note in the Principal Procedure field above.
        /// <summary>
        /// The secondary procedures
        /// </summary>
        private readonly Dictionary<int, string> _secondaryProcedures;
        /// <summary>
        /// Gets the secondary procedures.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetSecondaryProcedures(int key)
        {
            if (key >= 1 && key <= 24)
            {
                return _secondaryProcedures[key];
            }
            else
            {
                _errors.Add(new GrouperRecordException("Invalid key for Secondary Procedures.", "key for Secondary Procedures"));
                return null;
            }
        }

        /// <summary>
        /// Sets the secondary procedures.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetSecondaryProcedures(int key, string value)
        {
            if (key >= 1 && key <= 24)
            {
                if (value.Length <= 7)
                {
                    _secondaryProcedures[key] = value;
                }
                else
                {
                    _errors.Add(new GrouperRecordException("Invalid value for Secondary Procedures."));
                }
            }
            else
            {
                _errors.Add( new GrouperRecordException("Invalid key for Secondary Procedures."));
            }
        }

        /// <summary>
        /// The procedure date
        /// </summary>
        private readonly Dictionary<int, DateTime?> _procedureDate;
        /// <summary>
        /// Gets the procedure date.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public DateTime? GetProcedureDate(int key)
        {
            if (key >= 1 && key <= 25)
            {
                return _procedureDate[key];
            }
            _errors.Add(new GrouperRecordException("Invalid key for Procedure Date.","key for Procedure Date"));
            return null;
        }

        /// <summary>
        /// Sets the procedure date.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetProcedureDate(int key, DateTime? value)
        {
            // Position: 487        Length: 10      Occurs: 25
            // For future use. Procedure dates. The format is mm/dd/yyyy (for future use with POA logic.) All blanks if no value is entered. 
            // Up to 25 procedure dates accepted.
            if (key >= 1 && key <= 25)
            {
                _procedureDate[key] = value;
            }
            else
            {
                 _errors.Add(new GrouperRecordException("Invalid key for Procedure Date.","key for Procedure Date"));
            }
        }

        /// <summary>
        /// The apply hac logic
        /// </summary>
        private string _applyHACLogic;
        /// <summary>
        /// Gets or sets the apply hac logic.
        /// </summary>
        /// <value>
        /// The apply hac logic.
        /// </value>
        public string ApplyHACLogic
        {
            // Position: 737        Length: 1       Occurances: 1       
            // Values X or Z to be captured for use with HAC logic. These values reflect whether a hospital requires POA reporting.
            // X = Exempt from POA indicator reporting
            // Z = Requires POA indicator reporting
            get { return _applyHACLogic; }

            set
            {
                if (value.Length <= 1)
                {
                    _applyHACLogic = value;
                }
                else
                {
                    _applyHACLogic = string.Empty;
                    _errors.Add(new GrouperRecordException("Invalid value for Apply HAC Logic.","value for Apply HAC Logic"));
                }
            }
        }

        /// <summary>
        /// The optional information
        /// </summary>
        private string _optionalInformation;
        /// <summary>
        /// Gets or sets the optional information.
        /// </summary>
        /// <value>
        /// The optional information.
        /// </value>
        public string OptionalInformation
        {
            // Position: 739        Length: 72      Occurences: 1
            // Optional field. Left-justified, blank-filled. All blanks if no value is entered.
            get { return _optionalInformation; }

            set
            {
                if (value.Length <= 72)
                {
                    _optionalInformation = value;
                }
                else
                {
                    _optionalInformation = string.Empty;
                     _errors.Add(new GrouperRecordException("Invalid value for Optional Information.","OptionalInformation"));
                }
            }
        }

        // Position: 811        Length: 25      Occurs: 1
        // Not used. Blank-filled.

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperInputRecord"/> class.
        /// </summary>
        public GrouperInputRecord()
        {
            _secondaryDiagnoses = new Dictionary<int, string>();//(24, 8);
            _secondaryProcedures = new GrouperInputDictionary_String(24, 7);
            _procedureDate = new GrouperInputDictionary_Date(25);

            ClearData();
        }

        /// <summary>
        /// Clears the data.
        /// </summary>
        private void ClearData()
        {
            // Clear the grouper input file properties.
            PatientName = string.Empty;
            MedicalRecordNumber = string.Empty;
            AccountNumber = string.Empty;
            AdmitDate = null;
            DischargeDate = null;
            DischargeStatus = null;
            PrimaryPayer = null;
            LOS = null;
            BirthDate = null;
            Age = null;
            Sex = null;
            AdmitDiagnosis = string.Empty;
            PrimaryDiagnosis = string.Empty;
            for (var i = 1; i <= 24; i++)
            {
                SetSecondaryDiagnoses(i, string.Empty);
            }
            PrincipalProcedure = string.Empty;
            for (var i = 1; i <= 24; i++)
            {
                SetSecondaryProcedures(i, string.Empty);
            }
            for (var i = 1; i <= 25; i++)
            {
                SetProcedureDate(i, null);
            }
            ApplyHACLogic = string.Empty;
            OptionalInformation = string.Empty;

            _errors = new List<GrouperRecordException>();
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid()
        {
            // TODO: Do some error checking to make sure all required fields are included.
            return  _errors == null || _errors.Count == 0;
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <returns></returns>
        public string GetErrorMessage()
        {
            return null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            // Serialize the class properties into a single line for the grouper batch input file.

            var stringOut = new StringBuilder();
            stringOut.Append(FormatString(PatientName, 31));
            stringOut.Append(FormatString(MedicalRecordNumber, 13));
            stringOut.Append(FormatString(AccountNumber, 17));
            stringOut.Append(FormatDateTime(AdmitDate));
            stringOut.Append(FormatDateTime(DischargeDate));
            stringOut.Append(DischargeStatusAsString);
            stringOut.Append(PrimaryPayerAsString);
            stringOut.Append(LOSAsString);
            stringOut.Append(FormatDateTime(BirthDate));
            stringOut.Append(AgeAsString);
            stringOut.Append(SexAsString);
            stringOut.Append(FormatString(AdmitDiagnosis, 7));
            stringOut.Append(FormatString(PrimaryDiagnosis, 8));
            for (int idx = 1; idx <= 24; idx++)
            {
                stringOut.Append(FormatString(_secondaryDiagnoses[idx], 8));
            }
            stringOut.Append(FormatString(PrincipalProcedure, 7));
            for (int idx = 1; idx <= 24; idx++)
            {
                stringOut.Append(FormatString(_secondaryProcedures[idx], 7));
            }
            for (int idx = 1; idx <= 25; idx++)
            {
                stringOut.Append(FormatDateTime(_procedureDate[idx]));
            }
            stringOut.Append(FormatString(ApplyHACLogic, 1));
            stringOut.Append(FormatString(string.Empty, 1));
            stringOut.Append(FormatString(OptionalInformation, 72));
            stringOut.Append(FormatString(string.Empty, 25));

            return stringOut.ToString();
        }

        /// <summary>
        /// Formats the date time.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        private string FormatDateTime(DateTime? date)
        {
            if (date == null || date == DateTime.MinValue)
            {
                return "          ";
            }
            var formatedDateString = string.Format("{0:MM/dd/yyyy}", date);
            return formatedDateString;
        }

        /// <summary>
        /// Formats the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        private string FormatString(string text, int length)
        {
            // NOTE: Split this way thinking the substring function is going to be a heavier time burden than the length check.
            // The length should always be under the max, but being defensive.
            //var pattern = @"{0,-" + length + @"}";

            //return string.Format(pattern, text.Length > length 
            //                ? text.Substring(0, length) 
            //                : text);

            var textToUse = text.Length > length ? text.Substring(0, length) : text;
            return textToUse.PadRight(length);
        }
    }
}
