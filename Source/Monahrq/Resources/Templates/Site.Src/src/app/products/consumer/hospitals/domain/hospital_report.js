/**
 * Consumer Product
 * Hospital Reports Module
 * Hospital Report Svc
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.hospitals')
    .factory('CHReportSvc', CHReportSvc);


  CHReportSvc.$inject = ['$q', 'HospitalRepositorySvc', 'HospitalReportLoaderSvc'];
  function CHReportSvc($q, HospitalRepositorySvc, HospitalReportLoaderSvc) {
    /**
     * Service Interface
     */
    return {
      getHospitalOverviewReport: getHospitalOverviewReport,
      getHospitalMeasuresReport: getHospitalMeasuresReport
    };


    /**
     * Service Implementation
     */
    function getHospitalOverviewReport(hospitalIds, measureId) {
      var profiles, report, d;

      d = $q.defer();

      HospitalRepositorySvc.init()
        .then(function() {
          HospitalRepositorySvc.getProfiles(hospitalIds)
            .then(function (_profiles) {
              profiles = _profiles;
              return HospitalReportLoaderSvc.getQualityByMeasureReport(measureId);
            })
            .then(function (_report) {
              return report = _report.data;
            })
            .then(function() {
              if (report) {
                d.resolve(buildRows());
              }
              else {
                d.resolve(buildRowsNoMeasure());
              }
            });
        });

      return d.promise;

      /*
       * Internal Functions
       */
      function buildRowsNoMeasure() {
        /*var idIndex = _.object(hospitalIds, []);

        var reportRows = _.filter(report, function (row) {
          return _.has(idIndex, row.HospitalID);
        });

        var rows = _.map(reportRows, function (row) {
          return {
            id: row.HospitalID,
            natRating: row.NatRating,
            peerRating: row.PeerRating
          }
        });*/

        var rows = _.map(profiles, function (profile) {
          var row = {};
          //var profile = _.findWhere(profiles, {id: row.id});
          //if (profile == undefined) return;

          row.id = profile.id;

          row.name = profile.name;
          row.city = profile.city;
          row.state = profile.state;
          row.displayAddress = profile.diaplayAddress; // typo is in data format...
          row.totalBeds = profile.totalBeds;
          row.phoneNumber = profile.phoneNumber;

          var types = _.pluck(profile.types, 'type_Name');
          row.hospitalType = types.join(', ');

          row.parentOrganizationName = profile.parentOrganizationName;

          row.LatLng = profile.LatLng;

          return row;
        });

        return rows;
      }

      function buildRows() {
        var idIndex = _.object(hospitalIds, []);

        var reportRows = _.filter(report, function (row) {
          return _.has(idIndex, row.HospitalID);
        });

        var rows = _.map(reportRows, function (row) {
          return {
            id: row.HospitalID,
            natRating: row.NatRating,
            peerRating: row.PeerRating
          }
        });

        _.each(rows, function (row) {
          var profile = _.findWhere(profiles, {id: row.id});
          if (profile == undefined) return;

          row.name = profile.name;
          row.city = profile.city;
          row.state = profile.state;
          row.displayAddress = profile.diaplayAddress; // typo is in data format...
          row.totalBeds = profile.totalBeds;
          row.phoneNumber = profile.phoneNumber;

          var types = _.pluck(profile.types, 'type_Name');
          row.hospitalType = types.join(', ');

          row.parentOrganizationName = profile.parentOrganizationName;

          row.LatLng = profile.LatLng;
        });

        return rows;
      }
    }


    function getHospitalMeasuresReport(measureIds, zip, distance) {
      var d = $q.defer();

      HospitalRepositorySvc.init()
        .then(loadReport);

      return d.promise;


      function loadReport() {
        var reports = {};

        HospitalReportLoaderSvc.getQualityByMeasureReports(measureIds)
          .then(function (_reports) {
            _.each(_reports, function (report) {
              if (report.data) {
                reports[report.data[0].MeasureID] = report;
              }
            });
          })
          .then(function() {
            var model = buildRows(reports);
            d.resolve(model);
          })

      }

      /*
       * Internal Functions
       */
      function buildRows(reports) {
        var tempModel = {};
        var nearbyHospitals = {};
        var limitDistance = false;

        if (zip && distance) {
          limitDistance = true;
          _.each(HospitalRepositorySvc.findNearZip(zip, distance), function(h) {
            nearbyHospitals[h.hospital.Id] = h;
          });
        }

        _.each(measureIds, function(measureId) {
          if (!_.has(reports, measureId)) {
            return;
          }

          var report = reports[measureId].data;

          _.each(report, function(row) {
            if (limitDistance) {
              if (!_.has(nearbyHospitals, row.HospitalID)) {
                return;
              }
            }

            if (!_.has(tempModel, row.HospitalID)) {
              var profile;
              if (limitDistance) {
                var h = nearbyHospitals[row.HospitalID];
                profile = h.hospital;
              }
              else {
                profile = HospitalRepositorySvc.getIndexRecord(row.HospitalID);
              }

              var r = {
                id: profile.Id,
                name: profile.Name,
                city: profile.City,
                state: profile.State,
                zip: profile.Zip,
                distance: h ? Math.floor(h.distance) : null,
                LatLng: profile.LatLng
              };
              tempModel[row.HospitalID] = r;
            }

            tempModel[row.HospitalID][measureId] = row;
          })
        });

        return _.values(tempModel);
      }

    }

  }

})();
