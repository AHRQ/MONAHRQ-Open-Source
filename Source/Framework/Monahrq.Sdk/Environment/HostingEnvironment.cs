using System.IO;
using System.Reflection;

namespace Monahrq.Sdk.Environment
{
    /// <summary>
    /// the hosting environment extension method class.
    /// </summary>
    public static class HostingEnvironment
    {
        /// <summary>
        /// Extension method that maps the file paths.
        /// </summary>
        /// <param name="pathSegments">The path segments.</param>
        /// <returns></returns>
        public static string MapPath(params string[] pathSegments)
        {
            var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(root, Path.Combine(pathSegments));
        }
    }
}
