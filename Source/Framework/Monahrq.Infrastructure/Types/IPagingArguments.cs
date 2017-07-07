using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Types
{
    public interface IPagingArguments : INotifyPropertyChanged
    {
        int PageIndex { get; set; }
        int PageSize { get; set; }
        int TotalPages { get; }
        int? RowsCount { get; set; }
        PagingSortingColumns SortingColumns { get; set; }
        Action PagingFunction { get; set; }
        bool AllRecordsRequested { get; }
        bool IsFirstPage { get; }
        bool IsLastPage { get; }

    }
}
