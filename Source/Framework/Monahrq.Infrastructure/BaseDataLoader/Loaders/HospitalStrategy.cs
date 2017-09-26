using System;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Core.Import;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Extensions;
using NHibernate.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Windows;
using NHibernate;


namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="Hospital"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.Hospitals.Hospital, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof(IBasedataImporter))]
    public class HospitalStrategy : BaseDataDataReaderImporter<Hospital, int>
    {
        /// <summary>
        /// Gets the type of the import.
        /// </summary>
        /// <value>
        /// The type of the import.
        /// </value>
        protected override BaseDataImportStrategyType ImportType { get { return BaseDataImportStrategyType.Append; } }
        /// <summary>
        /// Gets the fileprefix.
        /// </summary>
        /// <value>
        /// The fileprefix.
        /// </value>
        protected override string Fileprefix { get { return "POSHospitals"; } }
        /// <summary>
        /// Gets the loader priority.
        /// </summary>
        /// <value>
        /// The loader priority.
        /// </value>
        public override int LoaderPriority { get { return 3; } }

        /// <summary>
        /// Gets or sets the registry.
        /// </summary>
        /// <value>
        /// The registry.
        /// </value>
        public static HospitalRegistry Registry { get; set; }
        /// <summary>
        /// Gets or sets the states.
        /// </summary>
        /// <value>
        /// The states.
        /// </value>
        public static List<State> States { get; set; }
        /// <summary>
        /// Gets or sets the counties.
        /// </summary>
        /// <value>
        /// The counties.
        /// </value>
        public static List<County> Counties { get; set; }
        /// <summary>
        /// Gets or sets the hospital categories.
        /// </summary>
        /// <value>
        /// The hospital categories.
        /// </value>
        public static List<HospitalCategory> HospitalCategories { get; set; }
        /// <summary>
        /// Gets or sets the health referral regions.
        /// </summary>
        /// <value>
        /// The health referral regions.
        /// </value>
        public static List<HealthReferralRegion> HealthReferralRegions { get; set; }
        /// <summary>
        /// Gets or sets the hospital service areas.
        /// </summary>
        /// <value>
        /// The hospital service areas.
        /// </value>
        public static List<HospitalServiceArea> HospitalServiceAreas { get; set; }
        /// <summary>
        /// Gets or sets the zip code to HRR and hs as.
        /// </summary>
        /// <value>
        /// The zip code to HRR and hs as.
        /// </value>
        public static List<ZipCodeToHRRAndHSA> ZipCodeToHRRAndHSAs { get; set; }

        /// <summary>
        /// Called when [imports satisfied].
        /// </summary>
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            VersionStrategy = new MonthAndYearBaseDataVersionStrategy(Logger, DataProvider, typeof(Hospital));
        }

        /// <summary>
        /// Pres the process file.
        /// </summary>
        /// <param name="files">The files.</param>
        protected override void PreProcessFile(ref string[] files)
        {
            files = new[] {files.OrderByDescending(f => f).First()};
        }

        private ISession session;
        private int updates;
        private int additions;

        /// <summary>
        /// Pres the load data.
        /// </summary>
        public override void PreLoadData()
        {
            base.PreLoadData();
            this.session = DataProvider.SessionFactory.OpenSession();

            Registry = session.Query<HospitalRegistry>().SingleOrDefault();
            States = session.Query<State>().ToList();
            Counties = session.Query<County>().ToList();
            HospitalCategories = session.Query<HospitalCategory>().ToList();
            HealthReferralRegions = session.Query<HealthReferralRegion>().ToList();
            HospitalServiceAreas = session.Query<HospitalServiceArea>().ToList();
            ZipCodeToHRRAndHSAs = session.Query<ZipCodeToHRRAndHSA>().ToList();
            foreach (var hospCat in HospitalCategories)
            {
                Registry.HospitalCategories.Add(hospCat);
            }
            this.updates = 0;
            this.additions = 0;
        }

        /// <summary>
        /// Posts the load data.
        /// </summary>
        public override void PostLoadData()
        {
            base.PostLoadData();

            this.session.SaveOrUpdate(Registry);
            this.session.Flush();
            this.session.Dispose();

            Registry = null;
            States = null;
            Counties = null;
            HospitalCategories = null;
            HealthReferralRegions = null;
            HospitalServiceAreas = null;
            ZipCodeToHRRAndHSAs = null;
        }

        /// <summary>
        /// Loads from reader.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns></returns>
        public override Hospital LoadFromReader(System.Data.IDataReader dr)
        {
            var cmsProviderId = dr.Guard<string>("PRVDR_NUM_MOD").Replace("'", string.Empty);
            
            // look for match in db
            var hosp = this.session.Query<Hospital>()
                .Where(h => h.CmsProviderID == cmsProviderId)
                .OrderByDescending(h => h.Id)
                .FirstOrDefault();
            if (hosp == null)
            {
                // create new record for insert
                hosp = new Hospital();
                Registry.Hospitals.Add(hosp);
                this.additions++;
            }
            else if (!hosp.IsSourcedFromBaseData)
                // match was entered by hand; don't do anything
                return null;
            else
                this.updates++;
            
            // overwrite everything with new data
            this.PopulateHospitalFromDataRow(hosp, dr);

            // load relationships
            this.MapHrrAndHsa(hosp);

            // Get the hospital category ID from the file.
            var hospCatId = dr.Guard<int>("PRVDR_CTGRY_SBTYP_CD");
            var hospCat = HospitalCategories.FirstOrDefault(x => x.CategoryID == hospCatId);
            if (hospCat != null && !hosp.Categories.Contains(hospCat))
                hosp.Categories.Add(hospCat);

            // update if this is an existing hospital
            this.session.SaveOrUpdate(hosp);
           
            return null; 
        }

        private void MapHrrAndHsa(Hospital hosp)
        {
            if (hosp.HealthReferralRegion != null && hosp.HospitalServiceArea != null)
                return;

            var zipCodeToHrrAndHsa = ZipCodeToHRRAndHSAs.FirstOrDefault(x => x.Zip == hosp.Zip);

            if (zipCodeToHrrAndHsa != null && hosp.HealthReferralRegion == null)
            {
                var hrr = HealthReferralRegions.FirstOrDefault(x => x.ImportRegionId == zipCodeToHrrAndHsa.HRRNumber);
                if (hrr != null)
                    hosp.HealthReferralRegion = hrr;
            }

            if (zipCodeToHrrAndHsa != null && hosp.HospitalServiceArea == null)
            {
                var hsa = HospitalServiceAreas.FirstOrDefault(x => x.ImportRegionId == zipCodeToHrrAndHsa.HSANumber);
                if (hsa != null)
                    hosp.HospitalServiceArea = hsa;
            }
        }

        private void PopulateHospitalFromDataRow(Hospital hosp, IDataReader dr)
        {
            // Get the hospital from the file.
            var state = States.FirstOrDefault(x => x.Abbreviation == dr.Guard<string>("STATE_CD").Trim());
            var county = state == null ? null : LookupCountyByFIPS(dr.Guard<string>("FIPS_CNTY_CD"), state.FIPSState);

            // zip must be left-padded with 0's since input file contains some 3 and 4-digit zip codes in northeast states
            //const string regExpression = @"^\d{5}(?:[-\s]\d{4})?$";
            string zip = null;
            var zipTemp = dr.Guard<string>("ZIP_CD");
            if (!string.IsNullOrEmpty(zipTemp) /*&& Regex.Match(zipTemp, regExpression).Success*/)
            {
                zip = zipTemp.Length < 5 ? zipTemp.PadLeft(5, '0') : zipTemp;
            }

            var medicareMedicaidProviderStr = dr.Guard<string>("MDCD_MDCR_PRTCPTG_PRVDR_SW");
            var medicareMedicaidProvider = !string.IsNullOrEmpty(medicareMedicaidProviderStr) && (medicareMedicaidProviderStr == "Y");

            var cardiacCatherizationServiceStr = dr.Guard<string>("LAB_SRVC_CD");
            var cardiacCatherizationService = !string.IsNullOrEmpty(cardiacCatherizationServiceStr) && (int.Parse(cardiacCatherizationServiceStr) >= 1);

            var diagnosticXRayServiceStr = dr.Guard<string>("DGNSTC_XRAY_ONST_NRSDNT_SW");
            var diagnosticXRayService = !string.IsNullOrEmpty(diagnosticXRayServiceStr) && diagnosticXRayServiceStr.EqualsIgnoreCase("Y");

            var emergencyServiceStr = dr.Guard<string>("DCTD_ER_SRVC_CD");
            var emergencyService = !string.IsNullOrEmpty(emergencyServiceStr) && (int.Parse(emergencyServiceStr) >= 1);

            var pediatricServiceStr = dr.Guard<string>("PED_SRVC_CD");
            var pediatricService = !string.IsNullOrEmpty(pediatricServiceStr) && (int.Parse(pediatricServiceStr) >= 1);

            var pediatricIcuServiceStr = dr.Guard<string>("PED_ICU_SRVC_CD");
            var pediatricIcuService = !string.IsNullOrEmpty(pediatricIcuServiceStr) && (int.Parse(pediatricIcuServiceStr) >= 1);

            var pharmacyServiceStr = dr.Guard<string>("PHRMCY_SRVC_CD");
            var pharmacyService = !string.IsNullOrEmpty(pharmacyServiceStr) && (int.Parse(pharmacyServiceStr) >= 1);

            var traumaServiceStr = dr.Guard<string>("SHCK_TRMA_SRVC_CD");
            var traumaService = !string.IsNullOrEmpty(traumaServiceStr) && (int.Parse(traumaServiceStr) >= 1);

            var urgentCareServiceStr = dr.Guard<string>("URGNT_CARE_SRVC_CD");
            var urgentCareService = !string.IsNullOrEmpty(urgentCareServiceStr) && (int.Parse(urgentCareServiceStr) >= 1);

            hosp.IsSourcedFromBaseData = true;
            hosp.CmsProviderID = dr.Guard<string>("PRVDR_NUM_MOD").Replace("'", string.Empty);
            hosp.Name = dr.Guard<string>("FAC_NAME").ToProper();
            hosp.Address = dr.Guard<string>("ST_ADR").ToProper();
            hosp.City = dr.Guard<string>("CITY_NAME").ToProper();
            hosp.State = state != null ? state.Abbreviation : null;
            hosp.Zip = zip;
            hosp.County = county != null ? county.CountyFIPS : null;
            hosp.Latitude = dr.Guard<double>("LATITUDE");
            hosp.Longitude = dr.Guard<double>("LONGITUDE");
            hosp.PhoneNumber = dr.Guard<string>("PHNE_NUM");
            hosp.FaxNumber = dr.Guard<string>("FAX_PHNE_NUM");
            hosp.Employees = dr.Guard<int>("EMPLEE_CNT");
            hosp.TotalBeds = dr.Guard<int>("BED_CNT");
            hosp.MedicareMedicaidProvider = medicareMedicaidProvider;
            hosp.HospitalOwnership = dr.Guard<string>("MLT_OWND_FAC_ORG_SW").ToProper();
            hosp.CardiacCatherizationService = cardiacCatherizationService;
            hosp.DiagnosticXRayService = diagnosticXRayService;
            hosp.EmergencyService = emergencyService;
            hosp.PediatricService = pediatricService;
            hosp.PediatricICUService = pediatricIcuService;
            hosp.PharmacyService = pharmacyService;
            hosp.TraumaService = traumaService;
            hosp.UrgentCareService = urgentCareService;
        }

        /// <summary>
        /// Lookups the county by fips.
        /// </summary>
        /// <param name="countyFIPS">The county fips.</param>
        /// <param name="stateFIPS">The state fips.</param>
        /// <returns></returns>
        private static County LookupCountyByFIPS(string countyFIPS, string stateFIPS)
        {
            int cFips;
            if (int.TryParse(countyFIPS, out cFips))
            {
                int sFips;
                if (int.TryParse(stateFIPS, out sFips))
                {
                    string countyCode = String.Format("{0:D2}{1:D3}", sFips, cFips);
                    return Counties.FirstOrDefault(c => c.CountyFIPS.Equals(countyCode));
                }
            }
            return null;
        }
    }
}
