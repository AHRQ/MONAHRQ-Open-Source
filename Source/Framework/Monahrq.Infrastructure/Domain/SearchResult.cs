using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.Domain
{
    public class SearchResult<TDomainType, TKey> : ObservableCollection<TDomainType>
         where TDomainType : IEntity<TKey>
    {
        public SearchResult()
        {
        }
        public SearchResult(IEnumerable<TDomainType> items)
            : base(items)
        {
        }
    }
}
