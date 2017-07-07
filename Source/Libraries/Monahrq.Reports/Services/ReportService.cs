using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Services;
using Monahrq.Reports.Model;
using Monahrq.Reports.ViewModels;
using Monahrq.Sdk.Common;
using Monahrq.Infrastructure.Entities.Domain;
using NHibernate.Linq;

namespace Monahrq.Reports.Services
{
    //public enum FilterKeys { None, ReportName, ReportType, Website, RecommendedAudiences }

    //public interface IReportService
    //{
    //    IList<string> Categories { get; }
    //    IList<AudienceModel> Audiences { get; }
    //    IList<string> ReportTypes { get; }
    //    IList<KeyValuePair<FilterKeys, string>> FilterEnumerationsMainView { get; }
    //    IList<ReportDetailsViewModel> GetReports();
    //    void Delete<T>(T entity, Action<bool, Exception> callback) where T : class, IEntity;

    //    Report SaveNewReport(string title, ObservableCollection<AudienceModel> audiences, string reportType);
    //    Report GetReportById(Guid id);
    //    ReportViewModel CurrentReport { get; set; }
    //    bool SaveReport(Report report);
    //    IList<string> GetWebsitesForReport(Guid reportId);
    //}

    //[Export(typeof(IReportService))]
    //public class ReportService : DataServiceBase, IReportService
    //{
    //    public ReportService()
    //    {
    //        var m = _getModelFromEnums<Audience, AudienceModel>();
    //        InitLazys();
    //    }

    //    private void InitLazys()
    //    {
    //        LazyFilterEnumerationsMainView = new Lazy<IList<KeyValuePair<FilterKeys, string>>>(() =>
    //            new List<KeyValuePair<FilterKeys, string>>()
    //            {
    //                new KeyValuePair<FilterKeys, string>(FilterKeys.None, string.Empty),
    //                new KeyValuePair<FilterKeys, string>(FilterKeys.ReportName, "Report Name"),
    //                new KeyValuePair<FilterKeys, string>(FilterKeys.ReportType, "Report Type"),
    //                new KeyValuePair<FilterKeys, string>(FilterKeys.Website, "Website"),
    //                new KeyValuePair<FilterKeys, string>(FilterKeys.RecommendedAudiences, "Recommended Audiences")
    //            }, true);
    //    }

    //    public IList<ReportDetailsViewModel> GetReports()
    //    {
    //        WaitCursor.Show();
    //        var list = new List<ReportDetailsViewModel>();

    //        GetAll<Report>((result, e) =>
    //        {
    //            if (e == null)
    //            {
    //                if (result != null && result.Any())
    //                {
    //                    result = result.OrderBy(x => x.Category).ThenBy(x => x.Name).ToList();
    //                }
    //                if (result != null) list.AddRange(result.ToList().Select(r => new ReportDetailsViewModel(r)));
    //            }
    //            else
    //            {
    //                EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
    //            }
    //        });
    //        return list;
    //    }

    //    public Report SaveNewReport(string title, ObservableCollection<AudienceModel> audiences, string reportType)
    //    {
    //        WaitCursor.Show();
    //        var factory = new ReportManifestFactory();
    //        var manifest = factory.InstalledManifests.FirstOrDefault(x => x.Name == reportType);

    //        var report = new Report(manifest);

    //        report.Name = title;

    //        // sum the bitflag values of each audience type that is selected, then set report.Audiences (of type enum Audience)
    //        uint aa = audiences.Where(a => a.IsSelected).ToList().Aggregate<AudienceModel, uint>(0, (current, a) => current + (uint)a.Value);

    //        report.Audiences = (Audience)aa;

    //        SaveReport(report);
    //        //Save(report, (o, e) =>
    //        //    {
    //        //        if (e != null)
    //        //        {
    //        //            EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
    //        //            report = null;
    //        //        }
    //        //    });

    //        return report;
    //    }

    //    public Report GetReportById(Guid id)
    //    {
    //        WaitCursor.Show();
    //        var report = new Report();
    //        GetEntityById<Report>(id, (o, e) =>
    //            {
    //                if (e == null)
    //                {
    //                    report = o;
    //                }
    //                else
    //                {
    //                    EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
    //                    report = null;
    //                }
    //            });

    //        return report;
    //    }

    //    public ReportViewModel CurrentReport { get; set; }
    //    public bool SaveReport(Report report)
    //    {
    //        //var isSaved = true;
    //        try
    //        {
    //            using (var session = GetSession())
    //            {
    //                using (var tx = session.BeginTransaction())
    //                {
    //                    session.Evict(report);

    //                    if (!report.IsPersisted)
    //                        session.SaveOrUpdate(report);
    //                    else
    //                        session.Merge(report);

    //                    session.Flush();

    //                    tx.Commit();

    //                    //if (tx.WasCommitted)
    //                    //{
    //                    //    session.Refresh(entity);
    //                    //    result = entity;
    //                    //}
    //                }
    //            }

    //            return true;
    //        }
    //        catch (Exception e)
    //        {
    //            Log(e, "Saving to DB", new EntityDescriptor(report));
    //            EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
    //            return false;
    //        }

    //        //callback(result, error);

    //        //Save(report, (o, e) =>
    //        //    {
    //        //        if (e == null)
    //        //        {
    //        //            IsSaved = true;
    //        //        }
    //        //        else
    //        //        {
    //        //            EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
    //        //        }
    //        //    });

    //        //return IsSaved;
    //    }


    //    public IList<string> Categories { get { return _getEnumDescriptors<ReportCategory>(); } }

    //    public IList<AudienceModel> Audiences { get { return _getModelFromEnums<Audience, AudienceModel>(); } }

    //    public IList<string> ReportTypes { get { return _getReportTypes(); } }

    //    public IList<KeyValuePair<FilterKeys, string>> FilterEnumerationsMainView
    //    {
    //        get
    //        {
    //            return LazyFilterEnumerationsMainView.Value;
    //        }
    //    }


    //    Lazy<IList<KeyValuePair<FilterKeys, string>>> LazyFilterEnumerationsMainView { get; set; }


    //    #region private helper methods

    //    private IList<AudienceModel> _getAudienceModels()
    //    {
    //        return (from Audience val in Enum.GetValues(typeof(Audience)) select new AudienceModel(val)).ToList();
    //    }

    //    private IList<TModel> _getModelFromEnums<TEnum, TModel>()
    //    {
    //        return (from TEnum val in Enum.GetValues(typeof(TEnum)) select (TModel)Activator.CreateInstance(typeof(TModel), new object[] { val })).ToList();
    //    }

    //    private IList<string> _getEnumDescriptors<T>()
    //    {
    //        var list = new List<string>();

    //        foreach (
    //            var members in from object val in Enum.GetValues(typeof(T)) select typeof(T).GetMember(val.ToString())
    //            )
    //        {
    //            list.AddRange(
    //                members.Select(memberInfo => memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false))
    //                       .Select(att => ((DescriptionAttribute)att[0]).Description));
    //        }
    //        return list;
    //    }

    //    private IEnumerable<string> _getReportTemplateFilePaths()
    //    {
    //        var files = new List<string>();
    //        var directory = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\BaseData\Reports");

    //        try
    //        {
    //            files.AddRange(directory.EnumerateFiles().Select(file => file.FullName));
    //        }

    //        catch (UnauthorizedAccessException uaEx)
    //        {
    //            EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(uaEx);
    //        }
    //        catch (PathTooLongException pathEx)
    //        {
    //            EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(pathEx);

    //        }

    //        return files;
    //    }

    //    private string _getFileContent(string path)
    //    {
    //        try
    //        {
    //            using (var sr = new StreamReader(path))
    //            {

    //                var line = sr.ReadToEnd();
    //                return line;
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
    //        }

    //        return null;
    //    }

    //    private IList<string> _getReportTypes()
    //    {
    //        try
    //        {
    //            //return (from file in _getReportTemplateFilePaths()
    //            //        select _getFileContent(file)
    //            //        into result select ReportManifest.Deserialize(result)
    //            //        into report select report.Name).ToList();

    //            var factory = new ReportManifestFactory();
    //            return factory.InstalledManifests.Select(manifest => manifest.Name).ToList();
    //        }
    //        catch (Exception e)
    //        {

    //            EventAggregator.GetEvent<ErrorNotificationEvent>().Publish(e);
    //        }

    //        return null;
    //    }

    //    #endregion

    //    /// <summary>
    //    /// Gets the websites for a specfified report.
    //    /// </summary>
    //    /// <param name="reportId">The report identifier.</param>
    //    /// <returns></returns>
    //    public IList<string> GetWebsitesForReport(Guid reportId)
    //    {
    //        using (var session = GetSession())
    //        {
    //            return session.Query<Website>()
    //                          .Where(x => x.Reports.Any(wm => wm.Report.Id == reportId))
    //                          .Select(w => w.Name).ToList();
    //        }
    //    }

    // }
}
