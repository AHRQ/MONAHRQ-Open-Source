/**
 * Consumer Product
 * Hospital Reports Module
 * Profile Costs Panel Controller
 *
 * This report shows cost data for the top 25 DRGs by number of discharges. It
 * will use an alternate Medicare report as source data if the report config specifies.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.hospitals')
    .controller('CHProfileCostsCtrl', CHProfileCostsCtrl);


  CHProfileCostsCtrl.$inject =  ['$scope', '$state', '$stateParams', 'SortSvc',
    'UDUtilReportSvc', 'UDUtilQuerySvc', 'UDMedicareReportSvc', 'ConsumerReportConfigSvc', 'drg'];
  function CHProfileCostsCtrl($scope, $state, $stateParams, SortSvc,
    UDUtilReportSvc, UDUtilQuerySvc, UDMedicareReportSvc, ConsumerReportConfigSvc, drg) {

    var drgIds = [];

    $scope.columns = [];
    $scope.drgs = [];
    $scope.drgData = {};

    init();

    function init() {
      loadReport($stateParams.id);
    }

    function getCurrentReportingYear() {
      var reportConfig = ConsumerReportConfigSvc.configForReport($state.current.data.report.id_summary);
      var year;

      if (_.isArray(reportConfig.ReportingYears) && reportConfig.ReportingYears.length > 0) {
        year = _.max(reportConfig.ReportingYears);
      }

      return year;
    }


    function loadReport(hospitalId) {
      if (ConsumerReportConfigSvc.qrProfileHasCCDataIP()) {
        UDUtilQuerySvc.reset();

        var reportingYear = getCurrentReportingYear();
        var query = UDUtilQuerySvc.query;
        query.groupBy.viewBy = 'hospital';
        query.groupBy.reportType = 'id';
        query.groupBy.groupBy = 'geo';
        query.groupBy.dimension = 'hospital';
        query.groupBy.value = hospitalId;
        query.viewBy = 'drg';
        query.level.type = 'summary';

        UDUtilReportSvc.getReport(query, reportingYear)
          .then(updateModel);
      }
      else if (ConsumerReportConfigSvc.qrProfileHasCCDataMedicare()) {
        UDMedicareReportSvc.getReport(hospitalId)
        .then(updateModel);
      }
    }

    function updateModel(data) {
      var id = getDRGId();
      var dischargesId = getDischargesId();
      var report;

      if (ConsumerReportConfigSvc.qrProfileHasCCDataIP()) {
        report = data.report['TableData'];
      }
      else {
        report = data.TableData;
      }

      SortSvc.objSortNumeric(report, dischargesId, 'dsc');

      for (var i = 0; i < report.length && i < 25; i++) {
        var d = _.findWhere(drg, {Id: +report[i][id]});
        $scope.drgs.push(d);
      }
      SortSvc.objSort($scope.drgs, 'Description', 'asc');
      drgIds = _.pluck($scope.drgs, 'Id');

      $scope.drgData['hospital'] = getDrgData(report);
      /*      if (ReportConfigSvc.qrProfileHasCCDataIP()) {
       $scope.drgData['regional'] = getDrgData(data['TotalData'][0]);
       $scope.drgData['national'] = getDrgData(data['NationalData'][0]);
       }*/
    }

    function getDrgData(rows){
      var id = getDRGId();
      var topDrgs = _.filter(rows, function(r) {
        return _.has(r, id) && _.contains(drgIds, +r[id]);
      });

      var result = {};
      _.each(topDrgs, function(r) {
        result[r[id]] = r;
      });

      return result;
    }

    function getDRGId() {
      if (ConsumerReportConfigSvc.qrProfileHasCCDataIP()) {
        return 'ID';
      }
      else {
        return 'DRGID';
      }
    }

    function getDischargesId() {
      if (ConsumerReportConfigSvc.qrProfileHasCCDataIP()) {
        return 'Discharges';
      }
      else {
        return 'NumDischarges';
      }
    }

  }

})();

