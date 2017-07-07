-- =====================================================================
-- physicians and medical practices
-- =====================================================================

-- PMP was created in a previous batch.  We must now append more Assocated MP Addresses to it.
-- Do  update first so we new records aren't processed twice in one batch.

update			pmp
set				pmp.AssociatedPMPAddresses = substring((
					select			',' + convert(varchar(20), mpxa.Id)
					from			-- Get Unique addresses from taret table.
									(
										select distinct	tptx.Line1
													,	tptx.Line2
													,	tptx.City
													,	tptx.State
													,	left((	case len(isnull(tptx.ZipCode,''))
																	when 4 then '0' + isnull(tptx.ZipCode,'')				
																	when 8 then '0' + isnull(tptx.ZipCode,'')
																	else isnull(tptx.ZipCode,'')
																end),5) as ZipCode
													,	mpx.Id as MPID
										from			Targets_PhysicianTargets tptx (nolock)
											inner join	MedicalPractices mpx (nolock)
															on	mpx.GroupPracticePacId = tptx.GroupPracticePacId
															and	mpx.Id = mp.Id	-- I think this is implicitly true.
															and	tptx.Npi = p.Npi
									) utptx
						inner join	Addresses mpxa (nolock) on mpxa.MedicalPractice_Id = utptx.MPID
					where			mpxa.AddressType = 'MedicalPractice'
						and			isnull(mpxa.Line1,'')			= isnull(utptx.Line1,'')
						and			isnull(mpxa.Line2,'')			= isnull(utptx.Line2,'')
						and			isnull(mpxa.City,'')			= isnull(utptx.City,'')
						and			isnull(mpxa.State,'')			= isnull(utptx.State,'')
						and			left(isnull(mpxa.ZipCode,''),5)	= isnull(utptx.ZipCode,'')
					for XML Path('')),2,8000) --as 'AssociatedPMPAddresses' 
from			Targets_PhysicianTargets tpt (nolock)
				-- Get unique Physician/MedicalPractice pairs in target table.
	inner join	(
					select			tptx.Npi
								,	tptx.GroupPracticePacId
								,	min(tptx.Id) as UIDx
					from			Targets_PhysicianTargets tptx (nolock)
					group by		tptx.Npi
								,	tptx.GroupPracticePacId
				) uniqueTpt on uniqueTpt.UIDx = tpt.Id
	inner join	Physicians p (nolock) on p.Npi = tpt.Npi /*AND P.PacId = PTP.PacId AND p.ProfEnrollId = PTP.ProfEnrollId*/
	inner join	MedicalPractices mp (nolock) on mp.GroupPracticePacId = tpt.GroupPracticePacId
	inner join	Physicians_MedicalPractices  pmp (nolock)
					on	pmp.Physician_Id = p.Id
					and	pmp.MedicalPractice_Id = mp.Id
where	((		tpt.GroupPracticePacId is not null
			or	tpt.OrgLegalName is not null)
	and			p.States='[@@State@@]'
	and			mp.State='[@@State@@]')


-- Add new records.
insert into		Physicians_MedicalPractices (Physician_Id,MedicalPractice_Id,AssociatedPMPAddresses)
select			p.Id
			,	mp.Id
			,	substring((
					select			',' + convert(varchar(20), mpxa.Id)
					from			-- Get Unique addresses from taret table.
									(
										select distinct	tptx.Line1
													,	tptx.Line2
													,	tptx.City
													,	tptx.State
													,	left((	case len(isnull(tptx.ZipCode,''))
																	when 4 then '0' + isnull(tptx.ZipCode,'')				
																	when 8 then '0' + isnull(tptx.ZipCode,'')
																	else isnull(tptx.ZipCode,'')
																end),5) as ZipCode
													,	mpx.Id as MPID
										from			Targets_PhysicianTargets tptx (nolock)
											inner join	MedicalPractices mpx (nolock)
															on	mpx.GroupPracticePacId = tptx.GroupPracticePacId
															and	mpx.Id = mp.Id	-- I think this is implicitly true.
															and	tptx.Npi = p.Npi
									) utptx
						inner join	Addresses mpxa (nolock) on mpxa.MedicalPractice_Id = utptx.MPID
					where			mpxa.AddressType = 'MedicalPractice'
						and			isnull(mpxa.Line1,'')			= isnull(utptx.Line1,'')
						and			isnull(mpxa.Line2,'')			= isnull(utptx.Line2,'')
						and			isnull(mpxa.City,'')			= isnull(utptx.City,'')
						and			isnull(mpxa.State,'')			= isnull(utptx.State,'')
						and			left(isnull(mpxa.ZipCode,''),5)	= isnull(utptx.ZipCode,'')
					for XML Path('')),2,8000) --as 'AssociatedPMPAddresses' 
from			Targets_PhysicianTargets tpt (nolock)
				-- Get unique Physician/MedicalPractice pairs in target table.
	inner join	(
					select			tptx.Npi
								,	tptx.GroupPracticePacId
								,	min(tptx.Id) as UIDx
					from			Targets_PhysicianTargets tptx (nolock)
					group by		tptx.Npi
								,	tptx.GroupPracticePacId
				) uniqueTpt on uniqueTpt.UIDx = tpt.Id
	inner join	Physicians p (nolock) on p.Npi = tpt.Npi /*AND P.PacId = PTP.PacId AND p.ProfEnrollId = PTP.ProfEnrollId*/
	inner join	MedicalPractices mp (nolock) on mp.GroupPracticePacId = tpt.GroupPracticePacId
where	((		tpt.GroupPracticePacId is not null
			or	tpt.OrgLegalName is not null)
	and			p.States='[@@State@@]'
	and			mp.State='[@@State@@]')
	and			not exists( select			*
							from			Physicians_MedicalPractices pmp
							where			pmp.Physician_id = p.Id
								and			pmp.MedicalPractice_Id = mp.Id)

--clean column values
Update Physicians_MedicalPractices 
  set AssociatedPMPAddresses = 
	CASE
    WHEN AssociatedPMPAddresses LIKE ',%,' THEN SUBSTRING(AssociatedPMPAddresses, 2, LEN(AssociatedPMPAddresses)-2)
    WHEN AssociatedPMPAddresses LIKE ',%'  THEN RIGHT(AssociatedPMPAddresses, LEN(AssociatedPMPAddresses)-1)
    WHEN AssociatedPMPAddresses LIKE '%,'  THEN LEFT(AssociatedPMPAddresses, LEN(AssociatedPMPAddresses)-1)
    ELSE AssociatedPMPAddresses END
Where AssociatedPMPAddresses is not null

