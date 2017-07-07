/**
 * Professional Product
 * Nursing Homes Module
 * Compare Page Controller
 *
 * This controller services as a simple wrapper around the report table. It manages the CMS
 * header and footer regions.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.nursing-homes')
    .controller('NHCompareCtrl', NHCompareCtrl);


  NHCompareCtrl.$inject = ['$scope', '$state', '$stateParams', 'NHQuerySvc', 'content'];
  function NHCompareCtrl($scope, $state, $stateParams, NHQuerySvc, content) {
    $scope.content = content;
    $scope.reportId = $state.current.data.report;

    NHQuerySvc.query.hospitals = $stateParams.hospitals;// ? $stateParams.hospitals.split(',') : [];
  }

})();
