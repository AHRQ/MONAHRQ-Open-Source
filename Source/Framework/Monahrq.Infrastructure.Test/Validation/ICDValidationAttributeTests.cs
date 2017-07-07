using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Validation;
using System.ComponentModel.DataAnnotations;

namespace Monahrq.Infrastructure.Test.Validation
{
    [TestClass]
    public class ICDValidationAttributeTests
    {
        [TestMethod]
        public void ICD9_E_ICD9_V()
        {
            TestDxPass("E871", "V3000");
        }

        [TestMethod]
        public void ICD9_E_ICD10_A()
        {
            TestDxPass("E871", "A150");
        }

        [TestMethod]
        public void ICD10_A_ICD10_A()
        {
            TestDxPass("A150", "A154");
        }

        [TestMethod]
        public void ICD9_n_ICD10_A()
        {
            TestDxFail("0240", "A154");
        }

        [TestMethod]
        public void Ambiguous_SingleDx()
        {
            TestDxPass("E871");
        }

        [TestMethod]
        public void Ambiguous_ICD9_Only()
        {
            TestDxPass("E871", "V3000", "7661", "76719");
        }

        [TestMethod]
        public void ICD9_ICD10_Mix1()
        {
            // mix of ICD9 (numeric) and ICD10 (alphanumeric, not using valid ICD9 alpha prefix) codes
            TestDxFail("E871", "A150", "4239", "42731", "41401", "25040", "40390", "5859", "4148");
        }


        [TestMethod]
        public void ICD9_ICD10_Mix2()
        {
            // mix of ICD9 (numeric) and ICD10 (alphanumeric, not using valid ICD9 alpha prefix) codes
            TestDxFail("A150", "A154", "2768", "25000", "5589", "41400", "412", "3659", "73670");
        }

        [TestMethod]
        public void Ambiguous_ICD10_Only()
        {
            TestDxPass("E871", "A150");
        }

        [TestMethod]
        public void ICD10_Only2()
        {
            TestDxPass("A150", "A154");
        }

        private void TestDxPass(string dx1, params string[] dx)
        {
            var t = new TestType
            {
                PrincipalDiagnosis = dx1,
            };
            for (var i = 0; i < dx.Length; i++)
                typeof(TestType).GetProperty("Diagnosis" + (i + 2)).SetValue(t, dx[i]);
            var r = Test(t, true);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(ValidationResult.Success, r);
        }

        private void TestDxFail(string dx1, params string[] dx)
        {
            var t = new TestType
            {
                PrincipalDiagnosis = dx1,
            };
            for (var i = 0; i < dx.Length; i++)
                typeof(TestType).GetProperty("Diagnosis" + (i + 2)).SetValue(t, dx[i]);
            var r = Test(t, true);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(ValidationResult.Success, r);
        }

        private ValidationResult Test(TestType t, bool doProcedureCheck)
        {
            var a = new ICDValidationAttribute(typeof(TestType), doProcedureCheck);
            var ctx = new ValidationContext(t);
            return a.GetValidationResult(t, ctx);
        }

        class TestType
        {
            public string PrincipalDiagnosis { get; set; }
            public string Diagnosis2 { get; set; }
            public string Diagnosis3 { get; set; }
            public string Diagnosis4 { get; set; }
            public string Diagnosis5 { get; set; }
            public string Diagnosis6 { get; set; }
            public string Diagnosis7 { get; set; }
            public string Diagnosis8 { get; set; }
            public string Diagnosis9 { get; set; }

            public string PrincipalProcedure { get; set; }
            public string Procedure2 { get; set; }
        }
    }
}
