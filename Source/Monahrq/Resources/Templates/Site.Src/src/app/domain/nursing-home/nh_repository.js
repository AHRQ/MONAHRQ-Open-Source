/**
 * Monahrq Nest
 * Core Domain Module
 * Nursing Home Report Repository Service
 *
 * This service provides a mix of loader and search functions for nursing homes.
 * It can load nursing home profile data by name or id, by type or rating, or
 * perform geographic * searches for homes by zip code, county, or street address.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('NHRepositorySvc', NHRepositorySvc);


  NHRepositorySvc.$inject = ['$q', '$rootScope', 'ResourceSvc', 'NHReportLoaderSvc', 'ZipDistanceSvc', 'MapQuestSvc'];
  function NHRepositorySvc($q, $rootScope, ResourceSvc, NHReportLoaderSvc, ZipDistanceSvc, MapQuestSvc) {
    var index = [], zipCache = [], zipQuery, HospitalZips = [];

    /**
     * Service Interface
     */
    return {
      init: init,
      getProfile: getProfile,
      getProfiles: getProfiles,
      findByAddress: findByAddress,
      findByCounty: findByCounty,
      findByZip: findByZip,
      findByType: findByType,
      findByInHospital: findByInHospital,
      findByMeasureRating: findByMeasureRating,
      findNear: findNear,
      findByName: findByName,
      all: all
    };



    /**
     * Service Implementation
     */
    function init() {
      var deferred = $q.defer();

      MapQuestSvc.init($rootScope.config.website_MapquestApiKey, $rootScope.config.website_States);

      _loadIndex()
        .then(function() {
          deferred.resolve();
        });

      return deferred.promise;
    }

    function getProfile(id) {
      return ResourceSvc.getNursingHomeProfile(id);
    }

    function getProfiles(ids) {
      var deferred, promises;
      deferred = $q.defer();

      promises = _.map(ids, function (id) {
        return getProfile(id);
      });

      $q.all(promises).then(function (profiles) {
          deferred.resolve(profiles);
        },
        function (reason) {
          deferred.reject(reason);
        });

      return deferred.promise;
    }

    function all() {
      var deferred = $q.defer();

      var result =_.filter(index, function(row) { return true; });
      deferred.resolve(result);

      return deferred.promise;
    }

    function findByAddress(address, city, zip, distance) {
      return _.filter(index, function(row) {
        return row.Zip == zip;
      });
    }

    function findByCounty(id) {
      var deferred = $q.defer();

      var result =_.filter(index, function(row) {
        return row.CountyID === id;
       });

      deferred.resolve(result);

      return deferred.promise;
    }

    function findByZip(zip, distance) {
      var deferred = $q.defer();

      ResourceSvc.getHospitalZips()
        .then(function(data) {
          HospitalZips = data;
          var zips = getZipCodesByDistance(zip, distance);

          var result =_.filter(index, function(row) {
            return _.contains(zips, row.Zip);
          });

          deferred.resolve(result);
        });

      return deferred.promise;
    }

    function findByType(id) {
      var deferred = $q.defer();

      if (id == 999) id = -1;

      var result = _.filter(index, function(row) {
        return row.TypeID === id;
      });

      deferred.resolve(result);

      return deferred.promise;
    }

    function findByInHospital(inHospital) {
      var deferred = $q.defer();

      var result = _.filter(index, function(row) {
        return row.InHospital === inHospital;
      });

      deferred.resolve(result);

      return deferred.promise;
    }

    function findByMeasureRating(measureId, rating) {
      return NHReportLoaderSvc.getMeasureReport(measureId)
        .then(function(report) {
          return _.map(
            _.filter(report.data, function(row) {
              return row.NatRating >= rating;
            }),
            function(match) {
              // match the output format of the other finds
              return {
                ID: match.NursingHomeID
              };
            });
        });
    }

    function findNearDemo() {
      return _.map(_.take(index, 10), function(row) {
        return {
          distance: 0,
          nursingHome: row
        };
      });
    }

    function findNear(location, distance) {
      var deferred;
      deferred = $q.defer();

      if ($rootScope.config.DE_IDENTIFICATION) {
        deferred.resolve(findNearDemo());
        return deferred.promise;
      }

      MapQuestSvc.geocode(location)
        .then(function(data) {
          if (data.length == 0) {
            deferred.resolve([]);
            return;
          }

          var geo = (_.first(data)).latLng;

          var hs = _.filter(_.map(index, function(row) {
            if (!_.has(row, 'LatLng')) return {};
            return {
              distance: ZipDistanceSvc.calcDist(geo.lat, geo.lng, row.LatLng[0], row.LatLng[1]),
              nursingHome: row
            }
          }), function(row) {
            return _.has(row, 'distance') && row.distance <= distance;
          });

          deferred.resolve(hs);
        });

      return deferred.promise;
    }

    function findByName(name) {
      var deferred;
      deferred = $q.defer();
      var term = name ? name.toUpperCase() : "";

      var results = _.filter(index, function(row) {
        return row.Name.toUpperCase().indexOf(term) >= 0;
      });

      deferred.resolve(results);

      return deferred.promise;
    }

    function _loadIndex() {
      return ResourceSvc.getNursingHomes()
        .then(function(result) {
          index = result;
        });
    }

    function getZipCodesByDistance(zip, distance) {
      var zk = zip + '|' + distance;

      if (zk === zipQuery) {
        return zipCache;
      }

      var hcoords = _.findWhere(HospitalZips, {Zip: zip});

      if (!hcoords) {
        return [];
      }

      var zips = _.filter(HospitalZips, function(z) {
        var dist = ZipDistanceSvc.calcDist(hcoords.Latitude, hcoords.Longitude, z.Latitude, z.Longitude);
        return dist <= distance;
      });

      zipQuery = zk;
      zipCache = _.pluck(zips, 'Zip');

      return zipCache;
    }

  }

})();
