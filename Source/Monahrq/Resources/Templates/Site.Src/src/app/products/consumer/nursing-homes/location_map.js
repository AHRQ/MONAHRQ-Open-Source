/**
 * Consumer Product
 * Nursing Homes Reports Module
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
  angular.module('monahrq.products.consumer.nursing-homes')
    .controller('CNHLocationMapCtrl', CNHLocationMapCtrl);


  CNHLocationMapCtrl.$inject = ['$scope', '$state', '$stateParams', '$timeout', 'NHRepositorySvc', 'CNHReportSvc',
    'ResourceSvc', 'UserStateSvc', 'MapMarkerSvc', 'zipDistances'];
  function CNHLocationMapCtrl($scope, $state, $stateParams, $timeout, NHRepositorySvc, CNHReportSvc,
                              ResourceSvc, UserStateSvc, MapMarkerSvc, zipDistances) {
    var nhIds, measureDefs, report, model, overallIDs, overallID, config;

    $scope.reportId = $state.current.data.report;
    $scope.query = {};
    $scope.zipDistances = zipDistances;
    $scope.showValidationErrors = false;

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
      $scope.searchStatus = 'NOT_STARTED';
      $scope.query.location = $stateParams.location;
      $scope.query.distance = +$stateParams.distance;

      if (!$scope.config.DE_IDENTIFICATION) {
        NHRepositorySvc.init()
          .then(loadReport);
      }
      else {
        $scope.searchStatus = 'COMPLETED';
        $('.report').focus();
      }
    }

    function loadReport() {
     $scope.searchStatus = 'SEARCHING';

      NHRepositorySvc.findNear($stateParams.location, $stateParams.distance)
        .then(function(data) {
          nhIds = _.pluck(_.pluck(data, 'nursingHome'), 'ID');

          if (nhIds.length == 0) {
            handleNoResults();
            return;
          }

          ResourceSvc.getConfiguration()
            .then(function(_config) {
              config = _config;
              overallIDs = _.compact([config.NURSING_OVERALL_ID, config.NURSING_OVERALL_QUALITY_ID, config.NURSING_OVERALL_STAFFING_ID, config.NURSING_OVERALL_HEALTH_ID, config.NURSING_OVERALL_FMLYRATE_ID]);
              overallID = config.NURSING_OVERALL_ID;

              ResourceSvc.getNursingHomeMeasures(overallIDs)
                .then(function(result) {
                  measureDefs = result;
                })
                .then(function() {
                  CNHReportSvc.getNursingHomeOverviewReport(nhIds, overallIDs)
                  .then(function(_report) {
                    report = _report;
                    updateMap();
                    $('.report').focus();
                  });
                });
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
        if (!(_.has(row, 'LatLng') && _.isArray(row.LatLng) && row.LatLng.length == 2)) return;
        var id = row.id;
        var ratingField = getRatingField();
        var ratingIcon = config.NURSING_OVERALL_ID && _.has(row[config.NURSING_OVERALL_ID], ratingField)
          ? MapMarkerSvc.markerForStars(row[config.NURSING_OVERALL_ID][ratingField])
          : null;

        if (row.LatLng[0] == 0 && row.LatLng[1] == 0) {
          return;
        }

        var marker = new google.maps.Marker({
          position: new google.maps.LatLng(row.LatLng[0], row.LatLng[1]),
          map: model.map,
          id: id,
          icon: ratingIcon
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

    function getRatingField() {
      var field;
      var gc = UserStateSvc.get(UserStateSvc.props.C_GEO_CONTEXT_NURSING);
      if (gc == 'peer') {
        field = 'PeerRating';
      }
      else if (gc == 'county') {
        field = 'CountyRating';
      }
      else if (gc == 'national' || gc == null) {
        field = 'NatRating';
      }
      return field;
    }

    function updateMarkerData(marker) {
      var field = getRatingField();
      var profile = _.findWhere(report, {id: marker.id});
      $scope.mapModel.currentMarkerData = {
        id: marker.id,
        name: profile.name,
        address: profile.displayAddress,
        overallRating : config.NURSING_OVERALL_ID && profile[config.NURSING_OVERALL_ID] ? profile[config.NURSING_OVERALL_ID][field] : null,
        overallInspection : config.NURSING_OVERALL_HEALTH_ID && profile[config.NURSING_OVERALL_HEALTH_ID] ? profile[config.NURSING_OVERALL_HEALTH_ID][field] : null,
        overallQuality : config.NURSING_OVERALL_QUALITY_ID && profile[config.NURSING_OVERALL_QUALITY_ID] ? profile[config.NURSING_OVERALL_QUALITY_ID][field] : null,
        overallStaffing :  config.NURSING_OVERALL_STAFFING_ID && profile[config.NURSING_OVERALL_STAFFING_ID] ? profile[config.NURSING_OVERALL_STAFFING_ID][field] : null,
        overallFamily:  profile[config.NURSING_OVERALL_FMLYRATE_ID] ? profile[config.NURSING_OVERALL_FMLYRATE_ID][field] : null
      };

      $scope.missingRating = $scope.mapModel.currentMarkerData.overallInspection == null ||
        $scope.mapModel.currentMarkerData.overallQuality == null ||
        $scope.mapModel.currentMarkerData.overallStaffing == null;
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


