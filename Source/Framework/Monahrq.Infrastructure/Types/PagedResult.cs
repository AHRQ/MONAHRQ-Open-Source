using System;
using System.Collections.Generic;
using System.Linq;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;

namespace Monahrq.Infrastructure.Types
{
    ///<summary>
    ///</summary>
    public interface IPagedList<T> : IEnumerable<T>
    {
        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        /// <value>The total count.</value>
        int TotalCount
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the total pages.
        /// </summary>
        /// <value>The total pages.</value>
        int PageCount
        {
            get;
        }
        /// <summary>
        /// Gets or sets the index of the page.
        /// </summary>
        /// <value>The index of the page.</value>
        int PageIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        int PageSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is previous page.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is previous page; otherwise, <c>false</c>.
        /// </value>
        bool IsPreviousPage
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is next page.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is next page; otherwise, <c>false</c>.
        /// </value>
        bool IsNextPage
        {
            get;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class PagedList<T> : ObservableList<T>, IPagedList<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        public PagedList(IList<T> items, int pageIndex, int pageSize)
        {
            PageSize = pageSize;
            TotalCount = items.Count;
            if (pageSize > 0)
            {
                PageCount = (int)Math.Ceiling(TotalCount / (double)pageSize);
            }
            else
            {
                PageCount = 0;
            }
            //check to see if page index exceeds
            pageIndex = pageIndex > PageCount ? PageCount : pageIndex;
            PageIndex = pageIndex;
            StartRecordIndex = (PageIndex - 1) * PageSize + 1;
            EndRecordIndex = TotalCount > pageIndex * pageSize ? pageIndex * pageSize : TotalCount;
            for (int i = StartRecordIndex - 1; i < EndRecordIndex; i++)
            {
                Add(items[i]);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="totalItemCount">The total item count.</param>
        public PagedList(IEnumerable<T> items, int pageIndex, int pageSize, int totalItemCount)
        {
            AddRange(items.ToList());
            TotalCount = totalItemCount;
            if (pageSize > 0)
            {
                PageCount = (int)Math.Ceiling(totalItemCount / (double)pageSize);
            }
            else
            {
                PageCount = 0;
            }
            //check to see if page index exceeds
            pageIndex = pageIndex > PageCount ? PageCount : pageIndex;
            PageIndex = pageIndex;
            PageSize = pageSize;
            StartRecordIndex = (pageIndex - 1) * pageSize + 1;
            EndRecordIndex = TotalCount > pageIndex * pageSize ? pageIndex * pageSize : totalItemCount;
        }

        /// <summary>
        /// Gets or sets the index of the page.
        /// </summary>
        /// <value>The index of the page.</value>
        public int PageIndex { get; set; }
        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        public int PageSize { get; set; }
        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        /// <value>The total count.</value>
        public int TotalCount { get; set; }
        /// <summary>
        /// Gets or sets the page count.
        /// </summary>
        /// <value>The page count.</value>
        public int PageCount { get; private set; }
        /// <summary>
        /// Gets or sets the start index of the record.
        /// </summary>
        /// <value>The start index of the record.</value>
        public int StartRecordIndex { get; private set; }
        /// <summary>
        /// Gets or sets the end index of the record.
        /// </summary>
        /// <value>The end index of the record.</value>
        public int EndRecordIndex { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is previous page.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is previous page; otherwise, <c>false</c>.
        /// </value>
        public bool IsPreviousPage
        {
            get
            {
                return (PageIndex > 0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is next page.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is next page; otherwise, <c>false</c>.
        /// </value>
        public bool IsNextPage
        {
            get
            {
                return (PageIndex * PageSize) <= TotalCount;
            }
        }
    }
}