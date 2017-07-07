using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Services.Hospitals;

namespace Monahrq.Infrastructure.Test.Integration.Domain.Hospitals.Mapping
{
	using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
	
	
	[TestClass, Ignore]
    public class HospitalRegistryServiceTest : RepositoryTestFixture<HospitalRegistryService, IHospitalRegistryService, HospitalRegistryServiceTest>
    {

        protected override bool LogSql
        {
            get { return true; }

        }

        [TestMethod]
        public override void ServiceCtorPositive()
        {
            base.ServiceCtorPositive();
        }

        [TestMethod]
        public void CanGetStates()
        {
            var service = FactoryService();
            var state = service.GetStates(new[] { "ca", "or" });
            Assert.AreEqual(2, state.Count());
        }


        [TestMethod]
        public void GenerateMappingReferencePositive()
        {
            var service = FactoryService();
            var states = service.GetStates(new[] { "ca", "or" });
            var reference = service.GenerateMappingReference(states, typeof(HospitalServiceArea));
            Assert.IsTrue(reference.Regions.Count > 0);
        }


        //[TestMethod]
        //public void CreateSessionPositive()
        //{
        //    var service =  ServiceLocator.Current.GetInstance<IHospitalRegistryService>();

        //    // Lets use california and oregon
        //    var states = service.GetStates(new [] { "ca", "or" });
            
        //    // get a reference dataset... hospitals and regions... in this case Hospital Service Areas
        //    var reference = service.GenerateMappingReference(states, typeof(HospitalServiceArea));
            
        //    //sanity check
        //    Assert.IsTrue(reference.Regions.Count > 0);
            
        //    // Lets start a mapping session
        //    var mappingSession = service.CreateHospitalRegionMapping<HospitalServiceArea>();

        //    // For example, we can ask the service to create a custom region for us
        //    var myCustomRegion = service.CreateRegion();
        //    myCustomRegion.Name = "My custom Region " + Guid.NewGuid().ToString();

        //    // lets get a hospital from our reference
        //    var someHosp = reference.Hospitals.First();

        //    // And lets create a mapping entry .... Hospital <--> Region
        //    mappingSession.AddEntry(someHosp, myCustomRegion);

        //    // lets get another hospital from our reference
        //    var someOtherHospital = reference.Hospitals.Last();

        //    // and map it to our custom region 
        //    mappingSession.AddEntry(someOtherHospital, myCustomRegion);
            
        //    /// lets save the mapping session
        //    service.Save(mappingSession);

       
            
        //}


     


    }
}
