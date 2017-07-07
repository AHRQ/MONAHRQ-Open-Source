using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Services;
using Monahrq.Websites.Model;
using Monahrq.Websites.ViewModels;
using Monahrq.Sdk.Common;
using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Websites.Services
{
    public interface IWebsiteReportDataService : IWebsiteDataService
    {
        IList<string> Categories { get; }
        IList<AudienceModel> Audiences { get; }
        IList<string> ReportTypes { get; }
        IList<string> FilterEnumerationsMainView { get; }
        IList<ReportViewModel> GetReports();
        void Delete<T>(T entity, Action<bool, Exception> callback) where T : INamedEntity;

        Report SaveNewReport(string title, ObservableCollection<AudienceModel> audiences, string reportType);
        Report GetReportById(Guid id);
        ReportViewModel CurrentReport { get; set; }
        bool SaveReport(Report report);
    }

    [Export(typeof(IWebsiteReportDataService))]
    public class WebsiteReportDataService : WebsiteDataService , IWebsiteReportDataService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteReportDataService"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        [ImportingConstructor]
        public WebsiteReportDataService(IDomainSessionFactoryProvider provider) : base(provider)
        {
            //var m = _getModelFromEnums<Audience, AudienceModel>();
        }

        public IList<ReportViewModel> GetReports()
        {
            WaitCursor.Show();
            var list = new List<ReportViewModel>();

            GetAll<Report>((result, e) =>
            {
                if (e == null)
                {
                    list.AddRange(result.Select(r => new ReportViewModel(r)));
                }
                else
                {
                    EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
                }
            });
            return list;
        }

        public Report SaveNewReport(string title, ObservableCollection<AudienceModel> audiences, string reportType)
        {
            WaitCursor.Show();
            var factory = new ReportManifestFactory();
            var manifest = factory.InstalledManifests.FirstOrDefault(x => x.Name == reportType);

            var report = new Report(manifest) {Name = title};

            // sum the bitflag values of each audience type that is selected, then set report.Audiences (of type enum Audience)
            var aa = audiences.Where(a => a.IsSelected).Aggregate<AudienceModel, uint>(0, (current, a) => current + (uint) a.Value);

            report.Audiences = (Audience)aa;
          
            Save(report,(o, e) =>
                {
                    if (e == null) return;

                    EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
                    report = null;
                });

            return report;
        }

        public Report GetReportById(Guid id)
        {
            WaitCursor.Show();
            var report = new Report();
            GetEntityById<Report>(id, (o, e) =>
                {
                   if (e == null)
                   {
                       report = o;
                   }
                   else
                   {
                       EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
                       report = null;
                   }
                });

            return report;
        }

        public ReportViewModel CurrentReport { get; set; }
        public bool SaveReport(Report report)
        {
            var isSaved = true;
            Save(report,(o, e) =>
                {
                    if (e == null)
                    {
                        isSaved = true;
                    }
                    else
                    {
                        EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
                    }
                });

            return isSaved;
        }


        public IList<string> Categories { get { return _getEnumDescriptors<ReportCategory>(); }}

        public IList<AudienceModel> Audiences { get { return _getModelFromEnums<Audience, AudienceModel>(); } }

        public IList<string> ReportTypes { get { return _getReportTypes();}}

        public IList<string> FilterEnumerationsMainView
        {
            get
            {
                // these names appear in the Reports filter dropdown per I3 spec:
                return new List<string> { "Report Name", "Report Type", "Website", "Recommended Audiences" };
            }
        }

        #region private helper methods

        private IList<TModel> _getModelFromEnums<TEnum, TModel>()
        {
            return (from TEnum val in Enum.GetValues(typeof (TEnum)) select (TModel) Activator.CreateInstance(typeof (TModel), new object[] {val})).ToList();
        }

        private IList<string> _getEnumDescriptors<T>()
        {
            var list = new List<string>();

            foreach (
                var members in from object val in Enum.GetValues(typeof (T)) select typeof (T).GetMember(val.ToString())
                )
            {
                list.AddRange(
                    members.Select(memberInfo => memberInfo.GetCustomAttributes(typeof (DescriptionAttribute), false))
                           .Select(att => ((DescriptionAttribute) att[0]).Description));
            }
            return list;
        }

        private IEnumerable<string> _getReportTemplateFilePaths()
        {
            var files = new List<string>();
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\BaseData\Reports");

            try
            {
                files.AddRange(directory.EnumerateFiles().Select(file => file.FullName));
            }

            catch (UnauthorizedAccessException uaEx)
            {
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(uaEx);
            }
            catch (PathTooLongException pathEx)
            {
                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(pathEx);

            }

            return files;
        }

        //private string _getFileContent(string path)
        //{
        //    try
        //    {
        //        using (var sr = new StreamReader(path))
        //        {

        //            var line = sr.ReadToEnd();
        //            return line;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
        //    }

        //    return null;
        //}

        private IList<string> _getReportTypes()
        {
            try
            {
                //return (from file in _getReportTemplateFilePaths()
                //        select _getFileContent(file)
                //        into result select ReportManifest.Deserialize(result)
                //        into report select report.Name).ToList();

                var factory = new ReportManifestFactory();
               return factory.InstalledManifests.Select(manifest => manifest.Name).ToList();
            }
            catch (Exception e)
            {

                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
            }

            return null;
        }

        #endregion

    }
}
