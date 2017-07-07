/**
 * Professional Product
 * Nursing Homes Domain Module
 * Nursing Home Query Service
 *
 * This service provides a simple model for processing and storing search parameters
 * from the page URL.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.nursing-homes.domain')
    .factory('NHQuerySvc', NHQuerySvc);


  function NHQuerySvc() {
    /**
     * Private Data
     */
    var queryTpl = {
        searchType: null,
        name: null,
        type: null,
        inHospital: null,
        overallRating: null,
        location: null,
        county: null,
        zip: null,
        zipDistance: null,
        displayType: null
      },
      query;

    init();


    /**
     * Service Interface
     */
    return {
      query: query,
      reset: reset,
      fromStateParams: fromStateParams,
      toStateParams: toStateParams,
      isSearchable: isSearchable

/*      setTopic: setTopic,
      setSubtopic: setSubtopic,
      setRegion: setRegion,
      setHospitalType: setHospitalType,
      setSearchType: setSearchType,
      setGeoType: setGeoType,
      setMeasure: setMeasure
      */
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

      query.searchType = sp.searchType;
      //query.searchTypeSel = {id: sp.searchType};
      query.name = sp.name;
      query.type= sp.type ? +sp.type : null;
      //query.hospitalTypeSel = {HospitalTypeID: +sp.hospitalType};
      query.inHospital = sp.inHospital;
      query.overallRating = sp.overallRating;

      query.location = sp.location;
      //query.geoTypeSel = {id: sp.geoType};
      query.zip = sp.zip;
      query.zipDistance = sp.zipDistance ? +sp.zipDistance : null;
      query.county = sp.county ? +sp.county : null;
      //query.regionSel = {RegionID: query.region};

      query.displayType = sp.displayType;
    }

    function toStateParams() {

      var sp = {
        searchType: query.searchType,
        name: query.name,
        type: query.type,
        inHospital: query.inHospital,
        overallRating: query.overallRating,
        location: query.location,
        zip: query.zip,
        zipDistance: query.zipDistance,
        county: query.county,
        displayType: query.displayType
      };

      return sp;
    }

    function isSearchable() {
      var result = true;

      if (query.searchType != 'location') {
        result = query[query.searchType] != null;
      }
      else {
        result = query[query.location] != null;
      }

      return result;
    }

/*    function setTopic(val) {
      query.topic = val;
      query.topicSel = {TopicCategoryID: val};
      return query.topicSel;
    }

    function setSubtopic(val) {
      query.subtopic = val;
      query.subtopicSel = {TopicID: val};
      return query.subtopicSel;
    }

    function setRegion(val) {
      query.region = val;
      query.regionSel = {RegionID: val};
      return query.regionSel;
    }

    function setHospitalType(val) {
      query.hospitalType = val;
      query.hospitalTypeSel = {HospitalTypeID: query.hospitalType};
      return query.hospitalTypeSel;
    }

    function setSearchType(val) {
      query.searchType = val;
      query.searchTypeSel = {id: query.searchType};
      return query.searchTypeSel;
    }

    function setGeoType(val) {
      query.geoType = val;
      query.geoTypeSel = {id: query.geoType};
      return query.geoTypeSel;
    }

    function setMeasure(val) {
      query.measure = val;
      query.measureSel = {MeasureID: query.measure};
      return query.measureSel;
    }
    */

  }

})();
