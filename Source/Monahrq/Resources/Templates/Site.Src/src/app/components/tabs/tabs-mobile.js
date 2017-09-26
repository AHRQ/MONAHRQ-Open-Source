/**
 * Monahrq Nest
 * Components Module
 * Tab and Panel Directives
 *
 * Accessible tab widget
 *
 *
 * <div data-mh-tabs="'section1'" data-mh-tabs-api="vm.tabAPI">
 *   <div data-mh-tab-list>
 *     <div data-mh-tab="'section1'" data-mh-tab-label="'Section 1'"></div>
 *     <div data-mh-tab="'section1'" data-mh-tab-label="'Section 2'"></div>
 *   </div>
 *   <div data-mh-tab-panel="'section1'">content</div>
 *   <div data-mh-tab-panel="'section2'">content</div>
 * </div>
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.tabs', [])
    .directive('mhTabs', tabs)
    .directive('mhTabList', tabList)
    .directive('mhTab', tab)
    .directive('mhTabPanel', tabPanel);

   function tabs() {
    var directive = {
      restrict: 'EA',
      replace: true,
      scope: {
        defaultTab: '=mhTabs',
        api: '=mhTabsApi'
      },
      link: link,
      controller: controller
    };
    return directive;


    function link(scope, elem, attrs) {
    }

    controller.$inject = ['$scope'];
    function controller($scope) {
      var tabs = {};
      this.isActiveTab = isActiveTab;
      this.setActiveTab = setActiveTab;
      this.getTabIndex = getTabIndex;

      $scope.api = {
        setActiveTab: setActiveTab,
        getActiveTab: getActiveTab
      };

      if ($scope.defaultTab) {
        setActiveTab($scope.defaultTab);
      }

      function isActiveTab(tab) {
        return _.has(tabs, tab);
      }

      function getActiveTab() {
        return _.first(_.keys(tabs));
      }

      function setActiveTab(tab) {
        tabs = {};
        tabs[tab] = 1;
      }

      function getTabIndex(tab) {
        if (isActiveTab(tab)) {
          return '0';
        }

        return '-1';
      }
    }
  }

  function tabList() {
    var directive = {
      restrict: 'EA',
      replace: true,
      transclude: true,
      scope: {
      },
      template: '<ul role="tablist" class="nav nav-tabs" data-ng-transclude></ul>'
    };
    return directive;
  }

  function tab() {
    var directive = {
      restrict: 'EA',
      require: '^mhTabs',
      replace: true,
      scope: {
        name: '=mhTab',
        label: '=mhTabLabel'
      },
      template: '<li class="tab" role="presentation" data-ng-class="{active: isActiveTab(name)}"><a id="tab-{{name}}" role="tab" aria-controls="panel-{{name}}" aria-selected="{{isActiveTab(name)}}" tabindex="{{getTabIndex(name)}}" href="#" data-ng-click="setActiveTab(name); $event.preventDefault()" data-ng-keydown="key($event)">{{label}}</a></li>',
      link: link,
      controller: controller
    };
    return directive;


    function link(scope, elem, attrs, parentController) {
      scope.isActiveTab = parentController.isActiveTab;
      scope.setActiveTab = parentController.setActiveTab;
      scope.getTabIndex = parentController.getTabIndex;
    }

    controller.$inject = ['$scope'];
    function controller($scope) {
      $scope.key = key;

      function key($event) {
        if ($event.keyCode === 37 || $event.keyCode === 38) { // left up
          $event.preventDefault();
          prevTab($event.currentTarget);
        }
        else if ($event.keyCode === 39 || $event.keyCode == 40) {  // right down
          $event.preventDefault();
          nextTab($event.currentTarget);
        }
      }

      function prevTab(el) {
        var $el = jQuery(el).parent();
        var $tabs = $el.parent().children();
        $tabs.eq(($tabs.index($el) - 1) % $tabs.length)
          .find('a')
          .focus();
      }

      function nextTab(el) {
        var $el = jQuery(el).parent();
        var $tabs = $el.parent().children();
        $tabs.eq(($tabs.index($el) + 1) % $tabs.length)
          .find('a')
          .focus();
      }
    }
  }

  function tabPanel() {
    var directive = {
      restrict: 'EA',
      require: '^mhTabs',
      replace: true,
      transclude: true,
      scope: {
        name: '=mhTabPanel'
      },
      template: '<div id="panel-{{name}}" class="tab-pane" role="tabpanel" aria-labelledby="tab-{{name}}" aria-hidden="{{!isActiveTab(name)}}" aria-expanded="{{isActiveTab(name)}}" data-ng-show="isActiveTab(name)" data-ng-transclude></div>',

      link: link,
      controller: controller
    };
    return directive;


    function link(scope, elem, attrs, parentController) {
      scope.isActiveTab = parentController.isActiveTab;
      scope.setActiveTab = parentController.setActiveTab;
    }

    controller.$inject = ['$scope'];
    function controller($scope) {
    }
  }
})();
