/**
 * Professional Product
 * Quality Ratings Module
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
  angular.module('monahrq.products.professional.quality-ratings')
    .controller('QRCompareCtrl', QRCompareCtrl);


  QRCompareCtrl.$inject = ['$scope', '$stateParams', 'QRQuerySvc', 'content'];
  function QRCompareCtrl($scope, $stateParams, QRQuerySvc, content) {
    $scope.content = content;
    $scope.reportId = null;

    QRQuerySvc.query.hospitals = $stateParams.hospitals;// ? $stateParams.hospitals.split(',') : [];

    QRQuerySvc.addReportChangeListener(onReportChange);
    $scope.$on('$destroy', function() {
      QRQuerySvc.removeReportChangeListener(onReportChange);
    });
    function onReportChange(reportId) {
      $scope.reportId = reportId;
    }

  }

})();
