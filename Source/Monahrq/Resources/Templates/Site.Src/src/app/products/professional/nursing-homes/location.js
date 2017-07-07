/**
 * Professional Product
 * Nursing Homes Module
 * Location Page Controller
 *
 * This controller serves as a simple wrapper around the report table. It manages the sub-report
 * tabs and the CMS header and footer regions.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.nursing-homes')
    .controller('NHLocationCtrl', NHLocationCtrl);


    NHLocationCtrl.$inject = ['$scope', '$state', '$stateParams', 'NHQuerySvc'];
    function NHLocationCtrl($scope, $state, $stateParams, NHQuerySvc) {
      $scope.content = {};
      $scope.showTabs = showTabs;
      $scope.goToTable = goToTable;
      $scope.goToMap = goToMap;
      $scope.isActiveTab = isActiveTab;
      $scope.reportId = $state.current.data.report;


      function showTabs() {
        return NHQuerySvc.isSearchable() && $stateParams.searchType;
      }

      function goToTable() {
        var sp = NHQuerySvc.toStateParams();
        sp.displayType = 'table';
        $state.go('top.professional.nursing-homes.location', sp);
      }

      function goToMap() {
        var sp = NHQuerySvc.toStateParams();
        sp.displayType = 'map';
        $state.go('top.professional.nursing-homes.location', sp);
      }

      function isActiveTab(name) {
        return NHQuerySvc.query.displayType === name;
      }
    }

})();



