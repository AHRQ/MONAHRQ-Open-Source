# Hospital Comparison
What is it?

## Data Source
Hospital comparison data may be obtained from the Medicare website at
https://data.medicare.gov/data/hospital-compare. This data is published as a 
Microsoft Access database (mdb) and as a collection of CSV files. MONAHRQ consumes
the Access-formatted version of the data.

## Handling Different Schema Versions
Different versions of the hospital comparison data set have been published with
different schemas. To handle the different schemas, the HospitalCompare Wing 
queries the database to determine the schema version and then executes an import
procedure that matches the schema version. The logic for this query may be found
in the `Resources\HospitalCompare\version.sql`. Note that this script is 
intended to be executed against a Microsoft Access database.

If a new database is published with a new schema, a new `Schema.Version.n.sql` 
script (also in `Resources\HospitalCompare`) will need to be created, and the 
existing `version.sql` script will need to be updated to correctly identify the
new schema version.

## Import Procedure
The import logic is implemented in the `StartImport()` method of 
`ViewModel\ProcessFileViewModel.cs`. The import process may be roughly 
summarized as the following steps:

1. Execute the `Resources\HospitalCompare\version.sql` file against the database to identify the schema version
2. Parse the appropriate `Resources\HospitalCompare\Schema.Version.n.sql` import script
3. For each select command in the import script:
   1. Execute the statement and use the result set to populate an in-memory `DataTable` instance (note: some statements have special handling)
   2. Bulk load the `DataTable` into the `Targets_HospitalCompareTargets`table of the MONAHRQ database
