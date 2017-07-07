/**
 * Professional Product
 * Quality Ratings Module
 * Quality Ratings Page Controller
 *
 * This controller handles the quality ratings landing page.
 */
(function() {
  'use strict';

  /**
   * Angular wiring
   */
  angular.module('monahrq.products.professional.quality-ratings')
    .controller('QualityRatingsCtrl', QualityRatingsCtrl);


  QualityRatingsCtrl.$inject = ['$scope', '$state', 'ModalTopicCategorySvc', 'content', 'measureTopicCategories'];
  function QualityRatingsCtrl($scope, $state, ModalTopicCategorySvc, content, measureTopicCategories) {
    $scope.content = content;
    $scope.topics = _.sortBy(measureTopicCategories, function(t){
      return t.Name;
    });

    $scope.gotoTopic = function(topicId){
      $state.go('top.professional.quality-ratings.condition', {
        topic: topicId
      });
    };

    $scope.modalTopicCategory = function(id) {
      ModalTopicCategorySvc.open(id);
    };

    $scope.showQRSearch = $scope.ReportConfigSvc.webElementAvailable('Quality_ConditionTopicExplore_Button')
      || $scope.ReportConfigSvc.webElementAvailable('Quality_HospitalExplore_Button');
  }

})();

