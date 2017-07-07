using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Infrastructure.Entities.Domain.Hospitals.Maps
{
    [MappingProviderExport]
    public class HospitalMap : HospitalRegistryItemMap<Hospital>
    {
        public HospitalMap()
        {
            var indexName = string.Format("IDX_{0}", EntityTableName);
            Map(x => x.CCR).Scale(4).Nullable();

            Map(x => x.CmsProviderID).Length(10).Index(indexName);
            Map(x => x.LocalHospitalId).Length(20).Index(indexName); 
            
            Map(x => x.Description).Length(1000);
            Map(x => x.Employees);
            Map(x => x.TotalBeds);
            Map(x => x.HospitalOwnership).Length(100);
            Map(x => x.PhoneNumber).Length(50);
            Map(x => x.FaxNumber).Length(50);
            Map(x => x.MedicareMedicaidProvider).Default("0");
            Map(x => x.EmergencyService).Default("0");
            Map(x => x.TraumaService).Default("0");
            Map(x => x.UrgentCareService).Default("0");
            Map(x => x.PediatricService).Default("0");
            Map(x => x.PediatricICUService).Default("0");
            Map(x => x.CardiacCatherizationService).Default("0");
            Map(x => x.PharmacyService).Default("0");
            Map(x => x.DiagnosticXRayService).Default("0");
            Map(x => x.ForceLocalHospitalIdValidation).Default("0");

            Map(x => x.IsDeleted).Default("0").Index(indexName);
            Map(x => x.IsArchived).Default("0").Index(indexName);
            Map(x => x.ArchiveDate); 
            Map(x => x.LinkedHospitalId);

            Map(x => x.Address).Length(100);
            Map(x => x.City).Length(100);
            Map(x => x.State).Length(10);
            Map(x => x.County).Length(10);
            Map(x => x.Zip).Length(12).Index(indexName);
            Map(x => x.Latitude);
            Map(x => x.Longitude); 

            References(x => x.HospitalServiceArea, "HospitalServiceArea_Id")
                .Not.LazyLoad()
                .Cascade.None()
                .ForeignKey(string.Format("FK_Hospitals_{0}", typeof(HospitalServiceArea).Name));

            References(x => x.HealthReferralRegion, "HealthReferralRegion_Id")
                .Not.LazyLoad()
                .Cascade.None()
                .ForeignKey(string.Format("FK_Hospital_{0}", typeof(HealthReferralRegion).Name));

            References(x => x.CustomRegion,"CustomRegion_Id")
                .Not.LazyLoad()
                .Cascade.None()
                .ForeignKey(string.Format("FK_Hospitals_{0}", typeof(CustomRegion).Name));

            HasManyToMany(x => x.Categories)
                .Table("Hospitals_" + Inflector.Pluralize(typeof(HospitalCategory).Name))
                .ParentKeyColumn("Hospital_Id")
                .ChildKeyColumn("Category_Id")
                .AsBag()
                .NotFound.Ignore()
                .Not.LazyLoad()
                .Not.Inverse()
                .Cascade.SaveUpdate();
        }
    }
}
