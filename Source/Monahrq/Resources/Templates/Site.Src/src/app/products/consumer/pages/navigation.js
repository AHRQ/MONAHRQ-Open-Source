/**
 * Consumer Product
 * Pages Module
 * Navigation Controller
 *
 * This controller generates the top-level navigational menu for the consumer website.
 */
(function() {
'use strict';


/**
 * Angular wiring
 */
angular.module('monahrq.products.consumer.pages')
    .controller('CNavigationCtrl', CNavigationCtrl);


  CNavigationCtrl.$inject = ['$rootScope', 'MenuSvc'];
  function CNavigationCtrl($rootScope, MenuSvc) {
    $rootScope.consumerMenu = [];

    init();

    function init() {
      var menu = MenuSvc.search({
        'product': MenuSvc.PRODUCT_CONSUMER,
        'menu': MenuSvc.MENU_MAIN
      });

      $rootScope.consumerMenu = _.filter(menu, function (item) {
        if (item.entity === null) {
          return true;
        }
        return $rootScope.ConsumerReportConfigSvc.hasEntity(item.entity);
      });
    }
  }

})();
