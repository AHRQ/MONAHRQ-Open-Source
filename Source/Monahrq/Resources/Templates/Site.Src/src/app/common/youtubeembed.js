(function () {
    'use strict';

    angular.module('monahrq.common', [])
        .directive('youtubeEmbed', [function () {
            return {
                restrict: 'EA',
                scope: {
                    vsrc: "="
                },
                link: function ($scope, element, attrs) {

                    $(function () {
                        $(element).after('<iframe width="300" height="200" src="' + $scope.vsrc + '" frameborder="0" allowfullscreen="" title="Health care quality video"></iframe>');
                        $(element).remove();
                    });
                }
            };
        }]);

}());