/**
 * Professional Product
 * Usage Data Report Module
 * Utilization Report County Map Controller
 *
 * This controller generates the usage data by county report and display the results in a
 * google map.
 *
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.usage-data')
    .controller('UDMapCountyUtilCtrl', UDMapCountyUtilCtrl);

  UDMapCountyUtilCtrl.$inject = ['$scope', '$state', '$stateParams', '$q', 'ResourceSvc', 'UDUtilReportSvc', 'UDUtilQuerySvc', 'SortSvc', 'hospitalCounties', 'ReportConfigSvc', 'ModalLegendSvc', 'MapUtilSvc'];
  function UDMapCountyUtilCtrl($scope, $state, $stateParams, $q, ResourceSvc, UDUtilReportSvc, UDUtilQuerySvc, SortSvc, counties, ReportConfigSvc, ModalLegendSvc, MapUtilSvc) {

    var states = [],
      report = [],
      stateGeo = {},
      fipsMap = {},
      labels = [];

    var defaultBaseColor = '#0066cc';
    $scope.legend = {
      scale: {},
      bands: {
        '5': {
          styleClass: "five",
          id: 5,
          high: null,
          low: null
        },
        '4': {
          styleClass: "four",
          id: 4,
          high: null,
          low: null
        },
        '3': {
          styleClass: "three",
          id: 3,
          high: null,
          low: null
        },
        '2': {
          styleClass: "two",
          id: 2,
          high: null,
          low: null
        },
        '1': {
          styleClass: "one",
          id: 1,
          high: null,
          low: null
        },
        '0': {
          styleClass: "zero",
          id: 0,
          high: null,
          low: null
        },
        '-1': {
          styleClass: "no-data",
          id: -1,
          high: null,
          low: null
        }
      }
    };

    $scope.reportSettings = {};
    $scope.mapModel = {myMap: null};

    $scope.mapOptions = {
      mapTypeId: google.maps.MapTypeId.ROADMAP
    };

    $scope.getBands = getBands;
    $scope.modalLegend = modalLegend;

    init();

    function init() {
      var increment = 0;

      if (!$scope.config.DE_IDENTIFICATION) {
        loadData($state.current.data.report.county_map);
        setupReportHeaderFooter();
      }

      $scope.baseColor = getBaseColor($state.current.data.report.county_map);

      _.each($scope.legend.bands, function(band, i) {
        var multiple,
          color;

        multiple = increment * .15;

        if (band.id === -1) {
          color = '#808080';
        }
        else {
          color = mapShades($scope.baseColor, multiple);
          color = 'hsl(' + color[0] + ',' + color[1] + '%,' + color[2] + '%)';
        }

        band.color = color;
        increment++;
      });

      _.each(counties, function(county) {
        fipsMap["" + county.CountyID] = county.FIPS;
      });

    }

    function setupReportHeaderFooter() {
      var id = $state.current.data.report.county_map;
      var report = ReportConfigSvc.configForReport(id);
      $scope.reportSettings.header = report.ReportHeader;
      $scope.reportSettings.footer = report.ReportFooter;
    }

    function loadData() {
      loadGeo()
        .then(loadReport)
        .then(processReport);
    }

    function loadGeo() {
      states = $scope.config.website_States;

      var promises = _.map(states, function (state) {
        return ResourceSvc.getCountyGeoFor(state.toLowerCase());
      });

      return $q.all(promises).then(function (geos) {
        _.each(states, function (state, i) {
          stateGeo[state.toLowerCase()] = geos[i];
        });
      });
    }

    function loadReport() {
      var query = UDUtilQuerySvc.query;
      var reportingYear = getReportingYear($state.current.data.report.county_map);
      return UDUtilReportSvc.getReport(query, reportingYear)
        .then(function(_report) {
          report = _report;
        });
    }

    function getReportingYear(id) {
      var reportData = ReportConfigSvc.configForReport(id);
      return _.max(reportData.ReportingYears);
    }

    function getBaseColor(id) {
      var reportData = ReportConfigSvc.configForReport(id);

      if (_.has(reportData, 'HeatmapBaseColor')) {
        return reportData.HeatmapBaseColor;
      }
      else {
        return defaultBaseColor;
      }
    }

    function getScale(id) {
      var reportData = ReportConfigSvc.configForReport(id),
        reportScale;

      _.each(reportData.IncludedColumns, function(columns) {
        _.each(columns, function(val, key, obj) {
          if (key === 'Scale') {
            reportScale = val;
          }
        });
      });

      return reportScale;
    }

    function mapShades(hex, adjustedLum) {
      var rgb = hex2rgb(hex),
        r = rgb[0],
        g = rgb[1],
        b = rgb[2],
        hsl,
        h,
        s,
        l,
        shade;

      hsl = rgbToHsl(r, g, b);
      h = hsl[0];
      s = hsl[1];
      l = hsl[2];

      shade = addingToLuminance(h, s, l, adjustedLum);

      return shade;
    }

    function processReport() {
      console.log('processing report');
      var bands = generateBands();
      generateLegend(bands);
      var shapes = generateShapes();
      var bounds = getBounds(shapes);
      buildGmap(shapes, bounds, bands);
    }


    function buildGmap(shapes, bounds, bands) {
      for (var i = 0; i < states.length; i++) {
        var state = states[i];
        var areaIds = _.keys(shapes[state]);

        for (var j = 0; j < areaIds.length; j++) {
          var area = shapes[state][areaIds[j]];

          for (var k = 0; k < area.length; k++) {
            var id = areaIds[j];
            var band = _.has(bands, id) ? bands[id] : '-1';

            var poly = new google.maps.Polygon({
              paths: area[k],
              strokeColor: '#000000',
              strokeOpacity: 1,
              strokeWeight: 1,
              fillColor: $scope.legend.bands[band].color,
              fillOpacity: 0.8
            });
            poly.setMap($scope.mapModel.myMap);
          }
        }
      }

      $scope.mapModel.myMap.panTo(bounds.getCenter());
      $scope.mapModel.myMap.fitBounds(bounds);

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

    function addLabel(title, desc, position) {

      var contentEl = document.createElement('div');
      contentEl.innerHTML = '<span class="fips">' + title + '</span><br>'
        + '<span class="name" style="display:none;">' + desc + '</span>';

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
        position: position,
        closeBoxURL: "",
        isHidden: false,
        pane: "floatPane",
        enableEventPropagation: true,
        pixelOffset: new google.maps.Size(-35, 0)
      };

      var ibLabel = new InfoBox(labelOptions);
      ibLabel.open($scope.mapModel.myMap);
      labels.push(ibLabel);

      var hoverShowHandler = (function (label) {
        return function () {
          $(label.content_).find('.name').fadeIn(300);
        }
      })(ibLabel);
      var hoverHideHandler = (function (label) {
        return function () {
          $(label.content_).find('.name').fadeOut(2000);
        }
      })(ibLabel);
      google.maps.event.addDomListener(ibLabel.content_, 'mouseenter', hoverShowHandler);
      google.maps.event.addDomListener(ibLabel.content_, 'mouseleave', hoverHideHandler);
    }

    function generateShapes() {
      var shapes = {}, points;
      labels = [];

      for (var i = 0; i < states.length; i++) {
        var state = states[i];
        var curGeo = stateGeo[state];
        shapes[state] = {};

        for (var j = 0; j < curGeo.counties.length; j++) {
          var polys = getPolysForCoords(curGeo.counties[j].boundary);
          shapes[state][curGeo.counties[j].countyFIPS] = polys;
          points = [];
          _.each(polys, function(pg) {
            points = points.concat(_.map(pg, function(p) {
              return {
                x: p.lng(),
                y: p.lat()
              };
            }));
          });

          var bounds = getBoundsForPolys(polys);
          var centroid = MapUtilSvc.calcPolygonCentroid(points);
          addLabel(curGeo.counties[j].countyFIPS, curGeo.counties[j].countyname, new google.maps.LatLng(centroid.y, centroid.x));
        }
      }

      return shapes;
    }

    function getPolysForCoords(coords) {
      var polys = [];
      var singlePoly = [];

      for (var i = 0; i < coords.length; i++) {
        if (_.has(coords[i], 'polygon')) {
          var p = [];
          _.each(coords[i].polygon, function(c) {
            p.push(new google.maps.LatLng(c.latlong[1], c.latlong[0]));
          });
          polys.push(p);
        }
        else {
          var point = new google.maps.LatLng(coords[i].latlong[1], coords[i].latlong[0]);
          singlePoly.push(point);
        }
      }

      if (singlePoly.length > 0) {
        polys.push(singlePoly);
      }

      return polys;
    }

    function getBounds(shapes) {
      var bounds = new google.maps.LatLngBounds();

      for (var i = 0; i < states.length; i++) {
        var state = states[i];
        var areaIds = _.keys(shapes[state]);

        for (var j = 0; j < areaIds.length; j++) {
          var area = shapes[state][areaIds[j]];

          for (var k = 0; k < area.length; k++) {
            var poly = area[k];

            for (var l = 0; l < poly.length; l++) {
              bounds.extend(poly[l]);
            }
          }
        }
      }

      return bounds;
    }

    function getBoundsForPolys(polys) {
      var bounds = new google.maps.LatLngBounds();

      for (var i = 0; i < polys.length; i++) {
        var poly = polys[i];

        for (var j = 0; j < poly.length; j++) {
          bounds.extend(poly[j]);
        }
      }

      return bounds;
    }

    function generateBands() {
      var bands = {};
      var curReport = _.map(report.report.TableData, function(row) {
        var id = _.has(row, 'CountyID') ? row.CountyID : row.ID;
          return {
            id: id,
            value: row.RateDischarges
          }
        });
        SortSvc.objSort(curReport, 'value', 'asc');

        var divisor = getBandDivisor(counties.length);
        var countyGroupSize = (counties.length / divisor) > 5 ? (counties.length / 5) : divisor;

        for (var i = 0; i < curReport.length; i++) {
          var fips = fipsMap[curReport[i].id];
          var rd = curReport[i].value;
          bands[""+fips] = rd == -1 || rd == -2 ? -1 : ~~(i / countyGroupSize);
        }

        return bands;
      }

      function getBandDivisor(numCounties) {
        var divisor;
        if (numCounties <= 10) {
          divisor = 2;
        }
        else if (numCounties <= 15) {
          divisor = 3;
        }
        else if (numCounties <= 20) {
          divisor = 4;
        }
        else {
          divisor = 5;
        }

        return divisor;
      }

      function generateLegend(bands) {
        //Get Units
        /*var topics = _.findWhere(ahsTopics, {id: +$scope.query.topic.topic});
        var selectedMeasure = _.findWhere(topics.measures, {id: $scope.query.topic.measure});
        $scope.legend.scale = selectedMeasure.scale;
        $scope.legend.scale.label = getScaleLabel(selectedMeasure.scale.scaleTarget);
        $scope.legend.scale.source = selectedMeasure.scale.scaleSource;
        */
        $scope.legend.scale = getScale($state.current.data.report.county_map);

        //Get bands
        _.each(report.report.TableData, function(row) {
          var id = _.has(row, 'CountyID') ? row.CountyID : row.ID;
          var fips = fipsMap[id];
          var band = bands[fips],
            sourceRate = row.RateDischarges,
            rate;

          //Catch -1 or -2 values
          if (sourceRate === -1 || sourceRate === -2) {
            rate = 0;
          }
          else {
            rate = sourceRate;
          }

          if ($scope.legend.bands[band].high === null || +rate > +$scope.legend.bands[band].high) {
            $scope.legend.bands[band].high = rate;
          }
          if ($scope.legend.bands[band].low === null || +rate < +$scope.legend.bands[band].low) {
            $scope.legend.bands[band].low = rate;
          }
        });

      }

      function getScaleLabel(target) {
        var label = '';
        if (target === 'Area') label = 'people';
        else if (target === 'Discharges') label = 'discharges';
        return label;
      }

      function getBands() {
        return [
          $scope.legend.bands['-1'],
          $scope.legend.bands['5'],
          $scope.legend.bands['4'],
          $scope.legend.bands['3'],
          $scope.legend.bands['2'],
          $scope.legend.bands['1'],
          $scope.legend.bands['0']
        ];
      }

    function hex2rgb(hex) {
      var rgb = hex.replace('#', '').match(/(.{2})/g);

      var i = 3;
      while (i--) {
        rgb[i] = parseInt(rgb[i], 16);
      }

      return rgb;
    }

    function rgbToHsl(r, g, b) {
      r /= 255, g /= 255, b /= 255;
      var max = Math.max(r, g, b), min = Math.min(r, g, b);
      var h, s, l = (max + min) / 2;

      if(max === min) {
        h = s = 0; // achromatic
      } else {
        var d = max - min;

        s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

        switch(max) {
          case r: h = (g - b) / d + (g < b ? 6 : 0); break;
          case g: h = (b - r) / d + 2; break;
          case b: h = (r - g) / d + 4; break;
        }

        h /= 6;
      }

      return [h, s, l];
    }

    function usableHsl(h, s, l) {
      h = Math.round(h * 360);
      s = Math.round(s * 100);
      l = Math.round(l * 100);

      return [h, s, l];
    }

    function addingToLuminance(h, s, l, adjLuminance) {
      var newHsl;

      l = l + adjLuminance;

      if (adjLuminance < 0) {
        adjLuminance = 0;
      }

      newHsl = usableHsl(h, s, l);

      return newHsl;
    }

    function modalLegend() {
      var id = $state.current.data.report['county_map'];
      ModalLegendSvc.open(id, '');
    }

  }
})();

