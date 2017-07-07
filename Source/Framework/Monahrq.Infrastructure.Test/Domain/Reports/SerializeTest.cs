using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Xml.Serialization;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using System.Text;
using System.IO;

namespace Monahrq.Infrastructure.Test.Domain.Reports
{
    [TestClass]
    public class SerializeTest
    {
        [TestMethod]
        public void SerializeSample()
        {
            using (var str = Assembly
                    .GetExecutingAssembly()
                    .GetManifestResourceStream(this.GetType(), "Sample.xml"))
            {
                var ser = new XmlSerializer(typeof(ReportManifest));
                var obj = ser.Deserialize(str);
            }
        }

        [TestMethod]
        public void TestSeralize()
        {
            var rep = CreateReport();
            var man = rep.AsManifest();
            var ser = new XmlSerializer(typeof(ReportManifest));
            var str = new MemoryStream();
            ser.Serialize(str, man);
            str.Flush();
            str.Position = 0;
            var bytes = str.ToArray();
            var s = UTF8Encoding.UTF8.GetString(bytes);
        }


        private static Report CreateReport()
        {
            var result = new Report();
            result.ReportAttributes = ReportAttributeOption.DRGsDischargesFilters | ReportAttributeOption.IncludedHospitals;
            result.Audiences = new List<Audience> { Audience.Professionals, Audience.Consumers };
            result.Category = ReportCategory.Quality;
			result.IsTrending = false;
            result.Description = "this is a description";
            new ComparisonKeyIconSet(result, "Key 1");
            new ComparisonKeyIconSet(result, "Key 2");
            new ComparisonKeyIconSet(result, "Key 3");
            new ReportColumn(result, "col1");
            new ReportColumn(result, "col2");
            new ReportColumn(result, "col3");
            result.Footnote = "this is a footnote";
            result.Name = "my report";
            return result;
        }
    }

    static class ReportExtension
    {
        internal static ReportManifest AsManifest(this Report report)
        {
            var result = new ReportManifest();
            result.Audiences = report.Audiences.Select(a => new RptManifestAudience() {AudienceType = a}).ToList();
            result.Category = report.Category;
			result.IsTrending = report.IsTrending;
            result.Columns.AddRange(report.Columns.ToList().Select(c => new RptManifestColumn(((Report)c.Report).SourceTemplate, c.Name) {IsMeasure = c.IsMeasure, MeasureCode = c.MeasureCode}));
            result.Name = report.Name;
            result.ReportAttributes = report.ReportAttributes;
            return result;
        }
    }
}
