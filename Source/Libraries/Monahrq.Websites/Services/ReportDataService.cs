using Monahrq.Infrastructure.Services;

namespace Monahrq.Websites.Services
{
    //[Export]
    //public class ReportDataService
    //{
    //    IDomainSessionFactoryProvider Provider { get; set; }

    //    [ImportingConstructor]
    //    public ReportDataService(IDomainSessionFactoryProvider provider)
    //    {
    //        LazyAudiences = new Lazy<KeyValuePair<Audience, string>>(() =>
    //            {
    //                var values = Enum.GetValues(typeof(Audience)) as Audience[];
    //                return values.Select(v => new KeyValuePair<Audience, string>(v, v.
    //            }, true);
    //        Provider = provider;
    //    }

    //    Lazy<KeyValuePair<Audience, string>> LazyAudiences { get; set; }

    //    public IEnumerable<KeyValuePair<Audience, string>> GetAudiences()
    //    {

    //    }
    //}

    public interface IReportDataService : IDataServiceBase
    {
    }

    public class ReportDataService : DataServiceBase, IReportDataService
    {
        
    }
}
