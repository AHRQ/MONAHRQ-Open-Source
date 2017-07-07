using System.ComponentModel;

namespace Monahrq.Websites.Model
{
    public enum WebsiteAudienceTypes
    {
        [Description("Please select Audience")]
        PleaseSelectAudience, 
        [Description("Consumers")]
        Consumers, 
        [Description("All Audiences")]
        AllAudiences
    }
}