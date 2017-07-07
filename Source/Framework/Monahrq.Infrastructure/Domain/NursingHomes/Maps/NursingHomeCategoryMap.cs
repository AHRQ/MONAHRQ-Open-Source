using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Domain.Data;

namespace Monahrq.Infrastructure.Domain.NursingHomes.Maps
{
    [SubclassMappingProviderExport]
    public class NursingHomeCategoryMap : SubclassMap<NursingHomeCategory>
    {
        public NursingHomeCategoryMap()
        {
            Map(x => x.OwnerCount)
                .Formula(NH_COUNT_QUERY)
                .Not.Insert()
                .Not.Update();

            DiscriminatorValue(typeof(NursingHomeCategory).Name);
        }

        private const string NH_COUNT_QUERY = @"(
                                                         SELECT count(nc.NursingHome_Id]) 
                                                         FROM [dbo].[NursingHomes_NursingHomeCategories] as nc
                                                         WHERE nc.[Category_Id] = Id
                                                      )";
    }
}
