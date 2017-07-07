using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using NHibernate.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Monahrq.Infrastructure.Types
{

    [ImplementPropertyChanged]
    public class PagingResults<T> : ListCollectionView, IPagingArguments
    {
        #region Fields and Constants

        private int _pageIndex;
        private int _pageSize;
        private int? _rowsCount;
        private PagingSortingColumns _sortingColumns = new PagingSortingColumns();

        #endregion

        #region Constructors

        public PagingResults() : base(new List<T>()) { }

        public PagingResults(IList list) : base(list) { }

        public PagingResults(IList list, IPagingArguments pagingArguments) : base(list)
        {
            this.SetPagingArguments(pagingArguments);
        }

        #endregion

        #region Properties

        public int PageIndex
        {
            get
            {
                //If page size changed the page index may exceed the total number of pages
                if (_pageIndex > TotalPages) return Math.Max(TotalPages, 1);
                else return _pageIndex;
            }
            set
            {
                if (value == _pageIndex) return;
                if (this.RowsCount == null) throw new Exception("PageIndex can not be set prior to RowsCount");
                //Page's index can not bet set to a value less than 1 or..
                if (value <= 0) value = 1;
                //..greater than the total number of pages
                if (value > TotalPages) value = Math.Max(TotalPages, 1);
                _pageIndex = value;
                //SetProperty(ref _pageIndex, value);
            }
        }

        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                if (value == _pageSize) return;
                //if (value <= 0) value = 10;
                if (value <= 0)
                {
                    //Consider an invalid size as a request for all records in one page
                    if (RowsCount != null && RowsCount > 0) value = (int)RowsCount;
                    else value = 50; //Coerce PageSize to 50 records' page
                }
                _pageSize = value;
            }
        }

        public int TotalPages
        {
            get
            {
                if (PageSize == 0) throw new Exception("PageSize must be set prior to accessing TotalPages");
                var rawTotalPages = (double)RowsCount / (double)PageSize;
                //Round up to next integer value
                var result = (int)Math.Ceiling(rawTotalPages);
                return result;
            }
        }

        public int? RowsCount
        {
            get { return _rowsCount; }
            set
            {
                if (value == _rowsCount) return;
                _rowsCount = value;
            }
        }

        public PagingSortingColumns SortingColumns
        {
            get { return _sortingColumns; }
            set
            {
                //SetProperty(ref _sortingColumns, value); 
                _sortingColumns = value;
            }
        }

        public Action PagingFunction { get; set; }

        public bool AllRecordsRequested
        {
            get
            {
                if (RowsCount == null) return true;
                if (PageSize == RowsCount) return true;
                else return false;
            }
        }

        public bool IsFirstPage
        {
            get
            {
                var isFirstPage = PageIndex == 1;
                return isFirstPage;
            }
        }

        public bool IsLastPage
        {
            get
            {
                var isLastPage = TotalPages == PageIndex;
                return isLastPage;
            }
        }


        #endregion

        #region Methods

        public void SetPagingArguments(IPagingArguments pagingArguments)
        {
            _rowsCount = pagingArguments.RowsCount;
            _pageSize = pagingArguments.PageSize;
            _pageIndex = pagingArguments.PageIndex;
            _sortingColumns = pagingArguments.SortingColumns;
            PagingFunction = pagingArguments.PagingFunction;
        }

        public void SetPagingArguments(int rowsCount, int pageSize, int pageIndex, Action pagingFunction = null, PagingSortingColumns sortingColumns = null)
        {
            RowsCount = rowsCount;
            PageSize = pageSize;
            PageIndex = pageIndex;
            if (pagingFunction != null) PagingFunction = pagingFunction;
            if (sortingColumns != null) SortingColumns = sortingColumns;
        }

        public static IQueryable<T> ApplySortExpressions(IQueryable<T> customersQuery, PagingSortingColumns sortingColumns)
        {
            bool isFirst = true;
            IOrderedQueryable<T> ordCustomersQuery = null;
            if (sortingColumns != null)
            {
                foreach (var sortCol in sortingColumns)
                {
                    var param = Expression.Parameter(typeof(T), sortCol.SortMemberPath);
                    var mySortExpression = Expression.Lambda<Func<T, String>>(Expression.Property(param, sortCol.SortMemberPath), param);
                    ordCustomersQuery = customersQuery as IOrderedQueryable<T>;
                    if (isFirst)
                    {
                        if (sortCol.SortDirection == ListSortDirection.Ascending)
                            customersQuery = customersQuery.OrderBy(mySortExpression);
                        else
                            customersQuery = customersQuery.OrderByDescending(mySortExpression);
                        isFirst = false;
                    }
                    else {
                        if (sortCol.SortDirection == ListSortDirection.Ascending)
                            ordCustomersQuery = ordCustomersQuery.ThenBy(mySortExpression);
                        else
                            ordCustomersQuery = ordCustomersQuery.ThenByDescending(mySortExpression);
                    }

                }
            }
            if (sortingColumns.Count < 2) return customersQuery;
            else return ordCustomersQuery;
        }

        #endregion
    }
}
