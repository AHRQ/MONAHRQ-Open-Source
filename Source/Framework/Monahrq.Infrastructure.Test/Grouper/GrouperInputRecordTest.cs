using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Exceptions;
using Monahrq.Infrastructure.Grouper;
using Monahrq.TestSupport;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Test.Grouper
{
    [TestClass]
    [Ignore]
    public class GrouperInputRecordTest : MefTestFixture
    {
        GrouperInputRecord inputRecord;

        protected override void ComposeFixtureInstances()
        {
            base.ComposeFixtureInstances();
            inputRecord = new GrouperInputRecord();
        }

        [TestMethod]
        public void PatientNameAssignValidTest()
        {
            string expected = "1234567890123456789012345678901";
            inputRecord.PatientName = expected;
            string actual = inputRecord.PatientName;

            Assert.AreSame(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for string length wasn't thrown.")]
        public void PatientNameAssignInvalidLengthTest()
        {
            string longString = "12345678901234567890123456789012";
            inputRecord.PatientName = longString;
        }

        [TestMethod]
        public void MedicalRecordNumberAssignValidTest()
        {
            string expected = "1234567890123";
            inputRecord.MedicalRecordNumber = expected;
            string actual = inputRecord.MedicalRecordNumber;

            Assert.AreSame(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for string length wasn't thrown.")]
        public void MedicalRecordNumberAssignInvalidLengthTest()
        {
            string longString = "12345678901234";
            inputRecord.MedicalRecordNumber = longString;
        }

        [TestMethod]
        public void AccountNumberAssignValidTest()
        {
            string expected = "12345678901234567";
            inputRecord.AccountNumber = expected;
            string actual = inputRecord.AccountNumber;

            Assert.AreSame(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for string length wasn't thrown.")]
        public void AccountNumberAssignInvalidLengthTest()
        {
            string longString = "123456789012345678";
            inputRecord.AccountNumber = longString;
        }

        [TestMethod]
        public void DischargeStatusAssignValidTest()
        {
            int expected = 20;
            inputRecord.DischargeStatus = expected;
            int? actual = inputRecord.DischargeStatus;

            Assert.AreEqual(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for invalid number wasn't thrown.")]
        public void DischargeStatusAssignInvalidValue()
        {
            int invalidValue = 9;
            inputRecord.DischargeStatus = invalidValue;
        }

        [TestMethod]
        public void DischargeStatusAssignNullTest()
        {
            int? expected = null;
            inputRecord.DischargeStatus = expected;
            int? actual = inputRecord.DischargeStatus;

            Assert.AreEqual(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        public void PrimaryPayerAssignValidTest()
        {
            int expected = 1;
            inputRecord.PrimaryPayer = expected;
            int? actual = inputRecord.PrimaryPayer;

            Assert.AreEqual(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for invalid number wasn't thrown.")]
        public void PrimaryPayerAssignInvalidValue()
        {
            int invalidValue = 11;
            inputRecord.PrimaryPayer = invalidValue;
        }

        [TestMethod]
        public void PrimaryPayerAssignNullTest()
        {
            int? expected = null;
            inputRecord.PrimaryPayer = expected;
            int? actual = inputRecord.PrimaryPayer;

            Assert.AreEqual(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        public void LOSAssignMinTest()
        {
            int expected = 0;
            inputRecord.LOS = expected;
            int? actual = inputRecord.LOS;

            Assert.AreEqual(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for invalid number wasn't thrown.")]
        public void LOSAssignBelowMinTest()
        {
            int invalidValue = -1;
            inputRecord.LOS = invalidValue;
        }

        [TestMethod]
        public void LOSAssignMaxTest()
        {
            int expected = 999;
            inputRecord.LOS = expected;
            int? actual = inputRecord.LOS;

            Assert.AreEqual(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for invalid number wasn't thrown.")]
        public void LOSAssignAboveMaxTest()
        {
            int invalidValue = 1000;
            inputRecord.LOS = invalidValue;
        }

        [TestMethod]
        public void LOSAssignNullTest()
        {
            int? expected = null;
            inputRecord.LOS = expected;
            int? actual = inputRecord.LOS;

            Assert.AreEqual(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        public void AgeAssignMinTest()
        {
            int expected = 0;
            inputRecord.Age = expected;
            int? actual = inputRecord.Age;

            Assert.AreEqual(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for invalid number wasn't thrown.")]
        public void AgeAssignBelowMinTest()
        {
            int invalidValue = -1;
            inputRecord.Age = invalidValue;
        }

        [TestMethod]
        public void AgeAssignMaxTest()
        {
            int expected = 124;
            inputRecord.Age = expected;
            int? actual = inputRecord.Age;

            Assert.AreEqual(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for invalid number wasn't thrown.")]
        public void AgeAssignAboveMaxTest()
        {
            int invalidValue = 125;
            inputRecord.Age = invalidValue;
        }

        [TestMethod]
        public void AgeAssignNullTest()
        {
            int? expected = null;
            inputRecord.Age = expected;
            int? actual = inputRecord.Age;

            Assert.AreEqual(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        public void SexAssignMinTest()
        {
            int expected = 0;
            inputRecord.Sex = expected;
            int? actual = inputRecord.Sex;

            Assert.AreEqual(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for invalid number wasn't thrown.")]
        public void SexAssignBelowMinTest()
        {
            int invalidValue = -1;
            inputRecord.Sex = invalidValue;
        }

        [TestMethod]
        public void SexAssignMaxTest()
        {
            int expected = 2;
            inputRecord.Sex = expected;
            int? actual = inputRecord.Sex;

            Assert.AreEqual(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for invalid number wasn't thrown.")]
        public void SexAssignAboveMaxTest()
        {
            int invalidValue = 3;
            inputRecord.Sex = invalidValue;
        }

        [TestMethod]
        public void SexAssignNullTest()
        {
            int? expected = null;
            inputRecord.Sex = expected;
            int? actual = inputRecord.Sex;

            Assert.AreEqual(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        public void AdmitDiagnosisAssignValidTest()
        {
            string expected = "1234567";
            inputRecord.AdmitDiagnosis = expected;
            string actual = inputRecord.AdmitDiagnosis;

            Assert.AreSame(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for string length wasn't thrown.")]
        public void AdmitDiagnosisAssignInvalidLengthTest()
        {
            string longString = "12345678";
            inputRecord.AdmitDiagnosis = longString;
        }

        [TestMethod]
        public void PrimaryDiagnosisAssignValidTest()
        {
            string expected = "12345678";
            inputRecord.PrimaryDiagnosis = expected;
            string actual = inputRecord.PrimaryDiagnosis;

            Assert.AreSame(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for string length wasn't thrown.")]
        public void PrimaryDiagnosisAssignInvalidLengthTest()
        {
            string longString = "123456789";
            inputRecord.PrimaryDiagnosis = longString;
        }

        [TestMethod]
        public void SecondaryDiagnosisAssignValidAtMinIndexTest()
        {
            string expected = "12345678";
            inputRecord.SetSecondaryDiagnoses(1, expected);
            string actual = inputRecord.GetSecondaryDiagnoses(1);

            Assert.AreSame(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosisAssignBelowMinIndexTest()
        {
            string longString = "12345678";
            inputRecord.SetSecondaryDiagnoses(0, longString);
        }

        [TestMethod]
        public void SecondaryDiagnosisAssignValidAtMaxIndexTest()
        {
            string expected = "12345678";
            inputRecord.SetSecondaryDiagnoses(24, expected);
            string actual = inputRecord.GetSecondaryDiagnoses(24);

            Assert.AreSame(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosisAssignAboveMaxIndexLengthTest()
        {
            string longString = "12345678";
            inputRecord.SetSecondaryDiagnoses(25, longString);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for string length wasn't thrown.")]
        public void SecondaryDiagnosisAssignInvalidLengthTest()
        {
            string longString = "123456789";
            inputRecord.SetSecondaryDiagnoses(1, longString);
        }

        [TestMethod]
        public void PrincipalProcedureAssignValidTest()
        {
            string expected = "1234567";
            inputRecord.PrincipalProcedure = expected;
            string actual = inputRecord.PrincipalProcedure;

            Assert.AreSame(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for string length wasn't thrown.")]
        public void PrincipalProcedureAssignInvalidLengthTest()
        {
            string longString = "12345678";
            inputRecord.PrincipalProcedure = longString;
        }

        [TestMethod]
        public void SecondaryProceduresAssignValidAtMinIndexTest()
        {
            string expected = "1234567";
            inputRecord.SetSecondaryProcedures(1, expected);
            string actual = inputRecord.GetSecondaryProcedures(1);

            Assert.AreSame(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryProceduresAssignBelowMinIndexTest()
        {
            string longString = "1234567";
            inputRecord.SetSecondaryProcedures(0, longString);
        }

        [TestMethod]
        public void SecondaryProceduresAssignValidAtMaxIndexTest()
        {
            string expected = "1234567";
            inputRecord.SetSecondaryProcedures(24, expected);
            string actual = inputRecord.GetSecondaryProcedures(24);

            Assert.AreSame(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryProceduresAssignAboveMaxIndexLengthTest()
        {
            string longString = "1234567";
            inputRecord.SetSecondaryProcedures(25, longString);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for string length wasn't thrown.")]
        public void SecondaryProceduresAssignInvalidLengthTest()
        {
            string longString = "12345678";
            inputRecord.SetSecondaryProcedures(1, longString);
        }

        [TestMethod]
        public void ApplyHACLogicAssignValidTest()
        {
            string expected = "1";
            inputRecord.ApplyHACLogic = expected;
            string actual = inputRecord.ApplyHACLogic;

            Assert.AreSame(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for string length wasn't thrown.")]
        public void ApplyHACLogicAssignInvalidLengthTest()
        {
            string longString = "12";
            inputRecord.ApplyHACLogic = longString;
        }

        [TestMethod]
        public void OptionalInformationAssignValidTest()
        {
            string expected = "123456789012345678901234567890123456789012345678901234567890123456789012";
            inputRecord.OptionalInformation = expected;
            string actual = inputRecord.OptionalInformation;

            Assert.AreSame(expected, actual, "Actual does not match expected results.");
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for string length wasn't thrown.")]
        public void OptionalInformationAssignInvalidLengthTest()
        {
            string longString = "1234567890123456789012345678901234567890123456789012345678901234567890123";
            inputRecord.OptionalInformation = longString;
        }

        [TestMethod]
        public void InputRecordToStringTest()
        {
            StringBuilder sbExpected = new StringBuilder();
            sbExpected.Append("PatientNamePatientNamePatientNa");
            inputRecord.PatientName = "PatientNamePatientNamePatientNa";
            sbExpected.Append("MedicalRecord");
            inputRecord.MedicalRecordNumber = "MedicalRecord";
            sbExpected.Append("AccountNumberAcco");
            inputRecord.AccountNumber = "AccountNumberAcco";
            sbExpected.Append("01/02/2010");
            inputRecord.AdmitDate = new DateTime(2010, 1, 2);
            sbExpected.Append("01/03/2010");
            inputRecord.DischargeDate = new DateTime(2010, 1, 3);
            sbExpected.Append("20");
            inputRecord.DischargeStatus = 20;
            sbExpected.Append("01");
            inputRecord.PrimaryPayer = 1;
            sbExpected.Append("987");
            inputRecord.LOS = 987;
            sbExpected.Append("01/04/1971");
            inputRecord.BirthDate = new DateTime(1971, 1, 4);
            sbExpected.Append("041");
            inputRecord.Age = 41;
            sbExpected.Append("1");
            inputRecord.Sex = 1;
            sbExpected.Append("34400  ");
            inputRecord.AdmitDiagnosis = "34400";
            sbExpected.Append("486     ");
            inputRecord.PrimaryDiagnosis = "486";
            sbExpected.Append("7802    ");
            inputRecord.SetSecondaryDiagnoses(1, "7802");
            sbExpected.Append("34691   ");
            inputRecord.SetSecondaryDiagnoses(2, "34691");
            sbExpected.Append("45341   ");
            inputRecord.SetSecondaryDiagnoses(3, "45341");
            sbExpected.Append("27651   ");
            inputRecord.SetSecondaryDiagnoses(4, "27651");
            sbExpected.Append("V4582   ");
            inputRecord.SetSecondaryDiagnoses(5, "V4582");
            sbExpected.Append("78651   ");
            inputRecord.SetSecondaryDiagnoses(6, "78651");
            sbExpected.Append("1890    ");
            inputRecord.SetSecondaryDiagnoses(7, "1890");
            sbExpected.Append("5601    ");
            inputRecord.SetSecondaryDiagnoses(8, "5601");
            sbExpected.Append("5589    ");
            inputRecord.SetSecondaryDiagnoses(9, "5589");
            sbExpected.Append("        "); //10
            sbExpected.Append("        "); //11
            sbExpected.Append("        "); //12
            sbExpected.Append("        "); //13
            sbExpected.Append("        "); //14
            sbExpected.Append("        "); //15
            sbExpected.Append("        "); //16
            sbExpected.Append("        "); //17
            sbExpected.Append("        "); //18
            sbExpected.Append("        "); //19
            sbExpected.Append("        "); //20
            sbExpected.Append("        "); //21
            sbExpected.Append("        "); //22
            sbExpected.Append("        "); //23
            sbExpected.Append("        "); //24
            sbExpected.Append("8622   ");
            inputRecord.PrincipalProcedure = "8622";
            sbExpected.Append("8628   ");
            inputRecord.SetSecondaryProcedures(1, "8628");
            sbExpected.Append("9904   ");
            inputRecord.SetSecondaryProcedures(2, "9904");
            sbExpected.Append("       "); //3
            sbExpected.Append("       "); //4
            sbExpected.Append("       "); //5
            sbExpected.Append("       "); //6
            sbExpected.Append("       "); //7
            sbExpected.Append("       "); //8
            sbExpected.Append("       "); //9
            sbExpected.Append("       "); //10
            sbExpected.Append("       "); //11
            sbExpected.Append("       "); //12
            sbExpected.Append("       "); //13
            sbExpected.Append("       "); //14
            sbExpected.Append("       "); //15
            sbExpected.Append("       "); //16
            sbExpected.Append("       "); //17
            sbExpected.Append("       "); //18
            sbExpected.Append("       "); //19
            sbExpected.Append("       "); //20
            sbExpected.Append("       "); //21
            sbExpected.Append("       "); //22
            sbExpected.Append("       "); //23
            sbExpected.Append("       "); //24
            sbExpected.Append("01/05/2010");
            inputRecord.SetProcedureDate(1, new DateTime(2010, 1, 5));
            sbExpected.Append("01/06/2010");
            inputRecord.SetProcedureDate(2, new DateTime(2010, 1, 6));
            sbExpected.Append("01/07/2010");
            inputRecord.SetProcedureDate(3, new DateTime(2010, 1, 7));
            sbExpected.Append("          "); //4
            sbExpected.Append("          "); //5
            sbExpected.Append("          "); //6
            sbExpected.Append("          "); //7
            sbExpected.Append("          "); //8
            sbExpected.Append("          "); //9
            sbExpected.Append("          "); //10
            sbExpected.Append("          "); //11
            sbExpected.Append("          "); //12
            sbExpected.Append("          "); //13
            sbExpected.Append("          "); //14
            sbExpected.Append("          "); //15
            sbExpected.Append("          "); //16
            sbExpected.Append("          "); //17
            sbExpected.Append("          "); //18
            sbExpected.Append("          "); //19
            sbExpected.Append("          "); //20
            sbExpected.Append("          "); //21
            sbExpected.Append("          "); //22
            sbExpected.Append("          "); //23
            sbExpected.Append("          "); //24
            sbExpected.Append("          "); //25
            sbExpected.Append("X");
            inputRecord.ApplyHACLogic = "X";
            sbExpected.Append("OptionalInformationOptionalInformationOptionalInformationOptionalInforma");
            inputRecord.OptionalInformation = "OptionalInformationOptionalInformationOptionalInformationOptionalInforma";

            string expected = string.Format("{0,-832}", sbExpected.ToString());
            string actual = inputRecord.ToString();
            //actual = inputRecord.GetString();
            Debug.WriteLine(expected);
            Debug.WriteLine(actual);

            Assert.AreEqual(expected, actual, "Actual does not match expected results.");
        }
    }
}
