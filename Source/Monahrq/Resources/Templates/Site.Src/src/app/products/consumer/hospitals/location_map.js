/**
 * Consumer Product
 * Hospital Reports Module
 * Location Map Page Controller
 *
 * This controller runs the same report as the by-location page, but instead of displaying
 * the results in a table, it shows them as pins on a map using the google maps api.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.hospitals')
    .controller('CHLocationMapCtrl', CHLocationMapCtrl);


  CHLocationMapCtrl.$inject = ['$scope', '$state', '$stateParams', '$timeout', 'HospitalRepositorySvc', 'CHReportSvc',
    'UserStateSvc', 'MapMarkerSvc', 'ResourceSvc', 'zipDistances'];
  function CHLocationMapCtrl($scope, $state, $stateParams, $timeout, HospitalRepositorySvc, CHReportSvc,
                             UserStateSvc, MapMarkerSvc, ResourceSvc, zipDistances) {
    var hospitalIds, report, model;

    $scope.query = {};
    $scope.zipDistances = zipDistances;
    $scope.showValidationErrors = false;
    $scope.reportId = $state.current.data.report;

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

    /* DEMO OPTIONS
    $scope.mapOptions = {
      center: new google.maps.LatLng(39.1125799,174.1126896),
      zoom: 6,
      mapTypeId: google.maps.MapTypeId.ROADMAP
    };
    */

    init();

    function init() {
      $scope.searchStatus = 'NOT_STARTED';
      $scope.query.location = $stateParams.location;
      $scope.query.distance = +$stateParams.distance;

      ResourceSvc.getMeasureDef($scope.config.HOSPITAL_OVERALL_ID)
        .then(function(def) {
          if (def.length > 0) {
            $scope.overallMeasureTitle = def[0].SelectedTitleConsumer;
          }
        });

      if (!$scope.config.DE_IDENTIFICATION) {
        HospitalRepositorySvc.init()
          .then(loadReport);
      }
      else {
        $scope.searchStatus = 'COMPLETED';
        $('.report').focus();
      }
    }

    function loadReport() {
     $scope.searchStatus = 'SEARCHING';

      HospitalRepositorySvc.findNear($stateParams.location, $stateParams.distance)
        .then(function(data) {
          hospitalIds = _.pluck(_.pluck(data, 'hospital'), 'Id');

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
      });
    }

    function handleNoResults() {
      $scope.searchStatus = 'NO_RESULTS';

      if ($scope.hasSearch) {
        $('.report').focus();
      }
    }

    function updateMap() {
      var bounds = new google.maps.LatLngBounds();
      var model = $scope.mapModel;

      if (model.map == null) return;

      $scope.searchStatus = 'COMPLETED';
      model.markers = [];

      _.each(report, function(row) {
        if (!(_.has(row, 'LatLng') && _.isArray(row.LatLng) && row.LatLng.length == 2 && row.LatLng[0] != 0 && row.LatLng[1] != 0)) return;
        var id = row.id;
        var ratingField = getRatingField();
        var rating = row[ratingField];

        var marker = new google.maps.Marker({
          position: new google.maps.LatLng(row.LatLng[0], row.LatLng[1]),
          map: model.map,
          id: id,
          icon: rating ? MapMarkerSvc.markerForRating(rating) : null
        });

        model.markers.push(marker);
      });

      /*  DEMO MARKER
        var marker = new google.maps.Marker({
          position: new google.maps.LatLng(39.1125799,174.1126896),
          map: model.map,
          id: 1
        });
        model.markers.push(marker);
      */

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

    function getRatingField() {
      var ratingField = 'natRating';
      if (UserStateSvc.get(UserStateSvc.props.C_GEO_CONTEXT_HOSPITAL) === 'state') {
        ratingField = 'peerRating';
      }
      return ratingField;
    }

    function updateMarkerData(marker) {
      var ratingField = getRatingField();
      var profile = _.findWhere(report, {id: marker.id});
      $scope.mapModel.currentMarkerData = {
        id: marker.id,
        name: profile.name,
        address: profile.displayAddress,
        rating: profile[ratingField]
      };
    }

    function canSearch() {
      return $scope.query.location && $scope.query.distance;
    }

    function updateSearch() {
      if (!canSearch()) {
        $scope.showValidationErrors = true;
        return;
      }
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

  }

})();


