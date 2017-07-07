/**
 * Professional Product
 * Usage Data Report Module
 * Avoidable Stays Condition Report Map Controller
 *
 * This controller loads the avoidable hospital stays by condition report, and displays
 * it in a google map.
 *
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UDMapAvoidableStaysCtrl', UDMapAvoidableStaysCtrl);

  UDMapAvoidableStaysCtrl.$inject = ['$scope', '$state', '$stateParams', '$timeout', '$q', 'UDAHSQuerySvc', 'ResourceSvc',
    'ModalLegendSvc', 'ReportConfigSvc', 'MapUtilSvc', 'counties', 'ahsTopics', 'ahs'];
  function UDMapAvoidableStaysCtrl($scope, $state, $stateParams, $timeout, $q, UDAHSQuerySvc, ResourceSvc, ModalLegendSvc,
    ReportConfigSvc, MapUtilSvc, counties, ahsTopics, ahs) {

    var selectedCountiesWithData = null,
        countyCords = [],
        boundsForCounties = new google.maps.LatLngBounds(),
        stateGeo = {}, labels = [];

    $scope.reportSettings = {};
    $scope.legend = {
      scale:{},
      bands:{
        '5': {
          styleClass: "five",
          id: 5,
          color: '#e7fdff',
          high: null,
          low: null
        },
        '4': {
          styleClass: "four",
          id: 4,
          color: '#e7fdff',
          high: null,
          low: null
        },
        '3': {
          styleClass: "three",
          id: 3,
          color: '#a6e1fc',
          high: null,
          low: null
        },
         '2': {
          styleClass: "two",
          id: 2,
          color: '#5fb8ef',
          high: null,
          low: null
        },
        '1': {
          styleClass: "one",
          id: 1,
          color: '#0582ff',
          high: null,
          low: null
        },
        '0': {
          styleClass: "zero",
          id: 0,
          color: '#0066cc',
          high: null,
          low: null
        },

        '-1': {
          styleClass: "no-data",
          id: -1,
          color: '#808080',
          high: null,
          low: null
        }
      }
    };

    $scope.mapModel = { myMap: null };

    $scope.mapOptions = {
      mapTypeId: google.maps.MapTypeId.ROADMAP
    };

    init();


    function init() {
      UDAHSQuerySvc.notifyReportChange($state.current.data.report.topic_map);

      if (!$scope.config.DE_IDENTIFICATION) {
        updateMap();
        setupReportHeaderFooter();
      }
    }

    $scope.modalLegend = function() {
      var id = $state.current.data.report['topic_map'];
      ModalLegendSvc.open(id, '');
    };

    $scope.getBands = function() {
      return [
        $scope.legend.bands['-1'],
        $scope.legend.bands['5'],
        $scope.legend.bands['4'],
        $scope.legend.bands['3'],
        $scope.legend.bands['2'],
        $scope.legend.bands['1'],
        $scope.legend.bands['0']
      ];
    };

    function setupReportHeaderFooter() {
      var id = $state.current.data.report.topic_map;
      var report = ReportConfigSvc.configForReport(id);
      $scope.reportSettings.header = report.ReportHeader;
      $scope.reportSettings.footer = report.ReportFooter;
    }

    function updateMap(n, o) {
      //if (n == o) return;

      var promises = _.map($scope.config.website_States, function(state) {
        return ResourceSvc.getCountyGeoFor(state.toLowerCase());
      });

      $q.all(promises).then(function(geos) {
        _.each($scope.config.website_States, function(state, i) {
          stateGeo[state.toLowerCase()] = geos[i];
        });

        getCounties();
        setLegend();
      });


    }

    function getScaleLabel(target) {
      var label = '';
      if (target == 'Area') label = 'people';
      else if (target == 'Discharges') label = 'discharges';
      return label;
    }

    function setLegend(){
      //Get Units
      var topics = _.findWhere(ahsTopics, {id: +$scope.query.topic.topic});
      var selectedMeasure = _.findWhere(topics.measures, {id: $scope.query.topic.measure});
      $scope.legend.scale = selectedMeasure.scale;
      $scope.legend.scale.label = getScaleLabel(selectedMeasure.scale.scaleTarget);
      $scope.legend.scale.source = selectedMeasure.scale.scaleSource;

      //Get bands
      for(var c = 0; c < selectedCountiesWithData.length; c++){
        var band = selectedCountiesWithData[c].band,
            sourceRate = selectedCountiesWithData[c].mapRate,
            rate;

        //Catch -1 or -2 values
        if(sourceRate === -1 || sourceRate === -2){
          rate = 0;
        }
        else{
          rate = sourceRate;
        }

        if($scope.legend.bands[band].high === null || +rate > +$scope.legend.bands[band].high){
          $scope.legend.bands[band].high = rate;
        }
        if($scope.legend.bands[band].low === null || +rate < +$scope.legend.bands[band].low){
          $scope.legend.bands[band].low = rate;
        }
      }
    }

    function getCounties(){
      labels = [];

      //Set up counties objects filtered based on query
      var topic = +$scope.query.topic.topic,
          measure = $scope.query.topic.measure;

      var selectedCounties = _.filter(ahs, function(row){
        return row.topicId == topic && row.measureId == measure && row.countyId != -1;
      });

      selectedCountiesWithData = _.map(selectedCounties, function(row){
        //Set name for the selected Counties
        var resultName = counties.filter(function(obj){
          return obj.CountyID == row.countyId;
        });
        row.name = resultName.length > 0 ? resultName[0].CountyName : '';
        row.fips = resultName.length > 0 ? resultName[0].FIPS : null;

        //Set cords for the selected Counties
        var geoCounties = _.has(stateGeo, row.state) ? stateGeo[row.state].counties : [];
        var resultCoords = geoCounties.filter(function(obj){
          return obj.countyFIPS == row.fips;
        });
        row.boundary = resultCoords.length > 0 ? resultCoords[0].boundary : [];

        return row;
      });

      //Draw counties and labels based on query results
      for( var i=0; i < selectedCountiesWithData.length; i++){
        for(var a=0; a< selectedCountiesWithData[i].boundary.length; a++){
          //exception for hawaii allows for multiple polygons per county
          if(selectedCountiesWithData[i].state === "hi"){
            var polygon = [];
            for(var d=0; d<selectedCountiesWithData[i].boundary[a].polygon.length; d++){
              var point = new google.maps.LatLng(
                selectedCountiesWithData[i].boundary[a].polygon[d].latlong[1],
                selectedCountiesWithData[i].boundary[a].polygon[d].latlong[0]
              );
              polygon.push(point);
              boundsForCounties.extend(point);
            }
            countyCords.push(polygon);
          }

          //everything but hawaii counties
          else{
            var point = new google.maps.LatLng(
              selectedCountiesWithData[i].boundary[a].latlong[1],
              selectedCountiesWithData[i].boundary[a].latlong[0]
            );
            countyCords.push(point);
            boundsForCounties.extend(point);
          }
        }

        // Setup bounds to determine center point
        // of counties for label placement
        var points = [];
        var bounds = new google.maps.LatLngBounds();
        for(var b = 0; b < countyCords.length; b++){
          //exception for hawaii allows for multiple polygons per county
          if(selectedCountiesWithData[i].state === "hi"){
            for(var j = 0; j < countyCords[b].length; j++){
              points.push({x: countyCords[b][j].lng(), y: countyCords[b][j].lat()});
              bounds.extend(countyCords[b][j]);
            }
          }
          // all non hawaii states
          else{
            points.push({x: countyCords[b].lng(), y: countyCords[b].lat()});
            bounds.extend(countyCords[b]);
          }
        }

        var centroid = MapUtilSvc.calcPolygonCentroid(points);

        //Set up label for county
        var contentEl = document.createElement('div');
        contentEl.innerHTML = '<span class="fips">' + selectedCountiesWithData[i].fips + '</span><br>'
          + '<span class="name" style="display:none;">' + selectedCountiesWithData[i].name + '</span>';

        var labelOptions = {
          content: contentEl,
          boxStyle: {
            textAlign: "center",
            fontSize: "7pt",
            fontWeight: "bold",
            background: "transparent"
          },
          zIndex: null,
          disableAutoPan: true,
          position: new google.maps.LatLng(centroid.y, centroid.x),
          closeBoxURL: "",
          isHidden: false,
          pane: "floatPane",
          enableEventPropagation: false,
          pixelOffset: new google.maps.Size(-10, 0)
        };

        var ibLabel = new InfoBox(labelOptions);
        labels.push(ibLabel);
        ibLabel.open($scope.mapModel.myMap);

        var hoverShowHandler = (function (label, county) {
          return function () {
            $(label.content_).find('.name').fadeIn(300);
          }
        })(ibLabel, selectedCountiesWithData[i]);
        var hoverHideHandler = (function (label, county) {
          return function () {
            $(label.content_).find('.name').fadeOut(2000);
          }
        })(ibLabel, selectedCountiesWithData[i]);
        google.maps.event.addDomListener(ibLabel.content_, 'mouseenter', hoverShowHandler);
        google.maps.event.addDomListener(ibLabel.content_, 'mouseleave', hoverHideHandler);

        // Draw county border
        //exception for hawaii allows for multiple polygons per county
        if(selectedCountiesWithData[i].state === "hi"){
          for(var p = 0; p < countyCords.length; p++){
            var countyBoundry = new google.maps.Polygon({
              paths: countyCords[p],
              strokeColor: '#000000',
              strokeOpacity: 1,
              strokeWeight: 1,
              fillColor: $scope.legend.bands[selectedCountiesWithData[i].band].color,
              fillOpacity: 0.8
            });
            countyBoundry.setMap($scope.mapModel.myMap);
          }
        }
        else{
          var countyBoundry = new google.maps.Polygon({
            paths: countyCords,
            strokeColor: '#000000',
            strokeOpacity: 1,
            strokeWeight: 1,
            fillColor: $scope.legend.bands[selectedCountiesWithData[i].band].color,
            fillOpacity: 0.8
          });
          countyBoundry.setMap($scope.mapModel.myMap);
        }
        countyCords = [];
      }

      $timeout(function() {
        google.maps.event.trigger($scope.mapModel.myMap, 'resize');
        //Set Center point for map based on selected Counties and zoom map to fit all selected counties
        $scope.mapModel.myMap.panTo(boundsForCounties.getCenter());
        $scope.mapModel.myMap.fitBounds(boundsForCounties);
      });

      google.maps.event.addListener($scope.mapModel.myMap, 'zoom_changed', function(e) {
        _.each(labels, function(label) {
          if($scope.mapModel.myMap.zoom >= 7) {
            label.setMap($scope.mapModel.myMap);
          }
          else {
            label.setMap(null);
          }
        })
      });
    }
  }

})();

