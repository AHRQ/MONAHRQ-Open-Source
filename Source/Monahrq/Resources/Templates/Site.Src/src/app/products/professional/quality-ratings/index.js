/**
 * Professional Product
 * Quality Ratings Report Module
 * Setup
 *
 * Configure the page states used by the quality ratings section of the professional website.
 */
(function() {
'use strict';

/**
 * Angular wiring
 */
angular.module('monahrq.products.professional.quality-ratings', [
  'monahrq.products.professional.quality-ratings.domain'
])
  .config(config)
  .run(run);


/**
 * Module config
 */
config.$inject = ['$stateProvider'];
function config($stateProvider) {
    $stateProvider.state('top.professional.quality-ratings', {
      url: '/quality-ratings',
      data: {
        pageTitle: 'Quality Ratings'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/professional/quality-ratings/views/index.html',
          controller: 'QualityRatingsCtrl',
          resolve: {
            content: function(ResourceSvc) {
              return ResourceSvc.getpgQualityRatings();
            },
            measureTopicCategories: function(ResourceSvc, ReportConfigSvc) {
              if (ReportConfigSvc.webElementAvailable('Quality_ConditionTopic_Tab')) {
                return ResourceSvc.getMeasureTopicCategories();
              }

              return [];
            }
          }
        },
        'qr-search@top.professional.quality-ratings': {
          templateUrl: 'app/products/professional/quality-ratings/views/search_begin.html',
          controller: 'QRSearchBeginCtrl'
        }
       }
    });

    $stateProvider.state('top.professional.quality-ratings.location', {
      url: '/location?hospitalType&searchType&geoType&zipDistance&zip&region&displayType',
      data: {
        pageTitle: 'Hospital - Quality Ratings',
        report: {
          'symbols': '2aaf7fba-7102-4c66-8598-a70597e2f82b'
        }
      },
      views: {
        'content@':{
          templateUrl: 'app/products/professional/quality-ratings/views/location.html',
          controller: 'QRLocationCtrl',
          resolve: {
            content: function(ResourceSvc) {
              return ResourceSvc.getpgQRLocation();
            }
          }
        },
        'qr-search-location@top.professional.quality-ratings.location': {
          templateUrl: 'app/products/professional/quality-ratings/views/search_location.html',
          controller: 'QRSearchLocationCtrl',
          resolve: {
            hospitals: function(ResourceSvc) {
              return ResourceSvc.getHospitals();
            },
            hospitalTypes: function(ResourceSvc) {
              return ResourceSvc.getHospitalTypes();
            },
            hospitalRegions: function(ResourceSvc) {
              return ResourceSvc.getHospitalRegions();
            },
            zipDistances: function(ResourceSvc) {
              return ResourceSvc.getZipDistances();
            }
          }
        },
        'qr-table-location@top.professional.quality-ratings.location':{
          templateUrl: function(stateParams) {
            if (!stateParams.displayType) return null;
            return 'app/products/professional/quality-ratings/views/' + stateParams.displayType + '_location.html';
          },
          controllerProvider: function($stateParams) {
            if (!$stateParams.displayType) return null;
            return 'QR' + lcase($stateParams.displayType) + 'LocationCtrl';
          },
          resolve: {
            hospitals: function(ResourceSvc) {
              return ResourceSvc.getHospitals();
            },
            hospitalTypes: function(ResourceSvc) {
              return ResourceSvc.getHospitalTypes();
            },
            hospitalZips: function(ResourceSvc) {
              return ResourceSvc.getHospitalZips();
            }
          }
        }
      }
    });

    $stateProvider.state('top.professional.quality-ratings.condition', {
      url: '/condition?topic&subtopic&searchType&hospitalName&hospitalType&geoType&zipDistance&zip&region',
      data: {
        pageTitle: 'Condition or Topic - Quality Ratings',
        report: {
          'symbols': '1bd85413-734b-4c7a-9aaf-c442d8c2face',
          'symbols_rar': '7aac8244-0f39-424a-85be-943b465ed61a',
          'bar_charts': '12546e5a-ca82-4d7e-aab5-c1db88a4fd33',
          'raw_data': '7aac8244-0f39-424a-85be-943b465ed61b'
        }
      },
      views: {
        'content@':{
          templateUrl: 'app/products/professional/quality-ratings/views/condition.html',
          controller: 'QRConditionCtrl',
          resolve: {
            content: function(ResourceSvc) {
              return ResourceSvc.getpgQRCondition();
            }
          }
        },
        'qr-search-condition@top.professional.quality-ratings.condition': {
          templateUrl: 'app/products/professional/quality-ratings/views/search_condition.html',
          controller: 'QRSearchConditionCtrl',
          resolve: {
            hospitals: function(ResourceSvc) {
              return ResourceSvc.getHospitals();
            },
            hospitalTypes: function(ResourceSvc) {
              return ResourceSvc.getHospitalTypes();
            },
            hospitalRegions: function(ResourceSvc) {
              return ResourceSvc.getHospitalRegions();
            },
            zipDistances: function(ResourceSvc) {
              return ResourceSvc.getZipDistances();
            },
            measureTopicCategories: function(ResourceSvc) {
              return ResourceSvc.getMeasureTopicCategories();
            },
            measureTopics: function(ResourceSvc) {
              return ResourceSvc.getMeasureTopics();
            }
          }
        },
        'qr-table-condition@top.professional.quality-ratings.condition':{
          templateUrl: 'app/products/professional/quality-ratings/views/table_condition.html',
          controller: 'QRTableConditionCtrl',
          resolve: {
            hospitals: function(ResourceSvc) {
              return ResourceSvc.getHospitals();
            },
            measureTopicCategories: function(ResourceSvc) {
              return ResourceSvc.getMeasureTopicCategories();
            },
            measureTopics: function(ResourceSvc) {
              return ResourceSvc.getMeasureTopics();
            },
            hospitalZips: function(ResourceSvc) {
              return ResourceSvc.getHospitalZips();
            }
          }
        }
      }
    });

    $stateProvider.state('top.professional.quality-ratings.compare', {
      url: '/compare?hospitals',
      data: {
        pageTitle: 'Compare - Quality Ratings',
        report: {
          'symbols': '2aaf7fba-7102-4c66-8598-a70597e2f821',
          'symbols_rar': '2aaf7fba-7102-4c66-8598-a70597e2f820',
          'bar_charts': '2aaf7fba-7102-4c66-8598-a70597e2f823'
        }
      },
      views: {
        'content@':{
          templateUrl: 'app/products/professional/quality-ratings/views/compare.html',
          controller: 'QRCompareCtrl',
          resolve: {
            content: function(ResourceSvc) {
              return ResourceSvc.getpgQRCompare();
            }
          }
        },
       'qr-table-compare@top.professional.quality-ratings.compare':{
          templateUrl: 'app/products/professional/quality-ratings/views/table_compare.html',
          controller: 'QRTableCompareCtrl',
          resolve: {
            hospitals: function(ResourceSvc) {
              return ResourceSvc.getHospitals();
            },
            measureTopicCategories: function(ResourceSvc) {
              return ResourceSvc.getMeasureTopicCategories();
            },
            measureTopics: function(ResourceSvc) {
              return ResourceSvc.getMeasureTopics();
            }
          }
        }
      }
    });


    $stateProvider.state('top.professional.quality-ratings.profile', {
      url: '/profile/:id',
      data: {
        pageTitle: 'Hospital Profile - Quality Ratings',
        report: {
          'id_summary': '2aaf7fba-7102-4c66-8598-a70597e2f824',
          'profile': '7af51434-5745-4538-b972-193f58e737d7'
        }
      },
      views: {
        'content@':{
          templateUrl: 'app/products/professional/quality-ratings/views/profile.html',
          controller: 'QRProfileCtrl',
          resolve: {
            content: function(ResourceSvc) {
              return ResourceSvc.getpgQRProfile();
            },
            hospitals: function(ResourceSvc) {
              return ResourceSvc.getHospitals();
            },
            hospitalProfile: function(ResourceSvc, $stateParams) {
              return ResourceSvc.getHospitalProfile(+$stateParams.id);
            }
          }
        },
        'qr-profile-hospital@top.professional.quality-ratings.profile': {
          templateUrl: 'app/products/professional/quality-ratings/views/profile_hospital.html'
        },
        'qr-profile-patient-experience@top.professional.quality-ratings.profile': {
          templateUrl: 'app/products/professional/quality-ratings/views/profile_patient-experience.html',
          controller: 'QRProfilePatientExperienceCtrl'
        },
        'qr-profile-ratings@top.professional.quality-ratings.profile': {
          templateUrl: 'app/products/professional/quality-ratings/views/profile_ratings.html',
          controller: 'QRProfileRatingsCtrl',
          resolve: {
          }
         },
        'qr-profile-utilization@top.professional.quality-ratings.profile': {
          templateUrl: 'app/products/professional/quality-ratings/views/profile_utilization.html',
          controller: 'QRProfileUtilizationCtrl',
          resolve: {
            drg: function(ResourceSvc) {
              return ResourceSvc.getDRG();
            }
          }
        },
        'qr-profile-physicians@top.professional.quality-ratings.profile': {
          templateUrl: 'app/products/professional/quality-ratings/views/profile_physicians.html',
          controller: 'QRProfilePhysiciansCtrl',
          resolve: {
            hospitals: function(ResourceSvc) {
              return ResourceSvc.getHospitals();
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

function lcase(s) {
  return s.substring(0, 1).toUpperCase() + s.substring(1);
}
})();

