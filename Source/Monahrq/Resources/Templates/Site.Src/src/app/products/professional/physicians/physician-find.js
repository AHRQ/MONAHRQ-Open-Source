/**
 * Professional Product
 * Physicians Module
 * Find Physician Page Controller
 *
 * This controller services as a simple wrapper around the search UI and report table. It manages the CMS
 * header and footer regions.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.physicians')
    .controller('PhysicianFindCtrl', PhysicianFindCtrl);


    PhysicianFindCtrl.$inject = ['$scope', '$state', '$stateParams', 'PhysicianQuerySvc'];
    function PhysicianFindCtrl($scope, $state, $stateParams, PhysicianQuerySvc) {
      $scope.content = {};
      $scope.reportId = $state.current.data.report;
    }

})();



