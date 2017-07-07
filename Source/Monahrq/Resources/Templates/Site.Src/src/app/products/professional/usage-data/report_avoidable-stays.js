/**
 * Professional Product
 * Usage Data Report Module
 * Avoidable Stays Condition Report Controller
 *
 * This controller services as a simple wrapper around the report table. It manages the
 * report header and footer regions.
 *
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UDReportAvoidableStaysCtrl', UDReportAvoidableStaysCtrl);

  UDReportAvoidableStaysCtrl.$inject = ['$scope', '$state', '$stateParams', 'UDAHSQuerySvc', 'content', 'ahsTopics'];
  function UDReportAvoidableStaysCtrl($scope, $state, $stateParams, UDAHSQuerySvc, content, ahsTopics) {

    $scope.query = UDAHSQuerySvc.query;
    $scope.content = content;

    $scope.currentCondition = currentCondition();
    $scope.currentMeasure = currentMeasure();

    if ($scope.currentCondition) {
      $scope.content.header.body = $scope.content.header.body + '<h3>' + $scope.currentCondition + '</h3><p>' + $scope.currentMeasure + '</p>';
    }

    $scope.showReport = function() {
      if (UDAHSQuerySvc.query.reportType === 'topic' && UDAHSQuerySvc.query.topic.measure) {
        return true;
      }
      else if (UDAHSQuerySvc.query.reportType === 'county' && !_.isEmpty(UDAHSQuerySvc.query.county.topics)) {
        return true;
      }

      return false;
    };

    $scope.showTabs = function() {
      return UDAHSQuerySvc.query.topic.measure;
    };

    $scope.goToTable = function() {
      var sp = UDAHSQuerySvc.toStateParams();
      sp.displayType = 'table';
      $state.go('top.professional.usage-data.avoidable-stays', sp);
      console.log(sp);
    };

    $scope.goToMap = function() {
      var sp = UDAHSQuerySvc.toStateParams();
      sp.displayType = 'map';
      $state.go('top.professional.usage-data.avoidable-stays', sp);
    };

    $scope.isActiveTab = function(name) {
      return $scope.query.displayType === name;
    };

    function currentConditionObj() {
      var condition = $scope.query.topic.topic;

      return _.findWhere(ahsTopics, {id: condition});
    }

     function currentCondition() {
      var conditionObj = currentConditionObj();

      if (conditionObj) {
        return conditionObj.name;
      }
    };

    function currentMeasure() {
      var conditionObj = currentConditionObj(),
        measure = UDAHSQuerySvc.query.topic.measure,
        measureObj;

      if (measure) {
        measureObj = _.findWhere(conditionObj.measures, {id: measure})
      }

      if (measureObj) {
        return measureObj.name;
      }
    }
  }

})();

