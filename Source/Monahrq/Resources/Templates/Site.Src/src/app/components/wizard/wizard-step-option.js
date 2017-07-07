/**
 * Monahrq Nest
 * Components Module
 * Wizard Step Options Directive
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.wizard')
    .directive('mhWizardStepOption', wizardStepOption);


  function wizardStepOption() {
    var directive = {
      restrict: 'EA',
      replace: true,
      transclude: true,
      require: '^mhWizardStep',
      scope: {
        optionSwitchOn: '=',
        optionLabel: '='
      },
      templateUrl: 'app/components/wizard/views/wizard-step-option.html',
      link: link,
      controller: controller,
      controllerAs: 'vm'
    };
    return directive;


    function link(scope, elem, attrs, parentController) {
      scope.isVisible = isVisible;

      scope.$watch(parentController.getOptionSwitch, function(nv, ov) {
        if (nv === undefined || nv == scope.optionSwitchOn) {
          parentController.setOptionLabel(scope.optionLabel);
        }
      });

      function isVisible() {
        var switchVal = parentController.getOptionSwitch();
        return switchVal === undefined || switchVal == scope.optionSwitchOn;
      }
    }

    controller.$inject = ['$scope'];
    function controller($scope) {
      var vm = this;

      init();

      function init() {
      }
    }
  }

})();