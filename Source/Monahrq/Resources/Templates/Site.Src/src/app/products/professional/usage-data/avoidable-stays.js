/**
 * Professional Product
 * Usage Data Report Module
 * Avoidable Stays Page Controller
 *
 * This controller services as a simple wrapper around the search and report views. It manages the CMS
 * header and footer regions.
 *
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UDAvoidableStaysCtrl', UDAvoidableStaysCtrl);


  UDAvoidableStaysCtrl.$inject =  ['$scope', '$state', '$stateParams', 'UDAHSQuerySvc'];
  function UDAvoidableStaysCtrl($scope, $state, $stateParams, UDAHSQuerySvc) {
    $scope.reportId = null;

    UDAHSQuerySvc.addReportChangeListener(onReportChange);
    $scope.$on('$destroy', function() {
      UDAHSQuerySvc.removeReportChangeListener(onReportChange);
    });
    function onReportChange(reportId) {
      $scope.reportId = reportId;
    }

  }

})();
