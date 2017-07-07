/**
 * Monahrq Nest
 * Components Module
 * Rating RAR Directive
 *
 * Render the risk-adjusted rate and confidence interval for the provided measure.
 *
 * <span mh-rating-rar="measure"></span>
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.rating-rar', [])
    .directive('mhRatingRar', ratingRar);


  function ratingRar() {
    /**
     * Directive Definition
     */
    return {
      restrict: 'A',
      scope: {
        measure: '=mhRatingRar'
      },
      templateUrl: 'app/components/rating_rar/views/rating_rar.html',
      controller: controller
    };

    /**
     * Directive Definition
     */
    function controller($scope) {
      $scope.hasData = function() {
        return $scope.measure
        && $scope.measure.riskAdjustedRate
        && $scope.measure.riskAdjustedCILow
        && $scope.measure.riskAdjustedCIHigh;
      }
    }
  }

})();

