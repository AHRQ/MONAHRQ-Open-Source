/**
 * Professional Product
 * Physician Report Module
 * Setup
 *
 * This configures the page states used by the physician section of the professional website.
 */

(function() {
  'use strict';

  /**
   * Angular Wiring
   */

  angular.module('monahrq.products.professional.physicians', [
    'monahrq.products.professional.physicians.domain'
  ])
    .config(config)
    .run(run);

  /**
   * Module Config
   */

  config.$inject = ['$stateProvider'];

  function config($stateProvider) {
    $stateProvider.state('top.professional.physicians', {
      url: '/physicians',
      data: {
        pageTitle: 'Physicians'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/professional/physicians/views/index.html',
          controller: 'PhysiciansCtrl'
        }
      }
    });

    $stateProvider.state('top.professional.physicians.find-physician', {
      url: '/find-physician?searchType&firstName&lastName&practiceName&city&zip&specialty&condition',
      data: {
        pageTitle: 'Find Physician',
        report: 'e007bb9c-e539-41d6-9d06-ff52f8a15bf6'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/professional/physicians/views/physician-find.html',
          controller: 'PhysicianFindCtrl'
        },
        'physician-find-search@top.professional.physicians.find-physician': {
          templateUrl: 'app/products/professional/physicians/views/search_physician.html',
          controller: 'PhysiciansSearchCtrl',
          resolve: {
            physicianSpecialty: function(ResourceSvc) {
              return ResourceSvc.getPhysicianSpecialties();
            }
          }
        },
        'physician-find-content@top.professional.physicians.find-physician': {
          templateUrl: 'app/products/professional/physicians/views/table_physician.html',
          controller: 'PhysicianTableCtrl',
          resolve: {
            physicianSpecialty: function(ResourceSvc) {
              return ResourceSvc.getPhysicianSpecialties();
            }
          }
        }
      }
    });

    $stateProvider.state('top.professional.physicians.profile', {
      url: '/profile/:id',
      data: {
        pageTitle: 'Physicians - Profile',
        report: '4c5727b4-0e85-4f80-ade9-418b49a1373e'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/professional/physicians/views/profile.html',
          controller: 'PhysiciansProfileCtrl',
          resolve: {
            hospitals: function(ResourceSvc) {
              return ResourceSvc.getHospitals();
            }
          }
        },
        'physician-profile-location@top.professional.physicians.profile': {
          templateUrl: 'app/products/professional/physicians/views/profile_location.html'
        },
        'physician-profile-detail@top.professional.physicians.profile': {
          templateUrl: 'app/products/professional/physicians/views/profile_detail.html'
        }
      }
    });
  }

  function run() {

  }
})();
