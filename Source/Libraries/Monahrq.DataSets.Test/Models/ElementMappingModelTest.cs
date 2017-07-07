using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Monahrq.Infrastructure.Configuration;
using Monahrq.DataSets.Model;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Monahrq.Sdk.DataProvider;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.DataSets.ViewModels.Crosswalk;
using Monahrq.Infrastructure.Entities.Domain.Wings.Repository;

namespace Monahrq.DataSets.Test.Models
{
    [TestClass]
    public class ElementMappingModelTest
    {
        [TestMethod]
        public void ApplyValueTestPositive()
        {
            var target = new FooTarget();
            var targetDef = TargetRepository.New(DoubleFactory.DummyWing, Guid.Empty, "FooTarget");
            var element = DoubleFactory.CreateElementDouble(targetDef, "SomeInt");
           
            var model = new ElementMappingModel<FooTarget>(element, Enumerable.Empty<CrosswalkViewModel>());
            model.ApplyValue(target, 100);
            Assert.AreEqual(target.SomeInt, 100);
        }

        [TestMethod]
        public void ApplyValueScopeTest()
        {
            // given an element with name "SomeEnum"
            var target = new FooTarget();
            var targetDef = TargetRepository.New(DoubleFactory.DummyWing, Guid.Empty, "FooTarget");

            var element = DoubleFactory.CreateElementDouble(targetDef, "SomeEnum");

            // find that property on target
           
            var model = new ElementMappingModel<FooTarget>(element, Enumerable.Empty<CrosswalkViewModel>());

            // set target's SomeEnum property value to "Something.value3"
            model.ApplyValue(target, Something.value3);
            Assert.AreEqual(target.SomeEnum, Something.value3);
        }

        [TestMethod]
        public void ApplyValueTestWrongType()
        {
            var target = new FooTarget();
            var targetDef = TargetRepository.New(DoubleFactory.DummyWing, Guid.Empty, "FooTarget");
            var element = DoubleFactory.CreateElementDouble(targetDef, "SomeInt");
         
            string expected = "zzzzzzzzzzzzzzz";
            var model = new ElementMappingModel<FooTarget>(element, Enumerable.Empty<CrosswalkViewModel>());
            // a string is assigned, so test that it's not equal to the SomeInt property
            model.ApplyValue(target, expected);
            Assert.AreNotEqual(expected, target.SomeInt);
        }

        [TestMethod]
        public void ApplyValueStringToIntTest()
        {
            var target = new FooTarget();
            var targetDef = TargetRepository.New(DoubleFactory.DummyWing, Guid.Empty, "FooTarget");
            var element = DoubleFactory.CreateElementDouble(targetDef, "SomeInt");
         
            var expected = 123;
            string ValueToAssign = expected.ToString();
            var model = new ElementMappingModel<FooTarget>(element, Enumerable.Empty<CrosswalkViewModel>());
            model.ApplyValue(target, ValueToAssign);
            Assert.AreEqual(expected, target.SomeInt);
        }

        [TestMethod]
        public void ApplyValueStringToDoubleIntoIntTest()
        {
            var target = new FooTarget();
            var targetDef = TargetRepository.New(DoubleFactory.DummyWing, Guid.Empty, "FooTarget");
            var element = DoubleFactory.CreateElementDouble(targetDef, "SomeInt");
         
            var expected = 123.45;
            string ValueToAssign = expected.ToString();

            var model = new ElementMappingModel<FooTarget>(element, Enumerable.Empty<CrosswalkViewModel>());

            // a string is assigned, so test that it's not equal to the SomeInt property
            model.ApplyValue(target, ValueToAssign);
            Assert.AreNotEqual((int)expected, target.SomeInt);
        }

        [TestMethod]
        public void GenericApplyValueStringToDoubleIntoIntTest()
        {
            var target = new FooTarget();
            var targetDef = TargetRepository.New(DoubleFactory.DummyWing, Guid.Empty, "FooTarget");
            var element = DoubleFactory.CreateElementDouble(targetDef, "SomeInt");
         
            var expected = 123.45;
            string ValueToAssign = expected.ToString();
            var type = typeof(ElementMappingModel<>);
            type = type.MakeGenericType(typeof(FooTarget));
            var ctor = type.GetConstructor(new[] { typeof(Element), typeof(IEnumerable<CrosswalkViewModel>) });
            var model = ctor.Invoke(new object[] { element, Enumerable.Empty<CrosswalkViewModel>() });
            var pi = type.GetMethod("ApplyValue");

            // a string is assigned, so test that it's not equal to the SomeInt property
            pi.Invoke(model, new object[] { target, ValueToAssign });
            Assert.AreNotEqual((int)expected, target.SomeInt);
        }

       
    }
}
