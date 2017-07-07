using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Entities.Domain;
using PropertyChanged;

namespace Monahrq.Infrastructure.Entities.Domain.BaseData
{

    [ImplementPropertyChanged]
    public class ReportingQuarter : Entity<int>
    {
    }


    namespace ViewModel
    {
        [ImplementPropertyChanged]
        public class ReportingQuarterViewModel : Entity<int>
        {
        }
    }


}
