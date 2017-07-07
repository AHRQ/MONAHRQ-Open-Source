using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Monahrq.Infrastructure.Data.Conventions;

namespace Monahrq.Infrastructure.Entities.Domain.Wings
{

    [Serializable, EntityTableName("Wings")]
    public partial class Wing
    {
        //private IList<Target> InternalTargets { get; set; }

        [XmlIgnore]
        public virtual IList<Target> Targets
        {
            get; set;
        }

        public virtual Guid WingGUID { get; set; }

		public DateTime? LastWingUpdate { get; set; }


		protected override void Initialize()
        {
            base.Initialize();
            Targets = new List<Target>();
        }

        public Wing()
            : base()
        {

        }

        public Wing(string name)
            : base(name)
        {
        }
        //static readonly Wing _null;
        public static Wing Null
        {
            get { return new Wing("<<NULL>>") {WingGUID = Guid.Empty, Description = "<<NULL>>"}; }
        }
    }
}
