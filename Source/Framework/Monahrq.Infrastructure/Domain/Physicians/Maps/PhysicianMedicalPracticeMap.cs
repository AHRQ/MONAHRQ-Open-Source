using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Infrastructure.Entities.Data.Strategies;
using Monahrq.Infrastructure.Entities.Domain.Maps;
using Monahrq.Infrastructure.Data.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Domain.Physicians.Maps
{

	[MappingProviderExport]
	public class PhysicianMedicalPraticeMap : EntityMap<PhysicianMedicalPractice, int, IdentityGeneratedKeyStrategy>
	{
	    /// <summary>
	    /// Initializes a new instance of the <see cref="PhysicianMedicalPraticeMap"/> class.
	    /// </summary>
	    public PhysicianMedicalPraticeMap()
	    {
	        var indexName = string.Format("IDX_{0}", EntityTableName);


			References(x => x.Physician, "Physician_Id")
                             .ForeignKey("FK_PMP_Physicians")
							 .Cascade.None()
							 .Nullable()
							 .Not.LazyLoad();

	        References(x => x.MedicalPractice, "MedicalPractice_Id")
                             .ForeignKey("FK_PMP_MedicalPractices")
                             .Cascade.None()
	                         .Nullable()
	                         .Not.LazyLoad();

			Map(x => x.AssociatedPMPAddresses).CustomType<IntListToStringType>()
								.Nullable();			

	        Map(x => x.Version).Index(indexName);
	    }

	    public override string EntityTableName
	    {
	        get
	        {
	            return "Physicians_MedicalPractices";
	        }
	    }

	    protected override PropertyPart NameMap()
	    {
	        return null;
	    }
	}
}
