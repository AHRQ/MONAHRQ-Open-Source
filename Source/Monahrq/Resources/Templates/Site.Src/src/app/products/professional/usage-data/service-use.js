/**
 * Professional Product
 * Usage Data Report Module
 * Utilization Report Page Controller
 *
 * This controller services as a simple wrapper around the search and report views. It manages the CMS
 * header and footer regions.
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UDServiceUseCtrl', UDServiceUseCtrl);

  UDServiceUseCtrl.$inject = ['$scope', '$state', '$stateParams', 'UDUtilQuerySvc'];
  function UDServiceUseCtrl($scope, $state, $stateParams, UDUtilQuerySvc) {
    $scope.reportId = null;

    UDUtilQuerySvc.addReportChangeListener(onReportChange);
    $scope.$on('$destroy', function() {
      UDUtilQuerySvc.removeReportChangeListener(onReportChange);
    });
    function onReportChange(reportId) {
      $scope.reportId = reportId;
    }

    $scope.getWebElementForType = function(name) {
      var elements, reportType;

      elements = {
        id: {
          table: 'IP_ViewRates_Tab',
          map: 'IP_MapView_Tab'
        },
        ed: {
          table: 'ED_ViewRates_Tab',
          map: 'ED_MapView_Tab'
        },
        county: {
          table: 'County_ViewRates_Tab',
          map: 'County_MapView_Tab'
        },
        region: {
          table: 'Region_ViewRates_Tab',
          map: 'Region_MapView_Tab'
        }
      };

      reportType = UDUtilQuerySvc.query.groupBy.reportType;
      if (!reportType) return null;
      return elements[reportType][name];
    }
  }

})();

