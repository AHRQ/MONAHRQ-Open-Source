/**
 * Monahrq Nest
 * Services Module
 * Map Util Service
 *
 * This module provides utility functions for working with mapping/geo.
 *
 * The centroid of the polygon attempts to reduce influence of far-flung points:
 * think of the Oklahoma panhandle, for example. The is useful if you need to
 * place a label over a polygon and want it centered in natural and legible way.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.services')
    .factory('MapUtilSvc', MapUtilSvc);


  function MapUtilSvc() {
    /**
     * ServiceInterface
     */
    return {
      calcPolygonCentroid: calcPolygonCentroid
    };

    /**
     * Service Implementation
     */
    function calcPolygonCentroid(points) {
      var first = points[0], last = points[points.length-1];
      if (first.x != last.x || first.y != last.y) points.push(first);
      var twicearea=0,
        x=0, y=0,
        nPts = points.length,
        p1, p2, f;
      for ( var i=0, j=nPts-1 ; i<nPts ; j=i++ ) {
        p1 = points[i]; p2 = points[j];
        f = p1.x*p2.y - p2.x*p1.y;
        twicearea += f;
        x += ( p1.x + p2.x ) * f;
        y += ( p1.y + p2.y ) * f;
      }
      f = twicearea * 3;
      return { x:x/f, y:y/f };
    }

  }

})();
