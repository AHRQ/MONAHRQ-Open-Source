/**
 * Professional Product
 * Pages Module
 * How to Use Page
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.pages')
    .controller('HowToUseCtrl', HowToUseCtrl);


  HowToUseCtrl.$inject = ['$sce', '$scope', '$state', '$stateParams'];
  function HowToUseCtrl($sce, $scope, $state, $stateParams) {

    $scope.getEntityGroupTitle = getEntityGroupTitle;


    function getEntityGroupTitle() {
      var rcs = $scope.ConsumerReportConfigSvc;
      var entityNames = {};
      if (rcs.hasEntity(rcs.entities.HOSPITAL)) {
        entityNames[rcs.entities.HOSPITAL] = 'hospital';
      }
      if (rcs.hasEntity(rcs.entities.NURSINGHOME)) {
        entityNames[rcs.entities.NURSINGHOME] = 'nursing home';
      }
      var entities = _.without(rcs.getEntities(), 'PHYSICIAN');
      var title;

      if (entities.length === 1) {
        title = entityNames[entities[0]];
      }
      else if (entities.length === 2) {
        title = entityNames[entities[0]] + ' or ' + entityNames[entities[1]];
      }

      return title;
    }

  }
})();

