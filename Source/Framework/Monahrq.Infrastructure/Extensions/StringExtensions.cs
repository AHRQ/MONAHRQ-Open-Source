using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Monahrq.Infrastructure.Extensions
{
    /// <summary>
    /// A set of useful extensions for the String class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts the HTML tagsto lower case.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static string ConvertHtmlTagstoLowerCase(this string source)
        {
            return source == null
                ? null
                : Regex.Replace(
                    source,
                    @"<[^<>]+>",
                    m => { return m.Value.ToLower(); },
                    RegexOptions.Multiline | RegexOptions.Singleline);
        }

        /// <summary>
        /// Determines whether [is null or empty] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value) || value.Trim().EqualsIgnoreCase("NULL") || value.Trim() == string.Empty;
        }

        /// <summary>
        /// Determines whether the source string contains the value string, in a case-insensitive fasion.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="value">The value string.</param>
        /// <returns>
        ///   <c>true</c> if source contains value; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsCaseInsensitive(this string source, string value)
        {
            if (string.IsNullOrEmpty(source)) return false;

            int results = source.IndexOf(value, StringComparison.CurrentCultureIgnoreCase);
            return results != -1;
        }

        /// <summary>
        /// Returns the given potion of the string before the [search] string. 
        /// the full string [s] is returned if the [search] string is not found
        /// </summary>
        /// <param name="s">string to search</param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static string SubStrBefore(this string s, string search)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return s.Contains(search) ? s.Substring(0, s.IndexOf(search)) : s;
        }

        /// <summary>
        /// Returns the given potion of the string after the [search] string. 
        /// an empty string [s] is returned if the [search] string is not found
        /// </summary>
        /// <param name="s">string to search</param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static string SubStrAfter(this string s, string search)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return s.Contains(search) ? s.Substring(s.IndexOf(search) + search.Length) : string.Empty;
        }

        /// <summary>
        /// Returns the given potion of the string before the last occurrence of the [search] string. 
        /// the full string [s] is returned if the [search] string is not found
        /// </summary>
        /// <param name="s">string to search</param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static string SubStrBeforeLast(this string s, string search)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return s.Contains(search) ? s.Substring(0, s.LastIndexOf(search)) : s;
        }

        /// <summary>
        /// Returns the given potion of the string after the last occurrence of the [search] string. 
        /// an empty string [s] is returned if the [search] string is not found
        /// </summary>
        /// <param name="s">string to search</param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static string SubStrAfterLast(this string s, string search)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return s.Contains(search) ? s.Substring(s.LastIndexOf(search) + search.Length) : string.Empty;
        }

        /// <summary>
        /// Determines whether [contains] [the specified source].
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="toCheck">To check.</param>
        /// <param name="comp">The comp.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified source]; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return !string.IsNullOrEmpty(source) && source.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Determines whether [contains ignore case] [the specified source].
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="toCheck">To check.</param>
        /// <returns>
        ///   <c>true</c> if [contains ignore case] [the specified source]; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsIgnoreCase(this string source, string toCheck)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        /// <summary>
        /// Equalses the ignore case.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="toCheck">To check.</param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string source, string toCheck)
        {
            if (string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(toCheck)) return false;

            return source != null && source.Equals(toCheck, StringComparison.InvariantCultureIgnoreCase);
        }

        //public static bool IsNullOrEmpty(this string item)
        //{
        //    if (item == null) return true;
        //    if (string.IsNullOrEmpty(item) || string.IsNullOrWhiteSpace(item)) return true;

        //    return false;
        //}

        /// <summary>
        /// To the proper.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string ToProper(this string str)
        {
            if (str == null)
            {
                return null;
            }
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            return cultureInfo.TextInfo.ToTitleCase(str.ToLower());
        }


        /// <summary>
        /// Camels the friendly.
        /// </summary>
        /// <param name="camel">The camel.</param>
        /// <returns></returns>
        public static string CamelFriendly(this string camel)
        {
            if (string.IsNullOrWhiteSpace(camel))
                return "";

            var sb = new StringBuilder(camel);

            for (var i = camel.Length - 1; i > 0; i--)
            {
                var current = sb[i];
                if ('A' <= current && current <= 'Z')
                {
                    sb.Insert(i, ' ');
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Ellipsizes the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="characterCount">The character count.</param>
        /// <returns></returns>
        public static string Ellipsize(this string text, int characterCount)
        {
            return text.Ellipsize(characterCount, "&#160;&#8230;");
        }

        /// <summary>
        /// Ellipsizes the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="characterCount">The character count.</param>
        /// <param name="ellipsis">The ellipsis.</param>
        /// <param name="wordBoundary">if set to <c>true</c> [word boundary].</param>
        /// <returns></returns>
        public static string Ellipsize(this string text, int characterCount, string ellipsis, bool wordBoundary = false)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            if (characterCount < 0 || text.Length <= characterCount)
                return text;

            // search beginning of word
            var backup = characterCount;
            while (characterCount > 0 && text[characterCount - 1].IsLetter())
            {
                characterCount--;
            }

            // search previous word
            while (characterCount > 0 && text[characterCount - 1].IsSpace())
            {
                characterCount--;
            }

            // if it was the last word, recover it, unless boundary is requested
            if (characterCount == 0 && !wordBoundary)
            {
                characterCount = backup;
            }

            var trimmed = text.Substring(0, characterCount);
            return trimmed + ellipsis;
        }

        public static string HtmlClassify(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            var friendlier = text.CamelFriendly();

            var result = new char[friendlier.Length];

            var cursor = 0;
            var previousIsNotLetter = false;
            for (var i = 0; i < friendlier.Length; i++)
            {
                var current = friendlier[i];
                if (IsLetter(current))
                {
                    if (previousIsNotLetter && i != 0)
                    {
                        result[cursor++] = '-';
                    }

                    result[cursor++] = Char.ToLowerInvariant(current);
                    previousIsNotLetter = false;
                }
                else
                {
                    previousIsNotLetter = true;
                }
            }

            return new string(result, 0, cursor);
        }

        /// <summary>
        /// Removes the tags.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <returns></returns>
        public static string RemoveTags(this string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            var result = new char[html.Length];

            var cursor = 0;
            var inside = false;
            for (var i = 0; i < html.Length; i++)
            {
                var current = html[i];

                switch (current)
                {
                    case '<':
                        inside = true;
                        continue;
                    case '>':
                        inside = false;
                        continue;
                }

                if (!inside)
                {
                    result[cursor++] = current;
                }
            }

            return new string(result, 0, cursor);
        }

        // not accounting for only \r (e.g. Apple OS 9 carriage return only new lines)
        /// <summary>
        /// Replaces the new lines with.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        public static string ReplaceNewLinesWith(this string text, string replacement)
        {
            return string.IsNullOrWhiteSpace(text)
                ? string.Empty
                : text
                    .Replace("\r\n", "\r\r")
                    .Replace("\n", string.Format(replacement, "\r\n"))
                    .Replace("\r\r", string.Format(replacement, "\r\n"));
        }

        /// <summary>
        /// To the hexadecimal string.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static string ToHexString(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        /// <summary>
        /// To the byte array.
        /// </summary>
        /// <param name="hex">The hexadecimal.</param>
        /// <returns></returns>
        public static byte[] ToByteArray(this string hex)
        {
            return Enumerable.Range(0, hex.Length).
                Where(x => 0 == x%2).
                Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).
                ToArray();
        }

        private static readonly char[] _validSegmentChars = "/?#[]@\"^{}|`<>\t\r\n\f ".ToCharArray();

        /// <summary>
        /// Determines whether [is valid URL segment] [the specified segment].
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns></returns>
        public static bool IsValidUrlSegment(this string segment)
        {
            // valid isegment from rfc3987 - http://tools.ietf.org/html/rfc3987#page-8
            // the relevant bits:
            // isegment    = *ipchar
            // ipchar      = iunreserved / pct-encoded / sub-delims / ":" / "@"
            // iunreserved = ALPHA / DIGIT / "-" / "." / "_" / "~" / ucschar
            // pct-encoded = "%" HEXDIG HEXDIG
            // sub-delims  = "!" / "$" / "&" / "'" / "(" / ")" / "*" / "+" / "," / ";" / "="
            // ucschar     = %xA0-D7FF / %xF900-FDCF / %xFDF0-FFEF / %x10000-1FFFD / %x20000-2FFFD / %x30000-3FFFD / %x40000-4FFFD / %x50000-5FFFD / %x60000-6FFFD / %x70000-7FFFD / %x80000-8FFFD / %x90000-9FFFD / %xA0000-AFFFD / %xB0000-BFFFD / %xC0000-CFFFD / %xD0000-DFFFD / %xE1000-EFFFD
            // 
            // rough blacklist regex == m/^[^/?#[]@"^{}|\s`<>]+$/ (leaving off % to keep the regex simple)

            return !segment.Any(_validSegmentChars);
        }

        /// <summary>
        /// Generates a valid technical name.
        /// </summary>
        /// <remarks>
        /// Uses a white list set of chars.
        /// </remarks>
        public static string ToSafeName(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            name = RemoveDiacritics(name);
            name = name.Strip(c =>
                c != '_'
                && c != '-'
                && !c.IsLetter()
                && !Char.IsDigit(c)
                );

            name = name.Trim();

            // don't allow non A-Z chars as first letter, as they are not allowed in prefixes
            while (name.Length > 0 && !IsLetter(name[0]))
            {
                name = name.Substring(1);
            }

            if (name.Length > 128)
                name = name.Substring(0, 128);

            return name;
        }

        /// <summary>
        /// Whether the char is a letter between A and Z or not
        /// </summary>
        public static bool IsLetter(this char c)
        {
            return ('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z');
        }

        /// <summary>
        /// Determines whether the specified c is space.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        public static bool IsSpace(this char c)
        {
            return (c == '\r' || c == '\n' || c == '\t' || c == '\f' || c == ' ');
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        public static bool EqualsAny(this string _this, params string[] stringList)
        {
            foreach (var element in stringList)
            {
                if (_this == null)
                    if (element == null) return true;
                    else continue;

                else if (_this.Equals(element))
                    return true;
            }
            return false;
		}
		public static bool EqualsAnyIgnoreCase(this string _this, params string[] stringList)
		{
			foreach (var element in stringList)
			{
				if (_this == null)
					if (element == null) return true;
					else continue;

				else if (_this.EqualsIgnoreCase(element))
					return true;
			}
			return false;
		}
		/// <summary>
		/// Checks if any of the given strings are contained within.
		/// </summary>
		/// <param name="_this"></param>
		/// <param name="stringList"></param>
		/// <returns></returns>
		public static bool ContainsAny(this string _this, params string[] stringList)
		{
			foreach (var element in stringList)
			{
				if (_this == null)
					if (element == null) return true;
					else continue;

				else if (_this.Contains(element))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Checks if any of the give strings are the beginning string.
		/// </summary>
		/// <param name="_this"></param>
		/// <param name="stringList"></param>
		/// <returns></returns>
		public static bool StartsWithAny(this string _this, params string[] stringList)
		{
			foreach (var element in stringList)
			{
				if (_this == null)
					if (element == null) return true;
					else continue;

				else if (_this.StartsWith(element))
					return true;
			}
			return false;
		}


        /// <summary>
        /// Removes the diacritics.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static string RemoveDiacritics(string name)
        {
            var stFormD = name.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var t in stFormD)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(t);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(t);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        /// <summary>
        /// Strips the specified subject.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="stripped">The stripped.</param>
        /// <returns></returns>
        public static string Strip(this string subject, params char[] stripped)
        {
            if (stripped == null || stripped.Length == 0 || String.IsNullOrEmpty(subject))
            {
                return subject;
            }

            Array.Sort(stripped);
            var result = new char[subject.Length];

            var cursor = 0;
            for (var i = 0; i < subject.Length; i++)
            {
                var current = subject[i];
                if (Array.BinarySearch(stripped, current) < 0)
                {
                    result[cursor++] = current;
                }
            }

            return new string(result, 0, cursor);
        }

        public static string Strip(this string subject, Func<char, bool> predicate)
        {

            var result = new char[subject.Length];

            var cursor = 0;
            for (var i = 0; i < subject.Length; i++)
            {
                var current = subject[i];
                if (!predicate(current))
                {
                    result[cursor++] = current;
                }
            }

            return new string(result, 0, cursor);
        }

        /// <summary>
        /// Anies the specified subject.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="chars">The chars.</param>
        /// <returns></returns>
        public static bool Any(this string subject, params char[] chars)
        {
            if (string.IsNullOrEmpty(subject) || chars == null || chars.Length == 0)
            {
                return false;
            }

            Array.Sort(chars);

            return subject.Any(current => Array.BinarySearch(chars, current) >= 0);
        }

        public static bool All(this string subject, params char[] chars)
        {
            if (string.IsNullOrEmpty(subject))
                return true;

            if (chars == null || chars.Length == 0)
                return false;

            Array.Sort(chars);

            return subject.All(current => Array.BinarySearch(chars, current) >= 0);
        }

        /// <summary>
        /// Translates the specified subject.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// from;Parameters must have the same length
        /// </exception>
        public static string Translate(this string subject, char[] from, char[] to)
        {
            if (string.IsNullOrEmpty(subject))
            {
                return subject;
            }

            if (@from == null || to == null)
            {
                throw new ArgumentNullException();
            }

            if (@from.Length != to.Length)
            {
                throw new ArgumentNullException(@"from", @"Parameters must have the same length");
            }

            var map = new Dictionary<char, char>(@from.Length);
            for (var i = 0; i < @from.Length; i++)
            {
                map[@from[i]] = to[i];
            }

            var result = new char[subject.Length];

            for (var i = 0; i < subject.Length; i++)
            {
                var current = subject[i];
                if (map.ContainsKey(current))
                {
                    result[i] = map[current];
                }
                else
                {
                    result[i] = current;
                }
            }

            return new string(result);
        }

        //public static bool IsNullOrEmpty(this string value)
        //{
        //    return string.IsNullOrEmpty(value) && value == " ";
        //}

        private const string usZipRegEx = @"^\d{5}(?:[-\s]\d{4})?$";

        public static bool IsValidZip(this string value)
        {
            return !string.IsNullOrEmpty(value) && System.Text.RegularExpressions.Regex.Match(value, usZipRegEx).Success;
        }

        private const string usPhoneNumberRegEx = @"^\d{10}$";

        public static bool IsValidPhoneNumber(this string value)
        {
            return !string.IsNullOrEmpty(value) && System.Text.RegularExpressions.Regex.Match(value, usPhoneNumberRegEx).Success;
        }

        private const string usFaxNumberRegEx = @"^\d{10}$";

        public static bool IsValidFaxNumber(this string value)
        {
            return !string.IsNullOrEmpty(value) && System.Text.RegularExpressions.Regex.Match(value, usFaxNumberRegEx).Success;
        }

        /// <summary>
        /// Overload which uses the culture info with the specified name
        /// </summary>
        public static string ToProper(this string str, string cultureInfoName)
        {
            if (str == null)
            {
                return null;
            }
            var cultureInfo = new CultureInfo(cultureInfoName);
            return cultureInfo.TextInfo.ToTitleCase(str.ToLower());
        }

        /// <summary>
        /// Overload which uses the specified culture info
        /// </summary>
        public static string ToProper(this string str, CultureInfo cultureInfo)
        {
            if (str == null)
            {
                return null;
            }
            return cultureInfo.TextInfo.ToTitleCase(str.ToLower());
        }

        private static string ReplaceEx(this string original, string pattern, string replacement)
        {
            if (string.IsNullOrEmpty(original)) return original;

            int position0, position1;
            var count = position0 = position1 = 0;
            var upperString = original.ToUpper();
            var upperPattern = pattern.ToUpper();
            var inc = (original.Length/pattern.Length)*
                      (replacement.Length - pattern.Length);
            var chars = new char[original.Length + Math.Max(0, inc)];
            while (
                (position1 = upperString.IndexOf(upperPattern, position0, StringComparison.InvariantCultureIgnoreCase)) !=
                -1)
            {
                for (var i = position0; i < position1; ++i)
                    chars[count++] = original[i];
                for (var i = 0; i < replacement.Length; ++i)
                    chars[count++] = replacement[i];
                position0 = position1 + pattern.Length;
            }
            if (position0 == 0) return original;
            for (var i = position0; i < original.Length; ++i)
                chars[count++] = original[i];
            return new string(chars, 0, count);
        }

        public static IList<string> StringSubList(this IList<string> source, int index)
        {
            return source.Take(index).ToList();
            //return source
            //    .Select((x, i) => new {Index = i, Value = x})
            //    .GroupBy(x => x.Index/3)
            //    .Select(x => x.Select(v => v.Value).ToList())
            //.ToList();
        }
		public static string UnQuote(this string source)
		{
		//	return source.Trim(new char[] { '"', '\'' });

			if (source == null) return source;
			if (source.Length < 2) return source;

			var quotePairs = new List<char[]>()
			{
				new char[2] {'"', '"'},
                new char[2] {'\'', '\''},
			};

			foreach (var quotePair in quotePairs)
			{
				if (source[0] == quotePair[0] && source.Last() == quotePair[1])
					return source.Substring(1, source.Length - 2);
			}
			return source;
		}

		public static string Coalesce(params string[] values)
		{
			foreach (var value in values)
				if (!value.IsNullOrEmpty())
					return value;
			return null;
		}
		public static bool AnyPopulated(params string[] values)
		{
			return Coalesce(values) != null;
		}

		public static string ConvertToHTMLParagraph(this string inputText)
        {
            var result = !string.IsNullOrEmpty(inputText) ? inputText.Replace(Environment.NewLine, "<br>") : null;
            return result;
        }

		public static string[] SplitAndKeep(this string source, char[] delimiters)
		{
			return source.SplitAndKeep(delimiters,StringSplitOptions.None);
		}
		public static string[] SplitAndKeep(this string source, char[] delimiters, StringSplitOptions options)
		{
			var pattern = @"(?<=[" + new string(delimiters) + @"])";
            return System.Text.RegularExpressions.Regex.Split(source, pattern/*,options*/);
		}
        public static bool IsValidIP(this string address)
        {
            if (!Regex.IsMatch(address, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b"))
                return false;

            IPAddress dummyIPAddress;
            return IPAddress.TryParse(address, out dummyIPAddress);
        }
    }
}
