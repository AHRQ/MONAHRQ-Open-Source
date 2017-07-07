/**
 * Monahrq Flutter
 * Basic Table Flutter
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('flutters.BasicTableFlutter', ['smart-table'])
    .config(config)
    .controller('BasicTableFlutterCtrl', BasicTableFlutterCtrl)
    .directive('basicTableFlutterCompile', BasicTableFlutterCompileDirective);


  var FLUTTER_ID = 'gov.ahrq.BasicTableFlutter';

  /**
   * Angular config
   * Configures the Flutter's state(s) for routing.
   */
  config.$inject = ['$stateProvider'];
  function config($stateProvider) {
    $stateProvider.state('top.professional.flutters.BasicTableFlutter', {
      url: '/basic-table?reportId&filter',
      views: {
        'content@': {
          templateUrl: 'flutters/basic-table/views/basic-table.html',
          controller: 'BasicTableFlutterCtrl',
          resolve: {
            config: function(FlutterConfigSvc) {
              return FlutterConfigSvc.get(FLUTTER_ID);
            },
            report: function($stateParams, config) {
              return _.findWhere(config.reports, {id: $stateParams.reportId});
            },
            wing: function(report, SimpleReportLoaderSvc) {
              return SimpleReportLoaderSvc.load(report.custom.report);
            }
          }
        }
      }
    });
  }

  /**
   * BasicTableFlutterCtrl
   * Builds data model and respond to user interactions.
   */
  BasicTableFlutterCtrl.$inject = ['$scope', '$filter', '$stateParams', 'config', 'report', 'wing'];
  function BasicTableFlutterCtrl($scope, $filter, $stateParams, config, report, wing) {
    $scope.getValue = getValue;
    $scope.page = report.page;
    $scope.table = report.custom.table;

    init();


    function init() {
      updateTable();
    }

    function updateTable() {
      var data = wing.data;

      if ($stateParams.filter) {
        var parts = $stateParams.filter.split('=');
        if (parts.length == 2) {
          data = _.filter(data, function(row) {
            return row[parts[0]] == parts[1];
          });
        }
      }

      $scope.model = data;
      $scope.displayModel = [];


      _.each($scope.model, function(row) {
        formatRow(row);
        $scope.displayModel.push(row);
      });

    }

    function formatRow(row) {
      _.each(report.custom.table.columns, function(col) {
        if (_.has(col, 'format')) {
          row['f' + col.name] = formatField(col, row[col.name]);
        }
      });
    }

    function formatField(col, value) {
      var options = [];
      if (_.has(col, 'formatOptions')) {
        options = col.formatOptions;
      }

      if (col.format === 'html') {
        return value; //$sce.trustAsHtml(value);
      }
      else {
        var params = [value].concat(options);
        return $filter(col.format).apply(undefined, params);
      }
    }

    function getValue(field, row) {
      if (_.has(row, 'f' + field)) {
        return row['f' + field];
      }

      return row[field];
    }
  }

  function BasicTableFlutterCompileDirective($compile) {
    // directive factory creates a link function
    return function(scope, element, attrs) {
      scope.$watch(
        function(scope) {
          // watch the 'compile' expression for changes
          return scope.$eval(attrs.basicTableFlutterCompile);
        },
        function(value) {
          // when the 'compile' expression changes
          // assign it into the current DOM
          element.html(value);

          // compile the new DOM and link it to the current
          // scope.
          // NOTE: we only compile .childNodes so that
          // we don't get into infinite loop compiling ourselves
          $compile(element.contents())(scope);
        }
      );
    };
  }

})();


