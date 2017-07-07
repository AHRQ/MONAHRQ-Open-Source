/**
 * Monahrq Nest
 * Components Module
 * Flutter Menu Directive
 *
 * Legacy directive to render a flutter's provided menu items. This has been superseded by
 * the Menu directive, which is able to render both application and flutter menus.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.menu')
    .directive('mhFlutterMenu', flutterMenu);

  function flutterMenu() {
    /**
     * Directive Definition
     */
    return {
      restrict: 'A',
      scope: {
        flutters: '=mhFlutterMenu',
        menuId: '=menuId'
      },
      templateUrl: 'app/components/menu/views/flutter_menu.html',
      controller: controller
    };

    /**
     * Directive Controller
     */
    function controller($scope, $state, SortSvc) {
      $scope.items = [];

      buildMenu();

      function buildMenu() {
        var items = _.flatten(_.pluck($scope.flutters, 'menuItems'));
        items = _.where(items, {menu: $scope.menuId});
        SortSvc.objSortNumeric(items, 'priority', 'asc');
        $scope.items = items;
      }
    }
  }

})();
