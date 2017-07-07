using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace Monahrq.Infrastructure.Domain.Maps
{
    public static class MappingExtensions
    {
        public static ManyToOnePart<TParent> CustomForeignKey<TParent>(this ManyToOnePart<TParent> parentReference, Type childType)
        {
            var name = string.Format("FK_{0}_child_to_{1}_Id", childType.Name, typeof(TParent).Name);
            return parentReference.ForeignKey(name);
        }
    }
}
