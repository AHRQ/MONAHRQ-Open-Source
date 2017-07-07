using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LinqKit;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Data.Transformers;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.FileSystem;
using Monahrq.Infrastructure.Services.Hospitals;

using Monahrq.Sdk.Services.Import.Exceptions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;

namespace Monahrq.Sdk.Services.Import
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Services.Import.EntityFileImporter" />
    [Export(ImporterContract.Hospital, typeof(IEntityFileImporter))]
    public class HospitalImporter : EntityFileImporter
    {
        private IList<CustomRegion> _availableCustomRegions = new List<CustomRegion>();
        private IList<HealthReferralRegion> _availableHealthReferralRegions = new List<HealthReferralRegion>();
        private IList<HospitalServiceArea> _availableHospitalServiceAreas = new List<HospitalServiceArea>();
        private IList<County> _availableCounties = new List<County>();
        private IList<System.Tuple<Hospital, bool, int?>> _finalHospitals = new List<System.Tuple<Hospital, bool, int?>>();
        private IList<Hospital> _finalArchivedHospitals = new List<Hospital>();
        private IList<Tuple<string, double?, double?>> _baseLatLongLookup = new List<Tuple<string, double?, double?>>();

        private HospitalRegistry _hospitalRegistry;
        private IStatelessSession _session;
        private ITransaction _transaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalImporter" /> class.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="hospitalRegistryService">The hospital registry service.</param>
        /// <param name="events">The events.</param>
        /// <param name="logger">The logger.</param>
        [ImportingConstructor]
        public HospitalImporter(IUserFolder folder
                                , IDomainSessionFactoryProvider provider
                                , IHospitalRegistryService hospitalRegistryService
                                , IEventAggregator events
                                , ILoggerFacade logger)
            : base(folder, provider, hospitalRegistryService, events, logger)
        {

        }

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <value>
        /// The file extension.
        /// </value>
        protected override string FileExtension
        {
            get { return ".csv"; }
        }

        /// <summary>
        /// Gets the continue prompt.
        /// </summary>
        /// <value>
        /// The continue prompt.
        /// </value>
        protected override string ContinuePrompt
        {
            get
            {
                return string.Format(@"Importing this file may overwrite your existing hospitals' information.{0}(Special Note: If you are including Region Ids, in your file,{0}please make sure to perform a custom region import prior importing hospitals.) {0}{0}Do you want to continue?", System.Environment.NewLine);
            }
        }

        /// <summary>
        /// Gets the expected count of values per line.
        /// </summary>
        /// <value>
        /// The expected count of values per line.
        /// </value>
        protected override int ExpectedCountOfValuesPerLine
        {
            get { return 24; }
        }

        /// <summary>
        /// Gets the expected high count of values per line.
        /// </summary>
        /// <value>
        /// The expected high count of values per line.
        /// </value>
        protected override int ExpectedHighCountOfValuesPerLine
        {
            get { return 26; }
        }

        /// <summary>
        /// Factories the hospital.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="hosp">The hosp.</param>
        /// <param name="source">The source.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private Hospital FactoryHospital(IStatelessSession session, Hospital hosp, HospitalDto source, IList<ImportError> errors)
        {
            //var convertToCustom = hosp != null;
            var result = hosp ?? HospitalRegistryService.CreateHospital();

            result.LocalHospitalId = source.Dshospid;
            result.Zip = source.Zip;
            result.Name = source.HospitalName;
            result.CCR = source.CCR;
            result.CmsProviderID = source.CmsProvdrNum;
            result.IsSourcedFromBaseData = false;
            result.Address = source.Address;
            result.City = source.City;
            result.Description = source.Description;
            result.Employees = source.Employees;
            result.TotalBeds = source.TotalBeds;
            result.HospitalOwnership = source.HospitalOwnership;
            result.PhoneNumber = source.PhoneNumber;
            result.FaxNumber = source.FaxNumber;
            result.MedicareMedicaidProvider = source.MedicareMedicaidProvider;
            result.EmergencyService = source.EmergencyService;
            result.TraumaService = source.TraumaService;
            result.UrgentCareService = source.UrgentCareService;
            result.PediatricService = source.PediatricService;
            result.PediatricICUService = source.PediatricICUService;
            result.CardiacCatherizationService = source.CardiacCatherizationService;
            result.PharmacyService = source.PharmacyService;
            result.DiagnosticXRayService = source.DiagnosticXRayService;

            //var errorMessage = string.Format("Hospital \"{0}\" (CMS Provider ID: {1}) does not have valid latitude and/or longitude values.");
            if (source.Latitude.HasValue || hosp == null)
                result.Latitude = source.Latitude;
            else
            {
                var latitude = _baseLatLongLookup.FirstOrDefault(bll => bll.Item1.ToUpper() == source.CmsProvdrNum.ToUpper());
                if (latitude != null)
                {
                    result.Latitude = latitude.Item2;
                }
                //else
                //{
                //    errors.Add(new ImportError { EntityName = "Hospital", ImportType = result.Name, ErrorMessage = errorMessage });
                //}
            }
            if (source.Longtitude.HasValue || hosp == null)
                result.Longitude = source.Longtitude;
            else
            {
                var longitude = _baseLatLongLookup.FirstOrDefault(bll => bll.Item1.ToUpper() == source.CmsProvdrNum.ToUpper());
                if (longitude != null)
                {
                    result.Longitude = longitude.Item3;
                }
                //else
                //{
                //    errors.Add(new ImportError { EntityName = "Hospital", ImportType = result.Name, ErrorMessage = errorMessage });
                //}
            }

            if (hosp != null && hosp.Categories.Any())
                result.Categories = hosp.Categories;
            else
            {
                var defaultCategory = GetDefaultCategory(session);

                if (defaultCategory != null)
                    result.Categories.Add(defaultCategory);
            }

            result.CustomRegion = !source.RegionId.HasValue
                                      ? null
                                      : _availableCustomRegions.FirstOrDefault(r => r.ImportRegionId == source.RegionId.GetValueOrDefault() && r.State.EqualsIgnoreCase(source.State));

            // If not a CMS Provider Id override, we will get the assign custom hospital to HRR and HSA regions based off zipcode.
            if (result.CustomRegion == null && (result.HealthReferralRegion == null || result.HospitalServiceArea == null))
            {
                if (result.HealthReferralRegion == null || result.HospitalServiceArea == null)
                {
                    if (!string.IsNullOrEmpty(result.Zip ?? string.Empty) && string.IsNullOrEmpty(result.CmsProviderID ?? string.Empty))
                    {
                        var regionLookup = GetRegionsByZipCode(session, result.Zip);

                        if (result.HealthReferralRegion == null)
                            result.HealthReferralRegion = regionLookup["HRR"] as HealthReferralRegion;

                        if (result.HospitalServiceArea == null)
                            result.HospitalServiceArea = regionLookup["HSA"] as HospitalServiceArea;
                    }
                }
            }

            if (result.State == null && source.State != null)
            {
                result.State = source.State;
            }

            if (source.FIPS.HasValue && source.FIPS.Value > 0 && result.State != null)
            {
                result.County = GetCountyByFIPSAndState(source.FIPS.ToString(), result.State);
            }

            return result;
        }

        /// <summary>
        /// The default category
        /// </summary>
        private HospitalCategory _defaultCategory;

        /// <summary>
        /// Gets the default category.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        private HospitalCategory GetDefaultCategory(IStatelessSession session)
        {
            if (_defaultCategory != null) return _defaultCategory;
            _defaultCategory = session.Query<HospitalCategory>().FirstOrDefault(hc => hc.Name.ToLower() == "general");
            return _defaultCategory;
        }

        /// <summary>
        /// Gets the existing hospital.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="cmsNumber">The CMS number.</param>
        /// <param name="localHospitalId">The local hospital identifier.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Sdk.Services.Import.Exceptions.EntityFileImportException">
        /// </exception>
        /// <exception cref="EntityFileImportException"></exception>
        protected Hospital GetExistingHospital(IStatelessSession session, string cmsNumber, string localHospitalId)
        {
            try
            {
                if (cmsNumber != null && cmsNumber.Length < 6)
                    cmsNumber = cmsNumber.PadLeft(6, '0');

                if (string.IsNullOrEmpty(cmsNumber))
                    return null;

                IList<Hospital> foundHospitals = new List<Hospital>();

                using (var sess = Provider.SessionFactory.OpenSession())
                    foundHospitals = sess.CreateCriteria<Hospital>()
                                        .Add(Restrictions.Eq("CmsProviderID", cmsNumber.ToUpper()))
                                            .Add(Restrictions.Eq("IsArchived", false))
                                            .Add(Restrictions.Eq("IsDeleted", false))
                                            .List<Hospital>();

                Hospital foundHospital = null;

                if (foundHospitals.Any())
                {
                    foundHospital = foundHospitals.Count == 1
                                                  ? foundHospitals[0]
                                                  : foundHospitals.OrderBy(h => h.Id).First();
                }

                return foundHospital;
            }
            catch (EntityFileImportException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw new EntityFileImportException(
                    string.Format("More than one hospital matched the incoming data. CMS NUMBER: {0}", cmsNumber));
            }
            catch (Exception ex)
            {
                throw new EntityFileImportException(
                    string.Format("Check on existing hospital, Id {0}, failed: {1}", cmsNumber, ex.Message), ex);
            }
        }

        /// <summary>
        /// Validates the county.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        /// <param name="errors">The errors.</param>
        /// <exception cref="System.Exception"></exception>
        private void ValidateCounty(Hospital hospital, IList<ImportError> errors)
        {
            ZipCodeVal zipCodeFound = null;
            CountyVal fipsCountyFound = null;

            if (!string.IsNullOrEmpty(hospital.Zip) && hospital.State != null)
            {
                zipCodeFound = new ZipCodeVal { Zip = hospital.Zip, StateAbbrev = hospital.State };
            }

            if (!string.IsNullOrEmpty(hospital.County ?? string.Empty))
            {
                var county = _availableCounties.FirstOrDefault(c => c.CountyFIPS.ToUpper() == hospital.County.ToUpper());

                fipsCountyFound = (county != null)
                                        ? new CountyVal { Fips = county.CountyFIPS, Name = county.Name, StateAbbrev = county.State }
                                        : null;
            }

            if (zipCodeFound != null && fipsCountyFound != null)
            {
                if (!CurrentStatesBeingManaged.Any(st => st.EqualsIgnoreCase(zipCodeFound.StateAbbrev)) &&
                    !CurrentStatesBeingManaged.Any(st => st.EqualsIgnoreCase(fipsCountyFound.StateAbbrev)))
                {
                    var errorMessage = string.Format(
                        "both zipcode \"{0}\" ({1}) and county FIPS \"{2}\" ({3}) are in different state(s) than your selected context. Please make sure the the zip code and County FIPS are in the correct regional context.",
                        zipCodeFound.Zip, zipCodeFound.StateAbbrev, fipsCountyFound.Name, fipsCountyFound.StateAbbrev);

                    errors.Add(ImportError.Create("Hospitals", fipsCountyFound.Name, errorMessage));
                    return;
                }
            }

            if (zipCodeFound != null && fipsCountyFound == null)
            {
                if (!CurrentStatesBeingManaged.Any(st => st.EqualsIgnoreCase(zipCodeFound.StateAbbrev)))
                {
                    var errorMessage = string.Format(
                        "zipcode \"{0}\" ({1}) is not in same state as your selected state context. Please make sure that the zip code is in the correct regional context.",
                        zipCodeFound.Zip, zipCodeFound.StateAbbrev);

                    errors.Add(ImportError.Create("Hospitals", zipCodeFound.Zip, errorMessage));
                    return;
                }
            }

            if (zipCodeFound == null && fipsCountyFound != null)
            {
                if (!CurrentStatesBeingManaged.Any(st => st.EqualsIgnoreCase(fipsCountyFound.StateAbbrev)))
                {
                    var errorMessage = string.Format(
                            "County FIPS \"{0}\" ({1}) is not in same state as your selected state context. Please make sure that the county FIPS is in the correct regional context.",
                            fipsCountyFound.Name, fipsCountyFound.StateAbbrev);

                    errors.Add(ImportError.Create("Hospitals", fipsCountyFound.Name, errorMessage));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        class ZipCodeVal
        {
            /// <summary>
            /// The zip
            /// </summary>
            public string Zip;
            /// <summary>
            /// The state abbrev
            /// </summary>
            public string StateAbbrev;
        }

        /// <summary>
        /// 
        /// </summary>
        class CountyVal
        {
            /// <summary>
            /// The fips
            /// </summary>
            public string Fips;
            /// <summary>
            /// The name
            /// </summary>
            public string Name;
            /// <summary>
            /// The state abbrev
            /// </summary>
            public string StateAbbrev;
        }

        /// <summary>
        /// Validates the hospital.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        /// <param name="errors">The errors.</param>
        private static void ValidateHospital(Hospital hospital, IList<ImportError> errors)
        {
            var msg = string.Empty;

            if (string.IsNullOrEmpty(hospital.Name))
            {
                msg = string.Format("A valid Name is required.");
            }

            if (string.IsNullOrEmpty(msg) && string.IsNullOrEmpty(hospital.LocalHospitalId))
            {
                msg = string.Format("{0} could not be added because the local hospital id is required.", hospital.Name);
            }
            if (!string.IsNullOrEmpty(msg)) errors.Add(ImportError.Create("Hospital", hospital.State, msg));
        }

        /// <summary>
        /// Validates the state.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        /// <param name="errors">The errors.</param>
        /// <exception cref="Monahrq.Sdk.Services.Import.Exceptions.EntityFileImportException">The hospital you are attempting to save a hospital where the state is not in the correct regional context: {0}
        /// + A hospital with that state \{2}\ but you are currently managing {3}.{0}
        /// + Either remove the correct the state or zip code from the import, or modify your current regional context to include {2}.</exception>
        private void ValidateState(Hospital hospital, IList<ImportError> errors)
        {
            if (hospital != null && hospital.State != null)
            {
                var isManaged = CurrentStatesBeingManaged.Any(state => state.ToUpper() == hospital.State.ToUpper());
                if (!isManaged)
                {
                    var errorMessage =
                        string.Format(
                            @"The hospital you are attempting to save a hospital where the state is not in the correct regional context: {1}. A hospital with in state '{0}' but you are currently managing {1}. Either remove and/or correct the state or zip code from the import, or modify your current regional context to include {0}."
                            , hospital.State, GetStatesCaptionString());

                    errors.Add(ImportError.Create("Hospital", hospital.State, errorMessage));
                }
            }
        }

        /// <summary>
        /// Gets the region expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        private Expression<Func<T, bool>> GetRegionExpression<T>(int? id) where T : Region
        {
            var criteria = PredicateBuilder.False<T>().Or(reg => reg.ImportRegionId == id.GetValueOrDefault());
            var stateCriteria = PredicateBuilder.False<T>();
            CurrentStatesBeingManaged.ForEach(s => stateCriteria = stateCriteria.Or(reg => reg.State.ToLower() == s.ToLower()));
            criteria = criteria.And(stateCriteria);

            return criteria;
        }

        /// <summary>
        /// Validates the value count.
        /// </summary>
        /// <param name="vals">The vals.</param>
        /// <param name="errors">The errors.</param>
        /// <exception cref="EntityFileImportException"></exception>
        /// <exception cref="System.Exception"></exception>
        public override void ValidateValueCount(string[] vals, IList<ImportError> errors)
        {
            if (ExpectedCountOfValuesPerLine == ExpectedHighCountOfValuesPerLine)
                base.ValidateValueCount(vals, errors);
            else
            {
                if (vals.Length != ExpectedCountOfValuesPerLine && vals.Length != ExpectedHighCountOfValuesPerLine)
                {
                    var errorMessage = string.Format("{0} import requires {1} or {2} values per line. {3} values read.",
                        ExportAttribute.ContractName, ExpectedCountOfValuesPerLine, ExpectedHighCountOfValuesPerLine,
                        vals.Length);
                    errors.Add(ImportError.Create("", GetName(vals), errorMessage));
                }
            }

            if (errors.Count > 0) return;

            if (vals[3].IsNullOrEmpty() && vals[6].IsNullOrEmpty()) // Validate Zipcode and CMS Provider are both not null.
            {
                var errorMessage =
                    string.Format("{0} could not be added because the Zipcode and/or CMS Provider ID cannot be both null.",
                        vals[2]);

                errors.Add(ImportError.Create("Hospital", vals[2], errorMessage));
            }

            if (errors.Count > 0) return;

            if (!vals[1].IsNullOrEmpty() && !vals[1].IsNumeric()) // Validate County Fips is valid numeric value.
            {
                var errorMessage =
                    string.Format("{0} could not be added because the FIPS value {1} is not a valid county FIPS code.",
                        vals[2], vals[1]);

                errors.Add(ImportError.Create("Hospital", vals[2], errorMessage));
            }
        }

        /// <summary>
        /// Validates the zip.
        /// </summary>
        /// <param name="dynHospital">The dyn hospital.</param>
        /// <param name="errors">The errors.</param>
        /// <exception cref="Monahrq.Sdk.Services.Import.Exceptions.EntityFileImportException"></exception>
        /// <exception cref="System.Exception"></exception>
        void ValidateZip(HospitalDto dynHospital, IList<ImportError> errors)
        {
            const string usZipRegEx = @"^\d{5}(?:[-\s]\d{4})?$";

            if (!string.IsNullOrEmpty(dynHospital.Zip) && dynHospital.Zip.Length < 5)
            {
                dynHospital.Zip = dynHospital.Zip.PadLeft(5, '0');
            }

            if (!string.IsNullOrEmpty(dynHospital.Zip) && (dynHospital.Zip.Length < 5 || !Regex.Match(dynHospital.Zip, usZipRegEx).Success))
            {

                throw new EntityFileImportException(string.Format("{0} could not be added because the Zipcode is invalid. Please provide a valid zipcode (XXXXX or XXXXX-XXXX).", dynHospital.Zip));
            }
        }

        /// <summary>
        /// Validates the region.
        /// </summary>
        /// <param name="hospital">The hospital.</param>
        /// <param name="errors">The errors.</param>
        /// <exception cref="EntityFileImportException"></exception>
        /// <exception cref="Monahrq.Sdk.Services.Import.Exceptions.EntityFileImportException"></exception>
        private void ValidateRegion(Hospital hospital, IList<ImportError> errors)
        {
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            var id = hospital.CustomRegion != null ? hospital.CustomRegion.Id : (int?)null;

            if (!id.HasValue) return;

            var criteria = GetRegionExpression<CustomRegion>(id);
            if (!_availableCustomRegions.Any(criteria.Compile()))
            {
                var regType = configService.HospitalRegion.SelectedRegionType;
                Region region = null;
                if (regType == typeof(HealthReferralRegion))
                {
                    var hrrcriteria = GetRegionExpression<HealthReferralRegion>(id);
                    region = _availableHealthReferralRegions.FirstOrDefault(hrrcriteria.Compile());
                }
                else if (regType == typeof(HospitalServiceArea))
                {
                    var hsacriteria = GetRegionExpression<HospitalServiceArea>(id);
                    region = _availableHospitalServiceAreas.FirstOrDefault(hsacriteria.Compile());
                }

                if (region != null)
                {
                    var abbr = region.State;
                    if (!CurrentStatesBeingManaged.Any(s => s.EqualsIgnoreCase(abbr)))
                    {
                        string statesString = GetStatesCaptionString();
                        var disp = regType.GetCustomAttribute<DisplayNameAttribute>();
                        var dispName = disp == null ? regType.Name : disp.DisplayName;
                        var errorMessage = string.Format("You attempted to import a hospital with region ID {1}. {0}" +
                                                     " You have no custom regions defined with that ID. While {4} {1} is located in {2}," +
                                                     " you are currently managing hospitals and {4} entities located in {3}.{0}" +
                                                         "Either modify the record's region ID, or load {4} {1} into your regional context."
                                                     , System.Environment.NewLine, id.Value, region.State,
                                                     statesString, dispName);

                        errors.Add(ImportError.Create(typeof(Hospital).Name, dispName, errorMessage));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the states caption string.
        /// </summary>
        /// <returns></returns>
        private string GetStatesCaptionString()
        {
            var popped = string.Empty;
            var stack = new Stack<string>(CurrentStatesBeingManaged.Select(s => s).OrderBy(s => s));
            if (stack.Count > 1)
            {
                popped = stack.Pop();
            }
            return string.Join(", ", stack) + (string.IsNullOrWhiteSpace(popped) ? string.Empty : (" and " + popped));
        }

        /// <summary>
        /// Processes the hospital.
        /// </summary>
        /// <param name="dynHospital">The dyn hospital.</param>
        /// <param name="errors">The errors.</param>
        private void ProcessHospital(HospitalDto dynHospital, IList<ImportError> errors)
        {
            if (errors.Any())
                return;

            var stopWatch = new Stopwatch();

            stopWatch.Start();

            ValidateZip(dynHospital, errors);

            // TODO: Add logic to resolve state from imported custom hospital if both CMS Provider Id and FIPS code are null.
            if ((dynHospital.State ?? string.Empty).IsNullOrEmpty() && !(dynHospital.Zip ?? string.Empty).IsNullOrEmpty())
            {
                var stopWatch2 = new Stopwatch();
                stopWatch2.Start();
                dynHospital.State = GetStateByZipCode(_session, dynHospital.Zip);
                stopWatch2.Stop();
                var msg1 = string.Format("Resolved state for Hospital \"{0}\"  in {1} seconds.{2}",
                                         dynHospital.HospitalName,
                                         TimeSpan.FromMilliseconds(stopWatch2.ElapsedMilliseconds).Seconds,
                                         System.Environment.NewLine);
                Debugger.Log(5, "Hospital Import", msg1);
            }

            var cmsid = !dynHospital.CmsProvdrNum.IsNullOrEmpty() ? dynHospital.CmsProvdrNum : null;
            var localHospId = !dynHospital.Dshospid.IsNullOrEmpty() ? dynHospital.Dshospid : null;

            //HospitalArchive archivedHospital = null;
            Hospital archivedHospital = null;
            var stopWatch3 = new Stopwatch();
            stopWatch3.Start();
            var hosp = GetExistingHospital(_session, cmsid, localHospId);
            stopWatch3.Stop();
            var msg2 = string.Format("Found existing or created new hospital for Hospital import \"{0}\"  in {1} seconds.{2}",
                                     dynHospital.HospitalName, TimeSpan.FromMilliseconds(stopWatch3.ElapsedMilliseconds).Seconds, System.Environment.NewLine);

            Debugger.Log(5, "Hospital Import", msg2);

            if (hosp != null)
            {
                if ((hosp.IsSourcedFromBaseData && !hosp.IsArchived) || !hosp.IsSourcedFromBaseData)
                {
                    archivedHospital = hosp.Archive();
                    hosp.Id = 0;
                }
                else
                {
                    errors.Add(ImportError.Create("Hospital", "Duplicate Hospital", string.Format("{0} hospital you are attempting to import has already been imported", dynHospital.HospitalName)));
                    return;
                }
            }

            if (_finalHospitals.Any(h => h.Item1.CmsProviderID == dynHospital.CmsProvdrNum && h.Item1.LocalHospitalId == dynHospital.Dshospid))
            {
                errors.Add(ImportError.Create("Hospital", "Duplicate Hospital", string.Format("{0} hospital you are attempting to import has already been imported", dynHospital.HospitalName)));
                return;
            }

            Hospital hospital = FactoryHospital(_session, hosp, dynHospital, errors);


            if (!errors.Any())
                ValidateState(hospital, errors);
            if (!errors.Any())
                ValidateCounty(hospital, errors);
            if (!errors.Any())
                ValidateRegion(hospital, errors);
            if (!errors.Any())
                ValidateHospital(hospital, errors);

            if (errors.Any())
                return;

            var stopWatch4 = new Stopwatch();
            stopWatch4.Start();

            //Save(_session, hospital);
            if (archivedHospital != null)
            {
                _finalHospitals.Add(new System.Tuple<Hospital, bool, int?>(hospital, true, archivedHospital.Id));
                _finalArchivedHospitals.Add(archivedHospital);
            }
            else
            {
                _finalHospitals.Add(new System.Tuple<Hospital, bool, int?>(hospital, false, null));
            }


            stopWatch4.Stop();
            var msg3 = string.Format("Saved Hospital import \"{0}\"  in {1} seconds.{2}",
                                     dynHospital.HospitalName,
                                     TimeSpan.FromMilliseconds(stopWatch4.ElapsedMilliseconds).Seconds,
                                     System.Environment.NewLine);

            Debugger.Log(5, "Hospital Import", msg3);

            //if (archivedHospital != null)
            //{
            //    archivedHospital.LinkedHospitalId = hospital.Id;
            //    Save(_session, archivedHospital);
            //}

            //if (!errors.Any())
            //    tx.Commit();
            //else
            //    tx.Rollback();
            //}
            //}

            //newHospital = HospitalRegistryService.Get<Hospital>(newHospital.Id);
            //Inserted.Add(hospital.Name);
            //Events.GetEvent<EntityImportedEvent<Hospital>>().Publish(hospital);

            stopWatch.Stop();
            var message = string.Format("{2}Hospital \"{0}\" imported in {3} ms ({1} s).{2}", hospital.Name,
                                        TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds).Seconds,
                                        System.Environment.NewLine, stopWatch.ElapsedMilliseconds);
            //Logger.Log(message, Category.Info, Priority.Medium);
            Debugger.Log(5, "Hospital Import", message);
        }

        /// <summary>
        /// Finals the processing.
        /// </summary>
        protected override void FinalProcessing()
        {
            foreach (var hospItem in _finalHospitals)
            {

                var hosp = hospItem.Item1;
                var hasAssociatedArchive = hospItem.Item2;
                var archivedHospId = hospItem.Item3;

                if (hosp == null) continue;

                Save(_session, hosp); // Save hospital

                if (hasAssociatedArchive && archivedHospId.HasValue && archivedHospId > 0) // Has an associated archived hospital
                {
                    var archivedHosp = _finalArchivedHospitals.FirstOrDefault(h => h.Id == archivedHospId);

                    if (archivedHosp != null)
                    {
                        archivedHosp.LinkedHospitalId = hosp.Id;
                        Save(_session, archivedHosp); // Save hospital
                    }
                }
            }

            if (_finalHospitals != null && _finalHospitals.Count > 0)
            {
                FlagWebsites();
            }
        }

        /// <summary>
        /// Ends the import.
        /// </summary>
        protected override void EndImport()
        {
            base.EndImport();

            if (_session == null || !_session.IsOpen) return;

            if (_transaction != null && _transaction.IsActive)
            {
                _transaction.Commit();
            }

            _session.Close();
            _session.Dispose();

            _availableCounties = new List<County>();
            _availableCustomRegions = new List<CustomRegion>();
            _availableHealthReferralRegions = new List<HealthReferralRegion>();
            _availableHospitalServiceAreas = new List<HospitalServiceArea>();
            _finalHospitals = new List<System.Tuple<Hospital, bool, int?>>();
            _finalArchivedHospitals = new List<Hospital>();
            _baseLatLongLookup = new List<Tuple<string, double?, double?>>();
        }

        /// <summary>
        /// Flags the websites.
        /// </summary>
        protected async void FlagWebsites()
        {
            await Task.Run(() =>
            {
                foreach (var state in CurrentStatesBeingManaged)
                {
                    var query = string.Format("Update [dbo].[Websites] Set [HospitalsChangedWarning]=1 Where [StateContext] like '%{0}%' and ([HospitalsChangedWarning] = 0 or [HospitalsChangedWarning] is null);",
                                               state);
                    _session.CreateSQLQuery(query)
                            .SetTimeout(5000)
                            .ExecuteUpdate();
                }
            });
        }

        /// <summary>
        /// Begins the import.
        /// </summary>
        protected override void BeginImport()
        {
            base.BeginImport();

            var statesAbbrevList = CurrentStatesBeingManaged.Where(x => x != null).Select(x => x).ToList();

            _session = Provider.SessionFactory.OpenStatelessSession();
            _transaction = _session.BeginTransaction();

            if (_finalHospitals == null) _finalHospitals = new List<Tuple<Hospital, bool, int?>>();
            if (_finalArchivedHospitals == null) _finalArchivedHospitals = new List<Hospital>();

            var queryRegionSpec = new Func<Region, bool>(r => CurrentStatesBeingManaged.Any(s => r.State != null && s.ToUpper() == r.State.ToUpper()));

            var allStateRegions = _session.Query<Region>()
                                         .Where(queryRegionSpec)
                                         .Select(r => new Tuple<string, Region>(r.GetType().Name, r)).ToList();

            var queryCountySpec = new Func<County, bool>(r => CurrentStatesBeingManaged.Any(s => r.State != null && s.ToUpper() == r.State.ToUpper()));

            _availableCounties = new List<County>(_session.Query<County>().Where(queryCountySpec).ToList());

            _availableCustomRegions = new List<CustomRegion>(allStateRegions.Where(sr => sr.Item1.ToUpper() == typeof(CustomRegion).Name.ToUpper())
                                                           .Select(sr => sr.Item2.As<CustomRegion>()).ToList());

            _availableHealthReferralRegions = new List<HealthReferralRegion>(allStateRegions.Where(sr => sr.Item1.ToUpper() == typeof(HealthReferralRegion).Name.ToUpper())
                                                                   .Select(sr => sr.Item2.As<HealthReferralRegion>()).ToList());

            _availableHospitalServiceAreas = new List<HospitalServiceArea>(allStateRegions.Where(sr => sr.Item1.ToUpper() == typeof(HospitalServiceArea).Name.ToUpper())
                                                                  .Select(sr => sr.Item2.As<HospitalServiceArea>()).ToList());

            var queryHospitalSpec = new Func<Hospital, bool>(r => CurrentStatesBeingManaged.Any(s => r.State != null && s.ToUpper() == r.State.ToUpper()));

            _baseLatLongLookup = _session.Query<Hospital>()
                                         .Where(hosp => hosp.IsSourcedFromBaseData)
                                         .Where(queryHospitalSpec)
                                         .Select(hosp => new Tuple<string, double?, double?>(hosp.CmsProviderID, hosp.Latitude, hosp.Longitude))
                                         .Distinct()
                                         .OrderBy(h => h.Item1)
                                         .ToList();

            GetDefaultCategory(_session);
        }

        /// <summary>
        /// Processes the values.
        /// </summary>
        /// <param name="vals">The vals.</param>
        /// <param name="errors">The errors.</param>
        protected override void ProcessValues(string[] vals, IList<ImportError> errors)
        {
            var dynHospital = new HospitalDto();
            dynHospital.Dshospid = vals[0].IsNullOrEmpty() ? null : vals[0];
            dynHospital.FIPS = vals[1].IsNullOrEmpty() ? null : (int?)int.Parse(vals[1]);
            dynHospital.HospitalName = vals[2];
            dynHospital.Zip = vals[3];
            dynHospital.CCR = vals[4].IsNullOrEmpty() ? null : (decimal?)decimal.Parse(vals[4]);
            dynHospital.RegionId = vals[5].IsNullOrEmpty() ? null : (int?)Convert.ToInt32(vals[5]);
            dynHospital.CmsProvdrNum = vals[6].IsNullOrEmpty() ? null : vals[6];
            dynHospital.Address = vals[7].IsNullOrEmpty() ? null : vals[7];
            dynHospital.City = vals[8].IsNullOrEmpty() ? null : vals[8];
            dynHospital.Description = vals[9].IsNullOrEmpty() ? null : vals[9];
            dynHospital.Employees = vals[10].IsNullOrEmpty() ? null : (int?)Convert.ToInt32(vals[10]);
            dynHospital.TotalBeds = vals[11].IsNullOrEmpty() ? null : (int?)Convert.ToInt32(vals[11]);
            dynHospital.HospitalOwnership = vals[12].IsNullOrEmpty() ? null : vals[12];
            dynHospital.PhoneNumber = vals[13].IsNullOrEmpty() ? null : vals[13];
            dynHospital.FaxNumber = vals[14].IsNullOrEmpty() ? null : vals[14];
            dynHospital.MedicareMedicaidProvider = !vals[15].IsNullOrEmpty() && Convert.ToBoolean(Convert.ToInt32(vals[15]));
            dynHospital.EmergencyService = !vals[16].IsNullOrEmpty() && Convert.ToBoolean(Convert.ToInt32(vals[16]));
            dynHospital.TraumaService = !vals[17].IsNullOrEmpty() && Convert.ToBoolean(Convert.ToInt32(vals[17]));
            dynHospital.UrgentCareService = !vals[18].IsNullOrEmpty() && Convert.ToBoolean(Convert.ToInt32(vals[18]));
            dynHospital.PediatricService = !vals[19].IsNullOrEmpty() && Convert.ToBoolean(Convert.ToInt32(vals[19]));
            dynHospital.PediatricICUService = !vals[20].IsNullOrEmpty() && Convert.ToBoolean(Convert.ToInt32(vals[20]));
            dynHospital.CardiacCatherizationService = !vals[21].IsNullOrEmpty() && Convert.ToBoolean(Convert.ToInt32(vals[21]));
            dynHospital.PharmacyService = !vals[22].IsNullOrEmpty() && Convert.ToBoolean(Convert.ToInt32(vals[22]));
            dynHospital.DiagnosticXRayService = !vals[23].IsNullOrEmpty() && Convert.ToBoolean(Convert.ToInt32(vals[23]));
            if (vals.Length == 26)
            {
                if (!vals[24].IsNullOrEmpty()) dynHospital.Latitude = Convert.ToDouble(vals[24]);
                if (!vals[25].IsNullOrEmpty()) dynHospital.Longtitude = Convert.ToDouble(vals[25]);
            }

            ProcessHospital(dynHospital, errors);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="vals">The vals.</param>
        /// <returns></returns>
        protected override string GetName(string[] vals)
        {
            if(vals == null) return null;

            return !string.IsNullOrEmpty(vals[2]) && vals[2].Length > 0 ? vals[2].Trim() : null;
        }

        #region bring calls here to run under single operation
        /// <summary>
        /// Saves the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="hospital">The hospital.</param>
        private void Save(IStatelessSession session, Hospital hospital)
        {
            if (!hospital.IsPersisted)
                session.Insert(hospital);
            else
                session.Update(hospital);

            if (hospital.Categories.Any() && !hospital.IsArchived)
            {
                SaveHospitalCategories(session, hospital, new List<ImportError>());
            }
        }

        /// <summary>
        /// Saves the hospital categories.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="hospital">The hospital.</param>
        /// <param name="errors">The errors.</param>
        private void SaveHospitalCategories(IStatelessSession session, Hospital hospital, IList<ImportError> errors)
        {
            try
            {
                if (hospital.Id == 0) return;

                const string queryDelete = @"IF EXISTS (SELECT Category_Id FROM Hospitals_HospitalCategories where Hospital_Id = {0})
                        BEGIN
	                        DELETE FROM Hospitals_HospitalCategories where Hospital_Id = {0};
                        END";

                session.CreateSQLQuery(string.Format(queryDelete, hospital.Id.ToString(CultureInfo.CurrentCulture))).ExecuteUpdate();


                if (hospital.Categories != null && hospital.Categories.Any())
                {
                    foreach (var category in hospital.Categories)
                    {
                        var queryInsert = string.Format(@"INSERT INTO Hospitals_HospitalCategories (Hospital_Id, Category_Id) VALUES ({0}, {1})", hospital.Id, category.Id);

                        session.CreateSQLQuery(queryInsert).ExecuteUpdate();
                    }
                }
            }
            catch (Exception exc)
            {
                var errorMessage =
                        string.Format(
                            "An error occurred while trying to save hospital categories for \"{1}\":{0}{2}."
                            , System.Environment.NewLine, hospital.Name, (exc.InnerException ?? exc).Message);

                errors.Add(ImportError.Create("Hospital", hospital.Name, errorMessage));
            }

        }

        /// <summary>
        /// Gets the existing region.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session">The session.</param>
        /// <param name="regionId">The region identifier.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Sdk.Services.Import.Exceptions.ExistingEntityLookupException"></exception>
        protected T GetExistingRegion<T>(ISession session, int regionId) where T : Region
        {
            try
            {
                return session.CreateCriteria<T>()
                              .Add(Restrictions.Eq("ImportRegionId", regionId))
                              .UniqueResult<T>();
            }
            catch (Exception ex)
            {
                throw new ExistingEntityLookupException(ExportAttribute, regionId, ex);
            }
        }

        /// <summary>
        /// Gets the state by zip code.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="zipCode">The zip code.</param>
        /// <returns></returns>
        /// <exception cref="Monahrq.Sdk.Services.Import.Exceptions.ExistingEntityLookupException"></exception>
        private string GetStateByZipCode(IStatelessSession session, string zipCode)
        {
            try
            {
                return session.CreateCriteria<ZipCodeToHRRAndHSA>()
                              .Add(Restrictions.InsensitiveLike("Zip", zipCode))
                              .SetProjection(Projections.Property("State"))
                              .UniqueResult<string>();
            }
            catch (Exception ex)
            {
                throw new ExistingEntityLookupException(ExportAttribute, zipCode, ex);
            }
        }

        /// <summary>
        /// Gets the regions by zip code.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="zipCode">The zip code.</param>
        /// <returns></returns>
        private IDictionary<string, Region> GetRegionsByZipCode(IStatelessSession session, string zipCode)
        {
            var regionResults = new Dictionary<string, Region>();

            var firstOrDefault = session.CreateCriteria<ZipCodeToHRRAndHSA>()
                                        .Add(Restrictions.InsensitiveLike("Zip", zipCode))
                                        .SetProjection(Projections.ProjectionList()
                                                                  .Add(Projections.Property("Zip"), "Item1")
                                                                  .Add(Projections.Property("HRRNumber"), "Item2")
                                                                  .Add(Projections.Property("HSANumber"), "Item3"))
                                        .SetCacheable(true)
                                        .SetCacheRegion("RegionsByZipCode:" + zipCode)
                                        .SetResultTransformer(new DelegateTransformer<System.Tuple<string, int?, int?>>(x => new System.Tuple<string, int?, int?>(x[0].ToString(), int.Parse(x[1].ToString()), int.Parse(x[2].ToString()))))
                                        .UniqueResult<System.Tuple<string, int?, int?>>();

            if (firstOrDefault == null)
            {
                regionResults.Add("HRR", null);
                regionResults.Add("HSA", null);
                return regionResults;
            }

            var hrr = _availableHealthReferralRegions.FirstOrDefault(r => r.ImportRegionId == firstOrDefault.Item2);

            regionResults.Add("HRR", hrr);

            var hsa = _availableHospitalServiceAreas.FirstOrDefault(r => r.Id == firstOrDefault.Item3);

            regionResults.Add("HSA", hsa);

            return regionResults;
        }

        /// <summary>
        /// Gets the state of the county by fips and.
        /// </summary>
        /// <param name="fipsCounty">The fips county.</param>
        /// <param name="stateAbrevation">The state abrevation.</param>
        /// <returns></returns>
        private string GetCountyByFIPSAndState(string fipsCounty, string stateAbrevation)
        {
            return _availableCounties.Where(c => c.CountyFIPS.ToLower() == fipsCounty.ToLower() &&
                                                  c.State.ToLower() == stateAbrevation.ToLower())
                                     .Select(c => c.CountyFIPS)
                                     .FirstOrDefault();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        class HospitalDto
        {
            /// <summary>
            /// Gets or sets the dshospid.
            /// </summary>
            /// <value>
            /// The dshospid.
            /// </value>
            public string Dshospid { get; set; }
            /// <summary>
            /// Gets or sets the fips.
            /// </summary>
            /// <value>
            /// The fips.
            /// </value>
            public int? FIPS { get; set; }
            /// <summary>
            /// Gets or sets the name of the hospital.
            /// </summary>
            /// <value>
            /// The name of the hospital.
            /// </value>
            public string HospitalName { get; set; }
            /// <summary>
            /// Gets or sets the zip.
            /// </summary>
            /// <value>
            /// The zip.
            /// </value>
            public string Zip { get; set; }
            /// <summary>
            /// Gets or sets the CCR.
            /// </summary>
            /// <value>
            /// The CCR.
            /// </value>
            public decimal? CCR { get; set; }
            /// <summary>
            /// Gets or sets the region identifier.
            /// </summary>
            /// <value>
            /// The region identifier.
            /// </value>
            public int? RegionId { get; set; }
            /// <summary>
            /// Gets or sets the CMS provdr number.
            /// </summary>
            /// <value>
            /// The CMS provdr number.
            /// </value>
            public string CmsProvdrNum { get; set; }
            /// <summary>
            /// Gets or sets the state.
            /// </summary>
            /// <value>
            /// The state.
            /// </value>
            public string State { get; set; }
            /// <summary>
            /// Gets or sets the HRR region.
            /// </summary>
            /// <value>
            /// The HRR region.
            /// </value>
            public HealthReferralRegion HRRRegion { get; set; }
            /// <summary>
            /// Gets or sets the hsa region.
            /// </summary>
            /// <value>
            /// The hsa region.
            /// </value>
            public HospitalServiceArea HSARegion { get; set; }

            /// <summary>
            /// Gets or sets the address.
            /// </summary>
            /// <value>
            /// The address.
            /// </value>
            public string Address { get; set; }
            /// <summary>
            /// Gets or sets the city.
            /// </summary>
            /// <value>
            /// The city.
            /// </value>
            public string City { get; set; }
            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            /// <value>
            /// The description.
            /// </value>
            public string Description { get; set; }
            /// <summary>
            /// Gets or sets the employees.
            /// </summary>
            /// <value>
            /// The employees.
            /// </value>
            public int? Employees { get; set; }
            /// <summary>
            /// Gets or sets the total beds.
            /// </summary>
            /// <value>
            /// The total beds.
            /// </value>
            public int? TotalBeds { get; set; }
            /// <summary>
            /// Gets or sets the hospital ownership.
            /// </summary>
            /// <value>
            /// The hospital ownership.
            /// </value>
            public string HospitalOwnership { get; set; }
            /// <summary>
            /// Gets or sets the phone number.
            /// </summary>
            /// <value>
            /// The phone number.
            /// </value>
            public string PhoneNumber { get; set; }
            /// <summary>
            /// Gets or sets the fax number.
            /// </summary>
            /// <value>
            /// The fax number.
            /// </value>
            public string FaxNumber { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [medicare medicaid provider].
            /// </summary>
            /// <value>
            /// <c>true</c> if [medicare medicaid provider]; otherwise, <c>false</c>.
            /// </value>
            public bool MedicareMedicaidProvider { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [emergency service].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [emergency service]; otherwise, <c>false</c>.
            /// </value>
            public bool EmergencyService { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [trauma service].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [trauma service]; otherwise, <c>false</c>.
            /// </value>
            public bool TraumaService { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [urgent care service].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [urgent care service]; otherwise, <c>false</c>.
            /// </value>
            public bool UrgentCareService { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [pediatric service].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [pediatric service]; otherwise, <c>false</c>.
            /// </value>
            public bool PediatricService { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [pediatric icu service].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [pediatric icu service]; otherwise, <c>false</c>.
            /// </value>
            public bool PediatricICUService { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [cardiac catherization service].
            /// </summary>
            /// <value>
            /// <c>true</c> if [cardiac catherization service]; otherwise, <c>false</c>.
            /// </value>
            public bool CardiacCatherizationService { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [pharmacy service].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [pharmacy service]; otherwise, <c>false</c>.
            /// </value>
            public bool PharmacyService { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether [diagnostic x ray service].
            /// </summary>
            /// <value>
            /// <c>true</c> if [diagnostic x ray service]; otherwise, <c>false</c>.
            /// </value>
            public bool DiagnosticXRayService { get; set; }
            /// <summary>
            /// Gets or sets the latitude.
            /// </summary>
            /// <value>
            /// The latitude.
            /// </value>
            public double? Latitude { get; set; }
            /// <summary>
            /// Gets or sets the longtitude.
            /// </summary>
            /// <value>
            /// The longtitude.
            /// </value>
            public double? Longtitude { get; set; }

        }
    }


}
