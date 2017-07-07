/**
 * Professional Product
 * Pages Module
 * Flutters Page
 *
 * Controller for the flutters landing page. This displays the section of the navigation for
 * flutter items.
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.pages')
    .controller('FluttersCtrl', FluttersCtrl);

  FluttersCtrl.$inject = ['$scope', '$state', 'FlutterConfigSvc'];
  function FluttersCtrl($scope, $state, FlutterConfigSvc) {
    $scope.menuClick = menuClick;
    $scope.getPrimaryMenu = getPrimaryMenu;
    $scope.flutters = _.filter(FlutterConfigSvc.getAll(), function (flutter) {
        var professional_menu = _.filter(flutter.menuItems, function (item) {
            if (item.product === "professional") {
                return true;
            }
        });

        if (professional_menu.length > 0) {
            return true;
        }

    });

    function getPrimaryMenu(flutterId) {
      var flutter = _.findWhere($scope.flutters, {id: flutterId});
      var menu = _.findWhere(flutter.menuItems, { primary: true, product: "professional" });
      return menu;
    }

    function menuClick(menu) {
      $state.go(menu.route.name, {
        reportId: menu.reportId
      });
    }
  }


})();


