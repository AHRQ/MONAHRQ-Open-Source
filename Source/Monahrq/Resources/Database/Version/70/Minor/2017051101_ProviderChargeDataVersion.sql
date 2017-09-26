UPDATE Wings_Datasets
SET ReportingYear = VersionYear
WHERE ReportingYear IS NULL 
AND ContentType_Id = 8

UPDATE Wings_Elements 
SET Hints = Name
WHERE Target_Id = 8
AND LEN(Hints) = 0 