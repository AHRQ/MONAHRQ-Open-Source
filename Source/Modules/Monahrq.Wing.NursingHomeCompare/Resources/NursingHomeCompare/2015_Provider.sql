SELECT
	pi1.PROVNAME as Name,
	pi1.[STATE] as [state],
	switch( 
pi1.CCRC_FACIL ="N" ,0 ,
pi1.CCRC_FACIL ="Y" ,1)
as [IsCCRCFacility],
	switch( 
pi1.SFF ="N",0, 
pi1.SFF ="Y",1
)as [IsSFFacility],
	switch(
pi1.CHOW_LAST_12MOS ="N",0,
pi1.CHOW_LAST_12MOS ="Y",1
)as [ChangedOwnership_12Mos],
0 as [IsDeleted]


      ,pi1.LBN as [LegalBusinessName]
      ,pi1.provnum as [ProviderId]
      ,pi1.[ADDRESS] as[Address]
      ,pi1.CITY as [City]
      ,pi1.ZIP as [Zip]
      ,pi1.COUNTY_SSA as [CountySSA]
      ,pi1.COUNTY_NAME as [CountyName]
      ,pi1.PHONE as [Phone]
      ,pi1.BEDCERT as [NumberCertBeds]
      ,pi1.RESTOT as [NumberResidCertBeds]
      ,pi1.PARTICIPATION_DATE as [ParticipationDate]
      ,pi1.CERTIFICATION as [Certification]
      ,pi1.RESFAMCOUNCIL as [ResFamCouncil]
      ,pi1.SPRINKLER_STATUS as [SprinklerStatus]
      ,switch( 
	  pi1.INHOSP='YES',1,
	  pi1.INHOSP='NO',0
	  ) as [InHospital]
      
      
      ,pi1.FILEDATE as [FileDate]
      
      ,Null as [Description]
      ,Null as [Accreditation]
      ,pi1.OWNERSHIP as [Ownership]
      ,Null as [InRetirementCommunity]
      ,Null as [HasSpecialFocus]
      ,Null as [CategoryType_Id]


FROM
	ProviderInfo pi1;