/**
 * Consumer Product
 * Hospital Reports Module
 * Infographic Page
 *
 * This controller generates the generic infographic page for hospitals.
 */
(function() {
  'use strict';


  /**
   * Angular wiring
   */
  angular.module('monahrq.products.consumer.hospitals')
    .controller('CHInfographicCtrl', CHInfographicCtrl);

  CHInfographicCtrl.$inject = ['$scope', '$stateParams', 'topics'];
  function CHInfographicCtrl($scope, $stateParams, topics) {
    $scope.showSurgical = showSurgical;

    init();

    function init() {
      $scope.preview = false;
      $scope.topicCategoryId = +$stateParams.id;

      var t = _.findWhere(topics, {TopicCategoryID: $scope.topicCategoryId});
      if (t) {
        $scope.topicName = t.Name;
      }
    }

    function showSurgical() {
      var name = $scope.topicName.toLowerCase();
      var terms = ['surgeries', 'surgery', 'operation', 'operations', 'surgical', 'surgically', 'operative'];

      /*if ($scope.query.topicId == 21) {// Surgical patient safety
        return false;
      }*/

      for (var i = 0; i < terms.length; i++) {
        var term = terms[i];
        if (name.indexOf(term) >= 0) {
          return true;
        }
      }

      return false;
    }

  }

})();
