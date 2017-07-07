/**
 * Professional Product
 * Usage Data Report Module
 * Utilization Report Table Controller
 *
 * This controller generates the utilization report and displays the results in a table.
 * It is more complex than most of the other table controllers because it generically
 * handles summary and drilldown views for four different data sets: two population reports
 * for county and region, and two hospital reports for inpatient and emergency.
 *
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UDTableUtilCtrl', UDTableUtilCtrl);

  UDTableUtilCtrl.$inject = ['$scope', '$state', '$stateParams', '$filter',
    'UDUtilQuerySvc', 'UDUtilReportSvc', 'SortSvc', 'ZipDistanceSvc', 'ModalLegendSvc',
    'ReportConfigSvc',
    'hospitals', 'hospitalRegions', 'hospitalCounties', 'hospitalTypes', 'hospitalZips', 'patientRegions', 'patientCounties',
    'ccs', 'ccsCategories', 'mdc', 'drg', 'drgCategories', 'prccs', 'prccsCategories',
    'stratification', 'age', 'sex', 'payer', 'race'];
  function UDTableUtilCtrl($scope, $state, $stateParams, $filter,
                           UDUtilQuerySvc, UDUtilReportSvc, SortSvc, ZipDistanceSvc, ModalLegendSvc,
                           ReportConfigSvc,
                           hospitals, hospitalRegions, hospitalCounties, hospitalTypes, hospitalZips, patientRegions, patientCounties,
                           ccs, ccsCategories, mdc, drg, drgCategories, prccs, prccsCategories,
                           stratification, age, sex, payer, race) {
    var processedRows;
    var lookupMap = {
        condition: {
          data: ccs,
          id: 'Id',
          label: 'Description'
        },
        mdc: {
          data: mdc,
          id: 'Id',
          label: 'Description'
        },
        drg: {
          data: drg,
          id: 'Id',
          label: 'Description'
        },
        procedure: {
          data: prccs,
          id: 'Id',
          label: 'Description'
        },
        hospital: {
          data: hospitals,
          id: 'Id',
          label: 'Name'
        },
        region: {
          data: hospitalRegions,
          id: 'RegionID',
          label: 'Name'
        },
        patientregion: {
          data: patientRegions,
          id: 'RegionID',
          label: 'Name'
        },
        patientcounty: {
          data: patientCounties,
          id: 'CountyID',
          label: 'CountyName'
        },
        county: {
          data: hospitalCounties,
          id: 'CountyID',
          label: 'CountyName'
        },
        hospitalType: {
          data: hospitalTypes,
          id: 'HospitalTypeID',
          label: 'Name'
        },
        zip: {
          data: null,
          id: 'Zip',
          label: 'Zip'
        }
      },
      groupFieldMap = {
        condition: 'CCSID',
        mdc: 'MDC',
        drg: 'DRG',
        procedure: 'PROC',
        hospital: 'HospitalID',
        region: 'RegionID',
        patientregion: 'RegionID',
        patientcounty: 'CountyID',
        county: 'CountyID',
        hospitalType: 'HospitalType',
        zip: 'Zip',
        detail: 'CatID'
      },
      stratLabels = {
        'Age': age,
        'Sex': sex,
        'Payer': payer,
        'Race': race
      },
      viewByTitles = {
        'condition': 'Condition or Topic',
        'drg': 'Diagnosis Related Group',
        'mdc': 'Major Diagnosis Category',
        'procedure': 'Procedure',
        'hospital': 'Hospital',
        'region': 'Geographic Region',
        'patientregion': 'Patient Region',
        'patientcounty': 'Patient County',
        'county': 'Geographic County',
        'hospitalType': 'Hospital Type',
        'zip': 'Zip Code'
      },
      tableTitles = {
        'condition': 'CCSDX',
        'condition-all': 'All CCS Diagnosis',
        'drg': 'DRG',
        'drg-all': 'All Diagnosis Related Group',
        'mdc': 'MDC',
        'mdc-all': 'All Major Diagnostic Category',
        'procedure': 'PRCCS',
        'procedure-all': 'All CCS Procedure',
        'hospital': 'Hospital',
        'hospital-all': 'All Hospitals',
        'region': 'Region',
        'region-all': 'All Regions',
        'patientregion': 'Region',
        'patientregion-all': 'All Regions',
        'patientcounty': 'County',
        'patientcounty-all': 'All Counties',
        'county': 'County',
        'county-all': 'All Counties',
        'hospitalType': 'Hospital Type',
        'hospitalType-all': 'All Hospital Types'
      },
      viewByOptions = {
        'id': {
          'geo': [
            {id: 'mdc', name: viewByTitles['mdc']},
            {id: 'drg', name: viewByTitles['drg']},
            {id: 'condition', name: viewByTitles['condition']},
            {id: 'procedure', name: viewByTitles['procedure']}
          ],
          'clinical': [
            {id: 'hospital', name: viewByTitles['hospital']},
            {id: 'county', name: viewByTitles['county']},
            {id: 'region', name: viewByTitles['region']},
            {id: 'hospitalType', name: viewByTitles['hospitalType']}
          ]
        },
        'ed': {
          'geo': [
            {id: 'condition', name: viewByTitles['condition']}
          ],
          'clinical': [
            {id: 'hospital', name: viewByTitles['hospital']},
            {id: 'county', name: viewByTitles['county']},
            {id: 'region', name: viewByTitles['region']},
            {id: 'hospitalType', name: viewByTitles['hospitalType']},
            {id: 'zip', name: viewByTitles['zip']}
          ]
        },
        'county': {
          'geo': [
            {id: 'mdc', name: viewByTitles['mdc']},
            {id: 'drg', name: viewByTitles['drg']},
            {id: 'condition', name: viewByTitles['condition']},
            {id: 'procedure', name: viewByTitles['procedure']}
          ],
          'clinical': [
            {id: 'patientcounty', name: viewByTitles['patientcounty']}
          ]
        },
        'region': {
          'geo': [
            {id: 'mdc', name: viewByTitles['mdc']},
            {id: 'drg', name: viewByTitles['drg']},
            {id: 'condition', name: viewByTitles['condition']},
            {id: 'procedure', name: viewByTitles['procedure']}
          ],
          'clinical': [
            {id: 'patientregion', name: viewByTitles['patientregion']}
          ]
        }
      };
    $scope.sortByOptions = [];
    $scope.viewByOptions = {};
    $scope.columns = {};
    $scope.reportSettings = {};

    $scope.getReportType = getReportType;
    $scope.setReportType = setReportType;
    $scope.showTable = showTable;
    $scope.showGroupRows = showGroupRows;
    $scope.showViewBy = showViewBy;
    $scope.drillup = drillup;
    $scope.drilldown = drilldown;
    $scope.drilldownCombined = drilldownCombined;
    $scope.getCell = getCell;
    $scope.modalLegend = modalLegend;
    $scope.onTrendingTable = onTrendingTable;
    $scope.onTrendingMeasureTable = onTrendingMeasureTable;
    $scope.selectText = selectText;

    var activeReportType = 'default';

    init();


    function init() {
      $scope.viewByOptions = viewByOptions[$scope.query.groupBy.reportType];

      $scope.query = UDUtilQuerySvc.query;


      if ($scope.query.viewBy == null || $scope.query.viewBy == undefined) {
        if ($scope.query.groupBy.groupBy == 'clinical') {
          if ($scope.query.groupBy.reportType == 'county') {
            $scope.query.viewBy = 'patientcounty';
          }
          else if ($scope.query.groupBy.reportType == 'region') {
            $scope.query.viewBy = 'patientregion';
          }
          else {
            $scope.query.viewBy = 'hospital';
          }
        } else {
          $scope.query.viewBy = 'condition';
        }
      }

      $scope.query.sortBy = 'ViewBy.asc';

      UDUtilQuerySvc.notifyReportChange(getActiveReportId());

      $scope.$watch('query.groupBy.dimension', updateTableTitle);
      $scope.$watch('query.groupBy.value', tableQueryWatchHandler);
      $scope.$watch('query.viewBy', viewByWatchHandler);
      $scope.$watch('query.sortBy', tableQueryWatchHandler);
      $scope.$watch('query.narrowBy.value', narrowByWatchHandler);
      $scope.$watch('query.narrowBy.value2', narrowByWatchHandler);

      setupReportHeaderFooter();
      tableQueryWatchHandler(0, 1);
    }

    $scope.isMissingReport = function() {
      return !UDUtilQuerySvc.hasReportData();
    };

    function setupReportHeaderFooter() {
      var report = getActiveReportConfig();
      if (report) {
        $scope.reportSettings.header = report.ReportHeader;
        $scope.reportSettings.footer = report.ReportFooter;
      }
    }

    function getReportType() {
      return activeReportType;
    }

    function setReportType(rt) {
      activeReportType = rt;
    }


    function getActiveReportId() {
      var key = $scope.query.groupBy.reportType + '_' + $scope.query.level.type;
      var id = $state.current.data.report[key];
      return id;
    }

    function getActiveReportConfig() {
      var id = getActiveReportId();
      var reportConfig = ReportConfigSvc.configForReport(id);
      return reportConfig;
    }

    function getTrendingReportId() {
      var key = $scope.query.groupBy.reportType + '_' + $scope.query.level.type + '_trending';
      var id = $state.current.data.report[key];
      return id;
    }

    function getTrendingReportConfig() {
      var id = getTrendingReportId();
      var reportConfig = ReportConfigSvc.configForReport(id);
      return reportConfig;
    }

    function getCurrentReportingYear() {
      var reportConfig = getActiveReportConfig();
      var year;

      if (_.isArray(reportConfig.ReportingYears) && reportConfig.ReportingYears.length > 0) {
        year = _.max(reportConfig.ReportingYears);
      }

      return year;
    }

    function getPastReportingYears() {
      var reportConfig = getActiveReportConfig();
      var years;

      if (_.isArray(reportConfig.ReportingYears) && reportConfig.ReportingYears.length > 0) {
        years = _.without(reportConfig.ReportingYears,_.max(reportConfig.ReportingYears));
      }

      return years;
    }

    function getNumberOfReportingYears() {
      var reportConfig = getActiveReportConfig();
      var numOfYears = reportConfig.ReportingYears;

      numOfYears = numOfYears.length;

      return numOfYears;
    }

    function getNumberOfReportingQuarters() {
      var qs = getQuartersForYear(getCurrentReportingYear());

      if (qs) {
        return qs.length;
      }

      return 1;
    }

    function getQuartersForYear(year) {
      var reportConfig = getTrendingReportConfig();

      if (reportConfig == null) {
        return [];
      }

      var yq = _.findWhere(reportConfig.ReportQuarters, {'Year': year});
      if (yq) {
        return yq.Quaters; // typo in data file
      }

      return [];
    }


    function buildColumns() {
      var rt = $scope.query.groupBy.reportType;
      var reportConfig = getActiveReportConfig();

      $scope.columns = _.union(
        [{
          name: 'ViewBy',
          title: '',
          'class': 'entity-large'
        }],
        _.map(reportConfig.IncludedColumns, function (col) {
          return {
            title: col.Name,
            name: col.DataElementLink,
            filter: col.DataFormat,
            filterParams: col.DataFormatParameters ? col.DataFormatParameters : []
          };
        })
      );
    }

    function modalLegend () {
      ModalLegendSvc.open(getActiveReportId(), '');
    }

    function onTrendingTable(row, isCombined) {
      UDUtilReportSvc.getTrendingReports($scope.query, getPastReportingYears())
        .then(function(reports) {
          $scope.trendingReport = getTrendingReport(row, reports, isCombined);
          activeReportType = 'trending';
        });
    }

    function onTrendingMeasureTable(col) {
      UDUtilReportSvc.getTrendingReports($scope.query, getPastReportingYears())
        .then(function(reports) {
          $scope.trendingReport = getTrendingMeasureReport(col, reports);
          activeReportType = 'trendingByMeasure';
        });
    }

    function selectText(element) {
      var doc = document,
        text = doc.getElementById(element),
        range,
        selection;

      if (doc.body.createTextRange) { //ms
        range = doc.body.createTextRange();
        range.moveToElementText(text);
        range.select();
      }
      else if (window.getSelection) { //all others
        selection = window.getSelection();
        range = doc.createRange();
        range.selectNodeContents(text);
        selection.removeAllRanges();
        selection.addRange(range);
      }
    }


    function showTable() {
      return !_.isNull(UDUtilQuerySvc.query.groupBy.value);
    }

    function showGroupRows(rowset) {
      if (UDUtilQuerySvc.query.level.type == 'detail' && rowset.group && rowset.group.CatID == 0) {
        return false;
      }

      return true;
    }

    function showViewBy() {
      if (($scope.query.groupBy.reportType === 'id'
        || ($scope.query.groupBy.reportType === 'ed'
        && $scope.query.groupBy.groupBy === 'clinical')
        || ($scope.query.groupBy.reportType === 'county'
        && $scope.query.groupBy.groupBy === 'geo')
        || ($scope.query.groupBy.reportType === 'region'
        && $scope.query.groupBy.groupBy === 'geo')
        )
        && $scope.query.level.type === 'summary') {
        return true;
      }
      return false;
    }

    function drillup() {
      $scope.query.level.type = 'summary';
      $scope.query.level.value = null;
      $state.go('top.professional.usage-data.service-use', UDUtilQuerySvc.toStateParams());
    }


    function drilldown(r) {
      $scope.query.level.type = 'detail';
      $scope.query.level.description = 'link description';
      //$scope.query.level.description = r.ViewBy;


      if ($scope.query.groupBy.groupBy === 'geo') {
        $scope.query.level.value = r[groupFieldMap[$scope.query.viewBy]]; //r.CCSID;
      }
      else {
        if ($scope.query.viewBy === 'patientcounty') {
          $scope.query.level.value = r.CountyID;
        }
        else if ($scope.query.viewBy === 'patientregion') {
          $scope.query.level.value = r.RegionID;
        }
        else {
          $scope.query.level.value = r.HospitalID;
        }
      }

      $state.go('top.professional.usage-data.service-use', UDUtilQuerySvc.toStateParams());
    }

    function drilldownCombined() {
      $scope.query.level.type = 'detail';
      $scope.query.level.value = '0';

      $state.go('top.professional.usage-data.service-use', UDUtilQuerySvc.toStateParams());
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

    function getDimensionOptionName() {
      var dimName = $scope.query.groupBy.dimension;
      if (!dimName) return;

      var dim = lookupMap[dimName].data, query = {};
      query[lookupMap[dimName].id] = +$scope.query.groupBy.value;
      var obj = _.findWhere(dim, query);
      return obj ? obj[lookupMap[dimName].label] : '';
    }

    function updateTableTitle(newValue, oldValue) {

      var tpls = {
        geo: '<%=name%>',
        geo_all: 'By <%=typeAll%>',
        geo_detail: '',
        clinical: '<%=name%> (<%=type%> <%=value%>)',
        clinical_all: 'By <%= typeAll %>',
        clinical_detail: ' for <%= hospital %> Hospital',
        county_clinical_detail: ' for <%= county %>',
        region_clinical_detail: ' for <%= region %>'
      };
      var type = tableTitles[$scope.query.groupBy.dimension],
        typeAll = tableTitles[$scope.query.groupBy.dimension + '-all'],
        value = $scope.query.groupBy.value,
        name = getDimensionOptionName(),
        hospital, hospitalName, county, countyName, region, regionName, viewByName,
        tpl, detTpl;

      if ($scope.query.viewBy == 'patientcounty') {
        county = _.findWhere(patientCounties, {CountyID: +$scope.query.level.value});
        if (county) {
          countyName = county.CountyName;
        }
      }
      if ($scope.query.viewBy == 'patientregion') {
        region = _.findWhere(patientRegions, {RegionID: +$scope.query.level.value});
        if (region) {
          regionName = region.Name;
        }
      }
      else {
        hospital = _.findWhere(hospitals, {Id: +$scope.query.level.value});
        if (hospital) {
          hospitalName = hospital.Name;
        }
      }

      viewByName = viewByTitles[$scope.query.viewBy];

      if($scope.query.level.type == 'detail' && $scope.query.viewBy == 'condition' && $scope.query.level.value > 0){
          viewByName = $scope.query.level.value ? _.where($scope.uiaCCSNarrow.data, {Id: +$scope.query.level.value}) : null;
          viewByName = viewByName[0].Description.replace($scope.query.level.value +' ','');
      }
      if($scope.query.level.type == 'detail' && $scope.query.viewBy == 'mdc' && $scope.query.level.value > 0){
          viewByName = $scope.query.level.value ? _.where($scope.uiaMDCNarrow.data, {Id: +$scope.query.level.value}) : null;
          viewByName = viewByName[0].Description.replace($scope.query.level.value +' ','');
      }
      if($scope.query.level.type == 'detail' && $scope.query.viewBy == 'drg' && $scope.query.level.value > 0){
          viewByName = $scope.query.level.value ? _.where($scope.uiaDRGNarrow.data, {Id: +$scope.query.level.value}) : null;
          viewByName = viewByName[0].Description.replace($scope.query.level.value +' ','');
      }
      if($scope.query.level.type == 'detail' && $scope.query.viewBy == 'procedure' && $scope.query.level.value > 0){
          viewByName = $scope.query.level.value ? _.where($scope.uiaPRCCSNarrow.data, {Id: +$scope.query.level.value}) : null;
          viewByName = viewByName[0].Description.replace($scope.query.level.value +' ','');
      }

      if($scope.query.narrowBy.name == 'condition' && $scope.query.narrowBy.value != 999999){
        viewByName = $scope.query.narrowBy.value ? _.where($scope.uiaCCSNarrow.data, {Id: +$scope.query.narrowBy.value}) : null;
        viewByName = viewByName[0].Description.replace($scope.query.narrowBy.value +' ','');
      }
      if($scope.query.narrowBy.name == 'mdc' && $scope.query.narrowBy.value != 999999){
        viewByName = $scope.query.narrowBy.value ? _.where($scope.uiaMDCNarrow.data, {Id: +$scope.query.narrowBy.value}) : null;
        viewByName = viewByName[0].Description.replace($scope.query.narrowBy.value +' ','');

      }
      if($scope.query.narrowBy.name == 'drg' && $scope.query.narrowBy.value != 999999){
        viewByName = $scope.query.narrowBy.value ? _.where($scope.uiaDRGNarrow.data, {Id: +$scope.query.narrowBy.value}) : null;
        viewByName = viewByName[0].Description.replace($scope.query.narrowBy.value +' ','');
      }
      if($scope.query.narrowBy.name == 'procedure' && $scope.query.narrowBy.value != 999999){
        viewByName = $scope.query.narrowBy.value ? _.where($scope.uiaPRCCSNarrow.data, {Id: +$scope.query.narrowBy.value}) : null;
        viewByName = viewByName[0].Description.replace($scope.query.narrowBy.value +' ','');
      }

      if ($scope.query.groupBy.value == 0) {
        tpl = tpls[$scope.query.groupBy.groupBy + '_all'];
      }
      else {
        tpl = tpls[$scope.query.groupBy.groupBy];
      }

      if ($scope.query.level.type == 'detail') {
        if ($scope.query.viewBy == 'patientcounty' && $scope.query.groupBy.groupBy == 'clinical') {
          detTpl = tpls['county_' + $scope.query.groupBy.groupBy + '_detail'];
        }
        else if ($scope.query.viewBy == 'patientregion' && $scope.query.groupBy.groupBy == 'clinical') {
          detTpl = tpls['region_' + $scope.query.groupBy.groupBy + '_detail'];
        }
        else {
          detTpl = tpls[$scope.query.groupBy.groupBy + '_detail'];
        }
      }
      if (($scope.query.groupBy.reportType === 'county'
              || $scope.query.groupBy.reportType === 'region'
              || $scope.query.groupBy.reportType === 'id'
              || $scope.query.groupBy.reportType === 'ed'
              ) && $scope.query.groupBy.groupBy === 'geo'
      && $scope.query.viewBy) {
        tpl += ' for ' + viewByName;
      }



      if (tpl) {
        var compiled = _.template(tpl);
        $scope.tableTitle = compiled({
          type: type,
          typeAll: typeAll,
          name: name,
          value: value
        });
      }

      if (detTpl) {
        var compiled = _.template(detTpl);
        $scope.tableTitle += compiled({
          hospital: hospitalName,
          county: countyName,
          region: regionName
        });
      }

    }

    function buildSortOptions() {
      $scope.sortByOptions = [];
      _.each($scope.columns, function (c) {
        var descName = ' (High to Low)';
        var ascName = ' (Low to High)';

        if (c.name === 'ViewBy') {
          descName = ' (Z to A)';
          ascName = ' (A to Z)';
        }

        $scope.sortByOptions.push({
          id: c.name + '.asc',
          name: c.title + ascName
        });
        $scope.sortByOptions.push({
          id: c.name + '.desc',
          name: c.title + descName
        });
      });
    }

    function narrowByWatchHandler(n, o) {
      var narrow = $scope.query.narrowBy;
      if (narrow.name
        || (narrow.name == null && narrow.value == null && narrow.value2 == null)) {
        tableQueryWatchHandler(0, 1);
      }
      updateTableTitle();
    }

    function viewByWatchHandler(newValue, oldValue) {
      if (newValue === oldValue) return;
      $scope.query.narrowBy.value = null;
      tableQueryWatchHandler(0, 1);
      updateTableTitle();
    }

    function tableQueryWatchHandler(newValue, oldValue) {
      if (newValue === oldValue) return;
      if ($scope.query.groupBy.value == undefined) return;

      var reportingYear = getCurrentReportingYear();

      UDUtilQuerySvc.setHasReportData(true);
      UDUtilReportSvc.getReport($scope.query, reportingYear)
        .then(updateTable,
        function (reason) {
          UDUtilQuerySvc.setHasReportData(false);
          JL("controllers.UDTableUtilCtrl").warn("error loading report file:" + reason);
        });
    }

    function updateTable(report) {
      var data = report.report;
      $scope.reportingYear = report.reportingYear;

      $scope.numOfYears = getNumberOfReportingYears();
      $scope.numOfQuarters = getNumberOfReportingQuarters();

      var viewBy = $scope.query.viewBy;
      if ($scope.query.level.type === 'detail') {
        viewBy = 'detail';
      }

      if ($scope.query.groupBy.groupBy == 'geo'
        && ($scope.query.groupBy.reportType == 'id' || $scope.query.groupBy.reportType == 'county' || $scope.query.groupBy.reportType == 'region')) {
        groupFieldMap['condition'] = 'ID';
        groupFieldMap['mdc'] = 'ID';
        groupFieldMap['drg'] = 'ID';
        groupFieldMap['procedure'] = 'ID';
      }

      setupReportHeaderFooter();

      buildColumns();;

      $scope.nationalData = $scope.totalData = null;
      if (data.NationalData && data.NationalData.length > 0)
        $scope.nationalData = formatRows(data.NationalData)[0];
      if (data.TotalData && data.TotalData.length > 0)
        $scope.totalData = formatRows(data.TotalData)[0];

      // fill in calculated row data
      var rows = populateRowLookups(data.TableData);

      updateViewByColumnTitle();

      buildSortOptions()

      // narrow the rows
      rows = narrowRows(rows);

      // format the rows
      rows = formatRows(rows);

      // sort the rows
      sortRows(rows);

      processedRows = rows;

      // group the rows
      //if ($scope.query.groupBy.groupBy != 'geo' && viewBy != 'hospital') {
      if (hasGroupingKey(rows, viewBy) || $scope.query.level.type == 'detail') {
        $scope.tableData = groupRows(rows, viewBy);
      }
      else {
        $scope.tableData = [
          {
            rows: rows
          }
        ];
      }
    }

    function getTrendingReport(forRow, reports, isCombined) {
      var report = {
        title: null,
        subtitle: null,
        columns: [],
        tableData: []
      };

      var searchFields = [];
      if ($scope.query.level.type == 'detail') {
        searchFields.push('CatID');
        searchFields.push('CatVal');

        var strat = _.findWhere(stratification, {Id: forRow.CatID});

        if (isCombined) {
          report.title = 'Combined Report';
        }
        else {
          report.title = $scope.tableTitle;
          report.subtitle = strat.Caption + ': ' + forRow.ViewBy;
        }
      }
      else {
        searchFields.push(groupFieldMap[$scope.query.viewBy]);

        if (isCombined) {
          report.title = 'Combined Report';
        }
        else {
          report.title = forRow.ViewBy;
        }
      }

      _.each($scope.columns, function(col) {
        if (col.name != 'ViewBy') {
          report.columns.push(col);
        }
      });

      _.each(getPastReportingYears(), function(year) {
        var search = {}, newRow;
        _.each(searchFields, function(sf) {
          search[sf] = forRow[sf];
        });

        if ($scope.query.level.type == 'detail') {
          search['CatVal'] = forRow['CatVal'];
        }

        if (isCombined) {
          newRow = _.first(reports[year].TotalData);
        }
        else {
          newRow = _.findWhere(reports[year].TableData, search);
        }

        if (newRow) {
          newRow.reportingYear = year;
          formatRow(newRow);
          formatQuarters(newRow, year);
          report.tableData.push(newRow);
        }
      });

      forRow.reportingYear = getCurrentReportingYear();
      formatQuarters(forRow, forRow.reportingYear);
      report.tableData.push(forRow);

      function formatQuarters(row, year) {
        _.each(report.columns, function(col) {
          var quarters = getQuartersForYear(+year);
          _.each(quarters, function(quarter) {
            var val = row['Q' + quarter + '_' + col.name];

            if (val && val != '-1') {
              var pq = [val].concat(col.filterParams);
              row['fQ' + quarter + '_' + col.name] = $filter(col.filter).apply(undefined, pq);
            }
          });
        });
      }

      return report;
    }

    function getTrendingMeasureReport(forCol, reports) {
      var report = {
        title: null,
        subtitle: null,
        primaryColumnHeading: null,
        columns: [],
        tableData: []
      };

     var idName = groupFieldMap[$scope.query.viewBy];

      if ($scope.query.level.type == 'detail') {
        var strat = _.findWhere(stratification, {Id: forRow.CatID});

        if (isCombined) {
          report.title = 'Combined Report';
        }
        else {
          report.title = $scope.tableTitle;
          report.subtitle = strat.Caption + ': ' + forRow.ViewBy;
        }
      }
      else {
        report.title = $scope.tableTitle;
        report.subtitle = forCol.title;
        report.primaryColumnHeading = $scope.columns[0].title;
      }

      _.each([getCurrentReportingYear()].concat(getPastReportingYears()), function(year) {
        report.columns.push({
          name: year,
          label: year
        });
      });

      var results = {};
      _.each(processedRows, function(row) {
        results[row[idName]] = {
          id: row[idName],
          name: row['ViewBy']
        };
      });

      reports[getCurrentReportingYear()] = {TableData: processedRows};
      _.each(reports, function(r, k) {
        _.each(r.TableData, function(row) {
          if (results[row[idName]]) {
            var quarters = getQuartersForYear(+k);
            var newRow = results[row[idName]];
            newRow[k] = row[forCol.name];

            _.each(quarters, function(quarter) {
              newRow['Q' + quarter + '_' + k] = row['Q' + quarter + '_' + forCol.name];
            });

            if (_.has(forCol, 'filter')) {
              var p = [newRow[k]].concat(forCol.filterParams);
              newRow['f' + k] = $filter(forCol.filter).apply(undefined, p);

              _.each(quarters, function(quarter) {
                var pq = [row['Q' + quarter + '_' + forCol.name]].concat(forCol.filterParams);
                newRow['fQ' + quarter + '_' + k] = $filter(forCol.filter).apply(undefined, pq);
              });
            }
          }
        });
      });

      report.tableData = _.toArray(results);

      return report;
    }

    function hasGroupingKey(rows, viewBy) {
      if ($scope.query.groupBy.reportType != 'region' && $scope.query.groupBy.reportType != 'county' && $scope.query.groupBy.groupBy != 'geo'
        && viewBy != 'hospital' &&
        rows.length > 0 && _.some(rows, function (r) {
          return _.has(r, viewBy + 'Name')
        })) {
        return true;
      }

      return false;
    }

    function updateViewByColumnTitle() {
      var vbcol = _.findWhere($scope.columns, {name: 'ViewBy'});

      if ($scope.query.groupBy.groupBy == 'geo') {
        vbcol.title = viewByTitles[$scope.query.viewBy];
      }
      else if ($scope.query.groupBy.reportType == 'county') {
        vbcol.title = viewByTitles['patientcounty'];
      }
      else if ($scope.query.groupBy.reportType == 'region') {
        vbcol.title = viewByTitles['patientregion'];
      }
      else {
        vbcol.title = viewByTitles['hospital'];
      }
    }

    function formatRows(rows) {
      var result = _.map(rows, function (r) {
        formatRow(r);
        return r;
      });

      return result;
    }

    function formatRow(row) {
      _.each($scope.columns, function (c) {
        if (_.has(c, 'filter')) {
          var p = [row[c.name]].concat(c.filterParams);
          if (p.length > 0 && p[0] === undefined) {
            return;
          }
          row['f' + c.name] = $filter(c.filter).apply(undefined, p);
        }
      });
    }

    function populateRowLookups(rows) {
      var result = _.map(rows, function (r) {
        var newR = r;
        newR.viewBy = '';

        _.each(_.keys(lookupMap), function (key) {
          if (r[groupFieldMap[key]]) {
            var name = '', fs;

            if (lookupMap[key].data == null) {
              name = r[lookupMap[key].id];
            }
            else {
              fs = _.filter(lookupMap[key].data, function (lr) {
                return lr[lookupMap[key].id] == r[groupFieldMap[key]];
              });

              if (fs.length > 0) {
                var f = fs[0];
                name = f[lookupMap[key].label];
              }
            }

            newR[key + 'Name'] = name;
          }
        });

        if (_.has(r, 'CatVal')) {
          if (r.CatID != 0) {
            var strat = _.findWhere(stratification, {Id: r.CatID});
            var cat = strat && _.findWhere(stratLabels[strat.Name], {Id: r.CatVal});
          }
          newR.ViewBy = cat ? cat.Name : '';
        }
        else if ($scope.query.groupBy.groupBy == 'geo') {
          newR.ViewBy = newR[$scope.query.viewBy + 'Name'];
        }
        else if ($scope.query.groupBy.reportType == 'county') {
          newR.ViewBy = newR.patientcountyName;
        }
        else if ($scope.query.groupBy.reportType == 'region') {
          newR.ViewBy = newR.patientregionName;
        }
        else {
          newR.ViewBy = newR.hospitalName;
        }

        return newR;
      });

      return result;
    }

    function narrowRows(rows) {
      var query = $scope.query;
      if (query.narrowBy.name && query.narrowBy.value != 999999) {
        return _.filter(rows, function (r) {
          var result = false;
          if (query.narrowBy.name == 'zip') {
            result = checkZipDistance(r.Zip, query.narrowBy.value, query.narrowBy.value2);
          }
          else {
            result = r[groupFieldMap[query.narrowBy.name]] == query.narrowBy.value;
          }
          return result;
        });
      }

      return rows;
    }

    function sortRows(rows) {
      if ($scope.query.level.type == 'summary') {
        var sortParams = $scope.query.sortBy.split("."),
          sortField = sortParams[0],
          sortDir = sortParams[1];
      }
      else {
        var sortField = 'CatVal', sortDir = 'asc';
      }
      SortSvc.objSort(rows, sortField, sortDir);
    }

    function groupRows(rows, groupingField) {
      var result,
        groupByTitle = groupingField.substring(0, 1).toUpperCase() + groupingField.substring(1);

      var grouped = _.groupBy(rows, function (r) {
        return r[groupFieldMap[groupingField]];
      });

      result = _.map(grouped, function (g, k) {
        var groupRow, groupName = '';

        if (g && g.length > 0) {
          if ($scope.query.level.type === 'summary') {
            groupName = g[0][groupingField + 'Name'];
          }
          else if ($scope.query.level.type === 'detail') {
            var strat = _.findWhere(stratification, {Id: g[0]['CatID']});
            groupName = strat && strat.Caption || '';
          }
        }

        groupRow = {
          ViewBy: groupName,
          title: groupByTitle,
          CatID: g[0]['CatID']
        };

        for (var i = 0; i < $scope.columns.length; i++) {
          var colName = $scope.columns[i].name;
          if (colName === 'ViewBy') continue;

          groupRow[colName] = _.reduce(g, function (memo, row) {
            return memo + row[colName];
          }, 0);
        }

        formatRow(groupRow);

        return {
          group: groupRow,
          rows: g
        };
      });

      return result;
    }

    var zipQuery = null, zipCache = [];

    function getZipCodesByDistance(zip, distance) {
      var zk = zip + '|' + distance;

      if (zk === zipQuery) {
        return zipCache;
      }

      var hcoords = _.findWhere(hospitalZips, {Zip: zip});

      if (!hcoords) {
        return [];
      }

      var zips = _.filter(hospitalZips, function (z) {
        var dist = ZipDistanceSvc.calcDist(hcoords.Latitude, hcoords.Longitude, z.Latitude, z.Longitude);
        return dist <= distance;
      });

      zipQuery = zk;
      zipCache = _.pluck(zips, 'Zip');

      return zipCache;
    }

    function checkZipDistance(hzip, zip, distance) {
      if (_.isNumber(hzip)) {
        hzip = "" + hzip;
        if (hzip.length < 5) {
          var diff = 5 - hzip.length;
          for (var i = 0; i < diff; i++) {
            hzip = "0" + hzip;
          }
        }
      }

      var zips = getZipCodesByDistance(zip, distance);
      return _.contains(zips, hzip);
    }

  }


})();
