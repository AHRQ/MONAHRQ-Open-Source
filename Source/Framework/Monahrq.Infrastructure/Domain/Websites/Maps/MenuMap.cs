using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Data.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Domain.Websites.Maps
{
    public class MenuMap : ClassMap<Menu>
    {
        public MenuMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.Product).Length(40);
            Map(x => x.Name).Length(40);
            Map(x => x.Type).Length(40);
            Map(x => x.Label).Length(200);
            Map(x => x.Priority);
            Map(x => x.Entity).Length(50);
            Map(x => x.Classes).CustomType<StringListToStringType>().Length(200);
            Map(x => x.DataSets).CustomType<StringListToStringType>().Length(200);
            Map(x => x.Routes).CustomType<JsonToStringType<RouteInfo>>().Nullable();
            Map(x => x.Target).Nullable();

            HasMany(x => x.SubMenus)
                .KeyColumn("ParentId")
                .Inverse()
                .ForeignKeyConstraintName("SubMenuConstraint")
                .AsBag().NotFound.Ignore()
                .Cascade.AllDeleteOrphan()
                .Not.LazyLoad();

            References(x => x.Owner)
                .Nullable()
                .Column("ParentId");

            SelectBeforeUpdate();
        }
    }
}
