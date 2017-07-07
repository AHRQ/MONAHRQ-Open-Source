using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.TestSupport;
using System;
using Monahrq.Infrastructure.Grouper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Test.Grouper
{
    [TestClass]
    [Ignore]
    public class GrouperTest : MefTestFixture
    {
        Infrastructure.Grouper.Grouper grouper;
        GrouperInputRecord inputRecord;
        GrouperOutputRecord outputRecord;

        protected override void ComposeFixtureInstances()
        {
            base.ComposeFixtureInstances();
            grouper = new Infrastructure.Grouper.Grouper();
            inputRecord = new GrouperInputRecord();
            outputRecord = new GrouperOutputRecord();
        }

        [TestMethod]
        public void RunGrouperAndCheckOutputTest()
        {
            // Note: I put all of the asserts in one test due to the high cost of actually running the grouper.
            inputRecord.PatientName = "PatientNamePatientNamePatientNa";
            inputRecord.MedicalRecordNumber = "MedicalRecord";
            inputRecord.AccountNumber = "AccountNumberAcco";
            inputRecord.AdmitDate = new DateTime(2010, 1, 2);
            inputRecord.DischargeDate = new DateTime(2010, 1, 3);
            inputRecord.DischargeStatus = 20;
            inputRecord.PrimaryPayer = 1;
            inputRecord.LOS = 987;
            inputRecord.BirthDate = new DateTime(1971, 1, 4);
            inputRecord.Age = 41;
            inputRecord.Sex = 1;
            inputRecord.AdmitDiagnosis = "34400";
            inputRecord.PrimaryDiagnosis = "486";
            inputRecord.SetSecondaryDiagnoses(1, "7802");
            inputRecord.SetSecondaryDiagnoses(2, "34691");
            inputRecord.SetSecondaryDiagnoses(3, "45341");
            inputRecord.SetSecondaryDiagnoses(4, "27651");
            inputRecord.SetSecondaryDiagnoses(5, "V4582");
            inputRecord.SetSecondaryDiagnoses(6, "78651");
            inputRecord.SetSecondaryDiagnoses(7, "1890");
            inputRecord.SetSecondaryDiagnoses(8, "5601");
            inputRecord.SetSecondaryDiagnoses(9, "5589");
            inputRecord.PrincipalProcedure = "8622";
            inputRecord.SetSecondaryProcedures(1, "8628");
            inputRecord.SetSecondaryProcedures(2, "9904");
            inputRecord.SetProcedureDate(1, new DateTime(2010, 1, 5));
            inputRecord.SetProcedureDate(2, new DateTime(2010, 1, 6));
            inputRecord.SetProcedureDate(3, new DateTime(2010, 1, 7));
            inputRecord.ApplyHACLogic = "X";
            inputRecord.OptionalInformation = "OptionalInformationOptionalInformationOptionalInformationOptionalInforma";
            grouper.AddRecordToBeGrouped(inputRecord);
            grouper.RunGrouper();
            grouper.GetGroupedRecord(outputRecord);

            // Make sure input parameters match after going through the grouper.
            Assert.AreEqual(inputRecord.PatientName, outputRecord.PatientName);
            Assert.AreEqual(inputRecord.MedicalRecordNumber, outputRecord.MedicalRecordNumber);
            Assert.AreEqual(inputRecord.AccountNumber, outputRecord.AccountNumber);
            Assert.AreEqual(inputRecord.AdmitDate, outputRecord.AdmitDate);
            Assert.AreEqual(inputRecord.DischargeDate, outputRecord.DischargeDate);
            Assert.AreEqual(inputRecord.DischargeStatus, outputRecord.DischargeStatus);
            Assert.AreEqual(inputRecord.PrimaryPayer, outputRecord.PrimaryPayer);
            Assert.AreEqual(inputRecord.LOS, outputRecord.LOS);
            Assert.AreEqual(inputRecord.BirthDate, outputRecord.BirthDate);
            Assert.AreEqual(inputRecord.Age, outputRecord.Age);
            Assert.AreEqual(inputRecord.Sex, outputRecord.Sex);
            Assert.AreEqual(inputRecord.AdmitDiagnosis, outputRecord.AdmitDiagnosis);
            Assert.AreEqual(inputRecord.PrimaryDiagnosis, outputRecord.PrimaryDiagnosis);
            Assert.AreEqual(inputRecord.GetSecondaryDiagnoses(1), outputRecord.GetSecondaryDiagnoses(1));
            Assert.AreEqual(inputRecord.GetSecondaryDiagnoses(2), outputRecord.GetSecondaryDiagnoses(2));
            Assert.AreEqual(inputRecord.GetSecondaryDiagnoses(3), outputRecord.GetSecondaryDiagnoses(3));
            Assert.AreEqual(inputRecord.GetSecondaryDiagnoses(4), outputRecord.GetSecondaryDiagnoses(4));
            Assert.AreEqual(inputRecord.GetSecondaryDiagnoses(5), outputRecord.GetSecondaryDiagnoses(5));
            Assert.AreEqual(inputRecord.GetSecondaryDiagnoses(6), outputRecord.GetSecondaryDiagnoses(6));
            Assert.AreEqual(inputRecord.GetSecondaryDiagnoses(7), outputRecord.GetSecondaryDiagnoses(7));
            Assert.AreEqual(inputRecord.GetSecondaryDiagnoses(8), outputRecord.GetSecondaryDiagnoses(8));
            Assert.AreEqual(inputRecord.GetSecondaryDiagnoses(9), outputRecord.GetSecondaryDiagnoses(9));
            Assert.AreEqual(inputRecord.PrincipalProcedure, outputRecord.PrincipalProcedure);
            Assert.AreEqual(inputRecord.GetSecondaryProcedures(1), outputRecord.GetSecondaryProcedures(1));
            Assert.AreEqual(inputRecord.GetSecondaryProcedures(2), outputRecord.GetSecondaryProcedures(2));
            Assert.AreEqual(inputRecord.GetProcedureDate(1), outputRecord.GetProcedureDate(1));
            Assert.AreEqual(inputRecord.GetProcedureDate(2), outputRecord.GetProcedureDate(2));
            Assert.AreEqual(inputRecord.GetProcedureDate(3), outputRecord.GetProcedureDate(3));
            Assert.AreEqual(inputRecord.ApplyHACLogic, outputRecord.ApplyHACLogic);
            Assert.AreEqual(inputRecord.OptionalInformation, outputRecord.OptionalInformation);

            // Check output parameters.
            Assert.AreEqual(270, outputRecord.MsgMceVersionUsed);
            Assert.AreEqual(167, outputRecord.InitialDRG);
            Assert.AreEqual(2, outputRecord.InitialMedicalSurgicalIndicator);
            Assert.AreEqual(4, outputRecord.FinalMDC);
            Assert.AreEqual(167, outputRecord.FinalDRG);
            Assert.AreEqual(2, outputRecord.FinalMedicalSurgicalIndicator);
            Assert.AreEqual(0, outputRecord.DRGReturnCode);
            Assert.AreEqual(0, outputRecord.MsgMceEditReturnCode);
            Assert.AreEqual(10, outputRecord.DiagnosticCodeCount);
            Assert.AreEqual(3, outputRecord.ProcedureCodeCount);
            Assert.AreEqual(12, outputRecord.PrincipalDiagnosisEditReturnFlag1);
            Assert.AreEqual(0, outputRecord.PrincipalDiagnosisEditReturnFlag2);
            Assert.AreEqual(0, outputRecord.PrincipalDiagnosisEditReturnFlag3);
            Assert.AreEqual(0, outputRecord.PrincipalDiagnosisEditReturnFlag4);
            Assert.AreEqual(0, outputRecord.PrincipalDiagnosisHACAssigned);
            Assert.AreEqual(0, outputRecord.PrincipalDiagnosisHAC);

            for (int i = 1; i <= 2; i++)
            {
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag1(i));
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag2(i));
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag3(i));
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag4(i));
            }
            Assert.AreEqual(11, outputRecord.GetSecondaryDiagnosesReturnFlag1(3));
            Assert.AreEqual(12, outputRecord.GetSecondaryDiagnosesReturnFlag2(3));
            Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag3(3));
            Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag4(3));
            for (int i = 5; i <= 6; i++)
            {
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag1(i));
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag2(i));
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag3(i));
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag4(i));
            }
            for (int i = 7; i <= 8; i++)
            {
                Assert.AreEqual(11, outputRecord.GetSecondaryDiagnosesReturnFlag1(i));
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag2(i));
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag3(i));
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag4(i));
            }
            for (int i = 9; i <= 24; i++)
            {
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag1(i));
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag2(i));
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag3(i));
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesReturnFlag4(i));
            }
            for (int i = 1; i <= 9; i++)
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesHACAssigned(i));
            for (int i = 10; i <= 24; i++)
                Assert.AreEqual(null, outputRecord.GetSecondaryDiagnosesHACAssigned(i));
            for (int i = 1; i <= 9; i++)
                Assert.AreEqual(0, outputRecord.GetSecondaryDiagnosesHAC(i));
            for (int i = 10; i <= 24; i++)
                Assert.AreEqual(null, outputRecord.GetSecondaryDiagnosesHAC(i));

            Assert.AreEqual(12, outputRecord.GetProcedureReturnFlag1(1));
            Assert.AreEqual(20, outputRecord.GetProcedureReturnFlag2(1));
            Assert.AreEqual(0, outputRecord.GetProcedureReturnFlag3(1));
            Assert.AreEqual(0, outputRecord.GetProcedureReturnFlag4(1));

            for (int i = 2; i <= 25; i++)
            {
                Assert.AreEqual(0, outputRecord.GetProcedureReturnFlag1(i));
                Assert.AreEqual(0, outputRecord.GetProcedureReturnFlag2(i));
                Assert.AreEqual(0, outputRecord.GetProcedureReturnFlag3(i));
                Assert.AreEqual(0, outputRecord.GetProcedureReturnFlag4(i));
            }
            for (int i = 1; i <= 3; i++)
                Assert.AreEqual(0, outputRecord.GetProcedureHACAssigned(i));
            for (int i = 4; i <= 25; i++)
                Assert.AreEqual(null, outputRecord.GetProcedureHACAssigned(i));
            Assert.AreEqual(1662, outputRecord.Initial4DigitDRG);
            Assert.AreEqual(1662, outputRecord.Final4DigitDRG);
            Assert.AreEqual(2, outputRecord.FinalDrgCcMccUsage);
            Assert.AreEqual(2, outputRecord.InitialDrgCcMccUsage);
            Assert.AreEqual(0, outputRecord.NumberOfUniqueHACMet);
            Assert.AreEqual(0, outputRecord.HACStatus);
            Assert.AreEqual((float)2.0068, outputRecord.CostWeight);
        }
    }
}
