/**
 * Consumer Product
 * Hospital Reports Module
 * Setup
 *
 * This defines the page states used by the hospital section of the consumer website.
 */
(function() {
'use strict';

/**
 * Angular wiring
 */
angular.module('monahrq.products.consumer.hospitals', [
/*  'monahrq.products.professional.quality-ratings.domain'*/
])
  .config(config)
  .run(run);


/**
 * Module config
 */
config.$inject = ['$stateProvider'];
function config($stateProvider) {

    

    $stateProvider.state('top.consumer.hospitals', {
      abstract: true,
      url: '/hospitals'
    });

    $stateProvider.state('top.consumer.hospitals.location', {
      url: '/location?location&distance',
      data: {
        pageTitle: 'Compare Hospitals Search',
        report: '2aaf7fba-7102-4c66-8598-a70597e2f82b'
      },
      views: {
        'content@': {
            templateUrl: 'app/products/consumer/hospitals/views/location.html',
          controller: 'CHLocationCtrl',
          resolve: {
            zipDistances: function(ResourceSvc) {
              return ResourceSvc.getZipDistances();
            }
          }
        }
      }
    });

    $stateProvider.state('top.consumer.hospitals.location-map', {
      url: '/location-map?location&distance',
      data: {
        pageTitle: 'Compare Hospitals Search Map',
        report: '2aaf7fba-7102-4c66-8598-a70597e2f82b'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/hospitals/views/location_map.html',
          controller: 'CHLocationMapCtrl',
          resolve: {
            zipDistances: function(ResourceSvc) {
              return ResourceSvc.getZipDistances();
            }
          }
        }
      }
    });

    $stateProvider.state('top.consumer.hospitals.topic', {
      url: '/topic?topicId&subtopicId&zip&distance&focus',
      data: {
        pageTitle: 'Compare Hospitals by Condition',
        report: '1bd85413-734b-4c7a-9aaf-c442d8c2face'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/hospitals/views/topic.html',
          controller: 'CHTopicCtrl',
          resolve: {
            zipDistances: function(ResourceSvc) {
              return ResourceSvc.getZipDistances();
            },
            topics: function(ResourceSvc) {
              return ResourceSvc.getMeasureTopicCategoriesConsumer();
            },
            subtopics: function(ResourceSvc) {
              return ResourceSvc.getMeasureTopicsConsumer();
            }
          }
        }
      }
    });

    $stateProvider.state('top.consumer.hospitals.topic-map', {
      url: '/topic-map?topicId&subtopicId&zip&distance',
      data: {
        pageTitle: 'Compare Hospitals by Condition Map',
        report: '1bd85413-734b-4c7a-9aaf-c442d8c2face'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/hospitals/views/topic_map.html',
          controller: 'CHTopicMapCtrl',
          resolve: {
            zipDistances: function (ResourceSvc) {
              return ResourceSvc.getZipDistances();
            },
            topics: function(ResourceSvc) {
              return ResourceSvc.getMeasureTopicCategoriesConsumer();
            },
            subtopics: function(ResourceSvc) {
              return ResourceSvc.getMeasureTopicsConsumer();
            }
          }
        }
      }
    });

    $stateProvider.state('top.consumer.hospitals.compare', {
      url: '/compare?ids&topicId',
      data: {
        pageTitle: 'Compare Hospitals',
        report: '2aaf7fba-7102-4c66-8598-a70597e2f821'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/hospitals/views/compare.html',
          controller: 'CHCompareCtrl',
          resolve: {
            topics: function (ResourceSvc) {
              return ResourceSvc.getMeasureTopicCategoriesConsumer();
            },
            subtopics: function (ResourceSvc) {
              return ResourceSvc.getMeasureTopicsConsumer();
            }
          }
        }
      }
    });

    $stateProvider.state('top.consumer.hospitals.profile', {
      url: '/profile/:id',
      data: {
        pageTitle: 'Hospital Profile',
        report: '7af51434-5745-4538-b972-193f58e737d7'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/hospitals/views/profile.html',
          controller: 'CHProfileCtrl',
          resolve: {
            zipDistances: function (ResourceSvc) {
              return ResourceSvc.getZipDistances();
            },
            hospitalZips: function (ResourceSvc) {
              return ResourceSvc.getHospitalZips();
            }
          }
        },
        'profile-tab1@top.consumer.hospitals.profile': {
          templateUrl: 'app/products/consumer/hospitals/views/profile_quality.html',
          controller: 'CHProfileQualityCtrl'
        },
        'profile-tab2@top.consumer.hospitals.profile': {
          templateUrl: 'app/products/consumer/hospitals/views/profile_costs.html',
          controller: 'CHProfileCostsCtrl',
          resolve: {
            drg: function (ResourceSvc) {
              return ResourceSvc.getDRG();
            }
          }
        },
        'profile-tab3@top.consumer.hospitals.profile': {
          templateUrl: 'app/products/consumer/hospitals/views/profile_physicians.html',
          controller: 'CHProfilePhysiciansCtrl',
          resolve: {
            hospitals: function (ResourceSvc) {
              return ResourceSvc.getHospitals();
            }
          }
        }
      }
    });

    $stateProvider.state('top.consumer.hospitals.cost-quality', {
      url: '/cost-quality?subtopicId&hospitalIds',
      data: {
        report: '7d841284-5179-44e5-a00e-bdd042b0a7bd'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/hospitals/views/cost_quality.html',
          controller: 'CHCostQualityCtrl',
          resolve: {
            topics: function (ResourceSvc) {
              return ResourceSvc.getMeasureTopicCategoriesConsumer();
            },
            subtopics: function (ResourceSvc) {
              return ResourceSvc.getMeasureTopicsConsumer();
            },
            costQualityTopics: function (ResourceSvc) {
              return ResourceSvc.getCostQualityTopics();
            }
          }
        }
      }
    });

    $stateProvider.state('top.consumer.hospitals.infographic', {
      url: '/infographic/:id',
      data: {
        pageTitle: 'Infographic'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/hospitals/views/infographic.html',
          controller: 'CHInfographicCtrl',
          resolve: {
            topics: function (ResourceSvc) {
              return ResourceSvc.getMeasureTopicCategoriesConsumer();
            }
          }
        }
      }
    });

    $stateProvider.state('top.consumer.hospitals.health-topics', {
      url: '/health-topics',
      data: {
        pageTitle: 'Browse Health Topics'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/hospitals/views/health_topics.html',
          controller: 'CHHealthTopicsCtrl',
          resolve: {
            topics: function (ResourceSvc) {
              return ResourceSvc.getMeasureTopicCategoriesConsumer();
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
