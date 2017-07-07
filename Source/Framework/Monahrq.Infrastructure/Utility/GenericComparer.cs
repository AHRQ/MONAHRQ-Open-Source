using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Utility
{
    public class GenericComparer<TContainer, TElement> : IComparer<TContainer>, IComparer
        where TContainer : class
        where TElement : IComparable
    {
        public GenericComparer(Func<TContainer, TElement> getter)
        {
            this.getter = getter;
        }

        private readonly Func<TContainer, TElement> getter;

        public int Compare(TContainer x, TContainer y)
        {
            if (x == null && y == null)
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;

            var ax = this.getter(x);
            var ay = this.getter(y);
            if (ax == null && ay == null)
                return 0;
            if (ax == null)
                return -1;
            if (ay == null)
                return 1;

            return ax.CompareTo(ay);
        }

        public int Compare(object x, object y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;
            var ax = x as TContainer;
            var ay = y as TContainer;
            return this.Compare(ax, ay);
        }
    }

    public class GenericComparer<T> : IComparer<T>, IComparer
        where T : class
    {
        public GenericComparer(Func<T, T, int> comparisonFunc)
        {
            this.comparisonFunc = comparisonFunc;
        }

        private readonly Func<T, T, int> comparisonFunc;

        public int Compare(T x, T y)
        {
            if (x == null && y != null)
                return -1;
            if (y == null && x != null)
                return 1;
            return this.comparisonFunc(x, y);
        }

        public int Compare(object x, object y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;
            var ax = x as T;
            var ay = y as T;
            return this.Compare(ax, ay);
        }
    }
}
