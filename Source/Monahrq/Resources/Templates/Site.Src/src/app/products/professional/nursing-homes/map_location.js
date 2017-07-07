/**
 * Professional Product
 * Nursing Homes Module
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
  angular.module('monahrq.products.professional.nursing-homes')
    .controller('NHMapLocationCtrl', NHMapLocationCtrl);


  NHMapLocationCtrl.$inject = ['$scope', '$state', 'NHQuerySvc', 'NHReportLoaderSvc', 'NHRepositorySvc', 'ResourceSvc', 'ModalLegendSvc', 'MapMarkerSvc'];
  function NHMapLocationCtrl($scope, $state, NHQuerySvc, NHReportLoaderSvc, NHRepositorySvc, ResourceSvc, ModalLegendSvc, MapMarkerSvc) {
    var reports = [],
        marker,
        finalReverseLookup = {},
        profileReverseLookup = {},
        config,
        OVERALL_ID;

    $scope.modalLegend = modalLegend;
    $scope.query = NHQuerySvc.query;
    $scope.haveSearched = false;
    $scope.mapModel = {
      map: null
    };
    $scope.mapOptions = {
      center: new google.maps.LatLng(34.9596271,-93.106521),
      zoom: 15,
      mapTypeId: google.maps.MapTypeId.ROADMAP
    };

    init();


    function init() {
      if (NHQuerySvc.query.searchType === 'overallRating') {
        NHQuerySvc.query.comparedTo = 'nat';
      }
      else {
        NHQuerySvc.query.comparedTo = 'peer';
      }

      if (!$scope.config.DE_IDENTIFICATION) {
        NHRepositorySvc.init().then(loadReport);
      }
    }

    function isDeIdentified() {
      return _.has($scope.config, 'DE_IDENTIFICATION') && $scope.config.DE_IDENTIFICATION === 1;
    }

    function loadReport() {
      if (!NHQuerySvc.isSearchable()) return;

      ResourceSvc.getConfiguration()
        .then(function(_config) {
          config = _config;
          var overallIDs = _.compact([config.NURSING_OVERALL_ID, config.NURSING_OVERALL_QUALITY_ID, config.NURSING_OVERALL_STAFFING_ID, config.NURSING_OVERALL_HEALTH_ID, config.NURSING_OVERALL_FMLYRATE_ID]);
          OVERALL_ID = config.NURSING_OVERALL_ID;

          ResourceSvc.getNursingHomeMeasures(overallIDs)
            .then(function() {
              NHReportLoaderSvc.getMeasureReports(overallIDs)
                .then(function(result) {
                  reports = result;
                })
                .then(searchHomes)
                .then(updateMap);
            });
        });
    }

    function updateMap(homes) {
      var ids = _.pluck(homes, 'ID'),
          bounds = new google.maps.LatLngBounds(),
          curZoom;

      if (isDeIdentified()) {
        $scope.mapModel.map.setZoom(4);
        return;
      }

      $scope.myMarkers = [];

      NHRepositorySvc.getProfiles(ids)
        .then(function(profiles) {
          var map = $scope.mapModel.map,
            finalReport = _.object(ids, _.map(profiles, function(profile) {
            return {
              id: profile.ID || profile.Id,
              name: profile.Name,
              address: profile.City + ', ' + profile.State + ' ' + profile.Zip
            };
          }));

          _.each(reports, function(report) {
            _.each(report.data, function(row) {
              if (_.has(finalReport, row.NursingHomeID)) {
                finalReport[row.NursingHomeID]["" + report.id] = row;
              }
            });
          });

          finalReport = _.values(finalReport);

          /* each nursing home profile should have a property with lat/lng.
             we want to update the gmap object $scope.mapModel.map so that we get a pin for each home.
             then we want to center and zoom the map so that all pins are visible.
             when you click a pin show a google map "popup" with nursing home name and address.

             update the nursing home profile data files with a new property:
              LatLng: [0, 0] // lat, lng -- pull test values from google maps, ones close enough together to be able to have them all visible at a reasonable zoom.
           */


          $scope.openMarkerInfo = function(marker) {
            console.log(marker);
            $scope.currentMarker = marker;
            $scope.updateMarkerData(marker);
            $scope.myInfoWindow.open(map, marker);
          };

          function getRatingField() {
            var field;
            if ($scope.query.comparedTo == 'nat') {
              field = 'NatRating';
            }
            else if ($scope.query.comparedTo == 'peer') {
              field = 'PeerRating';
            }
            else if ($scope.query.comparedTo == 'county') {
              field = 'CountyRating';
            }

            return field;
          }

          $scope.updateMarkerData = function() {
            if ($scope.currentMarker === undefined) return;
             var markerInfo,
                curMarkerData,
                ratingVals = 0,
                marker = $scope.currentMarker;

            curMarkerData = finalReverseLookup[marker.id];
            curMarkerData = finalReport[curMarkerData];

            var field = getRatingField();

            $scope.currentMarkerId = curMarkerData.id;
            $scope.currentMarkerName = curMarkerData.name;
            $scope.currentMarkerAdr = curMarkerData.address;
            $scope.currentMarkerRating = {
              overallRating : config.NURSING_OVERALL_ID ? curMarkerData[config.NURSING_OVERALL_ID][field] : null,
              overallInspection : config.NURSING_OVERALL_HEALTH_ID ? curMarkerData[config.NURSING_OVERALL_HEALTH_ID][field] : null,
              overallQuality : config.NURSING_OVERALL_QUALITY_ID ? curMarkerData[config.NURSING_OVERALL_QUALITY_ID][field] : null,
              overallStaffing :  config.NURSING_OVERALL_STAFFING_ID ? curMarkerData[config.NURSING_OVERALL_STAFFING_ID][field] : null,
              overallFamily:  curMarkerData[config.NURSING_OVERALL_FMLYRATE_ID] ? curMarkerData[config.NURSING_OVERALL_FMLYRATE_ID][field] : null
            };
            $scope.missingRating = true;

            _.each($scope.currentMarkerRating, function(rating) {
              if (rating !== '') {
                ratingVals++;
              }

              if (ratingVals >= 4) {
                $scope.missingRating = false;
              }

            });
          };

          $scope.$watch('query.comparedTo', $scope.updateMarkerData);

          _.each(profiles, function(profile, m) {
            if (!(_.has(profile, 'LatLng') && _.isArray(profile.LatLng) && profile.LatLng.length == 2)) return;
            var id = profile.ID || profile.Id;
            var row = _.findWhere(finalReport, {id: id});
            var ratingField = 'NatRating'; //getRatingField();
            var ratingIcon = config.NURSING_OVERALL_ID && _.has(row[config.NURSING_OVERALL_ID], ratingField)
              ? MapMarkerSvc.markerForStars(row[config.NURSING_OVERALL_ID][ratingField])
              : null;

            if (profile.LatLng[0] == 0 && profile.LatLng[1] == 0) {
              return;
            }

            marker = new google.maps.Marker({
              position: new google.maps.LatLng(profile.LatLng[0], profile.LatLng[1]),
              map: map,
              id: 'id_' + id,
              icon: ratingIcon
            });

            $scope.myMarkers.push(marker);
            profileReverseLookup['id_' + id] = m;
          });

          _.each(finalReport, function(report, m) {
            var id = report.ID || report.Id || report.id;
            console.log(report);

            finalReverseLookup['id_' + id] = m;
          });

          console.log(finalReverseLookup);

          _.each($scope.myMarkers, function(mark) {
            bounds.extend(mark.getPosition());
          });

          map.fitBounds(bounds);
          curZoom = map.getZoom();
          map.setZoom(curZoom - 1);

          $scope.haveSearched = true;
        });
    }

    function searchHomes() {
      var query = NHQuerySvc.query;

      if (query.searchType === 'type') {
        return NHRepositorySvc.findByType(query.type);
      }
      else if (query.searchType === 'inHospital') {
        return NHRepositorySvc.findByInHospital(query.inHospital == 0 ? false : true);
      }
      else if (query.searchType === 'overallRating') {
        return NHRepositorySvc.findByMeasureRating(OVERALL_ID, query.overallRating);
      }
      else if (query.searchType === 'location' && query.location === 'county') {
        if (query.county === 999999) {
          return NHRepositorySvc.all();
        }
        else {
          return NHRepositorySvc.findByCounty(query.county);
        }
      }
      else if (query.searchType === 'location' && query.location === 'zip') {
        return NHRepositorySvc.findByZip(query.zip, +query.zipDistance);
      }
    }

    function modalLegend(){
      var id = $state.current.data.report;
      ModalLegendSvc.open(id);
    }

  }

})();

