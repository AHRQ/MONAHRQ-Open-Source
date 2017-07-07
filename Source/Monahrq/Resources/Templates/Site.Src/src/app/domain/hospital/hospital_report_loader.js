/**
 * Monahrq Nest
 * Core Domain Module
 * Hospital Report Loader Service
 *
 * This service loads hospital data use the Simple Report Loader. It provides single
 * and bulk loaders for the following quality data:
 *
 * - Hospital Quality, all hospitals for a given measure
 * - Hospital Quality, all measures for a given hospital
 * - Cost Quality, all measures for a given hospital
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('HospitalReportLoaderSvc', HospitalReportLoaderSvc);


  HospitalReportLoaderSvc.$inject = ['$q', 'SimpleReportLoaderSvc'];
  function HospitalReportLoaderSvc($q, SimpleReportLoaderSvc) {
    /**
     * Private Data
     */
    $.monahrq = $.monahrq || {};
    $.monahrq.qualitydata = $.monahrq.qualitydata || {};
    $.monahrq.costdata = $.monahrq.costdata || {};

    var config = {
      measure: {
        rootObj: $.monahrq.qualitydata,
        reportPrefix: 'measure_',
        reportDir: 'Data/QualityRatings/Measure/',
        filePrefix: 'measure_',
        idInKey: true
      },
      hospital: {
        rootObj: $.monahrq.qualitydata,
        reportPrefix: 'hospital_',
        reportDir: 'Data/QualityRatings/Hospital/',
        filePrefix: 'hospital_',
        idInKey: true
      },
      costQuality: {
        rootObj: $.monahrq.costdata,
        reportPrefix: 'hospital_',
        reportDir: 'Data/CostQualityRatings/Hospital/',
        filePrefix: 'Hospital_',
        idInKey: true
      }
    };



    /**
     * Service Interface
     */
    return {
      getQualityByHospitalReport: getQualityByHospitalReport,
      getQualityByHospitalReports: getQualityByHospitalReports,
      getQualityByMeasureReport: getQualityByMeasureReport,
      getQualityByMeasureReports: getQualityByMeasureReports,
      getCostQualityByHospitalReport: getCostQualityByHospitalReport,
      getCostQualityByHospitalReports: getCostQualityByHospitalReports
    };


    /**
     * Service Implementation
     */
    function getQualityByHospitalReport(id) {
      return SimpleReportLoaderSvc.load(config.hospital, id);
    }

    function getQualityByHospitalReports(ids) {
      return SimpleReportLoaderSvc.bulkLoad(config.hospital, ids);
    }

    function getQualityByMeasureReport(id) {
      return SimpleReportLoaderSvc.load(config.measure, id);
    }

    function getQualityByMeasureReports(ids) {
      return SimpleReportLoaderSvc.bulkLoad(config.measure, ids);
    }

    function getCostQualityByHospitalReport(id) {
      return SimpleReportLoaderSvc.load(config.costQuality, id);
    }

    function getCostQualityByHospitalReports(ids) {
      return SimpleReportLoaderSvc.bulkLoad(config.costQuality, ids);
    }
  }

})();
