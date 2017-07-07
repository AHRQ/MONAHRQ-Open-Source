using System.Text.RegularExpressions;

namespace Monahrq.Theme.Controls.Wizard.Helpers
{
    public static class Extensions
    {

        /// <summary>
        /// http://social.msdn.microsoft.com/Forums/en-US/csharpgeneral/thread/791963c8-9e20-4e9e-b184-f0e592b943b0
        /// </summary>
        /// <returns>Ex: casedWordHTTPWriter becomes "Cased Word HTTP Writer", HotMomma becomes "Hot Momma"</returns>
        public static string SplitCamelCase( this string str )
        {
            return Regex.Replace( Regex.Replace( str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2" ), @"(\p{Ll})(\P{Ll})", "$1 $2" );
        }

    }
}
