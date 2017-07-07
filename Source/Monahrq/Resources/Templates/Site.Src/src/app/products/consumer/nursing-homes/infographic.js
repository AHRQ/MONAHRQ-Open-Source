/**
 * Consumer Product
 * Nursing Home Reports Module
 * Infographic Page
 *
 * This controller generates the generic infographic page for nursing homes.
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.nursing-homes')
    .controller('CNHInfographicCtrl', CNHInfographicCtrl);

  CNHInfographicCtrl.$inject = ['$scope', '$stateParams', 'report', 'ScrollToElSvc'];
  function CNHInfographicCtrl($scope, $stateParams, report, ScrollToElSvc) {
    $scope.getMeasure = getMeasure;
    $scope.gotoFootnote = gotoFootnote;
    $scope.footnoteAccordionAPI = {};

    init();

    function init() {
      $scope.report = report;
    }

    function getMeasure(name) {
      var m = _.findWhere(report.measures, {name: name});
      if (m) {
        return _.first(m.values);
      }
      return null;
    }

    function gotoFootnote($event, id) {
      $event.preventDefault();
      $scope.footnoteAccordionAPI.open('footnotesPanel');
      window.setTimeout(function() {
        ScrollToElSvc.scrollToEl(id);
      }, 0);
    }
  }

})();
