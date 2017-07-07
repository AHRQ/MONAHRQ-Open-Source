/**
 * Consumer Product
 * Nursing Homes Report Module
 * Setup
 *
 * Configure the page states used by the nursing homes section of the consumer website.
 */
(function() {
'use strict';

/**
 * Angular wiring
 */
angular.module('monahrq.products.consumer.nursing-homes', [
/*  'monahrq.products.consumer.nursing-homes.domain'*/
])
  .config(config)
  .run(run);


/**
 * Module config
 */
config.$inject = ['$stateProvider'];
function config($stateProvider) {
  $stateProvider.state('top.consumer.nursing-homes', {
    abstract: true,
    url: '/nursing-homes'
  });

  $stateProvider.state('top.consumer.nursing-homes.location', {
    url: '/location?location&distance',
    data: {
      pageTitle: 'Compare Nursing Homes Search',
      report: 'ba52b7b2-f4c8-4831-b910-1d036b94ae75'
    },
    views: {
      'content@': {
        templateUrl: 'app/products/consumer/nursing-homes/views/location.html',
        controller: 'CNHLocationCtrl',
        resolve: {
          zipDistances: function(ResourceSvc) {
            return ResourceSvc.getZipDistances();
          }
        }
      }
    }
  });

  $stateProvider.state('top.consumer.nursing-homes.location-map', {
    url: '/location-map?location&distance',
    data: {
      pageTitle: 'Compare Nursing Homes Search Map',
      report: 'ba52b7b2-f4c8-4831-b910-1d036b94ae75'
    },
    views: {
      'content@': {
        templateUrl: 'app/products/consumer/nursing-homes/views/location_map.html',
        controller: 'CNHLocationMapCtrl',
        resolve: {
          zipDistances: function(ResourceSvc) {
            return ResourceSvc.getZipDistances();
          }
        }
      }
    }
  });

  $stateProvider.state('top.consumer.nursing-homes.compare', {
    url: '/compare?ids',
    data: {
      pageTitle: 'Compare Nursing Homes',
      report: 'f2f2b7fe-8653-488b-8ed8-fd4417cd0f9e'
    },
    views: {
      'content@': {
        templateUrl: 'app/products/consumer/nursing-homes/views/compare.html',
        controller: 'CNHCompareCtrl',
        resolve: {
          nursingHomes: function(ResourceSvc) {
            return ResourceSvc.getNursingHomes();
          },
          measureTopics: function(ResourceSvc) {
            return ResourceSvc.getNursingHomeMeasureTopicsConsumer();
          }
        }
      }
    }
  });

  $stateProvider.state('top.consumer.nursing-homes.profile', {
    url: '/profile/:id',
    data: {
      pageTitle: 'Nursing Home Profile',
      report: '87e04110-46b0-4cae-9592-022c3111fac7'
    },
    views: {
      'content@': {
        templateUrl: 'app/products/consumer/nursing-homes/views/profile.html',
        controller: 'CNHProfileCtrl',
        resolve: {
          zipDistances: function (ResourceSvc) {
            return ResourceSvc.getZipDistances();
          },
          hospitalZips: function (ResourceSvc) {
            return ResourceSvc.getHospitalZips();
          }
        }
      },
      'profile-tab1@top.consumer.nursing-homes.profile': {
        templateUrl: 'app/products/consumer/nursing-homes/views/profile_measures.html',
        controller: 'CNHProfileMeasuresCtrl',
        resolve: {
          measureTopics: function (ResourceSvc) {
            return ResourceSvc.getNursingHomeMeasureTopicsConsumer();
          }
        }
      }
    }
  });

  $stateProvider.state('top.consumer.nursing-homes.infographic', {
    url: '/infographic',
    data: {
      pageTitle: 'Nursing Homes Infographic'
    },
    views: {
      'content@': {
        templateUrl: 'app/products/consumer/nursing-homes/views/infographic.html',
        controller: 'CNHInfographicCtrl',
        resolve: {
          'report': function(InfographicReportSvc) {
            return InfographicReportSvc.getNursingHomeReport();
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
