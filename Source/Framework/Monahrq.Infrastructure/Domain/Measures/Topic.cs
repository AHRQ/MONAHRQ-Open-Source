using System;
using System.Collections.Generic;
using System.ComponentModel;
using Monahrq.Infrastructure.Domain.Common;

namespace Monahrq.Infrastructure.Entities.Domain.Measures
{
    [Serializable]
    public class Topic : OwnedEntity<TopicCategory, int, int>
    {
        protected override void Initialize()
        {
            base.Initialize();
            Measures = new List<Measure>();
        }


        public Topic()
            : base()
        {
        }

        public Topic(TopicCategory owner, string name)
            : base(owner)
        {
            Name = name;
            owner.Topics.Add(this);
        }

        public virtual IList<Measure> Measures { get; private set; }
        public virtual string LongTitle { get; set; }
        public virtual string Description { get; set; }
        public virtual string WingTargetName { get; set; }
        public virtual bool IsUserCreated { get; set; }
        public virtual string ConsumerLongTitle { get; set; }
    }

    [Serializable]
    public class TopicCategory : Entity<int>
    {
        protected override void Initialize()
        {
            base.Initialize();
            Topics = new List<Topic>();
            CategoryType = TopicCategoryTypeEnum.Topic;
            Facts = new List<TopicFacts>();
            TopicFacts1 = "";
            TopicFacts2 = "";
            TopicFacts3 = "";
            TipsChecklist = "";
            TopicIcon = "";
        }

        protected TopicCategory()
            : base()
        {
			Facts = new List<TopicFacts>();
		}

        public TopicCategory(string name)
            : base()
        {
            Name = name;
			Facts = new List<TopicFacts>();
		}

		public IList<Topic> Topics { get; private set; }

        public virtual string LongTitle { get; set; }

        public virtual string Description { get; set; }

        public TopicTypeEnum? TopicType { get; set; }

        public virtual string WingTargetName { get; set; }

        public TopicCategoryTypeEnum? CategoryType { get; set; }

        public virtual bool IsUserCreated { get; set; }

        public virtual string ConsumerDescription { get; set; }

        public DateTime? DateCreated { get; set; }

        public List<TopicFacts> Facts { get; set; }


        public virtual string TopicFacts1 { get; set; }
        public virtual string TopicFacts2 { get; set; }
        public virtual string TopicFacts3 { get; set; }
        public virtual string TipsChecklist { get; set; }
        public virtual string TopicIcon { get; set; }
    }
}


