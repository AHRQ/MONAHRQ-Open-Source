/**
 * Monahrq Nest
 * Components Module
 * Rating Symbol Directive
 *
 * Render the symbol corresponding to a measure's 0-3 rating.
 *
 * <span mh-rating-symbol="rating"></span>
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.rating-symbol', [])
    .directive('mhRatingSymbol', ratingSymbol);


  function ratingSymbol() {
    /**
     * Directive Definition
     */
    return {
      restrict: 'A',
      scope: {
        mhRatingSymbol: '='
      },
      templateUrl: 'app/components/rating_symbol/views/rating_symbol.html'
    };
  }

})();

