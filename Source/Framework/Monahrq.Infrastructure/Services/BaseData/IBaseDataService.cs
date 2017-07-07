using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Monahrq.Infrastructure.Domain.BaseData;
using Monahrq.Infrastructure.Domain.BaseData.ViewModel;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Infrastructure.Services.BaseData
{
    /// <summary>
    /// The base data service interface.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.Services.IDataServiceBase" />
    public interface IBaseDataService : IDataServiceBase
    {
        /// <summary>
        /// Gets the reporting quarters.
        /// </summary>
        /// <value>
        /// The reporting quarters.
        /// </value>
        ObservableCollection<EntityViewModel<ReportingQuarter, int>> ReportingQuarters { get; }
        /// <summary>
        /// Gets the reporting years.
        /// </summary>
        /// <value>
        /// The reporting years.
        /// </value>
        ObservableCollection<string> ReportingYears { get; }
        /// <summary>
        /// Gets the measure filter lookup.
        /// </summary>
        /// <value>
        /// The measure filter lookup.
        /// </value>
        ObservableCollection<EntityViewModel<MeasureFilter, int>> MeasureFilterLookup { get; }
        /// <summary>
        /// Gets the target lookup.
        /// </summary>
        /// <value>
        /// The target lookup.
        /// </value>
        ObservableCollection<EntityViewModel<Target, int>> TargetLookup { get; }
        /// <summary>
        /// Searches the targets.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        ObservableCollection<EntityViewModel<Target, int>> SearchTargets(Expression<Func<Target, bool>> criteria);
        /// <summary>
        /// Stateses the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        ObservableCollection<EntityViewModel<State, int>> States(Expression<Func<State, bool>> criteria);
        /// <summary>
        /// Countieses the specified criteria.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        ObservableCollection<KeyValuePair<int, string>> Counties(Expression<Func<County, bool>> criteria);
    }
}
