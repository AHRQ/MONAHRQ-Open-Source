using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Domain.Websites
{
    public enum WebsiteState
    {
        Initialized,
        HasDatasources,
        HasMeasures,
        HasReports,
        CompletedDependencyCheck,
        Generating,
        Generated,
        Published,
        PublishCancelled
    }
}
