/**
 * Professional Product
 * Usage Data Report Module
 * Usage Data Page Controller
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UsageDataCtrl', UsageDataCtrl);

  UsageDataCtrl.$inject = ['$scope', '$state', 'content'];
  function UsageDataCtrl($scope, $state, content) {
    $scope.content = content;

    $scope.showUDSearch = $scope.ReportConfigSvc.webElementAvailable('Utilization_AHSFind_Button')
      || $scope.ReportConfigSvc.webElementAvailable('Utilization_ServiceUseFind_Button');
  }

})();

