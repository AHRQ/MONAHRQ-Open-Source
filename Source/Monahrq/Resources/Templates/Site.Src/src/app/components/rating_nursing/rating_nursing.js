/**
 * Monahrq Nest
 * Components Module
 * Rating Nursing Directive
 *
 * Render star ratings for nursing homes.
 *
 * <span data-mh-rating-nursing="row[col.id].NatRating"></span>
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.rating-nursing', [])
    .directive('mhRatingNursing', ratingNursing);


  function ratingNursing() {
    /**
     * Directive Definition
     */
    return {
      restrict: 'A',
      scope: {
        mhRatingNursing: '='
      },
      templateUrl: 'app/components/rating_nursing/views/rating_nursing.html',
      controller: controller
    };

    function controller($scope) {
      var filename = "themes/base/assets/stars_", ext = ".png";
      var titles = {
        5: ['Much better', 'than average'],
        4: ['Better', 'than average'],
        3: ['Average', ' '],
        2: ['Below', 'average'],
        1: ['Much below', 'average']
      };

      $scope.$watch('mhRatingNursing', updateRating);

      function updateRating() {
        var rating = +$scope.mhRatingNursing;
        if (rating >= 1 && rating <= 5) {
          $scope.imageURL = filename + rating + ext;
          $scope.title = titles[rating];
        }
        else {
          $scope.imageURL = undefined;
          $scope.title = "-";
        }
      }
    }
  }

})();

