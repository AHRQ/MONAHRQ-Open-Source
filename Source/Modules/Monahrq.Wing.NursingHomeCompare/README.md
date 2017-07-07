# Nursing Home Comparison
What is it?

## Data Source
Nursing Home comparison data may be obtained from the Medicare website at
https://data.medicare.gov/data/nursing-home-compare. This data is published as a 
Microsoft Access database (mdb) and as a collection of CSV files. MONAHRQ consumes
the Access-formatted version of the data.

## Handling Different Schema Versions
Different versions of the nursing home comparison data set have been published with
different schemas. To handle the different schemas, the NursingHomeCompare Wing 
queries the database to determine the schema version and then executes an import
procedure that matches the schema version. The schema version is determined by 
computing an MD5 hash of the column names of all tables in the Access database and
comparing the hash to known hash values in the `Resources\NursingHomeCompare\version.txt` 
file. This file helps determine which import script to use.

If a new database is published with a new schema, new import scripts will need to
be added to the `Resources\NursingHomeCompare` directory and the existing 
`version.txt` file will need to be updated to include the new schema information.

## Import Procedure
The import logic is implemented in the `StartImport()` method of 
`NHC\ViewModel\ProcessFileViewModel.cs`. The import process may be roughly 
summarized as the following steps:

1. Obtain the schema of the Access database and compute an MD5 hash of the column names
2. Find a match for the computed value in the `version.txt` file
4. Load data into `NursingHomes` table
   1. Delete existing data
   2. Execute the `[SchemaVersion]_Provider.sql` file against the Access database, saving the results to an in-memory `DataTable`
   3. Bulk load the `DataTable` into the `NursingHomes` table
   4. Adjust data according to the contents of the `NursingHomes_Audits` table
5. Load data into `Targets_NursingHomeTargets` table
   1. Execute the `[SchemaVersion]_Target.sql` file against the Access database, saving the results to an in-memory `DataTable`
   2. Bulk load the `DataTable` into the `Targets_NursingHomeTargets` table