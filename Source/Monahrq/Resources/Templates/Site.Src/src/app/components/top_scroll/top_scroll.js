/**
 * Monahrq Nest
 * Components Module
 * Top Scroll Directive
 *
 * Legacy directive. Mirror the bottom horizontal scrollbar with a virtual bar on the top
 * of the scrollable pane.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.top-scroll', [])
    .directive('mhTopScroll', topScroll);

  function topScroll() {
    /**
     * Directive Definition
     */
    return {
      restrict: 'A',
      scope: {
        targetId: '=mhTopScroll'
      },
      template: '<div class="top-scroll" style="height:20px;"></div>',
      controller: controller
    };

    /**
     * Directive Controller
     */
    function controller($scope, $element) {
      $scope.$watch(
        function () {
          var target = $($scope.targetId);
          return target[0].childNodes.length;
        },
        function (newValue, oldValue) {
          if (newValue !== oldValue) {
            var target = $($scope.targetId);
            var width = Math.max.apply(Math, target.children().map(function(){ return $(this).width(); }).get());

            $element.css({
              'overflow-x': 'auto',
              'overflow-y': 'hidden',
              'width': '100%'
            });

            $element.find('.top-scroll').css({
              'width': width + 'px'
            });

            $element.scroll(function() {
              target.scrollLeft($element.scrollLeft());
            });

            target.scroll(function() {
              $element.scrollLeft(target.scrollLeft());
            });
          }
        }
      );
    }
  }

})();

