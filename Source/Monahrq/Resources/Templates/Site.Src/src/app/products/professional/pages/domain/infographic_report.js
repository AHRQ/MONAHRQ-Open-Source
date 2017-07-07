/**
 * Professional Product
 * Pages Domain Module
 * Infographic Report Service
 *
 * This service provides methods for loading the various infographic reports:
 * - Surgical safety
 * - Generic infographic
 * - Nursing home
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.pages.domain')
    .factory('InfographicReportSvc', InfographicReportSvc);


  InfographicReportSvc.$inject = ['$q', 'DataLoaderSvc'];
  function InfographicReportSvc($q, DataLoaderSvc) {
    /**
     * Private Data
     */
    var opt = {
      reportDir: 'Data/Reports'
    };

    $.monahrq = $.monahrq || {};


    /**
     * Service Interface
     */
    return {
      getReport: getReport,
      getGenericReport: getGenericReport,
      getNursingHomeReport: getNursingHomeReport
    };


    /**
     * Service Implementation
     */
    function getUrl() {
      return  opt.reportDir + '/Infographic.js';
    }

    function getReport() {
      var deferred, report, url;
      deferred = $q.defer();

      url = getUrl();

      DataLoaderSvc.loadScript(url, function () {
        var data = $.monahrq['infographic'];

        if (_(data).isUndefined()) {
          return deferred.reject('Unable to load \"' + url + '\".');
        } else {
          return deferred.resolve(data);
        }

      }, function() {}, true);

      return deferred.promise;
    }

    function getGenericReport() {
      var deferred, report, url;
      deferred = $q.defer();

      url = opt.reportDir + '/GenericInfographic.js';

      DataLoaderSvc.loadScript(url, function () {
        var data = $.monahrq['generic_infographic'];

        if (_(data).isUndefined()) {
          return deferred.reject('Unable to load \"' + url + '\".');
        } else {
          return deferred.resolve(data);
        }

      }, function() {}, true);

      return deferred.promise;
    }

    function getNursingHomeReport() {
      var deferred, report, url;
      deferred = $q.defer();

      url = opt.reportDir + '/NursingHomeInfographic.js';

      DataLoaderSvc.loadScript(url, function () {
        var data = $.monahrq['nursinghome_infographic'];

        if (_(data).isUndefined()) {
          return deferred.reject('Unable to load \"' + url + '\".');
        } else {
          return deferred.resolve(data);
        }

      }, function() {}, true);

      return deferred.promise;
    }
  }

})();

