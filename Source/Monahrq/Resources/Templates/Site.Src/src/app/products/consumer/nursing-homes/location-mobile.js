/**
 * Consumer Product
 * Nursing Home Reports Module
 * Location Page Controller
 *
 * This controller handles the by-location search report. It will find all nursing homes within a
 * certain distance of the user-provided address. Rating reports are then loaded and
 * shown for any matching homes.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.nursing-homes')
    .controller('CNHLocationCtrl', CNHLocationCtrl);


  CNHLocationCtrl.$inject = ['$scope', '$state', '$stateParams', '$timeout', 'NHRepositorySvc', 'CNHReportSvc', 'ResourceSvc', 'SortSvc',
        'ModalLegendSvc', 'ModalMeasureSvc', 'ScrollToElSvc', 'ConsumerReportConfigSvc', 'UserStateSvc', 'MapMarkerSvc', 'zipDistances', 'nga11yAnnounce',
        'InfographicReportSvc', 'ModalGenericSvc', 'WalkthroughSvc', '$rootScope', 'testParam'];

  function CNHLocationCtrl($scope, $state, $stateParams, $timeout, NHRepositorySvc, CNHReportSvc, ResourceSvc, SortSvc,
      ModalLegendSvc, ModalMeasureSvc, ScrollToElSvc, ConsumerReportConfigSvc, UserStateSvc, MapMarkerSvc, zipDistances, nga11yAnnounce,
      InfographicReportSvc, ModalGenericSvc, WalkthroughSvc, $rootScope, testParam) {

      var nhIds, measureDefs, report, model, overallIDs, overallID, config;
    var compareIds = {};
    var isResultsWalkthrough = false;
    var pageId = 'nhLocation';
    var hasGuideTool = !_.isUndefined($rootScope.hasGuideTool) ? $rootScope.hasGuideTool : false;
    var listObj = [];

    $scope.reportId = $state.current.data.report;
    $scope.reportSettings = {};
    $scope.query = {};
    $scope.zipDistances = zipDistances;
    $scope.columnModel = [];
    $scope.distances = {};
    $scope.showValidationErrors = false;
    $scope.sortOptions = [];
    $scope.showInfographic = false;

    $scope.toggleCompare = toggleCompare;
    $scope.canCompare = canCompare;
    $scope.getCompareTabIndex = getCompareTabIndex;
    $scope.updateSearch = updateSearch;
    $scope.gotoMap = gotoMap;
    $scope.gotoCompare = gotoCompare;
    $scope.modalLegend = modalLegend;
    $scope.modalMeasure = modalMeasure;
    $scope.showCompareHelpModal = showCompareHelpModal;
    $scope.IntroOptions = WalkthroughSvc.IntroOptionsNHLocationNoResults();
    $scope.startWalkthrough = startWalkthrough;
    $scope.onbeforechange = onbeforechange;
    $scope.onExit = onExit;
    $scope.share = share;
    $scope.feedbackModal = feedbackModal;

    $scope.updateSearch = updateSearch;
    $scope.openMarkerInfo = openMarkerInfo;

    $scope.mapModel = {
        map: null,
        markers: [],
        currentMarkerData: {},
        infoWindow: null
    };

    $scope.mapOptions = {
        center: new google.maps.LatLng(34.9596271, -93.106521),
        zoom: 15,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };




    if ($scope.query.location == $stateParams.location) {
        activeTabName_nursingHome = '';
    }

      $scope.onTabSelect = function (tabName) {
        //localStorage.setItem("nurshighome_tab", tabName);
        activeTabName_nursingHome = tabName;
        //$rootScope.tempParam = tabName;
    }
    console.log('testParam.name' + $rootScope.tempParam);
    
    if (activeTabName_nursingHome == 'TableTab') {
        $scope.isTableActive = true;
        $scope.isMapActive = false;
    }
    else if (activeTabName_nursingHome == 'MapTab') {
        $scope.isTableActive = false;
        $scope.isMapActive = true;
    } else {
        $scope.isTableActive = true;
    }





    init();

    $scope.showMap = function () {
        $timeout(function () {
            updateMap();
        }, 100);
    };


    function init() {
      $scope.model = [];

      var gcNursing = UserStateSvc.get(UserStateSvc.props.C_GEO_CONTEXT_NURSING);
      $scope.query.compareTo = gcNursing ? gcNursing : 'national';

      $scope.searchStatus = 'NOT_STARTED';
      $scope.query.sort = 'name.asc';
      $scope.query.location = $stateParams.location;
      $scope.query.distance = $stateParams.distance ? +$stateParams.distance : getMaxDistance();

        localStorage.clear();
      InfographicReportSvc.getNursingHomeReport()
        .then(function(result) {
          if (result) {
            $scope.showInfographic = true;
          }
        });

      $scope.$watch('query.sort', updateSort);

      $scope.$watch('query.compareTo', function (nv, ov) {
          $scope.columnModel = _.filter(listObj, function (o) {
              return o.show(o);
          });

        if (nv === ov) return;
            UserStateSvc.set(UserStateSvc.props.C_GEO_CONTEXT_NURSING, nv);
      });

      if ($scope.query.location) {
        NHRepositorySvc.init()
          .then(loadReport);
      }

      setupReportHeaderFooter();
    }

    function share() {
        window.location = buildShareUrl();
    }

    function feedbackModal() {
        ModalFeedbackSvc.open($scope.config);
    }

    function buildShareUrl() {
        var url = escape(window.location);
        return "mailto:?to=&subject=Shared%20MONAHRQ%20Page&body=" + url;
    }

    function setupReportHeaderFooter() {
      var id = $state.current.data.report;
      var report = ConsumerReportConfigSvc.configForReport(id);
      if (report) {
        $scope.reportSettings.header = report.ReportHeader;
        $scope.reportSettings.footer = report.ReportFooter;
      }
    }

    function onExit() {
        WalkthroughSvc.setIntroIsRunning(false);
        if (isResultsWalkthrough) {
            WalkthroughSvc.OnExit(pageId);
        }
    }

    function startWalkthrough(step) {
        if (hasGuideTool) {
            if (WalkthroughSvc.getIntroIsRunning() || !WalkthroughSvc.hasClosedWalkthrough(pageId)) {
                isResultsWalkthrough = true;
                $scope.IntroOptions = WalkthroughSvc.IntroOptionsNHLocation();
                setTimeout(function () { $scope.walkthroughStart(step); }, 0);
            }
        } 
    }

    function onbeforechange() {
        if (this._currentStep == 0) {
            WalkthroughSvc.screenRead($scope.IntroOptions.steps);
        }
    }

    function getMaxDistance() {
      return _.last(zipDistances);
    }

    function loadReport() {
      $scope.searchStatus = 'SEARCHING';
      //var config;

       NHRepositorySvc.findNear($stateParams.location, $stateParams.distance)
        .then(function(data) {
           nhIds = _.pluck(_.pluck(data, 'nursingHome'), 'ID');

           _.each(data, function (row) {
             $scope.distances[row.nursingHome.ID] = Math.floor(row.distance);
           });

           if (nhIds.length == 0) {
             handleNoResults();
             return;
           }


           ResourceSvc.getConfiguration()
            .then(function(_config) {
              config = _config;
              overallID = config.NURSING_OVERALL_ID;
              overallIDs = _.compact([config.NURSING_OVERALL_ID, config.NURSING_OVERALL_QUALITY_ID, config.NURSING_OVERALL_STAFFING_ID, config.NURSING_OVERALL_HEALTH_ID]);

              return ResourceSvc.getNursingHomeMeasures(overallIDs);
            })
            .then(function(result) {
              measureDefs = result;

              // nhcahps data isn't always included. so we try to load that separately here.
              return ResourceSvc.getNursingHomeMeasure(config.NURSING_OVERALL_FMLYRATE_ID)
                .catch(function(error) {
                  return null;
                });
            })
            .then(function(result) {
              if (result) {
                measureDefs.push(result);
                overallIDs.push(config.NURSING_OVERALL_FMLYRATE_ID);
              }
              return CNHReportSvc.getNursingHomeOverviewReport(nhIds, overallIDs);
            })
            .then(function(_report) {
              report = _report;
              updateTable();
              buildColumnModel();
              $scope.searchStatus = 'COMPLETED';
              ScrollToElSvc.scrollToEl('.location .report');
            });
        });
    }

    function handleNoResults() {
      $scope.model = [];
      $scope.searchStatus = 'NO_RESULTS';

      if (WalkthroughSvc.getIntroIsRunning()) {
          $scope.IntroOptions = WalkthroughSvc.IntroOptionsNHLocationNoResults();
          $scope.walkthroughStart();
      }

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

        _.each(report, function (row) {
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


        _.each(model.markers, function (mark) {
            bounds.extend(mark.getPosition());
        });

        // need to force a resize to occur after ng-show on container takes effect
        $timeout(function () {
            google.maps.event.trigger(model.map, 'resize');
            model.map.fitBounds(bounds);
            model.map.setZoom(model.map.getZoom() - 1);
        });
    }

    function openMarkerInfo(marker) {
        updateMarkerData(marker);
        $scope.mapModel.infoWindow.open($scope.mapModel.map, marker);
        //$scope.infoWindow.open($scope.mapModel.map, marker);
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
        var profile = _.findWhere(report, { id: marker.id });
        $scope.mapModel.currentMarkerData = {
            id: marker.id,
            name: profile.name,
            address: profile.displayAddress,
            overallRating: config.NURSING_OVERALL_ID && profile[config.NURSING_OVERALL_ID] ? profile[config.NURSING_OVERALL_ID][field] : null,
            overallInspection: config.NURSING_OVERALL_HEALTH_ID && profile[config.NURSING_OVERALL_HEALTH_ID] ? profile[config.NURSING_OVERALL_HEALTH_ID][field] : null,
            overallQuality: config.NURSING_OVERALL_QUALITY_ID && profile[config.NURSING_OVERALL_QUALITY_ID] ? profile[config.NURSING_OVERALL_QUALITY_ID][field] : null,
            overallStaffing: config.NURSING_OVERALL_STAFFING_ID && profile[config.NURSING_OVERALL_STAFFING_ID] ? profile[config.NURSING_OVERALL_STAFFING_ID][field] : null,
            overallFamily: profile[config.NURSING_OVERALL_FMLYRATE_ID] ? profile[config.NURSING_OVERALL_FMLYRATE_ID][field] : null
        };

        $scope.missingRating = $scope.mapModel.currentMarkerData.overallInspection == null ||
          $scope.mapModel.currentMarkerData.overallQuality == null ||
          $scope.mapModel.currentMarkerData.overallStaffing == null;
    }






    function buildColumnModel() {
        var cols = [];

        _.each(measureDefs, function (def) {
            var col = {
                id: def.MeasureID,
                name: def.SelectedTitleConsumer,
                showForNational: true,
                showForState: true,
                showForCounty: true,
                show: function (col) {
                    if ($scope.query.compareTo == 'national') {
                        return col.showForNational;
                    }
                    if ($scope.query.compareTo == 'state') {
                        return col.showForState;
                    }
                    if ($scope.query.compareTo == 'county') {
                        return col.showForCounty;
                    }
                    return true;
                },
            }

            //  Calculate the sort options that show/hide a column.
            if (def.MeasuresName == "NH-OA-01") {
                col.showForState = false;
                for (var row in $scope.model) {
                    if ($scope.model[row][col.id].PeerRating != "-") {
                        col.showForState = true;
                        break;
                    }
                }
                col.showForCounty = false;
                for (var row in $scope.model) {
                    if ($scope.model[row][col.id].CountyRating != "-") {
                        col.showForCounty = true;
                    }
                }
            }
            else if (def.MeasuresName == "NH_COMP_OVERALL") {
                col.showForNational = false;
                for (var row in $scope.model) {
                    if (typeof $scope.model[row][col.id] === "undefined" ||
                        $scope.model[row][col.id] === undefined ||
                        typeof $scope.model[row][col.id].NatRating === "undefined") {
                        continue;
                    }
                    if ($scope.model[row][col.id].NatRating === null) {
                        $scope.model[row][col.id].NatRating = "-";
                    }
                    if ($scope.model[row][col.id].NatRating != "-") {
                        col.showForNational = true;
                    }
                }
            }

            //  Add column to final column list.
            cols.push(col);
        });

        listObj = cols;
        $scope.columnModel = _.filter(listObj, function (o) {
            return o.show(o);
        });
        buildSortOptions();
    }

    function buildSortOptions() {
      $scope.sortOptions.push({label: 'Name, a to z', value: 'name.asc'});
      $scope.sortOptions.push({label: 'Name, z to a', value: 'name.desc'});
      _.each($scope.columnModel, function(col) {
        $scope.sortOptions.push({
          label: col.name + ', 1 to 5',
          value: col.id + '.asc'
        });
        $scope.sortOptions.push({
          label: col.name + ', 5 to 1',
          value: col.id + '.desc'
        });
      });
    }

    function updateTable() {
      model = [];
      angular.copy(report, model);
      updateSort(0, 1);
      $scope.model = model;
    }

    function updateSort(ov, nv) {
      if (ov == nv) return;
      if (!$scope.query.sort) return;

      var sortParams = $scope.query.sort.split("."),
        sortField = sortParams[0],
        sortDir = sortParams[1];

      if (sortField == 'name') {
        SortSvc.objSort(model, sortField, sortDir);
      }
      else {
        var f;
        if ($scope.query.compareTo == 'national') {
          f = 'NatRating';
        }
        else if ($scope.query.compareTo == 'state') {
          f = 'PeerRating';
        }
        else if ($scope.query.compareTo == 'county') {
          f = 'CountyRating';
        }

        SortSvc.objSort2Numeric(model, sortField, f, sortDir);
      }
    }

    function canSearch() {
      return $scope.query.location && $scope.query.distance;
    }

    function updateSearch() {
      if (!canSearch()) {
        $scope.showValidationErrors = true;
        return;
      }

      if ($scope.query.location == $stateParams.location && $scope.query.distance == $stateParams.distance && $scope.model.length > 0) {
          if (WalkthroughSvc.getSearchComplete('nh')) {
              $scope.startWalkthrough();
          }
          else {
              $scope.startWalkthrough(3);
          }
      }

      $state.go('^.location', {
        location: $scope.query.location,
        distance: $scope.query.distance
      });
    }

    function toggleCompare(id) {
      var prevSize = _.size(compareIds);

      if (_.has(compareIds, id)) {
        delete compareIds[id];
      }
      else {
        compareIds[id] = true;
      }

      updateCompareStatusMsg(prevSize);
    }

    function updateCompareStatusMsg(prevSize) {
      if (canCompare() && _.size(compareIds) === 2 && prevSize === 1) {
        nga11yAnnounce.assertiveAnnounce("Compare button enabled");
      }
      else if (!canCompare() && _.size(compareIds) === 1 && prevSize === 2) {
        nga11yAnnounce.assertiveAnnounce("Compare button disabled");
      }
    }

    function canCompare() {
      var size = _.size(compareIds);
      return (size >= 2 && size <= 5);
    }

    function getCompareTabIndex() {
      return canCompare() ? '0' : '-1';
    }

    function gotoMap() {
      $state.go('^.location-map', {
        location: $scope.query.location,
        distance: $scope.query.distance
      });
    }

    function gotoCompare() {
      if (!canCompare()) return;

      $state.go('^.compare', {
        ids: _.keys(compareIds).join(',')
      });
    }

    function modalLegend(){
      var id = $state.current.data.report;
      ModalLegendSvc.open(id);
    }

    function modalMeasure(id) {
      ModalMeasureSvc.openNursingMeasure(id);
    }

    function showCompareHelpModal() {
        if (!canCompare())
            ModalGenericSvc.open('Help', 'Please select at least two nursing homes to view this report.  No more than five nursing homes may be selected at a time.');
    }

  }

})();
