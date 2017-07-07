using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Data.Conventions;

namespace Monahrq.Infrastructure.Entities.Domain.Wings
{
    [Serializable, EntityTableName("Wings_Generators")]
    public partial class Generator
    {
        public string Author { get; set; }
    }
}
