/**
 * Monahrq Nest
 * Components Module
 * Rating Star Hospital Directive
 *
 * Render star ratings for hospitals.
 *
 * <span data-mh-rating-star-hosp="row[col.id].NatRating"></span>
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.rating-star-hosp', [])
    .directive('mhRatingStarHosp', ratingStarHosp);


  function ratingStarHosp() {
    /**
     * Directive Definition
     */
    return {
      restrict: 'A',
      scope: {
        mhRatingStarHosp: '='
      },
      templateUrl: 'app/components/rating_star_hosp/views/rating_star_hosp.html',
      controller: controller
    };

    function controller($scope) {
      var filename = "themes/base/assets/stars_", ext = ".png";

      $scope.$watch('mhRatingStarHosp', updateRating);

      function updateRating() {
        var rating = +$scope.mhRatingStarHosp;
        if (rating >= 1 && rating <= 5) {
          $scope.imageURL = filename + rating + ext;
          $scope.title = titles[rating];
          $scope.hasNoData = false;
        }
        else {
          $scope.imageURL = undefined;
          $scope.hasNoData = true;
        }
      }
    }
  }

})();

