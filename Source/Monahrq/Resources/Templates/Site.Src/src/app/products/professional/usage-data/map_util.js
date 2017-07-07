/**
 * Professional Product
 * Usage Data Report Module
 * Utilization Report Map Controller
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UDMapUtilCtrl', UDMapUtilCtrl);

  UDMapUtilCtrl.$inject = ['$scope', '$state', '$stateParams'];
  function UDMapUtilCtrl($scope, $state, $stateParams) {

    $scope.mapOptions = {
      center: new google.maps.LatLng(35.784, -78.670),
      zoom: 15,
      mapTypeId: google.maps.MapTypeId.ROADMAP
    };

    $scope.legend = [
      {low: 540, high: 2355},
      {low: 336, high: 450},
      {low: 263, high: 316},
      {low: 104, high: 208},
      {low: 34, high: 93}
    ];

    $scope.caption = 'Geographic Region';

    $scope.riskLabel = 'Risk-adjusted rate per 100,000 people';

  }

})();

