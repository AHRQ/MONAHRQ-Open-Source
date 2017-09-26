using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Domain.Websites;

namespace Monahrq.Wing.Ahrq.DependencyChecks
{
    [Export(typeof(IWebsiteDependencyCheck))]
    public class RetiredIndicators : IWebsiteDependencyCheck
    {

        private static readonly string[] obsoleteMeasures = new[]
        {
            "IQI 21",
            "IQI 22",
            "IQI 23",
            "IQI 24",
            "IQI 25",
            "IQI 33",
            "IQI 34",
            "PSI 17",
            "PSI 18",
            "PSI 19"
        };

        public IEnumerable<ValidationResultViewModel> Check(Website currentWebsite)
        {
            if (!currentWebsite.Measures.Any(m => obsoleteMeasures.Contains(m.OriginalMeasure.MeasureCode)))
                yield break;

            yield return new ValidationResultViewModel
            {
                CompositionArea = WebsiteTabViewModels.Measures,
                HelpTopic = "1051",
                HelpText = "Click here for more information",
                Message = Messages.RetiredIndicators,
                Quality = ValidationLevel.Warning
            };
        }
    }
}
