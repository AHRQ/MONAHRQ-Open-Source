/**
 * Monahrq Nest
 * Components Module
 * Wizard Step Directive
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.wizard')
    .directive('mhWizardStep', wizardStep);


  function wizardStep() {
    var directive = {
      restrict: 'EA',
      replace: true,
      transclude: true,
      require: '^mhWizard',
      scope: {
        stepNum: '=',
        stepLabel: '=',
        stepVisible: '=',
        stepOptionSwitch: '='
      },
      templateUrl: 'app/components/wizard/views/wizard-step.html',
      link: link,
      controller: controller,
      controllerAs: 'vm'
    };
    return directive;


    function link(scope, elem, attrs, parentController) {
      scope.getActiveStep = parentController.getActiveStep;
      scope.setActiveStep = parentController.setActiveStep;
    }

    controller.$inject = ['$scope'];
    function controller($scope) {
      var vm = this;
      vm.isVisible = isVisible;
      vm.isActive = isActive;
      vm.getOptionSwitch = getOptionSwitch;
      vm.setOptionLabel = setOptionLabel;
      $scope.optionLabel = null;

      init();

      function init() {
        $scope.$watch('stepVisible', updateActive);
      }

      function isVisible() {
        return $scope.stepVisible;
      }

      function updateActive(nv, ov) {
        if (nv) {
          $scope.setActiveStep($scope.stepNum);
        }
      }

      function isActive() {
        return $scope.getActiveStep() == $scope.stepNum;
      }

      function getOptionSwitch() {
        return $scope.stepOptionSwitch;
      }

      function setOptionLabel(label) {
        $scope.optionLabel = label;
      }
    }
  }

})();