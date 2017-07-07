/**
 * Professional Product
 * Quality Ratings Module
 * Profile Page Physicians Tab Controller
 *
 * This controller shows all physicians affiliated with the hospital, grouped by
 * their specialty.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings')
    .controller('QRProfilePhysiciansCtrl', QRProfilePhysiciansCtrl);

  QRProfilePhysiciansCtrl.$inject = ['$scope', '$q', 'ResourceSvc', 'ReportConfigSvc', 'PhysicianAPISvc', 'PhysicianReportLoaderSvc', 'PhysicianDedupeSvc', 'SortSvc', 'hospitals'];
  function QRProfilePhysiciansCtrl($scope, $q, ResourceSvc, ReportConfigSvc, PhysicianAPISvc, PhysicianReportLoaderSvc, PhysicianDedupeSvc, SortSvc, hospitals) {
    var _physicians = [], visibleSpecialties = {};

    $scope.model = {};

    $scope.showSpecialty = showSpecialty;
    $scope.toggleSpecialty = toggleSpecialty;

    init();

    function init() {
      loadData();
    }

    function loadData() {
      ResourceSvc.getConfiguration()
        .then(function(config) {

          if (config.USED_REAL_TIME == 1) {
            PhysicianAPISvc.init(config);
            PhysicianAPISvc.findByHospAfl($scope.hospitalProfile.HospitalProviderID)
              .then(function (physicians) {
                _physicians = PhysicianDedupeSvc.dedupe(physicians, PhysicianDedupeSvc.DEDUPE_MERGE);
                updateModel();
              });
          }
          else {
            ResourceSvc.getPhysicianHospitalAffiliation()
            .then(function(affiliations) {
              var h = _.findWhere(hospitals, {Id: $scope.hospitalProfile.id});
              var npis = _.pluck(_.where(affiliations, {HospitalProviderID: h.HospitalProviderId}), 'Npi');
              PhysicianReportLoaderSvc.init(config);
              PhysicianReportLoaderSvc.findByNPIs(npis)
                .then(function(results) {
                  var results2 = _.flatten(_.map(results, function(r) {
                    return r.data || [];
                  }));
                  _physicians = PhysicianDedupeSvc.dedupe(results2, PhysicianDedupeSvc.DEDUPE_MERGE);
                  updateModel();
                });
            });
          }
        });
    }

    function updateModel() {
      $scope.model = _.groupBy(_physicians, 'pri_spec');
    }

    function showSpecialty(id) {
      return _.has(visibleSpecialties, id);
    }

    function toggleSpecialty(id) {
      if (showSpecialty(id)) {
        delete visibleSpecialties[id];
      }
      else {
        visibleSpecialties[id] = true;
      }
    }


  }

})();

