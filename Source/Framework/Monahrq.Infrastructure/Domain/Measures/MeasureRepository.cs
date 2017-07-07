using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Infrastructure.Entities.Domain.Measures
{
    [Export]
    public class MeasureRepository: RepositoryBase<Measure, int>
    {

        public MeasureRepository()
        {
        
        }

        [Import]
        IDomainSessionFactoryProvider WingProvider
        {
            get;
            set;
        }

        protected override IDomainSessionFactoryProvider DomainSessionFactoryProvider
        {
            get { return WingProvider; }
            set { WingProvider = value as IDomainSessionFactoryProvider; }
        }
    }
}
