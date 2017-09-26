/**
 * Consumer Product
 * Pages Module
 * Setup
 *
 * Initialize the states for the top-level consumer container as well as the
 * various static informational pages and infographics.
 */
(function() {
'use strict';


/**
 * Angular wiring
 */
angular.module('monahrq.products.consumer.pages', [
/*  'monahrq.products.professional.pages.domain'*/
])
  .config(config)
  .run(run);


/**
 * Module config
 */
config.$inject = ['$stateProvider'];
function config($stateProvider) {
    $stateProvider.state('top.consumer', {
      url: '/consumer',
      abstract: true,
      views: {
        'header@': {
            templateUrl:  'app/products/consumer/pages/views/header.html',
          controller: 'CHeaderCtrl'
        },
        'navigation@': {
            templateUrl: 'app/products/consumer/pages/views/navigation.html',
          controller: 'CNavigationCtrl'
        },
        'footer@': {
            templateUrl: 'app/products/consumer/pages/views/footer.html',
          controller: function ($scope, ResourceSvc, WalkthroughSvc, $rootScope, $state) {
            $scope.showGuideTool = false;
            $scope.showHelpLinks = false;
            $scope.toggleHelpLinks = toggleHelpLinks;
            $scope.beforeChangeEvent = function(el,scope) {
                WalkthroughSvc.setIntroIsRunning(true);
                if ($state.current.name === 'top.consumer.hospitals.location') {
                    WalkthroughSvc.setSearchComplete(false, 'hosp');
                }
                if ($state.current.name === 'top.consumer.nursing-homes.location') {
                    WalkthroughSvc.setSearchComplete(false, 'nh');
                }
                if ($state.current.name === 'top.consumer.physicians.search') {
                    WalkthroughSvc.setSearchComplete(false, 'ph');
                }
                if(this._currentStep == 0){
                    WalkthroughSvc.screenRead(el,scope, $scope.IntroOptions.steps);
                }
            }

            $scope.exitEvent = function() {
              WalkthroughSvc.setIntroIsRunning(false);
            }
            
            $scope.searchComplete = function () {
                if ($state.current.name === 'top.consumer.hospitals.location') {
                    WalkthroughSvc.setSearchComplete(true, 'hosp');
                }
                if ($state.current.name === 'top.consumer.nursing-homes.location') {
                    WalkthroughSvc.setSearchComplete(true, 'nh');
                }
                if ($state.current.name === 'top.consumer.physicians.search') {
                    WalkthroughSvc.setSearchComplete(true, 'ph');
                }
            }

            function toggleHelpLinks(){
              $scope.showHelpLinks = !$scope.showHelpLinks;
            }

            setIntroOptions($state.current.name);
        
            $rootScope.$on('$stateChangeSuccess',function(event, toState, toParams, fromState, fromParams) {
               setIntroOptions(toState.name);
            });

            function setIntroOptions(state) {
              $scope.showFooterBar = true;
              if(state === 'top.consumer.home'){
                $scope.IntroOptions = WalkthroughSvc.IntroOptionsLanding();
              }
              else if(state === 'top.consumer.hospitals.location'){
                $scope.IntroOptions = WalkthroughSvc.IntroOptionsLocationNoResults();
              }
              else if(state === 'top.consumer.hospitals.topic'){
                $scope.IntroOptions = WalkthroughSvc.IntroOptionsTopic();
              }
              else if(state === 'top.consumer.hospitals.compare'){
                $scope.IntroOptions = WalkthroughSvc.IntroOptionsCompare();
              }
              else if(state === 'top.consumer.nursing-homes.location'){
                $scope.IntroOptions = WalkthroughSvc.IntroOptionsNHLocationNoResults();
              }
              else if(state === 'top.consumer.physicians.search'){
                $scope.IntroOptions = WalkthroughSvc.IntroOptionsPhysiciansSearch();
              }
              else {
               $scope.showFooterBar = false;
              }
            }

            ResourceSvc.getpgAboutUs()
              .then(function(pg) {
                $scope.pgAboutUs = pg;
              });

            ResourceSvc.getConfiguration()
            .then(function (config) {
                $scope.showGuideTool = !_.isUndefined(config.products.consumer.website_guidetool) ? config.products.consumer.website_guidetool : false;
                $rootScope.hasGuideTool = $scope.showGuideTool;
            });

          }
        }
      }
    });

    $stateProvider.state('top.consumer.home', {
      url: '/',
      data: {
        pageTitle: 'Home'
      },
      views: {
        'content@': {
            templateUrl: 'app/products/consumer/pages/views/index.html',
          controller: 'CHomeCtrl'
        }
      }
    });

    $stateProvider.state('top.consumer.profile-search', {
      url: '/profile-search?type&term',
      data: {
        pageTitle: 'Profile Search'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/pages/views/profile_search.html',
          controller: 'CProfileSearchCtrl'
        }
      }
    });

    $stateProvider.state('top.consumer.about-ratings', {
      url: '/about-site/:page',
      data: {
        pageTitle: 'About This Site'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/pages/views/about-ratings.html',
          controller: 'AboutRatingsCtrl',
          resolve: {
            content: function(ResourceSvc) {
              return ResourceSvc.getpgResources();
            },
            measureTopicCategories: function(ResourceSvc, ConsumerReportConfigSvc) {
              if (ConsumerReportConfigSvc.webElementAvailable('Resource_AboutQR_Hospital')) {
                  return ResourceSvc.getMeasureTopicCategoriesConsumer();
              }

              return [];
            },
            measureTopics: function(ResourceSvc, ConsumerReportConfigSvc) {
              if (ConsumerReportConfigSvc.webElementAvailable('Resource_AboutQR_Hospital')) {
                  return ResourceSvc.getMeasureTopicsConsumer();
              }

              return [];
            },
            ahsTopics: function(ResourceSvc, ConsumerReportConfigSvc) {
              if (ConsumerReportConfigSvc.webElementAvailable('Resource_AboutQR_AHS')) {
                return ResourceSvc.getahsTopics();
              }

              return [];
            }
          }
        }
      }
    });

    /*$stateProvider.state('top.consumer.how-to-use', {
      url: '/how-to-use',
      data: {
        pageTitle: 'How to Use this Site'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/pages/views/how-to-use.html',
          controller: 'HowToUseCtrl'
        }
      }
    });*/

    $stateProvider.state('top.consumer.terms', {
      url: '/terms',
      data: {
        pageTitle: 'Terms and Conditions'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/pages/views/terms.html'
        }
      }
    });

    $stateProvider.state('top.consumer.disclaimer', {
      url: '/disclaimer',
      data: {
        pageTitle: 'Disclaimer'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/pages/views/disclaimer.html'
        }
      }
    });

    $stateProvider.state('top.consumer.infographic', {
      url: '/infographic',
      data: {
        pageTitle: 'Infographic'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/pages/views/infographic.html',
          controller: 'ConsumerInfographicCtrl',
          resolve: {
            report: function (InfographicReportSvc) {
              return InfographicReportSvc.getReport();
            },
            topics: function(ResourceSvc) {
              return ResourceSvc.getMeasureTopicCategoriesConsumer();
            }
          }
        }
      }
    });

    $stateProvider.state('top.consumer.infographic.footnotes', {
      url: '/footnotes',
      data: {
        pageTitle: 'Infographic'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/pages/views/infographic-footnotes.html',
          controller: 'ConsumerInfographicCtrl',
          resolve: {
            report: function (InfographicReportSvc) {
              return InfographicReportSvc.getReport();
            }
          }
        }
      }
    });

}

/**
 * Module run
 */
function run() {
}

})();
