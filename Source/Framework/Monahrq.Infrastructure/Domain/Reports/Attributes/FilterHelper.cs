using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Infrastructure.Entities.Domain.Reports.Attributes
{

    using Monahrq.Infrastructure.Utility.Extensions;

    public static class ReportFilterHelper
    {

        static Lazy<IEnumerable<ReportFilter>> LazyFilterGroups { get; set; }
        static Lazy<IDictionary<ReportFilter, string>> LazyFilterCaptions { get; set; }
        static Lazy<IDictionary<ReportFilter, IList<ReportFilter>>> LazyFilterGroupValues { get; set; }
        static Lazy<ReportFilter> LazyFilterGroupFlag { get; set; }

        static ReportFilterHelper()
        {
            LazyFilterGroups = new Lazy<IEnumerable<ReportFilter>>(() =>
                {
                    var vals = new List<ReportFilter>();
                    foreach (var val in Enum.GetValues(typeof(ReportFilter)).OfType<ReportFilter>().Where(val => val != ReportFilter.None))
                    {
                        var temp = ExtractGroup(val);
                        if (temp == val && !vals.Contains(val))
                        {
                            vals.Add(val);
                        }
                    }
                    return vals;
                }, true);

            LazyFilterCaptions = new Lazy<IDictionary<ReportFilter, string>>(() =>
            {
                var values = Enum.GetValues(typeof(ReportFilter)).OfType<ReportFilter>();
                ReportFilter filters = default(ReportFilter);
                values.ToList().ForEach(filter => filters |= filter);
                var descriptions = filters.GetAttributeValues<System.ComponentModel.DescriptionAttribute, string>(x => x.Description);
                return descriptions.ToDictionary(tuple => (ReportFilter)tuple.Item1, value => value.Item2);
            }, true);

            LazyFilterGroupValues = new Lazy<IDictionary<ReportFilter, IList<ReportFilter>>>(() =>
                {
                    var result = FilterGroups.ToDictionary(k => k, v => new List<ReportFilter>() as IList<ReportFilter>);
                    var values = Enum.GetValues(typeof(ReportFilter)) as ReportFilter[];
                    result.Keys.ToList().ForEach(group =>
                        {
                            var toAdd = values.Where(
                                (filter) =>
                                {
                                    var isGroup = filter.IsGroup();
                                    if (!isGroup)
                                    {
                                        var isGroupValue = (group & filter) == group;
                                        return isGroupValue;
                                    }
                                    return false;
                                });
                            (result[group] as List<ReportFilter>).AddRange(toAdd);
                        });
                    return result;
                }, true);

            LazyFilterGroupFlag = new Lazy<ReportFilter>(() =>
                {
                    var temp = default(ReportFilter);
                    LazyFilterGroups.Value.ToList()
                        .ForEach(group => temp |= group);
                    return temp;
                }, true);

        }

        public static IEnumerable<ReportFilter> FilterGroups
        {
            get
            {
                return LazyFilterGroups.Value;
            }
        }

        public static string GetCaption(this ReportFilter filter)
        {
            string result = string.Empty;
            if (!LazyFilterCaptions.Value.TryGetValue(filter, out result))
            {
                result = filter.ToString();
            }
            return string.IsNullOrEmpty(result) ? filter.ToString() : result;
        }

        public static IEnumerable<ReportFilter> GetValuesForGroup(this ReportFilter filter)
        {
            var result = new List<ReportFilter>();
            foreach (var group in FilterGroups)
            {
                if ((group & filter) == group)
                {
                    result.AddRange(LazyFilterGroupValues.Value[group]);
                }
            }
            return result;
        }

        public static ReportFilter ExtractGroup(this ReportFilter filter)
        {
            return (ReportFilter)(((uint)filter) >> 16 << 16);
        }

        public static bool IsGroup(this ReportFilter filter)
        {
            return ExtractGroup(filter) == filter;
        }

        static IEnumerable<ReportFilter> Values(this ReportFilter source)
        {
            foreach (ReportFilter filter in Enum.GetValues(typeof(ReportFilter)))
            {
                var test = (filter & source);
                if (test != ReportFilter.None && !test.IsGroup() && test == filter)
                {
                    yield return filter;
                }
            }
            yield break;
        }

        public static ReportFilter ToReportFilter(this HashSet<ReportFilter> filterSet)
        {
            var result = default(ReportFilter);
            filterSet.ToList().ForEach(filter => result |= filter);
            return result;
        }

        public static HashSet<ReportFilter> ToHashSet(this ReportFilter filters)
        {
            return new HashSet<ReportFilter>(filters.Values());
        }
        public static List<FilterItem> ToReportFilterList(this ReportFilter filters)
        {
            return new List<FilterItem>(filters.Values().Select(f => new FilterItem(f,  null)));
        }
    }
}

