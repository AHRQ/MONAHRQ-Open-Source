/**
 * Professional Product
 * Quality Ratings Module
 * Search Begin Block Controller
 *
 * This controller builds the search UI for the quality ratings landing page
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings')
    .controller('QRSearchBeginCtrl', QRSearchBeginCtrl);


  QRSearchBeginCtrl.$inject = ['$scope'];
  function QRSearchBeginCtrl($scope) {
    $scope.onlyOneSearch = $scope.ReportConfigSvc.webElementAvailable('Quality_ConditionTopicExplore_Button')
      != $scope.ReportConfigSvc.webElementAvailable('Quality_HospitalExplore_Button');
  }

})();

