/**
 * Professional Product
 * Quality Ratings Module
 * Condition Search Block Controller
 *
 * This controller manages the search interface for the condition report, and initiates
 * searches in response to user input.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings')
    .controller('QRSearchConditionCtrl', QRSearchConditionCtrl);


  QRSearchConditionCtrl.$inject = ['$rootScope', '$scope', '$state', '$stateParams', '_',
                                               'QRQuerySvc',
                                               'hospitals', 'hospitalTypes', 'zipDistances', 'hospitalRegions',
                                               'measureTopicCategories', 'measureTopics'];
  function QRSearchConditionCtrl($rootScope, $scope, $state, $stateParams, _,
    QRQuerySvc,
    hospitals, hospitalTypes, zipDistances, hospitalRegions,
    measureTopicCategories, measureTopics) {

    QRQuerySvc.fromStateParams($stateParams);
    QRQuerySvc.query.sortBy = 'name.asc';
    QRQuerySvc.query.displayType = 'symbols';
    QRQuerySvc.query.comparedTo = 'nat';

    $scope.querySvc = QRQuerySvc;
    $scope.query = QRQuerySvc.query;

    $scope.measureTopicCategories = measureTopicCategories;
    $scope.measureTopics = [];
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


    var selHos = $scope.query.hospitalName ? _.where(hospitals, {Id: +$scope.query.hospitalName}) : null;
    $scope.uiaHospitals = {
      rowLabel: 'Name',
      rowId: 'Id',
      widgetId: 'uia-hospital',
      widgetTitle: 'Hospital Name',
      defaultLabel: selHos && selHos.length > 0 ? selHos[0].Name : null,
      data: hospitals
    };


    $scope.showWizardTabs = function() {
      if ($scope.ReportConfigSvc.webElementAvailable('Quality_ConditionTopic_Tab')
       && $scope.ReportConfigSvc.webElementAvailable('Quality_Hospital_Tab')) {
        return true;
      }
      return false;
    };

    $scope.getActiveStep = function() {
      var fields = ['topic', 'subtopic', 'searchType', 'geoType', 'geoValue'];
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

    $scope.$watch('query.topic', function(n, o) {
      $scope.measureTopics = _.filter(measureTopics, function(r) {
        return r.TopicCategoryID == $scope.query.topic;
      });

      if (n != o) {
        resetNarrow();
        QRQuerySvc.setSubtopic(_.first($scope.measureTopics)['TopicID']);
      }
    });

    $scope.$watch('query.subtopic', function(n, o) {
      if (n === o) return;
      resetNarrow();
      search();
    });

    $scope.$watch('query.searchType', function(n, o) {
      if (n === o) return;
      resetNarrow('searchType');
    });

    $scope.$watch('query.geoType', function(n, o) {
      if (n === o) return;
      resetNarrow('geoType');
    });


    $scope.canNarrow = function() {
      return ($scope.query.hospitalName != undefined && $scope.query.hospitalName != null)
        || $scope.query.hospitalType || $scope.query.hospitalType === 0
        || !_.isEmpty($scope.query.zip)
        || $scope.query.region || $scope.query.region === 0;
    };

    $scope.narrow = function() {
      var sp = QRQuerySvc.toStateParams();
      $state.go('top.professional.quality-ratings.condition', sp);
    };

    function search() {
      var sp = QRQuerySvc.toStateParams();
      $state.go('top.professional.quality-ratings.condition', sp);
    }

    function resetNarrow(at) {
      var q = $scope.query;

      if (!at || at === 'geoType') {
        q.zip = null;
        q.zipDistance = null;
        q.region = null;
      }
      if (!at || at === 'searchType') {
        q.geoType = null;
        q.hospitalName = null;
        q.hospitalType = null;
      }
      if (!at) {
        q.searchType = null;
      }
    }

  }

})();

