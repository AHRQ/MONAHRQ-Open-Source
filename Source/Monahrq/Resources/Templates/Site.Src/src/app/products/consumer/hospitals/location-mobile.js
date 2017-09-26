/**
 * Consumer Product
 * Hospital Reports Module
 * Location Page Controller
 *
 * This controller handles the by-location search report. It will find all hospitals within a
 * certain distance of the user-provided address. Quality reports are then loaded and
 * shown for any matching hospitals.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.hospitals')
    .controller('CHLocationCtrl', CHLocationCtrl);


  CHLocationCtrl.$inject = ['$scope', '$state', '$stateParams', '$timeout', 'ResourceSvc', 'HospitalRepositorySvc', 'CHReportSvc',
    'SortSvc', 'ModalLegendSvc', 'ModalMeasureSvc', 'ScrollToElSvc', 'ConsumerReportConfigSvc', 'UserStateSvc',
    'zipDistances', 'nga11yAnnounce', 'ModalGenericSvc', 'WalkthroughSvc', '$rootScope', 'MapMarkerSvc'];

  function CHLocationCtrl($scope, $state, $stateParams, $timeout, ResourceSvc, HospitalRepositorySvc, CHReportSvc,
                          SortSvc, ModalLegendSvc, ModalMeasureSvc, ScrollToElSvc, ConsumerReportConfigSvc, UserStateSvc,
                          zipDistances, nga11yAnnounce, ModalGenericSvc, WalkthroughSvc, $rootScope, MapMarkerSvc) {
    var hospitalIds, report, model;
    var compareIds = {};
    
    var isResultsWalkthrough = false;
    var pageId = 'hospitalsLocaiton';
    var hasGuideTool = !_.isUndefined($rootScope.hasGuideTool) ? $rootScope.hasGuideTool : false;
      var activeTab;

    $scope.reportSettings = {};
    $scope.reportId = $state.current.data.report;
    $scope.query = {};
    $scope.zipDistances = zipDistances;
    $scope.distances = {};
    $scope.showValidationErrors = false;
    $scope.sortOptions = {
      hospital: [
        {label: 'Name (a-z)', value: 'name.asc'},
        {label: 'Name (z-a)', value: 'name.desc'}
      ]
    };


    $scope.toggleCompare = toggleCompare;
    $scope.canCompare = canCompare;
    $scope.getCompareTabIndex = getCompareTabIndex;
    $scope.updateSearch = updateSearch;
    $scope.gotoMap = gotoMap;
    $scope.gotoCompare = gotoCompare;
    $scope.modalLegend = modalLegend;
    $scope.modalMeasure = modalMeasure;
    $scope.showCompareHelpModal = showCompareHelpModal;
    $scope.IntroOptions = WalkthroughSvc.IntroOptionsLocationNoResults();
    $scope.startWalkthrough = startWalkthrough;
    $scope.beforeChangeEvent = beforeChangeEvent;
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
        activeTabName_hospital = '';
    } 
      

    $scope.onTabSelect = function (tabName) {
        //localStorage.setItem("hospital_tab", tabName);
        activeTabName_hospital = tabName;
    }

    if (activeTabName_hospital == 'TableTab') {
        $scope.isTableActive = true;
        $scope.isMapActive = false;
    }
    else if (activeTabName_hospital == 'MapTab') {
        $scope.isTableActive = false;
        $scope.isMapActive = true;
    } else {
        $scope.isTableActive = true;
    }




    init();

    $scope.showMap = function(){
      $timeout(function(){
        updateMap();
      }, 100);
    };

    function init() {
        //localStorage.clear();
      $scope.model = [];

      $scope.searchStatus = 'NOT_STARTED';

      var gcHospital = UserStateSvc.get(UserStateSvc.props.C_GEO_CONTEXT_HOSPITAL);
      $scope.query.compareTo = gcHospital ? gcHospital : 'state';

      $scope.query.sort = 'name.asc';
      $scope.query.location = $stateParams.location;
      $scope.query.distance = $stateParams.distance ? +$stateParams.distance : getMaxDistance();

      $scope.$watch('query.sort', updateSort);
      $scope.$watch('query.compareTo', function(nv, ov) {
        if (nv === ov) return;
        UserStateSvc.set(UserStateSvc.props.C_GEO_CONTEXT_HOSPITAL, nv);
      });

      if ($scope.query.location) {
        ResourceSvc.getMeasureDef($scope.config.HOSPITAL_OVERALL_ID)
          .then(function(def) {
            if (def.length > 0) {
              $scope.overallMeasureTitle = def[0].SelectedTitleConsumer;
            }
          });

        HospitalRepositorySvc.init()
          .then(loadReport);
      }
      //console.log('search status :' + $scope.searchStatus);
      setupReportHeaderFooter();


      //console.log('tab name token: ' + token);
      //if (token == 'TableTab') {
      //        $scope.isTableActive = true;
      //        $scope.isMapActive = false;
      //    }
      //else if (token == 'MapTab') {
      //        $scope.isTableActive = false;
      //        $scope.isMapActive = true;
      //    }

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

    function beforeChangeEvent(el, scope) {
        if (this._currentStep == 0) {
            WalkthroughSvc.screenRead($scope.IntroOptions.steps);
        }
    }

    function startWalkthrough(step) {
        if (hasGuideTool) {
            if (WalkthroughSvc.getIntroIsRunning() || !WalkthroughSvc.hasClosedWalkthrough(pageId)) {
                isResultsWalkthrough = true;
                $scope.IntroOptions = WalkthroughSvc.IntroOptionsLocation();
                setTimeout(function () { $scope.walkthroughStart(step); }, 0);
            }
        }
        
    }


    function getMaxDistance() {
      return _.last(zipDistances);
    }

    function loadReport() {
      $scope.searchStatus = 'SEARCHING';

      HospitalRepositorySvc.findNear($stateParams.location, $stateParams.distance)
        .then(function(data) {
          hospitalIds = _.pluck(_.pluck(data, 'hospital'), 'Id');

          _.each(data, function(row){
            $scope.distances[row.hospital.Id] = Math.floor(row.distance);
          });

          if (hospitalIds.length == 0) {
            handleNoResults();
            return;
          }

          CHReportSvc.getHospitalOverviewReport(hospitalIds, $scope.config.HOSPITAL_OVERALL_ID)
            .then(function(_report) {
              report = _report;
              updateTable(); 
              $scope.searchStatus = 'COMPLETED';
              ScrollToElSvc.scrollToEl('.location .report');
            });

            //CHReportSvc.getHospitalOverviewReport(hospitalIds, $scope.config.HOSPITAL_OVERALL_ID)
            //.then(function(_report) {
            //  report = _report;
            //  updateMap();
            //  $('.report').focus();
            //});
      });
    }

    function handleNoResults() {
      $scope.model = [];
      $scope.searchStatus = 'NO_RESULTS';
      if (WalkthroughSvc.getIntroIsRunning()) {
          $scope.walkthroughStart();
      }
      if ($scope.hasSearch) {
        $('.report').focus();
      }
    }

    function updateTable() {
      model = [];
      angular.copy(report, model);
      updateSort(0, 1);
      $scope.model = model;

      if (model.length > 0 && !_.has(model[0], 'natRating')) {
        $scope.noRating = true;
      }
    }


    function updateMap() {
        var bounds = new google.maps.LatLngBounds();
        var model = $scope.mapModel;

        if (model.map == null) return;

        $scope.searchStatus = 'COMPLETED';
        model.markers = [];

        _.each(report, function (row) {
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
    }


    function updateMarkerData(marker) {
        var ratingField = getRatingField();
        var profile = _.findWhere(report, { id: marker.id });
        $scope.mapModel.currentMarkerData = {
            id: marker.id,
            name: profile.name,
            address: profile.displayAddress,
            rating: profile[ratingField]
        };
    }

    function getRatingField() {
        var ratingField = 'natRating';
        if (UserStateSvc.get(UserStateSvc.props.C_GEO_CONTEXT_HOSPITAL) === 'state') {
            ratingField = 'peerRating';
        }
        return ratingField;
    }

    function updateSort(ov, nv) {
      if (ov == nv) return;

      var parts;
      parts = $scope.query.sort.split('.');

      if (parts.length == 2) {
        SortSvc.objSort(model, parts[0], parts[1]);
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
          if (WalkthroughSvc.getSearchComplete('hosp')) {
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
        nga11yAnnounce.assertiveAnnounce('Compare button enabled');
      }
      else if (!canCompare() && _.size(compareIds) === 1 && prevSize === 2) {
        nga11yAnnounce.assertiveAnnounce('Compare button disabled');
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
      ModalLegendSvc.open(id, 'symbols');
    }

    function modalMeasure(id) {
      ModalMeasureSvc.open(id);
    }

    function showCompareHelpModal() {
      if (!canCompare())
        ModalGenericSvc.open('Help', 'Please select at least two hospitals to view this report.  No more than five hospitals may be selected at a time.');
    }
  }

})();
