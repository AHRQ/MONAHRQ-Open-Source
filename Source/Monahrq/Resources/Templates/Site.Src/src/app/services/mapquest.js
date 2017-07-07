/**
 * Monahrq Nest
 * Services Module
 * MapQuest API Service
 *
 * Integrates with the MapQuest Geocoding API:
 * https://developer.mapquest.com/products/geocoding
 *
 * Given a free-form address, it will return its latitude and longitude,
 * along with other geo data that is not of interest.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.services')
    .factory('MapQuestSvc', MapQuestSvc);


  MapQuestSvc.$inject = ['$http', '$q'];
  function MapQuestSvc($http, $q) {
    var proto = 'https:' == document.location.protocol ? 'https://' : 'http://';
    var MQ_URL = proto + 'open.mapquestapi.com/';
    var MQ_GEOCODE_SVC = 'geocoding/v1/address';
    var apiKey, states;

    /**
     * Service Interface
     */
    return {
      init: init,
      geocode: geocode
    };


    /**
     * Service Implementation
     */
    function init(_apiKey, _states) {
      apiKey = _apiKey;
      states = _states ? _states : [];
      states = _.map(states, function(s) { return s.toUpperCase()});
    }

    function geocode(address) {
      var deferred = $q.defer();
      var url = MQ_URL + MQ_GEOCODE_SVC + '?key=' + apiKey;
      url += "&location=" + address;

      $http({method: 'GET', url: url}).
        success(function(data, status, headers, config) {
          deferred.resolve(extractGeoResults(data));
        }).
        error(function(data, status, headers, config) {
          deferred.resolve([]);
        });

      return deferred.promise;
    }

    function extractGeoResults(data) {
      var results = [];
      if (!_.has(data, 'results') || data.results.length == 0) return results;

      _.each(data.results[0].locations, function(loc) {
        if (loc.adminArea1 != 'US') return;
        if (loc.adminArea3 && !_.contains(states, loc.adminArea3)) return;

        results.push({
          latLng: loc.latLng,
          street: loc.street,
          city: loc.adminArea5,
          state: loc.adminArea3,
          zip: loc.postalCode
        });
      });

      return results;
    }

  }

})();

