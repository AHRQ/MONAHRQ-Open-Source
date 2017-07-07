using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Types
{
    public class PagingSortingColumn
    {
        public ListSortDirection? SortDirection { get; set; }
        public string SortMemberPath { get; set; }
    }
}
