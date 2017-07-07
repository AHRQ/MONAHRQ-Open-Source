/**
 * Professional Product
 * Physicians Module
 * Physicians Page Controller
 *
 * This controller corresponds to the root physician state
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.physicians')
    .controller('PhysiciansCtrl', PhysiciansCtrl);


  PhysiciansCtrl.$inject = ['$scope', '$state'];
  function PhysiciansCtrl($scope, $state) {
    $scope.content = {
      title: $state.current.data.pageTitle,
      body: "Nursing home content"
    };
  }

})();

