using System;
using System.Linq;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using NHibernate;
using NHibernate.Linq;
using Monahrq.Infrastructure.Test.Integration.Services;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Data.Extensibility;
using System.IO;
using log4net.Config;
using log4net;
using Microsoft.Practices.ServiceLocation;
using System.Collections.Generic;
using Monahrq.Infrastructure.Services;
using LinqKit;
using Monahrq.Infrastructure.Entities.Domain.Hospitals.Mapping;
using Monahrq.Infrastructure.Services.Hospitals;

namespace Monahrq.Infrastructure.Test.Integration.Domain.Hospitals
{

    [TestClass]
    [Ignore]
    public class IHospitalRegistryServiceTest : RepositoryTestFixture
    {

        [ClassInitialize()]
        public static void MyTestInitialize(TestContext testContext)
        {
            // Take care the log4net.config file is added to the deployment files of the testconfig
            FileInfo fileInfo;
            string fullPath = Path.Combine(System.Environment.CurrentDirectory, "log4net.config");
            fileInfo = new FileInfo(fullPath);
            // Reload the configuration
            XmlConfigurator.Configure(fileInfo);
            // test to see if it works
            // Logger instance named "MyApp".
            ILog log = LogManager.GetLogger(typeof(IHospitalRegistryServiceTest));
            log.Info("check");
        }

        protected override System.Collections.Generic.IEnumerable<Type> InjectedTypes
        {
            get
            {
                var temp = base.InjectedTypes.Concat(
                     new Type[] { typeof(DomainSessionFactoryProvider) 
                    , typeof(HospitalRegistryService)
                , typeof(ClassMappingTypeProvider)});
                temp = ApplyTestClassMaps(temp);
                return temp;
            }
        }

        private IEnumerable<Type> ApplyTestClassMaps(IEnumerable<Type> temp)
        {
            var mappingTypeProvider = new ClassMappingTypeProvider();
            return temp.Concat(mappingTypeProvider.ProviderTypes).Concat(mappingTypeProvider.ConventionTypes);
        }

        ISession CreateSession()
        {
            return Container.GetExportedValue<IDomainSessionFactoryProvider>().SessionFactory.OpenSession();
        }

        State GetState(string abbrv)
        {
            return GetStates(new[] { abbrv }).FirstOrDefault();
        }

        IEnumerable<State> GetStates(IEnumerable<string> abbrvs)
        {
            var crit = PredicateBuilder.False<State>();
            foreach (var abbr in abbrvs)
            {
                crit = crit.Or(s => s.Abbreviation == abbr);
            }
            using (var session = CreateSession())
            {
                return session.Query<State>().Where(crit).ToList();
            }
        }

        object sync = new object();

        [TestMethod]
        public void CreateRegionPositive()
        {

            var registryService = ServiceLocator.Current.GetInstance<IHospitalRegistryService>();
            var registry = registryService.CurrentRegistry;
            int cntHospitals;
            int cntCustomRegions;
            State virginia = null;
            using (var session = CreateSession())
            {
                cntHospitals = session.Query<Hospital>().Where(reg => reg.Registry.Id == registry.Id).Count();
                cntCustomRegions = session.Query<CustomRegion>().Where(reg => reg.Registry.Id == registry.Id).Count();
                virginia = session.Query<State>().Where(st => st.Abbreviation == "va").Single();
            }

            int id = 0;
            CustomRegion newRegion = registryService.CreateRegion();
            newRegion.Name = "Some name";
            newRegion.State = virginia;
            Assert.AreEqual(0, id);
            registryService.Save(newRegion);
            id = newRegion.Id;
            Assert.AreNotEqual(0, id);
            int cntHospitalsPost;
            int cntCustomRegionsPost;
            using (var session = CreateSession())
            {
                cntHospitalsPost = session.Query<Hospital>().Where(reg => reg.Registry.Id == registry.Id).Count();
                cntCustomRegionsPost = session.Query<CustomRegion>().Where(reg => reg.Registry.Id == registry.Id).Count();
            }

            Assert.AreEqual(cntHospitals, cntHospitalsPost);
            Assert.AreEqual(cntCustomRegions + 1, cntCustomRegionsPost);
            Region fetch = null;
            using (var session = CreateSession())
            {
                fetch = session.Get<CustomRegion>(id);
            }

            Assert.AreEqual(fetch, newRegion);

            newRegion = registryService.CreateRegion();
            newRegion.Name = "Some name1";
            newRegion.State = virginia;

            id = newRegion.Id;
            Assert.AreEqual(0, id);
            registryService.Save(newRegion);
            id = newRegion.Id;
            Assert.AreNotEqual(0, id);

            newRegion = registryService.CreateRegion();
            newRegion.Name = "Some name2";
            newRegion.State = virginia;
            registryService.Save(newRegion);
            id = newRegion.Id;
            newRegion = null;

            using (var session = CreateSession())
            {
                cntHospitalsPost = session.Query<Hospital>().Where(reg => reg.Registry.Id == registry.Id).Count();
                cntCustomRegionsPost = session.Query<CustomRegion>().Where(reg => reg.Registry.Id == registry.Id).Count();
            }
            Assert.AreEqual(cntHospitals, cntHospitalsPost);
            Assert.AreEqual(cntCustomRegions + 3, cntCustomRegionsPost);
        }


        [TestMethod]
        public void GenerateInitialMappingPositive()
        {
            var registryService = ServiceLocator.Current.GetInstance<IHospitalRegistryService>();
            var states = GetStates(new[] { "ms", "mi", "nv" });
            var mapping = registryService.GenerateMappingReference<HospitalServiceArea>(states);
            Assert.IsNotNull(mapping);
            Assert.IsNotNull(mapping.Hospitals);
            Assert.IsNotNull(mapping.Hospitals.Any());
        }

        [TestMethod]
        public void SaveHospitalRegionMapping()
        {
            var registryService = ServiceLocator.Current.GetInstance<IHospitalRegistryService>();
            var states = GetStates(new[] { "ms", "mi", "nv" });

            // lets create and save a custom region:

            CustomRegion newRegion = registryService.CreateRegion();

            // Make sure it has a unique name!!
            newRegion.Name = Guid.NewGuid().ToString();
            newRegion.State = states.Where(st => string.Equals(st.Abbreviation, "mi", StringComparison.OrdinalIgnoreCase)).First();
            registryService.Save(newRegion);

            // now lets get a reference mapping for those states for the states...

            var reference = registryService.GenerateMappingReference<HospitalServiceArea>(states);

            var myMappings = registryService.CreateHospitalRegionMapping<HospitalServiceArea>();
            // lets give thew session a name
            myMappings.Name = "Some task we are doing " + Guid.NewGuid().ToString();

            // this will be mapped to default
            var someHospital = reference.Hospitals[0];
            myMappings.AddEntry(someHospital, someHospital.HospitalServiceArea);
            // this will be mapped to a custom
            someHospital = reference.Hospitals[1];
            // lets select the region that we created above
            var referencedCustomREgion = reference.Regions.Where(reg => reg.Id == newRegion.Id).First();
            myMappings.AddEntry(someHospital, someHospital.HospitalServiceArea);

            /// lets save myMappings
            registryService.Save(myMappings);

            // Now lets save the mapping:

            //var mappedToDefaultHsa = new 
        }

        [TestMethod]
        public void DeleteCustomRegionPositive()
        {
            var registryService = ServiceLocator.Current.GetInstance<IHospitalRegistryService>();
            var registry = registryService.CurrentRegistry;
            int cntHospitals;
            int cntCustomRegions;
            State virginia = null;
            using (var session = CreateSession())
            {
                cntHospitals = session.Query<Hospital>().Where(reg => reg.Registry.Id == registry.Id).Count();
                cntCustomRegions = session.Query<CustomRegion>().Where(reg => reg.Registry.Id == registry.Id).Count();
                virginia = session.Query<State>().Where(st => st.Abbreviation == "va").Single();
            }

            int id = 0;
            CustomRegion newRegion = registryService.CreateRegion();
            newRegion.Name = "Some name";
            newRegion.State = virginia;
            Assert.AreEqual(0, id);
            registryService.Save(newRegion);
            id = newRegion.Id;
            Assert.AreNotEqual(0, id);
            registryService.Delete(newRegion);
            using (var session = CreateSession())
            {
                var test = session.Get<CustomRegion>(id);
                Assert.IsNull(test);
            }

        }

        [TestMethod]
        public void CreateRegionTypeExceptionPositive()
        {
            var result = HospitalRegionMapping.CreateRegionTypeException(typeof(HospitalServiceArea));
            Assert.IsInstanceOfType(result, typeof(InvalidRegionMappingException<HospitalServiceArea>));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidRegionMappingException<HealthReferralRegion>))]
        public void AddEntryInvalidHealthReferralRegionPositive()
        {
            var mockRegistry = Rhino.Mocks.MockRepository.GenerateMock<HospitalRegistry>();
            HospitalRegionMapping mapping = new HospitalRegionMapping(mockRegistry, typeof(HealthReferralRegion));
            var mockHsa = Rhino.Mocks.MockRepository.GenerateMock<HospitalServiceArea>();
            var mockHosp = Rhino.Mocks.MockRepository.GenerateMock<Hospital>();
            mapping.AddEntry(mockHosp, mockHsa);
        }

    }
}
