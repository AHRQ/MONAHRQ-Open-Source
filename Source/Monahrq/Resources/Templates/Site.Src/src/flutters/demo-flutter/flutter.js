/**
 * Monahrq Flutter
 * Demo Flutter
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('flutters.demoFlutter', [])
    .config(config)
    .controller('DemoFlutterCtrl', DemoFlutterCtrl);

  var FLUTTER_ID = 'org.myagency.DemoFlutter';

  /**
   * Angular config
   * Configures the Flutter's state(s) for routing.
   */
  config.$inject = ['$stateProvider'];
  function config($stateProvider) {
    $stateProvider.state('top.professional.flutters.demoFlutter', {
      url: '/demo-flutter?reportId',
      views: {
        'content@': {
          templateUrl: 'flutters/demo-flutter/views/flutter.html',
          controller: 'DemoFlutterCtrl',
          resolve: {
            config: function(FlutterConfigSvc) {
              return FlutterConfigSvc.get(FLUTTER_ID);
            },
           report: function($stateParams, config) {
              return _.findWhere(config.reports, {id: $stateParams.reportId});
            },
            // Example of loading a data file via a ui-router resolve. Note that this has a dependency on its
            // sibling resolve 'config'.
            nursingHomeIndex: function(report, SimpleReportLoaderSvc) {
              // The id parameter can be omitted if there is just a single data file with a static filename.
              return SimpleReportLoaderSvc.load(report.custom.nursingHomeIndex);
            }
          }
        }
      }
    });
  }

  /**
   * DemoFlutterCtrl
   * Builds data model and respond to user interactions.
   */
  DemoFlutterCtrl.$inject = ['$scope', 'SimpleReportLoaderSvc', 'config', 'report', 'nursingHomeIndex'];
  function DemoFlutterCtrl($scope, SimpleReportLoaderSvc, config, report, nursingHomeIndex) {
    var reportData = [], measureDefs = [];

    $scope.page = report.page;
    $scope.model = [];
    $scope.query = {
      rating: null,
      nhId: null
    };
    $scope.ratingOptions = [
      {id: 1, name: '1'},
      {id: 2, name: '2'},
      {id: 3, name: '3'},
      {id: 4, name: '4'},
      {id: 5, name: '5'}
    ];
    $scope.nursingHomeOptions = [];

    init();


    function init() {
      buildNursingHomeOptions();

      // Reload report data when the user selects a different nursing home from the dropdown.
      $scope.$watch('query.nhId', loadReport);

      // Filter an existing report when the user selects a different rating from the dropdown.
      $scope.$watch('query.rating', processReport);
    }

    function buildNursingHomeOptions() {
      $scope.nursingHomeOptions = _.map(nursingHomeIndex.data, function(row) {
        return {
          id: row.ID,
          name: row.Name
        };
      });
    }

    // Example of loading a report file in response to a user interaction on the report page.
    function loadReport(nv, ov) {
      if (nv == ov) return;

      $scope.query.rating = null;

      SimpleReportLoaderSvc.load(report.custom.nursingHome, $scope.query.nhId)
        .then(function(_report) {
          reportData = _report.data;

          // Once the report is loaded, we want to load every measure definition that the report references.
          var measureIds = _.pluck(reportData, 'MeasureID');
          loadMeasureDefs(measureIds)
            .then(processReport);
        });
    }

    function loadMeasureDefs(ids) {
      return SimpleReportLoaderSvc.bulkLoad(report.custom.measureDefs, ids)
        .then(function(defs) {
          // The report loader wraps each data file in an object containing extra metadata. We will unwrap it.
          measureDefs = _.pluck(defs, 'data');
        });
    }

    function processReport(nv, ov) {
      if (nv == ov || reportData.length == 0 || measureDefs.length == 0) return;

      // Find every report row whose NatRating matches the value the user specified
      var rows = _.filter(reportData, function(row) {
        return row.NatRating == $scope.query.rating;
      });

      // Build a model for the template to render. It is a simplified version of a report row, that also adds
      // a new property for the measure name.
      $scope.model = _.map(rows, function(row) {
        var measureDef = _.findWhere(measureDefs, {MeasureID: row.MeasureID});
        var measureName = measureDef ? measureDef.PlainTitle : null;

        return {
          MeasureID: row.MeasureID,
          MeasureName: measureName,
          NatRating: row.NatRating
        };
      });
    }

  }

})();


