IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spExportPhysician]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spExportPhysician]
GO


CREATE PROCEDURE [dbo].[spExportPhysician]
	@selection INT,  -- 0:base data, 1:Physician_Id,State 2:SpecialtyId 3:Zip 4:Practice
	@csvStates varchar(MAX),
	@zipCode varchar(max) = null	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	BEGIN TRY

	IF @selection = -1 -- profiles
		BEGIN
		    SELECT   -- get physicians belonging to a medical practice
					npi,
					PacId as ind_pac_id, 
					ProfEnrollId as ind_enrl_id,
					FirstName as frst_nm, 
					MiddleName as mid_nm, 
					LastName as lst_nm, 
					Suffix as suff, 
					Gender as gndr, 
					[Credentials] as cred,
					MedicalSchoolName as med_sch,
					GraduationYear as grd_yr,
					ISNULL(PrimarySpecialty, 'UNKNOWN') as pri_spec,
					SecondarySpecialty1 as sec_spec_1,
					SecondarySpecialty2 as sec_spec_2,
					SecondarySpecialty3 as sec_spec_3,
					SecondarySpecialty4 as sec_spec_4,
					'' as sec_spec_all,
					AcceptsMedicareAssignment as assgn,
					(CASE WHEN ParticipatesInERX = 0 THEN 'N' ELSE 'Y' END) as erx,
					(CASE WHEN ParticipatesInPQRS = 0 THEN 'N' ELSE 'Y' END) as pqrs,
					(CASE WHEN ParticipatesInEHR = 0 THEN 'N' ELSE 'Y' END) as ehr,
					MP.Name as org_lgl_nm,
					MP.GroupPracticePacId as org_pac_id,
					MP.DBAName as org_dba_nm,
					MP.NumberofGroupPracticeMembers as num_org_mem,
					A.Line1 as adr_ln_1,
					A.Line2 as adr_lin_2,
					A.City as cty,
					A.[State] as st,
					A.ZipCode as zip,
					'' as ln_2_sprs,
					ISNULL(PPP.hosp_afl_1, '') as hosp_afl_1,
					ISNULL(PPP.hosp_afl_2, '') as hosp_afl_2,
					ISNULL(PPP.hosp_afl_3, '') as hosp_afl_3,
					ISNULL(PPP.hosp_afl_4, '') as hosp_afl_4,
					ISNULL(PPP.hosp_afl_5, '') as hosp_afl_5,
					--'' as hosp_afl_1,
					--'' as hosp_afl_2,
					--'' as hosp_afl_3,
					--'' as hosp_afl_4,
					--'' as hosp_afl_5,
					'' as hosp_afl_lbn_1,
					'' as hosp_afl_lbn_2,
					'' as hosp_afl_lbn_3,
					'' as hosp_afl_lbn_4,
					'' as hosp_afl_lbn_5,
					'' as MOC,
					'' as MHI
				FROM dbo.Physicians PHY
				LEFT JOIN dbo.[Physicians_MedicalPractices] PMP ON PHY.Id = PMP.Physician_Id
				LEFT JOIN dbo.MedicalPractices MP ON PMP.MedicalPractice_Id = MP.Id
				LEFT JOIN dbo.Addresses A ON MP.Id = A.MedicalPractice_Id AND UPPER(A.[AddressType]) = 'MEDICALPRACTICE'
				LEFT JOIN dbo.[Physicians_AffiliatedHospitals] PAH ON PHY.Id = PAH.Physician_Id
			   --LEFT JOIN dbo.Hospitals H ON PAH.Hospital_CmsProviderId = H.CmsProviderID  
				LEFT OUTER JOIN (
					select phy_id,
					  max(case when rn = 1 then hosp_id else '' end) hosp_afl_1,
					  max(case when rn = 2 then hosp_id else '' end) hosp_afl_2,
					  max(case when rn = 3 then hosp_id else '' end) hosp_afl_3,
					  max(case when rn = 4 then hosp_id else '' end) hosp_afl_4,
					  max(case when rn = 5 then hosp_id else '' end) hosp_afl_5
					from
					(
					  SELECT phy_id, hosp_id, row_number() over(partition by phy_id order by hosp_id) rn
					  FROM (select P.Id as phy_id, PH.Hospital_CmsProviderId as hosp_id from Physicians P
							INNER JOIN Physicians_AffiliatedHospitals PH
							ON PH.Physician_Id = P.Id ) tt
					) ff
					group by phy_id
				) AS PPP
				ON PAH.Physician_Id = PPP.phy_id
				WHERE A.[State] IN (SELECT * from dbo.fnList2Table(@csvStates, ','))
				and	A.Id in (select * from dbo.fnList2Table(pmp.AssociatedPMPAddresses,','))
				AND PHY.IsDeleted = 0
				
				UNION
				
				SELECT   -- get physicians  hosps
					npi,
					PacId as ind_pac_id, 
					ProfEnrollId as ind_enrl_id,
					FirstName as frst_nm, 
					MiddleName as mid_nm, 
					LastName as lst_nm, 
					Suffix as suff, 
					Gender as gndr, 
					[Credentials] as cred,
					MedicalSchoolName as med_sch,
					GraduationYear as grd_yr,
					ISNULL(PrimarySpecialty,'UNKNOWN') as pri_spec,
					SecondarySpecialty1 as sec_spec_1,
					SecondarySpecialty2 as sec_spec_2,
					SecondarySpecialty3 as sec_spec_3,
					SecondarySpecialty4 as sec_spec_4,
					'' as sec_spec_all,
					AcceptsMedicareAssignment as assgn,
					(CASE WHEN ParticipatesInERX = 0 THEN 'N' ELSE 'Y' END) as erx,
					(CASE WHEN ParticipatesInPQRS = 0 THEN 'N' ELSE 'Y' END) as pqrs,
					(CASE WHEN ParticipatesInEHR = 0 THEN 'N' ELSE 'Y' END) as ehr,
					'' as org_lgl_nm,
					'' as org_pac_id,
					'' as org_dba_nm,
					'' as num_org_mem,
					--H.Address as adr_ln_1,
					'' as adr_ln_1,
					'' as adr_lin_2,
					--H.City as cty,
					--H.[State] as st,
					--H.Zip as zip,
					'' as cty,
					'' as st,
					'' as zip,
					'' as ln_2_sprs,
					ISNULL(PPP.hosp_afl_1, '') as hosp_afl_1,
					ISNULL(PPP.hosp_afl_2, '') as hosp_afl_2,
					ISNULL(PPP.hosp_afl_3, '') as hosp_afl_3,
					ISNULL(PPP.hosp_afl_4, '') as hosp_afl_4,
					ISNULL(PPP.hosp_afl_5, '') as hosp_afl_5,
					'' as hosp_afl_lbn_1,
					'' as hosp_afl_lbn_2,
					'' as hosp_afl_lbn_3,
					'' as hosp_afl_lbn_4,
					'' as hosp_afl_lbn_5,
					'' as MOC,
					'' as MHI
				FROM dbo.Physicians PHY
			LEFT JOIN dbo.[Physicians_AffiliatedHospitals] PAH ON PHY.Id = PAH.Physician_Id
			LEFT OUTER JOIN (
					select phy_id,
					  max(case when rn = 1 then hosp_id else '' end) hosp_afl_1,
					  max(case when rn = 2 then hosp_id else '' end) hosp_afl_2,
					  max(case when rn = 3 then hosp_id else '' end) hosp_afl_3,
					  max(case when rn = 4 then hosp_id else '' end) hosp_afl_4,
					  max(case when rn = 5 then hosp_id else '' end) hosp_afl_5
					from
					(
					  SELECT phy_id, hosp_id, row_number() over(partition by phy_id order by hosp_id) rn
					  FROM (select P.Id as phy_id, PH.Hospital_CmsProviderId as hosp_id from Physicians P
							INNER JOIN Physicians_AffiliatedHospitals PH
							ON PH.Physician_Id = P.Id ) tt
					) ff
					group by phy_id
				) AS PPP
				ON PAH.Physician_Id = PPP.phy_id
				WHERE 
				PHY.States IN (select * from dbo.fnList2Table(@csvStates, ',')) 
				and PHY.IsDeleted = 0 
				
				UNION
				
				SELECT   -- get physicians solo
					npi,
					PacId as ind_pac_id, 
					ProfEnrollId as ind_enrl_id,
					FirstName as frst_nm, 
					MiddleName as mid_nm, 
					LastName as lst_nm, 
					Suffix as suff, 
					Gender as gndr, 
					[Credentials] as cred,
					MedicalSchoolName as med_sch,
					GraduationYear as grd_yr,
					ISNULL(PrimarySpecialty, 'UNKNOWN') as pri_spec,
					ISNULL(SecondarySpecialty1, -1) as sec_spec_1,
					ISNULL(SecondarySpecialty2, -1) as sec_spec_2,
					ISNULL(SecondarySpecialty3, -1) as sec_spec_3,
					ISNULL(SecondarySpecialty4, -1) as sec_spec_4,
					'' as sec_spec_all,
					AcceptsMedicareAssignment as assgn,
					(CASE WHEN ParticipatesInERX = 0 THEN 'N' ELSE 'Y' END) as erx,
					(CASE WHEN ParticipatesInPQRS = 0 THEN 'N' ELSE 'Y' END) as pqrs,
					(CASE WHEN ParticipatesInEHR = 0 THEN 'N' ELSE 'Y' END) as ehr,
					'' as org_lgl_nm,
					'' as org_pac_id,
					'' as org_dba_nm,
					'' as num_org_mem,
					A.Line1 as adr_ln_1,
					A.Line2 as adr_lin_2,
					A.City as cty,
					A.[State] as st,
					A.ZipCode as zip,
					'' as ln_2_sprs,
					--'' as hosp_afl_1,
					--'' as hosp_afl_2,
					--'' as hosp_afl_3,
					--'' as hosp_afl_4,
					--'' as hosp_afl_5,
					ISNULL(PPP.hosp_afl_1, '') as hosp_afl_1,
					ISNULL(PPP.hosp_afl_2, '') as hosp_afl_2,
					ISNULL(PPP.hosp_afl_3, '') as hosp_afl_3,
					ISNULL(PPP.hosp_afl_4, '') as hosp_afl_4,
					ISNULL(PPP.hosp_afl_5, '') as hosp_afl_5,
					'' as hosp_afl_lbn_1,
					'' as hosp_afl_lbn_2,
					'' as hosp_afl_lbn_3,
					'' as hosp_afl_lbn_4,
					'' as hosp_afl_lbn_5,
					'' as MOC,
					'' as MHI
				FROM dbo.Physicians PHY
				INNER JOIN dbo.Addresses A ON PHY.Id = A.Physician_Id AND UPPER(A.[AddressType]) = 'PHYSICIAN'
				LEFT JOIN dbo.[Physicians_AffiliatedHospitals] PAH ON PHY.Id = PAH.Physician_Id
				LEFT OUTER JOIN (
					select phy_id,
					  max(case when rn = 1 then hosp_id else null end) hosp_afl_1,
					  max(case when rn = 2 then hosp_id else null end) hosp_afl_2,
					  max(case when rn = 3 then hosp_id else null end) hosp_afl_3,
					  max(case when rn = 4 then hosp_id else null end) hosp_afl_4,
					  max(case when rn = 5 then hosp_id else null end) hosp_afl_5
					from
					(
					  SELECT phy_id, hosp_id, row_number() over(partition by phy_id order by hosp_id) rn
					  FROM (select P.Id as phy_id, PH.Hospital_CmsProviderId as hosp_id from Physicians P
							INNER JOIN Physicians_AffiliatedHospitals PH
							ON PH.Physician_Id = P.Id ) tt
					) ff
					group by phy_id
				) AS PPP
				ON PAH.Physician_Id = PPP.phy_id
				WHERE  A.State IN (select * from dbo.fnList2Table(@csvStates, ',')) 
				and PHY.IsDeleted = 0 
				ORDER BY NPI, ZIP DESC;
			/*
			SELECT   -- get physicians belonging to a medical practice
					npi,
					PacId as ind_pac_id, 
					ProfEnrollId as ind_enrl_id,
					FirstName as frst_nm, 
					MiddleName as mid_nm, 
					LastName as lst_nm, 
					Suffix as suff, 
					Gender as gndr, 
					[Credentials] as cred,
					MedicalSchoolName as med_sch,
					GraduationYear as grd_yr,
					ISNULL(PrimarySpecialty, 'UNKNOWN') as pri_spec,
					SecondarySpecialty1 as sec_spec_1,
					SecondarySpecialty2 as sec_spec_2,
					SecondarySpecialty3 as sec_spec_3,
					SecondarySpecialty4 as sec_spec_4,
					'' as sec_spec_all,
					AcceptsMedicareAssignment as assgn,
					(CASE WHEN ParticipatesInERX = 0 THEN 'N' ELSE 'Y' END) as erx,
					(CASE WHEN ParticipatesInPQRS = 0 THEN 'N' ELSE 'Y' END) as pqrs,
					(CASE WHEN ParticipatesInEHR = 0 THEN 'N' ELSE 'Y' END) as ehr,
					MP.Name as org_lgl_nm,
					MP.GroupPracticePacId as org_pac_id,
					MP.DBAName as org_dba_nm,
					MP.NumberofGroupPracticeMembers as num_org_mem,
					A.Line1 as adr_ln_1,
					A.Line2 as adr_lin_2,
					A.City as cty,
					A.[State] as st,
					A.ZipCode as zip,
					'' as ln_2_sprs,
					'' as hosp_afl_1,
					'' as hosp_afl_2,
					'' as hosp_afl_3,
					'' as hosp_afl_4,
					'' as hosp_afl_5,
					'' as hosp_afl_lbn_1,
					'' as hosp_afl_lbn_2,
					'' as hosp_afl_lbn_3,
					'' as hosp_afl_lbn_4,
					'' as hosp_afl_lbn_5,
					'' as MOC,
					'' as MHI
				FROM dbo.Physicians PHY
				INNER JOIN dbo.[Physicians_MedicalPractices] PMP ON PHY.Id = PMP.Physician_Id
				INNER JOIN dbo.MedicalPractices MP ON PMP.MedicalPractice_Id = MP.Id
				INNER JOIN dbo.Addresses A ON MP.Id = A.MedicalPractice_Id AND UPPER(A.[AddressType]) = 'MEDICALPRACTICE'
				WHERE A.[State] IN (SELECT * from dbo.fnList2Table(@csvStates, ','))
				and	A.Id in (select * from dbo.fnList2Table(pmp.AssociatedPMPAddresses,','))
				AND PHY.IsDeleted = 0
				
				UNION
				
				SELECT   -- get physicians belonging to a medical practice, hosps
					npi,
					PacId as ind_pac_id, 
					ProfEnrollId as ind_enrl_id,
					FirstName as frst_nm, 
					MiddleName as mid_nm, 
					LastName as lst_nm, 
					Suffix as suff, 
					Gender as gndr, 
					[Credentials] as cred,
					MedicalSchoolName as med_sch,
					GraduationYear as grd_yr,
					ISNULL(PrimarySpecialty,'UNKNOWN') as pri_spec,
					SecondarySpecialty1 as sec_spec_1,
					SecondarySpecialty2 as sec_spec_2,
					SecondarySpecialty3 as sec_spec_3,
					SecondarySpecialty4 as sec_spec_4,
					'' as sec_spec_all,
					AcceptsMedicareAssignment as assgn,
					(CASE WHEN ParticipatesInERX = 0 THEN 'N' ELSE 'Y' END) as erx,
					(CASE WHEN ParticipatesInPQRS = 0 THEN 'N' ELSE 'Y' END) as pqrs,
					(CASE WHEN ParticipatesInEHR = 0 THEN 'N' ELSE 'Y' END) as ehr,
					'' as org_lgl_nm,
					'' as org_pac_id,
					'' as org_dba_nm,
					'' as num_org_mem,
					--H.Address as adr_ln_1,
					'' as adr_ln_1,
					'' as adr_lin_2,
					--H.City as cty,
					--H.[State] as st,
					--H.Zip as zip,
					'' as cty,
					'' as st,
					'' as zip,
					'' as ln_2_sprs,
					PPP.hosp_afl_1 as hosp_afl_1,
					PPP.hosp_afl_2 as hosp_afl_2,
					PPP.hosp_afl_3 as hosp_afl_3,
					PPP.hosp_afl_4 as hosp_afl_4,
					PPP.hosp_afl_5 as hosp_afl_5,
					'' as hosp_afl_lbn_1,
					'' as hosp_afl_lbn_2,
					'' as hosp_afl_lbn_3,
					'' as hosp_afl_lbn_4,
					'' as hosp_afl_lbn_5,
					'' as MOC,
					'' as MHI
				FROM dbo.Physicians PHY
			INNER JOIN dbo.[Physicians_AffiliatedHospitals] PAH ON PHY.Id = PAH.Physician_Id
			INNER JOIN dbo.Hospitals H ON PAH.Hospital_CmsProviderId = H.CmsProviderID  
				inner JOIN (
					select phy_id,
					  max(case when rn = 1 then hosp_id else '' end) hosp_afl_1,
					  max(case when rn = 2 then hosp_id else '' end) hosp_afl_2,
					  max(case when rn = 3 then hosp_id else '' end) hosp_afl_3,
					  max(case when rn = 4 then hosp_id else '' end) hosp_afl_4,
					  max(case when rn = 5 then hosp_id else '' end) hosp_afl_5
					from
					(
					  SELECT phy_id, hosp_id, row_number() over(partition by phy_id order by hosp_id) rn
					  FROM (select P.Id as phy_id, PH.Hospital_CmsProviderId as hosp_id from Physicians P
							INNER JOIN Physicians_AffiliatedHospitals PH
							ON PH.Physician_Id = P.Id ) tt
					) ff
					group by phy_id
				) AS PPP
				ON PHY.Id = PPP.phy_id

				WHERE 
				H.State IN (select * from dbo.fnList2Table(@csvStates, ',')) 
				and PHY.IsDeleted = 0 
				--and H.IsDeleted = 0
				
				UNION
				
				SELECT   -- get physicians solo
					npi,
					PacId as ind_pac_id, 
					ProfEnrollId as ind_enrl_id,
					FirstName as frst_nm, 
					MiddleName as mid_nm, 
					LastName as lst_nm, 
					Suffix as suff, 
					Gender as gndr, 
					[Credentials] as cred,
					MedicalSchoolName as med_sch,
					GraduationYear as grd_yr,
					ISNULL(PrimarySpecialty, 'UNKNOWN') as pri_spec,
					ISNULL(SecondarySpecialty1, -1) as sec_spec_1,
					ISNULL(SecondarySpecialty2, -1) as sec_spec_2,
					ISNULL(SecondarySpecialty3, -1) as sec_spec_3,
					ISNULL(SecondarySpecialty4, -1) as sec_spec_4,
					'' as sec_spec_all,
					AcceptsMedicareAssignment as assgn,
					(CASE WHEN ParticipatesInERX = 0 THEN 'N' ELSE 'Y' END) as erx,
					(CASE WHEN ParticipatesInPQRS = 0 THEN 'N' ELSE 'Y' END) as pqrs,
					(CASE WHEN ParticipatesInEHR = 0 THEN 'N' ELSE 'Y' END) as ehr,
					'' as org_lgl_nm,
					'' as org_pac_id,
					'' as org_dba_nm,
					'' as num_org_mem,
					A.Line1 as adr_ln_1,
					A.Line2 as adr_lin_2,
					A.City as cty,
					A.[State] as st,
					A.ZipCode as zip,
					'' as ln_2_sprs,
					'' as hosp_afl_1,
					'' as hosp_afl_2,
					'' as hosp_afl_3,
					'' as hosp_afl_4,
					'' as hosp_afl_5,
					'' as hosp_afl_lbn_1,
					'' as hosp_afl_lbn_2,
					'' as hosp_afl_lbn_3,
					'' as hosp_afl_lbn_4,
					'' as hosp_afl_lbn_5,
					'' as MOC,
					'' as MHI
				FROM dbo.Physicians PHY
				inner JOIN dbo.Addresses A ON PHY.Id = A.Physician_Id AND UPPER(A.[AddressType]) = 'PHYSICIAN'
				WHERE  A.State IN (select * from dbo.fnList2Table(@csvStates, ',')) 
				and PHY.IsDeleted = 0 
				ORDER BY NPI, ZIP DESC;
				*/
		END -- selection = -1
	ELSE IF @selection = 0 -- index file of physicians by state
		BEGIN
				SELECT distinct
					npi,
					FirstName as frst_nm, 
					LastName as lst_nm
				FROM dbo.Physicians PHY
				INNER JOIN dbo.[Physicians_MedicalPractices] PMP ON PHY.Id = PMP.Physician_Id
				INNER JOIN dbo.MedicalPractices MP ON PMP.MedicalPractice_Id = MP.Id
				INNER JOIN dbo.Addresses A ON A.MedicalPractice_Id = MP.Id
				WHERE 
				A.State IN (select * from dbo.fnList2Table(@csvStates, ',')) and PHY.IsDeleted = 0
				union			
				SELECT distinct
					npi,
					FirstName as frst_nm, 
					LastName as lst_nm
				FROM dbo.Physicians PHY
				INNER JOIN dbo.[Physicians_AffiliatedHospitals] PAH ON PHY.Id = PAH.Physician_Id
				INNER JOIN dbo.Hospitals H ON H.CmsProviderID = PAH.Hospital_CmsProviderId
				WHERE 
				H.State IN (select * from dbo.fnList2Table(@csvStates, ',')) and PHY.IsDeleted = 0
				union
				select distinct 
					npi,
					FirstName as frst_nm, 
					LastName as lst_nm
				FROM dbo.Physicians PHY
				INNER JOIN dbo.Addresses A ON PHY.Id = A.Physician_Id
				WHERE 
				A.State IN (select * from dbo.fnList2Table(@csvStates, ',')) and PHY.IsDeleted = 0
		END -- selection = 0
	ELSE IF @selection = 1 -- (profile file of physicians by State)
		BEGIN
			SELECT distinct 
				npi,
				FirstName as frst_nm, 
				MiddleName as mid_nm, 
				LastName as lst_nm, 
				Suffix as suff, 
				ISNULL(PrimarySpecialty, 'UNKNOWN') as pri_spec,
				MAX(MP.Name) as org_lgl_nm,
				MAX(A.City) as cty,
				A.[State] as st,
				MAX(A.ZipCode) as zip
			FROM dbo.Physicians PHY
			INNER JOIN dbo.[Physicians_MedicalPractices] PMP
			ON PHY.Id = PMP.Physician_Id
			INNER JOIN dbo.MedicalPractices MP
			ON PMP.MedicalPractice_Id = MP.Id
			INNER JOIN dbo.Addresses A
			ON A.MedicalPractice_Id = MP.Id
			WHERE A.[State] IN (SELECT * FROM dbo.fnList2Table(@csvStates, ',')) 
			GROUP BY 
				npi,
				FirstName, 
				MiddleName, 
				LastName, 
				Suffix, 
				PrimarySpecialty,
				A.[State]
			ORDER BY FirstName, LastName, A.State
		END -- @slection = 1
	ELSE IF @selection = 2 -- (group by Specialty Id, de dupe by org name,city,state,zip)
		BEGIN
			SELECT DISTINCT 
			npi,				
			PS.FirstName as frst_nm, 
			PS.MiddleName as mid_nm, 
			PS.LastName as lst_nm, 
			PS.Suffix as suff,
			IsNull(PS.PrimarySpecialty, 'UNKNOWN') as pri_spec,
			IsNull(MAX(B.Id), -1) as pri_spec_id,
			IsNull(MAX(T1.org_lgl_nm), '') as org_lgl_nm,
			MAX(T1.cty) as cty,
			T1.st,
			MAX(T1.zip) as zip, 
			IsNull(org_pac_id,'') as org_pac_id
		FROM 
			(SELECT DISTINCT 
				IsNull(MP.Name, '') AS org_lgl_nm, 
				IsNull(MP.GroupPracticePacId, -1) AS org_pac_id, 
				IsNull(MP.DBAName,'') AS org_dba_nm, 
				A.City AS cty, 
				A.State AS st, 
				A.ZipCode AS zip, 
				IsNull(PMP.Physician_Id, -1) as Physician_Id, 
				IsNull(MP.Id, -1) as Id
			FROM dbo.MedicalPractices AS MP 
				JOIN dbo.Physicians_MedicalPractices AS PMP ON MP.Id = PMP.MedicalPractice_Id 
				JOIN dbo.Addresses AS A ON A.MedicalPractice_Id = MP.Id 
				WHERE A.State IN (SELECT * FROM dbo.fnList2Table(@csvStates, ','))) as T1
				JOIN Physicians PS ON T1.Physician_Id = PS.Id
				left JOIN Base_ProviderSpecialities B ON PS.PrimarySpecialty = B.Name
		WHERE PS.IsDeleted = 0							
		GROUP BY 
			PS.npi,				
			PS.FirstName,
			PS.MiddleName, 
			PS.LastName, 
			PS.Suffix,
			PS.PrimarySpecialty,
			T1.st, 
			org_pac_id				
			
			UNION ALL
			
		SELECT DISTINCT PS.npi,				
			PS.FirstName as frst_nm, 
			PS.MiddleName as mid_nm, 
			PS.LastName as lst_nm, 
			PS.Suffix as suff,
			IsNull(PS.PrimarySpecialty, 'UNKNOWN') as pri_spec,
			IsNull(MAX(B.Id), -1) as pri_spec_id,
			IsNull(MAX(T2.org_lgl_nm), '') as org_lgl_nm,
			MAX(T2.cty) as cty,
			T2.st,
			MAX(T2.zip) as zip, 
			IsNull(org_pac_id,'') as org_pac_id 
				FROM 
					(SELECT DISTINCT IsNull(A.Line1, '') AS org_lgl_nm, '' AS org_pac_id,'' AS org_dba_nm, 
								A.City AS cty, A.State AS st, A.ZipCode AS zip, IsNull(PS.Id, -1) as Physician_Id,'' as Id
						FROM  dbo.Addresses AS A 
						JOIN dbo.Physicians PS ON A.Physician_Id = PS.Id 
						WHERE A.State IN (
							SELECT * FROM dbo.fnList2Table(@csvStates, ','))) as T2				
				JOIN Physicians PS ON T2.Physician_Id = PS.Id
				left JOIN Base_ProviderSpecialities B ON PS.PrimarySpecialty = B.Name				
				
		WHERE PS.IsDeleted = 0									
		GROUP BY PS.Npi,				
			PS.FirstName,
			PS.MiddleName, 
			PS.LastName, 
			PS.Suffix,
			PS.PrimarySpecialty,
			T2.st, 
			org_pac_id				
		ORDER BY pri_spec_id, 
			pri_spec, 
			frst_nm, 
			lst_nm,mid_nm
		END -- @selection = 2
	ELSE IF @selection = 3 -- (By zipcode, to remove dupe names we take the topmost of the dupe org name in the same zipcode)
		BEGIN
			SELECT DISTINCT npi,
				PS.FirstName as frst_nm, 
				PS.MiddleName as mid_nm, 
				PS.LastName as lst_nm, 
				PS.Suffix as suff, 
				ISNULL(PS.PrimarySpecialty, 'UNKNOWN') as pri_spec,
				MP.Name as org_lgl_nm,
				P.City as cty,
				P.[State] as st,
				P.zip as zip,
				SUBSTRING(P.Zip,1,5) as zip5,
				SUBSTRING(P.Zip,6,4) as zip4, 
				MP.GroupPracticePacId as org_pac_id
			FROM (
					SELECT DISTINCT PMP.Physician_Id, A.City, A.State, A.ZipCode as zip FROM Addresses A
						JOIN Physicians_MedicalPractices PMP ON A.MedicalPractice_Id = PMP.MedicalPractice_Id AND A.Id in (SElect * FROM dbo.fnList2Table(PMP.AssociatedPMPAddresses, ','))
						JOIN dbo.Base_ZipCodeToHRRAndHSAs Z ON SUBSTRING(A.ZipCode,1,5) = Z.Zip
					WHERE A.[State] IN (SELECT * FROM dbo.fnList2Table(@csvStates, ','))
					UNION
					SELECT DISTINCT P.Id, A.City, A.State, A.ZipCode as zip FROM Addresses A
						JOIN Physicians P ON A.Physician_Id = P.Id
						JOIN dbo.Base_ZipCodeToHRRAndHSAs Z ON SUBSTRING(A.ZipCode,1,5) = Z.Zip
					WHERE A.[State] IN (SELECT * FROM dbo.fnList2Table(@csvStates, ','))
				) AS P
					INNER JOIN Physicians PS ON P.Physician_Id = PS.Id
					LEFT JOIN dbo.[Physicians_MedicalPractices] PMP ON PS.Id = PMP.Physician_Id
					LEFT JOIN dbo.MedicalPractices MP ON PMP.MedicalPractice_Id = MP.Id
			WHERE PS.IsDeleted = 0 and SUBSTRING(P.Zip,1, 5) = SUBSTRING(@zipCode,1,5)
			GROUP BY Npi,
				PS.FirstName, 
				PS.MiddleName, 
				PS.LastName, 
				PS.Suffix, 
				PS.PrimarySpecialty,
				P.City,
				P.[State],
				P.zip,
				MP.Name, 	
				MP.GroupPracticePacId
			ORDER BY SUBSTRING(P.Zip,1,5), PS.FirstName, PS.LastName
				
		END -- @selection = 3
	ELSE IF @selection = 4 -- (By practice, to remove dupe names we take the topmost of the dupe org name,cty,st,zip)
		BEGIN

			select distinct	npi
						,	PS.FirstName as frst_nm
						,	PS.MiddleName as mid_nm
						,	PS.LastName as lst_nm
						,	PS.Suffix as suff
						,	ISNULL(PS.PrimarySpecialty, 'UNKNOWN') as pri_spec
						,	P.Name as org_lgl_nm
						,	P.GroupPracticePacId as org_pac_id
						,	P.City as cty
						,	P.[State] as st
						,	P.ZipCode as zip
			from			Physicians PS
				inner join	(	
								select distinct	pmp.Physician_Id
											,	mp.Name
											,	mp.GroupPracticePacId
											,	mp.DBAName
											,	A.City
											,	A.State
											,	A.ZipCode
											,	pmp.AssociatedPMPAddresses
								from			dbo.Physicians_MedicalPractices pmp 
									inner join	dbo.MedicalPractices mp on pmp.MedicalPractice_Id = mp.Id
									inner join	dbo.Addresses A on a.MedicalPractice_Id = pmp.MedicalPractice_Id
								where			A.[State] in (select * from dbo.fnList2Table(@csvStates, ','))
									and			A.Id in (select * from dbo.fnList2Table(pmp.AssociatedPMPAddresses,','))
							) P on PS.Id = P.Physician_Id
			where			PS.IsDeleted = 0
			order by		P.GroupPracticePacId
						,	P.Name
						,	PS.FirstName
						,	PS.LastName
				
		END -- @selection = 4
	END TRY

	BEGIN CATCH
		
		SELECT 
		 ERROR_NUMBER() AS ErrorNumber
		,ERROR_MESSAGE() AS ErrorMessage;
       
	END CATCH
	

END


GO


