using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NHibernate;
using NHibernate.Transform;

namespace Monahrq.Infrastructure.Data.Transformers
{
    public class TupleToPropertyResultTransformer : IResultTransformer
    {
        private readonly Type _result;
        private readonly PropertyInfo[] _properties;

        public TupleToPropertyResultTransformer(Type result, params string[] names)
        {
            _result = result;
            _properties = names.Select(result.GetProperty).ToArray();
        }

        /// <summary>
        /// </summary>
        /// <param name="tuple"></param>
        /// <param name="aliases"></param>
        /// <returns></returns>
        public object TransformTuple(object[] tuple, string[] aliases)
        {
            var instance = Activator.CreateInstance(_result);
            for (var i = 0; i < tuple.Length; i++)
            {
                _properties[i].SetValue(instance, tuple[i], null);
            }
            return instance;
        }

        /// <summary>
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public IList TransformList(IList collection)
        {
            return collection;
        }
    }

    /// <summary>
    /// Result transformer that allows to transform a result
    /// to an specific suplied <see cref="TEntity"/> with a 
    /// custom transform function based on a tuple that returns a <see cref="TEntity"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// IList result = s.CreateQuery(select f.Name, f.Description from Foo f)
    ///                     .SetResultTransformer(new DelegateTransformer[Foo](t => new Foo(t[0], t[1]))
    ///                     .List[Foo]();
    /// 
    /// NoFoo dto = result[0];
    /// </code>
    /// </example>
    /// <remarks>
    /// If you have a <see cref="ICriteria"/> or a <see cref="IQuery"/> with aliases you can use
    /// <see cref="NHibernate.Transform.AliasToBeanResultTransformer"/> class.
    /// </remarks>
    [Serializable]
    public class DelegateTransformer<TEntity> : IResultTransformer
    {
        private readonly Func<object[], TEntity> _transformFunction;

        public DelegateTransformer(Func<object[], TEntity> transformFunction)
        {
            if (transformFunction == null) throw new ArgumentNullException("transformFunction");

            _transformFunction = transformFunction;
        }

        #region IResultTransformer Members

        public IList TransformList(IList collection)
        {
            return collection;
        }

        public object TransformTuple(object[] tuple, string[] aliases)
        {
            return _transformFunction(tuple);
        }

        #endregion
    }
}
