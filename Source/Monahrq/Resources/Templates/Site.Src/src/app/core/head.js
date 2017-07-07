/**
 * Monahrq Nest
 * Core Module
 * Head Controller
 *
 * Manage the title displayed in the browser's title bar. The title consists of
 * a prefix set in the scope by the active route, and a global postfix provided by
 * the website_config.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.core')
    .controller('HeadCtrl', HeadCtrl);


  HeadCtrl.$inject = ['$rootScope', '$scope'];
  function HeadCtrl($rootScope, $scope) {
    $scope.title = "";

    $rootScope.$watch('config.website_BrowserTitle', function(n, o) {
      $scope.title = n;
    });

    $rootScope.$watch('pageTitle', function(n, o) {
      if (n) {
        $scope.title = n + ' - ' + $rootScope.config.website_BrowserTitle;
      }
      else if ($rootScope.config) {
        $scope.title = $rootScope.config.website_BrowserTitle;
      }
    });

  }

})();

