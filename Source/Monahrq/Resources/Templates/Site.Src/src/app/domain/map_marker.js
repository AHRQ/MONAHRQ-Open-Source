/**
 * Monahrq Nest
 * Core Domain Module
 * Map Marker Service
 *
 * This service maps quality rating values to corresponding icon URLs. It provides for
 * both the star icons as well as the above/average/below icons.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.domain')
    .factory('MapMarkerSvc', MapMarkerSvc);


  MapMarkerSvc.$inject = [];
  function MapMarkerSvc() {
    var baseDir = 'themes/base/assets/';
    var starsMap = {
      1: 'marker_n_1.png',
      2: 'marker_n_2.png',
      3: 'marker_n_3.png',
      4: 'marker_n_4.png',
      5: 'marker_n_5.png'
    };
    var ratingMap = {
      0: 'marker_rating_nodata.png',
      1: 'marker_rating_above.png',
      2: 'marker_rating_average.png',
      3: 'marker_rating_below.png'
    };

    return {
      markerForStars: markerForStars,
      markerForRating: markerForRating
    };

    function markerForStars(stars) {
      if (stars === '-') {
        return null;
      }
      return baseDir + starsMap[+stars];
    }

    function markerForRating(rating) {
      return baseDir + ratingMap[+rating];
    }
  }

})();
