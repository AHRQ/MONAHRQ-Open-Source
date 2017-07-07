/**
 * Monahrq Nest
 * Components Modals Module
 * QR Legend Modal
 *
 * Open a model providing help for quality ratings icons.
 *   svc.open(reportId, displayType, childConfig);
 *
 * reportId is the report page the modal is opening on. It is currently used to
 * load the interpretation settings from the report config.
 *
 * displayType is one of symbols, symbols_rar, or bar_charts, and affects which legend is shown.
 *
 * childConfig is a config object which supersedes the report's reportconfig object when preparing
 * the interpretation settings. The ReportConfig supports nested config sections, the caller would
 * select which they wish to use, if any.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.modals')
    .factory('ModalLegendSvc', ModalLegendSvc);

  ModalLegendSvc.$inject = ['$modal', '$state', '$rootScope', 'ReportConfigSvc', 'ConsumerReportConfigSvc'];
  function ModalLegendSvc($modal, $state, $rootScope, ReportConfigSvc, ConsumerReportConfigSvc) {
    /**
     * Private Data
     */
    var modalInstance;
    var activeEl;

    /**
     * Service Interface
     */
    return {
      open: open
    };

    /**
     * Service Implementation
     */
    function isProfessional() {
      return $state.includes('top.professional');
    }

    function open(reportId, displayType, childConfig) {
      saveFocus();
      $rootScope.modalOpen = true;

      modalInstance = $modal.open({
        templateUrl: 'app/components/modals/views/modal_legend.html',
        windowTemplateUrl: 'app/components/modals/views/modal_window.html',
        controller: function($scope) {
          var reportConfig = isProfessional() ? ReportConfigSvc.configForReport(reportId) : ConsumerReportConfigSvc.configForReport(reportId);
          $scope.help = {};

          $scope.hasHelp = function(id) {
            if (id == 'interpretation') {
              return _.has(reportConfig, 'ShowInterpretationFlag') && reportConfig.ShowInterpretationFlag;
            }
            else if (id == 'legend') {
              return displayType == 'symbols' || displayType == 'symbols_rar' || displayType == 'bar_charts';
            }

            return false;
          };

          $scope.showSymbol = function() {
            return displayType == 'symbols' || displayType == 'symbols_rar';
          };

          $scope.showBar = function() {
            return displayType == 'bar_charts';
          };

          $scope.getDefaultTab = function() {
            if ($scope.hasHelp('legend')) return 'legend';
            else return 'interpretation';
          };

          if ($scope.hasHelp('interpretation')) {
            var interp = reportConfig.InterpretationHTMLDescription;
            if (childConfig) {
              interp = childConfig.InterpretationHTMLDescription;
            }

            $scope.help.interpretation = interp;
          }
        }
      });

      modalInstance.result.then(restoreFocus, restoreFocus);
    }

    function saveFocus() {
      activeEl = document.activeElement;
    }

    function restoreFocus() {
      $rootScope.modalOpen = false;
      if (activeEl) {
        activeEl.focus();
      }
    }
  }

})();
