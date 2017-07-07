using System.IO;
using  PropertyChanged ;
namespace Monahrq.Wing.Ahrq.Model
{
    /// <summary>
    /// Class for file progress
    /// </summary>
    [ImplementPropertyChanged]
    public class FileProgress
    {
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }
        /// <summary>
        /// Gets or sets the lines done.
        /// </summary>
        /// <value>
        /// The lines done.
        /// </value>
        public int LinesDone { get; set; }
        /// <summary>
        /// Gets or sets the lines processed.
        /// </summary>
        /// <value>
        /// The lines processed.
        /// </value>
        public int LinesProcessed { get; set; }
        /// <summary>
        /// Gets or sets the lines errors.
        /// </summary>
        /// <value>
        /// The lines errors.
        /// </value>
        public int LinesErrors { get; set; }
        /// <summary>
        /// Gets or sets the lines duplicated.
        /// </summary>
        /// <value>
        /// The lines duplicated.
        /// </value>
        public int LinesDuplicated { get; set; }
        /// <summary>
        /// Gets or sets the percent complete.
        /// </summary>
        /// <value>
        /// The percent complete.
        /// </value>
        public int PercentComplete { get; set; }
        /// <summary>
        /// Gets or sets the total lines.
        /// </summary>
        /// <value>
        /// The total lines.
        /// </value>
        public int TotalLines { get; set; }

    }

    /// <summary>
    /// Extension class for <see cref="Monahrq.Wing.MedicareProviderCharge.Models.FileProgress"/>
    /// </summary>
    public static class FileProgressExtension
    {
        /// <summary>
        /// The number of lines the file contains.
        /// </summary>
        /// <param name="fp">The fp.</param>
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
