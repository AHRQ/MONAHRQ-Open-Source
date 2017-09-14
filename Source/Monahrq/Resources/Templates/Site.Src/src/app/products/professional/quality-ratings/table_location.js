/**
 * Professional Product
 * Quality Ratings Module
 * Location Table Controller
 *
 * This controller handles the by-location search report. It will find all hospitals
 * matching the user's query. Rating reports are then loaded and shown for any matching hospitals.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings')
    .controller('QRTableLocationCtrl', QRTableLocationCtrl);
 function QRTableLocationCtrl($scope, $state, _, QRQuerySvc, SortSvc,
    QRReportSvc, ZipDistanceSvc, ModalMeasureSvc, ModalLegendSvc,
    ReportConfigSvc, ResourceSvc, hospitals, hospitalTypes, hospitalZips, ModalGenericSvc) {

    var hospitalsToCompare = {};

    $scope.reportSettings = {};
    $scope.showMeasure = true;
    $scope.haveSearched = false;
    $scope.hospitals = hospitals;

    $scope.query = QRQuerySvc.query;
    $scope.query.sortBy = 'name.asc';

    $scope.showCompareHelpModal = showCompareHelpModal;

    $scope.measureIdOverallRating = $scope.config.HOSPITAL_OVERALL_ID;
    $scope.model = [];

    $scope.$watch('query.sortBy', updateSort);

    if ($scope.query.searchType) { tableQueryWatchHandler(0, 1); }

    ResourceSvc.getMeasureDef($scope.config.HOSPITAL_OVERALL_ID)
      .then(function(def) {
        if (def.length > 0) {
          $scope.overallMeasureTitle = def[0].SelectedTitle;
        }
      });

    setupReportHeaderFooter();

    function setupReportHeaderFooter() {
      var id = $state.current.data.report.symbols;
      var report = ReportConfigSvc.configForReport(id);
      if (report) {
        $scope.reportSettings.header = report.ReportHeader;
        $scope.reportSettings.footer = report.ReportFooter;
      }
    }

    function tableQueryWatchHandler(newValue, oldValue) {
      if (newValue === oldValue) return;

      QRReportSvc.getReportByMeasure($scope.config.HOSPITAL_OVERALL_ID)
      .then(updateTable,
        function() {
          updateTableNoMeasure();
        });
    }

    function updateSort() {
      if (!QRQuerySvc.query.sortBy) return;
      var sortParams = QRQuerySvc.query.sortBy.split("."),
        sortField = sortParams[0],
        sortDir = sortParams[1];

      SortSvc.objSort($scope.model, sortField, sortDir);
    }

    function updateTableNoMeasure() {
      var query = QRQuerySvc.query;

      $scope.showMeasure = false;
      $scope.haveSearched = true;

      var rows = _.filter(hospitals, function(r) {
        var inSet = false;

        if (query.searchType === 'hospitalType'
          && (_.contains(r.HospitalTypes, +query.hospitalType) || query.hospitalType == '999999')) {
          inSet = true;
        }
        else if (query.searchType === 'geo' && query.geoType === 'region'
          && (r.RegionID === +query.region || query.region == '999999')) {
          inSet = true;
        }
        else if (query.searchType === 'geo' && query.geoType === 'zip'
                 && checkZipDistance(r.Zip, query.zip, query.zipDistance)) {
          inSet = true;
        }

        return inSet;
      });

      $scope.model = _.map(rows,
        function(r) {
          var row, typeName;

          typeName = getHospitalTypeNames(r.HospitalTypes);

          row = {
            id: r.Id,
            name: r.Name,
            address: '', //h.address, TODO: populate address
            type: typeName
          };

          return row;
        });

      updateSort();
    }

    function updateTable(data) {
      var query = QRQuerySvc.query;

      $scope.showMeasure = true;
      $scope.haveSearched = true;

      var rows = _.filter(data, function(r) {
        var inSet = false;

        if (query.searchType === 'hospitalType'
          && (_.contains(r.HospitalType, +query.hospitalType) || query.hospitalType == '999999')) {
          inSet = true;
        }
        else if (query.searchType === 'geo' && query.geoType === 'region'
          && (r.RegionID === +query.region || query.region == '999999')) {
          inSet = true;
        }
        else if (query.searchType === 'geo' && query.geoType === 'zip'
                 && checkZipDistance(r.ZipCode, query.zip, query.zipDistance)) {
          inSet = true;
        }

        return inSet;
      });

      $scope.model = _.map(rows,
        function(r) {
          var h, row, typeName;

          h = _.findWhere(hospitals, {Id: r.HospitalID});
          typeName = getHospitalTypeNames(r.HospitalType);

          row = {
            id: h.Id,
            name: h.Name,
            address: '', //h.address, TODO: populate address
            type: typeName,
            rating: r.PeerRating
          };

          return row;
        });

        updateSort();
    }

    function getHospitalTypeNames(types) {
      var types, typeName;

      types = _.filter(hospitalTypes, function(ht) {
        return _.contains(types, ht.HospitalTypeID);
      });

      typeName = _.reduce(types, function(memo, row) {
        return memo + ', ' + row.Name;
      }, '');

      typeName = typeName.substring(2);

      return typeName;
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

    $scope.modalLegend = function(displayType){
      var id = $state.current.data.report[displayType];
      ModalLegendSvc.open(id, displayType);
    };

    $scope.toggleHospitalCompare = function (id) {
      if (_.has(hospitalsToCompare, id)) {
        delete hospitalsToCompare[id];
      }
      else {
        hospitalsToCompare[id] = true;
      }
    };

    $scope.canCompare = function() {
      var size = _.size(hospitalsToCompare);
      return (size >= 2 && size <= 5);
    };

    $scope.compareHospitals = function() {
      if (!$scope.canCompare()) return;

      $state.go('top.professional.quality-ratings.compare', {
        hospitals: _.keys(hospitalsToCompare)
      });
    };

    $scope.modalMeasure = function(id) {
      ModalMeasureSvc.open(id);
    };

    $scope.showCompare = function() {
      return $scope.ReportConfigSvc.webElementAvailable('Quality_Compare_Column');
    }


    function showCompareHelpModal() {
        if (!$scope.canCompare())
            ModalGenericSvc.open('Help', 'Please select at least two hospitals to view this report.  No more than five hospitals may be selected at a time.');
    }

  }

})();

