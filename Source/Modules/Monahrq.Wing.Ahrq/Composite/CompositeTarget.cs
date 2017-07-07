using System;
using FluentNHibernate.Mapping;
using Monahrq.Infrastructure.Core.Attributes;
using Monahrq.Infrastructure.Domain.Data;
using Monahrq.Wing.Ahrq.Common;

namespace Monahrq.Wing.Ahrq.Composite
{
    /// <summary>
    /// Class for composite data.
    /// </summary>
    /// <seealso cref="Monahrq.Wing.Ahrq.Common.AhrqTarget" />
    [Serializable]
    [WingTarget("AHRQ-QI Composite Data", Constants.WingTargetGuid, "Mapping target for AHRQ-QI Composite Data", false, 3,
        PublisherName = "Agency for Healthcare Research and Quality (AHRQ)",
        PublisherEmail = "moanhrq@ahrq.gov",
        PublisherWebsite = "http://monahrq.ahrq.gov/")]
    public class CompositeTarget : AhrqTarget
    {}

    /// <summary>
    /// Class for composite map.
    /// </summary>
    /// <seealso cref="FluentNHibernate.Mapping.SubclassMap{CompositeTarget}" />
    [SubclassMappingProviderExport]
    public class CompositeTargetMap : SubclassMap<CompositeTarget>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeTargetMap"/> class.
        /// </summary>
        public CompositeTargetMap()
        {
            DiscriminatorValue("composite");
        }
    }
}
