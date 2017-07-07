/**
 * Consumer Product
 * Hospital Reports Module
 * Topic Map Page Controller
 *
 * This controller generates the same quality measures report by hospital as the tabular
 * controller, but renders the hospitals as pins on a map using the google maps api.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.hospitals')
    .controller('CHTopicMapCtrl', CHTopicMapCtrl);


  CHTopicMapCtrl.$inject = ['$scope', '$state', '$stateParams', '$timeout', 'HospitalRepositorySvc', 'CHReportSvc',
    'UserStateSvc', 'MapMarkerSvc', 'zipDistances', 'topics', 'subtopics'];
  function CHTopicMapCtrl($scope, $state, $stateParams, $timeout, HospitalRepositorySvc, CHReportSvc,
                          UserStateSvc, MapMarkerSvc, zipDistances, topics, subtopics) {
    var report;

    $scope.reportId = $state.current.data.report;
    $scope.hasResults = false;
    $scope.hasSearch = false;
    $scope.query = {};
    $scope.zipDistances = zipDistances;
    $scope.topics = [];
    $scope.subTopics = [];

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
      $scope.query.topicId = $stateParams.topicId ? +$stateParams.topicId : null;
      $scope.query.subtopicId = $stateParams.subtopicId ? +$stateParams.subtopicId : null;
      $scope.query.zip = $stateParams.zip;
      $scope.query.distance = +$stateParams.distance;

      buildTopics();
      if ($scope.query.topicId) {
        var t = _.findWhere(topics, {TopicCategoryID: $scope.query.topicId});
        if (t) {
          $scope.topicName = t.Name;
        }
        buildSubTopics($scope.query.topicId);
      }

      if (!$scope.config.DE_IDENTIFICATION) {
        HospitalRepositorySvc.init()
          .then(loadReport);
      }
      else {
        $scope.hasSearch = true;
        $scope.hasResults = true;
        $('.report').focus();
      }
    }

    function buildTopics() {
      $scope.topics = _.map(topics, function(topic) {
        return {
          id: topic.TopicCategoryID,
          name: topic.Name
        };
      });
    }

    function buildSubTopics(topicId) {
      $scope.subtopics = _.map(_.filter(subtopics, function(subtopic) {
        return subtopic.TopicCategoryID == topicId;
      }),
      function(subtopic) {
        return {
          id: subtopic.TopicID,
          name: subtopic.Name,
          measures: subtopic.Measures
        }
      });

      if ($scope.query.subtopicId === null) {
        $scope.query.subtopicId = _.first($scope.subtopics).id;
      }
    }

    function loadReport() {
      var query, measureIds;

      $scope.hasSearch = true;
      $scope.hasResults = true;

      query = $scope.query;
      if (!query.topicId || !query.subtopicId) return;

      var subtopic = _.findWhere($scope.subtopics, {id: +query.subtopicId});
      measureIds = subtopic.measures;
      measureIds.push($scope.config.HOSPITAL_OVERALL_ID);

      CHReportSvc.getHospitalMeasuresReport(measureIds, $scope.query.zip, $scope.query.distance)
        .then(function(_report) {
          report = _report;
          updateMap();
          $('.report').focus();
        })
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
        var ratingField = getRatingField();
        var rating = _.has(row, $scope.config.HOSPITAL_OVERALL_ID) && _.has(row[$scope.config.HOSPITAL_OVERALL_ID], ratingField)
          ? row[$scope.config.HOSPITAL_OVERALL_ID][ratingField]
          : null;

        var marker = new google.maps.Marker({
          position: new google.maps.LatLng(row.LatLng[0], row.LatLng[1]),
          map: model.map,
          id: id,
          icon: rating ? MapMarkerSvc.markerForRating(rating) : null
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
      var ratingField = 'NatRating';
      if (UserStateSvc.get(UserStateSvc.props.C_GEO_CONTEXT_HOSPITAL) === 'state') {
        ratingField = 'PeerRating';
      }
      return ratingField;
    }

    function updateMarkerData(marker) {
      var ratingField = getRatingField();
      var profile = _.findWhere(report, {id: marker.id});
      $scope.mapModel.currentMarkerData = {
        id: marker.id,
        name: profile.name,
        address: profile.city + ', ' + profile.state,
        rating: _.has(profile, $scope.config.HOSPITAL_OVERALL_ID) ? profile[$scope.config.HOSPITAL_OVERALL_ID][ratingField] : null
      };
    }

    function updateSearch(type) {
      if (type == 'topic') {
        buildSubTopics($scope.query.topicId);
        $scope.query.subtopicId = _.first($scope.subtopics).id;
      }

      $state.go('^.topic-map', {
        topicId: $scope.query.topicId,
        subtopicId: $scope.query.subtopicId,
        zip: $scope.query.zip,
        distance: $scope.query.distance
      });
    }

    function gotoTable() {
      $state.go('^.topic', {
        topicId: $scope.query.topicId,
        subtopicId: $scope.query.subtopicId,
        zip: $scope.query.zip,
        distance: $scope.query.distance
      });
    }

  }

})();


