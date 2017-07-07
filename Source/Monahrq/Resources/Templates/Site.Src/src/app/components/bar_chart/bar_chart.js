/**
 * Monahrq Nest
 * Components Module
 * Bar Chart Directive
 *
 * value: bar width proportion, <0-100>
 * average: quality rating, affects color and labeling, <0-4>
 * <div data-bar-chart="value" label="'foo'" data-average="average"></div>
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.bar-chart', [])
    .directive('barChart', barChart);

  function barChart() {
    /**
     * Private Data
     */
    var color = {
      aboveAvg: '#00a178', //green
      belowAvg: '#ffdd76', //yellow
      atAvg: '#00688c', //blue
      border: '#01698e',
      background: '#ffffff'
    },
    fillMap = {
      aboveAvg: 'url(#aboveAvgLine)',
      belowAvg: 'url(#belowAvgLine)',
      atAvg: 'url(#averageLine)'
    },
    labelMap = {
      missing: 'Missing',
      aboveAvg: 'Above Average',
      belowAvg: 'Below Average',
      atAvg: 'Average'
    },
    ratingMap = {
      0: 'missing',
      1: 'aboveAvg',
      2: 'atAvg',
      3: 'belowAvg'
    };

    /**
     * Directive Definition
     */
    return {
      restrict: 'A',
      scope: {
        value: '=barChart',
        average: '=',
        label: '='
      },
      templateUrl: 'app/components/bar_chart/views/bar_chart.html',
      link: link
    };

    /**
     * Directive Link
     */
    function link(scope, element, attrs) {
      scope.$watch('bar-chart', update);
      scope.$watch('average', update);
      function update() {
        updateChart(scope);
      }
    }

    function updateChart(scope) {
      scope.graph = {
        width:100,
        height:20
      };

      scope.wrapper = {
        x:1,
        y:1,
        width:scope.graph.width,
        height:scope.graph.height,
        fill: color.background,
        stroke: color.border,
        strokeWidth:1,
        label: getLabel(scope)
      };

      scope.bar = {
        x:scope.wrapper.x+scope.wrapper.strokeWidth,
        y:scope.wrapper.y+scope.wrapper.strokeWidth,
        width:getBarWidth(scope),
        height:scope.wrapper.height - scope.wrapper.strokeWidth * 2,
        fill: getFill(scope),
        stroke: getFill(scope),
        pattern: getFillPattern(scope),
        strokeWidth:0
      };

      scope.showBar = function() {
        return _.contains(['1', '2', '3'], scope.average);
      };

      scope.showLabel = function() {
        return scope.label && scope.average != '0' && scope.label != '-';
      };

      scope.showNoData = function() {
        return scope.average == '0' || scope.label == '-';
      };
    }
    function getBarWidth(scope) {
      var w = scope.value / 100.0 * (scope.wrapper.width - scope.wrapper.strokeWidth * 2);
      return w || 0;
    }

    function getFill(scope) {
      return color[ratingMap[scope.average]];
    }

    function getFillPattern(scope) {
      return fillMap[ratingMap[scope.average]];
    }

    function getLabel(scope){
      return labelMap[ratingMap[scope.average]];
    }

  }

})();

