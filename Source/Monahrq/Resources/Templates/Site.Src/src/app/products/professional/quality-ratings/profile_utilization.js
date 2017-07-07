/**
 * Professional Product
 * Quality Ratings Module
 * Profile Page Utilization Tab Controller
 *
 * This report shows discharge and cost data for the top 25 DRGs by number of discharges. It
 * will use an alternate Medicare report as source data if the report config specifies.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings')
    .controller('QRProfileUtilizationCtrl', QRProfileUtilizationCtrl);


  QRProfileUtilizationCtrl.$inject =  ['$scope', '$state', '$filter', 'SortSvc',
    'UDUtilReportSvc', 'UDUtilQuerySvc', 'UDMedicareReportSvc', 'ReportConfigSvc', 'drg', 'ModalLegendSvc'];
  function QRProfileUtilizationCtrl($scope, $state, $filter, SortSvc,
    UDUtilReportSvc, UDUtilQuerySvc, UDMedicareReportSvc, ReportConfigSvc, drg, ModalLegendSvc) {

    var visibleDRGs = {}, drgIds = [];

    $scope.columns = [];
    $scope.drgs = [];
    $scope.drgData = {};

    function buildColumns() {
      var cols = [];

      if (ReportConfigSvc.qrProfileHasCCDataIP()) {
        var report = ReportConfigSvc.configForReport($state.current.data.report.id_summary);
        cols = _.map(report.IncludedColumns, function (c) {
          return {
            field: c.DataElementLink,
            label: c.Name,
            format: c.DataFormat
          };
        });
      }
      else {
        cols = [
          {
            field: 'NumDischarges',
            label: 'Number of discharges',
            format: 'number'
          },
          {
            field: 'MeanCharges',
            label: 'Mean Charges in dollars',
            format: 'nfcurrency'
          },
          {
            field: 'MeanCost',
            label: 'Mean Cost in dollars',
            format: 'nfcurrency'
          },
          {
            field: 'MeanTotalPayments',
            label: 'Mean Total Payments in dollars',
            format: 'nfcurrency'
          }
        ]
      }

      $scope.columns = cols;
    }

    $scope.showDRG = function(id) {
      return _.has(visibleDRGs, id);
    };

    $scope.toggleDRG = function (id) {
      if ($scope.showDRG(id)) {
        delete visibleDRGs[id];
      }
      else {
        visibleDRGs[id] = true;
      }
    };

    $scope.openLegendModal = function(){
      ModalLegendSvc.open($state.current.data.report.id_summary);
    };

    $scope.isMedicare = function() {
      return ReportConfigSvc.qrProfileHasCCDataMedicare();
    };

    $scope.getFilteredValue = function(col, value) {
      if (col.format && value && value != -1) {
        return $filter(col.format)(value);
      }
      return '-';
    };


    buildColumns();
    loadReport($scope.hospitalProfile.id);


    function getCurrentReportingYear() {
      var reportConfig = ReportConfigSvc.configForReport($state.current.data.report.id_summary);
      var year;

      if (_.isArray(reportConfig.ReportingYears) && reportConfig.ReportingYears.length > 0) {
        year = _.max(reportConfig.ReportingYears);
      }

      return year;
    }


    function loadReport(hospitalId) {
      if (ReportConfigSvc.qrProfileHasCCDataIP()) {
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
      else if (ReportConfigSvc.qrProfileHasCCDataMedicare()) {
        UDMedicareReportSvc.getReport(hospitalId)
        .then(updateModel);
      }
    }

    function updateModel(data) {
      var id = getDRGId();
      var dischargesId = getDischargesId();
      var report;

      if (ReportConfigSvc.qrProfileHasCCDataIP()) {
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
      if (ReportConfigSvc.qrProfileHasCCDataIP()) {
        return 'ID';
      }
      else {
        return 'DRGID';
      }
    }

    function getDischargesId() {
      if (ReportConfigSvc.qrProfileHasCCDataIP()) {
        return 'Discharges';
      }
      else {
        return 'NumDischarges';
      }
    }

  }

})();

