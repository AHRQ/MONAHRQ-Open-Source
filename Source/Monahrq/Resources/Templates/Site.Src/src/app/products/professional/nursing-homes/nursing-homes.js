/**
 * Professional Product
 * Nursing Homes Module
 * Nursing Homes Page Controller
 *
 * This controller corresponds to the root nursing home state
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.nursing-homes')
    .controller('NursingHomesCtrl', NursingHomesCtrl);


  NursingHomesCtrl.$inject = ['$scope', '$state'];
  function NursingHomesCtrl($scope, $state) {
    $scope.content = {
      title: $state.current.data.pageTitle,
      body: "Nursing home content"
    };
  }

})();

