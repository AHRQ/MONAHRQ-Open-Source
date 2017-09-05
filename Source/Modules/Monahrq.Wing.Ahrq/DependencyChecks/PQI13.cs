using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using Monahrq.Wing.Ahrq.Area;
using Monahrq.Wing.Ahrq.Common;
using NHibernate.Linq;

namespace Monahrq.Wing.Ahrq.DependencyChecks
{
    [Export(typeof(IWebsiteDependencyCheck))]
    public class PQI13 : IWebsiteDependencyCheck
    {
        [Import]
        private IDomainSessionFactoryProvider dataserviceProvider;

        public IEnumerable<ValidationResultViewModel> Check(Website currentWebsite)
        {
            // make sure PQI 13 is a selected measure in this website
            if (currentWebsite.Measures.All(m => m.OriginalMeasure.MeasureCode != "PQI 13"))
                yield break;

            // and that no data has been imported for PQI 13
            using (var session = this.dataserviceProvider.SessionFactory.OpenStatelessSession())
            {
                var foundAny = false;
                foreach (var datasetId in currentWebsite
                    .Datasets
                    .Where(ds => ds.Dataset.ContentType.Measures.Any(m => m.MeasureCode == "PQI 13"))
                    .Select(ds => ds.Dataset.Id))
                {
                    if (session.QueryOver<AhrqTarget>()
                            .Where(t => t.Dataset.Id == datasetId && t.MeasureCode == "PQI 13")
                            .RowCount() > 0)
                    {
                        foundAny = true;
                        break;
                    }
                }
                if (!foundAny)
                    yield break;
            }

            yield return new ValidationResultViewModel
            {
                CompositionArea = WebsiteTabViewModels.Measures,
                HelpTopic = "1051",
                HelpText = "Click here for more information",
                Message = Messages.PQI13,
                Quality = ValidationLevel.Warning
            };
        }
    }
}
