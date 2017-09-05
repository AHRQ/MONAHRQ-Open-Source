using System.Collections.Generic;

namespace Monahrq.Infrastructure.Domain.Websites
{
    public interface IWebsiteDependencyCheck
    {
        IEnumerable<ValidationResultViewModel> Check(Website currentWebsite);
    }
}
