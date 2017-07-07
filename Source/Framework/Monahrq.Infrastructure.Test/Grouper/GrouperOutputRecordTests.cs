using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Exceptions;
using Monahrq.Infrastructure.Grouper;
using Monahrq.TestSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Test.Grouper
{
    [TestClass]
    [Ignore]
    public class GrouperOutputRecordTests : MefTestFixture
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
            grouper.AddRecordToBeGrouped(inputRecord);
            grouper.RunGrouper();
            grouper.GetGroupedRecord(outputRecord);
        }

        [TestMethod]
        public void SecondaryDiagnosesGetAtMinIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnoses(1);
        }
        
        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosesGetBelowMinIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnoses(0);
        }
        
        [TestMethod]
        public void SecondaryDiagnosesGetAtMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnoses(24);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosesGetAboveMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnoses(25);
        }

        [TestMethod]
        public void SecondaryProceduresGetAtMinIndexLengthTest()
        {
            outputRecord.GetSecondaryProcedures(1);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryProceduresGetBelowMinIndexLengthTest()
        {
            outputRecord.GetSecondaryProcedures(0);
        }

        [TestMethod]
        public void SecondaryProceduresGetAtMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryProcedures(24);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryProceduresGetAboveMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryProcedures(25);
        }

        [TestMethod]
        public void ProcedureDateGetAtMinIndexLengthTest()
        {
            outputRecord.GetProcedureDate(1);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void ProcedureDateGetBelowMinIndexLengthTest()
        {
            outputRecord.GetProcedureDate(0);
        }

        [TestMethod]
        public void ProcedureDateGetAtMaxIndexLengthTest()
        {
            outputRecord.GetProcedureDate(25);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void ProcedureDateGetAboveMaxIndexLengthTest()
        {
            outputRecord.GetProcedureDate(26);
        }

        [TestMethod]
        public void SecondaryDiagnosesReturnFlag1GetAtMinIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag1(1);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosesReturnFlag1GetBelowMinIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag1(0);
        }

        [TestMethod]
        public void SecondaryDiagnosesReturnFlag1GetAtMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag1(24);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosesReturnFlag1GetAboveMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag1(25);
        }

        [TestMethod]
        public void SecondaryDiagnosesReturnFlag2GetAtMinIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag2(1);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosesReturnFlag2GetBelowMinIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag2(0);
        }

        [TestMethod]
        public void SecondaryDiagnosesReturnFlag2GetAtMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag2(24);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosesReturnFlag2GetAboveMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag2(25);
        }

        [TestMethod]
        public void SecondaryDiagnosesReturnFlag3GetAtMinIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag3(1);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosesReturnFlag3GetBelowMinIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag3(0);
        }

        [TestMethod]
        public void SecondaryDiagnosesReturnFlag3GetAtMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag3(24);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosesReturnFlag3GetAboveMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag3(25);
        }

        [TestMethod]
        public void SecondaryDiagnosesReturnFlag4GetAtMinIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag4(1);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosesReturnFlag4GetBelowMinIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag4(0);
        }

        [TestMethod]
        public void SecondaryDiagnosesReturnFlag4GetAtMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag4(24);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosesReturnFlag4GetAboveMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesReturnFlag4(25);
        }

        [TestMethod]
        public void SecondaryDiagnosesHACAssignedGetAtMinIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesHACAssigned(1);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosesHACAssignedGetBelowMinIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesHACAssigned(0);
        }

        [TestMethod]
        public void SecondaryDiagnosesHACAssignedGetAtMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesHACAssigned(24);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosesHACAssignedGetAboveMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesHACAssigned(25);
        }

        [TestMethod]
        public void SecondaryDiagnosesHACGetAtMinIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesHAC(1);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosesHACGetBelowMinIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesHAC(0);
        }

        [TestMethod]
        public void SecondaryDiagnosesHACGetAtMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesHAC(24);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void SecondaryDiagnosesHACGetAboveMaxIndexLengthTest()
        {
            outputRecord.GetSecondaryDiagnosesHAC(25);
        }

        // ************************

        [TestMethod]
        public void ProcedureReturnFlag1GetAtMinIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag1(1);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void ProcedureReturnFlag1GetBelowMinIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag1(0);
        }

        [TestMethod]
        public void ProcedureReturnFlag1GetAtMaxIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag1(25);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void ProcedureReturnFlag1GetAboveMaxIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag1(26);
        }

        [TestMethod]
        public void ProcedureReturnFlag2GetAtMinIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag2(1);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void ProcedureReturnFlag2GetBelowMinIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag2(0);
        }

        [TestMethod]
        public void ProcedureReturnFlag2GetAtMaxIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag2(25);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void ProcedureReturnFlag2GetAboveMaxIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag2(26);
        }

        [TestMethod]
        public void ProcedureReturnFlag3GetAtMinIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag3(1);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void ProcedureReturnFlag3GetBelowMinIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag3(0);
        }

        [TestMethod]
        public void ProcedureReturnFlag3GetAtMaxIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag3(25);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void ProcedureReturnFlag3GetAboveMaxIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag3(26);
        }

        [TestMethod]
        public void ProcedureReturnFlag4GetAtMinIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag4(1);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void ProcedureReturnFlag4GetBelowMinIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag4(0);
        }

        [TestMethod]
        public void ProcedureReturnFlag4GetAtMaxIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag4(25);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void ProcedureReturnFlag4GetAboveMaxIndexLengthTest()
        {
            outputRecord.GetProcedureReturnFlag4(26);
        }

        [TestMethod]
        public void ProcedureHACAssignedGetAtMinIndexLengthTest()
        {
            outputRecord.GetProcedureHACAssigned(1);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void ProcedureHACAssignedGetBelowMinIndexLengthTest()
        {
            outputRecord.GetProcedureHACAssigned(0);
        }

        [TestMethod]
        public void ProcedureHACAssignedGetAtMaxIndexLengthTest()
        {
            outputRecord.GetProcedureHACAssigned(25);
        }

        [TestMethod]
        [ExpectedException(typeof(GrouperRecordException), "Exception for index out of range wasn't thrown.")]
        public void ProcedureHACAssignedGetAboveMaxIndexLengthTest()
        {
            outputRecord.GetProcedureHACAssigned(26);
        }
    }
}
