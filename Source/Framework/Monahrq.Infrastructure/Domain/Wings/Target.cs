using System.ComponentModel.Composition;
using System.Xml.Serialization;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Domain.Wings;
using Monahrq.Infrastructure.Entities.Domain.Measures;
using System;
using System.Collections.Generic;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.Wings
{
    [ImplementPropertyChanged] 
    [Serializable, EntityTableName("Wings_Targets")]
    public class Target : WingOwnedWingItem<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Target"/> class.
        /// </summary>
        public Target()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Target"/> class.
        /// </summary>
        /// <param name="wing">The wing.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="name">The name.</param>
        public Target(Wing wing, Guid guid, string name)
            : base(wing, name)
        {
            Guid = guid;

            if (Owner != null)
                Owner.Targets.Add(this);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            Elements = new List<Element>();
            Measures = new List<Measure>();
            Scopes = new List<Scope>();
        }
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public virtual Guid Guid { get; set; }

        
        public virtual string DisplayName
        {
            get { return IsCustom && IsDisabled ? string.Format("{0} - Disabled", Name) : Name; }
            set { Name = value; }
        }

        /// <summary>
        /// Gets or sets the type of the color.
        /// </summary>
        /// <value>
        /// The type of the color.
        /// </value>
        public virtual string ClrType { get; set; }
        /// <summary>
        /// Gets or sets the internal scopes.
        /// </summary>
        /// <value>
        /// The internal scopes.
        /// </value>
        [XmlIgnore]
        private IList<Scope> InternalScopes { get; set; }
        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>
        /// The scopes.
        /// </value>
        [XmlIgnore]
        public virtual IList<Scope> Scopes
        {
            get
            {
                return ReferenceEquals(this, Null) ?
                    new List<Scope>()
                    : InternalScopes;
            }
            set
            {
                InternalScopes = value;
            }
        }
        /// <summary>
        /// Gets or sets the elements.
        /// </summary>
        /// <value>
        /// The elements.
        /// </value>
        [XmlIgnore]
        public virtual IList<Element> Elements
        {
            get;
            set;
        }
        /// <summary>
        /// Gets the measures.
        /// </summary>
        /// <value>
        /// The measures.
        /// </value>
        [XmlIgnore]
        public virtual IList<Measure> Measures { get; private set; }

        static readonly Target _null = new Target(Wing.Null, Guid.Empty, "<<NULL>>") {  Description = "<<NULL>>" };

        /// <summary>
        /// Gets the null.
        /// </summary>
        /// <value>
        /// The null.
        /// </value>
        [XmlIgnore]
        public static Target Null
        {
            get
            {
                return _null;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is reference target.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is reference target; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsReferenceTarget { get; set; }
        /// <summary>
        /// Gets or sets the display order.
        /// </summary>
        /// <value>
        /// The display order.
        /// </value>
        public virtual int DisplayOrder { get; set; }
        /// <summary>
        /// Gets or sets the name of the database schema.
        /// </summary>
        /// <value>
        /// The name of the database schema.
        /// </value>
        public virtual string DbSchemaName { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is custom.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is custom; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsCustom { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is Trending Enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is Trending Enabled; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsTrendingEnabled { get; set; }
        /// <summary>
        /// Gets or sets the import SQL script.
        /// </summary>
        /// <value>
        /// The import SQL script.
        /// </value>
        public string ImportSQLScript { get; set; }
        /// <summary>
        /// Gets or sets the create SQL.
        /// </summary>
        /// <value>
        /// The create SQL.
        /// </value>
        public string CreateSqlScript { get; set; }
        /// <summary>
        /// Gets or sets the add meauser SQL.
        /// </summary>
        /// <value>
        /// The create SQL.
        /// </value>
        public string AddMeausersSqlScript { get; set; }
        /// <summary>
        /// Gets or sets the add Reports SQL.
        /// </summary>
        /// <value>
        /// The create SQL.
        /// </value>
        public string AddReportsSqlScript { get; set; }
        /// <summary>
        /// Gets or sets the allow multiple imports.
        /// </summary>
        /// <value>
        /// The allow multiple imports.
        /// </value>
        public bool AllowMultipleImports { get; set; }
        /// <summary>
        /// Gets or sets the wizard steps.
        /// </summary>
        /// <value>
        /// The wizard steps.
        /// </value>
        public DynamicStepTypeEnum? ImportType { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is disabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disabled; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsDisabled { get; set; }
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public virtual Infrastructure.Domain.Wings.Version Version { get; set; }
        /// <summary>
        /// Gets or sets the publisher.
        /// </summary>
        /// <value>
        /// The publisher.
        /// </value>
        public virtual string Publisher { get; set; }
        /// <summary>
        /// Gets or sets the publisher email.
        /// </summary>
        /// <value>
        /// The publisher email.
        /// </value>
        public virtual string PublisherEmail { get; set; }
        /// <summary>
        /// Gets or sets the publisher website.
        /// </summary>
        /// <value>
        /// The publisher website.
        /// </value>
        public virtual string PublisherWebsite { get; set; }

        /// <summary>
        /// Gets or sets the wing target XML file path.
        /// </summary>
        /// <value>
        /// The wing target XML file path.
        /// </value>
        public string WingTargetXmlFilePath { get; set; }

        /// <summary>
        /// Gets or sets the name of the template file.
        /// </summary>
        /// <value>
        /// The name of the template file.
        /// </value>
        public string TemplateFileName { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has associated template.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has associated template; otherwise, <c>false</c>.
        /// </value>
        public bool HasAssociatedTemplate { get { return !string.IsNullOrEmpty(TemplateFileName); } }
    }

    public interface ITargetRepository : IRepository<Target, int>
    {
    }

    namespace Repository
    {
        [Export(typeof(ITargetRepository))]
        public partial class TargetRepository : RepositoryBase<Target, int>,
                 ITargetRepository
        {
            [Import(typeof(ISessionFactoryProvider))]
            protected override IDomainSessionFactoryProvider DomainSessionFactoryProvider { get; set; }
        }

    }
}
