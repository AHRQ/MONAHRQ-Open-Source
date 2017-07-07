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
using System.Linq;


namespace Monahrq.Infrastructure.BaseDataLoader.Loaders
{
    /// <summary>
    /// The <see cref="Hospital"/> base data import strategy.
    /// </summary>
    /// <seealso cref="Monahrq.Infrastructure.BaseDataLoader.BaseDataDataReaderImporter{Monahrq.Infrastructure.Entities.Domain.Hospitals.Hospital, System.Int32}" />
    [Export(DataImportContracts.BaseDataLoader, typeof (IBasedataImporter))]
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
        {}

        /// <summary>
        /// Pres the load data.
        /// </summary>
        public override void PreLoadData()
        {
            base.PreLoadData();
            using (var session = DataProvider.SessionFactory.OpenStatelessSession())
            {
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
            }


        }

        /// <summary>
        /// Posts the load data.
        /// </summary>
        public override void PostLoadData()
        {
            base.PostLoadData();

            using (var session = DataProvider.SessionFactory.OpenSession())
            {
                session.SaveOrUpdate(Registry);
            }

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

            var hosp = new Hospital(Registry)
            {
                IsSourcedFromBaseData = true,
                CmsProviderID = dr.Guard<string>("PRVDR_NUM_MOD").Replace("'", string.Empty),
                Name = dr.Guard<string>("FAC_NAME").ToProper(),
                Address = dr.Guard<string>("ST_ADR").ToProper(),
                City = dr.Guard<string>("CITY_NAME").ToProper(),
                State = state != null ? state.Abbreviation : null,
                Zip = zip,
                County = county != null ? county.CountyFIPS : null,
				Latitude = dr.Guard<double>("LATITUDE"),
				Longitude = dr.Guard<double>("LONGITUDE"),


                PhoneNumber = dr.Guard<string>("PHNE_NUM"),
                FaxNumber = dr.Guard<string>("FAX_PHNE_NUM"),

                Employees = dr.Guard<int>("EMPLEE_CNT"),
                TotalBeds = dr.Guard<int>("BED_CNT"),
                MedicareMedicaidProvider = medicareMedicaidProvider,
                HospitalOwnership = dr.Guard<string>("MLT_OWND_FAC_ORG_SW").ToProper(),

                CardiacCatherizationService = cardiacCatherizationService,
                DiagnosticXRayService = diagnosticXRayService,
                EmergencyService = emergencyService,
                PediatricService = pediatricService,
                PediatricICUService = pediatricIcuService,
                PharmacyService = pharmacyService,
                TraumaService = traumaService,
                UrgentCareService = urgentCareService
            };

            if (hosp.HealthReferralRegion == null || hosp.HospitalServiceArea == null)
            {
                var zipCodeToHrrAndHsa = ZipCodeToHRRAndHSAs.FirstOrDefault(x => x.Zip == hosp.Zip);

                if (zipCodeToHrrAndHsa != null && hosp.HealthReferralRegion == null)
                {
                    HealthReferralRegion hrr = HealthReferralRegions.FirstOrDefault(x => x.ImportRegionId == zipCodeToHrrAndHsa.HRRNumber);
                    if (hrr != null)
                    {
                        hosp.HealthReferralRegion = hrr;
                    }
                }

                if (zipCodeToHrrAndHsa != null && hosp.HospitalServiceArea == null)
                {
                    HospitalServiceArea hsa = HospitalServiceAreas.FirstOrDefault(x => x.ImportRegionId == zipCodeToHrrAndHsa.HSANumber);
                    if (hsa != null)
                    {
                        hosp.HospitalServiceArea = hsa;
                    }
                }
            }

            // Get the hospital category ID from the file.
            // int hospCatID = dr.Guard<int>("PRVDR_CTGRY_CD");
            var hospCatID = dr.Guard<int>("PRVDR_CTGRY_SBTYP_CD");

            //// Try to find the hospital category and add it to the hospital.
            HospitalCategory hospCat = HospitalCategories.FirstOrDefault(x => x.CategoryID == hospCatID);
            if (hospCat != null)
            {
                hosp.Categories.Add(hospCat);
            }

            // NOTE: Must save and flush data here or categories are not being saved!
            using (var session = DataProvider.SessionFactory.OpenSession())
            {
                try
                {
                    session.SaveOrUpdate(hosp);
                    session.Flush();
                }
                catch(Exception exc)
                {
                    base.Logger.Log(exc.GetBaseException().Message, Microsoft.Practices.Prism.Logging.Category.Exception, Microsoft.Practices.Prism.Logging.Priority.High);
                }
            }
            
            // NOTE: Returning null here so that it's not added to the bulk insert at the end. We've already saved the hospital above.
            return null;
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
