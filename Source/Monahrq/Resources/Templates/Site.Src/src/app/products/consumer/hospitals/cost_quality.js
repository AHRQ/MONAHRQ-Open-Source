/**
 * Consumer Product
 * Hospital Reports Module
 * Cost and Quality Page Controller
 *
 * This controller builds the report for cost and quality data for the hospitals the
 * user selected.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.hospitals')
    .controller('CHCostQualityCtrl', CHCostQualityCtrl);

  CHCostQualityCtrl.$inject = ['$scope', '$state', '$stateParams', '$q', 'ResourceSvc', 'HospitalRepositorySvc', 'HospitalReportLoaderSvc',
    'ModalMeasureSvc', 'ModalLegendSvc', 'ConsumerReportConfigSvc', 'costQualityTopics'];
  function CHCostQualityCtrl($scope, $state, $stateParams, $q, ResourceSvc, HospitalRepositorySvc, HospitalReportLoaderSvc,
    ModalMeasureSvc, ModalLegendSvc, ConsumerReportConfigSvc, costQualityTopics) {
    var subtopicId, hospitalIds, reports;

    $scope.reportId = $state.current.data.report;
    $scope.reportSettings = {};
    $scope.query = {
      compareTo: 'state'
    };

    $scope.modalMeasure = modalMeasure;
    $scope.modalLegend = modalLegend;
    $scope.backToReport = backToReport;

    init();


    function init() {
      subtopicId = _.isString($stateParams.subtopicId) ? +$stateParams.subtopicId : null;
      hospitalIds = _.isString($stateParams.hospitalIds) ? _.map($stateParams.hospitalIds.split(','), function(id) { return +id;}) : [];

      HospitalRepositorySvc.init()
        .then(loadData);

      setupReportHeaderFooter();
    }

    function setupReportHeaderFooter() {
      var id = $state.current.data.report;
      var report = ConsumerReportConfigSvc.configForReport(id);
      if (report) {
        $scope.reportSettings.header = report.ReportHeader;
        $scope.reportSettings.footer = report.ReportFooter;
      }
    }

    function loadData() {
      var hospitals = HospitalRepositorySvc.getIndexRecords(hospitalIds);

      $scope.hospitals = _.map(hospitals, function(h) {
        return {
          id: h.Id,
          name: h.Name
        };
      });

      var topic = _.findWhere(costQualityTopics, {Id: subtopicId});
      $scope.topic = topic;
      $scope.measures = topic.Measures;
      loadMeasures();

      HospitalReportLoaderSvc.getCostQualityByHospitalReports(hospitalIds)
        .then(function(_reports) {
          reports = _reports;
          updateData();
        });
    }

    function loadMeasures() {
      var ids = _.pluck($scope.measures, 'MeasureId');
      var promises = [];

      _.each(ids, function(id) {
        promises.push(ResourceSvc.getCostQualityMeasure(id));
      });

      $q.all(promises)
        .then(function(data) {
          $scope.measureDefs = {};
          _.each(_.flatten(data), function(m) {
            $scope.measureDefs[m.MeasureId] = m;
          });
        });
    }

    function updateData() {
      var model = {};

      _.each(reports, function(report) {
        _.each(report.data, function(measure) {
          if (!_.has(model, measure.MeasureId)) {
            model[measure.MeasureId] = {};
          }
          model[measure.MeasureId][measure.HospitalId] = measure;
        });
      });

      $scope.model = model;
    }

    function modalMeasure(id) {
      ModalMeasureSvc.openCostQualityMeasure(id);
    }

    function modalLegend(){
      var id = $state.current.data.report;
      ModalLegendSvc.open(id);
    }

    function backToReport($event) {
      $event.preventDefault();

      if ($state.previous.name) {
        var stateName = $state.previous.name.name;
        var stateParams = $state.previous.params;
      }

      if (stateName && _.isString(stateName) && stateName.length > 0) {
        $state.go(stateName, stateParams);
      }
      else {
        $state.go('top.consumer.hospitals.location');
      }
    }
 }

})();


