/**
 * Professional Product
 * Quality Ratings Domain Module
 * Quality Reports Query Service
 *
 * This service provides a simple model for processing and storing search parameters
 * from the page URL.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings.domain')
    .factory('QRQuerySvc', QRQuerySvc);


  function QRQuerySvc() {
    /**
     * Private Data
     */
    var queryTpl = {
        topic: null,
        topicSel: null,
        subtopic: null,
        subtopicSel: null,
        searchType: null,
        hospitalName: null,
        hospitalType: null,
        geoType: null,
        zip: null,
        zipDistance: null,
        region: null,
        displayType: null
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
      setSubtopic: setSubtopic,
      setRegion: setRegion,
      setHospitalType: setHospitalType,
      setSearchType: setSearchType,
      setGeoType: setGeoType,
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

      query.topic = sp.topic ? +sp.topic : null;
      query.subtopic = sp.subtopic ? +sp.subtopic : null;
      query.searchType = sp.searchType;
      query.hospitalName = sp.hospitalName;
      query.hospitalType = sp.hospitalType ? +sp.hospitalType : null;
      query.geoType = sp.geoType;
      query.zip = sp.zip;
      query.zipDistance = sp.zipDistance ? +sp.zipDistance : null;
      query.region = sp.region ? +sp.region : null;
      query.displayType = sp.displayType ? sp.displayType : 'table'
    }

    function toStateParams() {

      var sp = {
        topic: query.topic,
        subtopic: query.subtopic,
        searchType: query.searchType,
        hospitalName: query.hospitalName,
        hospitalType: query.hospitalType,
        geoType: query.geoType,
        zip: query.zip,
        zipDistance: query.zipDistance,
        region: query.region,
        displayType: query.displayType
      };

      return sp;
    }

    function setTopic(val) {
      query.topic = val;
      return query.topic;
    }

    function setSubtopic(val) {
      query.subtopic = val;
      return query.subtopic;
    }

    function setRegion(val) {
      query.region = val;
      return query.region;
    }

    function setHospitalType(val) {
      query.hospitalType = val;
      return query.hospitalType;
    }

    function setSearchType(val) {
      query.searchType = val;
      return query.searchType;
    }

    function setGeoType(val) {
      query.geoType = val;
      return query.geoType;
    }

    function setMeasure(val) {
      query.measure = val;
      return query.measure;
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

  }

})();
