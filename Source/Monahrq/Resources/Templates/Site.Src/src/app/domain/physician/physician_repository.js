/**
 * Monahrq Nest
 * Core Domain Module
 * Physician Repository Service
 *
 * This service provides the function of searching for physicians by name
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('PhysicianRepositorySvc', PhysicianRepositorySvc);


  PhysicianRepositorySvc.$inject = ['$q', '$rootScope', 'ResourceSvc', 'PhysicianAPISvc', 'PhysicianReportLoaderSvc', 'PhysicianDedupeSvc'];
  function PhysicianRepositorySvc($q, $rootScope, ResourceSvc, PhysicianAPISvc, PhysicianReportLoaderSvc, PhysicianDedupeSvc) {

    /**
     * Service Interface
     */
    return {
      init: init,
      findByName: findByName
    };



    /**
     * Service Implementation
     */
    function init() {

    }

    function findByName(first, last) {
      var isRemote = $rootScope.config.USED_REAL_TIME == 1;
      var promise;

      if (isRemote) {
        promise = findByNameRemote(first, last);
      }
      else {
        promise = findByNameLocal(first, last);
      }

      return promise.then(function(physicians) {
        return PhysicianDedupeSvc.dedupe(physicians, PhysicianDedupeSvc.DEDUPE_MERGE);
      });
    }

    function findByNameLocal(first, last) {
      PhysicianReportLoaderSvc.init($rootScope.config);
      return PhysicianReportLoaderSvc.findByName(first, last)
        .then(function(result) {
          var physicians = _.flatten(_.map(result, function(row) {
            return row.data || [];
          }));
          return physicians;
        });
    }

    function findByNameRemote(first, last) {
      PhysicianAPISvc.init($rootScope.config);
      return PhysicianAPISvc.findByName(first, last);
    }

  }

})();
