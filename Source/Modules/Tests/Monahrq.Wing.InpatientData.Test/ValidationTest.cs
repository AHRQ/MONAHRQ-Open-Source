using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Validation;
using Monahrq.Wing.Ahrq.Area;
using Monahrq.Wing.Discharge.Inpatient;

namespace Monahrq.Wing.InpatientData.Test
{
    [TestClass]
    public class ValidationTest
    {
        [TestMethod]
        public void ValidationTestValidateInpatientTarget()
        {
            var inpatient = new InpatientTarget();
            var engine = new InstanceValidator<InpatientTarget>();
            var result = engine.ValidateInstance(inpatient);
            Assert.IsTrue(result.PropertyErrors.Count > 0);
        }

        private static bool IsRecord(Type type)
        {
            var idProp = type.GetProperty("Id");
            if (idProp == null) return false;
            var domainType = typeof(IEntity<>);
            domainType = domainType.MakeGenericType(idProp.PropertyType);
            return type.GetProperty("Id") != null &&
                   (type.GetProperty("Id").GetAccessors() ?? Enumerable.Empty<MethodInfo>()).All(x => x.IsVirtual) &&
                   !type.IsSealed &&
                   !type.IsAbstract &&
                    domainType.IsAssignableFrom(type);
        }

        [TestMethod]
        public void TestIsRecord()
        {
            var type = typeof(AreaTarget);
            var isrecord = IsRecord(type);
            Assert.IsTrue(isrecord);

        }
    }
}
