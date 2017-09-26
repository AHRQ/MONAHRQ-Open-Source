/**
 * Monahrq Nest
 * Core Domain Module
 * Menu Service
 *
 * This service stores the navigation menu used in the app header and footer.
 * The menu is loaded during Angular bootstrap; menu sections for each flutter is
 * then merged in during flutter plugin initialization.
 *
 * Each menu item is a JS object, so the search options is a simple key:value equality
 * check.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('MenuSvc', MenuSvc);


  MenuSvc.$inject = [];
  function MenuSvc() {
    var menu = [];

    return {
      PRODUCT_CONSUMER: 'consumer',
      PRODUCT_PROFESSIONAL: 'professional',
      TYPE_STANDARD: 'standard',
      TYPE_FLUTTER: 'flutter',
      TYPE_CONFIG: 'menu-config',
      MENU_MAIN: 'main',

      search: search,
      set: set,
      addFlutter: addFlutter
    };

    function search(options) {
      return _.where(menu, options);
    }

    function set(_menu) {
      menu = menu.concat(_menu);
    }

    function addFlutter(flutter) {
      menu = menu.concat(flutter.menuItems);
    }

  }

})();
