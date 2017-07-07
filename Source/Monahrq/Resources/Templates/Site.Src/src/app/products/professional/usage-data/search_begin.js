/**
 * Professional Product
 * Usage Data Report Module
 * Search Begin Block Controller
 *
 * This controller builds the search UI for the utilization landing page
 *
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UDSearchBeginCtrl', UDSearchBeginCtrl);

  UDSearchBeginCtrl.$inject = ['$scope'];
  function UDSearchBeginCtrl($scope) {
    $scope.onlyOneSearch = $scope.ReportConfigSvc.webElementAvailable('Utilization_AHSFind_Button')
      != $scope.ReportConfigSvc.webElementAvailable('Utilization_ServiceUseFind_Button');
  }

})();


