/**
 * Professional Product
 * Usage Data Report Module
 * Utilization Report Controller
 *
 * This controller services as a simple wrapper around the report table. It manages the
 * report header and footer regions, as well as the report narrowing toolbar displayed
 * next to the table.
 *
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UDReportUtilCtrl', UDReportUtilCtrl);

  UDReportUtilCtrl.$inject = ['$scope', '$state', '$stateParams', 'UDUtilQuerySvc',
    'ReportConfigSvc', 'content',
    'hospitals', 'hospitalRegions', 'hospitalTypes', 'hospitalCounties', 'zipDistances', 'patientRegions', 'patientCounties',
    'ccs', 'ccsCategories', 'mdc', 'drg', 'drgCategories', 'prccs', 'prccsCategories'];
  function UDReportUtilCtrl($scope, $state, $stateParams, UDUtilQuerySvc,
    ReportConfigSvc, content,
    hospitals, hospitalRegions, hospitalTypes, hospitalCounties, zipDistances, patientRegions, patientCounties,
    ccs, ccsCategories, mdc, drg, drgCategories, prccs, prccsCategories) {

    var narrowState = {};

    $scope.querySvc = UDUtilQuerySvc;
    $scope.query = UDUtilQuerySvc.query;

    $scope.content = content;

    $scope.hospitals = hospitals;
    $scope.hospitalRegions= [{RegionID: 999999, Name: 'All'}].concat(hospitalRegions);
    $scope.hospitalCounties = hospitalCounties;
    $scope.hospitalTypes = [{HospitalTypeID: 999999, Name: 'All'}].concat(hospitalTypes);
    $scope.zipDistances = zipDistances;
    $scope.patientRegions= [{RegionID: 999999, Name: 'All'}].concat(patientRegions);
    $scope.patientCounties = patientCounties;
    $scope.ccsCategories = ccsCategories;
    $scope.ccs = _.groupBy(ccs, function(r) { return r.CategoryID; });
    $scope.mdc = mdc;
    $scope.drgCategories = drgCategories;
    $scope.drg = _.groupBy(drg, function(r) { return r.CategoryID; });
    $scope.prccsCategories = prccsCategories;
    $scope.prccs = _.groupBy(prccs, function(r) { return r.CategoryID; });

    function uiaCatify(data, dataCat) {
      return _.map(data, function(r) {
        var cat = _.findWhere(dataCat, {Id: r.CategoryID});
        var catName = cat ? cat.Name : '(Unknown Category)';

        return {
          Id: r.Id,
          Description: r.Id + ' ' + r.Description,
          category: catName
        };
      });
    }

    function uiaGetSelected(data) {
      return $scope.query.narrowBy.value ? _.where(data, {Id: +$scope.query.narrowBy.value}) : null;
    }

    var uiaCCS = uiaCatify(ccs, ccsCategories);
    var selCCS = uiaGetSelected(uiaCCS);
    $scope.uiaCCSNarrow = {
      rowLabel: 'Description',
      rowId: 'Id',
      widgetId: 'uia-ccs-narrow',
      widgetTitle: 'Select a Condition or Topic',
      defaultLabel: selCCS && selCCS.length > 0 ? selCCS[0].Description : null,
      hasAll: true,
      allId: 999999,
      data: uiaCCS,
      onChange: function() {
        $scope.query.narrowBy.name = 'condition';
      }
    };

    var uiaDRG = uiaCatify(drg, drgCategories);
    var selDRG = uiaGetSelected(uiaDRG);
    $scope.uiaDRGNarrow = {
      rowLabel: 'Description',
      rowId: 'Id',
      widgetId: 'uia-drg-narrow',
      widgetTitle: 'Select a Diagnostic Related Group',
      defaultLabel: selDRG && selDRG.length > 0 ? selDRG[0].Description : null,
      hasAll: true,
      allId: 999999,
      data: uiaDRG,
      onChange: function() {
        $scope.query.narrowBy.name = 'drg';
      }
    };

    var uiaPRCCS = uiaCatify(prccs, prccsCategories);
    var selPRCCS = uiaGetSelected(uiaPRCCS);
    $scope.uiaPRCCSNarrow = {
      rowLabel: 'Description',
      rowId: 'Id',
      widgetId: 'uia-prccs-narrow',
      widgetTitle: 'Select a Procedure',
      defaultLabel: selPRCCS && selPRCCS.length > 0 ? selPRCCS[0].Description : null,
      hasAll: true,
      allId: 999999,
      data: uiaPRCCS,
      onChange: function() {
        $scope.query.narrowBy.name = 'procedure';
      }
    };

    var uiaMDC = _.map(mdc, function(r) {
      return {
        Id: r.Id,
        Description: (+r.Id != 99 ? r.Id + ' ' : '') + r.Description,
      };
    });
    var selMDC = uiaGetSelected(uiaMDC);
    $scope.uiaMDCNarrow = {
      rowLabel: 'Description',
      rowId: 'Id',
      widgetId: 'uia-mdc-narrow',
      widgetTitle: 'Select a Major Diagnosis Category',
      defaultLabel: selMDC && selMDC.length > 0 ? selMDC[0].Description : null,
      hasAll: true,
      allId: 999999,
      data: uiaMDC,
      onChange: function() {
        $scope.query.narrowBy.name = 'mdc';
      }
    };

    var selHos = $scope.query.narrowBy.value ? _.where(hospitals, {Id: +$scope.query.narrowBy.value}) : null;
    $scope.uiaHospitalsNarrow = {
      rowLabel: 'Name',
      rowId: 'Id',
      widgetId: 'uia-hospital-narrow',
      widgetTitle: 'Select a Hospital',
      defaultLabel: selHos && selHos.length > 0 ? selHos[0].Name : null,
      data: hospitals,
      onChange: function() {
        $scope.query.narrowBy.name = 'hospital';
      }
    };

    var selCounty = $scope.query.narrowBy.value ? _.where(hospitalCounties, {CountyID: +$scope.query.narrowBy.value}) : null;
    $scope.uiaCountiesNarrow = {
      rowLabel: 'CountyName',
      rowId: 'CountyID',
      widgetId: 'uia-county-narrow',
      widgetTitle: 'Select a County',
      defaultLabel: selCounty && selCounty.length > 0 ? selCounty[0].CountyName : null,
      hasAll: true,
      allId: 999999,
      data: hospitalCounties,
      onChange: function() {
        $scope.query.narrowBy.name = 'county';
      }
    };

    var selPatientCounty = $scope.query.narrowBy.value ? _.where(patientCounties, {CountyID: +$scope.query.narrowBy.value}) : null;
    $scope.uiaPatientCountiesNarrow = {
      rowLabel: 'CountyName',
      rowId: 'CountyID',
      widgetId: 'uia-county-narrow',
      widgetTitle: 'Select a Patient County',
      defaultLabel: selPatientCounty && selPatientCounty.length > 0 ? selPatientCounty[0].CountyName : null,
      hasAll: true,
      allId: 999999,
      data: patientCounties,
      onChange: function() {
        $scope.query.narrowBy.name = 'patientcounty';
      }
    };

    $scope.utilDrillup = function() {
      $scope.query.level.type = 'summary';
      $scope.query.level.value = null;
      $state.go('top.professional.usage-data.service-use', UDUtilQuerySvc.toStateParams());
    };

    $scope.showReport = function() {
      return !(_.isNull(UDUtilQuerySvc.query.groupBy.value) || _.isUndefined(UDUtilQuerySvc.query.groupBy.value) || _.isNaN(UDUtilQuerySvc.query.groupBy.value));
    };

    $scope.goToTable = function() {
      var sp = UDUtilQuerySvc.toStateParams();
      sp.displayType = 'table';
      $state.go('top.professional.usage-data.service-use', sp);
    };

    $scope.goToMap = function() {
      var sp = UDUtilQuerySvc.toStateParams();
      sp.displayType = 'map';
      $state.go('top.professional.usage-data.service-use', sp);
    };

    $scope.narrowViewAll = function() {
      clearNarrow();
    };

    $scope.$watch('query.viewBy', function(nv, ov) {
      $scope.toggleNarrow(nv);
    });

    $scope.showSideSearch = function() {
      return $scope.query.displayType != 'map';
    };

    $scope.isActiveNarrow = function(name) {
      return narrowState[name] == true;
    };

    $scope.toggleNarrow = function(name) {
      var oldState = narrowState[name];
      narrowState = {};
      narrowState[name] = !oldState;
      clearNarrow();
    };

    function getReportId() {
      var rt = $scope.query.groupBy.reportType;
      return $state.current.data.report[rt + '_summary'];
    }

    $scope.canShowNarrow = function(name) {
      var vb = UDUtilQuerySvc.query.viewBy,
        rt = UDUtilQuerySvc.query.groupBy.reportType,
        gb = UDUtilQuerySvc.query.groupBy.groupBy,
        gbop = (gb === 'geo' ? 'clinical' : 'geo');

      if (vb == name
         || (gb === 'geo' && name === 'view-all-clinical')
         || (gb === 'clinical' && name === 'view-all-geo')) {
        return true;
      }

      var rptConfig = ReportConfigSvc.configForReport(getReportId());
      var dimNames = [];
      if (gbop == 'geo' && rptConfig) {
        dimNames = rptConfig.GeoInfo;
      }
      else if (rptConfig) {
        dimNames = rptConfig.ClinicalDRGAndDiagnosis;
      }
      var dim = ReportConfigSvc.getDimensionNameFrom(name);
      if (gbop === 'geo' && _.contains(dimNames, dim)) {
        return true;
      }

      // force allow zip searches in narrow, until we can put it in search also
      if (gb === 'clinical' && name == 'zip') {
        return true;
      }

      return false;
    };

    $scope.hasTab = function(name) {
      var tableName, mapName, hasTable, hasMap, isCustomRegion = false;

      tableName = $scope.getWebElementForType('table');
      mapName = $scope.getWebElementForType('map');
      if (!tableName || !mapName) return false;

      hasTable = $scope.ReportConfigSvc.webElementAvailable(tableName);
      hasMap = $scope.ReportConfigSvc.webElementAvailable(mapName);

      if ($scope.query.groupBy.reportType == 'region' && $scope.config.REGIONAL_CONTEXT == 'CustomRegion')  {
        isCustomRegion = true;
      }

      if (name == 'table' && hasTable) {
        return true;
      }
      else if (name == 'map' && hasMap && $scope.query.groupBy.groupBy == 'clinical' && !isCustomRegion) {
        return true;
      }

      return false;
    };

    $scope.isActiveTab = function(name) {
      return $scope.query.displayType == name;
    };

    $scope.checkZipNarrow = function() {
      var query = $scope.query;
      if (query.narrowBy.value2 && query.narrowBy.value && query.narrowBy.value.length == 5) {
        query.narrowBy.name = 'zip';
      }
      else {
        query.narrowBy.name = null;
      }
    };


    function clearNarrow() {
      $scope.query.narrowBy.name = null;
      $scope.query.narrowBy.value = null;
      $scope.query.narrowBy.value2 = null;
    }

  }

})();

