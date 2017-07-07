using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.DataSets.Model;
using System.Collections;
using Monahrq.Infrastructure.Validation;
using System.Collections.Generic;
using Monahrq.Sdk.Extensibility.Data;
using System.Linq;
using Monahrq.DataSets.ViewModels.Crosswalk;
using Monahrq.Wing.Discharge.Inpatient;
using Monahrq.Infrastructure.Entities.Domain.Wings.Repository;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.DataSets.Test.Models
{
    [TestClass]
    public class TargetMapperTest
    {
        static Target DummyTarget
        {
            get
            {
                var wing = WingRepository.New(Guid.NewGuid().ToString());
                return TargetRepository.New(wing, Guid.Empty, Guid.NewGuid().ToString());
            }
        }

        //[TestMethod]
        //public void TestCtor()
        //{
        //    var mapper = new TargetMapper<InpatientTarget>(DummyTarget, Enumerable.Empty<MappedFieldEntryViewModel>());
        //    var po = new PrivateObject(mapper);
        //    var ElementMaps = po.GetProperty("ElementMaps") as IDictionary;
        //    Assert.IsTrue(ElementMaps.Count > 0);

        //    var errors = new List<ValidationError>();
        //    var engine = new InstanceValidator<Monahrq.Wing.Discharge.Inpatient.InpatientTarget>();
        //    var result = engine.ValidateInstance(mapper.Target);
        //}

        [TestMethod]
        [Ignore]
        public void TestWarnings()
        {
            var wing = WingRepository.New(Guid.NewGuid().ToString());
            var target = TargetRepository.New(wing, Guid.Empty, Guid.NewGuid().ToString());
            var mapper = new TargetMapper<FooTarget>(target, Enumerable.Empty<MappedFieldEntryViewModel>());
            var po = new PrivateObject(mapper);
            var ElementMaps = po.GetProperty("ElementMaps") as IDictionary;
            Assert.IsTrue(ElementMaps.Count > 0);

            var errors = new List<ValidationError>();
            var engine = new InstanceValidator<FooTarget>();
            var result = engine.ValidateInstance(mapper.Target);
            Assert.IsTrue(result.PropertyWarnings.Count > 0);
        }

        [TestMethod]
        [Ignore]
        public void TestIndexer()
        {
            var mapper = new TargetMapper<FooTarget>(DummyTarget, Enumerable.Empty<MappedFieldEntryViewModel>());
            mapper["SomeEnum"] = Something.value2;
            var dispMapper = mapper as ITargetMapper;
            var value = dispMapper.Target.SomeEnum;
            Assert.AreEqual(value, Something.value2);
        }

        [TestMethod]
        [Ignore]
        public void TestTargetMapperFactory()
        {
            var type = typeof(FooTarget);
            var target = DummyTarget;
            target.ClrType = type.AssemblyQualifiedName;
            var factory = new TargetMapperFactory(target);
            var mapper = factory.CreateMapper(Enumerable.Empty<MappedFieldEntryViewModel>());
            mapper["SomeEnum"] = Something.value2;
            var value = mapper.Target.SomeEnum;
            Assert.AreEqual(value, Something.value2);
        }

        [TestMethod]
        [Ignore]
        public void TestReflection()
        {
            var typeString = typeof(InpatientTarget).AssemblyQualifiedName;
            var mapperType = typeof(TargetMapper<>).MakeGenericType(Type.GetType(typeString));
            dynamic mapper = Activator.CreateInstance(mapperType, DummyTarget, Enumerable.Empty<MappedFieldEntryViewModel>());
            //var repotype = typeof(IExtensionRepository<>).MakeGenericType(Type.GetType(typeString));
            //dynamic repo = Activator.CreateInstance(repotype);
            //repo.Update(mapper.Target);
            //var targetType = Type.GetType("Monahrq.Wing.InpatientData.InpatientTarget, Monahrq.Wing.InpatientData, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null");
            //type = type.MakeGenericType(targetType);

            //dynamic repo = ServiceLocator.Current.GetInstance<IWindsorContainer>().Resolve(type);

        }

    }
}
