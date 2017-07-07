/**
 * Professional Product
 * Usage Data Domain Module
 * AHS Query Service
 *
 * This service provides a simple model for processing and storing search parameters
 * from the page URL.
 */
angular.module('monahrq.domain')
.factory('UDAHSQuerySvc', function () {
  /**
   * Private Data
   */
  var queryTpl = {
    reportType: null,
    displayType:null,
    county: {
      county:null,
      topics:{},
    },
    topic: {
      topic:null,
      measure:null
    }
  },
  query,
  reportChangeListeners = [];

  init();


  /**
   * Service Interface
   */
  return {
    query: query,
    reset: reset,
    fromStateParams: fromStateParams,
    toStateParams: toStateParams,
    setTopic: setTopic,
    setMeasure: setMeasure,

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
  }

  function fromStateParams(sp) {
    reset();
    query.reportType = sp.reportType;
    query.displayType = sp.displayType;

    if (query.reportType === 'county') {
      var topics = {};
      if (sp.topics) {
        _.each(sp.topics.split(','), function(v) {
          topics[v] = true;
        });
      }

      query.county.county = +sp.county;
      query.county.topics = topics;
    }
    else if (query.reportType === 'topic') {
      query.topic.topic = +sp.topic;
      query.topic.topicSel = {id: +sp.topic};
      query.topic.measure = sp.measure;
      query.topic.measureSel = {id: sp.measure};
    }
  }

  function toStateParams() {
    if (query.reportType === 'county') {
      var topics = [], topicStr = null;
      _.each(query.county.topics, function(v, k) {
        if (v == true) topics.push(k);
      });

      topicStr = topics.join();

      var sp = {
        reportType: query.reportType,
        displayType: query.displayType,
        county: query.county.county,
        topics: topicStr
      };
    }
    else if (query.reportType === 'topic') {
      var sp = {
        reportType: query.reportType,
        displayType: query.displayType,
        topic: query.topic.topic,
        measure: query.topic.measure
      };
    }

    return sp;
  }

  function setTopic(val) {
    query.topic.topic = val;
    query.topic.topicSel = {id: val};
    return query.topic.topicSel;
  }

  function setMeasure(val) {
    query.topic.measure = val;
    query.topic.measureSel = {id: val};
    return query.topic.measureSel;
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

