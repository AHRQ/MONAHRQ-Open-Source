/**
 * Professional Product
 * Pages Module
 * Setup
 *
 * Initialize the states for the top-level professional container as well as the
 * various static informational pages and infographics.
 */
(function() {
'use strict';


/**
 * Angular wiring
 */
    angular.module('monahrq.products.professional.pages', [
      'monahrq.products.professional.pages.domain'
    ])

  .config(config)
  .run(run);


/**
 * Module config
 */
config.$inject = ['$stateProvider'];
function config($stateProvider) {
  function isLocal() {
    return window.location.protocol == 'file:';
  }

    $stateProvider.state('top.professional', {
      url: '/professional',
      abstract: true,
      views: {
       'header@': {
          templateUrl: 'app/products/professional/pages/views/header.html',
          controller: 'HeaderCtrl'
        },
        'navigation@': {
          templateUrl: 'app/products/professional/pages/views/navigation.html',
          controller: 'NavigationCtrl'
        },
        'footer@': {
          templateUrl: 'app/products/professional/pages/views/footer.html',
          controller: 'FooterCtrl',
          resolve: {
            pgAboutUs: function (ResourceSvc) {
              return ResourceSvc.getpgAboutUs();
            }
          }
        }
      }
    });

    if (!isLocal()) {
      $stateProvider.state('top.professional.flutters', {
        url: '/flutters',
        views: {
          'content@': {
            templateUrl: 'app/products/professional/pages/views/flutters.html',
            controller: 'FluttersCtrl'
          }
        }
      });
    }


   $stateProvider.state('top.professional.home', {
      url: '/',
      views: {
        'content@': {
          templateUrl: 'app/products/professional/pages/views/index.html',
          controller: 'HomeCtrl',
          resolve: {
            content: function(ResourceSvc) {
              return ResourceSvc.getpgHome();
            }
          }
        }
      }
    });

    $stateProvider.state('top.professional.resources', {
        url: '/resources/:page',
        data: {
            pageTitle: 'Resources'
        },
        views: {
            'content@': {
                templateUrl: 'app/products/professional/pages/views/resources.html',
                controller: 'ResourcesCtrl',
                resolve: {
                    content: function(ResourceSvc) {
                        return ResourceSvc.getpgResources();
                    },
                    measureTopicCategories: function(ResourceSvc, ReportConfigSvc) {
                        if (ReportConfigSvc.webElementAvailable('Resource_AboutQR_Hospital')) {
                            return ResourceSvc.getMeasureTopicCategories();
                        }

                        return [];
                    },
                    measureTopics: function(ResourceSvc, ReportConfigSvc) {
                        if (ReportConfigSvc.webElementAvailable('Resource_AboutQR_Hospital')) {
                            return ResourceSvc.getMeasureTopics();
                        }

                        return [];
                    },
                    ahsTopics: function(ResourceSvc, ReportConfigSvc) {
                        if (ReportConfigSvc.webElementAvailable('Resource_AboutQR_AHS')) {
                            return ResourceSvc.getahsTopics();
                        }

                        return [];
                    }
                }
            }


        }
    });
    //    .state('top.professional.resources.AboutQualityRatings', {
    //    url: '/resources/AboutQualityRatings',
    //    data: {
    //        pageTitle: 'Resources'
    //    },
    //    views: {
    //        'content@': {
    //            templateUrl: 'aboutQR.html',
    //            controller: 'ResourcesCtrl',
    //            resolve: {
    //                content: function (ResourceSvc) {
    //                    return ResourceSvc.getpgResources();
    //                },
    //                measureTopicCategories: function (ResourceSvc, ReportConfigSvc) {
    //                    if (ReportConfigSvc.webElementAvailable('Resource_AboutQR_Hospital')) {
    //                        return ResourceSvc.getMeasureTopicCategories();
    //                    }

    //                    return [];
    //                },
    //                measureTopics: function (ResourceSvc, ReportConfigSvc) {
    //                    if (ReportConfigSvc.webElementAvailable('Resource_AboutQR_Hospital')) {
    //                        return ResourceSvc.getMeasureTopics();
    //                    }

    //                    return [];
    //                },
    //                ahsTopics: function (ResourceSvc, ReportConfigSvc) {
    //                    if (ReportConfigSvc.webElementAvailable('Resource_AboutQR_AHS')) {
    //                        return ResourceSvc.getahsTopics();
    //                    }

    //                    return [];
    //                }
    //            }
    //        }
    //    }

    //});

    $stateProvider.state('top.professional.about-us', {
      url: '/about-us',
      data: {
        pageTitle: 'About Us'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/professional/pages/views/about-us.html',
          controller: 'AboutUsCtrl',
          resolve: {
            pgAboutUs: function (ResourceSvc) {
              return ResourceSvc.getpgAboutUs();
            }
          }
        }
      }
    });

    $stateProvider.state('top.professional.infographic', {
      url: '/infographic',
      data: {
        pageTitle: 'Infographic'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/professional/pages/views/infographic.html',
          controller: 'InfographicCtrl',
          resolve: {
            report: function (InfographicReportSvc) {
              return InfographicReportSvc.getReport();
            }
          }
        }
      }
    });

    $stateProvider.state('top.professional.infographic.footnotes', {
      url: '/footnotes',
      data: {
        pageTitle: 'Infographic'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/professional/pages/views/infographic-footnotes.html',
          controller: 'InfographicCtrl',
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
