/**
 * Professional Product
 * Pages Module
 * Site Footer Block
 *
 * This controller corresponds to the site-wide page footer on the professional site.
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.pages')
    .controller('FooterCtrl', FooterCtrl);

  FooterCtrl.$inject = ['$scope', 'pgAboutUs'];
  function FooterCtrl($scope, pgAboutUs) {
    $scope.pgAboutUs = pgAboutUs;
  }
})();

