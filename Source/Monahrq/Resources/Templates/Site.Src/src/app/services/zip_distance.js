/**
 * Monahrq Nest
 * Services Module
 * Zip Distance Calculation Service
 *
 * Calculate the distance between two points, specified by their latitude and longitude.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.services')
    .factory('ZipDistanceSvc', ZipDistanceSvc);


  function ZipDistanceSvc() {
    /**
     * ServiceInterface
     */
    return {
      calcDist: calcDist
    };

    /**
     * Service Implementation
     */
    function deg2rad(degrees) {
      return degrees * (Math.PI / 180);
    }

    function rad2deg(radians) {
      return radians * (180 / Math.PI);
    }

    function calcDist(lat_A, long_A, lat_B, long_B) {
      var distance = Math.sin(deg2rad(lat_A))
      * Math.sin(deg2rad(lat_B))
      + Math.cos(deg2rad(lat_A))
      * Math.cos(deg2rad(lat_B))
      * Math.cos(deg2rad(long_A - long_B));

      distance = (rad2deg(Math.acos(distance))) * 69.09;

      return distance;
    }

  }

})();
