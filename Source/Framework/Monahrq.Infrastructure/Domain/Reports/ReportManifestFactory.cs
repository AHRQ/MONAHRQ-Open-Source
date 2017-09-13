using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Infrastructure.Entities.Domain.Reports
{
    public class ReportManifestFactory
    {

        public string ReportFolder
        {
            get
            {
                var root = Path.GetDirectoryName(this.GetType().Assembly.Location);
                var path = Path.Combine(root, @"Domain\Reports\Data");
                return path;
            }
        }

        public IEnumerable<ReportManifest> InstalledManifests { get; private set; }

        public ReportManifestFactory()
        {
            var reportXmls = Directory.GetFiles(ReportFolder, "*.xml");
            var temp = new List<ReportManifest>();

            var logger = ServiceLocator.Current.GetInstance<ILogWriter>();
            reportXmls.ToList().ForEach(reportXml =>
                {
                    try
                    {
                        var fileInfo = new FileInfo(reportXml);
                        var xml = File.ReadAllText(reportXml);
                        var manifest = ReportManifest.Deserialize(xml);
                        manifest.FileLastModifiedDate = fileInfo.LastWriteTime == DateTime.MinValue ? new DateTime(2016, 01, 01) : fileInfo.LastWriteTime;

                        if (!temp.Any(t => t.Name.EqualsIgnoreCase(manifest.Name)))
                            temp.Add(manifest);
                    }
                    catch (Exception exc)
                    {
                        var excToUse = exc.InnerException ?? exc;
                        var errMessage =
                            string.Format(
                                "An exception occurred when loading report manifests. Please see below for error.{0}Message: {1}{0}Stack Trace: {2}",
                                Environment.NewLine, excToUse.Message, excToUse.StackTrace);

                        logger.Write(exc, "Error loading report manifest from file {0}", reportXml);

                    }
                });
            InstalledManifests = temp;
        }
    }
}
