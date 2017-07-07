using System;
using System.Globalization;
using System.Windows.Data;
using System.Text.RegularExpressions;

namespace Monahrq.Theme.Converters
{
	/// <summary>
	/// Converts Html to plain text.
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class HtmlToTextConverter : IValueConverter
    {
		/// <summary>
		/// Converts a value.
		/// </summary>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var content = value as string;
            if (content == null)
                return null;

            try
            {
                //XElement xdoc = new XElement("any", content);
                //var plaintext = xdoc.Value.ToString();
                //content = HttpUtility.HtmlDecode(content);

                var plaintext = StripHTML(content);
                return plaintext;
            }
            catch (Exception)
            {
                return content;
            }
        }

		/// <summary>
		/// Converts a value.
		/// </summary>
		/// <param name="value">The value that is produced by the binding target.</param>
		/// <param name="targetType">The type to convert to.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

		// Remove HTML Development formatting
		// See:  http://www.codeproject.com/Articles/11902/Convert-HTML-to-Plain-Text
		/// <summary>
		/// Strips the HTML.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns></returns>
		private string StripHTML(string source)
        {
            try
            {
                string result;

                // Replace line breaks with space because browsers inserts space
                result = source.Replace("\r", " ");
                result = result.Replace("\n", " ");

                // Remove step-formatting
                result = result.Replace("\t", string.Empty);
                // Remove repeating spaces because browsers ignore them
                result = Regex.Replace(result, @"( )+", " ");

                // Remove the header (prepare first by clearing attributes)
                result = Regex.Replace(result, @"<( )*head([^>])*>", "<head>", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"(<( )*(/)( )*head( )*>)", "</head>", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "(<head>).*(</head>)", string.Empty, RegexOptions.IgnoreCase);

                // remove all scripts (prepare first by clearing attributes)
                result = Regex.Replace(result, @"<( )*script([^>])*>", "<script>", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"(<( )*(/)( )*script( )*>)", "</script>", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"(<script>).*(</script>)", string.Empty, RegexOptions.IgnoreCase);

                // remove all styles (prepare first by clearing attributes)
                result = Regex.Replace(result, @"<( )*style([^>])*>", "<style>", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"(<( )*(/)( )*style( )*>)", "</style>", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "(<style>).*(</style>)", string.Empty, RegexOptions.IgnoreCase);

                // insert tabs in spaces of <td> tags
                result = Regex.Replace(result, @"<( )*td([^>])*>", "\t", RegexOptions.IgnoreCase);

                // insert line breaks in places of <BR> and <LI> tags
                result = Regex.Replace(result, @"<( )*br( )*>", "\r", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"<( )*li( )*>", "\r", RegexOptions.IgnoreCase);

                // do not insert new pararagraphs for <DIV> tags
                // insert line paragraphs (double line breaks) in place of <P>, and <TR> tags
                string linebreak = "\r";        // not \r\r
                result = Regex.Replace(result, @"<( )*div([^>])*>", "", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"<( )*tr([^>])*>", linebreak, RegexOptions.IgnoreCase);

                // BUG: This is replacing <p>SOME TEXT</p> with... \rSOME TEXT. Per Jason, we should only replace </p><p> with \r
                result = Regex.Replace(result, @"<( )*p([^>])*>", linebreak, RegexOptions.IgnoreCase);

                // Remove remaining tags like <a>, links, images, comments etc - anything that's enclosed inside < >
                result = Regex.Replace(result, @"<[^>]*>", string.Empty, RegexOptions.IgnoreCase);

                // replace special characters:
                result = Regex.Replace(result, @" ", " ", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&bull;", " * ", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&lsaquo;", "<", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&rsaquo;", ">", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&trade;", "(tm)", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&frasl;", "/", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&lt;", "<", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&gt;", ">", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&copy;", "(c)", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, @"&reg;", "(r)", RegexOptions.IgnoreCase);

                // Remove all others. More can be added, see
                // http://hotwired.lycos.com/webmonkey/reference/special_characters/
                result = Regex.Replace(result, @"&(.{2,6});", string.Empty, RegexOptions.IgnoreCase);

                // make line breaking consistent
                result = result.Replace("\n", "\r");

                // Replace 3 or more consecutive linebreaks with 2, and 4 or more tabs with 4.
                // Prepare first to remove any whitespaces in between the escaped characters and remove redundant tabs in between line breaks
                result = Regex.Replace(result, "(\r)( )+(\r)", "\r\r", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "(\t)( )+(\t)", "\t\t", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "(\t)( )+(\r)", "\t\r", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "(\r)( )+(\t)", "\r\t", RegexOptions.IgnoreCase);
                // Remove redundant tabs
                result = Regex.Replace(result, "(\r)(\t)+(\r)", "\r\r", RegexOptions.IgnoreCase);
                // Remove multiple tabs following a line break with just one tab
                result = Regex.Replace(result, "(\r)(\t)+", "\r\t", RegexOptions.IgnoreCase);
                
                // Initial replacement target string for line breaks
                string breaks = "\r\r\r";
                // Initial replacement target string for tabs
                string tabs = "\t\t\t\t\t";
                for (int index = 0; index < result.Length; index++)
                {
                    result = result.Replace(breaks, "\r\r");
                    result = result.Replace(tabs, "\t\t\t\t");
                    breaks = breaks + "\r";
                    tabs = tabs + "\t";
                }

                // KLUDGE: remove the leading linebreak that gets inserted by the <p> replacement above.
                // The problem with this kludge is that it will remove a leading linebreak if the user manually inserted one in the plaintext.
                if (result.StartsWith("\r"))
                    result = result.Substring(1, result.Length - 1);

                return result;
            }
            catch
            {
                return source;
            }
        }
    }
}