/**
 * Monahrq Nest
 * Components Module
 * Help Icon Directive
 *
 * Render an accessible, clickable help icon
 *
 * <span data-mh-help-icon data-sr-label="'Help for Something'" data-mh-on-click="modalTopic(topicId)"></span>
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.help-icon', [])
    .directive('mhHelpIcon', helpIcon);

  function helpIcon() {
    /**
     * Directive Definition
     */
    return {
      restrict: 'A',
      scope: {
        label: '=?mhHelpIcon',
        srLabel: '=?',
        mhOnClick: '&',
        mhIsVisible: '&'
      },
      templateUrl: 'app/components/help_icon/views/help_icon_mobile.html',
      controller: controller
    };

    /**
     * Directive Controller
     */
    function controller($scope) {
      if (!$scope.label) $scope.label = '';
      if (!$scope.srLabel) $scope.srLabel = 'Open Help';

      $scope.isVisible = function() {
        if (_.isFunction($scope.mhIsVisible) && $scope.mhIsVisible() !== undefined) {
          return $scope.mhIsVisible();
        }

        return true;
      }
    }
  }

})();

