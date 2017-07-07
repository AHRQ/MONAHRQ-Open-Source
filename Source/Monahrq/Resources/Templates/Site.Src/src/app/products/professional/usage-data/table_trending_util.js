/**
 * Professional Product
 * Usage Data Report Module
 * Trending Table Controller
 *
 * This controller generates the trending by measure report, and displays the
 * results in a table.
 *
 * Note that the ICD 10 switchover date is fixed at Q4 2015, which is reflected in
 * the reportSettings object.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UDTableTrendingUtilCtrl', UDTableTrendingUtilCtrl);


  UDTableTrendingUtilCtrl.$inject = ['$scope', '$state', 'SortSvc', 'ReportConfigSvc',];
  function UDTableTrendingUtilCtrl($scope, $state, SortSvc, ReportConfigSvc) {
    var report = $scope.trendingReport;
    var reportConfig = getActiveReportConfig();
    var isTableActive;

    $scope.vm = {};
    var vm = $scope.vm;

    vm.report = null;
    vm.columnOptions = [];
    vm.chartColumnName = null;
    vm.chartData = {};
    vm.chartConfig = {};
    vm.reportSettings = {
      icd10Year: 2015,
      icd10Quarter: 4
    };

    vm.getQuartersForYear = getQuartersForYear;
    vm.getCell = getCell;
    vm.onChartSelect = onChartSelect;
    vm.onTableSelect = onTableSelect;
    vm.showTable = showTable;
    vm.showChart = showChart;
    vm.getChartColumn = getChartColumn;
    vm.toggleQuarters = toggleQuarters;
    vm.back = back;
    vm.isICD10Quarter = isICD10Quarter;


    init();


    function init() {
      isTableActive = true;

      SortSvc.objSort(report.tableData, 'reportingYear', 'asc');
      vm.report = report;
      vm.reportingYears = reportConfig.ReportingYears || [];
      vm.reportingQuarters = reportConfig.ReportQuarters || [];
      vm.showQuarters = true;

      setupReportHeaderFooter();
      buildColumnDropdownOptions();

      $scope.$watch('vm.chartColumnName', function(nv, ov) {
        if (nv === ov) return;
        buildChart();
      });
    }

    function setupReportHeaderFooter() {
      var report = getActiveReportConfig();
      if (report) {
        vm.reportSettings.header = report.ReportHeader;
        vm.reportSettings.footer = report.ReportFooter;
      }
    }

    function toggleQuarters() {
      vm.showQuarters = !vm.showQuarters;

      if (vm.showChart()) {
        console.log('DB: building chart');
        buildChart();
      }
    }

    function getQuartersForYear(year) {
      var yq = _.findWhere(reportConfig.ReportQuarters, {'Year': year});
      if (yq) {
        return yq.Quaters; // typo in data file
      }
    }

    function buildColumnDropdownOptions() {
           _.each(report.columns, function(c) {
        vm.columnOptions.push({
          id: c.name,
          name: c.title
        });
      });
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

    function getChartColumn() {
      return _.findWhere(report.columns, {name: vm.chartColumnName});
    }

    function onChartSelect(columnDef, row) {
      isTableActive = false;
      vm.chartColumnName = columnDef.name;
      buildChart();
    }

    function onTableSelect() {
      isTableActive = true;
    }

    function showChart() {
      return !isTableActive;
    }

    function showTable() {
      return isTableActive;
    }

    function back() {
      $scope.setReportType('default');
    }

    function buildChart() {
      var activeCol = getChartColumn();

      vm.chartConfig = {
        title: '', // chart title
        tooltips: true,
        labels: false, // labels on data points

        lineLegend: null,
        lineCurveType: 'linear', // change this as per d3 guidelines to avoid smoothline

        colors: ['#000' /*, '#f00'*/],

        width: '100%',
        height: '400px',

        isAnimate: false, // run animations while rendering chart
        yAxisTickFormat: 's', //refer tickFormats in d3 to edit this value
        //xAxisMaxTicks: 7 // Optional: maximum number of X axis ticks to show if data points exceed this number
        xDemarcation: null,
        /*legend: {
          display: false
        }*/
      };

      vm.chartData.series = [vm.chartColumnName/*, 'ICD 10'*/];//_.pluck(report.tableData, 'reportingYear');
      vm.chartData.data = [];

      _.each(report.tableData, function(row, rowIndex) {
        if (vm.showQuarters) {
          _.each(getQuartersForYear(row.reportingYear), function(quarter, qIndex) {
             var dp = {
              tooltip: getCell('Q' + quarter + '_' + vm.chartColumnName, row),
              x: row.reportingYear + ' Q' + quarter,
              y: [row['Q' + quarter + '_' + vm.chartColumnName]]
            };

            if (isICD10Quarter(row.reportingYear, quarter)) {
              vm.chartConfig.xDemarcation = dp.x;
              vm.chartConfig.xDemarcationLabelLeft = 'ICD 9';
              vm.chartConfig.xDemarcationLabelRight = 'Post ICD 10';
            }

            vm.chartData.data.push(dp);
          });
        }
        else {
          vm.chartData.data.push({
            tooltip: getCell(vm.chartColumnName, row),
            x: row.reportingYear,
            y: [row[vm.chartColumnName]]
          });
        }
      });
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
