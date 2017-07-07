/**
 * Professional Product
 * Quality Ratings Module
 * Condition Table Controller
 *
 * This controller handles the by-condition search report. It will find all hospitals
 * matching the user's query. Rating reports are then loaded and shown for any matching hospitals.
 *
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings')
    .controller('QRTableConditionCtrl', QRTableConditionCtrl);

  QRTableConditionCtrl.$inject = ['$scope', '$state', '$stateParams', '$filter', '$q', '_',
    'QRQuerySvc', 'QRReportSvc', 'ResourceSvc', 'SortSvc', 'ModalQRSvc', 'ModalMeasureSvc',
    'ModalTopicCategorySvc', 'ModalTopicSvc', 'ZipDistanceSvc', 'ReportConfigSvc',
    'measureTopicCategories', 'measureTopics', 'hospitals', 'hospitalZips',
    'ModalLegendSvc', 'ModalGenericSvc'];
  function QRTableConditionCtrl($scope, $state, $stateParams, $filter, $q, _,
    QRQuerySvc, QRReportSvc, ResourceSvc, SortSvc, ModalQRSvc, ModalMeasureSvc,
    ModalTopicCategorySvc, ModalTopicSvc, ZipDistanceSvc, ReportConfigSvc,
    measureTopicCategories, measureTopics, hospitals, hospitalZips,
    ModalLegendSvc, ModalGenericSvc){

    var hospitalsToCompare = {}, measureDefs = {};

    $scope.selectText = function(element) {
      var doc = document,
          text = doc.getElementById(element),
          range,
          selection;

      if (doc.body.createTextRange) { //ms
          range = doc.body.createTextRange();
          range.moveToElementText(text);
          range.select();
      }
      else if (window.getSelection) { //all others
          selection = window.getSelection();
          range = doc.createRange();
          range.selectNodeContents(text);
          selection.removeAllRanges();
          selection.addRange(range);
      }
    };
        $scope.selectTextMultiple = function (array_of_elements, destination) {
            var doc = document,
                    selection;
            var range = [];
            var text = [];
            selection = window.getSelection();
            selection.removeAllRanges();

            for (var i = 0, len = array_of_elements.length; i < len; i++) {
                var clonedObject = doc.getElementById(array_of_elements[i]).cloneNode(true);
                clonedObject.id = array_of_elements[i] + "_copy";
                doc.getElementById(destination).appendChild(clonedObject);

                /*
                if (doc.body.createTextRange) { //ms
                    text[array_of_elements[i]] = doc.getElementById(array_of_elements[i]);
                    range[array_of_elements[i]] = doc.body.createTextRange();
                    range[array_of_elements[i]].moveToElementText(text[array_of_elements[i]]);
                    range[array_of_elements[i]].select();

                }
                else if (window.getSelection) { //all others
                    text[array_of_elements[i]] = doc.getElementById(array_of_elements[i]);
                    range[array_of_elements[i]] = doc.createRange();
                    range[array_of_elements[i]].selectNodeContents(text[array_of_elements[i]]);

                    selection.addRange(range[array_of_elements[i]]);
                }
                */

            }



        };

    $scope.haveSearched = false;
    $scope.showHeaderMore = false;
    $scope.topicHelp = {};
    $scope.reportSettings = {};

    $scope.querySvc = QRQuerySvc;
    $scope.query = QRQuerySvc.query;
    $scope.query.comparedTo = 'nat';
    $scope.query.sortBy = 'name.asc';

    $scope.modelPrimary = [];
    $scope.modelRawData = [];
    $scope.columnsPrimary = [];
    $scope.columnsRawData = [];
    $scope.showCompareHelpModal = showCompareHelpModal;

    if (QRQuerySvc.query.topic && QRQuerySvc.query.subtopic) {
      loadData();
    }
    $scope.$watch('query.sortBy', updateSort);
    $scope.$watch('query.subtopic', function() {
      $scope.query.sortBy = 'Name.asc';
      $scope.query.measure = null;
      $scope.query.displayType = $scope.getDefaultDisplayType();
      QRQuerySvc.notifyReportChange($state.current.data.report[$scope.query.displayType]);
    });
    $scope.$watch('query.measure', function(n, o) {
      if (n == o || n == undefined) return;
      loadData();
    });
    $scope.$watch('query.displayType', function(n, o) {
      if (n == o) return;

      QRQuerySvc.notifyReportChange($state.current.data.report[n]);

      if (n == 'raw_data' && _.isEmpty($scope.query.measure) && !_.isEmpty($scope.columnsPrimary)) {
        QRQuerySvc.setMeasure($scope.columnsPrimary[0].MeasureID);
        //$scope.query.measure = $scope.columnsPrimary[0].MeasureID;
      }
      setupReportHeaderFooter();
      buildSortOptions();
    });

    setupReportHeaderFooter();

    function setupReportHeaderFooter() {
      var dt = $scope.query.displayType;
      if (dt) {
        var id = $state.current.data.report[dt];
        var report = ReportConfigSvc.configForReport(id);
        if (report) {
          $scope.reportSettings.header = report.ReportHeader;
          $scope.reportSettings.footer = report.ReportFooter;
        }
      }
    }


    $scope.getCell = function(name, row) {
      /*var fname = 'f' + name;
      if (_.has(row, fname)) {
        return row[fname];
      }*/
      return row[name];
    };

    $scope.modalLegend = function(displayType){
      var id = $state.current.data.report[displayType];
      ModalLegendSvc.open(id, displayType);
    };

    $scope.hasTopicHelp = function() {
      var id = +$scope.query.topic;
      var t = _.findWhere(measureTopicCategories, {TopicCategoryID: id});
      return t && ((t.LongTitle && t.LongTitle.length > 0) || (t.Description && t.Description.length > 0));
    };

    $scope.hasSubtopicHelp = function() {
      var id = +$scope.query.subtopic;
      var t = _.findWhere(measureTopics, {TopicID: id});
      return t && ((t.LongTitle && t.LongTitle.length > 0) || (t.Description && t.Description.length > 0));
    };

    function loadData() {
      var query, measureIds, promises = [];

      query = QRQuerySvc.query;
      if (!query.topic || !query.subtopic) return;

      var topic = _.findWhere(measureTopics, {TopicID: +query.subtopic});

      measureIds = topic.Measures;
      promises.push(ResourceSvc.getMeasureDefs(measureIds));
      promises.push(QRReportSvc.getReportsByMeasures(measureIds))

      $q.all(promises)
      .then(function(result) {
        var reports = {}, measureDefData, reportData;
        measureDefs = {};
        measureDefData = result[0];
        reportData = result[1];

        _.each(measureDefData, function(def) {
          measureDefs[def.MeasureID] = def;
        });

        _.each(reportData, function(report) {
          reports[report[0].MeasureID] = report;
        });

        updateSearch(measureDefs, reports);
      });
    }


    function updateSort(event) {
      if (!QRQuerySvc.query.sortBy) return;
      var sortParams = QRQuerySvc.query.sortBy.split("."),
        sortField = sortParams[0],
        sortDir = sortParams[1];

      if ($scope.showRawData()) {
        if (sortField === 'Name') {
          SortSvc.objSort($scope.modelRawData, sortField, sortDir);
        }
        else
        {
          SortSvc.objSortNumeric($scope.modelRawData, sortField, sortDir);
        }
      }
      else {
        var level = ($scope.showPeer() ? 'PeerRating' : 'NatRating');

        if (sortField === 'Name') {
          SortSvc.objSort($scope.modelPrimary, sortField, sortDir);
        }
        // per-measure ratings
        else {
          SortSvc.objSort2Numeric($scope.modelPrimary, sortField, level, sortDir);
        }
      }
    }

    function updateSearch(measureDefs, data) {
      var query, ids, selectedMeasures;

      query = QRQuerySvc.query;
      if (!query.topic || !query.subtopic) return;

      $scope.haveSearched = true;

      var subtopic = _.findWhere(measureTopics, {TopicID: +$scope.query.subtopic});
      $scope.topicHelp = {
        title: subtopic.Name,
        body: subtopic.Description,
        bodyMore: null
      };

      // build the column model.
      $scope.columnsPrimary  = _.map(measureDefs, function(m) { return m; });
      SortSvc.objSort($scope.columnsPrimary, 'MeasuresName', 'asc');


      // transform the raw report data into a tabular format
      var hospitalRows = {};
      _.each(data, function(report) {
        // narrow the rows
        var hospitals = _.filter(report, function(row) {
          return checkNarrow(query, row);
        });

        _.each(hospitals, function(h) {
          if (!_.has(hospitalRows, h.HospitalID)) hospitalRows[h.HospitalID] = {};
          hospitalRows[h.HospitalID][h.MeasureID] = h;
          hospitalRows[h.HospitalID].HospitalID = h.HospitalID;
        });
      });

      $scope.modelPrimary = _.map(hospitalRows, buildRowPrimary);

      if ($scope.query.measure) {
        var m = _.findWhere(measureDefs, {MeasureID: +$scope.query.measure});
        if (m) {
          var columns = buildRawColumns(m);

          $scope.columnsRawData = columns;
          $scope.modelRawData = buildRawRows(m, columns, $scope.modelPrimary);
          $scope.rawMeasure = m;
        }
      }

      buildSortOptions();

      updateSort();
    }

    function checkNarrow(query, row) {
      var result = false;

      if (query.searchType) {
        if (query.searchType == 'hospitalName' && query.hospitalName == row.HospitalID) {
          result = true;
        }
        else if (query.searchType == 'hospitalType' && (_.contains(row.HospitalType, +query.hospitalType)
          || query.hospitalType == '999999')) {
          result = true;
        }
        else if (query.searchType == 'geo') {
          if (query.geoType == 'region' && (query.region == row.RegionID
            || query.region == '999999')) {
            result = true;
          }
          else if (query.geoType == 'zip'
            && checkZipDistance(row.ZipCode, query.zip, query.zipDistance)) {
            result = true;
          }
        }
      }
      else {
        result = true;
      }

      return result;
    }

    function getNumRawDataColumns(measure) {
      if (measure == undefined) return;
      var numCols = 0;
      _.each(_.keys(measure), function(k) {
        var a = k.substring(0, 7);
        if (a && a === 'ColDesc') {
          var n = +k.substring(7);
          if (n > numCols) numCols = n;
        }
      });

      return numCols;
    }

    function buildRawRows(measure, columns, model) {
      return _.map(model, function(h) {
        var row = {
          HospitalID: h.HospitalID,
          Name: h.Name
        }

        var measuredata = h[measure.MeasureID];
        if (measuredata) {
          _.each(columns, function(col) {
            row[col.id] = measuredata[col.id];

            if (_.has(col, 'filter') && col.filter != 'string') {
              row['f' + col.id] = $filter(col.filter)(row[col.id], col.round);
            }
          });
        }

        return row;
      });
    }

    function buildRawColumns(measure) {
      var numCols = getNumRawDataColumns(measure), columns = [];
      if ($scope.modelPrimary.length > 0) {
        var data = $scope.modelPrimary[0][measure.MeasureID];
      }

      for (var i = 1; i <= numCols; i++) {
        var desc = measure['ColDesc' + i], rnd = measure['ColRounding' + i], format = 'number', round = 0;
        if (_.isEmpty(desc)) {
          if (data && data['Col' + i] != null && data['Col' + i] != "") {
            desc = 'Column ' + i;
          }
          else {
            continue;
          }
        }

        if (rnd == '') {
          format = 'string';
        }
        else if (+rnd  >= 0) {
          format = 'number';
          round = +rnd;
        }

        var col = {
          id: 'Col' + i,
          name: desc,
          filter: format,
          round: round,
          natRating: measure['NatCol' + i],
          peerRating: measure['PeerCol' + i],
          natTop10Rating: measure['NatCol' + i] ? measure['NatTop10'] : null,
          peerTop10Rating: measure['PeerCol' + i] ? measure['PeerTop10'] : null
        }

        columns.push(col);
      }

      return columns;
    }

    function buildRowPrimary(hm) {
      var query, row, h, result;

      query = QRQuerySvc.query;
      h = _.findWhere(hospitals, {Id: hm.HospitalID});

      row = {
        Name: h.Name
      };

      result = _.extend(row, hm);

      return result;
    }

    function buildSortOptions() {
      $scope.sortOptions = [
        {
          id: 'Name.asc',
          name: 'Hospital Name (A to Z)'
        },
        {
          id: 'Name.desc',
          name: 'Hospital Name (Z to A)'
        }
      ];

      if ($scope.showRawData()) {
        _.each($scope.columnsRawData, function(c) {
          $scope.sortOptions.push({
            id: c.id + '.desc',
            name: c.name + ' (High to Low)'
          });
          $scope.sortOptions.push({
            id: c.id + '.asc',
            name: c.name + ' (Low to High)'
          });
        });
       }
      else {
        // these are rating fields
        _.each($scope.columnsPrimary, function(c) {
          $scope.sortOptions.push({
            id: c.MeasureID + '.asc',
            name: c.SelectedTitle + ' (High to Low)'
          });
          $scope.sortOptions.push({
            id: c.MeasureID + '.desc',
            name: c.SelectedTitle + ' (Low to High)'
          });
        });
      }
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

    $scope.getDefaultDisplayType = function() {
      if (ReportConfigSvc.webElementAvailable('Quality_Cond_Display_Symbols_Dropdown')) {
        return 'symbols';
      }
      else if (ReportConfigSvc.webElementAvailable('Quality_Cond_Display_SymbolsAndRAR_Dropdown')) {
        return 'symbols_rar';
      }
      else if (ReportConfigSvc.webElementAvailable('Quality_Cond_Display_BarChart_Dropdown')) {
        return 'bar_charts';
      }
      else if (ReportConfigSvc.webElementAvailable('Quality_Cond_Display_RawData_Dropdown')) {
        return 'raw_data';
      }
    };

    $scope.showSymbol = function(measure) {
      var show = false,
        dt = $scope.query.displayType;

      if (dt === 'symbols' || dt === 'symbols_rar') {
        show = true;
      }

      if (measure && (($scope.showPeer() && measure.PeerRating == '-1')
        || (!$scope.showPeer() && measure.NatRating == '-1'))) {
        show = false;
      }

      return show;
    };

    $scope.showChart = function() {
      var dt = $scope.query.displayType;
      return  dt === 'bar_charts';
    };

    $scope.showRAR = function(measure) {
      var show = false,
        dt = $scope.query.displayType;

      if (dt === 'symbols_rar') {
        show = true;
      }

      if (measure
          && (dt === 'symbols' || dt === 'symbols_rar')
          && (($scope.showPeer() && measure.PeerRating == '-1')
              || (!$scope.showPeer() && measure.NatRating == '-1'))) {
        show = true;
      }

      return show;
    };

    $scope.showCompareLegend = function(measureDef) {
      var measure = $scope.modelPrimary.length > 0 ? $scope.modelPrimary[0][measureDef.MeasureID] : null;

      if ((($scope.showPeer() && measure.PeerRating == '-1')
              || (!$scope.showPeer() && measure.NatRating == '-1'))) {
        return false;
      }

      return true;
    };

    $scope.showRawData = function() {
      var dt = $scope.query.displayType;
      return  dt === 'raw_data';
    };

    $scope.showPeer = function() {
      return $scope.query.comparedTo === 'peer';
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

    $scope.toggleHeaderMore = function(){
      $scope.showHeaderMore = !$scope.showHeaderMore;
    };

    $scope.modalMeasure = function(id) {
      ModalMeasureSvc.open(id);
    };

    $scope.modalQR = function(id) {
      ModalQRSvc.open(id);
    };

    $scope.modalTopicCategory = function(id) {
      ModalTopicCategorySvc.open(id);
    };

    $scope.modalTopic = function(id) {
      ModalTopicSvc.open(id);
    };


    $scope.updateCompare = function() {
      if ($scope.query.comparedTo == 'nat') {
        $scope.compareRatingField = 'NatRating';
        $scope.compareFilledField = 'NatFilled';
      }
      else if ($scope.query.comparedTo == 'peer') {
        $scope.compareRatingField = 'PeerRating';
        $scope.compareFilledField = 'PeerFilled';
      }
    };

    $scope.displayTypeChanged = function() {
      hospitalsToCompare = {};
      //START:[MONNGBD-13] barchart convert svg to png
      /*if($scope.query.displayType == 'bar_charts'){
        setTimeout(function(){
          $('.svg-bar-chart').each(function(){
            $(this).toImage();
          });
        },1000);

      }*/
      //END:[MONNGBD-13] barchart convert svg to png
    };

    $scope.getTopicCategoryName = function() {
      var c = _.findWhere(measureTopicCategories, {TopicCategoryID: +$scope.query.topic});
      if (c) return c.Name;
    };

    $scope.getTopicName = function() {
      var c = _.findWhere(measureTopics, {TopicID: +$scope.query.subtopic});
      if (c) return c.Name;
    };

    $scope.ratingHeadingLabelFor = function(type) {
      var label, compare = $scope.query.comparedTo;

      if (type == 'mean') {
        if (compare == 'nat') {
          label = 'NatLabel';
        }
        else if (compare == 'peer') {
          label = 'PeerLabel';
        }
      }
      else if (type == 'top10') {
        if (compare == 'nat') {
          label = 'NatTop10Label';
        }
        else if (compare == 'peer') {
          label = 'PeerTop10Label';
        }
      }

      return label;
    };

    $scope.ratingHeadingValueFor = function(type) {
      var val, compare = $scope.query.comparedTo;

      if (type == 'mean') {
        if (compare == 'nat') {
          val = 'NatRateAndCI';
        }
        else if (compare == 'peer') {
          val = 'PeerRateAndCI';
        }
      }
      else if (type == 'top10') {
        if (compare == 'nat') {
          val = 'NatTop10';
        }
        else if (compare == 'peer') {
          val = 'PeerTop10';
        }
      }

      return val;
    };

    $scope.showRatingMeasures = function(type) {
      var label = $scope.ratingHeadingLabelFor(type),
        val = $scope.ratingHeadingValueFor(type),
        show = false;

      _.each(measureDefs, function(m) {
        if (m[val] && m[val].length > 0) show = true;
      });

      return show;
    };

    $scope.showRatingRaw = function(type) {
      var m = $scope.rawMeasure, numCols = 0,
        hasData = false, tcol, t10col;

      if (m) {
        numCols = getNumRawDataColumns(m);
      }
      else {
        return false;
      }

      if (type == 'nat') {
        tcol = 'NatCol';
        t10col = 'NatTop10';
      }
      else if (type == 'peer') {
        tcol = 'PeerCol';
        t10col = 'PeerTop10';
      }
      else {
        return false;
      }

      if (m[t10col] == -1) {
        return false;
      }

      for (var i = 1; i <= numCols; i++) {
        var val = m[tcol + i];
        if (val && val.length > 0 && val >= 0) {
          hasData = true;
          break;
        }
      }

      return hasData;
   };

   $scope.showCompare = function() {
     return $scope.ReportConfigSvc.webElementAvailable('Quality_Compare_Column');
   };

   $scope.getColspan = function() {
     var span = 2;

     if (!$scope.showCompare()) {
       span -= 1;
     }

     return span;
   };

   $scope.updateCompare();

  function showCompareHelpModal() {
    if (!$scope.canCompare())
      ModalGenericSvc.open('Help', 'Please select at least two hospitals to view this report.  No more than five hospitals may be selected at a time.');
  }

  }

})();

/**
     * Converts an SVG element to an IMG element with the same dimensions
     * and same visual content. The IMG src will be a base64-encoded image.
     * Works in Webkit and Firefox (not IE).
     */
    $.fn.toImage = function() {
        $(this).each(function() {
            var svg$ = $(this);
            var width = svg$.width();
            var height = svg$.height();

            // Create a blob from the SVG data
            var svgData = new XMLSerializer().serializeToString(this);
            var blob = new Blob([svgData], { type: "image/svg+xml;charset=utf-8" });

            // Get the blob's URL
            var domUrl = self.URL || self.webkitURL || self;
            var blobUrl = domUrl.createObjectURL(blob);

            // Load the blob into a temporary image
            $('<img />')
                .width(width)
                .height(height)
                .on('load', function() {
                    try {
                        var canvas = document.createElement('canvas');
                        canvas.width = width;
                        canvas.height = height;
                        var ctx = canvas.getContext('2d');

                        // Start with white background (optional; transparent otherwise)
                        ctx.fillStyle = '#fff';
                        ctx.fillRect(0, 0, width, height);

                        // Draw SVG image on canvas
                        ctx.drawImage(this, 0, 0);

                        // Replace SVG tag with the canvas' image
                        svg$.replaceWith($('<img />').attr({
                            src: canvas.toDataURL(),
                            width: width,
                            height: height
                        }));
                    } finally {
                        domUrl.revokeObjectURL(blobUrl);
                    }
                })
                .attr('src', blobUrl);
        });
    };

//START: [MONNGBD-4] multiple range copy to clipboard
function setTooltip(btn, message) {
  $(btn).attr('data-original-title', message)
    .tooltip({placement: 'left',trigger: 'click'})
    .tooltip('show');
}

function hideTooltip(btn) {
    $(btn).tooltip('hide').removeAttr('data-original-title');
}
$(document).ready(function(){
  var clipboard = new Clipboard('.select-to-copy-btn', {
    text: function(trigger) {
      var element = $('.select-to-copy-btn').attr('data-copy-element').split('&');

      var total_element = element.length;
      var last_element = element.length - 1;

      var content = '';

      for( var i = 0; i <  total_element; i ++ ){
        $(element[i]).css('border','4px solid #2ABC98');
        content += $(element[i]).html();
      }

      $(element[last_element]).find('.sticky-wrap').css('margin','0');
      $(element[last_element]).css('margin','10px 0');

      $('#copy-text-container').html(content);
      setTooltip('.select-to-copy-btn', 'Copied to clipboard'); //Show tooltip

      setTimeout(function(){
        for( var i = 0; i <  total_element; i ++ ){
          $(element[i]).css('border','0px');
        }
        $(element[last_element]).find('.sticky-wrap').css('margin','0');
        hideTooltip('.select-to-copy-btn'); //Hide tooltip
      }, 2000);

      return;
    }
  });
});
//END: [MONNGBD-4] multiple copy to clipboard


