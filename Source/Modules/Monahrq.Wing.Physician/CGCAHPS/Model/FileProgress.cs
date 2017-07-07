using System.IO;
using PropertyChanged;

namespace Monahrq.Wing.Physician.CGCAHPS.Model
{
    [ImplementPropertyChanged]
    public class FileProgress
    {
        public string FileName { get; set; }
        public int LinesDone { get; set; }
        public int LinesProcessed { get; set; }
        public int LinesErrors { get; set; }
        public int LinesDuplicated { get; set; }
        public int PercentComplete { get; set; }
        public int TotalLines { get; set; }

    }

    public static class FileProgressExtension
    {
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
