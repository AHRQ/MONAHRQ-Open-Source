/**
 * Monahrq Nest
 * Core Module
 * Application Bootstrap
 *
 * This module loads plugins and module dependencies, bootstraps angular, loads core data files, loads
 * flutters, and sets up app-wide error handling and logging.
 */
//(function() { TODO: needs some refactoring before this can be an IIFE
  'use strict';


  /**
   * Angular wiring
   */
  var mainApp = angular.module('monahrq', [
    'ngCookies',
    'ngResource',
    'ngSanitize',
    'ui.router',
    'ui.autocomplete',
    'ui.bootstrap',
    'ui.map',
    'underscore',
    'oc.lazyLoad',
    'sticky',
    'angularSpinner',
    'placeholderShim',
    'ngA11y',
    'angular-intro',

    'monahrq.core',
    'monahrq.services',
    'monahrq.domain',
    'monahrq.components',
    'monahrq.products.professional',
    'monahrq.products.consumer'
  ])
    .config(config)
    .run(run);


  angular.module('monahrq.core', []);


  config.$inject = ['$stateProvider', '$urlRouterProvider'];
  function config($stateProvider, $urlRouterProvider) {

    function isLocal() {
      return window.location.protocol == 'file:';
    }

    $stateProvider.state('top', {
      abstract: true,
      views: {
        'loader': {
          template: '',
          controller: 'LoaderCtrl',
          resolve: {
            websiteConfig: function (ResourceSvc) {
              return ResourceSvc.getConfiguration();
            },
            reportConfig: function (ResourceSvc, websiteConfig) {
              if (_.contains(websiteConfig.active_products, 'professional')) {
                return ResourceSvc.getReportConfig();
              }
              else {
                return [];
              }
            },
            consumerReportConfig: function (ResourceSvc, websiteConfig) {
              if (_.contains(websiteConfig.active_products, 'consumer')) {
                return ResourceSvc.getConsumerReportConfig();
              }
              else {
                return [];
              }
            },
            menu: function(ResourceSvc) {
              return ResourceSvc.getMenu();
            }
          }
        },
        'flutter': {
          template: '',
          controller: function () {
          },
          resolve: {
            flutters: function (FlutterLoaderSvc) {
              if (isLocal()) {
                return [];
              }
              return FlutterLoaderSvc.loadAll();
            }
          }
        }
      }
    });

    $stateProvider.state('top.default', {
      url: '/',
      views: {
        'content@': {
          template: '',
          controller: function($state, $scope) {
            var prod = $scope.config.default_product ? $scope.config.default_product : 'professional';
            $state.go('top.' +  prod + '.home');
          }
        }
      }
    });

    $urlRouterProvider.otherwise('/');
  }

  run.$inject = ['$rootScope', '$state', '$stateParams', '$location', '$window', '$timeout'];
  function run($rootScope, $state, $stateParams, $location, $window, $timeout) {
    $rootScope.$state = $state;
    $rootScope.$stateParams = $stateParams;
    $rootScope.printFriendly = false;


    function updatePrintFriendly(scope, params) {
      if (_.has(params, 'print') && params.print) {
        scope.printFriendly = true;
      }
      else {
        scope.printFriendly = false;
      }
    }

    $rootScope.$on('$stateChangeStart',
      function (event, toState, toParams, fromState, fromParams) {
       updatePrintFriendly(event.currentScope, toParams);
      });

    $rootScope.$on('$stateChangeSuccess', function (event, toState, toParams, fromState, fromParams) {
      if ($window.ga) {
        $window.ga('set', {
          page:  $location.path(),
          title: toState.data && toState.data.pageTitle ? toState.data.pageTitle : ''
        });
        $window.ga('send', 'pageview');
      }

      $state.previous = {
        name: fromState,
        params: fromParams
      };

      if (toState.data && toState.data.pageTitle) {
        $rootScope.pageTitle = toState.data.pageTitle;
      }
      else {
        $rootScope.pageTitle = undefined;
      }

      window.scrollTo(0, 0);

      JL('app.stateChangeSuccess').debug({
        msg: 'Success changing states',
        fromState: fromState,
        fromParams: fromParams,
        toState: toState,
        toParams: toParams
      });
    });

    $rootScope.$on('$stateChangeError',
      function (event, toState, toParams, fromState, fromParams, error) {
        JL('app.stateChangeError').error({
          msg: 'Error changing states',
          error: error.message || error,
          fromState: fromState,
          fromParams: fromParams,
          toState: toState,
          toParams: toParams
        });
      });

    $rootScope.$on('$locationChangeSuccess', function () {
      $rootScope.$broadcast('LocationChange');
    });
  }

  angular.module('monahrq').factory('$exceptionHandler', function () {
    return function (exception, cause) {
      exception.message += ' (caused by "' + cause + '")';
      JL().fatalException("Exception", exception);
    };
  });

  function onGoogleReady() {
    setupLogging();
    BuildInfoBox();
    modalTrap();
    angular.element(document).ready(function () {
      angular.bootstrap(document, ['monahrq']);
    });
  }

  function modalTrap() {
    jQuery(document).on("focus", function (event) {
      var $dialog = jQuery("#modalWindow");

      if ($dialog.is(':visible') && !jQuery.contains($dialog[0], event.target) && $dialog[0] !== event.target) {
        event.stopPropagation();
        $dialog.find('.close').focus();
      }
    }, true);
  }

  function setupLogging() {
    JL.setOptions({
      enabled: true
    });

    var appender = JL.createConsoleAppender('con');
    appender.setOptions({
      "level": JL.getTraceLevel()
    });

    var logger = JL();
    logger.setOptions({
      "level": JL.getTraceLevel(),
      //"level": JL.getErrorLevel(),
      "appenders": [appender]
    });
  }

//})();
