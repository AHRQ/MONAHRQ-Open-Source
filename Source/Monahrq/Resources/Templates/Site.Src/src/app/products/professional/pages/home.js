/**
 * Professional Product
 * Pages Module
 * Home Page
 *
 * This controller corresponds to the front page of the professional site.
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.pages')
    .controller('HomeCtrl', HomeCtrl);

  HomeCtrl.$inject = ['$scope', '$sce', 'content'];
  function HomeCtrl($scope, $sce, content) {
    $scope.content = content;

    if ($scope.config.products.professional.HOMEPAGE_VIDEO == 1) {
      $scope.videoUrl = $sce.trustAsResourceUrl($scope.config.products.professional.HOMEPAGE_VIDEO_URL);
    }

    $scope.showHomeSearch = $scope.ReportConfigSvc.webElementAvailable('Quality_ConditionTopicExplore_Button')
      || $scope.ReportConfigSvc.webElementAvailable('Quality_HospitalExplore_Button')
      || $scope.ReportConfigSvc.webElementAvailable('Physician_Explore_Button')
      || $scope.ReportConfigSvc.webElementAvailable('Nursing_Explore_Button')
      || $scope.ReportConfigSvc.webElementAvailable('Nursing_Content');
  }


})();


