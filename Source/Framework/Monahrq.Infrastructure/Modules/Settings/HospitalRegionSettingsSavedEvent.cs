using Microsoft.Practices.Prism.Events;
using Monahrq.Infrastructure.Configuration;

namespace Monahrq.Sdk.Modules.Settings
{

    /// <summary>
    /// Published when Hopsital Region Settings are saved.
    /// Payload HospitalRegion settgins that were saved
    /// </summary>
    public class HospitalRegionSettingsSavedEvent : CompositePresentationEvent<HospitalRegionElement>{}

}
