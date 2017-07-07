/**
 * Professional Product
 * Quality Ratings Module
 * Location Search Block Controller
 *
 * This controller manages the search interface for the location report, and initiates
 * searches in response to user input.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings')
    .controller('QRSearchLocationCtrl', QRSearchLocationCtrl);


  QRSearchLocationCtrl.$inject = ['$rootScope', '$scope', '$state', '$stateParams', '_', 'QRQuerySvc',
    'hospitals', 'hospitalTypes', 'hospitalRegions', 'zipDistances'];
  function QRSearchLocationCtrl($rootScope, $scope, $state, $stateParams, _, QRQuerySvc,
    hospitals, hospitalTypes, hospitalRegions, zipDistances) {

    $scope.hospitals = hospitals;
    $scope.hospitalTypes = [{HospitalTypeID: 999999, Name: 'All'}].concat(hospitalTypes);
    $scope.hospitalRegions = [{RegionID: 999999, Name: 'All'}].concat(hospitalRegions);
    $scope.zipDistances = zipDistances;
    $scope.searchTypes = [
      {id: 'hospitalName', name: 'Hospital Name'},
      {id: 'hospitalType', name: 'Hospital Type'},
      {id: 'geo', name: 'Geographic Info'}
    ];
    $scope.geoTypes = [
      {id: 'zip', name: 'Zip Code'},
      {id: 'region', name: 'Region'}
    ];


    QRQuerySvc.fromStateParams($stateParams);

    if ($state.is('top.professional.quality-ratings.profile')) {
      QRQuerySvc.query.searchType = 'hospitalName';
      QRQuerySvc.query.hospitalName = $stateParams.id;
    }

    $scope.querySvc = QRQuerySvc;
    $scope.query = QRQuerySvc.query;

    var selHos = $scope.query.hospitalName ? _.where(hospitals, {Id: +$scope.query.hospitalName}) : null;
    $scope.uiaHospitals = {
      rowLabel: 'Name',
      rowId: 'Id',
      widgetId: 'uia-hospital',
      widgetTitle: 'Hospital Name',
      defaultLabel: selHos && selHos.length > 0 ? selHos[0].Name : null,
      data: hospitals
    };



    $scope.$watch('query.searchType', function(n, o) {
      if (n === o) return;
      resetQuery('searchType');
    });

    $scope.$watch('query.geoType', function(n, o) {
      if (n === o) return;
      resetQuery('geoType');
    });


    $scope.showWizardTabs = function() {
      if ($scope.ReportConfigSvc.webElementAvailable('Quality_ConditionTopic_Tab')
       && $scope.ReportConfigSvc.webElementAvailable('Quality_Hospital_Tab')) {
        return true;
      }
      return false;
    };

    $scope.getActiveStep = function() {
      var fields = ['searchType', 'geoType', 'geoValue'];
      for (var i = 0; i < fields.length; i++) {
        var val = $scope.query[fields[i]];
        if (val == null || val == '') {
          return fields[i];
        }
      }

      return null;
    };

    $scope.isActiveStep = function(step) {
      return $scope.getActiveStep() == step;
    };


    $scope.showZipRegion = function(target){
      return target === $scope.query.searchType;
    };

    $scope.canSearch = function() {
      return ($scope.query.hospitalName != undefined && $scope.query.hospitalName != null)
        || $scope.query.hospitalType || $scope.query.hospitalType === 0
        || !_.isEmpty($scope.query.zip)
        || $scope.query.region || $scope.query.region === 0;
    };

    $scope.search = function(){
      if ($scope.query.hospitalName != undefined && $scope.query.hospitalName != null) {
        $state.go('top.professional.quality-ratings.profile', {
          id: $scope.query.hospitalName
        });
      }
      else {
        var sp = QRQuerySvc.toStateParams();
        $state.go('top.professional.quality-ratings.location', sp);
      }
    };


    function resetQuery(at) {
      var q = $scope.query;

      if (at === 'geoType') {
        q.zip = null;
        q.zipDistance = null;
        q.region = null;
      }
      if (at === 'searchType') {
        q.geoType = null;
        q.hospitalName = null;
        q.hospitalType = null;
      }
    }
  }

})();

