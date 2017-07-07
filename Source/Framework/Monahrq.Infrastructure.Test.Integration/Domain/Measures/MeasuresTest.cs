using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Infrastructure.Entities.Domain.Measures.Fields;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.TestSupport;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Monahrq.Infrastructure.Entities.Domain.Wings.Repository;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.Test.Integration.Domain.Measures
{
	using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
	
	[TestClass, Ignore]
    public class MeasuresTest : MefTestFixture
    {

        static readonly Lazy<ISessionFactory> _lazyFactory = new Lazy<ISessionFactory>(() =>
            {
                var factoryProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
                return factoryProvider.SessionFactory;
            });


        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Monahrq.Infrastructure.Test.Integration.Properties.Settings.TestConnection"].ConnectionString);
            var testDb = builder.InitialCatalog;
            if (string.IsNullOrEmpty(testDb)) testDb = "[{2A373A8F-CFE9-46E4-9315-89547F9F7E4F}]";
            builder.InitialCatalog = "master";
            builder.IntegratedSecurity = true;
            using (var con = new SqlConnection(builder.ConnectionString))
            {
                con.Open();
                var db = con.GetSchema("Databases")
                    .Rows.OfType<DataRow>()
                    .FirstOrDefault(row =>
                            string.Equals(
                                row["database_name"].ToString(),
                                testDb,
                                StringComparison.OrdinalIgnoreCase)
                                );
                if (db == null)
                {
                    using (var cmd = new SqlCommand(string.Format("create database [{0}]", testDb), con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            builder.InitialCatalog = testDb;
            var serv = new ConfigurationService();
            var temp = serv.ConnectionSettings;
            temp.ConnectionString = builder.ConnectionString;
            serv.ConnectionSettings = temp;
        }

        protected override IEnumerable<Type> InjectedTypes
        {
            get
            {
                return new[] { typeof(ConfigurationService), typeof(DomainSessionFactoryProvider) };
            }
        }


        [TestInitialize()]
        public void Initialize()
        {
            using (var session = _lazyFactory.Value.OpenSession())
            {

                var wings = session.Query<Wing>().ToList();
                var tx = session.BeginTransaction();
                try
                {
                    wings.ForEach(session.Delete);
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        [TestMethod]
        public void CanCorrectlyMapTargetMeasures()
        {
            Random rand = new Random();
            var wing = DummyWing();
            while (wing.Targets.Count < 5)
            {
                var target = wing.Targets.DummyItem<Wing, Target>(wing);
                target.ClrType = "dummy";
                while (target.Measures.Count < 5)
                {
                    var measure = target.Measures.DummyItem<Target, Measure>(target);
                }
            }

            using (var session = _lazyFactory.Value.OpenSession())
            {
                session.Persist(wing);
                session.Flush();
            }

            using (var session = _lazyFactory.Value.OpenSession())
            {
                var items = session.Query<Target>();
                Assert.AreEqual(wing.Targets.Count, items.Count());
                foreach (var expected in wing.Targets)
                {
                    var actual = items.FirstOrDefault(c => c.Id == expected.Id);
                    Assert.IsNotNull(actual);
                    foreach (var measure in actual.Measures)
                    {
                        var priv = new PrivateObject(measure);
                        var actualOwner = priv.GetProperty("Owner") as Target;
                        Assert.IsTrue(measure.Id > 0);
                        Assert.AreEqual(expected.Id, actualOwner.Id);
                    }
                }
            }
        }

        private Wing DummyWing()
        {
            var wing = WingRepository.New(Guid.NewGuid().ToString());
            wing.WingGUID = Guid.NewGuid();
            return wing;
        }

        [TestMethod]
        public void CanCorrectlyMapMeasuresMetadata()
        {
            Random rand = new Random();
            var wing = DummyWing();
            while (wing.Targets.Count < 5)
            {
                var target = wing.Targets.DummyItem<Wing, Target>(wing);
                target.ClrType = "dummy";
                while (target.Measures.Count < 5)
                {
                    var measure = target.Measures.DummyItem<Target, Measure>(target);
                    //while (measure.Metadata.Count < 5)
                    //{
                    //    var metaData = measure.Metadata.DummyItem<Measure, MetadataItem>(measure);
                    //    metaData.Value = Guid.NewGuid().ToString();
                    //    measure.Metadata.Add(metaData);
                    //}
                }
            }
            using (var session = _lazyFactory.Value.OpenSession())
            {
                session.Persist(wing);
                session.Flush();
            }

            using (var session = _lazyFactory.Value.OpenSession())
            {
                var items = session.Query<Target>();
                Assert.AreEqual(wing.Targets.Count, items.Count());
                foreach (var expected in wing.Targets)
                {
                    var actual = items.FirstOrDefault(c => c.Id == expected.Id);
                    Assert.IsNotNull(actual);
                    //foreach (var measure in actual.Measures)
                    //{
                    //    //foreach (var metadata in measure.Metadata)
                    //    //{
                    //    //    var priv = new PrivateObject(metadata);
                    //    //    var actualOwner = priv.GetProperty("Owner") as Measure;
                    //    //    Assert.IsTrue(metadata.Id > 0);
                    //    //    Assert.IsTrue(!string.IsNullOrEmpty(metadata.Name));
                    //    //    Assert.AreEqual(measure.Id, actualOwner.Id);
                    //    //}
                    //}
                }
            }
        }

        //[TestMethod]
        //public void CanCorrectlyMapMeasuresTopics()
        //{
        //    Random rand = new Random();
        //    var wing = DummyWing();
        //    while (wing.Targets.Count < 5)
        //    {
        //        var target = wing.Targets.DummyItem<Wing, Target>(wing);
        //        target.ClrType = "dummy";
        //        while (target.Measures.Count < 5)
        //        {
        //            var measure = target.Measures.DummyItem<Target, Measure>(target);
        //            while (measure.Topics.Count < 5)
        //            {
        //                var root = measure.Topics.DummyItem<Measure, TopicSubtopic>(measure);
        //                root.RootTopic.Value = Guid.NewGuid().ToString();
        //                while (root.Subtopics.Count < 5)
        //                {
        //                    var sub = new Topic(
        //                     string.Format("{0} SubTopic: {1}",  
        //                            root.RootTopic.Name, root.Subtopics.Count+1));
        //                    sub.Value = Guid.NewGuid().ToString();
        //                    root.Subtopics.Add(sub);
        //                }
        //                measure.Topics.Add(root);
        //            }
        //        }
        //    }
        //    using (var session = LazyFactory.Value.OpenSession())
        //    {
        //        session.Persist(wing);
        //        session.Flush();
        //    }

        //    using (var session = LazyFactory.Value.OpenSession())
        //    {
        //        var items = session.Query<Target>();
        //        Assert.AreEqual(wing.Targets.Count, items.Count());
        //        foreach (var expected in wing.Targets)
        //        {
        //            var actual = items.FirstOrDefault(c => c.Id == expected.Id);
        //            Assert.IsNotNull(actual);
        //            foreach (var measure in actual.Measures)
        //            {
        //                foreach (var topic in measure.Topics)
        //                {
        //                    var priv = new PrivateObject(topic);
        //                    var actualOwner = priv.GetProperty("Owner") as Measure;
        //                    Assert.IsTrue(topic.Id > 0);
        //                    Assert.IsTrue(!string.IsNullOrEmpty(topic.RootTopic.Name));
        //                    Assert.AreEqual(measure.Id, actualOwner.Id);
        //                    foreach (var sub in topic.Subtopics)
        //                    {
        //                        var priv2 = new PrivateObject(sub);
        //                        var actualOwner2 = priv2.GetProperty("Owner") as RootTopic;
        //                        Assert.IsTrue(sub.Id > 0);
        //                        Assert.IsTrue(!string.IsNullOrEmpty(sub.Name));
        //                        Assert.AreEqual(topic.Id, actualOwner2.Id);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        [TestMethod]
        public void CanCorrectlyMapMeasuresHigherScoresAreBetter()
        {
            string expectedMessage = Guid.NewGuid().ToString();
            short expectedLength = short.MaxValue;
            Editability expectedEditability = Enum.GetValues(typeof(Editability)).OfType<Editability>().Last();

            Random rand = new Random();
            var wing = DummyWing();
            while (wing.Targets.Count < 5)
            {
                var target = wing.Targets.DummyItem<Wing, Target>(wing);
                target.ClrType = "dummy";
                while (target.Measures.Count < 5)
                {
                    var measure = target.Measures.DummyItem<Target, Measure>(target);
                    //measure.HigherScoresAreBetter.CurrentValue = true;
                    //measure.HigherScoresAreBetter.EditRule.Editability = expectedEditability;
                    //measure.HigherScoresAreBetter.EditRule.Message = expectedMessage;
                    //measure.HigherScoresAreBetter.ValidationRule.IsRequired = true;
                    //measure.HigherScoresAreBetter.ValidationRule.MaxLength = expectedLength;
                    //measure.HigherScoresAreBetter.ValidationRule.Message = expectedMessage;
                }
            }
            using (var session = _lazyFactory.Value.OpenSession())
            {
                session.Persist(wing);
                session.Flush();
            }

            using (var session = _lazyFactory.Value.OpenSession())
            {
                var items = session.Query<Target>();
                Assert.AreEqual(wing.Targets.Count, items.Count());
                foreach (var expected in wing.Targets)
                {
                    var actual = items.FirstOrDefault(c => c.Id == expected.Id);
                    Assert.IsNotNull(actual);
                    foreach (var measure in actual.Measures)
                    {
                        var theField = measure.HigherScoresAreBetter;
                        var actualValue = theField;
                        Assert.AreEqual(true, actualValue);
                        //var actualValMessage = theField.ValidationRule.Message;
                        //Assert.AreEqual(expectedMessage, actualValMessage);
                        //var actualEditMessage = theField.EditRule.Message;
                        //Assert.AreEqual(expectedMessage, actualEditMessage);
                        //var actualValLen = theField.ValidationRule.MaxLength;
                        //Assert.AreEqual(expectedLength, actualValLen);
                        //var actualReq = theField.ValidationRule.IsRequired;
                        //Assert.AreEqual(true, actualReq);
                    }
                }
            }
        }

        //[TestMethod]
        //public void ExampleSelect()
        //{
        //    var rand = new Random();
        //    var wing = DummyWing();
        //    while (wing.Targets.Count < 5)
        //    {
        //        var target = wing.Targets.DummyItem<Wing, Target>(wing);
        //        target.ClrType = "dummy";
        //        while (target.Measures.Count < 5)
        //        {
        //            var measure = target.Measures.DummyItem<Target, Measure>(target);
        //            measure.MeasureTitle.Plain.Value = "Plain : " + Guid.NewGuid().ToString();
        //            measure.MeasureTitle.Policy.Value = "Poliicy : " + Guid.NewGuid().ToString();
        //            measure.MeasureTitle.Clinical.Value = "Clinical : " + Guid.NewGuid().ToString();
        //            var select = rand.Next(0, 4);
        //            measure.MeasureTitle.Plain.IsSelected = select == 0;
        //            measure.MeasureTitle.Policy.IsSelected = select == 1;
        //            measure.MeasureTitle.Clinical.IsSelected = select == 2;
        //        }
        //    }
        //    using (var session = LazyFactory.Value.OpenSession())
        //    {
        //        session.Persist(wing);
        //        session.Flush();
        //    }

        //    using (var session = LazyFactory.Value.OpenSession())
        //    {
        //        var items = session.Query<Measure>();

        //        foreach (var item in items)
        //        {

        //            Console.Write("Id: {0}, Title: {1}, Dataset: {2}",
        //                item.Id, item.MeasureTitle.Selected.Value, item.Owner.Name);
        //        }
        //    }
        //}

        [TestMethod]
        public void CanCorrectlyMapUrl()
        {
            string expectedMessage = Guid.NewGuid().ToString();
            short expectedLength = short.MaxValue;
            Editability expectedEditability = Enum.GetValues(typeof(Editability)).OfType<Editability>().Last();
            var expectedUri = new Uri(@"http://www.google.com");
            Random rand = new Random();
            var wing = DummyWing();
            while (wing.Targets.Count < 5)
            {
                var target = wing.Targets.DummyItem<Wing, Target>(wing);
                target.ClrType = "dummy";
                while (target.Measures.Count < 5)
                {
                    var measure = target.Measures.DummyItem<Target, Measure>(target);
                    measure.Url = expectedUri.AbsoluteUri;
                    //measure.Url.EditRule.Editability = expectedEditability;
                    //measure.Url.EditRule.Message = expectedMessage;
                    //measure.Url.ValidationRule.IsRequired = true;
                    //measure.Url.ValidationRule.MaxLength = expectedLength;
                    //measure.Url.ValidationRule.Message = expectedMessage;
                }
            }
            using (var session = _lazyFactory.Value.OpenSession())
            {
                session.Persist(wing);
                session.Flush();
            }

            using (var session = _lazyFactory.Value.OpenSession())
            {
                var items = session.Query<Target>();
                Assert.AreEqual(wing.Targets.Count, items.Count());
                foreach (var expected in wing.Targets)
                {
                    var actual = items.FirstOrDefault(c => c.Id == expected.Id);
                    Assert.IsNotNull(actual);
                    foreach (var measure in actual.Measures)
                    {
                        var theField = measure.Url;
                        var actualValue = theField;
                        Assert.AreEqual(expectedUri.AbsoluteUri, actualValue);
                        //var actualValMessage = theField.ValidationRule.Message;
                        //Assert.AreEqual(expectedMessage, actualValMessage);
                        //var actualEditMessage = theField.EditRule.Message;
                        //Assert.AreEqual(expectedMessage, actualEditMessage);
                        //var actualValLen = theField.ValidationRule.MaxLength;
                        //Assert.AreEqual(expectedLength, actualValLen);
                        //var actualReq = theField.ValidationRule.IsRequired;
                        //Assert.AreEqual(true, actualReq);
                    }
                }
            }
        }

        [TestMethod]
        public void CanCorrectlyMapTitle()
        {
            string expectedPolicy = Guid.NewGuid().ToString();
            string expectedPlain = Guid.NewGuid().ToString();
            string expectedClinical = Guid.NewGuid().ToString();
            Random rand = new Random();
            var wing = DummyWing();
            while (wing.Targets.Count < 5)
            {
                var target = wing.Targets.DummyItem<Wing, Target>(wing);
                target.ClrType = "dummy";
                while (target.Measures.Count < 5)
                {
                    var measure = target.Measures.DummyItem<Target, Measure>(target);
                    measure.MeasureTitle.Clinical = expectedClinical;
                    measure.MeasureTitle.Plain = expectedPlain;
                    measure.MeasureTitle.Policy = expectedPolicy;
                }
            }
            using (var session = _lazyFactory.Value.OpenSession())
            {
                session.Persist(wing);
                session.Flush();
            }

            using (var session = _lazyFactory.Value.OpenSession())
            {
                var items = session.Query<Target>();
                Assert.AreEqual(wing.Targets.Count, items.Count());
                foreach (var expected in wing.Targets)
                {
                    var actual = items.FirstOrDefault(c => c.Id == expected.Id);
                    Assert.IsNotNull(actual);
                    foreach (var measure in actual.Measures)
                    {
                        var theField = measure.MeasureTitle;
                        var actualValue = theField.Clinical;
                        Assert.AreEqual(expectedClinical, actualValue);
                        actualValue = theField.Plain;
                        Assert.AreEqual(expectedPlain, actualValue);
                        actualValue = theField.Policy;
                        Assert.AreEqual(expectedPolicy, actualValue);
                    }
                }
            }
        }

        [TestMethod]
        public void CanCorrectlyMapStatePeerBenchmark()
        {
            Random rand = new Random();
            //bool expectedIsMean = true;
            //bool expectedIsProvided = true;
            //string expectedProvidedBenchmark = Guid.NewGuid().ToString();
            var expectedCalculationMethod = StatePeerBenchmarkCalculationMethod.Provided;
            var expectedProvidedBenchmark = rand.Next();
            var wing = DummyWing();
            while (wing.Targets.Count < 5)
            {
                var target = wing.Targets.DummyItem<Wing, Target>(wing);
                target.ClrType = "dummy";
                while (target.Measures.Count < 5)
                {
                    var measure = target.Measures.DummyItem<Target, Measure>(target);
                    //measure.StatePeerBenchmark.IsMean = expectedIsMean;
                    //measure.StatePeerBenchmark.IsProvided = expectedIsProvided;
                    measure.StatePeerBenchmark.CalculationMethod = expectedCalculationMethod;
                    measure.StatePeerBenchmark.ProvidedBenchmark = expectedProvidedBenchmark;
                }
            }
            using (var session = _lazyFactory.Value.OpenSession())
            {
                session.Persist(wing);
                session.Flush();
            }

            using (var session = _lazyFactory.Value.OpenSession())
            {
                var items = session.Query<Target>();
                Assert.AreEqual(wing.Targets.Count, items.Count());
                foreach (var expected in wing.Targets)
                {
                    var actual = items.FirstOrDefault(c => c.Id == expected.Id);
                    Assert.IsNotNull(actual);
                    foreach (var measure in actual.Measures)
                    {
                        var theField = measure.StatePeerBenchmark;
                        //Assert.AreEqual(expectedIsMean, theField.IsMean);
                        //Assert.AreEqual(expectedIsProvided, theField.IsProvided);
                        Assert.AreEqual(expectedCalculationMethod, theField.CalculationMethod);
                        Assert.AreEqual(expectedProvidedBenchmark, theField.ProvidedBenchmark);
                    }
                }
            }
        }

        [TestMethod]
        public void CanCorrectlyMapDescription()
        {
            string expectedMessage = Guid.NewGuid().ToString();
            short expectedLength = short.MaxValue;
            Editability expectedEditability = Enum.GetValues(typeof(Editability)).OfType<Editability>().Last();
            var expectedDesc = Guid.NewGuid().ToString();
            Random rand = new Random();
            var wing = DummyWing();
            while (wing.Targets.Count < 5)
            {
                var target = wing.Targets.DummyItem<Wing, Target>(wing);
                target.ClrType = "dummy";
                while (target.Measures.Count < 5)
                {
                    var measure = target.Measures.DummyItem<Target, Measure>(target);
                    measure.Description = expectedDesc;
                    //measure.Description.EditRule.Editability = expectedEditability;
                    //measure.Description.EditRule.Message = expectedMessage;
                    //measure.Description.ValidationRule.IsRequired = true;
                    //measure.Description.ValidationRule.MaxLength = expectedLength;
                    //measure.Description.ValidationRule.Message = expectedMessage;
                }
            }
            using (var session = _lazyFactory.Value.OpenSession())
            {
                session.Persist(wing);
                session.Flush();
            }

            using (var session = _lazyFactory.Value.OpenSession())
            {
                var items = session.Query<Target>();
                Assert.AreEqual(wing.Targets.Count, items.Count());
                foreach (var expected in wing.Targets)
                {
                    var actual = items.FirstOrDefault(c => c.Id == expected.Id);
                    Assert.IsNotNull(actual);
                    foreach (var measure in actual.Measures)
                    {
                        var theField = measure.Description;
                        var actualValue = theField;
                        Assert.AreEqual(expectedDesc, actualValue);
                        //var actualValMessage = theField.ValidationRule.Message;
                        //Assert.AreEqual(expectedMessage, actualValMessage);
                        //var actualEditMessage = theField.EditRule.Message;
                        //Assert.AreEqual(expectedMessage, actualEditMessage);
                        //var actualValLen = theField.ValidationRule.MaxLength;
                        //Assert.AreEqual(expectedLength, actualValLen);
                        //var actualReq = theField.ValidationRule.IsRequired;
                        //Assert.AreEqual(true, actualReq);
                    }
                }
            }
        }
    }

    static class ListHelpers
    {
        public static T DummyItem<TOwner, T>(this IEnumerable<T> items, TOwner owner)
                where T : class
                where TOwner : IEntity
        {
            var tType = typeof(T).GetProperty("Id").PropertyType;
            var ctorArgTypes = new[] { typeof(TOwner), typeof(string) };
            var ctorArgs = new object[] { owner, string.Format("{0} {1}: {2}", 
                        owner.Name, typeof(T).Name,  items.Count()) };
            var ctor = typeof(T).GetConstructor(ctorArgTypes);
            return ctor.Invoke(ctorArgs) as T;
        }

    }
}
