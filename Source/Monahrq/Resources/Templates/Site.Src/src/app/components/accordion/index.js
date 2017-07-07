/**
 * WinQI
 * Accordion Component Module
 * Setup
 */
'use strict';

var angular = require('angular');

/**
 * Angular wiring
 */
angular.module('winqi.components.accordion', [])
  .config(config)
  .run(run)
  .directive('winqiAccordion', require('./accordion').accordion)
  .directive('winqiAccordionToggle', require('./accordion').accordionToggle)
  .directive('winqiAccordionPanel', require('./accordion').accordionPanel);

/**
 * Module config
 */
config.$inject = [];
function config() {
}

/**
 * Module run
 */
function run() {
}

