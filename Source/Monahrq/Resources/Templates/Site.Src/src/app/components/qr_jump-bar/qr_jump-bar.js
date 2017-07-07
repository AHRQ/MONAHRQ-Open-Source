/**
 * Monahrq Nest
 * Components Module
 * Quality Ratings Jump Bar Directive
 *
 * Legacy directive providing navigation via dropdowns.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.qr-jump-bar', [])
    .directive('qrJumpBar', qrJumpBar);

  function qrJumpBar() {
    /**
     * Directive Definition
     */
    return {
      restrict: 'A',
      replace: true,
      scope: {
        hospitals: '=',
        topics: '='
      },
      templateUrl: 'app/components/qr_jump-bar/views/qr_jump-bar.html',
      controller: controller
    };

    /**
     * Directive Controller
     */
    function controller($scope, $state) {
      $scope.gotoHospital = function() {
        $state.go('top.quality-ratings.profile', {
          id: $scope.hospitalId
        });
      };

      $scope.gotoTopic = function() {
        $state.go('top.quality-ratings.condition', {
          topic: $scope.topicId
        });
      };
    }
  }
})();

