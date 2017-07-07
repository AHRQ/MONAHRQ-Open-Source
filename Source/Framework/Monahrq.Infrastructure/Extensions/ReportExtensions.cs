using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Monahrq.Infrastructure.Entities.Domain.Reports;

namespace Monahrq.Infrastructure.Extensions
{
    public static class ReportExtentions
    {
        public static Dictionary<string, BitmapImage> GetPreviewImages(this Report report, BitmapCacheOption bitmapCacheOption = BitmapCacheOption.Default)
        {
            return new Dictionary<string, BitmapImage>()
            {
                { "Professional", GetBitmapImage(report.SourceTemplate.PreviewImage)},
                { "Consumer" , GetBitmapImage(report.SourceTemplate.ConsumerPreviewImage)},
            };
        }

        private static BitmapImage GetBitmapImage(string previewImage, BitmapCacheOption bitmapCacheOption = BitmapCacheOption.Default)
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Domain\Reports\Data");
            var filepath = Path.Combine(dir, previewImage ?? @"missing_template.png");

            if (string.IsNullOrEmpty(previewImage) || !new FileInfo(filepath).Exists)
            {
                filepath = Path.Combine(dir, @"missing_template.png");
            }

            var uri = new Uri(filepath, UriKind.RelativeOrAbsolute);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = bitmapCacheOption;
            image.UriSource = uri;
            image.EndInit();
            image.Freeze();
            return image;
        }

    }
}
