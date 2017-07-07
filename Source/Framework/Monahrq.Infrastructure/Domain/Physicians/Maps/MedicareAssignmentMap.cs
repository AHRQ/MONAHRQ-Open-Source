using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;

namespace Monahrq.Infrastructure.Domain.Physicians.Maps
{
    public class MedicareAssignmentMap : EntityMap<MedicareAssignment, int, IdentityGeneratedKeyStrategy>
    {
        public MedicareAssignmentMap()
        {
            var indexName = string.Format("IDX_{0}", EntityTableName);

            Map(x => x.AcceptsMedicareAssignment).Index(indexName).Length(2).Nullable();
            Map(x => x.ParticipatesInERX).Nullable().Index(indexName);
            Map(x => x.ParticipatesInPQRS).Nullable().Index(indexName);
            Map(x => x.ParticipatesInEHR).Nullable().Index(indexName);
            Map(x => x.Version).Index(indexName);
        }
    }

    [MappingProviderExport]
    public class PhysicianMedicareAssignmentMap : EntityMap<PhysicianMedicareAssignment, int, IdentityGeneratedKeyStrategy>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicianMedicareAssignmentMap"/> class.
        /// </summary>
        public PhysicianMedicareAssignmentMap()
        {
            References(x => x.Assignment, "MedicareAssignment_Id")
                             .Cascade.SaveUpdate()
                             .Not.Nullable()
                             .Not.LazyLoad();

            Map(x => x.Version);
        }

        public override string EntityTableName
        {
            get
            {
                return "Physicians_MedicareAssignments";
            }
        }

        protected override PropertyPart NameMap()
        {
            return null;
        }

    }
}
