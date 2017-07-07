using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using Monahrq.Infrastructure.Domain.BaseData;
using Monahrq.Infrastructure.Domain.BaseData.ViewModel;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using NHibernate.Linq;

namespace Monahrq.Infrastructure.Services.BaseData
{
    /// <summary>
    /// The base data service.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Services.DataServiceBase" />
    /// <seealso cref="Monahrq.Infrastructure.Services.BaseData.IBaseDataService" />
    [Export(typeof(IBaseDataService))]
    public class BaseDataService : DataServiceBase, IBaseDataService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDataService"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        [ImportingConstructor]
        public BaseDataService(IDomainSessionFactoryProvider provider)
        { }

        /// <summary>
        /// Gets the reporting quarters.
        /// </summary>
        /// <value>
        /// The reporting quarters.
        /// </value>
        public ObservableCollection<EntityViewModel<ReportingQuarter, int>> ReportingQuarters
        {
            get
            {
                using (var session = GetStatelessSession())
                {
                    var temp = session.Query<ReportingQuarter>()
                                      .OrderBy(o => o.Name)
                                      .Select(quarter => new EntityViewModel<ReportingQuarter, int>(quarter))
                                      .ToList();

                    // TODO: don't add null item here, use Jason's SelectListItem class and add friendly string for item 0
                    temp.Add(new EntityViewModel<ReportingQuarter, int>(null));

                    temp.Sort();
                    return new ObservableCollection<EntityViewModel<ReportingQuarter, int>>(temp);
                }
            }
        }

        /// <summary>
        /// Gets the reporting years.
        /// </summary>
        /// <value>
        /// The reporting years.
        /// </value>
        public ObservableCollection<string> ReportingYears
        {
            // return a list of years from 2009 up to and including the current (run-time) year
            get
            {
                const int FIRST_YEAR = 2009;
                return Enumerable.Range(FIRST_YEAR, DateTime.Now.Year - FIRST_YEAR + 1)
                                .Select(y => y.ToString())
                                .ToObservableCollection<string>();
            }
        }

        /// <summary>
        /// Gets the measure filter lookup.
        /// </summary>
        /// <value>
        /// The measure filter lookup.
        /// </value>
        public ObservableCollection<EntityViewModel<MeasureFilter, int>> MeasureFilterLookup
        {
            get
            {
                using (var session = GetStatelessSession())
                {
                    var temp = session.Query<MeasureFilter>()
                                      .Select(quarter => new EntityViewModel<MeasureFilter, int>(quarter))
                                      .ToList();

                    temp.Add(new EntityViewModel<MeasureFilter, int>(null));
                    temp.Sort();
                    return new ObservableCollection<EntityViewModel<MeasureFilter, int>>(temp);
                }
            }
        }

        /// <summary>
        /// Gets the target lookup.
        /// </summary>
        /// <value>
        /// The target lookup.
        /// </value>
        public ObservableCollection<EntityViewModel<Target, int>> TargetLookup
        {
            get
            {
                using (var session = GetStatelessSession())
                {
                    var temp = session.Query<Target>()
                                      .OrderBy(o => o.Name)
                                      .Select(target => new EntityViewModel<Target, int>(target))
                                      .ToList();

                    temp.Add(new EntityViewModel<Target, int>(null));
                    temp.Sort();

                    return new ObservableCollection<EntityViewModel<Target, int>>(temp);
                }
            }
        }

        /// <summary>
        /// Searches the targets.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public ObservableCollection<EntityViewModel<Target, int>> SearchTargets(Expression<Func<Target, bool>> criteria)
        {
            using (var session = GetStatelessSession())
            {
                var temp = session.Query<Target>()
                            .Where(criteria)
                            .OrderBy(o => o.Name)
                            .Select(target => new EntityViewModel<Target, int>(target))
                            .ToList();

                temp.Add(new EntityViewModel<Target, int>(null));
                temp.Sort();
                return new ObservableCollection<EntityViewModel<Target, int>>(temp);
            }
        }

        /// <summary>
        /// Stateses the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public ObservableCollection<EntityViewModel<State, int>> States(Expression<Func<State, bool>> criteria)
        {
            var temp = new List<EntityViewModel<State, int>>();
            using (var session = GetStatelessSession())
            {
                var query = criteria != null
                    ? session.Query<State>().Where(criteria)
                    : session.Query<State>();

                temp = query.OrderBy(o => o.Name)
                        .Select(target => new EntityViewModel<State, int>(target))
                        .ToList();
            }

            temp.Add(new EntityViewModel<State, int>(null));
            temp.Sort();

            return new ObservableCollection<EntityViewModel<State, int>>(temp);
        }

        /// <summary>
        /// Countieses the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public ObservableCollection<KeyValuePair<int, string>> Counties(Expression<Func<County, bool>> criteria)
        {
            var result = new List<KeyValuePair<int, string>>();
            using (var session = GetStatelessSession())
            {
                var temp = session.Query<County>()
                                  .Where(criteria)
                                  .OrderBy(o => o.Name)
                                  .ToList();

                if (!temp.Any())
                {
                    result.Add(new KeyValuePair<int, string>(0, "No Counties Avaliavble."));
                }
                else
                {
                    result.Add(new KeyValuePair<int, string>(0, "Select county."));
                    result.AddRange(temp.Select(c => new KeyValuePair<int, string>(c.Id, c.Name)));
                }

                return result.ToObservableCollection();
            }
        }
    }
}
