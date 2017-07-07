using System;
using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Wing.Ahrq.Common;

namespace Monahrq.Wing.Ahrq.Area
{
    /// <summary>
    /// Class for Area target.
    /// </summary>
    /// <seealso cref="Monahrq.Wing.Ahrq.Common.AhrqTarget" />
    [Serializable]
    [WingTarget("AHRQ-QI Area Data", Constants.WingTargetGuid, "Mapping target for AHRQ-QI Area Data", false, 2,
        PublisherName = "Agency for Healthcare Research and Quality (AHRQ)",
        PublisherEmail = "moanhrq@ahrq.gov",
        PublisherWebsite = "http://monahrq.ahrq.gov/")]
    public class AreaTarget : AhrqTarget
    {}

    /// <summary>
    /// Class for Area Target map.
    /// </summary>
    /// <seealso cref="FluentNHibernate.Mapping.SubclassMap{AreaTarget}" />
    [SubclassMappingProviderExport]
    public class AreaTargetMap : SubclassMap<AreaTarget>
    {
        public AreaTargetMap()
        {
            DiscriminatorValue("area");
        }
    }
}
