using System;
using System.ComponentModel;
using System.Reflection;

namespace Monahrq.Wing.Dynamic.ViewModels
{
    /// <summary>
    /// Helper class for Enum
    /// </summary>
    /// <typeparam name="T"></typeparam>
    static class EnumHelper<T>
    {
        /// <summary>
        /// Gets the enum description.
        /// </summary>
        /// <param name="anEnum">An enum.</param>
        /// <returns></returns>
        public static string GetEnumDescription(object anEnum)
        {
            Type type = typeof(T);
            var field = type.GetField(anEnum.ToString());
            if (field == null) return string.Empty;
            var customAttribute = field.GetCustomAttribute<DescriptionAttribute>(false);
            return customAttribute == null ? anEnum.ToString() : customAttribute.Description;
        }
    }
}