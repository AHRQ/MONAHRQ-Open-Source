/**
 * Professional Product
 * Pages Module
 * Navigation Block
 *
 * This controller generates the top-level navigational menu for the professional website.
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.pages')
    .controller('NavigationCtrl', NavigationCtrl);

  NavigationCtrl.$inject = ['$rootScope', 'MenuSvc'];
  function NavigationCtrl($rootScope, MenuSvc) {
    $rootScope.professionalMenu = [];

    init();

    function init() {
      var menu = MenuSvc.search({
        'product': MenuSvc.PRODUCT_PROFESSIONAL,
        'menu': MenuSvc.MENU_MAIN
      });

      $rootScope.professionalMenu = _.filter(menu, function (item) {
        if (item.entity === null) {
          return true;
        }

        return $rootScope.ReportConfigSvc.hasEntity(item.entity);
      });

    }

  }
})();

