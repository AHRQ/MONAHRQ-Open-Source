using System.Windows.Forms.DataVisualization.Charting;
using Monahrq.Infrastructure.Data.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Services;
using Monahrq.Sdk.Utilities;
using NHibernate.Properties;

namespace Monahrq.Infrastructure.Utility
{
    public static class TypeUtlity
    {
        public static string EntityTableName<T>()
        {
            return typeof(T).EntityTableName();
        }

        public static string EntityTableName(this Type entityType)
        {
            var entityTableName = Inflector.Pluralize(entityType.Name);
            //var entityTableNameAttrib = entityType.GetCustomAttributes(typeof (EntityTableNameAttribute), true).FirstOrDefault() as EntityTableNameAttribute;
            var entityTableNameAttrib = ResolveAttribute(entityType);

            if (entityTableNameAttrib != null)
            {
                entityTableName = entityTableNameAttrib.TableName;
            }
            else
            {
                if (entityType.Namespace.EqualsIgnoreCase("Monahrq.Infrastructure.Entities.Domain.BaseData"))
                {
                    entityTableName = "Base_" + entityTableName;
                }

                if (entityType.Namespace.EqualsIgnoreCase("Monahrq.Infrastructure.Entities.Domain.Hospitals") &&
                    (entityTableName != "Hospitals"))
                {
                    entityTableName = "Hospitals_" + entityTableName;
                }

                if (entityType.Namespace.EqualsIgnoreCase("Monahrq.Infrastructure.Domain.ClinicalDimensions"))
                {
                    entityTableName = "ClinicalDimensions_" + entityTableName;
                }

                if (!entityTableName.StartsWith("Targets_") && entityType.Name.EndsWith("Target") &&
                    entityType.Namespace.StartsWith("Monahrq.Wing"))
                {
                    entityTableName = "Targets_" + entityTableName;
                }

                if (entityType.Namespace.EqualsIgnoreCase("Monahrq.Infrastructure.Domain.Regions"))
                {
                    entityTableName = Inflector.Pluralize(typeof(Region).Name); 
                }
            }

            return entityTableName;
        }
   
        private static EntityTableNameAttribute ResolveAttribute(Type entityType)
        {
            if (entityType == null || entityType == typeof(Entity<int>)) return null;

            var entityTableNameAttrib = entityType.GetCustomAttributes(typeof(EntityTableNameAttribute), true).FirstOrDefault() as EntityTableNameAttribute;

            if (entityTableNameAttrib == null)
            {
                return ResolveAttribute(entityType.BaseType);
            }
            else
            {
                return entityTableNameAttrib;
            }
        }
    }

    public class GenericEqualityComparer<T> : IEqualityComparer<T>
    {
        Func<T, T, bool> Compare { get; set; }
        Func<T, int> Hash { get; set; }
        public GenericEqualityComparer(Func<T, T, bool> compare, Func<T, int> hash)
        {
            Compare = compare;
            Hash = hash;
        }

        public bool Equals(T x, T y)
        {
            return Compare(x, y);
        }

        public int GetHashCode(T obj)
        {
            return Hash(obj);
        }

    }

    
}
