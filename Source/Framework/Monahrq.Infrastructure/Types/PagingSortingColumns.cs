using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.ComponentModel;

namespace Monahrq.Infrastructure.Types
{
    public class PagingSortingColumns : List<PagingSortingColumn>
    {
        public void SetSortList(IEnumerable<DataGridColumn> Columns, DataGridColumn nextColumn)
        {
            this.Clear();
            //Save previously sorted columns
            foreach (var col in Columns.Where(c => c.SortDirection != null))
            {
                var sortDescription = new PagingSortingColumn();
                sortDescription.SortDirection = col.SortDirection;
                sortDescription.SortMemberPath = col.SortMemberPath;
                this.Add(sortDescription);
            }
            //Save column that is being sorted
            var alreadySorted = this.Where(s => s.SortMemberPath == nextColumn.SortMemberPath).Any();
            //if column already sorted toggle the direction...
            if (alreadySorted)
            {
                var sDesc = this.Where(s => s.SortMemberPath == nextColumn.SortMemberPath).First();
                sDesc.SortDirection = ToggleSortDirection(sDesc.SortDirection);

            }
            //otherwise add column with an ascending direction
            else {
                var sDesc = new PagingSortingColumn();
                sDesc.SortMemberPath = nextColumn.SortMemberPath;
                sDesc.SortDirection = ListSortDirection.Ascending;
                this.Add(sDesc);
            }
        }
        public ListSortDirection? ToggleSortDirection(ListSortDirection? sortDirection)
        {
            if (sortDirection == ListSortDirection.Ascending) return ListSortDirection.Descending;
            else return ListSortDirection.Ascending;
        }


    }
}
