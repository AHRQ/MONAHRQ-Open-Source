/**
 * Professional Product
 * Pages Module
 * About Us Page
 *
 * Controller to load content for the about us page.
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.pages')
    .controller('AboutUsCtrl', AboutUsCtrl);


  AboutUsCtrl.$inject = ['$sce', '$scope', 'pgAboutUs'];
  function AboutUsCtrl($sce, $scope, pgAboutUs) {
    $scope.pgAboutUs = pgAboutUs;
    $scope.contentTrusted = $sce.trustAsHtml($scope.pgAboutUs.pageContent);
  }

})();
