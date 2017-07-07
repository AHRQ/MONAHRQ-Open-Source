/**
 * Monahrq Nest
 * Services Module
 * Walkthrough Service
 * This service allows for state keeping while users are moving through walkthrough,
 * Setting options for intro.js,
 * Defining Walkthrough text
 */
(function () {
    'use strict';

    /**
     * Angular wiring
     */
    angular.module('monahrq.services')
      .factory('WalkthroughSvc', WalkthroughSvc);

    WalkthroughSvc.$inject = ['nga11yAnnounce'];
    function WalkthroughSvc(nga11yAnnounce) {
        /**
         * Service Interface
         */
        return {
            IntroOptionsLocation: IntroOptionsLocation,
            IntroOptionsLocationNoResults: IntroOptionsLocationNoResults,
            IntroOptionsLanding: IntroOptionsLanding,
            IntroOptionsTopic: IntroOptionsTopic,
            IntroOptionsCompare: IntroOptionsCompare,
            IntroOptionsNHLocation: IntroOptionsNHLocation,
            IntroOptionsNHLocationNoResults: IntroOptionsNHLocationNoResults,
            IntroOptionsPhysiciansSearch: IntroOptionsPhysiciansSearch,
            IntroOptionsPhysiciansSearchResults: IntroOptionsPhysiciansSearchResults,
            getIntroIsRunning: getIntroIsRunning,
            setIntroIsRunning: setIntroIsRunning,
            BeforeChangeEvent: BeforeChangeEvent,
            OnExit: OnExit,
            hasClosedWalkthrough: hasClosedWalkthrough,
            setSearchComplete: setSearchComplete,
            getSearchComplete: getSearchComplete,
            screenRead: screenRead
        };


        /**
         * Service Implementation
         */

        var isRunning = false,
        hasFinishedSearch = {
            'hosp': false,
            'nh': false,
            'ph': false
        };

        function setIntroIsRunning(val) {
            isRunning = val;
        }
        function getIntroIsRunning() {
            return isRunning;
        }

        function setSearchComplete(val, page) {
            hasFinishedSearch[page] = val;
        }

        function getSearchComplete(page) {
            return hasFinishedSearch[page];
        }

        function screenRead(steps) {
            var message = '';
            for (var i = 0; i < steps.length; i++) {
                var stepNo = i + 1;
                message += ' Step ' + stepNo + ' ';
                message += steps[i].intro;
            }

            if (message != '') {
                nga11yAnnounce.assertiveAnnounce(message);
            }
        }

        function hasFinishedSearch() {
            return
        }

        function BeforeChangeEvent(targetElement, scope, steps) {
            var message = '',
                id = targetElement.getAttribute('id'),
                messageObj = steps.filter(function (obj) {
                    return obj.element == '#' + id;
                });

            var message = messageObj[0].intro;
            if (message != '') {
                nga11yAnnounce.assertiveAnnounce(message);
            }
        }

        function hasClosedWalkthrough(page) {
            var nameEQ = page + "=";
            var ca = document.cookie.split(';');
            for (var i = 0; i < ca.length; i++) {
                var c = ca[i];
                while (c.charAt(0) == ' ') c = c.substring(1, c.length);
                if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
            }
            return null;
        }

        function OnExit(page) {
            var d = new Date();
            var y = d.getFullYear() + 10;
            document.cookie = page + "=0; expires=Thu, 18 Dec " + y + " 12:00:00 UTC";
        }

        function IntroOptionsCompare() {
            return {
                steps: [
                {
                    element: '#hospitalToCompare0',
                    intro: "The hospitals you selected to compare appear at the top."
                },
                {
                    element: '#topic-item0',
                    intro: "Select one or more health topics to see quality ratings and compare the hospitals you selected."
                },
                {
                    element: '#report__no-resultsText',
                    intro: 'Once you\'ve selected your health topic(s), quality ratings for that health topic will appear for each hospital you selected.'
                },
                {
                    element: '#report__no-resultsText',
                    intro: 'If your topic has a "cost for care" associated, you may click that button to compare the average cost of care at each facility for the specified topic.'
                },
                {
                    element: '#compareTo-national',
                    intro: 'The results displayed are automatically compared to other hospitals in the state (State Average). You can change the display to compare your results to other hospitals in the U.S. (National Average).'
                },
                {
                    element: '#compareDifferentHospitalsBtn',
                    intro: 'If you want to compare different hospitals, you can click "Select different hospitals" to change your hospital selection at any time.'
                }
                ],
                showStepNumbers: true,
                exitOnOverlayClick: true,
                tooltipClass: 'srhide',
                exitOnEsc: true,
                nextLabel: '<strong>NEXT</strong>',
                prevLabel: '<strong>PREV</strong>',
                skipLabel: 'Exit',
                dontLabel: 'Thank you'
            };
        }

        function IntroOptionsPhysiciansSearchResults() {
            return {
                steps: [
                {
                    element: '#doc0',
                    intro: 'If you want to view ratings for only one physician, click on the physician\'s name.'
                },
                {
                    element: '#sortBy',
                    intro: 'You can sort your results by any of the criteria in this menu.'
                },
                {
                    element: '#searchContainer',
                    intro: 'You can widen or narrow your search by selecting different options to search.'
                },
                {
                    element: '#resultsHeader',
                    intro: 'If available, use the quality ratings to see how the physicians performed.'
                }
                ],
                showStepNumbers: true,
                exitOnOverlayClick: true,
                tooltipClass: 'srhide',
                exitOnEsc: true,
                nextLabel: '<strong>NEXT</strong>',
                prevLabel: '<strong>PREV</strong>',
                skipLabel: 'Exit',
                dontLabel: 'Thank you'
            };
        }

        function IntroOptionsPhysiciansSearch() {
            return {
                steps: [
                {
                    element: '#searchStepOne',
                    intro: "Select a search type by which you want to find your doctor."
                },
                {
                    element: '#searchStepTwo',
                    intro: "More fields will appear based on your search type. Select an option, or type more details to narrow your search. <img src=\"themes/consumer/assets/images/doctorOptions.gif\" width=\"550\" alt=\"further options for doctor search\"/>"
                },
                {
                    element: '#searchStepThree',
                    intro: 'Click "Show Results Below" to see a list of doctors based on your search criteria.<img src="themes/consumer/assets/images/showResults.gif" width="550" alt="Click Show resutls button"/>'
                }
                ],
                showStepNumbers: true,
                exitOnOverlayClick: true,
                tooltipClass: 'srhide',
                exitOnEsc: true,
                nextLabel: '<strong>NEXT</strong>',
                prevLabel: '<strong>PREV</strong>',
                skipLabel: 'Exit',
                dontLabel: 'Thank you'
            };
        }

        function IntroOptionsCompare() {
            return {
                steps: [
                {
                    element: '#hospitalToCompare0',
                    intro: "The hospitals you selected to compare appear at the top."
                },
                {
                    element: '#topic-item0',
                    intro: "Select one or more health topics to see quality ratings and compare the hospitals you selected."
                },
                {
                    element: '#report__no-resultsText',
                    intro: 'Once you\'ve selected your health topic(s), quality ratings for that health topic will appear for each hospital you selected.'
                },
                {
                    element: '#report__no-resultsText',
                    intro: 'If your topic has a "cost for care" associated, you may click that button to compare the average cost of care at each facility for the specified topic.'
                },
                {
                    element: '#compareTo-national',
                    intro: 'The results displayed are automatically compared to other hospitals in the state (State Average). You can change the display to compare your results to other hospitals in the U.S. (National Average).'
                },
                {
                    element: '#compareDifferentHospitalsBtn',
                    intro: 'If you want to compare different hospitals, you can click "Select different hospitals" to change your hospital selection at any time.'
                }
                ],
                showStepNumbers: true,
                exitOnOverlayClick: true,
                tooltipClass: 'srhide',
                exitOnEsc: true,
                nextLabel: '<strong>NEXT</strong>',
                prevLabel: '<strong>PREV</strong>',
                skipLabel: 'Exit',
                dontLabel: 'Thank you'
            };
        }

        function IntroOptionsTopic() {
            return {
                steps: [
                {
                    element: '#subtopicTab0',
                    intro: "Click on the colored tabs to see different quality ratings for each subtopic."
                },
                {
                    element: '#measureKey0',
                    intro: "The quality ratings within each subtopic appear in this table for each of the hospitals in the list below."
                },
                {
                    element: '#search-distance',
                    intro: "You can refine your results by entering a zip code and widening or narrowing your search area."
                },
                {
                    element: '#selectDifferentTopicLink',
                    intro: "If you would like to view another health topic, you can click \"Select a different topic\" and choose a new health topic from the list"
                },
                {
                    element: '#learnMoreVisualReportBtn',
                    intro: "Click \"Learn more about this topic with our visual report\" to be taken back to the visual report for this health topic. Visual reports provide definitions of the health topic, interesting facts, and handy tips and checklists for you to consider before you compare hospitals."
                },
                {
                    element: '#compareButton',
                    intro: "Compare hospitals by selecting two to five hospitals and clicking \"COMPARE\" at the top or bottom of the table. <a href=\"/#/consumer/hospitals/location\">Need more help comparing</a>?"
                }
                ],
                showStepNumbers: true,
                exitOnOverlayClick: true,
                tooltipClass: 'srhide',
                exitOnEsc: true,
                nextLabel: '<strong>NEXT</strong>',
                prevLabel: '<strong>PREV</strong>',
                skipLabel: 'Exit',
                dontLabel: 'Thank you'
            };
        }

        function IntroOptionsLanding() {
            return {
                steps: [
                {
                    element: '#step1',
                    intro: "Select the type of health care provider you want to find."
                },
                {
                    element: '#step2',
                    intro: "Enter a street address, city, or zip code in the search box to find health care providers in your area."
                },
                {
                    element: '#step3',
                    intro: "Click \"SEARCH\" to see the list of health care providers in your area along with quality ratings for those providers."
                }],
                showStepNumbers: true,
                exitOnOverlayClick: true,
                tooltipClass: 'srhide',
                exitOnEsc: true,
                nextLabel: '<strong>NEXT</strong>',
                prevLabel: '<strong>PREV</strong>',
                skipLabel: 'Exit',
                dontLabel: 'Thank you'
            };
        }

        function IntroOptionsNHLocationNoResults() {
            return {
                steps: [
                {
                    element: '#search-location',
                    intro: "Enter a street address, city, or zip code in the search box to find nursing homes in your area."
                },
                {
                    element: '#search-distance',
                    intro: "You can narrow your search area by selecting within how many miles you would like to search."
                },
                {
                    element: '#updateSearch',
                    intro: "Click \"Show Results Below\" to see the list of nursing homes in your area along with quality ratings for those providers."
                }],
                showStepNumbers: true,
                exitOnOverlayClick: true,
                tooltipClass: 'srhide',
                exitOnEsc: true,
                nextLabel: '<strong>NEXT</strong>',
                prevLabel: '<strong>PREV</strong>',
                skipLabel: 'Exit',
                dontLabel: 'Thank you'
            };
        };

        function IntroOptionsNHLocation() {
            return {
                steps: [
                {
                    element: '#check0',
                    intro: "Once you have found the nursing homes you want to compare, check the boxes next to the name of the nursing homes."
                },
                {
                    element: '#check1',
                    intro: "You may select 2-5 nursing homes to compare."
                },
                {
                    element: '#compareBtn',
                    intro: "Once you have selected the nursing homes you want to compare, click \"Compare\" to see the quality ratings of these nursing homes."
                },
                {
                    element: '#nhName0',
                    intro: "If you want to view ratings for only one nursing home, click on the nursing home name."
                },
                {
                    element: '#sortBy',
                    intro: "You can sort your results by any of the criteria in this menu."
                },
                {
                    element: '#mapResultsLink',
                    intro: "Click \"View these results on a map\" to see the exact location of the nursing home."
                },
                {
                    element: '#compareResultsWrapper',
                    intro: "The results displayed are automatically compared to other nursing homes in the the U.S. (National Average). You can change the display to compare your results to other nursing homes in the state (State Average) or in the county (County Average)."
                },
                {
                    element: '#serch-form__contain',
                    intro: "You can widen or narrow your search area by selecting a distance range to search."
                },
                {
                    element: '#rating0',
                    intro: "Browse the quality ratings to see how the nursing home performed. Then use the quality ratings to help you select which nursing homes to compare further."
                }
                ],
                showStepNumbers: true,
                exitOnOverlayClick: true,
                tooltipClass: 'srhide',
                exitOnEsc: true,
                nextLabel: '<strong>NEXT</strong>',
                prevLabel: '<strong>PREV</strong>',
                skipLabel: 'Exit',
                dontLabel: 'Thank you'
            };
        };

        function IntroOptionsLocationNoResults() {
            return {
                steps: [
                {
                    element: '#search-location',
                    intro: "Enter a street address, city, or zip code in the search box to find hospitals in your area."
                },
                {
                    element: '#search-distance',
                    intro: "You can narrow your search area by selecting within how many miles you would like to search."
                },
                {
                    element: '#updateSearch',
                    intro: "Click \"Show Results Below\" to see the list of hospitals in your area along with quality ratings for those providers."
                }
                ],
                showStepNumbers: true,
                exitOnOverlayClick: true,
                tooltipClass: 'srhide',
                exitOnEsc: true,
                nextLabel: '<strong>NEXT</strong>',
                prevLabel: '<strong>PREV</strong>',
                skipLabel: 'Exit',
                dontLabel: 'Thank you'
            };
        };



        function IntroOptionsLocation() {
            return {
                steps: [
                {
                    element: '#check0',
                    intro: "Once you have found the hospitals you want to compare, check the boxes next to the name of the hospitals."
                },
                {
                    element: '#check1',
                    intro: "You may select 2-5 hospitals."
                },
                {
                    element: '#compareBtn',
                    intro: "Once you have selected the hospitals you want to compare, click \"Compare\" to see the quality ratings of these hospitals."
                },
                {
                    element: '#hospitalName0',
                    intro: "If you want to view ratings for only one hospital, click on the hospital name."
                },
                {
                    element: '#sortBy',
                    intro: "You can sort your results by any of the criteria in this menu."
                },
                {
                    element: '#mapResultsLink',
                    intro: "Click \"View these results on a map\" to see the exact location of the hospital."
                },
                {
                    element: '#compareResultsWrapper',
                    intro: "The results displayed are automatically compared to other hospitals in the state (State Average). You can change the display to compare your results to other hospitals in the U.S. (National Average)."
                },
                {
                    element: '#serch-form__contain',
                    intro: "You can widen or narrow your search area by selecting a distance range to search."
                },
                {
                    element: '#patient-rec0',
                    intro: "Browse the quality ratings to see how the hospital performed. Then use the quality ratings to help you select which hospitals to compare further."
                }
                ],
                showStepNumbers: true,
                exitOnOverlayClick: true,
                tooltipClass: 'srhide',
                exitOnEsc: true,
                nextLabel: '<strong>NEXT</strong>',
                prevLabel: '<strong>PREV</strong>',
                skipLabel: 'Exit',
                dontLabel: 'Thank you'
            };
        }
    }

})();