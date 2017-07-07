/**
 * Monahrq Nest
 * Core Module
 * Main Controller
 *
 * The Main controller lives on the <html> element and provides some global behavior.
 * It primarily sets styling hooks on containers to effect different appearances for
 * consumer vs professional, flutters, etc.
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.core')
    .controller('MainCtrl', MainCtrl);


  MainCtrl.$inject = ['$scope', '$state'];
  function MainCtrl($scope, $state) {
    $scope.navActive = function (state) {
      if (state == 'top.professional.quality-ratings') {
        // the 'quality ratings' tab show not be active unless quality reports are avail
        if (!($scope.ReportConfigSvc.webElementAvailable('Quality_ConditionTopic_Tab'))) {
          return false;
        }
      }

      return $state.includes(state);
    };

    $scope.skipNav = function ($event) {
      document.getElementById('content').focus();
      $event.preventDefault();
    };

    $scope.contentClass = function() {
      var classes = [];

      if ($state.includes('top.professional.home')) {
        classes.push( 'page--home_page');
      }

      if ($state.includes('top.professional.infographic') || $state.includes('top.consumer.infographic')) {
        classes.push('page--infographic');
      }

      return classes;
    };

    $scope.wrapperClass = function() {
      var classes = [];

      if ($state.includes('top.professional.flutters')) {
        classes.push('flutter');
      }

      if ($state.includes('top.professional')) {
        classes.push( 'professional');
      }


      if ($state.includes('top.consumer')) {
       classes.push( 'consumer');
      }

      if ($state.is('top.consumer.home')) {
        classes.push( 'front');
      }

      return classes.join(' ');
    }
  }

})();
