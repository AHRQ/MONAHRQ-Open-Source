/**
 * Monahrq Nest
 * Core Domain Module
 * Hospital Repository Service
 *
 * This service provides a mix of loader and search functions for hospitals. It
 * can load hospital profile data by name or id, as well as perform geographic
 * searches for hospitals by zip code or street address.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('HospitalRepositorySvc', HospitalRepositorySvc);


  HospitalRepositorySvc.$inject = ['$q', '$rootScope', 'ResourceSvc', 'MapQuestSvc', 'ZipDistanceSvc'];
  function HospitalRepositorySvc($q, $rootScope, ResourceSvc, MapQuestSvc, ZipDistanceSvc) {
    var index = [], HospitalZips = [];

    /**
     * Service Interface
     */
    return {
      init: init,
      getProfile: getProfile,
      getProfiles: getProfiles,
      getIndexRecord: getIndexRecord,
      getIndexRecords: getIndexRecords,
      findNear: findNear,
      findNearZip: findNearZip,
      findByName: findByName
    };



    /**
     * Service Implementation
     */
    function init() {
      var deferred = $q.defer();

      MapQuestSvc.init($rootScope.config.website_MapquestApiKey, $rootScope.config.website_States);

      _loadIndex()
        .then(function() {
          return ResourceSvc.getHospitalZips()
            .then(function(z) {
              HospitalZips = z;
            });
        })
        .then(function() {
          deferred.resolve();
        });

      return deferred.promise;
    }

    function getIndexRecord(id) {
      return _.findWhere(index, {Id: id});
    }

    function getIndexRecords(ids) {
      return _.filter(index, function(row) {
        return _.contains(ids, row.Id);
      });
    }

    function getProfile(id) {
      return ResourceSvc.getHospitalProfile(id);
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

    function findNearDemo() {
      return _.map(_.take(index, 10), function(row) {
        return {
          distance: 0,
          hospital: row
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
              hospital: row
            }
          }), function(row) {
            return _.has(row, 'distance') && row.distance <= distance;
          });

          deferred.resolve(hs);
        });

      return deferred.promise;
    }

    function findNearZip(zip, distance) {
      var hcoords = _.findWhere(HospitalZips, {Zip: zip});
      if (hcoords == null || hcoords.length == 0) return [];

      var hospitals = _.filter(_.map(index, function(row) {
        var d = null;
        if (_.has(row, 'LatLng')) {
          d = ZipDistanceSvc.calcDist(hcoords.Latitude, hcoords.Longitude, row.LatLng[0], row.LatLng[1]);
        }

        return {
          hospital: row,
          distance: d
        };
      }), function(h) {
        return h.distance && h.distance <= distance;
      });

      return hospitals;
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
      return ResourceSvc.getHospitals()
        .then(function(result) {
          index = result;
        });
    }
  }

})();
