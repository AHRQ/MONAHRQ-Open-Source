/**
 * Professional Product
 * Quality Ratings Module
 * Location Map Controller
 *
 * This controller runs the same report as the by-location page, but instead of displaying
 * the results in a table, it shows them as pins on a map using the google maps api.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings')
    .controller('QRMapLocationCtrl', QRMapLocationCtrl);

  QRMapLocationCtrl.$inject = ['$scope', '$state', '$stateParams', '$timeout', 'ResourceSvc', 'ZipDistanceSvc', 'CHReportSvc',
    'QRQuerySvc', 'MapMarkerSvc', 'hospitals', 'hospitalZips'];
  function QRMapLocationCtrl($scope, $state, $stateParams, $timeout, ResourceSvc, ZipDistanceSvc, CHReportSvc,
                             QRQuerySvc, MapMarkerSvc, hospitals, hospitalZips){
    var hospitalIds, report, model;

    $scope.query = QRQuerySvc.query;
    $scope.hasResults = false;
    $scope.hasSearch = false;

    $scope.updateSearch = updateSearch;
    $scope.gotoTable = gotoTable;
    $scope.openMarkerInfo = openMarkerInfo;

    $scope.mapModel = {
      map: null,
      markers: [],
      currentMarkerData: {}
    };

    $scope.mapOptions = {
      center: new google.maps.LatLng(34.9596271,-93.106521),
      zoom: 15,
      mapTypeId: google.maps.MapTypeId.ROADMAP
    };

    init();

    function init() {
      ResourceSvc.getMeasureDef($scope.config.HOSPITAL_OVERALL_ID)
        .then(function(def) {
          if (def.length > 0) {
            $scope.overallMeasureTitle = def[0].SelectedTitle;
          }
        });

      if (!$scope.config.DE_IDENTIFICATION) {
        loadReport();
      }
    }

    function loadReport() {
      var query = $scope.query;

      var results = _.filter(hospitals, function(h) {
        var inSet = false;

        if (query.searchType === 'hospitalType'
          && (_.contains(h.HospitalTypes, +query.hospitalType) || query.hospitalType == '999999')) {
          inSet = true;
        }
        else if (query.searchType === 'geo' && query.geoType === 'region'
          && (h.RegionID === +query.region || query.region == '999999')) {
          inSet = true;
        }
        else if (query.searchType === 'geo' && query.geoType === 'zip'
                 && checkZipDistance(h.Zip, query.zip, query.zipDistance)) {
          inSet = true;
        }

        return inSet;
      });

      hospitalIds = _.pluck(results, 'Id');

      if (hospitalIds.length == 0) {
        handleNoResults();
        return;
      }

      CHReportSvc.getHospitalOverviewReport(hospitalIds, $scope.config.HOSPITAL_OVERALL_ID)
        .then(function(_report) {
          report = _report;
          updateMap();
          $('.report').focus();
        });
    }

    function handleNoResults() {
      $scope.hasResults = false;

      if ($scope.hasSearch) {
        $('.report').focus();
      }
    }

    function updateMap() {
      var bounds = new google.maps.LatLngBounds();
      var model = $scope.mapModel;

      if (model.map == null) return;

      $scope.hasResults = true;
      model.markers = [];

      _.each(report, function(row) {
        if (!(_.has(row, 'LatLng') && _.isArray(row.LatLng) && row.LatLng.length == 2 && row.LatLng[0] != 0 && row.LatLng[1] != 0)) return;
        var id = row.id;

        var marker = new google.maps.Marker({
          position: new google.maps.LatLng(row.LatLng[0], row.LatLng[1]),
          map: model.map,
          id: id,
          icon: _.has(row, 'peerRating') ? MapMarkerSvc.markerForRating(row.peerRating) : null
        });

        model.markers.push(marker);
      });


      _.each(model.markers, function(mark) {
        bounds.extend(mark.getPosition());
      });

      // need to force a resize to occur after ng-show on container takes effect
      $timeout(function() {
        google.maps.event.trigger(model.map, 'resize');
        model.map.fitBounds(bounds);
        model.map.setZoom(model.map.getZoom() - 1);
      });
    }

    function openMarkerInfo(marker) {
      updateMarkerData(marker);
      $scope.infoWindow.open($scope.mapModel.map, marker);
    }

    function updateMarkerData(marker) {
      var profile = _.findWhere(report, {id: marker.id});
      $scope.mapModel.currentMarkerData = {
        id: marker.id,
        name: profile.name,
        address: profile.displayAddress,
        rating: profile.peerRating
      };
    }

    function updateSearch() {
      $state.go('^.location-map', {
        location: $scope.query.location,
        distance: $scope.query.distance
      });
    }

    function gotoTable() {
      $state.go('^.location', {
        location: $scope.query.location,
        distance: $scope.query.distance
      });
    }


    var zipQuery = null, zipCache = [];
    function getZipCodesByDistance(zip, distance) {
      var zk = zip + '|' + distance;

      if (zk === zipQuery) {
        return zipCache;
      }

      var hcoords = _.findWhere(hospitalZips, {Zip: zip});

      if (!hcoords) {
        return [];
      }

      var zips = _.filter(hospitalZips, function(z) {
        var dist = ZipDistanceSvc.calcDist(hcoords.Latitude, hcoords.Longitude, z.Latitude, z.Longitude);
        return dist <= distance;
      });

      zipQuery = zk;
      zipCache = _.pluck(zips, 'Zip');

      return zipCache;
    }

    function checkZipDistance(hzip, zip, distance) {
      var zips = getZipCodesByDistance(zip, distance);
      return _.contains(zips, hzip);
    }

  }

})();

