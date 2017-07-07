using System;
using System.Data;
using Monahrq.Infrastructure.Utility;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace Monahrq.Infrastructure.Data
{
    public class XmlToObjectType<T> : IUserType //, IParameterizedType
        where T : class, new()
    {
        #region Equals member

        bool IUserType.Equals(object x, object y)
        {
            return (x == y) || ((x != null) && x.Equals(y));
        }

        #endregion

        #region IUserType Members

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object DeepCopy(object value)
        {
            return value == null ? null : CloneHelper.Clone(value as T);
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public bool IsMutable
        {
            get { return true; }
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            Int32 index = rs.GetOrdinal(names[0]);
            if (rs.IsDBNull(index))
            {
                return null;
            }

            return XmlHelper.Deserialize<T>((string)rs[index]);
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            if (value == null || value == DBNull.Value)
            {
                NHibernateUtil.String.NullSafeSet(cmd, null, index);
            }
            else
            {
                var xml = XmlHelper.Serialize(value);
                NHibernateUtil.String.Set(cmd, xml.OuterXml, index);
            }
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public Type ReturnedType
        {
            get { return typeof(T); }
        }

        public SqlType[] SqlTypes
        {
            get { return new SqlType[] { new XmlSqlType() }; }
        }

        #endregion

        //#region IParameterizedType Members

        //private string _knownTypesStr;
        //private List<Type> _knowsTypes;

        //public void SetParameterValues(System.Collections.IDictionary parameters)
        //{
        //    _knownTypesStr = (string)parameters["knowntypes"];

        //    if (string.IsNullOrEmpty(_knownTypesStr)) return;

        //    _knowsTypes = new List<Type>();
        //    foreach (string str in _knownTypesStr.Split(';'))
        //    {
        //        Type t = Type.GetType(str, false);
        //        if (t != null)
        //            _knowsTypes.Add(t);
        //    }
        //}

        //#endregion
    }
}
