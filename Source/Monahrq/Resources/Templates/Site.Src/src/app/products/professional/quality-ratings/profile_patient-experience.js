/**
 * Professional Product
 * Quality Ratings Module
 * Profile Page Quality Tab PE Table Controller
 *
 * This controller generates the report for patient experience quality ratings for
 * a given hospital.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings')
    .controller('QRProfilePatientExperienceCtrl', QRProfilePatientExperienceCtrl);


  QRProfilePatientExperienceCtrl.$inject = ['$scope', '$q', 'QRReportSvc', 'ResourceSvc'];
  function QRProfilePatientExperienceCtrl($scope, $q, QRReportSvc, ResourceSvc) {

    loadData();

    function loadData() {
      var promises = [];
      promises.push(ResourceSvc.getMeasureDefs([$scope.config.PATIENT_EXPERIENCE_ID]));
      promises.push(QRReportSvc.getReportByHospital($scope.hospitalProfile.id));

      $q.all(promises)
      .then(function(result) {
        var measures, report;
        measures = result[0];
        report = result[1];
        updateSearch(measures, report);
      });
    }

    function updateSearch(measures, report) {
      var peMeasures, ids;

      ids = _.map([$scope.config.PATIENT_EXPERIENCE_ID], function(id) { return +id; });

      peMeasures = _.filter(report, function(row) {
        return _.contains(ids, row.MeasureID);
      });

      $scope.measures = [];

      $scope.measures = _.map(peMeasures, function(measure) {
        var name, row, mdef;

        mdef = _.findWhere(measures, {MeasureID: measure.MeasureID});

        row = {
          name: mdef.SelectedTitle,
          rating: measure.NatRating
        };

        return row;
      });
    }

  }

})();


