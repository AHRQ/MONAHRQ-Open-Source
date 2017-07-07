/**
 * Monahrq Nest
 * Components Module
 * Flutter Menu Item Directive
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.menu')
    .directive('mhFlutterMenuItem', flutterMenuItem);

  function flutterMenuItem() {
    /**
     * Directive Definition
     */
    return {
      restrict: 'A',
      replace: true,
      scope: {
        menu: '=mhFlutterMenuItem'
      },
      templateUrl: 'app/components/menu/views/flutter_menu_item.html',
      controller: controller
    };

    /**
     * Directive Controller
     */
    function controller($scope, $state) {
      var menu = $scope.menu;

      $scope.menuClick = menuClick;
      $scope.isActive = isActive;
      $scope.classes = menu.classes && menu.classes.length > 0 ? menu.classes.join(' ') : '';

      function isActive() {
        return $state.includes(menu.route.name, {reportId: menu.reportId});
      }

      function menuClick() {
        $state.go(menu.route.name, {
          reportId: menu.reportId
        });
      }
    }
  }

})();
