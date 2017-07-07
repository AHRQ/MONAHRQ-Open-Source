/**
 * Consumer Product
 * Nursing Home Reports Module
 * Nursing Home Report Svc
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.nursing-homes')
    .factory('CNHReportSvc', CNHReportSvc);


  CNHReportSvc.$inject = ['$q', 'ResourceSvc', 'NHRepositorySvc', 'NHReportLoaderSvc'];
  function CNHReportSvc($q, ResourceSvc, NHRepositorySvc, NHReportLoaderSvc) {
    /**
     * Service Interface
     */
    return {
      getNursingHomeOverviewReport: getNursingHomeOverviewReport
    };


    /**
     * Service Implementation
     */
    function getNursingHomeOverviewReport(nhIds, measureIds) {
      var profiles, reports, d;

      d = $q.defer();

      NHRepositorySvc.init()
        .then(function () {
          NHRepositorySvc.getProfiles(nhIds)
            .then(function (_profiles) {
              profiles = _profiles;
              return NHReportLoaderSvc.getMeasureReports(measureIds);
            })
            .then(function (_reports) {
              return reports = _reports
            })
            .then(function () {
              d.resolve(buildRows());
            });
        });

      return d.promise;

      /*
       * Internal Functions
       */
      function buildRows() {
        var rows = _.object(nhIds, _.map(profiles, function(profile) {
          return {
            id: profile.ID,
            name: profile.Name,
            displayAddress: profile.DisplayAddress,
            city: profile.City,
            state: profile.State,
            LatLng: profile.LatLng
          };
        }));

        _.each(reports, function(report) {
          _.each(report.data, function(row) {
            if (_.has(rows, row.NursingHomeID)) {
              rows[row.NursingHomeID][""+report.id] = row;
            }
          });
        });

        rows = _.values(rows);

        return rows;
      }
    }

  }

})();
