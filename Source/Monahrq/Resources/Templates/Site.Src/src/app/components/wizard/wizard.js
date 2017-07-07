/**
 * Monahrq Nest
 * Components Module
 * Wizard Directive
 *
 * A directive for building multi-step forms, where selections on one step affect options available
 * on the following step.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.wizard', [])
    .directive('mhWizard', wizard);


  function wizard() {
    var directive = {
      restrict: 'EA',
      replace: true,
      transclude: true,
      scope: {
      },
      templateUrl: 'app/components/wizard/views/wizard.html',
      link: link,
      controller: controller,
      controllerAs: 'vm'
    };
    return directive;


    function link(scope, elem, attrs) {
    }

    controller.$inject = ['$scope'];
    function controller($scope) {
      var activeStep;
      var vm = this;

      vm.getActiveStep = getActiveStep;
      vm.setActiveStep = setActiveStep;

      init();

      function init() {
      }

      function setActiveStep(step) {
        activeStep = step;
      }

      function getActiveStep() {
        return activeStep;
      }

    }
  }

})();
