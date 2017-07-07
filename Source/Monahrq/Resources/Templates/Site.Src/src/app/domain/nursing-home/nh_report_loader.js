/**
 * Monahrq Nest
 * Core Domain Module
 * Nursing Home Report Loader Service
 *
 * This service loads nursing home data use the Simple Report Loader. It provides single
 * and bulk loaders for the following quality data:
 *
 * - Nursing home ratings, all homes for a given measure
 * - Nursing home ratings, all measures for a given nursing home
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('NHReportLoaderSvc', NHReportLoaderSvc);


  NHReportLoaderSvc.$inject = ['$q', 'SimpleReportLoaderSvc'];
  function NHReportLoaderSvc($q, SimpleReportLoaderSvc) {
    /**
     * Private Data
     */
    $.monahrq = $.monahrq || {};
    $.monahrq.NursingHomes = $.monahrq.NursingHomes || {};
    $.monahrq.NursingHomes.Report = $.monahrq.NursingHomes.Report || {};
    $.monahrq.NursingHomes.Report.Measures = $.monahrq.NursingHomes.Report.Measures || [];
    $.monahrq.NursingHomes.Report.NursingHomes = $.monahrq.NursingHomes.Report.NursingHomes || [];

    var config = {
      measure: {
        rootObj: $.monahrq.NursingHomes.Report,
        reportName: 'Measures',
        reportDir: 'Data/NursingHomes/Measures/',
        filePrefix: 'Measure_'
      },
      nursingHome: {
        rootObj: $.monahrq.NursingHomes.Report,
        reportName: 'NursingHomes',
        reportDir: 'Data/NursingHomes/NursingHomes/',
        filePrefix: 'NursingHome_'
      }
    };



    /**
     * Service Interface
     */
    return {
      getNursingHomeReport: getNursingHomeReport,
      getNursingHomeReports: getNursingHomeReports,
      getMeasureReport: getMeasureReport,
      getMeasureReports: getMeasureReports
    };


    /**
     * Service Implementation
     */
    function getNursingHomeReport(id) {
      return SimpleReportLoaderSvc.load(config.nursingHome, id);
    }

    function getNursingHomeReports(ids) {
      return SimpleReportLoaderSvc.bulkLoad(config.nursingHome, ids);
    }

    function getMeasureReport(id) {
      return SimpleReportLoaderSvc.load(config.measure, id);
    }

    function getMeasureReports(ids) {
      return SimpleReportLoaderSvc.bulkLoad(config.measure, ids);
    }

  }

})();
