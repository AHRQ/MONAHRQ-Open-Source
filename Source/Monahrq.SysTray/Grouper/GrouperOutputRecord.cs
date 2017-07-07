using Monahrq.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.SysTray.Grouper
{
    /// <summary>
    /// Class for Grouper output record
    /// </summary>
    /// <seealso cref="Monahrq.SysTray.Grouper.IGrouperOutputRecord" />
    public class GrouperOutputRecord : IGrouperOutputRecord
    {
        // Contains properties for input and and upload record.
        // If the uploaded line (returned from the grouper) is set and valid, the value for properties will be returned from that record.
        // Otherwise, whatever value was explicitely set in the class is returned.

        /// <summary>
        /// The temporary int
        /// </summary>
        private int _tempInt;
        /// <summary>
        /// The temporary float
        /// </summary>
        private float _tempFloat;
        /// <summary>
        /// The temporary date
        /// </summary>
        private DateTime _tempDate;

        // Upload line returned from the grouper.
        /// <summary>
        /// The output record
        /// </summary>
        private string _outputRecord;
        /// <summary>
        /// Gets or sets the output record.
        /// </summary>
        /// <value>
        /// The output record.
        /// </value>
        public string OutputRecord
        {
            get { return _outputRecord; }

            set
            {
                try
                {
                    if (value.Length == 1903)
                    {
                        _outputRecord = value;
                        _outputRecordValid = true;
                    }
                    else
                    {
                        _outputRecordValid = false;
                        //throw new GrouperRecordException("Invalid grouper output record.");
                    }
                }
                catch //(Exception ex)
                {
                    // TODO: Log exception to session log.
                    //throw new GrouperRecordException("Error assigning record returned from grouper.", ex);
                }

            }
        }

        /// <summary>
        /// The output record valid
        /// </summary>
        private bool _outputRecordValid;
        /// <summary>
        /// Gets a value indicating whether [output record valid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [output record valid]; otherwise, <c>false</c>.
        /// </value>
        public bool OutputRecordValid
        {
            get { return _outputRecordValid; }
        }



        #region Grouper Input File Properties

        /// <summary>
        /// Gets the name of the patient.
        /// </summary>
        /// <value>
        /// The name of the patient.
        /// </value>
        public string PatientName
        {
            // Position: 1      Length: 31      Occurences: 1
            // Alphanumeric. Left-justified, blank-filled. All blanks if no value is entered.
            get { return GetStringFromOutputRecord(0, 31); }
        }

        /// <summary>
        /// Gets the medical record number.
        /// </summary>
        /// <value>
        /// The medical record number.
        /// </value>
        public string MedicalRecordNumber
        {
            // Position: 32     Length: 13      Occurences: 1
            // Medical record number. Alphanumeric. Left-justified, blank-filled. All blanks if no value is entered.
            get { return GetStringFromOutputRecord(31, 13); }
        }

        /// <summary>
        /// Gets the account number.
        /// </summary>
        /// <value>
        /// The account number.
        /// </value>
        public string AccountNumber
        {
            // Position: 45     Length: 17      Occurences: 1       
            // Account number. Alphanumeric. Left-justified, blank-filled. All blanks if no value is entered.
            get { return GetStringFromOutputRecord(44, 17); }
        }

        /// <summary>
        /// Gets the admit date.
        /// </summary>
        /// <value>
        /// The admit date.
        /// </value>
        public DateTime? AdmitDate
        {
            // Position: 62     Length: 10      Occurences: 1       
            // Admit date. mm/dd/yyyy format. All blanks if no value is entered. Used in age and LOS calculations.
            get { return GetDateTimeFromOutputRecord(61); }
        }

        /// <summary>
        /// Gets the discharge date.
        /// </summary>
        /// <value>
        /// The discharge date.
        /// </value>
        public DateTime? DischargeDate
        {
            // Position: 72     Length: 10      Occurences: 1       
            // Discharge date. mm/dd/yyyy. All blanks if no value is entered. Used in LOS calculation.
            get { return GetDateTimeFromOutputRecord(71); }
        }

        /// <summary>
        /// Gets the discharge status.
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
            get { return GetIntFromOutputRecord(81, 2); }
        }

        /// <summary>
        /// Gets the primary payer.
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
            get { return GetIntFromOutputRecord(83, 2); }
        }

        /// <summary>
        /// Gets the los.
        /// </summary>
        /// <value>
        /// The los.
        /// </value>
        public int? LOS
        {
            // Position: 86     Length: 5       Occurences: 1
            // Length of stay. Right-justified, zero-filled. All blanks if no value is entered. Calculated LOS overrides entered LOS.
            get { return GetIntFromOutputRecord(85, 5); }
        }

        /// <summary>
        /// Gets the birth date.
        /// </summary>
        /// <value>
        /// The birth date.
        /// </value>
        public DateTime? BirthDate
        {
            // Position: 91     Length: 10      Occurences: 1       
            // Birth date. mm/dd/yyyy format. All blanks if no value is entered. Used in age calculation.
            get { return GetDateTimeFromOutputRecord(90); }
        }

        /// <summary>
        /// Gets the age.
        /// </summary>
        /// <value>
        /// The age.
        /// </value>
        public int? Age
        {
            // Position: 101     Length: 3       Occurences: 1       
            // Age. Right-justified, zero-filled. All blanks if no value is entered. Valid values: 0–124 years. Calculated age overrides entered age.
            get { return GetIntFromOutputRecord(100, 3); }
        }

        /// <summary>
        /// Gets the sex.
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
            get { return GetIntFromOutputRecord(103, 1); }
        }

        /// <summary>
        /// Gets the admit diagnosis.
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
            get { return GetStringFromOutputRecord(104, 7); }
        }

        /// <summary>
        /// Gets the primary diagnosis.
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
            get { return GetStringFromOutputRecord(111, 8); }
        }

        /// <summary>
        /// Gets the secondary diagnoses.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">Invalid key for Secondary Diagnoses.</exception>
        public string GetSecondaryDiagnoses(int key)
        {
            // Position: 120        Length: 8       Occurences: 24      
            // Diagnoses. First 7 bytes left-justified, blank filled. Eighth byte represents POA indicator. Up to 24 ICD-9-CM diagnosis codes 
            // without decimals. Valid values:
            // Y = present at the time of inpatient admission
            // N = not present at the time of inpatient admission
            // U = the documentation is insufficient to determine if the condition was present at the time of inpatient admission
            // W = the provider is unable to clinically determine whether the condition was present at the time of inpatient admission or not
            // 1 = Unreported/Not used - Exempt from POA reporting
            // Blank = Exempt from POA reportign
            if (key >= 1 && key <= 24)
            {
                return GetStringFromOutputRecord((120 + ((key - 1) * 8) -1),8);
            }
            else
            {
                throw new GrouperRecordException("Invalid key for Secondary Diagnoses.");
            }
        }

        /// <summary>
        /// Gets the principal procedure.
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
            get { return GetStringFromOutputRecord(311, 7); }
        }

        /// <summary>
        /// Gets the secondary procedures.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">Invalid key for Secondary Procedures.</exception>
        public string GetSecondaryProcedures(int key)
        {
            // Position: 319        Length: 7       Occurs: 24      
            // Procedure codes. Seven left-justified characters, blank-filled. Up to 24 ICD-9-CM procedure codes without decimal.
            // See the note in the Principal Procedure field above.
            if (key >= 1 && key <= 24)
            {
                return GetStringFromOutputRecord((319 + ((key - 1) * 7) - 1), 7);
            }
            else
            {
                throw new GrouperRecordException("Invalid key for Secondary Procedures.");
            }
        }

        /// <summary>
        /// Gets the procedure date.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">Invalid key for Procedure Date.</exception>
        public DateTime? GetProcedureDate(int key)
        {
            // Position: 487        Length: 10      Occurs: 25
            // For future use. Procedure dates. The format is mm/dd/yyyy (for future use with POA logic.) All blanks if no value is entered. 
            // Up to 25 procedure dates accepted.
            if (key >= 1 && key <= 25)
            {
                return GetDateTimeFromOutputRecord((487 + ((key - 1) * 10) - 1));
            }
            else
            {
                throw new GrouperRecordException("Invalid key for Procedure Date.");
            }
        }

        /// <summary>
        /// Gets the apply hac logic.
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
            get { return GetStringFromOutputRecord(736, 1); }
        }

        /// <summary>
        /// Gets the optional information.
        /// </summary>
        /// <value>
        /// The optional information.
        /// </value>
        public string OptionalInformation
        {
            // Position: 738        Length: 72      Occurences: 1
            // Optional field. Left-justified, blank-filled. All blanks if no value is entered.
            get { return GetStringFromOutputRecord(738, 72); }
        }

        // Position: 810        Length: 25      Occurs: 1
        // Not used. Blank-filled.

        #endregion


        #region Grouper Output File Properties

        /// <summary>
        /// Gets the MSG mce version used.
        /// </summary>
        /// <value>
        /// The MSG mce version used.
        /// </value>
        public int? MsgMceVersionUsed
        {
            // Position: 835        Length: 3       Occurs: 1
            // Version of the software used to process the claim. Right-justified, blank-filled. Stored without decimal point.
            // Valid values: 300, 290, 280, 270, 260, 251, 250, 240.
            get { return GetIntFromOutputRecord(836, 3); }
        }

        /// <summary>
        /// Gets the initial DRG.
        /// </summary>
        /// <value>
        /// The initial DRG.
        /// </value>
        public int? InitialDRG
        {
            // Position: 838        Length: 3       Occurs: 1
            // Initial diagnosis related group. Right-justified, zero-filled.
            get { return GetIntFromOutputRecord(839, 3); }
        }

        /// <summary>
        /// Gets the initial medical surgical indicator.
        /// </summary>
        /// <value>
        /// The initial medical surgical indicator.
        /// </value>
        public int? InitialMedicalSurgicalIndicator
        {
            // Position: 841        Length: 1       Occurs: 1
            // Initial medical/surgical indicator.
            // 0 = DRG return code was not zero
            // 1 = Medical DRG
            // 2 = Surgical DRG
            get { return GetIntFromOutputRecord(842, 1); }
        }

        /// <summary>
        /// Gets the final MDC.
        /// </summary>
        /// <value>
        /// The final MDC.
        /// </value>
        public int? FinalMDC
        {
            // Position: 843        Length: 2       Occurs: 1       
            // Major diagnostic category. Right-justified, zero-filled.
            get { return GetIntFromOutputRecord(842, 2); }
        }

        /// <summary>
        /// Gets the final DRG.
        /// </summary>
        /// <value>
        /// The final DRG.
        /// </value>
        public int? FinalDRG
        {
            // Position: 845        Length: 3       Occurs: 1       
            // Final diagnosis related group. Right-justified, zero-filled.
            get { return GetIntFromOutputRecord(844, 3); }
        }

        /// <summary>
        /// Gets the final medical surgical indicator.
        /// </summary>
        /// <value>
        /// The final medical surgical indicator.
        /// </value>
        public int? FinalMedicalSurgicalIndicator
        {
            // Position: 847        Length: 1       Occurs: 1
            // Final medical/surgical indicator.
            // 0 = DRG return code was not zero
            // 1 = Medical DRG
            // 2 = Surgical DRG
            get { return GetIntFromOutputRecord(848, 1); }
        }

        /// <summary>
        /// Gets the DRG return code.
        /// </summary>
        /// <value>
        /// The DRG return code.
        /// </value>
        public int? DRGReturnCode
        {
            // Position: 848        Length: 2       Occurs: 1       
            // Numeric. Right-justified, zero-filled. Valid values:
            // 0 = OK, DRG assigned
            // 1 = Diagnosis code cannot be used as PDX
            // 2 = Record does not meet criteria for any DRG
            // 3 = Invalid age 4 = Invalid sex
            // 5 = Invalid discharge status
            // 10 = Illogical PDX
            // 11 = Invalid PDX
            // 12 = POA logic nonexempt - HAC-POA(s) invalid or missing or 1 (batch only)
            // 13 = POA logic invalid/missing - HAC-POA(s) are N, U (batch only)
            // 14 = POA logic invalid/missing - HAC-POA(s) invalid/missing or 1 (batch only)
            // 18 = POA logic invld/mssng - multiple distinct HAC-POAs not Y,W (batch only)
            get { return GetIntFromOutputRecord(849, 2); }
        }

        /// <summary>
        /// Gets the MSG mce edit return code.
        /// </summary>
        /// <value>
        /// The MSG mce edit return code.
        /// </value>
        public int? MsgMceEditReturnCode
        {
            // Position: 850        Length: 4       Occurs: 1
            // Right-justified, zero-filled. Valid values:
            // 0000 = MCE - No errors found
            // 0001 = MCE - Pre-payment error
            // 0002 = MCE - Post-payment error
            // 0003 = MCE - Pre- and post-payment errors
            // 0004 = MCE - Invalid discharge date
            // See page 3.19 for information on which edits are classified as pre- and post-payment errors.
            get { return GetIntFromOutputRecord(851, 4); }
        }

        /// <summary>
        /// Gets the diagnostic code count.
        /// </summary>
        /// <value>
        /// The diagnostic code count.
        /// </value>
        public int? DiagnosticCodeCount
        {
            // Position: 855        Length: 2       Occurs: 1
            // Number of diagnosis codes processed. Right-justified, zero-filled. This field does not include the admit diagnosis.
            get { return GetIntFromOutputRecord(855, 2); }
        }

        /// <summary>
        /// Gets the procedure code count.
        /// </summary>
        /// <value>
        /// The procedure code count.
        /// </value>
        public int? ProcedureCodeCount
        {
            // Position: 857        Length: 2       Occurs: 1       
            // Number of procedure codes processed. Right-justified, zero-filled.
            get { return GetIntFromOutputRecord(857, 2); }
        }

        /// <summary>
        /// Gets the principal diagnosis edit return flag1.
        /// </summary>
        /// <value>
        /// The principal diagnosis edit return flag1.
        /// </value>
        public int? PrincipalDiagnosisEditReturnFlag1
        {
            // Position: 859        Lenght: 8       Occurs: 1
            // 1st Flag.
            // Two-byte flag. Right-justified, zero-filled. A maximum of four flags can be returned for each diagnosis code. Valid values:
            // 01 = Invalid diagnosis code
            // 02 = Sex conflict
            // 03 = Not applicable for principal diagnosis
            // 04 = Age conflict
            // 05 = E-code as principal diagnosis
            // 06 = Non-specific principal diagnosis (MCE versions 15.0–23.0 only)
            // 07 = Manifestation code as principal diagnosis
            // 08 = Questionable admission
            // 09 = Unacceptable principal diagnosis
            // 10 = Secondary diagnosis required
            // 11 = Not applicable for principal diagnosis
            // 12 = Diagnosis affected both initial and final DRG assignment
            // 13 = MSP alert (MCE versions 15.0–17.0 only)
            // 15 = Diagnosis affected the final DRG only
            // 16 = Diagnosis affected the initial DRG only
            // 17 = Diagnosis is a MCC for initial DRG and a Non-CC for final DRG
            // 18 = Diagnosis is a CC for initial DRG and a Non-CC for final DRG
            // 19 = Wrong Procedure Performed
            // 99 = Principal diagnosis part of HAC assignment criteria
            get { return GetIntFromOutputRecord(859, 8); }
        }

        //public int? PrincipalDiagnosisEditReturnFlag2
        //{
        //    // Position: 867       Lenght: 2       Occurs: 1
        //    // 2nd Flag.
        //    get { return GetIntFromOutputRecord(867, 2); }
        //}

        //public int? PrincipalDiagnosisEditReturnFlag3
        //{
        //    // Position: 869        Lenght: 2       Occurs: 1
        //    // 3rd Flag.
        //    get { return GetIntFromOutputRecord(869, 2); }
        //}

        //public int? PrincipalDiagnosisEditReturnFlag4
        //{
        //    // Position: 871        Lenght: 2       Occurs: 1
        //    // 4th Flag.
        //    get { return GetIntFromOutputRecord(871, 2); }
        //}

        public int? PrincipalDiagnosisHACAssigned
        {
            // Position: 867        Length: 2       Occurs: 1
            // Hospital Acquired Condition (HAC) assignment criteria #1
            // 00 = Criteria to be assigned as an HAC not met
            // 11 = Infection after bariatric surgery
            // Blank = Diagnosis was not considered by grouper
            get { return GetIntFromOutputRecord(867, 2); }
        }

        /// <summary>
        /// Gets the principal diagnosis hac assigned2.
        /// </summary>
        /// <value>
        /// The principal diagnosis hac assigned2.
        /// </value>
        public int? PrincipalDiagnosisHACAssigned2
        {
            // Position: 869        Length: 2       Occurs: 1
            // Hospital Acquired Condition (HAC) assignment criteria #2
            // 00 = Criteria to be assigned as an HAC not met
            // 11 = Infection after bariatric surgery
            // Blank = Diagnosis was not considered by grouper
            get { return GetIntFromOutputRecord(869, 2); }
        }

        /// <summary>
        /// Gets the principal diagnosis hac assigned3.
        /// </summary>
        /// <value>
        /// The principal diagnosis hac assigned3.
        /// </value>
        public int? PrincipalDiagnosisHACAssigned3
        {
            // Position: 871       Length: 2       Occurs: 1
            // Hospital Acquired Condition (HAC) assignment criteria #3
            // 00 = Criteria to be assigned as an HAC not met
            // 11 = Infection after bariatric surgery
            // Blank = Diagnosis was not considered by grouper
            get { return GetIntFromOutputRecord(871, 2); }
        }

        /// <summary>
        /// Gets the principal diagnosis hac assigne4.
        /// </summary>
        /// <value>
        /// The principal diagnosis hac assigne4.
        /// </value>
        public int? PrincipalDiagnosisHACAssigne4
        {
            // Position: 873       Length: 2       Occurs: 1
            // Hospital Acquired Condition (HAC) assignment criteria #4
            // 00 = Criteria to be assigned as an HAC not met
            // 11 = Infection after bariatric surgery
            // Blank = Diagnosis was not considered by grouper
            get { return GetIntFromOutputRecord(873, 2); }
        }

        /// <summary>
        /// Gets the principal diagnosis hac assigne5.
        /// </summary>
        /// <value>
        /// The principal diagnosis hac assigne5.
        /// </value>
        public int? PrincipalDiagnosisHACAssigne5
        {
            // Position: 875        Length: 2       Occurs: 1
            // Hospital Acquired Condition (HAC) assignment criteria #5
            // 00 = Criteria to be assigned as an HAC not met
            // 11 = Infection after bariatric surgery
            // Blank = Diagnosis was not considered by grouper
            get { return GetIntFromOutputRecord(875, 2); }
        }

        /// <summary>
        /// Gets the principal diagnosis hac.
        /// </summary>
        /// <value>
        /// The principal diagnosis hac.
        /// </value>
        public int? PrincipalDiagnosisHAC
        {
            // Position: 877        Length: 1       Occurs: 1
            // Hospital Acquired Condition (HAC)
            // 0 = HAC not applicable
            // 1 = HAC criteria met
            // 2 = HAC criteria not met
            // 3 = Dx on HAC list, but HAC not applicable due to PDX/SDX exclusion
            // 4 = HAC not applicable, hospital is excempt from POA reporting
            // Blank = Diagnosis was not considered by grouper
            get { return GetIntFromOutputRecord(877, 1); }
        }

        /// <summary>
        /// Gets the principal diagnosis ha c2.
        /// </summary>
        /// <value>
        /// The principal diagnosis ha c2.
        /// </value>
        public int? PrincipalDiagnosisHAC2
        {
            // Position: 878        Length: 1       Occurs: 1
            // Hospital Acquired Condition (HAC)
            // 0 = HAC not applicable
            // 1 = HAC criteria met
            // 2 = HAC criteria not met
            // 3 = Dx on HAC list, but HAC not applicable due to PDX/SDX exclusion
            // 4 = HAC not applicable, hospital is excempt from POA reporting
            // Blank = Diagnosis was not considered by grouper
            get { return GetIntFromOutputRecord(878, 1); }
        }

        /// <summary>
        /// Gets the principal diagnosis ha c3.
        /// </summary>
        /// <value>
        /// The principal diagnosis ha c3.
        /// </value>
        public int? PrincipalDiagnosisHAC3
        {
            // Position: 879        Length: 1       Occurs: 1
            // Hospital Acquired Condition (HAC)
            // 0 = HAC not applicable
            // 1 = HAC criteria met
            // 2 = HAC criteria not met
            // 3 = Dx on HAC list, but HAC not applicable due to PDX/SDX exclusion
            // 4 = HAC not applicable, hospital is excempt from POA reporting
            // Blank = Diagnosis was not considered by grouper
            get { return GetIntFromOutputRecord(879, 1); }
        }

        /// <summary>
        /// Gets the principal diagnosis ha c4.
        /// </summary>
        /// <value>
        /// The principal diagnosis ha c4.
        /// </value>
        public int? PrincipalDiagnosisHAC4
        {
            // Position: 880        Length: 1       Occurs: 1
            // Hospital Acquired Condition (HAC)
            // 0 = HAC not applicable
            // 1 = HAC criteria met
            // 2 = HAC criteria not met
            // 3 = Dx on HAC list, but HAC not applicable due to PDX/SDX exclusion
            // 4 = HAC not applicable, hospital is excempt from POA reporting
            // Blank = Diagnosis was not considered by grouper
            get { return GetIntFromOutputRecord(880, 1); }
        }

        /// <summary>
        /// Gets the principal diagnosis ha c5.
        /// </summary>
        /// <value>
        /// The principal diagnosis ha c5.
        /// </value>
        public int? PrincipalDiagnosisHAC5
        {
            // Position: 881        Length: 1       Occurs: 1
            // Hospital Acquired Condition (HAC)
            // 0 = HAC not applicable
            // 1 = HAC criteria met
            // 2 = HAC criteria not met
            // 3 = Dx on HAC list, but HAC not applicable due to PDX/SDX exclusion
            // 4 = HAC not applicable, hospital is excempt from POA reporting
            // Blank = Diagnosis was not considered by grouper
            get { return GetIntFromOutputRecord(881, 1); }
        }

        /// <summary>
        /// Gets the secondary diagnoses return flag1.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">Invalid key for Secondary Diagnoses Return Flag 1.</exception>
        public int? GetSecondaryDiagnosesReturnFlag1(int key)
        {
            // Position: 882        Length: 2 (10)      Occurences: 24 (5 flag groups of 2 bytes before repeating)
            // 1st flag
            // Two-byte flag. Right-justified, zero-filled. A maximum of four flags can be returned for each diagnosis code. Valid values:
            // 01 = Invalid diagnosis code
            // 02 = Sex conflict
            // 03 = Duplicate of principal diagnosis
            // 04 = Age conflict
            // 11 = Secondary diagnosis is a CC
            // 12 = Diagnosis affected both initial and final DRG assignment
            // 13 = MSP alert (discontinued 10/01/01)
            // 14 = Secondary diagnosis is an MCC
            // 15 = Diagnosis affected the final DRG only
            // 16 = Diagnosis affected the initial DRG only
            // 17 = Diagnosis is a MCC for initial DRG and a Non-CC for final DRG
            // 18 = Diagnosis is a CC for initial DRG and a Non-CC for final DRG
            // 19 = Wrong procedure performed
            // 99 = Secondary diagnosis is a HAC
            if (key >= 1 && key <= 24)
            {
                return GetIntFromOutputRecord((882 + ((key - 1) * 10) - 1),2);
            }
            throw new GrouperRecordException("Invalid key for Secondary Diagnoses Return Flag 1.");
        }

        //public int? GetSecondaryDiagnosesReturnFlag2(int key)
        //{
        //    // Position: 883        Length: 10 (10)      Occurences: 24 (5 flag groups of 2 bytes before repeating)
        //    // 2nd Flag
        //    if (key >= 1 && key <= 24)
        //    {
        //        return GetIntFromOutputRecord((883 + ((key - 1) * 10) - 1), 10);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Secondary Diagnoses Return Flag 2.");
        //    }
        //}

        //public int? GetSecondaryDiagnosesReturnFlag3(int key)
        //{
        //    // Position: 885        Length: 2 (10)      Occurences: 24 (5 flag groups of 2 bytes before repeating)
        //    // 3rd Flag
        //    if (key >= 1 && key <= 24)
        //    {
        //        return GetIntFromOutputRecord((885 + ((key - 1) * 10) - 1),2);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Secondary Diagnoses Return Flag 3.");
        //    }
        //}

        //public int? GetSecondaryDiagnosesReturnFlag4(int key)
        //{
        //    // Position: 887        Length: 2 (10)      Occurences: 24 (5 flag groups of 2 bytes before repeating)
        //    // 4th Flag
        //    if (key >= 1 && key <= 24)
        //    {
        //        return GetIntFromOutputRecord((887 + ((key - 1) * 10) - 1),2);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Secondary Diagnoses Return Flag 4.");
        //    }
        //}

        //public int? GetSecondaryDiagnosesReturnFlag5(int key)
        //{
        //    // Position: 889        Length: 2 (10)      Occurences: 24 (5 flag groups of 2 bytes before repeating)
        //    // 4th Flag
        //    if (key >= 1 && key <= 24)
        //    {
        //        return GetIntFromOutputRecord((889 + ((key - 1) * 10) - 1), 2);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Secondary Diagnoses Return Flag 5.");
        //    }
        //}


        /// <summary>
        /// Gets the secondary diagnoses hac assigned.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">Invalid key for Secondary Diagnoses HAC Assigned 1.</exception>
        public int? GetSecondaryDiagnosesHACAssigned(int key)
        {
            // Position: 1074       Length: 2 (10)      Occurences: 24 (5 flag groups of 2 bytes before repeating)
            // Hospital Acquired Condition (HAC) assigned
            // 00 = Criteria to be assigned as an HAC not met
            // 01 = Foreign object retained after surgery
            // 02 = Air embolism
            // 03 = Blood incompatibility
            // 04 = Pressure ulcers
            // 05 = Falls and trauma
            // 06 = Catheter associated UTI
            // 07 = Vascular catheter-associated infection
            // 08 = Infection after CABG
            // 09 = Manifestations of poor glycemic control
            // 10 = DVT/PE after knee or hip replacement
            // 11 = Infection after bariatric surgery
            // 12 = Infection after certain orthopedic procedures of spine, shoulder and elbow
            // 13 = Surgical Site Infection Following Cardiac Device Procedures
            // 14 = Pneumothorax w/ Venous Catheterization
            // Blank = Diagnosis was not considered by grouper
            if (key >= 1 && key <= 24)
            {
                return GetIntFromOutputRecord((1074 + ((key - 1) * 10) - 1), 2);
            }
            else
            {
                throw new GrouperRecordException("Invalid key for Secondary Diagnoses HAC Assigned 1.");
            }
        }

        //public int? GetSecondaryDiagnosesHACAssigned2(int key)
        //{
        //    // Position: 1075       Length: 8 (10)      Occurences: 24 (5 flag groups of 2 bytes before repeating)
        //    // Hospital Acquired Condition (HAC) assigned
        //    if (key >= 1 && key <= 24)
        //    {
        //        return GetIntFromOutputRecord((1075 + ((key - 1) * 10) - 1), 2);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Secondary Diagnoses HAC Assigned 2.");
        //    }
        //}

        //public int? GetSecondaryDiagnosesHACAssigned3(int key)
        //{
        //    // Position: 1077       Length: 2 (10)      Occurences: 24 (5 flag groups of 2 bytes before repeating)
        //    // Hospital Acquired Condition (HAC) assigned
        //    if (key >= 1 && key <= 24)
        //    {
        //        return GetIntFromOutputRecord((1077 + ((key - 1) * 10) - 1), 2);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Secondary Diagnoses HAC Assigned 3.");
        //    }
        //}

        //public int? GetSecondaryDiagnosesHACAssigned4(int key)
        //{
        //    // Position: 1079       Length: 2 (10)      Occurences: 24 (5 flag groups of 2 bytes before repeating)
        //    // Hospital Acquired Condition (HAC) assigned
        //    if (key >= 1 && key <= 24)
        //    {
        //        return GetIntFromOutputRecord((1079 + ((key - 1) * 10) - 1), 2);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Secondary Diagnoses HAC Assigned 4.");
        //    }
        //}

        //public int? GetSecondaryDiagnosesHACAssigned5(int key)
        //{
        //    // Position: 1081       Length: 2 (10)      Occurences: 24 (5 flag groups of 2 bytes before repeating)
        //    // Hospital Acquired Condition (HAC) assigned
        //    if (key >= 1 && key <= 24)
        //    {
        //        return GetIntFromOutputRecord((1081 + ((key - 1) * 10) - 1), 2);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Secondary Diagnoses HAC Assigned 5.");
        //    }
        //}

        /// <summary>
        /// Gets the secondary diagnoses hac.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">Invalid key for Secondary Diagnoses HAC.</exception>
        public int? GetSecondaryDiagnosesHAC(int key)
        {
            // Position: 1314       Lenght: 1 (5)       Occurences: 24 (5 flag groups of 1 byte before repeating)
            // Hospital Acquired Condition (HAC)
            // 0 = HAC not applicable
            // 1 = HAC criteria met
            // 2 = HAC criteria not met
            // Blank = Diagnosis was not considered by grouper
            if (key >= 1 && key <= 24)
            {
                return GetIntFromOutputRecord((1314 + ((key - 1) * 5) - 1), 1);
            }
            else
            {
                throw new GrouperRecordException("Invalid key for Secondary Diagnoses HAC.");
            }
        }

        //public int? GetSecondaryDiagnosesHAC2(int key)
        //{
        //    // Position: 1314       Lenght: 1 (5)       Occurences: 24 (5 flag groups of 1 byte before repeating)
        //    // Hospital Acquired Condition (HAC) 2
        //    if (key >= 1 && key <= 24)
        //    {
        //        return GetIntFromOutputRecord((1314 + ((key - 1) * 5) - 1), 1);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Secondary Diagnoses HAC 2.");
        //    }
        //}

        //public int? GetSecondaryDiagnosesHAC3(int key)
        //{
        //    // Position: 1315       Lenght: 1 (5)       Occurences: 24 (5 flag groups of 1 byte before repeating)
        //    // Hospital Acquired Condition (HAC) 3
        //    if (key >= 1 && key <= 24)
        //    {
        //        return GetIntFromOutputRecord((1315 + ((key - 1) * 5) - 1), 1);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Secondary Diagnoses HAC 3.");
        //    }
        //}

        //public int? GetSecondaryDiagnosesHAC4(int key)
        //{
        //    // Position: 1316       Lenght: 1 (5)       Occurences: 24 (5 flag groups of 1 byte before repeating)
        //    // Hospital Acquired Condition (HAC) 4
        //    if (key >= 1 && key <= 24)
        //    {
        //        return GetIntFromOutputRecord((1316 + ((key - 1) * 5) - 1), 1);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Secondary Diagnoses HAC 4.");
        //    }
        //}

        //public int? GetSecondaryDiagnosesHAC5(int key)
        //{
        //    // Position: 1317       Lenght: 1 (5)       Occurences: 24 (5 flag groups of 1 byte before repeating)
        //    // Hospital Acquired Condition (HAC) 5
        //    if (key >= 1 && key <= 24)
        //    {
        //        return GetIntFromOutputRecord((1317 + ((key - 1) * 5) - 1), 1);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Secondary Diagnoses HAC 5.");
        //    }
        //}

        /// <summary>
        /// Gets the procedure return flag1.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">Invalid key for Procedure Return Flag 1.</exception>
        public int? GetProcedureReturnFlag1(int key)
        {
            // Position: 1434       Length: 2 (8)       Occurences: 25 (4 flag groups of 2 bytes before repeating)
            //  Two-byte flag. Right-justified, zero-filled. A maximum of four flags can be returned for each procedure code. Valid values:
            //  01 = Invalid procedure code
            //  02 = Sex conflict
            //  12* = Procedure affected both initial and final DRG assignment
            //  15* = Procedure affected the final DRG assignment only
            //  16* = Procedure affected the initial DRG assignment only
            //  20 = Procedure is an OR procedure
            //  21 = Non-specific OR procedure (MCE versions 15.0 - 23.0 only)
            //  22 = Open biopsy check (MCE versions 2.0 - 27.0 only)
            //  23 = Non-covered procedure
            //  24 = Bilateral procedure
            //  30 = Lung volume reduction surgery (LVRS) - limited coverage
            //  31 = Lung transplant - limited coverage
            //  32 = Combo heart/lung transplant - limited coverage
            //  33 = Heart transplant - limited coverage
            //  34 = Implantable hrt assist - limited coverage
            //  35 = Intest/multi-visceral transplant - limited coverage
            //  36 = Liver transplant - limited coverage
            //  37 = Kidney transplant - limited coverage
            //  38 = Pancreas transplant - limited coverage
            //  39 = Artificial Heart Transplant-Limit Coverage
            //  40 = Procedure inconsistent with LOS
            //  99 = Procedure part of HAC assignment criteria
            //  * When there are two or more procedures on the record that could impact either the initial, final or both DRG assignments:
            //    * If one of these procedures is in the first procedure position, that procedure will be flagged as 12,15 or 16 as appropriate in the “Procedure edit return” field with the following exceptions:
            //      a. If a single procedure designating a complete system is tied with a combination pair that also designated a complete system, the single procedure will be flagged regardless of position.
            //      b. If multiple combinations of lead/device pairs are tied then only one pair will be flagged regardless of position.
            //      c. If the two procedures tied are an OR and non-OR, the OR will be flagged regardless of position.
            //    * If none of the tied procedures is in the first procedure position, then the procedure with the lowest ascii/index value will be flagged.
            if (key >= 1 && key <= 25)
            {
                return GetIntFromOutputRecord((1434 + ((key - 1) * 8) - 1),2);
            }
            throw new GrouperRecordException("Invalid key for Procedure Return Flag 1.");
        }

        //public int? GetProcedureReturnFlag2(int key)
        //{
        //    // Position: 1435       Length: 2 (8)       Occurences: 25 (4 flag groups of 2 bytes before repeating)
        //    if (key >= 1 && key <= 25)
        //    {
        //        return GetIntFromOutputRecord((1435 + ((key - 1) * 8) - 1),2);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Procedure Return Flag 2.");
        //    }
        //}

        //public int? GetProcedureReturnFlag3(int key)
        //{
        //    // Position: 1437       Length: 2 (8)       Occurences: 25 (4 flag groups of 2 bytes before repeating)
        //    if (key >= 1 && key <= 25)
        //    {
        //        return GetIntFromOutputRecord((1437 + ((key - 1) * 8) - 1),2);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Procedure Return Flag 3.");
        //    }
        //}

        //public int? GetProcedureReturnFlag4(int key)
        //{
        //    // Position: 1439       Length: 2 (8)       Occurences: 25 (4 flag groups of 2 bytes before repeating)
        //    if (key >= 1 && key <= 25)
        //    {
        //        return GetIntFromOutputRecord((1439 + ((key - 1) * 8) - 1),2);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Procedure Return Flag 4.");
        //    }
        //}

        /// <summary>
        /// Gets the procedure hac assigned.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">Invalid key for Procedure HAC Assigned.</exception>
        public int? GetProcedureHACAssigned(int key)
        {
            // Position: 1634       Lenght: 2       Occurences: 25
            // Hospital Acquired Condition (HAC) assignment criteria
            // 00 = Criteria to be assigned as an HAC not met
            // 08 = Infection after CABG
            // 10 = DVT/PE after knee or hip replacement
            // 11 = Infection after bariatric surgery
            // 12 = Infection after certain orthopedic procedures of spine, shoulder and elbow
            // 13 = Surgical Site Infection Following Cardiac Device Procedures
            // 14 = Latrogenic Pneumothorax w/ Venous Catheterization
            // Blank = Procedure not considered by grouper
            if (key >= 1 && key <= 25)
            {
                return GetIntFromOutputRecord((1634 + ((key - 1) * 10) - 1), 2);
            }
            throw new GrouperRecordException("Invalid key for Procedure HAC Assigned.");
        }

        //public int? GetProcedureHACAssigned2(int key)
        //{
        //    // Position: 1635       Lenght: 2       Occurences: 25
        //    // Hospital Acquired Condition (HAC) assignment criteria 2
        //    if (key >= 1 && key <= 25)
        //    {
        //        return GetIntFromOutputRecord((1635 + ((key - 1) * 10) - 1), 2);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Procedure HAC Assigned 2.");
        //    }
        //}

        //public int? GetProcedureHACAssigned3(int key)
        //{
        //    // Position: 1637       Lenght: 2       Occurences: 25
        //    // Hospital Acquired Condition (HAC) assignment criteria 2
        //    if (key >= 1 && key <= 25)
        //    {
        //        return GetIntFromOutputRecord((1637 + ((key - 1) * 10) - 1), 2);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Procedure HAC Assigned 3.");
        //    }
        //}

        //public int? GetProcedureHACAssigned4(int key)
        //{
        //    // Position: 1639       Lenght: 2       Occurences: 25
        //    // Hospital Acquired Condition (HAC) assignment criteria 2
        //    if (key >= 1 && key <= 25)
        //    {
        //        return GetIntFromOutputRecord((1639 + ((key - 1) * 10) - 1), 2);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Procedure HAC Assigned 4.");
        //    }
        //}

        //public int? GetProcedureHACAssigned5(int key)
        //{
        //    // Position: 1641       Lenght: 2       Occurences: 25
        //    // Hospital Acquired Condition (HAC) assignment criteria 2
        //    if (key >= 1 && key <= 25)
        //    {
        //        return GetIntFromOutputRecord((1641 + ((key - 1) * 10) - 1), 2);
        //    }
        //    else
        //    {
        //        throw new GrouperRecordException("Invalid key for Procedure HAC Assigned 5.");
        //    }
        //}

        /// <summary>
        /// Gets the initial4 digit DRG.
        /// </summary>
        /// <value>
        /// The initial4 digit DRG.
        /// </value>
        public int? Initial4DigitDRG
        {
            // Position: 1884       Length: 4       Occurences: 1
            // Initial 4-digit DRG. Right-justified, zero-filled.
            get { return GetIntFromOutputRecord(1884, 4); }
        }

        /// <summary>
        /// Gets the final4 digit DRG.
        /// </summary>
        /// <value>
        /// The final4 digit DRG.
        /// </value>
        public int? Final4DigitDRG
        {
            // Position: 1887       Length: 4       Occurences: 1
            // Final 4-digit DRG. Right-justified, zero-filled.
            get { return GetIntFromOutputRecord(1888, 4); }
        }

        /// <summary>
        /// Gets the final DRG cc MCC usage.
        /// </summary>
        /// <value>
        /// The final DRG cc MCC usage.
        /// </value>
        public int? FinalDrgCcMccUsage
        {
            // Position: 1892       Length: 1       Occurences: 1
            // 0 = DRG assigned is not based on the presence of CC or MCC
            // 1 = DRG assigned is based on presence of MCC
            // 2 = DRG assigned is based on presence of CC.
            get { return GetIntFromOutputRecord(1892, 1); }
        }

        /// <summary>
        /// Gets the initial DRG cc MCC usage.
        /// </summary>
        /// <value>
        /// The initial DRG cc MCC usage.
        /// </value>
        public int? InitialDrgCcMccUsage
        {
            // Position: 1893       Length: 1       Occurences: 1
            // 0 = DRG assigned is not based on the presence of a CC or MCC
            // 1 = DRG assigned is based on presence of MCC
            // 2 = DRG assigned is based on presence of CC
            get { return GetIntFromOutputRecord(1893, 1); }
        }

        /// <summary>
        /// Gets the number of unique hac met.
        /// </summary>
        /// <value>
        /// The number of unique hac met.
        /// </value>
        public int? NumberOfUniqueHACMet
        {
            // Position: 1894       Length: 2       Occurences: 1
            // The number of Unique Hospital Acquired Conditions that have been met.
            get { return GetIntFromOutputRecord(1894, 2); }
        }

        /// <summary>
        /// Gets the hac status.
        /// </summary>
        /// <value>
        /// The hac status.
        /// </value>
        public int? HACStatus
        {
            // Position: 18965       Length: 1       Occurences: 1
            // HAC Status
            // 0 – HAC Status: Not Applicable
            // 1 – HAC Status: One or more HAC criteria met; Final DRG does not change
            // 2 – HAC Status: One or more HAC criteria met; Final DRG changes
            // 3 – HAC Status: One or more HAC criteria met; Final DRG changes to ungroupable
            get { return GetIntFromOutputRecord(1896, 1); }
        }

        /// <summary>
        /// Gets the cost weight.
        /// </summary>
        /// <value>
        /// The cost weight.
        /// </value>
        public float? CostWeight
        {
            // Position: 1897       Length: 7       Occurences: 1
            // The DRG cost weight. This 7-byte field is displayed as 2 digits, followed by a decimal point, followed by 4 digits.
            get { return GetFloatFromOutputRecord(1897, 7); }
        }

        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="GrouperOutputRecord"/> class.
        /// </summary>
        public GrouperOutputRecord()
        {
            ClearData();
        }

        /// <summary>
        /// Clears the data.
        /// </summary>
        public void ClearData()
        {
            _outputRecord = "";
            _outputRecordValid = false;
        }

        /// <summary>
        /// Gets the string from output record.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">Invalid grouper output record.</exception>
        private string GetStringFromOutputRecord(int start, int length)
        {
            if (OutputRecordValid)
            {
                return _outputRecord.Substring(start, length).Trim();
            }
            throw new GrouperRecordException("Invalid grouper output record.");
        }

        /// <summary>
        /// Gets the int from output record.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">Invalid grouper output record.</exception>
        private int? GetIntFromOutputRecord(int start, int length)
        {
            if (OutputRecordValid)
            {
                if (Int32.TryParse(_outputRecord.Substring(start, length), out _tempInt))
                {
                    return _tempInt;
                }
                // There was no record, which is not necessarily an error.
                return null;
            }
            throw new GrouperRecordException("Invalid grouper output record.");
        }

        /// <summary>
        /// Gets the date time from output record.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">Invalid grouper output record.</exception>
        private DateTime? GetDateTimeFromOutputRecord(int start)
        {
            if (OutputRecordValid)
            {
                if (DateTime.TryParse(_outputRecord.Substring(start, 10), out _tempDate))
                {
                    return _tempDate;
                }
                return null;
            }
            throw new GrouperRecordException("Invalid grouper output record.");
        }

        /// <summary>
        /// Gets the float from output record.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Infrastructure.Exceptions.GrouperRecordException">Invalid grouper output record.</exception>
        private float? GetFloatFromOutputRecord(int start, int length)
        {
            if (OutputRecordValid)
            {
                if (float.TryParse(_outputRecord.Substring(start, length), out _tempFloat))
                {
                    return _tempFloat;
                }
                return null;
            }
            throw new GrouperRecordException("Invalid grouper output record.");
        }
    }
}
