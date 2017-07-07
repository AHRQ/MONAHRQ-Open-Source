/**
 * Professional Product
 * Nursing Homes Report Module
 * Setup
 *
 * Configure the page states used by the nursing homes section of the professional website.
 */
(function() {
'use strict';

/**
 * Angular wiring
 */
angular.module('monahrq.products.professional.nursing-homes', [
  'monahrq.products.professional.nursing-homes.domain'
])
  .config(config)
  .run(run);


/**
 * Module config
 */
config.$inject = ['$stateProvider'];
function config($stateProvider) {
    $stateProvider.state('top.professional.nursing-homes', {
      url: '/nursing-homes',
      data: {
        pageTitle: 'Nursing Homes'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/professional/nursing-homes/views/index.html',
          controller: 'NursingHomesCtrl'
        }
       }
    });

    $stateProvider.state('top.professional.nursing-homes.location', {
      url: '/location?searchType&type&inHospital&overallRating&location&zipDistance&zip&county&displayType',
      data: {
        pageTitle: 'Nursing Home - Summary Ratings',
        report: 'ba52b7b2-f4c8-4831-b910-1d036b94ae75'
      },
      views: {
        'content@':{
          templateUrl: 'app/products/professional/nursing-homes/views/location.html',
          controller: 'NHLocationCtrl',
          resolve: {
          }
        },
        'nh-search-location@top.professional.nursing-homes.location': {
          templateUrl: 'app/products/professional/nursing-homes/views/search_location.html',
          controller: 'NHSearchLocationCtrl',
          resolve: {
            nursingHomes: function(ResourceSvc) {
              return ResourceSvc.getNursingHomes();
            },
            nursingHomeTypes: function(ResourceSvc) {
              return ResourceSvc.getNursingHomeTypes();
            },
            nursingHomeCounties: function(ResourceSvc) {
              return ResourceSvc.getNursingHomeCounties();
            },
            zipDistances: function(ResourceSvc) {
              return ResourceSvc.getZipDistances();
            }
          }
        },
        'nh-content-location@top.professional.nursing-homes.location': {
          templateUrl: function(stateParams) {
            if (!stateParams.displayType) return null;
            return 'app/products/professional/nursing-homes/views/' + stateParams.displayType + '_location.html';
          },
          controllerProvider: function($stateParams) {
            if (!$stateParams.displayType) return null;
            return 'NH' + lcase($stateParams.displayType) + 'LocationCtrl';
          },
          resolve: {
            nursingHomeTypes: function(ResourceSvc) {
              return ResourceSvc.getNursingHomeTypes();
            },
            nursingHomeCounties: function(ResourceSvc) {
              return ResourceSvc.getNursingHomeCounties();
            },
            zipDistances: function(ResourceSvc) {
              return ResourceSvc.getZipDistances();
            }
          }
        }
      }
    });

    $stateProvider.state('top.professional.nursing-homes.profile', {
      url: '/profile/:id',
      data: {
        pageTitle: 'Nursing Home Profile',
        report: '87e04110-46b0-4cae-9592-022c3111fac7'
      },
      views: {
        'content@':{
          templateUrl: 'app/products/professional/nursing-homes/views/profile.html',
          controller: 'NHProfileCtrl',
          resolve: {
            content: function(ResourceSvc) {
              return null;
            },
            profile: function(NHRepositorySvc, $stateParams) {
              return NHRepositorySvc.getProfile(+$stateParams.id);
            },
            nursingHomeCounties: function(ResourceSvc) {
              return ResourceSvc.getNursingHomeCounties();
            }
          }
        },
        'nh-profile-detail@top.professional.nursing-homes.profile': {
          templateUrl: 'app/products/professional/nursing-homes/views/profile_detail.html'
        },
        'nh-profile-ratings@top.professional.nursing-homes.profile': {
          templateUrl: 'app/products/professional/nursing-homes/views/profile_ratings.html',
          controller: 'NHProfileRatingsCtrl',
          resolve: {
            measureTopics: function (ResourceSvc) {
              return ResourceSvc.getNursingHomeMeasureTopics();
            }
          }
        }
      }
    });

    $stateProvider.state('top.professional.nursing-homes.compare', {
      url: '/compare?ids',
      data: {
        pageTitle: 'Compare - Nursing Homes',
        report: 'f2f2b7fe-8653-488b-8ed8-fd4417cd0f9e'
      },
      views: {
        'content@':{
          templateUrl: 'app/products/professional/nursing-homes/views/compare.html',
          controller: 'NHCompareCtrl',
          resolve: {
            content: function(ResourceSvc) {
              return null; //ResourceSvc.getpgQRCompare();
            }
          }
        },
       'nh-table-compare@top.professional.nursing-homes.compare':{
          templateUrl: 'app/products/professional/nursing-homes/views/table_compare.html',
          controller: 'NHTableCompareCtrl',
          resolve: {
            nursingHomes: function(ResourceSvc) {
              return ResourceSvc.getNursingHomes();
            },
            measureTopics: function(ResourceSvc) {
              return ResourceSvc.getNursingHomeMeasureTopics();
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

