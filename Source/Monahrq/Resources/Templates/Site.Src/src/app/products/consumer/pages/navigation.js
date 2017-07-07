/**
 * Consumer Product
 * Pages Module
 * Navigation Controller
 *
 * This controller generates the top-level navigational menu for the consumer website.
 */
(function () {
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
            var flutterMenu = _.filter(MenuSvc.search({
                'product': MenuSvc.PRODUCT_CONSUMER,
                'type': MenuSvc.TYPE_FLUTTER}),
                    function (item) {
                        if (item.parent != null) {
                            return true;
                        }
                    }
                );

            var load = false;

            _.each(flutterMenu, function (item) {
                var fluterParent = MenuSvc.search({
                    'product': MenuSvc.PRODUCT_CONSUMER,
                    'type': MenuSvc.TYPE_FLUTTER,
                    'id': item.parent
                });

                if (fluterParent.length == 0) {
                    load = true;
                }
            });

            if (load) {
                MenuSvc.addMainFlutter(flutterMenu).then(function () {
                    loadNav();
                });
            } else {
                loadNav();
            }
        }

        function loadNav() {
            var menu = MenuSvc.search({
                'product': MenuSvc.PRODUCT_CONSUMER,
                'menu': MenuSvc.MENU_MAIN
            });

            $rootScope.consumerMenu = _.filter(menu, function (item) {
                if (item.type == "flutter" && item.parent != null) {
                    return false;
                } else {
                    if (item.entity === null) {
                        return true;
                    }
                    return $rootScope.ConsumerReportConfigSvc.hasEntity(item.entity);
                }
            });

        }

    }

})();
