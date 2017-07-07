using System;
using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Wing.Ahrq.Common;

namespace Monahrq.Wing.Ahrq.Provider
{
    /// <summary>
    /// Class for provider target.
    /// </summary>
    /// <seealso cref="AhrqTarget" />
    [Serializable]
    [WingTarget("AHRQ-QI Provider Data", Constants.WingTargetGuid, "Mapping target for AHRQ-QI Provider Data", false, 4,
        PublisherName = "Agency for Healthcare Research and Quality (AHRQ)",
        PublisherEmail = "moanhrq@ahrq.gov",
        PublisherWebsite = "http://monahrq.ahrq.gov/")]
    public class ProviderTarget : AhrqTarget
    {}

    /// <summary>
    /// Class for provider target map
    /// </summary>
    /// <seealso cref="FluentNHibernate.Mapping.SubclassMap{ProviderTarget}" />
    [SubclassMappingProviderExport]
    public class ProviderTargetMap : SubclassMap<ProviderTarget>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderTargetMap"/> class.
        /// </summary>
        public ProviderTargetMap()
        {
            DiscriminatorValue("provider");
        }
    }
}
