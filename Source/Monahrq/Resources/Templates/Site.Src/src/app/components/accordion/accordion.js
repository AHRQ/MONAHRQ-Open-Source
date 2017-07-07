/**
 * Monahrq Nest
 * Components Module
 * Accordion and Panel Directives
 *
 * The accordion widget allows the user to toggle the visibility of a section of content.
 * It does not implement mutually exclusive sections.
 *
 * <div data-mh-accordion>
 *   <div data-mh-accordion-toggle="'id1'" data-mh-accordion-label="'Title'"></div>
 *   <div data-mh-accordion-panel="'id1'"></div>
 *   <div data-mh-accordion-toggle="'id2'" data-mh-accordion-label="'Title'"></div>
 *   <div data-mh-accordion-panel="'id2'"></div>
 * </div>
 */
(function() {
'use strict';

angular.module('monahrq.components.accordion', [])
  .directive('mhAccordion', accordion)
  .directive('mhAccordionToggle', accordionToggle)
  .directive('mhAccordionPanel', accordionPanel);

function cleanName(name) {
  if (_.isString(name) && !_.isEmpty(name)) {
    var re = /\s+/g;
    return name.replace(re, '_');
  }

  return name;
}

// TODO: add option for true accordion behavior (only one panel open at a time)
function accordion() {
  Controller.$inject = ['$scope'];

  var directive = {
    restrict: 'EA',
    scope: {
      api: '=mhAccordionApi'
    },
    link: link,
    controller: Controller
  };
  return directive;

  function link(scope, elem, attrs) {
  }

  function Controller($scope) {
    var items = {};
    var that = this;
    this.isActive = isActive;
    this.setActive = setActive;
    this.internalapi = $scope.api || {};
    this.internalapi.open = open;


    function open(name) {
      items[name] = true;
    }

    function isActive(item) {
      return _.has(items, item) && items[item] === true;
    }

    function setActive(item) {
      //items = {};
      if (!_.has(items, item)) {
        items[item] = false;
      }
      items[item] = !items[item];
    }
  }
}

function accordionToggle() {
  Controller.$inject = ['$scope'];

  var directive = {
    restrict: 'EA',
    require: '^mhAccordion',
    replace: true,
    scope: {
      _name: '=mhAccordionToggle',
      label: '=mhAccordionLabel'
    },
    templateUrl: 'app/components/accordion/views/accordionToggle.tpl.html',
    link: link,
    controller: Controller
  };
  return directive;

  function link(scope, elem, attrs, parentController) {
    scope.isActive = parentController.isActive;
    scope.setActive = parentController.setActive;
    scope.name = cleanName(scope._name);
  }

  function Controller($scope) {
  }
}

//TODO: need way of allowing use of tbody vs div for panel
function accordionPanel() {
  Controller.$inject = ['$scope'];

  var directive = {
    restrict: 'EA',
    require: '^mhAccordion',
    replace: true,
    transclude: true,
    scope: {
      _name: '=mhAccordionPanel'
    },
    templateUrl: 'app/components/accordion/views/accordionPanel.tpl.html',
    link: link,
    controller: Controller
  };
  return directive;

  function link(scope, elem, attrs, parentController) {
    scope.isActive = parentController.isActive;
    scope.setActive = parentController.setActive;
    scope.name = cleanName(scope._name);
  }

  function Controller($scope) {
  }
}

})();
