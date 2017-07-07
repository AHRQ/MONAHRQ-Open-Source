/**
 * Consumer Product
 * Hospital Reports Module
 * Profile Physicians Panel Controller
 *
 * This controller shows all physicians affiliated with the hospital, grouped by
 * their specialty.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.hospitals')
    .controller('CHProfilePhysiciansCtrl', CHProfilePhysiciansCtrl);

  CHProfilePhysiciansCtrl.$inject = ['$stateParams', '$scope', 'ResourceSvc', 'PhysicianAPISvc', 'PhysicianReportLoaderSvc', 'PhysicianDedupeSvc', 'hospitals'];
  function CHProfilePhysiciansCtrl($stateParams, $scope, ResourceSvc, PhysicianAPISvc, PhysicianReportLoaderSvc, PhysicianDedupeSvc, hospitals) {
    var _physicians = [], visibleSpecialties = {};
    var hospitalId = +$stateParams.id;

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
            ResourceSvc.getHospitalProfile(hospitalId)
              .then(function(h) {
                PhysicianAPISvc.init(config);
                PhysicianAPISvc.findByHospAfl(h.HospitalProviderID)
                  .then(function (physicians) {
                    _physicians = PhysicianDedupeSvc.dedupe(physicians, PhysicianDedupeSvc.DEDUPE_MERGE);
                    updateModel();
                  });
              });
          }
          else {
            ResourceSvc.getPhysicianHospitalAffiliation()
            .then(function(affiliations) {
              var h = _.findWhere(hospitals, {Id: hospitalId});
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

