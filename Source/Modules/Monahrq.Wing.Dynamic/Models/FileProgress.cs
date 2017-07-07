using PropertyChanged;

namespace Monahrq.Wing.Dynamic.Models
{
    /// <summary>
    /// Class to handle the file progress operations
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
        /// Gets or sets the number of lines done.
        /// </summary>
        /// <value>
        /// The number of lines done.
        /// </value>
        public int LinesDone { get; set; }
        /// <summary>
        /// Gets or sets the number of lines processed.
        /// </summary>
        /// <value>
        /// The number of lines processed.
        /// </value>
        public int LinesProcessed { get; set; }
        /// <summary>
        /// Gets or sets the number of lines having errors.
        /// </summary>
        /// <value>
        /// The number of lines having errors.
        /// </value>
        public int LinesErrors { get; set; }
        /// <summary>
        /// Gets or sets the number of lines duplicated.
        /// </summary>
        /// <value>
        /// The number of lines duplicated.
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
        /// Gets or sets the total number of lines in the file.
        /// </summary>
        /// <value>
        /// The total number of lines in the file.
        /// </value>
        public int TotalLines { get; set; }

    }
}
