using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Monahrq.Infrastructure.Assert.Extensions
{    
    /// <summary>
    /// Defines extension methods for the <see cref="ArgumentHelper"/> class.
    /// </summary>
    /// <remarks>
    /// This class defines extensions methods for the <see cref="ArgumentHelper"/>. All extension methods simply delegate to the
    /// appropriate member of the <see cref="ArgumentHelper"/> class.
    /// </remarks>
    /// <example>
    /// The following code ensures that the <c>name</c> argument is not <see langword="null"/>:
    /// <code>
    /// public void DisplayDetails(string name)
    /// {
    ///     name.AssertNotNull("name");
    ///     //now we know that name is not null
    ///     ...
    /// }
    /// </code>
    /// </example>
    public static class ArgumentHelperExtensions
    {
        /// <summary>
        /// Asserts the not null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg">The argument.</param>
        /// <param name="argName">Name of the argument.</param>
        /// <include file="../ArgumentHelper.doc.xml" path="doc/member[@name=&quot;AssertNotNull{T}(T,string)&quot;]/*" />
        [DebuggerHidden]
        public static void AssertNotNull<T>(this T arg, string argName)
            where T : class
        {
            ArgumentHelper.AssertNotNull(arg, argName);
        }

        /// <summary>
        /// Asserts the not null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg">The argument.</param>
        /// <param name="argName">Name of the argument.</param>
        /// <include file="../ArgumentHelper.doc.xml" path="doc/member[@name=&quot;AssertNotNull{T}(Nullable{T},string)&quot;]/*" />
        [DebuggerHidden]
        public static void AssertNotNull<T>(this T? arg, string argName)
            where T : struct
        {
            ArgumentHelper.AssertNotNull(arg, argName);
        }

        /// <summary>
        /// Asserts the generic argument not null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg">The argument.</param>
        /// <param name="argName">Name of the argument.</param>
        /// <include file="../ArgumentHelper.doc.xml" path="doc/member[@name=&quot;AssertGenericArgumentNotNull{T}(T,string)&quot;]/*" />
        [DebuggerHidden]
        public static void AssertGenericArgumentNotNull<T>(this T arg, string argName)
        {
            ArgumentHelper.AssertGenericArgumentNotNull(arg, argName);
        }

        /// <summary>
        /// Asserts the not null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg">The argument.</param>
        /// <param name="argName">Name of the argument.</param>
        /// <param name="assertContentsNotNull">if set to <c>true</c> [assert contents not null].</param>
        /// <include file="../ArgumentHelper.doc.xml" path="doc/member[@name=&quot;AssertNotNull{T}(IEnumerable{T},string,bool)&quot;]/*" />
        [DebuggerHidden]
        public static void AssertNotNull<T>(this IEnumerable<T> arg, string argName, bool assertContentsNotNull)
        {
            ArgumentHelper.AssertNotNull(arg, argName, assertContentsNotNull);
        }

        /// <summary>
        /// Asserts the not null or empty.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="argName">Name of the argument.</param>
        /// <include file="../ArgumentHelper.doc.xml" path="doc/member[@name=&quot;AssertNotNullOrEmpty(string,string)&quot;]/*" />
        [DebuggerHidden]
        public static void AssertNotNullOrEmpty(this string arg, string argName)
        {
            ArgumentHelper.AssertNotNullOrEmpty(arg, argName);
        }

        /// <summary>
        /// Asserts the not null or empty.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="argName">Name of the argument.</param>
        /// <include file="../ArgumentHelper.doc.xml" path="doc/member[@name=&quot;AssertNotNullOrEmpty(IEnumerable,string)&quot;]/*" />
        [DebuggerHidden]
        public static void AssertNotNullOrEmpty(this IEnumerable arg, string argName)
        {
            ArgumentHelper.AssertNotNullOrEmpty(arg, argName);
        }

        /// <summary>
        /// Asserts the not null or empty.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="argName">Name of the argument.</param>
        /// <include file="../ArgumentHelper.doc.xml" path="doc/member[@name=&quot;AssertNotNullOrEmpty(ICollection,string)&quot;]/*" />
        [DebuggerHidden]
        public static void AssertNotNullOrEmpty(this ICollection arg, string argName)
        {
            ArgumentHelper.AssertNotNullOrEmpty(arg, argName);
        }

        /// <summary>
        /// Asserts the not null or white space.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="argName">Name of the argument.</param>
        /// <include file="../ArgumentHelper.doc.xml" path="doc/member[@name=&quot;AssertNotNullOrWhiteSpace(string,string)&quot;]/*" />
        [DebuggerHidden]
        public static void AssertNotNullOrWhiteSpace(this string arg, string argName)
        {
            ArgumentHelper.AssertNotNullOrWhiteSpace(arg, argName);
        }

        /// <summary>
        /// Asserts the enum member.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <param name="argName">Name of the argument.</param>
        /// <include file="../ArgumentHelper.doc.xml" path="doc/member[@name=&quot;AssertEnumMember{TEnum}(TEnum,string)&quot;]/*" />
        [DebuggerHidden]
        [CLSCompliant(false)]
        public static void AssertEnumMember<TEnum>(this TEnum enumValue, string argName)
            where TEnum : struct, IConvertible
        {
            ArgumentHelper.AssertEnumMember(enumValue, argName);
        }

        /// <summary>
        /// Asserts the enum member.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <param name="argName">Name of the argument.</param>
        /// <param name="validValues">The valid values.</param>
        /// <include file="../ArgumentHelper.doc.xml" path="doc/member[@name=&quot;AssertEnumMember{TEnum}(TEnum,string,TEnum[])&quot;]/*" />
        [DebuggerHidden]
        [CLSCompliant(false)]
        public static void AssertEnumMember<TEnum>(this TEnum enumValue, string argName, params TEnum[] validValues)
            where TEnum : struct, IConvertible
        {
            ArgumentHelper.AssertEnumMember(enumValue, argName, validValues);
        }
    }
}