using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Entities.Domain.Hospitals.Maps
{
    [SubclassMappingProviderExport]
    public class HospitalCategoryMap : SubclassMap<HospitalCategory>
    {
        public HospitalCategoryMap()
        {
            Map(x => x.OwnerCount)
                .Formula(HOSPITAL_COUNT_QUERY)
                .Not.Insert()
                .Not.Update();

            base.DiscriminatorValue(typeof(HospitalCategory).Name);

            //Cache.NonStrictReadWrite().Region(Inflector.Pluralize(typeof(Category).Name));
        }

        private const string HOSPITAL_COUNT_QUERY = @"(
                                                         SELECT count(hc.[Hospital_Id]) 
                                                         FROM [dbo].[Hospitals_HospitalCategories] as hc
                                                         WHERE hc.[Category_Id] = Id
                                                      )";
    }
}