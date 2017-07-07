using PropertyChanged;
using System;

namespace Monahrq.Infrastructure.Entities.Domain.Measures
{
    [Serializable]
    [ImplementPropertyChanged]
    public class TopicFacts
    {
        public string Name { get; set; }

        public string ImagePath { get; set; }

        public string Text { get; set; }

        public string CitationText { get; set; }

    }
}