/**
 * Monahrq Nest
 * Components Modals Module
 * Measure Modal
 *
 * Display measure help content for the specified reporting domain.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.components.modals')
    .factory('ModalMeasureSvc', ModalMeasureSvc);

  ModalMeasureSvc.$inject = ['$modal', '$rootScope'];
  function ModalMeasureSvc($modal, $rootScope) {
    /**
     * Private Data
     */
    var modalInstance;
    var activeEl;

    var SAMap = {
      'Area RA': 'Numerator, Denominator, Observed Rate, Risk Adjusted Rate',
      'Area': 'Numerator, Denominator, Observed Rate, Risk Adjusted Rate',
      'Provider RA': 'Numerator, Denominator, Observed Rate and CI, Expected Rate, Risk-Adjusted Rate and CI',
      'Provider': 'Numerator, Denominator, Observed Rate and CI',
      'Composite': 'Composite Ratio and CI',
      'Volume': 'Volume of Procedures',
      'Process': 'Denominator, Observed Rate',
      'Outcome': 'Denominator, Risk-Adjusted Rate and CI',
      'Structural': 'Observed (by answer type), Response Rate (collapsed)',
      'Binary': 'Observed (by answer type), Response Rate (collapsed)',
      'YNM': 'Observed (by answer type), Response Rate (collapsed)',
      'Scale': 'Observed (by answer type), Response Rate (collapsed)',
      'Categorical': 'Observed (by answer type), Response Rate (collapsed)',
      'CMS Ratio': 'Measure Ratio'
    };

    /**
     * Service Interface
     */
    return {
      open: openHospitalMeasure,
      openHospitalMeasure: openHospitalMeasure,
      openNursingMeasure: openNursingMeasure,
      openCostQualityMeasure: openCostQualityMeasure,
      openPhysicianMeasure: openPhysicianMeasure,
      openPhysicianHedisMeasure: openPhysicianHedisMeasure
    };

    /**
     * Service Implementation
     */
    function getFMeasureSource(src) {
      var fms = src;

      if (src == 'AHRQ') {
        fms = src + ' Quality Indicator';
      }
      else if (src == 'CMS') {
        fms = src + ' Hospital Compare';
      }

      return fms;
    }

    function getFStatisticsAvailable(sa) {
      var fsa = sa;

      if (_.has(SAMap, sa)) {
        fsa = SAMap[sa];
      }

      return fsa;
    }

    function openCostQualityMeasure(id) {
      open('costQuality', id);
    }

    function openNursingMeasure(id) {
      open('nursing', id);
    }

    function openHospitalMeasure(id) {
      open('hospital', id);
    }

    function openPhysicianMeasure(id) {
      open('physician', id);
    }

    function openPhysicianHedisMeasure(id) {
      open('physicianHedis', id);
    }

    function open(source, id) {
      $rootScope.modalOpen = true;
      saveFocus();

      modalInstance = $modal.open({
        templateUrl: 'app/components/modals/views/modal_measure_mobile.html',
        windowTemplateUrl: 'app/components/modals/views/modal_window.html',
        controller: function($scope, $state, measure) {
          $scope.source = source;
          $scope.measure= _.isArray(measure) ? measure[0] : measure;
          $scope.measure.fMeasureSource = getFMeasureSource($scope.measure.MeasureSource);
          $scope.measure.fStatisticsAvailable = getFStatisticsAvailable($scope.measure.StatisticsAvailable);

          $scope.isConsumer = function() {
            return $state.includes('top.consumer');
          };

          $scope.isProfessional = function() {
            return $state.includes('top.professional');
          };

          $scope.gotoAboutQR = function() {
            $scope.$close();

            if ($state.includes('top.professional')) {
              if (source === 'hospital') {
                $state.go('top.professional.resources', {page: 'AboutQualityRatings'});
              }
              else if (source === 'nursing') {
                $state.go('top.professional.resources', {page: 'NHAboutQualityRatings'});
              }
            }
            else if ($state.includes('top.consumer')) {
              if (source === 'hospital') {
                $state.go('top.consumer.about-ratings', {page: 'resourceHospitalQuality'});
              }
              else if (source === 'nursing') {
                $state.go('top.consumer.about-ratings', {page: 'resourceNursingHomeQuality'});
              }
            }
          };
        },
        resolve: {
          measure: function(ResourceSvc) {
            if (source === 'hospital') {
              return ResourceSvc.getMeasureDef(id);
            }
            else if (source === 'nursing') {
              return ResourceSvc.getNursingHomeMeasure(id);
            }
            else if (source === 'costQuality') {
              return ResourceSvc.getCostQualityMeasure(id);
            }
            else if (source === 'physician') {
              return ResourceSvc.getMedicalPracticeMeasure(id);
            }
            else if (source === 'physicianHedis') {
              return ResourceSvc.getPhysicianMeasure(id);
            }
          }
        }
      });

      modalInstance.result.then(restoreFocus, restoreFocus);
    }

    function saveFocus() {
      activeEl = document.activeElement;
    }

    function restoreFocus() {
      $rootScope.modalOpen = false;
      if (activeEl) {
        activeEl.focus();
      }
    }
  }

})();
