using System.IO;

namespace Monahrq.Wing.Dynamic.Models
{
    /// <summary>
    /// Extension class for <see cref="FileProgress"/>
    /// </summary>
    public static class FileProgressExtension
    {
        /// <summary>
        /// Counts the nuber of line.
        /// </summary>
        /// <param name="fp">The fileprogree instance.</param>
        /// <returns></returns>
        public static int LinesCount(this FileProgress fp)
        {
            if (fp.TotalLines > 0)
                return fp.TotalLines;

            using (var fs = new FileStream(fp.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fs))
            {
                while (reader.ReadLine() != null)
                {
                    fp.TotalLines++;
                }
            }
            return fp.TotalLines;
        }
    }
}