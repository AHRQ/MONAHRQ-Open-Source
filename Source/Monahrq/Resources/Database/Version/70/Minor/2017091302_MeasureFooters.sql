UPDATE Topics 
SET [Description] = 'Weighted national estimates from HCUP National (Nationwide) Emergency Department Sample (NEDS), 2014, Agency for Healthcare Research and Quality (AHRQ), based on data collected by individual States and provided to AHRQ by the States. Total number of weighted visits in the U.S. based on HCUP NEDS = 137,807,901. Statistics based on estimates with a relative standard error (standard error / weighted estimate) greater than 0.30 or with standard error = 0 in the nationwide statistics (NIS, NEDS, and KID) are not reliable. These statistics are suppressed and are designated with an asterisk (*). The estimates of standard errors in HCUPnet were calculated using SUDAAN software. These estimates may differ slightly if other software packages are used to calculate variances.'
WHERE [Name] = 'ED Utilization'

UPDATE Topics 
SET [Description] = 'Weighted national estimates from HCUP National (Nationwide) Inpatient Sample (NIS), 2014, Agency for Healthcare Research and Quality (AHRQ), based on data collected by individual States and provided to AHRQ by the States. Total number of weighted discharges in the U.S. based on HCUP NIS = 35,358,818. Statistics based on estimates with a relative standard error (standard error / weighted estimate) greater than 0.30 or with standard error = 0 in the nationwide statistics (NIS, NEDS, and KID) are not reliable. These statistics are suppressed and are designated with an asterisk (*). The estimates of standard errors in HCUPnet were calculated using SUDAAN software. These estimates may differ slightly if other software packages are used to calculate variances.'
WHERE [Name] = 'Inpatient Hospital Utilization'

UPDATE Reports
SET Footnote = '<p> * Weighted national estimates from HCUP National (Nationwide) Inpatient Sample (NIS), 2014, Agency for Healthcare Research and Quality (AHRQ), based on data collected by individual States and provided to AHRQ by the States. Total number of weighted discharges in the U.S. based on HCUP NIS = 35,358,818. Statistics based on estimates with a relative standard error (standard error / weighted estimate) greater than 0.30 or with standard error = 0 in the nationwide statistics (NIS, NEDS, and KID) are not reliable. These statistics are suppressed and are designated with an asterisk (*). The estimates of standard errors in HCUPnet were calculated using SUDAAN software. These estimates may differ slightly if other software packages are used to calculate variances.<br/>** All statistics are unadjusted.</p>'
WHERE [Name] IN (
	'County Rates Report',
	'County Rates Detail Report',
	'Inpatient Hospital Discharge Utilization Report',
	'Inpatient Utilization Detail Report',
	'Region Rates Report',
	'Region Rates Detail Report'
)

UPDATE Reports 
SET Footnote = '<p> Weighted national estimates from HCUP National (Nationwide) Emergency Department Sample (NEDS), 2014, Agency for Healthcare Research and Quality (AHRQ), based on data collected by individual States and provided to AHRQ by the States. Total number of weighted visits in the U.S. based on HCUP NEDS = 137,807,901. Statistics based on estimates with a relative standard error (standard error / weighted estimate) greater than 0.30 or with standard error = 0 in the nationwide statistics (NIS, NEDS, and KID) are not reliable. These statistics are suppressed and are designated with an asterisk (*). The estimates of standard errors in HCUPnet were calculated using SUDAAN software. These estimates may differ slightly if other software packages are used to calculate variances.<br/> All statistics are unadjusted.</p>'
WHERE [Name] IN (
	'ED Utilization Detailed Report',
	'Emergency Department Treat-and-Release (ED) Utilization Report'
	)
