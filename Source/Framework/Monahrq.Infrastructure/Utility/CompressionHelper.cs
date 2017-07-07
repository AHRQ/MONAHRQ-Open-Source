using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NHibernate.Transform;

namespace Monahrq.Infrastructure.Utility
{
    public static class CompressionHelper
    {
        public static void Extract(string zipPath, string extractPath, Encoding encoding = null, bool deleteFileFirst = true)
        {
            if (deleteFileFirst && Directory.Exists(extractPath))
            {
                foreach (var file in Directory.EnumerateFiles(extractPath, "*", SearchOption.AllDirectories).ToList())
                {
                    if (file.EndsWith(".zip")) continue;
                    if(File.Exists(file))
                        File.Delete(file);
                }
            }

            ZipFile.ExtractToDirectory(zipPath, extractPath, encoding ?? Encoding.UTF8);
        }

        //public static void Extract(string zipPath, string extractPath, Encoding encoding)
        //{
        //    ZipFile.ExtractToDirectory(zipPath, extractPath, encoding);
        //}
    }
}
