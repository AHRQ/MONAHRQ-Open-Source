/**
 * Professional Product
 * Physicians Domain Module
 * Physician Query Service
 *
 * This service provides a simple model for processing and storing search parameters
 * from the page URL.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.physicians.domain')
    .factory('PhysicianQuerySvc', PhysicianQuerySvc);


  function PhysicianQuerySvc() {
    /**
     * Private Data
     */
    var queryTpl = {
        searchType: null,
        firstName: null,
        lastName: null,
        practiceName: null,
        city: null,
        zip: null,
        specialty: null,
        condition: null
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
      query.name = sp.name;
      query.firstName = sp.firstName;
      query.lastName = sp.lastName;
      query.practiceName = sp.practiceName;
      query.city = sp.city;
      query.zip = sp.zip;
      query.specialty = sp.specialty;
      query.condition = sp.condition;
    }

    function toStateParams() {

      var sp = {
        searchType: query.searchType,
        firstName: query.firstName,
        lastName: query.lastName,
        practiceName: query.practiceName,
        city: query.city,
        zip: query.zip,
        specialty: query.specialty,
        condition: query.condition
      };

      return sp;
    }

    function isSearchable() {
      var result = true;

      if (query.searchType === 'name') {
        result = query.firstName || query.lastName;
      }
      else {
        result = query[query.searchType] != null;
      }

      return result;
    }

  }

})();
