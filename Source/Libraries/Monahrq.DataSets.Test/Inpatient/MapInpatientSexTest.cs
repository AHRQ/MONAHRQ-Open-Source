using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.DataSets.Model;
using Monahrq.DataSets.ViewModels.Crosswalk;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain.Wings.Repository;
using Monahrq.Wing.Discharge.Inpatient;

namespace Monahrq.DataSets.Test.Inpatient
{

    [TestClass]
    public class MapInpatientSexTest
    {
        static Target DummyTarget
        {
            get
            {
                return TargetRepository.New(DummyWing, Guid.Empty, Guid.NewGuid().ToString());
            }
        }

        static Monahrq.Infrastructure.Entities.Domain.Wings.Wing DummyWing
        {
            get
            {
                return WingRepository.New(Guid.NewGuid().ToString());
            }
        }

        static Scope DummyScope(Type scopetype)
        {
            return new Scope(DummyTarget, Guid.NewGuid().ToString(), scopetype);
        }

        [TestMethod]
        [Ignore]
        public void TestNullMapsToScopePositive()
        {
            var scopes = Enum.GetValues(typeof(Sex))
                    .OfType<Sex>()
                    .Select(val => new ScopeValue(DummyScope(val.GetType()), val.ToString())
                        {
                            Value = val,
                            Description = val.ToString()
                        }).ToList();
            var propertyInfo = typeof(InpatientTarget).GetProperty("Sex");
            Assert.IsNotNull(propertyInfo);
            var element = new Element(DummyTarget, propertyInfo.Name);
            element.Scope = new Scope(DummyTarget, "Sex", typeof(Sex));
            element.Scope.Description = "Sex";
            PrivateObject prv = new PrivateObject(element.Scope);
            prv.SetProperty("Id", 1);
            scopes.ForEach(s => element.Scope.Values.Add(s));
            var fieldEntry = new FieldEntry(new DataColumn("Sex"));
            fieldEntry.Bin.AddValue(null);
            fieldEntry.Bin.AddValue(int.MinValue);
            fieldEntry.Bin.AddValue(0);
            fieldEntry.Bin.AddValue(int.MaxValue);
            var expected = Sex.Male;
            var notexpected = Sex.Female;
            fieldEntry.Bin[null].ScopeValue = scopes.FirstOrDefault(v => object.Equals(v.Value, expected));
            fieldEntry.Bin[int.MinValue].ScopeValue = scopes.FirstOrDefault(v => object.Equals(v.Value, notexpected));
            fieldEntry.Bin[0].ScopeValue = scopes.FirstOrDefault(v => object.Equals(v.Value, notexpected));
            fieldEntry.Bin[int.MaxValue].ScopeValue = scopes.FirstOrDefault(v => object.Equals(v.Value, notexpected));

            var mappedField = new MappedFieldEntryViewModel(propertyInfo, element, fieldEntry);

            var mapper = new TargetMapper<InpatientTarget>(DummyTarget, new[] { mappedField });

            mapper[element] = DBNull.Value;
            Assert.AreEqual(expected, mapper.Target.Sex);


            //var po = new PrivateObject(mapper);
            //var ElementMaps = po.GetProperty("ElementMaps") as IDictionary;
            //Assert.IsTrue(ElementMaps.Count > 0);

            //var errors = new List<ValidationError>();
            //var engine = new InstanceValidator<Monahrq.Wing.Discharge.Inpatient.InpatientTarget>();
            //var result = engine.ValidateInstance(mapper.Target);

        }
    }
}
