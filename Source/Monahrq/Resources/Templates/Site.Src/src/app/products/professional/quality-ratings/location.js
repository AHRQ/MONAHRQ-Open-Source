/**
 * Professional Product
 * Quality Ratings Module
 * Location Page Controller
 *
 * This controller services as a simple wrapper around the search UI and report table.
 * It manages the CMS header and footer regions.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings')
    .controller('QRLocationCtrl', QRLocationCtrl);


    QRLocationCtrl.$inject = ['$scope', '$state', '$stateParams', 'QRQuerySvc', 'content'];
    function QRLocationCtrl($scope, $state, $stateParams, QRQuerySvc, content) {
      $scope.content = content;
      $scope.showTabs = showTabs;
      $scope.goToTable = goToTable;
      $scope.goToMap = goToMap;
      $scope.isActiveTab = isActiveTab;
      $scope.reportId = $state.current.data.report.symbols;

      QRQuerySvc.query.hospitalType = $stateParams.hospitalType;
      QRQuerySvc.query.searchType = $stateParams.searchType;
      QRQuerySvc.query.geoType = $stateParams.geoType;
      QRQuerySvc.query.zipDistance = $stateParams.zipDistance;
      QRQuerySvc.query.zip = $stateParams.zip;
      QRQuerySvc.query.region = $stateParams.region;


      function showTabs() {
        return _.isString($stateParams.displayType);
      }

      function goToTable() {
        var sp = QRQuerySvc.toStateParams();
        sp.displayType = 'table';
        $state.go('^.location', sp);
      }

      function goToMap() {
        var sp = QRQuerySvc.toStateParams();
        sp.displayType = 'map';
        $state.go('^.location', sp);
      }

      function isActiveTab(name) {
        return QRQuerySvc.query.displayType === name;
      }
    }

})();



