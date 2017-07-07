/**
 * Professional Product
 * Usage Data Report Module
 * Avoidable Stays Report Search Block Controller
 *
 * This controller manages the search interface for the avoidable stays reports, and initiates
 * searches in response to user input.
 *
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UDSearchAvoidableStaysCtrl', UDSearchAvoidableStaysCtrl);

  UDSearchAvoidableStaysCtrl.$inject = ['$scope', '$state', '$stateParams', 'UDAHSQuerySvc', 'ahsTopics', 'counties'];
  function UDSearchAvoidableStaysCtrl($scope, $state, $stateParams, UDAHSQuerySvc, ahsTopics, counties) {

    UDAHSQuerySvc.fromStateParams($stateParams);
    $scope.querySvc = UDAHSQuerySvc;
    $scope.query = UDAHSQuerySvc.query;

    $scope.ahsTopics = ahsTopics;
    $scope.counties = _.filter(counties, function(c) {
      return c.CountyID != -1;
    });

    var selCounty = $scope.query.county.county ? _.where($scope.counties, {CountyID: +$scope.query.county.county}) : null;
    $scope.uiaCounties = {
      rowLabel: 'CountyName',
      rowId: 'CountyID',
      widgetId: 'uia-county',
      widgetTitle: 'Select a County',
      defaultLabel: selCounty && selCounty.length > 0 ? selCounty[0].CountyName : null,
      hasAll: false,
      excludeZero: true,
      data: $scope.counties
    };


    $scope.getMeasures = function(id) {
      var t = _.findWhere(ahsTopics, {'id': +id});
      if (t) {
        return t.measures;
      }
      return null;
    };


    $scope.getActiveStep = function() {
      var fields = {
        'topic': ['topic', 'measure'],
        'county': ['county', 'topics']
      };

      if ($scope.query.reportType) {
        var fs = fields[$scope.query.reportType];

        for (var i = 0; i < fs.length; i++) {
          var val = null;
          val = $scope.query[$scope.query.reportType][fs[i]];
          if (val == undefined) {
            return fs[i];
          }
        }
      }
      else {
        return 'reportType';
      }

      return null;
    };

    $scope.isActiveStep = function(step) {
      return $scope.getActiveStep() == step;
    };

    $scope.canSearchCounty = function() {
      return _.size($scope.query.county.topics) > 0
        && _.some($scope.query.county.topics);
    };

    $scope.$watch('query.reportType', function(n, o) {
      if (n === o) return;

      UDAHSQuerySvc.reset();
      UDAHSQuerySvc.query.reportType = n;

      if (_.size($scope.query.county.topics) == 0) {
        _.each(ahsTopics, function(t) {
          $scope.query.county.topics[t.id] = true;
        });
      }

    });

    $scope.$watch('query.topic.topic', function(n, o) {
      if (n == o) return;
      var m = _.first($scope.getMeasures(UDAHSQuerySvc.query.topic.topic));
      if (m) {
        UDAHSQuerySvc.setMeasure(m.id);
      }
    });

    $scope.$watch('query.topic.measure', function(n, o) {
      if (n == o) return;
      if (n) {
        $scope.search();
      }
    });

    $scope.search = function() {
      var sp = UDAHSQuerySvc.toStateParams();
      if (sp.reportType === 'topic' && !sp.displayType) {
        var hasTable = $scope.ReportConfigSvc.webElementAvailable('AHS_ViewRates_Tab'),
          hasMap = $scope.ReportConfigSvc.webElementAvailable('AHS_MapView_Tab');

        if (hasTable) {
          sp.displayType = 'table';
        }
        else {
          sp.displayType = 'map';
        }
      }
      $state.go('top.professional.usage-data.avoidable-stays', sp, {inherit:false});
    }
  }

})();

