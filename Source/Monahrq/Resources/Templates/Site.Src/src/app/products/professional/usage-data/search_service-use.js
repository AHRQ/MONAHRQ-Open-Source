/**
 * Professional Product
 * Usage Data Report Module
 * Utilization Report Search Block Controller
 *
 * This controller manages the search interface for the utilization reports, and initiates
 * searches in response to user input.
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UDSearchServiceUseCtrl', UDSearchServiceUseCtrl);

  UDSearchServiceUseCtrl.$inject = ['$scope', '$state', '$stateParams', 'UDUtilQuerySvc', 'ReportConfigSvc',
    'hospitals', 'hospitalRegions', 'hospitalTypes', 'hospitalCounties', 'zipDistances', 'patientRegions', 'patientCounties',
    'ccs', 'ccsCategories', 'mdc', 'drg', 'drgCategories', 'prccs', 'prccsCategories'];
  function UDSearchServiceUseCtrl($scope, $state, $stateParams, UDUtilQuerySvc, ReportConfigSvc,
    hospitals, hospitalRegions, hospitalTypes, hospitalCounties, zipDistances, patientRegions,  patientCounties,
    ccs, ccsCategories, mdc, drg, drgCategories, prccs, prccsCategories) {

    $scope.dimensionOptions = [];

    UDUtilQuerySvc.fromStateParams($stateParams);
    $scope.querySvc = UDUtilQuerySvc;
    $scope.query = UDUtilQuerySvc.query;


    updateDimensionOptions();
    defaultDimensionOption();

    $scope.hospitals = hospitals;
    $scope.hospitalRegions= hospitalRegions;
    $scope.hospitalCounties = hospitalCounties;
    $scope.hospitalTypes = hospitalTypes;
    $scope.zipDistances = zipDistances;
    $scope.patientRegions = patientRegions;
    $scope.patientCounties = patientCounties;
    $scope.ccsCategories = ccsCategories;
    $scope.ccs = _.groupBy(ccs, function(r) { return r.CategoryID; });
    $scope.mdc = mdc;
    $scope.drgCategories = drgCategories;
    $scope.drg = _.groupBy(drg, function(r) { return r.CategoryID; });
    $scope.prccsCategories = prccsCategories;
    $scope.prccs = _.groupBy(prccs, function(r) { return r.CategoryID; });


    var uiaCCS = _.map(ccs, function(r) {
      var cat = _.findWhere(ccsCategories, {Id: r.CategoryID});
      var catName = cat ? cat.Name : null;

      var ccs = {
        Id: r.Id,
        Description: r.Id + ' ' + r.Description,
        category: catName
      };
      return ccs;
    });

    var selCCS = $scope.query.groupBy.value ? _.where(uiaCCS, {Id: +$scope.query.groupBy.value}) : null;
    $scope.uiaCCS = {
      rowLabel: 'Description',
      rowId: 'Id',
      widgetId: 'uia-ccs',
      widgetTitle: 'Select a Condition or Topic',
      defaultLabel: selCCS && selCCS.length > 0 ? selCCS[0].Description : null,
      hasAll: true,
      data: uiaCCS
    };

    var uiaMDC = _.map(mdc, function(r) {
      var mdc = {
        Id: r.Id,
        Description: (+r.Id != 99 ? r.Id + ' ' : '') + r.Description,
      };

      return mdc;
    });

    var selMDC = $scope.query.groupBy.value ? _.where(uiaMDC, {Id: +$scope.query.groupBy.value}) : null;
    $scope.uiaMDC = {
      rowLabel: 'Description',
      rowId: 'Id',
      widgetId: 'uia-mdc',
      widgetTitle: 'Select a Major Diagnosis Category',
      defaultLabel: selMDC && selMDC.length > 0 ? selMDC[0].Description : null,
      hasAll: true,
      data: uiaMDC
    };

    var uiaDRG = _.map(drg, function(r) {
      var cat = _.findWhere(drgCategories, {Id: r.CategoryID});
      var catName = cat ? cat.Name : '(Unknown Category)';

      var drg = {
        Id: r.Id,
        Description: r.Id + ' ' + r.Description,
        category: catName
      };
      return drg;
    });

    var selDRG = $scope.query.groupBy.value ? _.where(uiaDRG, {Id: +$scope.query.groupBy.value}) : null;
    $scope.uiaDRG = {
      rowLabel: 'Description',
      rowId: 'Id',
      widgetId: 'uia-drg',
      widgetTitle: 'Select a Diagnostic Related Group',
      defaultLabel: selDRG && selDRG.length > 0 ? selDRG[0].Description : null,
      hasAll: true,
      data: uiaDRG
    };

    var uiaPRCCS = _.map(prccs, function(r) {
      var cat = _.findWhere(prccsCategories, {Id: r.CategoryID});
      var catName = cat ? cat.Name : '(Unknown Category)';

      var prccs = {
        Id: r.Id,
        Description: r.Id + ' ' + r.Description,
        category: catName
      };
      return prccs;
    });

    var selPRCCS = $scope.query.groupBy.value ? _.where(uiaPRCCS, {Id: +$scope.query.groupBy.value}) : null;
    $scope.uiaPRCCS = {
      rowLabel: 'Description',
      rowId: 'Id',
      widgetId: 'uia-prccs',
      widgetTitle: 'Select a Procedure',
      defaultLabel: selPRCCS && selPRCCS.length > 0 ? selPRCCS[0].Description : null,
      hasAll: true,
      data: uiaPRCCS
    };

    var selHos = $scope.query.groupBy.value ? _.where(hospitals, {Id: +$scope.query.groupBy.value}) : null;
    $scope.uiaHospitals = {
      rowLabel: 'Name',
      rowId: 'Id',
      widgetId: 'uia-hospital',
      widgetTitle: 'Select a Hospital',
      defaultLabel: selHos && selHos.length > 0 ? selHos[0].Name : null,
      hasAll: true,
      data: hospitals
    };

    var selCounty = $scope.query.groupBy.value ? _.where(hospitalCounties, {CountyID: +$scope.query.groupBy.value}) : null;
    $scope.uiaCounties = {
      rowLabel: 'CountyName',
      rowId: 'CountyID',
      widgetId: 'uia-county',
      widgetTitle: 'Select a County',
      defaultLabel: selCounty && selCounty.length > 0 ? selCounty[0].CountyName : null,
      hasAll: true,
      data: hospitalCounties
    };

    var selPatientCounty = $scope.query.groupBy.value ? _.where(patientCounties, {CountyID: +$scope.query.groupBy.value}) : null;
     $scope.uiaPatientCounties = {
      rowLabel: 'CountyName',
      rowId: 'CountyID',
      widgetId: 'uia-county',
      widgetTitle: 'Select a Patient County',
      defaultLabel: selPatientCounty && selPatientCounty.length > 0 ? selPatientCounty[0].CountyName : null,
      hasAll: true,
      data: patientCounties
    };

    $scope.getActiveStep = function() {
      var fields = ['viewBy', 'reportType', 'groupBy', 'dimension', 'value'];
      for (var i = 0; i < fields.length; i++) {
        var val = $scope.query.groupBy[fields[i]];
        if (val == null || val == '') {
          return fields[i];
        }
      }

      return null;
    };

    $scope.isActiveStep = function(step) {
      return $scope.getActiveStep() == step;
    };

    $scope.$watch('query.groupBy.viewBy',  function(newValue, oldValue) {
      if (newValue === oldValue) return;
      resetGroupBy('reportType');
    });

    $scope.$watch('query.groupBy.reportType',  function(newValue, oldValue) {
      if (newValue === oldValue) return;
      resetGroupBy('groupBy');
    });

    $scope.$watch('query.groupBy.groupBy',  function(newValue, oldValue) {
      if (newValue === oldValue) return;
      updateDimensionOptions();
      resetGroupBy('dimension');
      defaultDimensionOption();
    });

    $scope.$watch('query.groupBy.dimension',  function(newValue, oldValue) {
      if (newValue === oldValue) return;
      resetGroupBy('value');
    });

    $scope.$watch('query.groupBy.value', function(newValue, oldValue) {
      if (newValue === oldValue) return;
      if (newValue) {
        goToUtilizationReport();
      }
    });

    function getReportId() {
      var rt = $scope.query.groupBy.reportType;
      return $state.current.data.report[rt + '_summary'];
    }

    function updateDimensionOptions() {
      var gb = $scope.query.groupBy.groupBy,
        rt = $scope.query.groupBy.reportType;

      if (!gb || !rt) return;

      var rptConfig = ReportConfigSvc.configForReport(getReportId());
      var dimNames = [];
      if (gb == 'geo') {
        dimNames = rptConfig.GeoInfo;
      }
      else {
        dimNames = rptConfig.ClinicalDRGAndDiagnosis;
      }

      $scope.dimensionOptions = _.map(dimNames, function(name) {
        var id = ReportConfigSvc.getDimensionIdFrom(name);
        return {
          id: id,
          name: name
        };
      });


/*        _.filter(dimensions[gb], function(r) {
        return _.contains(r.reportTypes, rt);
      });*/

/*      if (gb === 'geo') {
        $scope.dimensionPlaceholder = 'Geo'
      }
      else  {
        $scope.dimensionPlaceholder = 'Clinical';
      }
      */
    }

    function defaultDimensionOption() {
      if ($scope.query.groupBy.groupBy && _.size($scope.dimensionOptions) == 1) {
        UDUtilQuerySvc.setDimension($scope.dimensionOptions[0].id);
      }
    }

    function resetGroupBy(at) {
      var gb =  $scope.query.groupBy;

      if (at === 'value' || at == 'dimension' || at == 'groupBy' || at == 'reportType' || at == 'viewBy') {
        UDUtilQuerySvc.setValue(gb.dimension, null);
        gb.value2 = null;
      }
      if (at === 'dimension' || at == 'groupBy' || at == 'reportType' || at == 'viewBy') {
        UDUtilQuerySvc.setDimension(null);
      }
      if (at === 'groupBy' || at == 'reportType' || at == 'viewBy') {
        gb.groupBy = null;
      }
      if (at === 'reportType' || at == 'viewBy') {
        gb.reportType = null;
      }
      if (at === 'viewBy') {
        gb.viewBy = null;
      }

      $scope.query.level.type = 'summary';
      $scope.query.level.value = null;
      $scope.query.viewBy = null;
      $scope.query.displayType = null;
    }

    function goToUtilizationReport() {
      var sp = UDUtilQuerySvc.toStateParams();

      if (!sp.displayType) {
        var tableName, hasTable;
        tableName = $scope.getWebElementForType('table');

        if (!tableName) {
          sp.displayType = 'map';
        }
        else {
          hasTable = $scope.ReportConfigSvc.webElementAvailable(tableName);
          if (hasTable) {
            sp.displayType = 'table';
          }
          else {
            sp.displayType = 'map';
          }
        }
      }

      if (!sp.levelType) sp.levelType = 'summary';

      $state.go('top.professional.usage-data.service-use', sp);
    }

  }

})();

