/**
 * Professional Product
 * Usage Data Domain Module
 * Utilization Query Service
 *
 * This service provides a simple model for processing and storing search parameters
 * from the page URL.
 */
angular.module('monahrq.domain')
  .factory('UDUtilQuerySvc', function () {
  /**
   * Private Data
   */
  var queryTpl = {
    groupBy: {
      viewBy: null,
      reportType: null,
      groupBy: null,
      dimension: null,
      value: null,
      value2: null
    },
    viewBy: null,
    sortBy: null,
    narrowBy: {
      name: null,
      value: null,
      value2: null
    },
    displayType: null,
    level: {
      type: null,
      value: null
    }
  },
  query,
  reportChangeListeners = [];

  var hasReportData = true;

  var dimensionFields = {
    'region': 'RegionID',
    'patientregion': 'RegionID',
    'hospitalType': 'HospitalTypeID'
  };

  init();


  /**
   * Service Interface
   */
  return {
    query: query,
    reset: reset,
    fromStateParams: fromStateParams,
    toStateParams: toStateParams,
    setDimension: setDimension,
    setValue: setValue,
    setNarrowValue: setNarrowValue,
    hasReportData: getHasReportData,
    setHasReportData: setHasReportData,

    notifyReportChange: notifyReportChange,
    addReportChangeListener: addReportChangeListener,
    removeReportChangeListener: removeReportChangeListener
  };


  /**
   * Service Implementation
   */
  function init() {
    query = angular.copy(queryTpl);
  }

  function reset() {
    angular.copy(queryTpl, query);
    hasReportData = true;
  }

  function fromStateParams(sp) {
    reset();
    query.groupBy.viewBy = sp.viewBy;
    query.groupBy.reportType = sp.reportType;
    query.groupBy.groupBy = sp.groupBy;
    query.groupBy.dimension = sp.dimension;
    query.groupBy.value = +sp.value;
    query.groupBy.value2 = sp.value2;
    query.displayType = sp.displayType;
    query.level.type = sp.levelType;
    query.level.value = sp.levelValue;
    query.viewBy = sp.levelViewBy;

    query.groupBy.dimensionSel = {id: sp.dimension};
    var df = dimensionFields[sp.dimension];
    query.groupBy.valueSel = {};
    query.groupBy.valueSel[df] = +sp.value;
    query.groupBy.value2Sel = sp.value2;
    query.narrowBy.valueSel = {};
  }

  function toStateParams() {
    var sp = {
      viewBy: query.groupBy.viewBy,
      reportType: query.groupBy.reportType,
      groupBy: query.groupBy.groupBy,
      dimension: query.groupBy.dimension,
      value: query.groupBy.value,
      value2: query.groupBy.value2,
      displayType: query.displayType,
      levelType: query.level.type,
      levelValue: query.level.value,
      levelViewBy: query.viewBy
    };

    return sp;
  }

  function setDimension(val) {
    query.groupBy.dimension = val;
    query.groupBy.dimensionSel = {id: val};
    return query.groupBy.dimensionSel;
  }

  function setValue(field, val) {
    query.groupBy.value = val;

    if (field == null) {
      query.groupBy.valueSel = {};
    }
    else if (_.contains(dimensionFields, field)) {
      query.groupBy.valueSel = {};
      query.groupBy.valueSel[field] = val;
    }

    return query.groupBy.valueSel;
  }

  function setNarrowValue(field, val) {
    query.narrowBy.value = val;

    if (field == null) {
      query.narrowBy.valueSel = {};
    }
    else if (_.contains(dimensionFields, field)) {
      query.narrowBy.valueSel = {};
      query.narrowBy.valueSel[field] = val;
    }

    return query.narrowBy.valueSel;
  }

  function getHasReportData() {
    return hasReportData;
  }

  function setHasReportData(b) {
    hasReportData = b;
  }

  function notifyReportChange(reportId) {
    _.each(reportChangeListeners, function(fn) {
      if (_.isFunction(fn)) {
        fn(reportId);
      }
    });
  }

  function addReportChangeListener(fn) {
    reportChangeListeners.push(fn);
  }

  function removeReportChangeListener(fn) {
    reportChangeListeners = _.without(reportChangeListeners, fn);
  }

});

