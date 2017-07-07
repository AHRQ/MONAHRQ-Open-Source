/**
 * Consumer Product
 * Physician Report Module
 * Setup
 *
 * This configures the page states used by the physician section of the consumer website.
 */

(function() {
  'use strict';

  /**
   * Angular Wiring
   */

  angular.module('monahrq.products.consumer.physicians', [
/*    'monahrq.products.consumer.physicians.domain'*/
  ])
    .config(config)
    .run(run);

  /**
   * Module Config
   */

  config.$inject = ['$stateProvider'];
  function config($stateProvider) {
    $stateProvider.state('top.consumer.physicians', {
      abstract: true,
      url: '/physicians'
    });

    $stateProvider.state('top.consumer.physicians.search', {
      url: '/search?searchType&firstName&lastName&practiceName&location&specialty&condition',
      data: {
        pageTitle: 'Find Doctors',
        report: 'e007bb9c-e539-41d6-9d06-ff52f8a15bf6'
      },
      views: {
       'content@': {
         templateUrl: 'app/products/consumer/physicians/views/search.html',
         controller: 'PSearchCtrl',
         resolve: {
            physicianSpecialty: function(ResourceSvc) {
              return ResourceSvc.getPhysicianSpecialties();
            }
         }
       }
     }
    });

    $stateProvider.state('top.consumer.physicians.profile', {
      url: '/profile/:id',
      data: {
        pageTitle: 'Doctor Profile',
        report: '4c5727b4-0e85-4f80-ade9-418b49a1373e'
      },
      views: {
        'content@': {
          templateUrl: 'app/products/consumer/physicians/views/profile.html',
          controller: 'PProfileCtrl',
          resolve: {
            hospitals: function(ResourceSvc) {
              return ResourceSvc.getHospitals();
            }
          }
        },
        'physician-profile-location@top.consumer.physicians.profile': {
          templateUrl: 'app/products/consumer/physicians/views/profile_location.html'
        },
        'physician-profile-detail@top.consumer.physicians.profile': {
          templateUrl: 'app/products/consumer/physicians/views/profile_detail.html'
        }
      }
    });

  }

  function run() {

  }
})();
