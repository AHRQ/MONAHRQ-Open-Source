/**
 * Monahrq Nest
 * Components Module
 * Compare Direction Directive
 *
 * Output a visual arrow and label indicating whether higher or lower scores are better.
 *
 * <span data-mh-compare-direction data-higher-scores-are-better="true"></span>
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.compare-direction', [])
    .directive('mhCompareDirection', compareDirection);

  function compareDirection() {
    /**
     * Directive Definition
     */
    return {
      restrict: 'A',
      scope: {
        higherScoresAreBetter: '='
      },
      templateUrl: 'app/components/compare_direction/views/compare_direction.html'
    };
  }

})();


