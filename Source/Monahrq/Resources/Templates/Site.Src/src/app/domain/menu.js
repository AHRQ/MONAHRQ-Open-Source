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
(function () {
    'use strict';

    /**
     * Angular wiring
     */
    angular.module('monahrq.domain')
      .factory('MenuSvc', MenuSvc);


    MenuSvc.$inject = ['$q', 'DataLoaderSvc'];
    function MenuSvc($q, DataLoaderSvc) {
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
            addFlutter: addFlutter,
            addMainFlutter: addMainFlutter
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

        function addMainFlutter(fluterItem) {
            var deferred, url;
            deferred = $q.defer();

            url = "flutters/flutter-main-menu.js";

            DataLoaderSvc.loadScript(url, function () {
                var data = $.monahrq.Flutter.MainMenu;

                if (_(data).isUndefined()) {
                    return deferred.reject('Unable to load \"' + url + '\".');
                } else {
                    _.each(fluterItem, function (item) {
                        var flutterMain = search({
                            'id': item.parent,
                            'product': item.product,
                            'type': 'flutter'
                        });

                        if (item.product == "professional" && flutterMain.length == 0) {
                            var menuItem = data.menuItems.professional;
                            menuItem.id = item.parent;
                            set(menuItem);
                        }

                        if (item.product == "consumer" && flutterMain.length == 0) {
                            var menuItem = data.menuItems.consumer;
                            menuItem.id = item.parent;
                            set(menuItem);
                        }
                    });


                    return deferred.resolve(data);
                }

            }, function () {
            }, true);

            return deferred.promise;
        }


    }

})();
