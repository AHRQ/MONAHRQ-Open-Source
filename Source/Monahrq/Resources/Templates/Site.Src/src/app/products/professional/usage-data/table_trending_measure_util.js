/**
 * Professional Product
 * Usage Data Report Module
 * Trending Table By Measure Controller
 *
 * This controller generates the trending by measure report, and displays the
 * results in a table.
 *
 * Note that the ICD 10 switchover date is fixed at Q4 2015, which is reflected in
 * the reportSettings object.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UDTableTrendingMeasureUtilCtrl', UDTableTrendingMeasureUtilCtrl);


  UDTableTrendingMeasureUtilCtrl.$inject = ['$scope', '$state', 'SortSvc', 'ReportConfigSvc'];
  function UDTableTrendingMeasureUtilCtrl($scope, $state, SortSvc, ReportConfigSvc) {
    var report = $scope.trendingReport;
    var reportConfig = getActiveReportConfig();

    $scope.vm = {};
    var vm = $scope.vm;

    vm.reportSettings = {
      icd10Year: 2015,
      icd10Quarter: 4
    };
    vm.report = null;
    vm.makeColumns = makeColumns;
    vm.getCell = getCell;
    vm.getQuartersForYear = getQuartersForYear;
    vm.back = back;
    vm.isICD10Quarter = isICD10Quarter;

    init();


    function init() {
      SortSvc.objSort(report.columns, 'label', 'asc');
      SortSvc.objSort(report.tableData, 'name', 'asc');
      vm.report = report;
      vm.reportingYears = reportConfig.ReportingYears || [];
      vm.reportingQuarters = reportConfig.ReportQuarters || [];

      setupReportHeaderFooter();
      makeColumns();
    }

    function setupReportHeaderFooter() {
      var report = getActiveReportConfig();
      if (report) {
        vm.reportSettings.header = report.ReportHeader;
        vm.reportSettings.footer = report.ReportFooter;
      }
    }

    function getQuartersForYear(year) {
      var yq = _.findWhere(reportConfig.ReportQuarters, {'Year': year});
      if (yq) {
        return yq.Quaters; // typo in data file
      }
    }

    function makeColumns() {
      var years = reportConfig.ReportingYears || [];
      var quarters = [];

      var columns = [];

      if (vm.showQuarters != true) {
        columns = report.columns;
      }
      else {
        for (var i = 0; i < years.length; i++) {
          columns.push({
            name: years[i],
            label: 'Total',
            year: years[i],
            quarter: null
          });

          quarters = getQuartersForYear(years[i]);

          for (var j = 0; j < quarters.length; j++) {
            columns.push({
              name: 'Q' + quarters[j] + '_' + years[i],
              label: 'Q' + quarters[j],
              year: years[i],
              quarter: quarters[j]
            });
          }
        }
      }

      vm.columns = columns;
    }

    function getCell(name, row) {
      var fname = 'f' + name;

      if (row == null) return null;

      if (row[name] == '-1') return '-';
      if (row[name] == '-2') return 'c';

      if (_.has(row, fname)) {
        return row[fname];
      }
      else if (_.has(row, name)) {
        return row[name];
      }

      return null;
    }

    function back() {
      $scope.setReportType('default');
    }

    function getActiveReportId() {
      var key = $scope.query.groupBy.reportType + '_' + $scope.query.level.type + '_trending';
      var id = $state.current.data.report[key];
      return id;
    }

    function getActiveReportConfig() {
      var id = getActiveReportId();
      var reportConfig = ReportConfigSvc.configForReport(id);
      return reportConfig;
    }

    function isICD10Quarter(year, quarter) {
      return year == vm.reportSettings.icd10Year && quarter == vm.reportSettings.icd10Quarter;
    }
  }
})();
