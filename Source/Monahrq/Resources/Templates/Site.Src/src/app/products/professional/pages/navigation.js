/**
 * Professional Product
 * Pages Module
 * Navigation Block
 *
 * This controller generates the top-level navigational menu for the professional website.
 */
(function () {
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
            var flutterMenu = _.filter(MenuSvc.search({
                'product': MenuSvc.PRODUCT_PROFESSIONAL,
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
                    'product': MenuSvc.PRODUCT_PROFESSIONAL,
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

