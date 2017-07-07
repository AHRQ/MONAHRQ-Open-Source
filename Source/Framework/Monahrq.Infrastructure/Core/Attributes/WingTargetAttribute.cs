using System;
using Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain.Wings.Repository;
using Version = Monahrq.Infrastructure.Domain.Wings.Version;

namespace Monahrq.Infrastructure.Core.Attributes
{
    /// <summary>
    /// Provides metadata for a Wing target (<see cref="DatasetRecord"/> implementation) to MONAHRQ
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class WingTargetAttribute : NamedDescriptionAttribute
    {
        /// <summary>
        /// Unique <see cref="Guid"/> that will be used to refer to this Wing target throughout MONAHRQ
        /// </summary>
        public Guid Guid { get; private set; }

        //todo: what is a reference target? none are defined
        public bool IsReferenceTarget { get; private set; }

        //todo: why is this redefined?
        public new string Name { get; private set; }

        /// <summary>
        /// Used to sort targets in the UI. Lower values are displayed first.
        /// </summary>
        public int DisplayOrder { get; private set; }

        public bool IsTrendingEnabled { get; private set; }

        /// <summary>
        /// Name of the entity publishing this target type
        /// </summary>
        /// <remarks>
        /// This value is not used in generated websites
        /// </remarks>
        public string PublisherName { get; set; }

        /// <summary>
        /// Contact email address of the entity publishing this target type
        /// </summary>
        /// <remarks>
        /// This value is not used in generated websites
        /// </remarks>
        public string PublisherEmail { get; set; }

        /// <summary>
        /// Website of the entity publishing this target type
        /// </summary>
        /// <remarks>
        /// This value is not used in generated websites
        /// </remarks>
        public string PublisherWebsite { get; set; }

        public WingTargetAttribute(string name, string targetGuid, string description, bool isReferenceTarget, bool isTrendingEnabled, int displayOrder = 0)
            : this(name, targetGuid, description, isReferenceTarget,displayOrder)
        {
            this.IsTrendingEnabled = isTrendingEnabled;
        }

        public WingTargetAttribute(string name, string targetGuid, string description, bool isReferenceTarget, int displayOrder = 0)
            : base(name, description)
        {
            this.Guid = Guid.Parse(targetGuid);
            this.Name = name;
            this.IsReferenceTarget = isReferenceTarget;
            this.DisplayOrder = displayOrder;
        }


        public WingTargetAttribute(string name, string targetGuid, bool isReferenceTarget)
            : this(name, targetGuid, name, isReferenceTarget)
        {
        }

        public Target CreateTarget(Wing wing, Guid guid, Type type, bool isTrendingEnabled, int displayOrder)
        {
            var result = TargetRepository.New(wing, guid, this.Name);
            result.Description = this.Description;
            result.ClrType = type.AssemblyQualifiedName;
            result.IsReferenceTarget = this.IsReferenceTarget;
            result.DisplayOrder = displayOrder;
            result.IsDisabled = false;
            result.Publisher = this.PublisherName ?? "Unknown";
            result.PublisherEmail = this.PublisherEmail ?? "";
            result.PublisherWebsite = this.PublisherWebsite ?? "";
            result.IsTrendingEnabled = isTrendingEnabled;
            var assemblyVersion = type.Assembly.GetName().Version;
            result.Version = new Version
            {
                Major = assemblyVersion.Major,
                Minor = assemblyVersion.Minor,
                Milestone = assemblyVersion.Revision
                //Number = string.Format("{0}.{1}.{2}", assemblyVersion.Major, assemblyVersion.Minor,
                //                       assemblyVersion.Revision)
            };
            return result;
        }
    }
}