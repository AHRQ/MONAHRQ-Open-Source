/**
 * Consumer Products
 * Pages Module
 * Profile Search Results Page
 *
 * This controller runs by-name searches for all three entity types: hospitals,
 * nursing homes, and physicians.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.pages')
    .controller('CProfileSearchCtrl', CProfileSearchCtrl);


  CProfileSearchCtrl.$inject = ['$scope', '$state', '$stateParams', '$q', 'HospitalRepositorySvc', 'NHRepositorySvc', 'PhysicianRepositorySvc', 'ScrollToElSvc', 'ConsumerReportConfigSvc'];
  function CProfileSearchCtrl($scope, $state, $stateParams, $q, HospitalRepositorySvc, NHRepositorySvc, PhysicianRepositorySvc, ScrollToElSvc, ConsumerReportConfigSvc) {
    var entityNames = {
      'hospital': 'hospital',
      'nursing': 'nursing home',
      'physician': 'doctor'
    };
    $scope.showValidationErrors = false;

    $scope.query = {};
    $scope.search = search;
    $scope.hospitalResults = [];
    $scope.nursingHomeResults = [];
    $scope.physicianResults = [];
    $scope.hasSearch = false;
    $scope.hasResult = false;
    $scope.share = share;
    $scope.feedbackModal = feedbackModal;

    init();


    function init() {
      $scope.query.name = $stateParams.term;
      $scope.query.type = $stateParams.type;
      $scope.entityName = entityNames[$scope.query.type];


      if ($scope.query.type && $scope.query.name) {
        $scope.hasSearch = true;
        var ps = [];

        if (ConsumerReportConfigSvc.hasEntity(ConsumerReportConfigSvc.entities.HOSPITAL)) {
          ps.push(HospitalRepositorySvc.init());
        }
        if (ConsumerReportConfigSvc.hasEntity(ConsumerReportConfigSvc.entities.NURSINGHOME)) {
          ps.push(NHRepositorySvc.init());
        }

        $q.all(ps)
          .then(loadData);
      }
    }

    function share() {
        window.location = buildShareUrl();
    }

    function feedbackModal() {
        ModalFeedbackSvc.open($scope.config);
    }

    function buildShareUrl() {
        var url = escape(window.location);
        return "mailto:?to=&subject=Shared%20MONAHRQ%20Page&body=" + url;
    }

    function loadData() {
      var n = $scope.query.name;
      var type = $scope.query.type;

      if (type === 'hospital') {
        HospitalRepositorySvc.findByName(n)
          .then(processHospital);
      }
      else if (type === 'nursing') {
        NHRepositorySvc.findByName(n)
          .then(processNursing);
      }
      else if ( type === 'physician') {
        var first = '', last = '';
        var parts = n.split(' ');
        if (parts.length == 1) {
          last = parts[0];
        }
        else if (parts.length == 2) {
          first = parts[0];
          last = parts[1];
        }

        PhysicianRepositorySvc.findByName(first, last)
          .then(processPhysician);
      }
    }

    function processHospital(result) {
      if (result.length === 0) return;
      ScrollToElSvc.scrollToEl('.profile .report');

      $scope.hasResult = true;

      $scope.hospitalResults = _.map(result, function(row) {
        return {
          id: row.Id,
          name: row.Name,
          city: row.City,
          state: row.State
        }
      });
    }

    function processNursing(result) {
      if (result.length === 0) return;
      ScrollToElSvc.scrollToEl('.profile .report');

      $scope.hasResult = true;

      $scope.nursingHomeResults = _.map(result, function(row) {
        return {
          id: row.ID,
          name: row.Name,
          city: row.City,
          state: row.State
        }
      });

    }

    function processPhysician(result) {
      if (result.length === 0) return;
      ScrollToElSvc.scrollToEl('.profile .report');

      $scope.hasResult = true;

      $scope.physicianResults = _.map(result, function(row) {
        return {
          id: row.npi,
          name: row.frst_nm + ' ' + row.lst_nm,
          city: row.cty,
          state: row.st
        }
      });
    }

    function canSearch() {
      return $scope.query.name;
    }

    function search() {
      if (!canSearch()) {
        $scope.showValidationErrors = true;
        return;
      }
      $state.go('top.consumer.profile-search', {type: $scope.query.type, term: $scope.query.name});
    }

  }

})();

