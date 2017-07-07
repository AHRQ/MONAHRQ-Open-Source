/**
 * Professional Product
 * Quality Ratings Module
 * Condition Page Controller
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
    .controller('QRConditionCtrl', QRConditionCtrl);


  QRConditionCtrl.$inject = ['$scope', '$stateParams', 'QRQuerySvc', 'content'];
  function QRConditionCtrl($scope, $stateParams, QRQuerySvc, content) {
    $scope.content = content;
    $scope.reportId = null;

    QRQuerySvc.query.topic = $stateParams.topic;
    QRQuerySvc.query.subtopic = $stateParams.subtopic;
    QRQuerySvc.query.searchType = $stateParams.searchType;
    QRQuerySvc.query.hospitalName = $stateParams.hospitalName;
    QRQuerySvc.query.hospitalType = $stateParams.hospitalType;
    QRQuerySvc.query.geoType = $stateParams.geoType;
    QRQuerySvc.query.zip = $stateParams.zip;
    QRQuerySvc.query.zipDistance= $stateParams.zipDistance;
    QRQuerySvc.query.region = $stateParams.region;

    QRQuerySvc.addReportChangeListener(onReportChange);
    $scope.$on('$destroy', function() {
      QRQuerySvc.removeReportChangeListener(onReportChange);
    });
    function onReportChange(reportId) {
      $scope.reportId = reportId;
    }

  }

})();

