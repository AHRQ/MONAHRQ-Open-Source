using System;
using System.Collections.Generic;
using Monahrq.Infrastructure.Data.Conventions;
using Monahrq.Infrastructure.Domain.Wings;

namespace Monahrq.Infrastructure.Entities.Domain.Wings
{
    [Serializable, EntityTableName("Wings_Elements")]
    public partial class Element : TargetOwnedWingItem<int>
    {
        const char delimiter = 'Ô';
        
        protected Element() : base() { }

        public Element(Target target, string name)
            : base(target, name) 
        {
            target.Elements.Add(this);
        }

        #region Properties

        public virtual Scope Scope { get; set; }

        public virtual bool IsRequired { get; set; }

        public virtual int Ordinal { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public virtual DataTypeEnum? Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is unique.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is unique; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsUnique { get; set; }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>
        /// The scale.
        /// </value>
        public virtual int? Scale { get; set; }

        /// <summary>
        /// Gets or sets the precision.
        /// </summary>
        /// <value>
        /// The percision.
        /// </value>
        public virtual int? Precision { get; set; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public virtual int? Length { get; set; }


        public virtual Wing DependsOn { get; set; }

        public virtual string Hints { get; set; }

        public virtual string LongDescription { get; set; }

        public virtual IEnumerable<string> MappingHints
        {
            get
            {
                var temp = Hints;
                if (string.IsNullOrEmpty((temp ?? string.Empty).Trim())) return new string[0];
                return temp.Split(new [] {delimiter});
            }
            set
            {
                Hints = string.Join(delimiter.ToString(), (value ?? new List<string>()));
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            Hints = string.Empty;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("[{0}] {1}", Ordinal, Description);
        }
        #endregion
    }
}