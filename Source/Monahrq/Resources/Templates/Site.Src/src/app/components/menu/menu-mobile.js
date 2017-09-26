/**
 * Monahrq Nest
 * Components Module
 * Menu Directive
 *
 * Render the provided menu data. The format of the data is found in the Menu base file.
 *
 * <div data-mh-menu="consumerMenu" data-nav-class="'nav--header'"></div>
 */
(function () {
    'use strict';

    /**
     * Angular wiring
     */
    angular.module('monahrq.components.menu')
      .directive('mhMenu', menu);

    menu.$inject = ['$compile', 'SortSvc'];
    function menu($compile, SortSvc) {
        var childTpl;

        init();

        /**
         * Directive Definition
         */
        return {
            restrict: 'A',
            scope: {
                menu: '=mhMenu',
                navClass: '=navClass',
                navDepth: '=?navDepth',
                navIgnoreClasses: '=?navIgnoreClasses'
            },
            replace: true,
            link: link,
            controller: controller
        };

        function init() {
            childTpl = _.template('<li data-ng-class="{active: navActive([<%=activeName%>], [<%=ignoreName%>])}">' +
             '<a href="" class="nav__link" data-ng-class="{active: navActive([<%=activeName%>], [<%=ignoreName%>])}" data-ui-sref="<%=routeName%>(<%=params%>)">' +
             '<span><%=label%></span>' +
             '</a>');
        }

        function getNamesParam(name) {
            return _.isArray(name)
              ? _.map(name, function (a) { return '\'' + a + '\''; }).join(',')
              : '\'' + name + '\'';
        }

        function makeChild(data) {

            var params = {
                label: data.label.replace('<br>', ' '),
                routeName: data.route.name,
                activeName: getNamesParam(data.route.activeName),
                ignoreName: getNamesParam(data.route.ignoreName),
                params: _.size(data.route.params) > 0 ? JSON.stringify(data.route.params).replace(/\"/g, '\'') : ''
            };

            var el = angular.element(childTpl(params));

            return el;
        }

        function hasChildren(menuItems, item) {
            return _.any(menuItems, function (mi) {
                return mi.parent === item.id;
            });
        }

        function getChildren(menuItems, parent, navDepth) {
            var results = [];
            var cs = _.filter(menuItems, function (item) {
                if (item.parent == parent && (item.type == 'standard' || item.type == 'flutter')) {
                    return true;
                }
                return false;
            });

            SortSvc.objSortNumeric(cs, 'priority', 'asc');

            _.each(cs, function (c) {
                var cEl = makeChild(c);

                if (hasChildren(menuItems, c) && (navDepth > 1 || navDepth == undefined)) {
                    var gcs = getChildren(menuItems, c.id);
                    var ul = angular.element('<ul></ul>');
                    ul.attr('data-ng-show', 'navActive([' + getNamesParam(c.route.activeName) + '], [' + getNamesParam(c.route.ignoreName) + '])');
                    addMenuClasses(menuItems, c.id, ul);
                    ul.append(gcs);
                    cEl.append(ul);
                    cEl.addClass('has-children');
                }

                results.push(cEl);
            });

            return results;
        }

        function getMenuConfig(menuItems, target) {
            var mc = _.findWhere(menuItems, { target: target, type: 'menu-config' });
            return mc;
        }

        function addMenuClasses(menuItems, target, el) {
            var menuConfig = getMenuConfig(menuItems, target);
            if (menuConfig && menuConfig.classes) {
                _.each(menuConfig.classes, function (c) {
                    el.addClass(c);
                });
            }
        }

        function updateActive(elem) {
            setTimeout(function () {
                var maxDepth = elem.find('ul:visible').length;

                elem.find('>nav')
                  .removeClass(function (index, className) {
                      return (className.match(/(^|\s)nav-depth-\S+/g) || []).join(' ');
                  })
                  .addClass('nav-depth-' + maxDepth);
            });
        }

        function link(scope, elem, attrs) {
            var $root = angular.element('<nav role="navigation" aria-label="Site Navigation"><ul class="nav navbar-nav"></ul></nav>');
            
            var $rootUl = $root.find('ul');

            if (scope.navClass) {
                $root.addClass(scope.navClass);
            }
            if (scope.navIgnoreClasses == undefined) {
                addMenuClasses(scope.menu, null, $rootUl);
            }

            var children = getChildren(scope.menu, null, scope.navDepth);
            if (children.length > 0) {
                $rootUl.append(children);
            }

            var content = $compile($root)(scope);
            elem.append(content);

            updateActive(elem);
            scope.$on('$stateChangeSuccess', function () {
                updateActive(elem);
            });
        }

        /**
         * Directive Controller
         */
        function controller($scope, $state, SortSvc) {
            $scope.navActive = navActive;

            function navActive(states, ignores) {
                return _.any(states, function (state) {
                    if (ignores && _.any(ignores, function (ignore) { return $state.includes(ignore); })) {
                        return false;
                    }
                    else {
                        return $state.includes(state);
                    }
                });

            }
        }
    }

})();
