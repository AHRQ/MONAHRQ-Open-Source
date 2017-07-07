using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Data;
using Monahrq.Infrastructure.Domain.Common;
using Monahrq.Infrastructure.Domain.Physicians;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Exceptions;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using NHibernate;
using Newtonsoft.Json.Linq;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using IsolationLevel = System.Data.IsolationLevel;
using ResourceHelper = Monahrq.Infrastructure.Utility.Extensions.ReflectionHelper;

namespace Monahrq.Sdk.Services.Import
{
    /// <summary>
    /// The Physicians Api Importer class. Retrieves real-time physician data from the https://data.medicare.gov/ api data services.
    /// </summary>
    public class PhysiciansImporter
    {
        #region Fields and Constants
        // https://data.medicare.gov/resource/mj5m-pzi6.json
        private const string HOST = "http://data.medicare.gov/resource/mj5m-pzi6.json?";
        private const string HOST_CSV = "http://data.medicare.gov/resource/mj5m-pzi6.csv?";
        public const string APP_TOKEN = "G0CpEv389cevkAlI4ErIWyuLx";
        private const int BATCH_SIZE = 10000;
        private const int TIME_OUT = 5000;
        private int _importedPhysicianCount;
        //private List<Infrastructure.Domain.Physicians.Physician> _savedPhysicians;
        private readonly IDomainSessionFactoryProvider _provider;
        private readonly IConfigurationService _configurationService;

        private readonly HybridDictionary _physiciansDictionary = new HybridDictionary();
        private readonly HybridDictionary _medicalPracticesDictionary = new HybridDictionary();

        private const string TEMP_FILE_NAME = "{0}_{1}_{2}.csv";
        private readonly string _tempFileDirectoryPath;

        private int _batchSizeToUse;
        private bool _fetchingBatch;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysiciansImporter"/> class.
        /// </summary>
        public PhysiciansImporter()
        {
            _provider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            _configurationService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            _tempFileDirectoryPath = Path.Combine(MonahrqContext.GetUserApplicatioTempFolder(), "PhysicianImport");
            TotalCombinedPhysicians = 0;
        }

        #region Properties and Events
        public event EventHandler ValueChanged;
        public event EventHandler FetchingBatchChanged;

        public string CurrentState { get; set; }
        /// <summary>
        /// Gets or sets the imported physician count.
        /// </summary>
        /// <value>
        /// The imported physician count.
        /// </value>
        public int ImportedPhysicianCount
        {
            get { return _importedPhysicianCount; }
            set
            {
                _importedPhysicianCount = value;
                OnValueChanged();
            }
        }

        /// <summary>
        /// Gets the total physicians.
        /// </summary>
        /// <value>
        /// The total physicians.
        /// </value>
        public int TotalPhysicians { get; private set; }

        public int TotalCombinedPhysicians { get; private set; }

        public bool FetchingBatch
        {
            get { return _fetchingBatch; }
            set
            {
                _fetchingBatch = value;
                OnFetchingBatchChanged();
            }
        }

        public bool HasErrors { get; set; } 

        /// <summary>
        /// Gets the total physicians saved.
        /// </summary>
        /// <value>
        /// The total physicians saved.
        /// </value>
        public int TotalPhysiciansSaved { get; private set; }

        /// <summary>
        /// Gets the total medical practices saved.
        /// </summary>
        /// <value>
        /// The total medical practices saved.
        /// </value>
        public int TotalMedicalPracticesSaved { get; private set; }

        /// <summary>
        /// Gets or sets the name of the physician bulk import target.
        /// </summary>
        /// <value>
        /// The name of the physician bulk import target.
        /// </value>
        public string PhysicianBulkImportTargetName { get; set; }

        /// <summary>
        /// Gets or sets the dataset identifier.
        /// </summary>
        /// <value>
        /// The dataset identifier.
        /// </value>
        public int DatasetId { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// Checks the internet connection.
        /// </summary>
        /// <returns></returns>
        private bool CheckInternetConnection()
        {
            return MonahrqContext.CheckIfConnectedToInternet(false);
        }

        /// <summary>
        /// Imports the physicians.
        /// </summary>
        /// <param name="downloadState">State of the download.</param>
        /// <exception cref="DataSetImportException">
        /// Internet connection lost or not able to connect.
        /// or
        /// Internet connection lost or not able to connect.
        /// or
        /// Internet connection lost or not able to connect.
        /// </exception>
        [Obsolete("This method has been deprecated. Please use the ImportPhysiciansTest3 method instead.")]
        public void ImportPhysicians(string downloadState)
        {
            // TODO: add the date to the arguments and filter based upon that? Or lookup based upon what's in the database in this procedure?
            try
            {
                if (!CheckInternetConnection())
                    throw new DataSetImportException("Internet connection lost or not able to connect.");

                ImportedPhysicianCount = 0;
                TotalPhysicians = 0;

                // TODO: Need to check for internet connection and timeouts.
                var request = WebRequest.Create(new Uri(string.Format("{0}st={1}&$select=count(*)", HOST, downloadState))) as HttpWebRequest;
                request.Method = "GET";
                request.ProtocolVersion = new Version("1.1");
                request.PreAuthenticate = true;
                request.Headers.Add("X-App-Token", APP_TOKEN);
                request.Accept = "application/json";

                using (var responseStream = request.GetResponse().GetResponseStream())
                {
                    string response = new StreamReader(responseStream).ReadToEnd();
                    response = response.Substring(1, response.Length - 2);
                    JObject json = JObject.Parse(response);
                    TotalPhysicians = (int)json["count"];
                }

                if (TotalPhysicians <= 0)
                {
                    return;
                    // Return error about no results returned? Or just "No new records found."?
                }

                TotalCombinedPhysicians += TotalPhysicians;

                using (var session = _provider.SessionFactory.OpenSession())
                {
                    using (var trans = session.BeginTransaction())
                    {
                               
                        session.SetBatchSize(BATCH_SIZE);
                        // Get the physicians in batches for importing.
                        for (int i = 0; i <= (TotalPhysicians / BATCH_SIZE); i += 1)
                        {

                            //DateTime start = DateTime.Now;
                             if (!CheckInternetConnection())
                                    throw new DataSetImportException("Internet connection lost or not able to connect.");

                                request = WebRequest.Create(new Uri(string.Format("{0}st={1}&$limit={2}&$offset={3}", HOST, downloadState, BATCH_SIZE, i)))
                                          as HttpWebRequest;
                                request.Method = "GET";
                                request.ProtocolVersion = new Version("1.1");
                                request.PreAuthenticate = true;
                                request.Headers.Add("X-App-Token", APP_TOKEN);
                                request.Accept = "application/json";
                                using (var responseStream = request.GetResponse().GetResponseStream())
                                {
                                    if (!CheckInternetConnection())
                                        throw new DataSetImportException("Internet connection lost or not able to connect.");

                                    string response = new StreamReader(responseStream).ReadToEnd();
                                    var physicians = JObject.Parse("{'Physicians':" + response + "}");
                                    foreach (var importedPhysician in physicians["Physicians"])
                                    {
                                        // TODO: Update progressbar.
                                        ImportedPhysicianCount++;

                                        int result;

                                        // Check if physician exists, if not create a new one.
                                        long npi = 0;
                                        if (importedPhysician.SelectToken("npi") != null)
                                        {
                                            long.TryParse(importedPhysician.SelectToken("npi").ToString(), out npi);
                                        }
                                        string indPacID = (importedPhysician.SelectToken("ind_pac_id") == null)
                                                              ? string.Empty
                                                              : importedPhysician.SelectToken("ind_pac_id").ToString();
                                        string profEnrollId = (importedPhysician.SelectToken("ind_enrl_id") == null)
                                                                  ? string.Empty
                                                                  : importedPhysician.SelectToken("ind_enrl_id")
                                                                                     .ToString();

                                        Physician physician = session.Query<Physician>()
                                                                     .FirstOrDefault(
                                                                         x =>
                                                                         x.Npi == npi && x.PacId == indPacID &&
                                                                         x.ProfEnrollId == profEnrollId);
      

                                        if (physician == null)
                                        {
                                            // Create a new physician.
                                            physician = new Physician();
                                            physician.Npi = npi;
                                            physician.PacId = indPacID;
                                            physician.ProfEnrollId = profEnrollId;

                                            TotalPhysiciansSaved++;
                                        }
                                        physician.SkipAudit = true;

                                        //Infrastructure.Domain.Physicians.Physician physician = GetPhysician(npi, pacID, profEnrollId);

                                        // Name and gender.
                                        physician.FirstName = (importedPhysician.SelectToken("frst_nm") == null)
                                                                  ? string.Empty
                                                                  : importedPhysician.SelectToken("frst_nm").ToString();
                                        physician.MiddleName = (importedPhysician.SelectToken("mid_nm") == null)
                                                                   ? string.Empty
                                                                   : importedPhysician.SelectToken("mid_nm").ToString();
                                        physician.LastName = (importedPhysician.SelectToken("lst_nm") == null)
                                                                 ? string.Empty
                                                                 : importedPhysician.SelectToken("lst_nm").ToString();
                                        physician.Suffix = (importedPhysician.SelectToken("suff") == null)
                                                               ? string.Empty
                                                               : importedPhysician.SelectToken("suff").ToString();
                                        if (importedPhysician.SelectToken("gndr") != null)
                                        {
                                            switch (importedPhysician.SelectToken("gndr").ToString())
                                            {
                                                case "M":
                                                    physician.Gender = GenderEnum.Male;
                                                    break;
                                                case "F":
                                                    physician.Gender = GenderEnum.Female;
                                                    break;
                                            }
                                        }

                                        // School information.
                                        physician.MedicalSchoolName = (importedPhysician.SelectToken("med_sch") == null)
                                                                          ? string.Empty
                                                                          : importedPhysician.SelectToken("med_sch")
                                                                                             .ToString();
                                        if (importedPhysician.SelectToken("grd_yr") != null)
                                        {
                                            if (Int32.TryParse(importedPhysician.SelectToken("grd_yr").ToString(),
                                                               out result))
                                            {
                                                physician.GraduationYear = result;
                                            }
                                        }

                                        // Medical specialties.
                                        physician.PrimarySpecialty = (importedPhysician.SelectToken("pri_spec") == null)
                                                                         ? string.Empty
                                                                         : importedPhysician.SelectToken("pri_spec")
                                                                                            .ToString();
                                        physician.SecondarySpecialty1 = (importedPhysician.SelectToken("sec_spec_1") ==
                                                                         null)
                                                                            ? string.Empty
                                                                            : importedPhysician.SelectToken("sec_spec_1")
                                                                                               .ToString();
                                        physician.SecondarySpecialty2 = (importedPhysician.SelectToken("sec_spec_2") ==
                                                                         null)
                                                                            ? string.Empty
                                                                            : importedPhysician.SelectToken("sec_spec_2")
                                                                                               .ToString();
                                        physician.SecondarySpecialty3 = (importedPhysician.SelectToken("sec_spec_3") ==
                                                                         null)
                                                                            ? string.Empty
                                                                            : importedPhysician.SelectToken("sec_spec_3")
                                                                                               .ToString();
                                        physician.SecondarySpecialty4 = (importedPhysician.SelectToken("sec_spec_4") ==
                                                                         null)
                                                                            ? string.Empty
                                                                            : importedPhysician.SelectToken("sec_spec_4")
                                                                                               .ToString();

                                        // Medicare assignment / participation.
                                        if (importedPhysician.SelectToken("assgn") != null)
                                        {
                                            switch (importedPhysician.SelectToken("assgn").ToString())
                                            {
                                                case "Y":
                                                    physician.AcceptsMedicareAssignment = MedicalAssignmentEnum.Y;
                                                    break;
                                                case "M":
                                                    physician.AcceptsMedicareAssignment = MedicalAssignmentEnum.M;
                                                    break;
                                            }
                                        }
                                        if (importedPhysician.SelectToken("erx") != null)
                                            physician.ParticipatesInERX =
                                                importedPhysician.SelectToken("erx").ToString() == "Y";
                                        if (importedPhysician.SelectToken("pqrs") != null)
                                            physician.ParticipatesInPQRS =
                                                importedPhysician.SelectToken("pqrs").ToString() ==
                                                "Y";
                                        if (importedPhysician.SelectToken("ehr") != null)
                                            physician.ParticipatesInEHR =
                                                importedPhysician.SelectToken("ehr").ToString() == "Y";

                                        //physician.CouncilBoardCertification = (importedPhysician.SelectToken("pri_spec") == null) ? string.Empty : importedPhysician.SelectToken("pri_spec").ToString();

                                        //ForeignLanguages = new List<LanguageModeEnum>();



                                        // Add the medical practice
                                        if (importedPhysician.SelectToken("org_pac_id") != null)
                                        {
                                            string orgPacID = importedPhysician.SelectToken("org_pac_id").ToString();

                                            // Check to see if pac ID is associated with the physician.

										PhysicianMedicalPractice physicianMedicalPractice =
											session.Query<PhysicianMedicalPractice>()
											.FirstOrDefault(x =>
												x.Physician.Npi == npi &&
												x.MedicalPractice.GroupPracticePacId == orgPacID);

										if (physicianMedicalPractice == null)
										{
                                            MedicalPractice medicalPractice = session
                                                .Query<MedicalPractice>()
                                                .FirstOrDefault(x => x.GroupPracticePacId == orgPacID);


                                            if (medicalPractice == null)
                                            {
                                                // There is no practice yet, so create it.
                                                medicalPractice = new MedicalPractice();
                                                medicalPractice.GroupPracticePacId = orgPacID;

                                                TotalMedicalPracticesSaved++;
                                            }
                                            medicalPractice.SkipAudit = true;

                                            // Update the rest of the info for the medical practice.
                                            medicalPractice.Name = (importedPhysician.SelectToken("org_lgl_nm") == null)
                                                                       ? string.Empty
																	   : importedPhysician.SelectToken("org_lgl_nm").ToString();
                                            if (importedPhysician.SelectToken("num_org_mem") != null)
                                            {
												if (Int32.TryParse(
                                                        importedPhysician.SelectToken("num_org_mem").ToString(),
                                                        out result))
                                                {
                                                    medicalPractice.NumberofGroupPracticeMembers = result;
                                                }
                                            }

											physicianMedicalPractice = new PhysicianMedicalPractice
													{
														Physician = physician,
														MedicalPractice = medicalPractice,
														AssociatedPMPAddresses = new List<int>()
													};
										}

                                            // Check to see if it's associated with the physician.
										if (physician.PhysicianMedicalPractices != null &&
											!physician.PhysicianMedicalPractices.Any(x => x.Equals(physicianMedicalPractice)))
                                            {
                                                // Add it to the physician.
                                                //MedicalPractice physicianMedicalPractice =
                                                //    new MedicalPractice();
                                                //physicianMedicalPractice.SkipAudit = true;
											physician.PhysicianMedicalPractices.Add(physicianMedicalPractice);
                                            }

										// Get this physician medical practice address.
                                            // Check to see if the address is part of the medical practice.
										// If it isn't, add it to the MedicalPractice.
										// Also check if this address is part of the Physician's Associated MedicalPractice Address.
										// If not, add it.
										var tokenAddressLine1	= importedPhysician.SelectToken("adr_ln_1");
										var tokenAddressLine2	= importedPhysician.SelectToken("adr_ln_2");
										var tokenCity			= importedPhysician.SelectToken("cty");
										var tokenState			= importedPhysician.SelectToken("st");
										var tokenZipCode		= importedPhysician.SelectToken("zip");

										if ((tokenAddressLine1 ?? tokenAddressLine2 ?? tokenCity ?? tokenState ?? tokenZipCode) != null)
                                            {
											string addressLine1 = (tokenAddressLine1 == null) ? string.Empty : tokenAddressLine1.ToString();
											string addressLine2 = (tokenAddressLine2 == null) ? string.Empty : tokenAddressLine2.ToString();
											string city			= (tokenCity == null) ? string.Empty : tokenCity.ToString();
											string state		= (tokenState == null) ? string.Empty : tokenState.ToString();
											string zipCode		= (tokenZipCode == null) ? string.Empty : tokenZipCode.ToString();

											MedicalPracticeAddress address = 
												session.Query<MedicalPracticeAddress>()
														.FirstOrDefault(x =>	x.Line1 == addressLine1 && x.Line2 == addressLine2 &&
                                                                                                 x.City == city && x.State == state &&
                                                                                                 x.ZipCode == zipCode);

											// If null, MedicalPracticeAddress doesn't exists.  We create it and add to MedicalPractice.
                                                if (address == null)
                                                {
                                                    address = new MedicalPracticeAddress();
                                                    address.Line1 = addressLine1;
                                                    address.Line2 = addressLine2;
                                                    address.City = city;
                                                    address.State = state;
                                                    address.ZipCode = zipCode;
                                                    // NOTE: Ignoring ln_2_sprs
                                                }
                                                address.SkipAudit = true;

											if (physicianMedicalPractice.MedicalPractice.Addresses == null)
												physicianMedicalPractice.MedicalPractice.Addresses = new List<MedicalPracticeAddress>();

											if (!physicianMedicalPractice.MedicalPractice.Addresses.Contains(address))
												physicianMedicalPractice.MedicalPractice.Addresses.Add(address);


											//  Check Physicians associated addresses.
											if (address.Id == null)
											{
												// TODO
												// Address is new to the Medical Practice.  Sadly we don't have an Id yet - so how do I get this "Id" to add it
												// to the AssociatedAddresses list.
												// ?? Will this work??
												session.SaveOrUpdate(address);
												physicianMedicalPractice.AssociatedPMPAddresses.Add(address.Id);
											}
											else
											{
												if (!physicianMedicalPractice.AssociatedPMPAddresses.Contains(address.Id))
													physicianMedicalPractice.AssociatedPMPAddresses.Add(address.Id);
											}
											
                                            }
                                        }

                                        // Check hospital affiliations.
                                        if (importedPhysician.SelectToken("hosp_afl_1") != null)
										CheckHospital(physician, importedPhysician.SelectToken("hosp_afl_1").ToString());
                                        if (importedPhysician.SelectToken("hosp_afl_2") != null)
										CheckHospital(physician, importedPhysician.SelectToken("hosp_afl_2").ToString());
                                        if (importedPhysician.SelectToken("hosp_afl_3") != null)
										CheckHospital(physician, importedPhysician.SelectToken("hosp_afl_3").ToString());
                                        if (importedPhysician.SelectToken("hosp_afl_4") != null)
										CheckHospital(physician, importedPhysician.SelectToken("hosp_afl_4").ToString());
                                        if (importedPhysician.SelectToken("hosp_afl_5") != null)
										CheckHospital(physician, importedPhysician.SelectToken("hosp_afl_5").ToString());

                                        //Credentials = new List<CredentialEnum>();

                                        session.SaveOrUpdate(physician);

                                        session.Flush();
                                        //trans.Commit();
                                        session.Clear();
                                    }
                                }
                                //session.Flush();
                                //trans.Commit();
                                //session.Clear();
                            }
                         trans.Commit();
                        //DateTime end = DateTime.Now;
                        //TimeSpan elapsed = end - start;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Imports the physicians test.
        /// </summary>
        /// <param name="downloadState">State of the download.</param>
        /// <exception cref="DataSetImportException">
        /// Internet connection lost or not able to connect.
        /// </exception>
        [Obsolete("This method has been deprecated. Please use the ImportPhysiciansTest3 method instead.")]
        public void ImportPhysiciansTest(string downloadState)
        {
            // TODO: add the date to the arguments and filter based upon that? Or lookup based upon what's in the database in this procedure?
            try
            {
                if (!CheckInternetConnection())
                    throw new DataSetImportException("Internet connection lost or not able to connect.");

                ImportedPhysicianCount = 0;
                TotalPhysicians = 0;

                // TODO: Need to check for internet connection and timeouts.
                var request = WebRequest.Create(new Uri(string.Format("{0}st={1}&$select=count(*)", HOST, downloadState))) as HttpWebRequest;

                if (request == null)
                    throw new DataSetImportException("Internet connection lost or not able to connect.");

                request.Method = "GET";
                request.ProtocolVersion = new Version("1.1");
                request.PreAuthenticate = true;
                request.Headers.Add("X-App-Token", APP_TOKEN);
                request.Accept = "application/json";

                try
                {

                    using (var sReader = new StreamReader(request.GetResponse().GetResponseStream()))
                    {
                        var response = sReader.ReadToEnd();
                        response = response.Substring(1, response.Length - 2);
                        JObject json = JObject.Parse(response);
						TotalPhysicians = (int)json["count"];
                    }

                }
                catch (WebException exc)
                {
                    throw new DataSetImportException("Internet connection lost or not able to connect. " + exc.GetBaseException().Message);
                }
                

                if (TotalPhysicians <= 0)
                {
                    return;
                    // Return error about no results returned? Or just "No new records found."?
                }

                var physiciansTable = InitializeDataTable(/*@"SELECT TOP 0 [Id],[Npi],[PacId],[ProfEnrollId],[FirstName],[MiddleName],[LastName],[Suffix],[Gender],[Credentials],[ForeignLanguages],[MedicalSchoolName],[GraduationYear],[CouncilBoardCertification],[PrimarySpecialty],[SecondarySpecialty1],
                                                            [SecondarySpecialty2],[SecondarySpecialty3],[SecondarySpecialty4],[AcceptsMedicareAssignment],[ParticipatesInERX],[ParticipatesInPQRS],[ParticipatesInEHR],[States],[Version],[IsDeleted]
                                                            FROM [dbo].[Physicians]",*/ typeof(Physician).EntityTableName());

                var medicalPracticesTable = InitializeDataTable(/*@"SELECT TOP 0 [Id],[Name],[GroupPracticePacId],[DBAName],[NumberofGroupPracticeMembers],[State],[Version]
                                                                  FROM [dbo].[MedicalPractices]",*/ "MedicalPractices");

                var addressesTable = InitializeDataTable(/*@"SELECT TOP 0 [Id],[AddressType],[Line1],[Line2],[Line3],[City],[State],[ZipCode],[Index],[Version],[MedicalPractice_Id]
                                                           FROM [dbo].[Addresses]",*/ "Addresses");

				var physicinMedPracticeTable = InitializeDataTable(/*@"SELECT TOP 0 [Physician_Id],[MedicalPractice_Id],[AssociatedPMPAddresses]
                                                                     FROM [dbo].[Physicians_MedicalPractices]",*/ "Physicians_MedicalPractices");

                using (var session = _provider.SessionFactory.OpenStatelessSession())
                {
                    session.SetBatchSize(BATCH_SIZE);

                    TotalMedicalPracticesSaved = 0;
                    TotalPhysiciansSaved = 0;

                    var blockingCollection = new BlockingCollection<Action<object>>();

                    // Get the physicians in batches for importing.
					for (int i = 0; i <= (TotalPhysicians / BATCH_SIZE); i += 1)
                    {
                        //DateTime start = DateTime.Now;
                        //using (var trans = session.BeginTransaction())
                        //{
                        if (!CheckInternetConnection())
                            throw new DataSetImportException("Internet connection lost or not able to connect.");

                        request = WebRequest.Create(new Uri(string.Format("{0}st={1}&$order=npi&$$exclude_system_fields=false&$limit={2}&$offset={3}",
                                        HOST, downloadState, BATCH_SIZE, i))) as HttpWebRequest;

                        if (request == null)
                            throw new DataSetImportException("Internet connection lost or not able to connect.");

                        request.Method = "GET";
                        request.ProtocolVersion = new Version("1.1");
                        request.PreAuthenticate = true;
                        request.Headers.Add("X-App-Token", APP_TOKEN);
                        request.Accept = "application/json";

                        //using (var sReader = new StreamReader(request.GetResponse().GetResponseStream()))
                        //{

                        using (var apiResponse = request.GetResponse())
                        {
                            if (!CheckInternetConnection())
                                throw new DataSetImportException(
                                    "Internet connection lost or not able to connect.");

                            string response;
                            using (var sReader = new StreamReader(apiResponse.GetResponseStream()))
                                response = sReader.ReadToEnd();

                            var physicians = JObject.Parse("{'Physicians':" + response + "}");
                            foreach (var importedPhysician in physicians["Physicians"])
                            {
                                // TODO: Update progressbar.
                                ImportedPhysicianCount++;

                                physiciansTable.Clear();
                                medicalPracticesTable.Clear();
                                physicinMedPracticeTable.Clear();
                                addressesTable.Clear();

                                //using (var trans = new TransactionScope(TransactionScopeOption.Required))
                                //{

                                // Check if physician exists, if not create a new one.
                                var npi = 0;
                                if (importedPhysician.SelectToken("npi") != null)
                                {
                                    Int32.TryParse(importedPhysician.SelectToken("npi").ToString(), out npi);
                                }
                                var indPacID = (importedPhysician.SelectToken("ind_pac_id") == null)
                                                   ? null
                                                   : importedPhysician.SelectToken("ind_pac_id").ToString();
                                var profEnrollId = (importedPhysician.SelectToken("ind_enrl_id") == null)
                                                       ? null
                                                       : importedPhysician.SelectToken("ind_enrl_id")
                                                                          .ToString();

                                var versionFileDate = (importedPhysician.SelectToken(":updated_at") == null)
														  ? (long?)null
                                                          : Convert.ToInt64(
                                                              importedPhysician.SelectToken(":updated_at").ToString());

                                //var physician = session.Query<Physician>()
                                //                       .FirstOrDefault(x => x.Npi == npi && x.PacId == indPacID && x.ProfEnrollId == profEnrollId);
                                
                                var query = new StringBuilder();
                                query.AppendLine("SELECT TOP 1 * FROM [dbo].[Physicians]");
                                query.AppendLine("WHERE [Npi]=" + npi + " AND [PacId]='" + indPacID +
                                                 "' AND [ProfEnrollId]='" + profEnrollId + "' ");


                                DataRow physicianRow = null;

                                //if (_physiciansDictionary.Contains(npi))
                                //    physicianRow = _physiciansDictionary[npi] as DataRow;

                                if (physicianRow == null)
                                {
                                    physicianRow = physiciansTable.NewRow();
                                    using (
                                        var sqlCommand = new SqlCommand(query.ToString(),
                                                                        session.Connection as SqlConnection))
                                    {
                                        //session.Transaction.Enlist(sqlCommand);

                                        //sqlCommand.CommandText = query.ToString();
                                        sqlCommand.CommandType = CommandType.Text;
                                        sqlCommand.CommandTimeout = TIME_OUT;

                                        using (SqlDataReader reader = sqlCommand.ExecuteReader())
                                        {
                                            //adapter.Fill(physiciansTable);

                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    physicianRow = reader.ToDataTableRow(physiciansTable);
                                                }
                                            }
                                            else
                                            {
                                                // Create a new physician.

                                                physicianRow["Npi"] = npi;
                                                physicianRow["PacId"] = indPacID;
                                                physicianRow["ProfEnrollId"] = profEnrollId;

                                                physicianRow["IsDeleted"] = 0;
                                                if (versionFileDate.HasValue)
                                                    physicianRow["Version"] = versionFileDate.Value;
                                                else
                                                    physicianRow["Version"] = DBNull.Value;
                                                physicianRow["States"] = DBNull.Value;

                                                TotalPhysiciansSaved++;
                                            }
                                            reader.Close();
                                        }
                                    }
                                }

                                int result;
                                if (physicianRow != null)
                                {
                                    //if (physician == null)
                                    //{
                                    //  // Create a new physician.
                                    //    physician = new Physician();
                                    //    physician.Npi = npi;
                                    //    physician.PacId = indPacID;
                                    //    physician.ProfEnrollId = profEnrollId;

                                    //    //TotalPhysiciansSaved++;
                                    //}

                                    // physician.SkipAudit = true;

                                    // Name and gender.
                                    // physician.FirstName = (importedPhysician.SelectToken("frst_nm") == null)
                                    //                                       ? string.Empty
                                    //                                       : importedPhysician.SelectToken("frst_nm").ToString();

                                    physicianRow["FirstName"] = (importedPhysician.SelectToken("frst_nm") ==
                                                                 null)
                                                                    ? string.Empty
                                                                    : importedPhysician.SelectToken("frst_nm")
                                                                                       .ToString();

                                    //physician.MiddleName = (importedPhysician.SelectToken("mid_nm") == null)
                                    //                                       ? string.Empty
                                    //                                       : importedPhysician.SelectToken("mid_nm").ToString();

                                    if (importedPhysician.SelectToken("mid_nm") == null)
                                        physicianRow["MiddleName"] = DBNull.Value;
                                    else
                                        physicianRow["MiddleName"] =
                                            importedPhysician.SelectToken("mid_nm").ToString();

                                    //physician.LastName = (importedPhysician.SelectToken("lst_nm") == null)
                                    //                                       ? string.Empty
                                    //                                       : importedPhysician.SelectToken("lst_nm").ToString();

                                    if (importedPhysician.SelectToken("lst_nm") == null)
                                        physicianRow["LastName"] = DBNull.Value;
                                    else
                                        physicianRow["LastName"] =
                                            importedPhysician.SelectToken("lst_nm").ToString();

                                    //physician.Suffix = (importedPhysician.SelectToken("suff") == null)
                                    //                                       ? null
                                    //                                       : importedPhysician.SelectToken("suff").ToString();

                                    if (importedPhysician.SelectToken("suff") == null)
                                        physicianRow["Suffix"] = DBNull.Value;
                                    else
                                        physicianRow["Suffix"] =
                                            importedPhysician.SelectToken("suff").ToString();

                                    if (importedPhysician.SelectToken("gndr") != null)
                                    {
                                        switch (importedPhysician.SelectToken("gndr").ToString())
                                        {
                                            case "M":
                                                physicianRow["Gender"] = GenderEnum.Male.ToString();
                                                //physician.Gender = GenderEnum.Male;
                                                break;
                                            case "F":
                                                physicianRow["Gender"] = GenderEnum.Female.ToString();
                                                // physician.Gender = GenderEnum.Female;
                                                break;
                                        }
                                    }
                                    else
                                        physicianRow["Gender"] = DBNull.Value;

                                    // School information.
                                    //physician.MedicalSchoolName = (importedPhysician.SelectToken("med_sch") == null)
                                    //                                  ? string.Empty
                                    //                                  : importedPhysician.SelectToken("med_sch").ToString();

                                    physicianRow["MedicalSchoolName"] = (importedPhysician.SelectToken("med_sch") == null)
                                                                                ? null
                                                                                : importedPhysician.SelectToken("med_sch").ToString();

                                    if (importedPhysician.SelectToken("grd_yr") != null)
                                    {
                                        if (Int32.TryParse(importedPhysician.SelectToken("grd_yr").ToString(),
                                                           out result))
                                        {
                                            // physician.GraduationYear = result;

                                            if (result <= 0)
                                                physicianRow["GraduationYear"] = DBNull.Value;
                                            else
                                                physicianRow["GraduationYear"] = result;
                                        }
                                        else
                                            physicianRow["GraduationYear"] = DBNull.Value;
                                    }
                                    else
                                        physicianRow["GraduationYear"] = DBNull.Value;

                                    // Medical specialties.
                                    //physician.PrimarySpecialty = (importedPhysician.SelectToken("pri_spec") == null)
                                    //                                               ? string.Empty
                                    //                                               : importedPhysician.SelectToken("pri_spec").ToString();

                                    if (importedPhysician.SelectToken("pri_spec") == null)
                                        physicianRow["PrimarySpecialty"] = DBNull.Value;
                                    else
                                        physicianRow["PrimarySpecialty"] =
                                            importedPhysician.SelectToken("pri_spec").ToString();

                                    //physician.SecondarySpecialty1 = (importedPhysician.SelectToken("sec_spec_1") == null)
                                    //                                                    ? null
                                    //                                                    : importedPhysician.SelectToken("sec_spec_1").ToString();

                                    if (importedPhysician.SelectToken("sec_spec_1") == null)
                                        physicianRow["SecondarySpecialty1"] = DBNull.Value;
                                    else
                                        physicianRow["SecondarySpecialty1"] =
                                            importedPhysician.SelectToken("sec_spec_1").ToString();

                                    //physician.SecondarySpecialty2 = (importedPhysician.SelectToken("sec_spec_2") == null)
                                    //                                                    ? null
                                    //                                                    : importedPhysician.SelectToken("sec_spec_2").ToString();

                                    if (importedPhysician.SelectToken("sec_spec_2") == null)
                                        physicianRow["SecondarySpecialty2"] = DBNull.Value;
                                    else
                                        physicianRow["SecondarySpecialty2"] =
                                            importedPhysician.SelectToken("sec_spec_2").ToString();

                                    //physician.SecondarySpecialty3 = (importedPhysician.SelectToken("sec_spec_3") == null)
                                    //                                    ? null
                                    //                                    : importedPhysician.SelectToken("sec_spec_3")
                                    //                                                       .ToString();

                                    if (importedPhysician.SelectToken("sec_spec_3") == null)
                                        physicianRow["SecondarySpecialty3"] = DBNull.Value;
                                    else
                                        physicianRow["SecondarySpecialty3"] =
                                            importedPhysician.SelectToken("sec_spec_3").ToString();

                                    //physician.SecondarySpecialty4 = (importedPhysician.SelectToken("sec_spec_4") == null)
                                    //                                    ? null
                                    //                                    : importedPhysician.SelectToken("sec_spec_4")
                                    //                                                       .ToString();

                                    if (importedPhysician.SelectToken("sec_spec_4") == null)
                                        physicianRow["SecondarySpecialty4"] = DBNull.Value;
                                    else
                                        physicianRow["SecondarySpecialty4"] =
                                            importedPhysician.SelectToken("sec_spec_4").ToString();

                                    // Medicare assignment / participation.
                                    if (importedPhysician.SelectToken("assgn") != null)
                                    {
                                        switch (importedPhysician.SelectToken("assgn").ToString())
                                        {
                                            case "Y":
                                                // physician.AcceptsMedicareAssignment = MedicalAssignmentEnum.Y;
                                                physicianRow["AcceptsMedicareAssignment"] =
                                                    MedicalAssignmentEnum.Y.ToString();
                                                break;
                                            case "M":
                                                //physician.AcceptsMedicareAssignment = MedicalAssignmentEnum.M;
                                                physicianRow["AcceptsMedicareAssignment"] =
                                                    MedicalAssignmentEnum.M.ToString();
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        physicianRow["AcceptsMedicareAssignment"] = DBNull.Value;
                                    }

                                    if (importedPhysician.SelectToken("erx") != null)
                                    {
                                        //physician.ParticipatesInERX = importedPhysician.SelectToken("erx").ToString() == "Y";
                                        physicianRow["ParticipatesInERX"] =
                                            importedPhysician.SelectToken("erx").ToString() == "Y" ? 1 : 0;
                                    }
                                    else
                                    {
                                        physicianRow["ParticipatesInERX"] = DBNull.Value;
                                    }

                                    if (importedPhysician.SelectToken("pqrs") != null)
                                    {
                                        //physician.ParticipatesInPQRS = importedPhysician.SelectToken("pqrs").ToString() == "Y";
                                        physicianRow["ParticipatesInPQRS"] =
                                            importedPhysician.SelectToken("pqrs").ToString() == "Y" ? 1 : 0;
                                    }
                                    else
                                    {
                                        physicianRow["ParticipatesInPQRS"] = DBNull.Value;
                                    }

                                    if (importedPhysician.SelectToken("ehr") != null)
                                    {
                                        //physician.ParticipatesInEHR = importedPhysician.SelectToken("ehr").ToString() == "Y";
                                        physicianRow["ParticipatesInEHR"] =
                                            importedPhysician.SelectToken("ehr").ToString() == "Y" ? 1 : 0;
                                    }
                                    else
                                    {
                                        physicianRow["ParticipatesInEHR"] = DBNull.Value;
                                    }

                                    // Check hospital affiliations.
                                    //if (importedPhysician.SelectToken("hosp_afl_1") != null)
                                    //    CheckHospital(physician,
                                    //                  importedPhysician.SelectToken("hosp_afl_1").ToString());
                                    //if (importedPhysician.SelectToken("hosp_afl_2") != null)
                                    //    CheckHospital(physician,
                                    //                  importedPhysician.SelectToken("hosp_afl_2").ToString());
                                    //if (importedPhysician.SelectToken("hosp_afl_3") != null)
                                    //    CheckHospital(physician,
                                    //                  importedPhysician.SelectToken("hosp_afl_3").ToString());
                                    //if (importedPhysician.SelectToken("hosp_afl_4") != null)
                                    //    CheckHospital(physician,
                                    //                  importedPhysician.SelectToken("hosp_afl_4").ToString());
                                    //if (importedPhysician.SelectToken("hosp_afl_5") != null)
                                    //    CheckHospital(physician,
                                    //                  importedPhysician.SelectToken("hosp_afl_5").ToString());

                                    physiciansTable.Rows.Add(physicianRow);
                                    //physiciansTable.ImportRow(physicianRow);
                                }

                                //physician.CouncilBoardCertification = (importedPhysician.SelectToken("pri_spec") == null) ? string.Empty : importedPhysician.SelectToken("pri_spec").ToString();

                                //ForeignLanguages = new List<LanguageModeEnum>();

                                // Save physician
                                var physicianId = Save(physiciansTable, session);

                                if (physicianRow["Id"] == DBNull.Value && physicianId > 0)
                                    physicianRow["Id"] = physicianId;

                                //if (
                                //    !_physiciansDictionary.Keys.OfType<object>()
                                //                          .ToList()
                                //                          .Any(k => k.ToString() == npi.ToString()))
                                //{
                                //    var clonedRow = physiciansTable.NewRow();
                                //    clonedRow.ItemArray = physicianRow.ItemArray.Clone() as object[];
                                //    _physiciansDictionary.Add(npi, clonedRow);
                                //}


                                // Add the medical practice
                                if (importedPhysician.SelectToken("org_pac_id") != null)
                                {
                                    string orgPacID = importedPhysician.SelectToken("org_pac_id").ToString();

                                    // Check to see if pac ID is associated with the physician.
                                    // var medicalPractice = session.Query<MedicalPractice>().FirstOrDefault(x => x.GroupPracticePacId == orgPacID);

                                    DataRow medicalPractRow = null;

                                    //if (_medicalPracticesDictionary.Contains(orgPacID))
                                    //    medicalPractRow = _medicalPracticesDictionary[orgPacID] as DataRow;

                                    if (medicalPractRow == null)
                                    {
                                        query = new StringBuilder();
                                        query.AppendLine("SELECT TOP 1 * FROM [dbo].[MedicalPractices] ");
                                        query.AppendLine("WHERE [GroupPracticePacId]='" + orgPacID + "'; ");

                                        medicalPractRow = medicalPracticesTable.NewRow();
                                        using (
                                            var sqlCommand = new SqlCommand(query.ToString(),
                                                                            session.Connection as SqlConnection)
                                            )
                                        {
                                            //sqlCommand.CommandText = query.ToString();
                                            sqlCommand.CommandType = CommandType.Text;
                                            sqlCommand.CommandTimeout = TIME_OUT;

                                            //session.Transaction.Enlist(sqlCommand);
                                            using (SqlDataReader reader = sqlCommand.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                        medicalPractRow = reader.ToDataTableRow(medicalPracticesTable);
                                                }
                                                else
                                                {
                                                    // Create a new physician.
                                                    medicalPractRow["GroupPracticePacId"] = orgPacID;
                                                    medicalPractRow["State"] = DBNull.Value;
                                                    if (versionFileDate.HasValue)
                                                        medicalPractRow["Version"] = versionFileDate.Value;
                                                    else
                                                        medicalPractRow["Version"] = DBNull.Value;

                                                    //medicalPracticesTable.Rows.Add(medicalPractRow);

                                                    TotalMedicalPracticesSaved++;
                                                }
                                                reader.Close();
                                            }
                                        }
                                    }

                                    if (medicalPractRow != null)
                                    {
                                        medicalPractRow["Name"] =
                                            (importedPhysician.SelectToken("org_lgl_nm") == null)
                                                ? string.Empty
                                                : importedPhysician.SelectToken("org_lgl_nm").ToString();


                                        if (importedPhysician.SelectToken("num_org_mem") != null)
                                        {
                                            if (
                                                Int32.TryParse(
                                                    importedPhysician.SelectToken("num_org_mem").ToString(),
                                                    out result))
                                            {
                                                //medicalPractice.NumberofGroupPracticeMembers = result > 0 ? result : (int?) null;

                                                if (result > 0)
                                                    medicalPractRow["NumberofGroupPracticeMembers"] = result;
                                                else
                                                    medicalPractRow["NumberofGroupPracticeMembers"] =
                                                        DBNull.Value;
                                            }
                                            else
                                                medicalPractRow["NumberofGroupPracticeMembers"] = DBNull.Value;
                                        }
                                        else
                                            medicalPractRow["NumberofGroupPracticeMembers"] = DBNull.Value;

                                        medicalPracticesTable.Rows.Add(medicalPractRow);
                                        //medicalPracticesTable.ImportRow(medicalPractRow);
                                    }

                                    //if (medicalPractice == null)
                                    //{
                                    //    // There is no practice yet, so create it.
                                    //    medicalPractice = new MedicalPractice();
                                    //    medicalPractice.GroupPracticePacId = orgPacID;

                                    //    //TotalMedicalPracticesSaved++;
                                    //}
                                    //medicalPractice.SkipAudit = true;

                                    // Update the rest of the info for the medical practice.
                                    //medicalPractice.Name = (importedPhysician.SelectToken("org_lgl_nm") == null)
                                    //                           ? string.Empty
                                    //                           : importedPhysician.SelectToken("org_lgl_nm").ToString();


                                    // Check to see if it's associated with the physician.
                                    //if (physician.Practices != null &&
                                    //    !physician.Practices.Any(x => x.Equals(medicalPractice)))
                                    //{
                                    //    // Add it to the physician.
                                    //    //MedicalPractice physicianMedicalPractice = new MedicalPractice();
                                    //    //physicianMedicalPractice.SkipAudit = true;
                                    //    physician.Practices.Add(medicalPractice);
                                    //}

                                    // Save Medical Practice
                                    var medicalPracticeId = Save(medicalPracticesTable, session);

                                    if ((medicalPractRow["Id"] == DBNull.Value) && medicalPracticeId > 0)
                                        medicalPractRow["Id"] = medicalPracticeId;

                                    //if (!_medicalPracticesDictionary.Contains(medicalPractRow["GroupPracticePacId"]))
                                    //{
                                    //    var clonedRow = medicalPracticesTable.NewRow();
                                    //    clonedRow.ItemArray = medicalPractRow.ItemArray.Clone() as object[];
                                    //    _medicalPracticesDictionary.Add(medicalPractRow["GroupPracticePacId"],
                                    //                                    medicalPractRow);
                                    //}


									// Check to see if medical practice / physician relationship is saved.


                                    query = new StringBuilder();
                                    query.AppendLine("SELECT TOP 1 * FROM [dbo].[" + physicinMedPracticeTable.TableName +
                                                     "] ");
									query.AppendLine("WHERE [Physician_Id]=" + physicianId + " ");
									query.AppendLine("  AND [MedicalPractice_Id]=" + medicalPracticeId + "; "); // 

                                    DataRow physicinMedPracticeRow = physicinMedPracticeTable.NewRow();

                                    using (
                                        var sqlCommand = new SqlCommand(query.ToString(),
                                                                        session.Connection as SqlConnection))
                                    {
                                        //sqlCommand.CommandText = query.ToString();
                                        sqlCommand.CommandType = CommandType.Text;
                                        sqlCommand.CommandTimeout = TIME_OUT;

                                        //session.Transaction.Enlist(sqlCommand);
                                        using (SqlDataReader reader = sqlCommand.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    physicinMedPracticeRow =
                                                        reader.ToDataTableRow(physicinMedPracticeTable);
                                                }
                                            }
                                            else
                                            {
                                                // Create a new physician address.
												physicinMedPracticeRow["Physician_Id"] = physicianId;
												physicinMedPracticeRow["MedicalPractice_Id"] = medicalPracticeId;
                                            }
                                            reader.Close();
                                        }
                                    }

                                    if (physicinMedPracticeRow != null)
                                        //physicinMedPracticeTable.ImportRow(physicinMedPracticeRow);
                                        physicinMedPracticeTable.Rows.Add(physicinMedPracticeRow);

                                    SavePMPRelation(physicinMedPracticeTable, session); // save relationship


                                    // Check to see if the address is part of the medical practice.
                                    // Get the physician's address.
                                    if (importedPhysician.SelectToken("adr_ln_1") != null ||
                                        importedPhysician.SelectToken("adr_ln_2") != null ||
                                        importedPhysician.SelectToken("cty") != null ||
                                        importedPhysician.SelectToken("st") != null ||
                                        importedPhysician.SelectToken("zip") != null)
                                    {
                                        var addressLine1 = (importedPhysician.SelectToken("adr_ln_1") == null)
                                                               ? string.Empty
                                                               : importedPhysician.SelectToken("adr_ln_1")
                                                                                  .ToString();

                                        var addressLine2 = (importedPhysician.SelectToken("adr_ln_2") == null)
                                                               ? null
                                                               : importedPhysician.SelectToken("adr_ln_2")
                                                                                  .ToString();

                                        var city = (importedPhysician.SelectToken("cty") == null)
                                                       ? string.Empty
                                                       : importedPhysician.SelectToken("cty").ToString();

                                        var state = (importedPhysician.SelectToken("st") == null)
                                                        ? string.Empty
                                                        : importedPhysician.SelectToken("st").ToString();

                                        var zipCode = (importedPhysician.SelectToken("zip") == null)
                                                          ? string.Empty
                                                          : importedPhysician.SelectToken("zip").ToString();


                                        //var address = session.Query<MedicalPracticeAddress>()
                                        //                     .FirstOrDefault(x => x.Line1 == addressLine1 && x.Line2 == addressLine2 &&
                                        //                                          x.City == city && x.State == state &&
                                        //                                          x.ZipCode == zipCode);

                                        query = new StringBuilder();
                                        query.AppendLine("SELECT TOP 1 * FROM [dbo].[Addresses] ");
                                        query.AppendLine(
											"WHERE [AddressType]='" + typeof(MedicalPractice).Name + "' AND [Line1]='" +
                                            addressLine1.Replace("'", "''") + "' ");
                                        if (!string.IsNullOrEmpty(addressLine2))
                                            query.Append(" AND [Line2]='" + addressLine2.Replace("'", "''") +
                                                         "' ");
                                        else
                                            query.Append(" AND [Line2] is NULL ");

                                        query.AppendLine(" AND [City] ='" + city + "' AND [State]='" + state +
                                                         "' AND [ZipCode]='" + zipCode +
                                                         "' AND [MedicalPractice_Id]=" + medicalPracticeId +
                                                         "; "); // 

                                        DataRow medicalPractAddressRow = addressesTable.NewRow();

                                        using (
                                            var sqlCommand = new SqlCommand(query.ToString(),
                                                                            session.Connection as SqlConnection)
                                            )
                                        {
                                            //sqlCommand.CommandText = query.ToString();
                                            //sqlCommand.CommandType = CommandType.Text;
                                            sqlCommand.CommandTimeout = TIME_OUT;

                                            //session.Transaction.Enlist(sqlCommand);
                                            using (SqlDataReader reader = sqlCommand.ExecuteReader())
                                            {
                                                if (reader.HasRows)
                                                {
                                                    while (reader.Read())
                                                    {
                                                        medicalPractAddressRow =
                                                            reader.ToDataTableRow(addressesTable);
                                                    }
                                                }
                                                else
                                                {
                                                    // Create a new physician address.
                                                    medicalPractAddressRow["AddressType"] = "MedicalPractice";
                                                    medicalPractAddressRow["Index"] = 0;
                                                    medicalPractAddressRow["Version"] = DateTime.Now.Ticks;
                                                    medicalPractAddressRow["MedicalPractice_Id"] = medicalPracticeId;
                                                }
                                                reader.Close();
                                            }
                                        }

                                        if (medicalPractAddressRow != null)
                                        {
                                            medicalPractAddressRow["Line1"] = addressLine1;

                                            if (string.IsNullOrEmpty(addressLine2))
                                                medicalPractAddressRow["Line2"] = DBNull.Value;
                                            else
                                                medicalPractAddressRow["Line2"] = addressLine2;

                                            medicalPractAddressRow["city"] = city;
                                            medicalPractAddressRow["state"] = state;
                                            medicalPractAddressRow["ZipCode"] = zipCode;

                                            addressesTable.Rows.Add(medicalPractAddressRow);
                                        }

                                        //if (address == null)
                                        //{
                                        //    address = new MedicalPracticeAddress();
                                        //    address.Line1 = addressLine1;
                                        //    address.Line2 = addressLine2;
                                        //    address.City = city;
                                        //    address.State = state;
                                        //    address.ZipCode = zipCode;
                                        //    address.Version = DateTime.Now.Ticks;
                                        //    //medicalPractAddressRow["Line1"] = addressLine1;
                                        //    //medicalPractAddressRow["Line2"] = addressLine2;
                                        //    //medicalPractAddressRow["city"] = city;
                                        //    //medicalPractAddressRow["state"] = state;
                                        //    //medicalPractAddressRow["ZipCode"] = zipCode;
                                        //    //medicalPractAddressRow["Index"] = 0;
                                        //    //medicalPractAddressRow["Version"] = DateTime.Now.Ticks;
                                        //    //medicalPractAddressRow["MedicalPractice_Id"] = medicalPracticeId;
                                        //    // NOTE: Ignoring ln_2_sprs
                                        //}
                                        //address.SkipAudit = true;

                                        //if (medicalPractice.Addresses == null)
                                        //    medicalPractice.Addresses = new List<MedicalPracticeAddress>();

                                        //if (!medicalPractice.Addresses.Contains(address))
                                        //    medicalPractice.Addresses.Add(address);

                                        // Save medical practice address
                                        Save(addressesTable, session);
                                    }
                                }
                                else if (importedPhysician.SelectToken("adr_ln_1") != null ||
                                         importedPhysician.SelectToken("adr_ln_2") != null ||
                                         importedPhysician.SelectToken("cty") != null ||
                                         importedPhysician.SelectToken("st") != null ||
                                         importedPhysician.SelectToken("zip") != null)
                                    // The address is associated with physician.
                                {
                                    var addressLine1 = (importedPhysician.SelectToken("adr_ln_1") == null)
                                                           ? string.Empty
                                                           : importedPhysician.SelectToken("adr_ln_1")
                                                                              .ToString();

                                    var addressLine2 = (importedPhysician.SelectToken("adr_ln_2") == null)
                                                           ? string.Empty
                                                           : importedPhysician.SelectToken("adr_ln_2")
                                                                              .ToString();

                                    var city = (importedPhysician.SelectToken("cty") == null)
                                                   ? string.Empty
                                                   : importedPhysician.SelectToken("cty").ToString();

                                    var state = (importedPhysician.SelectToken("st") == null)
                                                    ? string.Empty
                                                    : importedPhysician.SelectToken("st").ToString();

                                    var zipCode = (importedPhysician.SelectToken("zip") == null)
                                                      ? string.Empty
                                                      : importedPhysician.SelectToken("zip").ToString();

                                    //var address = session.Query<PhysicianAddress>()
                                    //                     .FirstOrDefault(x => x.Line1 == addressLine1 && x.Line2 == addressLine2 &&
                                    //                                          x.City == city && x.State == state &&
                                    //                                          x.ZipCode == zipCode);

                                    query = new StringBuilder();
                                    query.AppendLine("SELECT TOP 1 * FROM [dbo].[Addresses] ");
									query.AppendLine("WHERE [AddressType]='" + typeof(Physician).Name +
                                                     "' AND [Line1]='" + addressLine1.Replace("'", "''") + "' ");
                                    if (!string.IsNullOrEmpty(addressLine2))
                                        query.Append(" AND [Line2]='" + addressLine2.Replace("'", "''") + "' ");
                                    else
                                        query.Append(" AND [Line2] is NULL ");
                                    query.AppendLine("  AND [City] ='" + city.Replace("'", "''") +
                                                     "' AND [State]='" + state + "' AND [ZipCode]='" + zipCode +
                                                     "' AND [Physician_Id]=" + physicianId + "; ");

                                    //"SELECT TOP 1 * FROM [dbo].[Andresses] WHERE [AddressType]='Physician' AND [Line1]='" + addressLine1 + "' AND [Line2]='" + addressLine2 + "' " +
                                    //"AND [City] ='" + city + "' AND [State]='" + state + "' AND [ZipCode]='" + zipCode + "'; ";

                                    DataRow physicianAddressRow = addressesTable.NewRow();
                                    using (var sqlCommand = new SqlCommand(query.ToString(),
                                                                           session.Connection as SqlConnection))
                                    {
                                        //sqlCommand.CommandText = query.ToString();
                                        sqlCommand.CommandType = CommandType.Text;
                                        sqlCommand.CommandTimeout = TIME_OUT;

                                        using (SqlDataReader reader = sqlCommand.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    physicianAddressRow = reader.ToDataTableRow(addressesTable);
                                                    //addressesTable.Rows.Add(physicianAddressRow);
                                                }
                                            }
                                            else
                                            {
                                                // Create a new physician address.
                                                physicianAddressRow["AddressType"] = "Physician";
                                                physicianAddressRow["Index"] = 0;
                                                physicianAddressRow["Version"] = DateTime.Now.Ticks;
                                                physicianAddressRow["Physician_Id"] = physicianId;
                                            }
                                            reader.Close();
                                        }
                                    }

                                    if (physicianAddressRow != null)
                                    {
                                        physicianAddressRow["Line1"] = addressLine1;
                                        if (!string.IsNullOrEmpty(addressLine2))
                                            physicianAddressRow["Line2"] = addressLine2;
                                        else
                                            physicianAddressRow["Line2"] = DBNull.Value;

                                        physicianAddressRow["City"] = city;
                                        physicianAddressRow["State"] = state;
                                        physicianAddressRow["ZipCode"] = zipCode;

                                        addressesTable.Rows.Add(physicianAddressRow);
                                    }

                                    //if (address == null)
                                    //{
                                    //    address = new PhysicianAddress();
                                    //    address.Line1 = addressLine1;
                                    //    address.Line2 = addressLine2;
                                    //    address.City = city;
                                    //    address.State = state;
                                    //    address.ZipCode = zipCode;
                                    //    address.Version = DateTime.Now.Ticks;
                                    //    // NOTE: Ignoring ln_2_sprs
                                    //}
                                    //address.SkipAudit = true;

                                    //if (physician.Addresses == null)
                                    //    physician.Addresses = new List<PhysicianAddress>();

                                    //if (!physician.Addresses.Contains(address))
                                    //    physician.Addresses.Add(address);

                                    // Save physician's Address
                                    Save(addressesTable, session);
                                }

                                //if (ImportedPhysicianCount == BATCH_SIZE)
                                //{
                                //    Save(physiciansTable.Clone()/*, medicalPracticesTable.Clone(),
                                //             addressesTable.Clone(), physicinMedPracticeTable.Clone()*/);

                                //    physiciansTable.Rows.Clear();
                                //    medicalPracticesTable.Rows.Clear();
                                //    addressesTable.Rows.Clear();
                                //    physicinMedPracticeTable.Rows.Clear();
                                //}


                                //if (!physician.IsPersisted)
                                //    session.Insert(physician);
                                //else
                                //    session.Update(physician);

                                //session.Flush();
                                //session.Clear();

                                //    trans.Complete();
                                //}


                            }
                        }
                        //session.Flush();
                        //trans.Commit();
                        //session.Clear();
                        //}
                        //trans.Commit();
                        //DateTime end = DateTime.Now;
                        //TimeSpan elapsed = end - start;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Imports the physicians test2.
        /// </summary>
        /// <param name="downloadState">State of the download.</param>
        /// <exception cref="DataSetImportException">
        /// Internet connection lost or not able to connect.
        /// </exception>
        [Obsolete("This method has been deprecated. Please use the ImportPhysiciansTest3 method instead.")]
        public void ImportPhysiciansTest2(string downloadState)
        {
            var physiciansTable = InitializeDataTable(PhysicianBulkImportTargetName);


            using (var session = _provider.SessionFactory.OpenStatelessSession())
            {
                try
                {
                    if (!CheckInternetConnection())
                        throw new DataSetImportException("Internet connection lost or not able to connect.");

                    ImportedPhysicianCount = 0;
                    TotalPhysicians = 0;
                    TotalMedicalPracticesSaved = 0;
                    TotalPhysiciansSaved = 0;

                    // TODO: Need to check for internet connection and timeouts.
                    var request =
                        WebRequest.Create(new Uri(string.Format("{0}st={1}&$select=count(*)", HOST, downloadState))) as
                        HttpWebRequest;

                    if (request == null)
                        throw new DataSetImportException("Internet connection lost or not able to connect.");

                    request.Method = "GET";
                    request.ProtocolVersion = new Version("1.1");
                    request.PreAuthenticate = true;
                    request.Headers.Add("X-App-Token", APP_TOKEN);
                    request.Accept = "application/json";

                    try
                    {
                        using (var responseStream = request.GetResponse().GetResponseStream())
                        {
                            string response = "0";
							using (var sReader = new StreamReader(responseStream))
                                response = sReader.ReadToEnd();

                            response = response.Substring(1, response.Length - 2);
                            JObject json = JObject.Parse(response);
							TotalPhysicians = (int)json["count"];
                        }
                    }
                    catch (WebException exc)
                    {
                        throw new DataSetImportException("Internet connection lost or not able to connect. " +
                                                         exc.GetBaseException().Message);
                    }

                    if (TotalPhysicians <= 0)
                    {
                        return; // Return error about no results returned? Or just "No new records found."?
                    }

                    _batchSizeToUse = (TotalPhysicians < BATCH_SIZE) ? _batchSizeToUse = TotalPhysicians : BATCH_SIZE;

                    session.SetBatchSize(_batchSizeToUse);

                    // Get the physicians in batches for importing.
                    for (int i = 0; i <= (TotalPhysicians / _batchSizeToUse); i += 1)
                    {
                        //DateTime start = DateTime.Now;
                        //using (var trans = session.BeginTransaction())
                        //{
                        if (!CheckInternetConnection())
                            throw new DataSetImportException("Internet connection lost or not able to connect.");

                        request = 
                            WebRequest.Create(new Uri(string.Format(
                                        "{0}st={1}&$order=npi&$$exclude_system_fields=false&$limit={2}&$offset={3}",
                                        HOST, downloadState, _batchSizeToUse, i))) as HttpWebRequest;

                        if (request == null)
                            throw new DataSetImportException("Internet connection lost or not able to connect.");

                        request.Method = "GET";
                        request.ProtocolVersion = new Version("1.1");
                        request.PreAuthenticate = true;
                        request.Headers.Add("X-App-Token", APP_TOKEN);
                        request.Accept = "application/json";

                        using (var responseStream = request.GetResponse().GetResponseStream())
                        {
                            if (!CheckInternetConnection())
                                throw new DataSetImportException("Internet connection lost or not able to connect.");

                            string response = string.Empty;
                            using (var stream = new StreamReader(responseStream))
                                response = stream.ReadToEnd();

                            var physicians = JObject.Parse("{'Physicians':" + response + "}");
                            foreach (var importedPhysician in physicians["Physicians"])
                            {
                                // TODO: Update progressbar.
                                ImportedPhysicianCount++;

                                DataRow physicianRow = physiciansTable.NewRow();

                                // Check if physician exists, if not create a new one.
                                var npi = 0;
                                if (importedPhysician.SelectToken("npi") != null)
                                {
                                    Int32.TryParse(importedPhysician.SelectToken("npi").ToString(), out npi);
                                }
                                var indPacID = (importedPhysician.SelectToken("ind_pac_id") == null)
                                                   ? null
                                                   : importedPhysician.SelectToken("ind_pac_id").ToString();
                                var profEnrollId = (importedPhysician.SelectToken("ind_enrl_id") == null)
                                                       ? null
                                                       : importedPhysician.SelectToken("ind_enrl_id")
                                                                          .ToString();

                                var versionFileDate = (importedPhysician.SelectToken(":updated_at") == null)
														  ? (long?)null
                                                          : Convert.ToInt64(importedPhysician.SelectToken(":updated_at").ToString());

                                // Create a new physician.
                                physicianRow["Npi"] = npi;
                                physicianRow["PacId"] = indPacID;
                                physicianRow["ProfEnrollId"] = profEnrollId;

                                if (importedPhysician.SelectToken("frst_nm") == null)
                                    physicianRow["FirstName"] = DBNull.Value;
                                else 
                                    physicianRow["FirstName"] = importedPhysician.SelectToken("frst_nm").ToString();

                                if (importedPhysician.SelectToken("mid_nm") == null)
                                    physicianRow["MiddleName"] = DBNull.Value;
                                else
                                    physicianRow["MiddleName"] =
                                        importedPhysician.SelectToken("mid_nm").ToString();

                                if (importedPhysician.SelectToken("lst_nm") == null)
                                    physicianRow["LastName"] = DBNull.Value;
                                else
                                    physicianRow["LastName"] = importedPhysician.SelectToken("lst_nm").ToString();

                                if (importedPhysician.SelectToken("suff") == null)
                                    physicianRow["Suffix"] = DBNull.Value;
                                else
                                    physicianRow["Suffix"] =
                                        importedPhysician.SelectToken("suff").ToString();

                                if (importedPhysician.SelectToken("gndr") != null)
                                {
                                    switch (importedPhysician.SelectToken("gndr").ToString())
                                    {
                                        case "M":
                                            physicianRow["Gender"] = GenderEnum.Male.ToString();
                                            break;
                                        case "F":
                                            physicianRow["Gender"] = GenderEnum.Female.ToString();
                                            break;
                                    }
                                }
                                else
                                    physicianRow["Gender"] = DBNull.Value;

                                // School information.
                                if (importedPhysician.SelectToken("med_sch") != null)
                                    physicianRow["MedicalSchoolName"] = importedPhysician.SelectToken("med_sch").ToString();
                                else
                                    physicianRow["MedicalSchoolName"] = DBNull.Value;

                                int result;
                                if (importedPhysician.SelectToken("grd_yr") != null)
                                {
                                    if (Int32.TryParse(importedPhysician.SelectToken("grd_yr").ToString(), out result))
                                    {
                                        if (result <= 0)
                                            physicianRow["GraduationYear"] = DBNull.Value;
                                        else
                                            physicianRow["GraduationYear"] = result;
                                    }
                                    else
                                        physicianRow["GraduationYear"] = DBNull.Value;
                                }
                                else
                                    physicianRow["GraduationYear"] = DBNull.Value;

                                // Medical specialties.

                                if (importedPhysician.SelectToken("pri_spec") == null)
                                    physicianRow["PrimarySpecialty"] = DBNull.Value;
                                else
                                    physicianRow["PrimarySpecialty"] =
                                        importedPhysician.SelectToken("pri_spec").ToString();

                                if (importedPhysician.SelectToken("sec_spec_1") == null)
                                    physicianRow["SecondarySpecialty1"] = DBNull.Value;
                                else
                                    physicianRow["SecondarySpecialty1"] =
                                        importedPhysician.SelectToken("sec_spec_1").ToString();

                                if (importedPhysician.SelectToken("sec_spec_2") == null)
                                    physicianRow["SecondarySpecialty2"] = DBNull.Value;
                                else
                                    physicianRow["SecondarySpecialty2"] =
                                        importedPhysician.SelectToken("sec_spec_2").ToString();

                                if (importedPhysician.SelectToken("sec_spec_3") == null)
                                    physicianRow["SecondarySpecialty3"] = DBNull.Value;
                                else
                                    physicianRow["SecondarySpecialty3"] =
                                        importedPhysician.SelectToken("sec_spec_3").ToString();

                                if (importedPhysician.SelectToken("sec_spec_4") == null)
                                    physicianRow["SecondarySpecialty4"] = DBNull.Value;
                                else
                                    physicianRow["SecondarySpecialty4"] =
                                        importedPhysician.SelectToken("sec_spec_4").ToString();

                                // Medicare assignment / participation.
                                if (importedPhysician.SelectToken("assgn") != null)
                                {
                                    switch (importedPhysician.SelectToken("assgn").ToString())
                                    {
                                        case "Y":
                                            physicianRow["AcceptsMedicareAssignment"] = MedicalAssignmentEnum.Y.ToString();
                                            break;
                                        case "M":
                                            physicianRow["AcceptsMedicareAssignment"] = MedicalAssignmentEnum.M.ToString();
                                            break;
                                    }
                                }
                                else
                                {
                                    physicianRow["AcceptsMedicareAssignment"] = DBNull.Value;
                                }

                                if (importedPhysician.SelectToken("erx") != null)
                                {
                                    physicianRow["ParticipatesInERX"] =
                                        importedPhysician.SelectToken("erx").ToString() == "Y" ? 1 : 0;
                                }
                                else
                                {
                                    physicianRow["ParticipatesInERX"] = DBNull.Value;
                                }

                                if (importedPhysician.SelectToken("pqrs") != null)
                                {
                                    physicianRow["ParticipatesInPQRS"] =
                                        importedPhysician.SelectToken("pqrs").ToString() == "Y" ? 1 : 0;
                                }
                                else
                                {
                                    physicianRow["ParticipatesInPQRS"] = DBNull.Value;
                                }

                                if (importedPhysician.SelectToken("ehr") != null)
                                {
                                    physicianRow["ParticipatesInEHR"] =
                                        importedPhysician.SelectToken("ehr").ToString() == "Y" ? 1 : 0;
                                }
                                else
                                {
                                    physicianRow["ParticipatesInEHR"] = DBNull.Value;
                                }

                                // Medical practice related
                                if (importedPhysician.SelectToken("org_lgl_nm") == null)
                                    physicianRow["OrgLegalName"] = DBNull.Value;
                                else
                                    physicianRow["OrgLegalName"] = importedPhysician.SelectToken("org_lgl_nm")
                                                                                    .ToString();

                                if (importedPhysician.SelectToken("org_dba_nm") == null)
                                    physicianRow["DBAName"] = DBNull.Value;
                                else
                                    physicianRow["DBAName"] = importedPhysician.SelectToken("org_dba_nm")
                                                                                    .ToString();


                                if (importedPhysician.SelectToken("org_pac_id") != null)
                                    physicianRow["GroupPracticePacId"] = importedPhysician.SelectToken("org_pac_id").ToString();
                                else
                                    physicianRow["GroupPracticePacId"] = DBNull.Value;

                                

                                if (importedPhysician.SelectToken("num_org_mem") != null)
                                {
                                    if (Int32.TryParse(importedPhysician.SelectToken("num_org_mem").ToString(),
                                                       out result))
                                    {
                                        if (result > 0)
                                            physicianRow["NumberofGroupPracticeMembers"] = result;
                                        else
                                            physicianRow["NumberofGroupPracticeMembers"] = DBNull.Value;
                                    }
                                    else
                                        physicianRow["NumberofGroupPracticeMembers"] = DBNull.Value;
                                }
                                else
                                    physicianRow["NumberofGroupPracticeMembers"] = DBNull.Value;

                                // Check to see if the address is part of the medical practice.
                                // Get the physician's address.
                                if (importedPhysician.SelectToken("adr_ln_1") != null ||
                                    /*importedPhysician.SelectToken("adr_ln_2") != null ||*/
                                    importedPhysician.SelectToken("cty") != null ||
                                    importedPhysician.SelectToken("st") != null ||
                                    importedPhysician.SelectToken("zip") != null)
                                {
                                    var addressLine1 = (importedPhysician.SelectToken("adr_ln_1") == null)
                                                           ? string.Empty
                                                           : importedPhysician.SelectToken("adr_ln_1").ToString();

                                    var addressLine2 = (importedPhysician.SelectToken("adr_ln_2") == null)
                                                           ? null
                                                           : importedPhysician.SelectToken("adr_ln_2").ToString();

                                    var city = (importedPhysician.SelectToken("cty") == null)
                                                   ? string.Empty
                                                   : importedPhysician.SelectToken("cty").ToString();

                                    var state = (importedPhysician.SelectToken("st") == null)
                                                    ? string.Empty
                                                    : importedPhysician.SelectToken("st").ToString();

                                    var zipCode = (importedPhysician.SelectToken("zip") == null)
                                                      ? string.Empty
                                                      : importedPhysician.SelectToken("zip").ToString();

                                    physicianRow["Line1"] = addressLine1;

                                    if (string.IsNullOrEmpty(addressLine2))
                                        physicianRow["Line2"] = DBNull.Value;
                                    else
                                        physicianRow["Line2"] = addressLine2;

                                    if (importedPhysician.SelectToken("ln_2_sprs") == null)
                                        physicianRow["MarkerofAdressLine2Suppression"] = DBNull.Value;
                                    else
                                        physicianRow["MarkerofAdressLine2Suppression"] = (importedPhysician.SelectToken("ln_2_sprs").ToString() == "Y")
                                                ? 1 : 0;

                                    if (string.IsNullOrEmpty(city)) 
                                        physicianRow["city"] = DBNull.Value;
                                    else 
                                        physicianRow["city"] = city;

                                    physicianRow["state"] = state;
                                    physicianRow["ZipCode"] = zipCode;
                                }

                                if (!versionFileDate.HasValue)
                                    physicianRow["Version"] = DBNull.Value;
                                else
                                    physicianRow["Version"] = versionFileDate;

                                physicianRow["Dataset_Id"] = DatasetId;

                                physiciansTable.Rows.Add(physicianRow);

                                if (_batchSizeToUse == physiciansTable.Rows.Count ||
                                    TotalPhysicians == physiciansTable.Rows.Count)
                                {
                                    BulkInsert(physiciansTable, session.Connection.ConnectionString, _batchSizeToUse);
                                    physiciansTable.Clear();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    if (physiciansTable.Rows.Count > 0)
                    {
                        BulkInsert(physiciansTable, session.Connection.ConnectionString, _batchSizeToUse);
                        physiciansTable.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Imports the physicians test3.
        /// </summary>
        /// <param name="downloadState">State of the download.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <param name="runAsync">if set to <c>true</c> [run asynchronous].</param>
        /// <exception cref="DataSetImportException">
        /// Internet connection lost or not able to connect.
        /// </exception>
        /// <exception cref="AggregateException"></exception>
        public void ImportPhysiciansTest3(string downloadState, CancellationToken cancelToken, bool runAsync = true)
        {
            // TODO: add the date to the arguments and filter based upon that? Or lookup based upon what's in the database in this procedure?
            try
            {
                CurrentState = downloadState;

                if (!CheckInternetConnection())
                    throw new DataSetImportException("Internet connection lost or not able to connect.");

                ImportedPhysicianCount = 0;
                TotalPhysicians = 0;
                TotalMedicalPracticesSaved = 0;
                TotalPhysiciansSaved = 0;

                // TODO: Need to check for internet connection and timeouts.
                var request =
                    WebRequest.Create(new Uri(string.Format("{0}st={1}&$select=count(*)", HOST, downloadState))) as
                    HttpWebRequest;

                if (request == null)
                    throw new DataSetImportException("Internet connection lost or not able to connect.");

                request.Method = "GET";
                request.KeepAlive = true;
                request.Timeout = 300000;
                request.ProtocolVersion = new Version("1.1");
                request.PreAuthenticate = true;
                request.Headers.Add("X-App-Token", APP_TOKEN);
                request.Accept = "application/json";

                try
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    WebResponse webResponse;
                    try
                    {
                        webResponse = request.GetResponse();
                    }
                    catch (WebException exc)
                    {
                        string errorMessage = exc.GetBaseException().Message;
                        if (exc.Response != null)
                        {
                            using (WebResponse response = exc.Response)
                            {
                                HttpWebResponse httpResponse = (HttpWebResponse)response;
                                using (Stream data = response.GetResponseStream())
                                {
                                    using (var reader = new StreamReader(data))
                                    {
                                        errorMessage = reader.ReadToEnd();
                                    }
                                }
                            }
                        }

                        HasErrors = true;

                        throw new DataSetImportException("An error occurred while retrieving data from api. " +
                                                         errorMessage);
                    }

                    using (var sReader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        var response = sReader.ReadToEnd();
                        response = response.Substring(1, response.Length - 2);
                        JObject json = JObject.Parse(response);
						TotalPhysicians = (int)json["count"];
                    }
                }
                catch (WebException exc)
                {
                    throw new DataSetImportException("Internet connection lost or not able to connect. " +
                                                     exc.GetBaseException().Message);
                }


                if (TotalPhysicians <= 0)
                {
                    return;
                    // Return error about no results returned? Or just "No new records found."?
                }

                TotalCombinedPhysicians += TotalPhysicians;

                
                var exceptions = new List<Exception>();
                var version = DateTime.Now.Ticks;

                _batchSizeToUse = (TotalPhysicians < BATCH_SIZE) ? TotalPhysicians : BATCH_SIZE;


                if (!runAsync)
                {
                    var actions = new List<Action>();


                    // Get the physicians in batches for importing asynchronously.
                    for (var i = 0; i <= (TotalPhysicians / BATCH_SIZE); i += 1)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        ProcessResults2(_provider, downloadState, version, i,
                                        results =>
                                        {
                                            if (results.Status && !results.IsFinished)
                                                ImportedPhysicianCount++;
                                            else
                                            {
                                                TotalPhysiciansSaved = GetTotalItemsSaved(typeof(Physician), "Npi", string.Format("Where [States]='{0}';", downloadState));
                                                TotalMedicalPracticesSaved = GetTotalItemsSaved(typeof(MedicalPractice), "GroupPracticePacId", string.Format("Where [State]='{0}';", downloadState));
                                            }
                                        },
                                        results => {
                                            exceptions.Add(results.Exception);
                                        }, true, cancelToken);

                        if (HasErrors)
                            break;
                    }
                }
                else
                {
                    var actions = new List<Action>();


                    var po = new ParallelOptions
                        {
                            MaxDegreeOfParallelism = System.Environment.ProcessorCount,
                            CancellationToken = cancelToken
                        };
                    // Get the physicians in batches for importing asynchronously.
					for (var i = 0; i <= (TotalPhysicians / BATCH_SIZE); i += 1)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            break;
                        }
                        var processAction = new Action(() =>
                            {
                                ProcessResults2(_provider, downloadState, version, i,
                                                results =>
                                                    {
                                                        if (results.Status && !results.IsFinished)
                                                            ImportedPhysicianCount++;
                                                        else
                                                        {}
                                                    },
                                                results => exceptions.Add(results.Exception), true, cancelToken);
                            });

                        actions.Add(processAction);
                    }

                    Parallel.ForEach(actions, po, (k, loopstate) =>
                    {

                        if (cancelToken.IsCancellationRequested)
                        {
                            loopstate.Stop();
                        }

                        // Not really sure how this gets fired...
                        if (loopstate.ShouldExitCurrentIteration || loopstate.IsExceptional)
                            loopstate.Stop();


                        Thread.Sleep(1000);

                        k();

                    });
                }


                NormalizeData(downloadState);



                if (exceptions.Any())
                {
                    throw new AggregateException(exceptions);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void NormalizeData(string downloadState)
        {
            //Import/update database
            // Insert / Update Physicians, Addresses and Hospital Affiliation
            var physicianScript = ResourceHelper.ReadEmbeddedResourceFile(GetType().Assembly, "Monahrq.Sdk.Resources.PhysiciansImport.ImportUpdatePhysicians.sql")
                                                .Replace("[@@State@@]", downloadState);
            var medPracticeScript = ResourceHelper.ReadEmbeddedResourceFile(GetType().Assembly, "Monahrq.Sdk.Resources.PhysiciansImport.ImportUpdateMedicalPractices.sql")
                                                  .Replace("[@@State@@]", downloadState);
            var physicianMedPracticeScript = ResourceHelper.ReadEmbeddedResourceFile(GetType().Assembly, "Monahrq.Sdk.Resources.PhysiciansImport.ImportUpdatePhysiciansMedPractices.sql")
                                                           .Replace("[@@State@@]", downloadState);

            List<string> queryList = new List<string>();
            queryList.Add(physicianScript);
            queryList.Add(medPracticeScript);
            queryList.Add(physicianMedPracticeScript);
            queryList.Add(string.Format(@"UPDATE ph SET ph.[IsDeleted] =
                                        CASE WHEN tpt.[npi] is null THEN 1
                                        ELSE 0
                                        END
                                        FROM [dbo].[Physicians] AS ph
                                        LEFT JOIN [dbo].[Targets_PhysicianTargets] AS tpt
                                        ON ph.[npi] = tpt.[npi]
                                        WHERE ph.States = '{0}';", downloadState));
            
            foreach(var q in queryList)
            {
                using (var session = _provider.SessionFactory.OpenStatelessSession())
                {
                    using (var trans = session.BeginTransaction())
                    {
                        session.CreateSQLQuery(q)
                               .SetTimeout(180000)
                               .ExecuteUpdate();

                        trans.Commit();
                    }
                }
            }


            TotalPhysiciansSaved = GetTotalItemsSaved(typeof(Physician), "Npi",
                                                      string.Format("Where [States]='{0}';", downloadState));
            TotalMedicalPracticesSaved = GetTotalItemsSaved(typeof(MedicalPractice), "GroupPracticePacId",
                                                            string.Format("Where [State]='{0}';", downloadState));

            //clean up Targets_PhysicianTargets tables before import data
            using (var session = _provider.SessionFactory.OpenStatelessSession())
            {
                session.CreateSQLQuery("truncate table [dbo].[Targets_PhysicianTargets];")
                       .SetTimeout(300000)
                       .ExecuteUpdate();

            }
        }
        /// <summary>
        /// Deletes the temporary directory.
        /// </summary>
        public void DeleteTempDirectory()
        {
            try
            {
                if(Directory.Exists(_tempFileDirectoryPath))
                    Directory.Delete(_tempFileDirectoryPath, true);
            }
            catch
            {}
        }

        /// <summary>
        /// Gets the total items saved.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="columnToCheck">The column to check.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <returns></returns>
        private int GetTotalItemsSaved(Type entityType, string columnToCheck, string whereClause)
        {
            var query = new StringBuilder();
            query.AppendLine(string.Format("SELECT Count(Distinct [{0}]) from [dbo].[{1}] ", columnToCheck, entityType.EntityTableName()));
            query.AppendLine(whereClause);

            using (var session = _provider.SessionFactory.OpenStatelessSession())
            {
                return session.CreateSQLQuery(query.ToString()).UniqueResult<int>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        class CallBackResults
        {
            /// <summary>
            /// Gets or sets the state.
            /// </summary>
            /// <value>
            /// The state.
            /// </value>
            public string State { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="CallBackResults"/> is status.
            /// </summary>
            /// <value>
            ///   <c>true</c> if status; otherwise, <c>false</c>.
            /// </value>
            public bool Status { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this instance is finished.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is finished; otherwise, <c>false</c>.
            /// </value>
            public bool IsFinished { get; set; }
            /// <summary>
            /// Gets or sets the exception.
            /// </summary>
            /// <value>
            /// The exception.
            /// </value>
            public Exception Exception { get; set; }
        }

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="apiUri">The API URI.</param>
        /// <param name="apiToken">The API token.</param>
        /// <returns></returns>
        /// <exception cref="DataSetImportException">Internet connection lost or not able to connect.</exception>
        private HttpWebRequest CreateRequest(Uri apiUri, string apiToken)
        {
            var request = WebRequest.Create(apiUri) as HttpWebRequest;

            if (request == null)
                throw new DataSetImportException("Internet connection lost or not able to connect.");

            request.Method = "GET";
            request.KeepAlive = true;
            request.Timeout = 300000;
            request.ProtocolVersion = new Version("1.1");
            request.PreAuthenticate = true;
            request.Headers.Add("X-App-Token", APP_TOKEN);
            request.Accept = "text/csv; charset=utf-8";

            return request;
        }

        /// <summary>
        /// Processes the results2.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="stateToProcess">The state to process.</param>
        /// <param name="version">The version.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="progressCallBack">The progress call back.</param>
        /// <param name="exceptionCallBack">The exception call back.</param>
        /// <param name="runAsync">if set to <c>true</c> [run asynchronous].</param>
        /// <param name="cToken">The c token.</param>
        /// <exception cref="OperationCanceledException">
        /// </exception>
        /// <exception cref="DataSetImportException">
        /// Internet connection lost or not able to connect.
        /// </exception>
        /// <exception cref="TaskCanceledException"></exception>
        private void ProcessResults2(IDomainSessionFactoryProvider provider, string stateToProcess, long version, int offset,
                                     Action<CallBackResults> progressCallBack, Action<CallBackResults> exceptionCallBack, bool runAsync, CancellationToken cToken)
        {
			var obj = new object[] { };
            int recordCount = 0;
            var physiciansTable = InitializeDataTable(PhysicianBulkImportTargetName);

            if (!Directory.Exists(_tempFileDirectoryPath))
                Directory.CreateDirectory(_tempFileDirectoryPath);

            var tempFiles = Directory.EnumerateFiles(_tempFileDirectoryPath).ToList(); // Clear any temporary files in temp directory.
            if(tempFiles.Any())
            {
                lock (obj)
                {
                    foreach (var file in tempFiles)
                        File.Delete(file);
                }
            }

            var uploadFilePath = Path.Combine(_tempFileDirectoryPath, string.Format(TEMP_FILE_NAME, Guid.NewGuid().ToString().SubStrBefore("-"), stateToProcess, offset));

            try
            {
                if (cToken.IsCancellationRequested)
                    throw new OperationCanceledException(string.Format("user has cancelled importing physicians for state(s) \"{0}\"", stateToProcess));


                lock (obj)
                {
                    if (File.Exists(uploadFilePath))
                        File.Delete(uploadFilePath);
                }
                _batchSizeToUse = (TotalPhysicians < BATCH_SIZE) ? _batchSizeToUse = TotalPhysicians : BATCH_SIZE;

                // Get the physicians in batches for importing.
                if (!CheckInternetConnection())
                    throw new DataSetImportException("Internet connection lost or not able to connect.");

                var requestUri = new Uri(string.Format("{0}st={1}&$order=npi&$$exclude_system_fields=false&$limit={2}&$offset={3}", HOST_CSV, stateToProcess, _batchSizeToUse, _batchSizeToUse * offset)); //offset

                var request = CreateRequest(requestUri, APP_TOKEN);

                WebResponse webResponse;

                try
                {
                    webResponse = request.GetResponse();
                }
                catch(WebException exc)
                {
                    HasErrors = true;

                    string errorMessage = exc.Message;

                    if (exc.Response != null)
                    {
                        using (WebResponse response = exc.Response)
                        {
                            HttpWebResponse httpResponse = (HttpWebResponse)response;
                            using (Stream data = response.GetResponseStream())
                            {
                                using (var reader = new StreamReader(data))
                                {
                                    errorMessage = reader.ReadToEnd();
                                }
                            }
                        }
                    }

                    throw new DataSetImportException("An error occurred while retrieving data from api. " +
                                                     errorMessage);
                }

                using (var responseStream = webResponse.GetResponseStream())
                {
                    lock (obj)
                    {
                        //using (var s = File.Create(uploadFilePath))
                        //{
                        //    responseStream.CopyTo(s);
                        //}

                        using (FileStream ms = File.Create(uploadFilePath))
                        {
                            int count = 0;
                            do
                        {
                                byte[] buf = new byte[1024];
                                count = responseStream.Read(buf, 0, 1024);
                                ms.Write(buf, 0, count);
                            }
                            while (responseStream.CanRead && count > 0);
                        }

                    }
                }
                FetchingBatch = false;

                var tb = new DataTable("Temp");
                lock (obj)
                {
                    using (var connection = new OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
                                                Path.GetDirectoryName(uploadFilePath) +
                                                "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\""))
                    {
                        connection.Open();

                        using (
                            var cmd = new OleDbCommand("SELECT * FROM " + Path.GetFileName(uploadFilePath),
                                                       connection))
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    tb.Load(reader);
                                }
                            }
                        }
                    }
                }

                foreach (DataRow row in tb.Rows)
                {
                    var physicianRow = physiciansTable.NewRow();

                    physicianRow["Npi"] = row["NPI"];
                    physicianRow["PacId"] = row["PAC ID"];
                    physicianRow["ProfEnrollId"] = row["Professional Enrollment ID"];
                    physicianRow["LastName"] = row["Last Name"];
                    physicianRow["FirstName"] = row["First Name"];
                    physicianRow["MiddleName"] = row["Middle Name"];
                    physicianRow["Suffix"] = row["Suffix"];
                    physicianRow["Gender"] = row["Gender"];
                    physicianRow["Credential"] = row["Credential"];
                    physicianRow["MedicalSchoolName"] = row["Medical school name"];
                    physicianRow["GraduationYear"] = row["Graduation year"];
                    physicianRow["PrimarySpecialty"] = row["Primary specialty"];
                    physicianRow["SecondarySpecialty1"] = row["Secondary specialty 1"];
                    physicianRow["SecondarySpecialty2"] = row["Secondary specialty 2"];
                    physicianRow["SecondarySpecialty3"] = row["Secondary specialty 3"];
                    physicianRow["SecondarySpecialty4"] = row["Secondary specialty 4"];
                    physicianRow["AllSecondarySpecialties"] = row["All secondary specialties"];
                    physicianRow["OrgLegalName"] = row["Organization legal name"];
                    physicianRow["DBAName"] = DBNull.Value; //row["Organization DBA name"];
                    physicianRow["GroupPracticePacId"] = row["Group Practice PAC ID"];
                    physicianRow["NumberofGroupPracticeMembers"] = row["Number of Group Practice members"];
                    physicianRow["Line1"] = row["Line 1 Street Address"];
                    physicianRow["Line2"] = row["Line 2 Street Address"];
                    physicianRow["MarkerofAdressLine2Suppression"] = row["Marker of address line 2 suppression"] ==
                                                                     DBNull.Value ||
                                                                     row["Marker of address line 2 suppression"]
                                                                         .ToString() == "N"
                                                                         ? 0
                                                                         : 1;
                    physicianRow["City"] = row["City"];
                    physicianRow["State"] = row["State"];
                    physicianRow["ZipCode"] = row["Zip Code"];
                    if (row["Hospital affiliation CCN 1"] != DBNull.Value)
                        physicianRow["HospitalAffiliationCCN1"] = row["Hospital affiliation CCN 1"].ToString().PadLeft(6, '0');
                    //else 
                    //    physicianRow["HospitalAffiliationCCN1"] = DBNull.Value;
                    physicianRow["HospitalAffiliationLBN1"] = row["Hospital affiliation LBN 1"];
                    if (row["Hospital affiliation CCN 2"] != DBNull.Value)
                        physicianRow["HospitalAffiliationCCN2"] = row["Hospital affiliation CCN 2"].ToString().PadLeft(6, '0');
                    //else physicianRow["HospitalAffiliationCCN2"] = DBNull.Value;
                    physicianRow["HospitalAffiliationLBN2"] = row["Hospital affiliation LBN 2"];
                    if (row["Hospital affiliation CCN 3"] != DBNull.Value)
                        physicianRow["HospitalAffiliationCCN3"] = row["Hospital affiliation CCN 3"].ToString().PadLeft(6, '0');
                    //else physicianRow["HospitalAffiliationCCN3"] = DBNull.Value;
                    physicianRow["HospitalAffiliationLBN3"] = row["Hospital affiliation LBN 3"];
                    if (row["Hospital affiliation CCN 4"] != DBNull.Value)
                        physicianRow["HospitalAffiliationCCN4"] = row["Hospital affiliation CCN 4"].ToString().PadLeft(6, '0');
                    //else physicianRow["HospitalAffiliationCCN4"] = DBNull.Value;
                    physicianRow["HospitalAffiliationLBN4"] = row["Hospital affiliation LBN 4"];
                    if (row["Hospital affiliation CCN 5"] != DBNull.Value)
                        physicianRow["HospitalAffiliationCCN5"] = row["Hospital affiliation CCN 5"].ToString().PadLeft(6, '0');
                    //else physicianRow["HospitalAffiliationCCN5"] = DBNull.Value;
                    physicianRow["HospitalAffiliationLBN5"] = row["Hospital affiliation LBN 5"];

                    physicianRow["AcceptsMedicareAssignment"] = row["Professional accepts Medicare Assignment"];

                    physicianRow["ParticipatesInERX"] = DBNull.Value;
                    physicianRow["ParticipatesInPQRS"] = row["Reported Quality Measures"] == DBNull.Value ||
                                                         row["Reported Quality Measures"].ToString() == "N"
                                                             ? 0
                                                             : 1;
                    physicianRow["ParticipatesInEHR"] = row["Used electronic health records"] == DBNull.Value ||
                                                        row["Used electronic health records"].ToString() == "N"
                                                            ? 0
                                                            : 1;
                    physicianRow["Version"] = version;
                    physicianRow["Dataset_Id"] = DatasetId;

                    physiciansTable.Rows.Add(physicianRow);
                    
                    progressCallBack(new CallBackResults
                        {
                            Status = true,
                            State = stateToProcess
                        });
                    recordCount++;
                }

                if (cToken.IsCancellationRequested)
                    throw new OperationCanceledException(string.Format("user has cancelled importing physicians for state(s) \"{0}\"", stateToProcess));

                if (physiciansTable.Rows.Count == _batchSizeToUse)
                {
                    BulkInsert(physiciansTable, _configurationService.ConnectionSettings.ConnectionString,
                               _batchSizeToUse);
                    physiciansTable.Clear();
                }
            }
            catch (Exception exc)
            {
                HasErrors = true;

                exceptionCallBack(new CallBackResults
                    {
                        Status = false,
                        IsFinished = true,
                        State = stateToProcess,
                        Exception = exc.GetBaseException()
                    });
                return;
            }
            finally
            {
                if (!cToken.IsCancellationRequested)
                {
                    if (physiciansTable.Rows.Count > 0)
                    {
                        BulkInsert(physiciansTable, _configurationService.ConnectionSettings.ConnectionString,
                                   _batchSizeToUse);
                        physiciansTable.Clear();
                    }

                    lock (obj)
                    {
                        if (File.Exists(uploadFilePath))
                            File.Delete(uploadFilePath);
                    }
                }
            }

            try
            {
                if (HasErrors) return;

                //// Insert / Update Physicians, Addresses and Hospital Affiliation
                if (!runAsync)
                {
                    if (cToken.IsCancellationRequested)
                        throw new OperationCanceledException(string.Format("user has cancelled importing physicians for states \"{0}\"", stateToProcess));

                    FetchingBatch = true;
                    // Insert / Update Physicians, Addresses and Hospital Affiliation
                    var physicianScript = ResourceHelper.ReadEmbeddedResourceFile(GetType().Assembly,
                                                                                  "Monahrq.Sdk.Resources.PhysiciansImport.ImportUpdatePhysicians.sql")
                                                        .Replace("[@@State@@]", stateToProcess);
                    var medPracticeScript = ResourceHelper.ReadEmbeddedResourceFile(GetType().Assembly,
                                                                                    "Monahrq.Sdk.Resources.PhysiciansImport.ImportUpdateMedicalPractices.sql")
                                                          .Replace("[@@State@@]", stateToProcess);
                    var physicianMedPracticeScript = ResourceHelper.ReadEmbeddedResourceFile(GetType().Assembly,
                                                                                             "Monahrq.Sdk.Resources.PhysiciansImport.ImportUpdatePhysiciansMedPractices.sql")
                                                                   .Replace("[@@State@@]", stateToProcess);

                    var completQuery = new StringBuilder();

                    completQuery.AppendLine(physicianScript);
                    completQuery.AppendLine(medPracticeScript);
                    completQuery.AppendLine(physicianMedPracticeScript);
                     

                    using (var session = _provider.SessionFactory.OpenStatelessSession())
                    {
                        using (var trans = session.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            try
                            {
                                if (cToken.IsCancellationRequested)
                                    throw new TaskCanceledException(string.Format("user has cancelled importing physicians for states \"{0}\"", stateToProcess));

                                session.CreateSQLQuery(completQuery.ToString())
                                       .SetTimeout(180000)
                                       .ExecuteUpdate();

                                trans.Commit();
                            }
                            catch (OperationCanceledException tExc)
                            {
                                trans.Rollback();
                                throw new OperationCanceledException(tExc.GetBaseException().Message);
                            }
                            catch(Exception exc)
                            {
                                HasErrors = true;
                                trans.Rollback();
                                throw new DataSetImportException(exc.GetBaseException().Message);
                            }
                            finally
                            {
                                //EnableDisableTableIndexes(session, false, "Physicians_MedicalPractices");

                                session.CreateSQLQuery("truncate table [dbo].[Targets_PhysicianTargets];")
                                       .SetTimeout(300000)
                                       .ExecuteUpdate();
                            }
                        }

                        //session.CreateSQLQuery("truncate table [dbo].[Targets_PhysicianTargets];")
                        //       .SetTimeout(1000)
                        //       .ExecuteUpdate();
                    }
                }                

                progressCallBack(new CallBackResults
                {
                    Status = true,
                    State = stateToProcess,
                    IsFinished = true
                });
            }
            catch (Exception exc)
            {

                HasErrors = true;
                exceptionCallBack(new CallBackResults
                {
                    Status = false,
                    State = stateToProcess,
                    IsFinished = true,
                    Exception = exc.GetBaseException()
                });
                return;
            }
            finally
            {
                lock (obj)
                {
                    if (File.Exists(uploadFilePath))
                        File.Delete(uploadFilePath);
                }
            }
            
        }

        /// <summary>
        /// Processes the results.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="stateToProcess">The state to process.</param>
        /// <param name="offSet">The off set.</param>
        /// <param name="progressCallBack">The progress call back.</param>
        /// <param name="exceptionCallBack">The exception call back.</param>
        /// <exception cref="DataSetImportException">
        /// Internet connection lost or not able to connect.
        /// or
        /// Internet connection lost or not able to connect.
        /// or
        /// Internet connection lost or not able to connect.
        /// </exception>
        [Obsolete("This method has been deprecated. Please use the ProcessResults2 method instead.")]
        private void ProcessResults(IDomainSessionFactoryProvider provider, string stateToProcess, int offSet, Action<CallBackResults> progressCallBack, Action<CallBackResults> exceptionCallBack)
        {

            try
            {
				var physiciansTable = InitializeDataTable(typeof(Physician).EntityTableName());
                var medicalPracticesTable = InitializeDataTable("MedicalPractices");
                var addressesTable = InitializeDataTable("Addresses");
				var physicinMedPracticeTable = InitializeDataTable("Physicians_MedicalPractices");

            using (var session = provider.SessionFactory.OpenStatelessSession())
            {
                session.SetBatchSize(BATCH_SIZE);
                //DateTime start = DateTime.Now;
                //using (var trans = session.BeginTransaction())
                //{
                if (!CheckInternetConnection())
                    throw new DataSetImportException("Internet connection lost or not able to connect.");

                var request =
                    WebRequest.Create(
                        new Uri(
                            string.Format("{0}st={1}&$order=npi&$$exclude_system_fields=false&$limit={2}&$offset={3}",
                                          HOST, stateToProcess, BATCH_SIZE, offSet))) as HttpWebRequest;

                if (request == null)
                    throw new DataSetImportException("Internet connection lost or not able to connect.");

                request.Method = "GET";
                request.ProtocolVersion = new Version("1.1");
                request.PreAuthenticate = true;
                request.Headers.Add("X-App-Token", APP_TOKEN);
                request.Accept = "application/json";

                //using (var sReader = new StreamReader(request.GetResponse().GetResponseStream()))
                //{

                using (var apiResponse = request.GetResponse())
                {
                    if (!CheckInternetConnection())
                        throw new DataSetImportException(
                            "Internet connection lost or not able to connect.");

                    string response;
                    using (var sReader = new StreamReader(apiResponse.GetResponseStream()))
                        response = sReader.ReadToEnd();

                    var physicians = JObject.Parse("{'Physicians':" + response + "}");
                    foreach (var importedPhysician in physicians["Physicians"])
                    {
                        // TODO: Update progressbar.
                        //ImportedPhysicianCount++;

                        physiciansTable.Clear();
                        medicalPracticesTable.Clear();
                        physicinMedPracticeTable.Clear();
                        addressesTable.Clear();

                        //using (var trans = new TransactionScope(TransactionScopeOption.Required))
                        //{

                        // Check if physician exists, if not create a new one.
                        var npi = 0;
                        if (importedPhysician.SelectToken("npi") != null)
                        {
                            Int32.TryParse(importedPhysician.SelectToken("npi").ToString(), out npi);
                        }
                        var indPacID = (importedPhysician.SelectToken("ind_pac_id") == null)
                                           ? null
                                           : importedPhysician.SelectToken("ind_pac_id").ToString();
                        var profEnrollId = (importedPhysician.SelectToken("ind_enrl_id") == null)
                                               ? null
                                               : importedPhysician.SelectToken("ind_enrl_id")
                                                                  .ToString();

                        var versionFileDate = (importedPhysician.SelectToken(":updated_at") == null)
													  ? (long?)null
                                                  : Convert.ToInt64(
                                                      importedPhysician.SelectToken(":updated_at").ToString());

                        //var physician = session.Query<Physician>()
                        //                       .FirstOrDefault(x => x.Npi == npi && x.PacId == indPacID && x.ProfEnrollId == profEnrollId);

                        var query = new StringBuilder();
                        query.AppendLine("SELECT TOP 1 * FROM [dbo].[Physicians]");
                        query.AppendLine("WHERE [Npi]=" + npi + " AND [PacId]='" + indPacID +
                                         "' AND [ProfEnrollId]='" + profEnrollId + "' ");


                        DataRow physicianRow = null;

                        //if (_physiciansDictionary.Contains(npi))
                        //    physicianRow = _physiciansDictionary[npi] as DataRow;

                        if (physicianRow == null)
                        {
                            physicianRow = physiciansTable.NewRow();
                            using (
                                var sqlCommand = new SqlCommand(query.ToString(),
                                                                session.Connection as SqlConnection))
                            {
                                //session.Transaction.Enlist(sqlCommand);

                                //sqlCommand.CommandText = query.ToString();
                                sqlCommand.CommandType = CommandType.Text;
                                sqlCommand.CommandTimeout = TIME_OUT;

                                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                                {
                                    //adapter.Fill(physiciansTable);

                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            physicianRow = reader.ToDataTableRow(physiciansTable);
                                        }
                                    }
                                    else
                                    {
                                        // Create a new physician.

                                        physicianRow["Npi"] = npi;
                                        physicianRow["PacId"] = indPacID;
                                        physicianRow["ProfEnrollId"] = profEnrollId;

                                        physicianRow["IsDeleted"] = 0;
                                        if (versionFileDate.HasValue)
                                            physicianRow["Version"] = versionFileDate.Value;
                                        else
                                            physicianRow["Version"] = DBNull.Value;
                                        physicianRow["States"] = DBNull.Value;

                                        TotalPhysiciansSaved++;
                                    }
                                    reader.Close();
                                }
                            }
                        }

                        int result;
                        if (physicianRow != null)
                        {
                            physicianRow["FirstName"] = (importedPhysician.SelectToken("frst_nm") ==
                                                         null)
                                                            ? string.Empty
                                                            : importedPhysician.SelectToken("frst_nm")
                                                                               .ToString();

                            if (importedPhysician.SelectToken("mid_nm") == null)
                                physicianRow["MiddleName"] = DBNull.Value;
                            else
                                physicianRow["MiddleName"] =
                                    importedPhysician.SelectToken("mid_nm").ToString();

                            if (importedPhysician.SelectToken("lst_nm") == null)
                                physicianRow["LastName"] = DBNull.Value;
                            else
                                physicianRow["LastName"] =
                                    importedPhysician.SelectToken("lst_nm").ToString();

                            if (importedPhysician.SelectToken("suff") == null)
                                physicianRow["Suffix"] = DBNull.Value;
                            else
                                physicianRow["Suffix"] =
                                    importedPhysician.SelectToken("suff").ToString();

                            if (importedPhysician.SelectToken("gndr") != null)
                            {
                                switch (importedPhysician.SelectToken("gndr").ToString())
                                {
                                    case "M":
                                        physicianRow["Gender"] = GenderEnum.Male.ToString();
                                        //physician.Gender = GenderEnum.Male;
                                        break;
                                    case "F":
                                        physicianRow["Gender"] = GenderEnum.Female.ToString();
                                        // physician.Gender = GenderEnum.Female;
                                        break;
                                }
                            }
                            else
                                physicianRow["Gender"] = DBNull.Value;

                            // School information.

                            physicianRow["MedicalSchoolName"] = (importedPhysician.SelectToken("med_sch") == null)
                                                                    ? null
                                                                    : importedPhysician.SelectToken("med_sch").ToString();

                            if (importedPhysician.SelectToken("grd_yr") != null)
                            {
									if (Int32.TryParse(importedPhysician.SelectToken("grd_yr").ToString(), out result))
                                {
                                    // physician.GraduationYear = result;

                                    if (result <= 0)
                                        physicianRow["GraduationYear"] = DBNull.Value;
                                    else
                                        physicianRow["GraduationYear"] = result;
                                }
                                else
                                    physicianRow["GraduationYear"] = DBNull.Value;
                            }
                            else
                                physicianRow["GraduationYear"] = DBNull.Value;


                            if (importedPhysician.SelectToken("pri_spec") == null)
                                physicianRow["PrimarySpecialty"] = DBNull.Value;
                            else
                                physicianRow["PrimarySpecialty"] =
                                    importedPhysician.SelectToken("pri_spec").ToString();

                            //physician.SecondarySpecialty1 = (importedPhysician.SelectToken("sec_spec_1") == null)
                            //                                                    ? null
                            //                                                    : importedPhysician.SelectToken("sec_spec_1").ToString();

                            if (importedPhysician.SelectToken("sec_spec_1") == null)
                                physicianRow["SecondarySpecialty1"] = DBNull.Value;
                            else
                                physicianRow["SecondarySpecialty1"] =
                                    importedPhysician.SelectToken("sec_spec_1").ToString();

                            //physician.SecondarySpecialty2 = (importedPhysician.SelectToken("sec_spec_2") == null)
                            //                                                    ? null
                            //                                                    : importedPhysician.SelectToken("sec_spec_2").ToString();

                            if (importedPhysician.SelectToken("sec_spec_2") == null)
                                physicianRow["SecondarySpecialty2"] = DBNull.Value;
                            else
                                physicianRow["SecondarySpecialty2"] =
                                    importedPhysician.SelectToken("sec_spec_2").ToString();

                            //physician.SecondarySpecialty3 = (importedPhysician.SelectToken("sec_spec_3") == null)
                            //                                    ? null
                            //                                    : importedPhysician.SelectToken("sec_spec_3")
                            //                                                       .ToString();

                            if (importedPhysician.SelectToken("sec_spec_3") == null)
                                physicianRow["SecondarySpecialty3"] = DBNull.Value;
                            else
                                physicianRow["SecondarySpecialty3"] =
                                    importedPhysician.SelectToken("sec_spec_3").ToString();

                            //physician.SecondarySpecialty4 = (importedPhysician.SelectToken("sec_spec_4") == null)
                            //                                    ? null
                            //                                    : importedPhysician.SelectToken("sec_spec_4")
                            //                                                       .ToString();

                            if (importedPhysician.SelectToken("sec_spec_4") == null)
                                physicianRow["SecondarySpecialty4"] = DBNull.Value;
                            else
                                physicianRow["SecondarySpecialty4"] =
                                    importedPhysician.SelectToken("sec_spec_4").ToString();

                            // Medicare assignment / participation.
                            if (importedPhysician.SelectToken("assgn") != null)
                            {
                                switch (importedPhysician.SelectToken("assgn").ToString())
                                {
                                    case "Y":
                                        // physician.AcceptsMedicareAssignment = MedicalAssignmentEnum.Y;
                                        physicianRow["AcceptsMedicareAssignment"] =
                                            MedicalAssignmentEnum.Y.ToString();
                                        break;
                                    case "M":
                                        //physician.AcceptsMedicareAssignment = MedicalAssignmentEnum.M;
                                        physicianRow["AcceptsMedicareAssignment"] =
                                            MedicalAssignmentEnum.M.ToString();
                                        break;
                                }
                            }
                            else
                            {
                                physicianRow["AcceptsMedicareAssignment"] = DBNull.Value;
                            }

                            if (importedPhysician.SelectToken("erx") != null)
                            {
                                //physician.ParticipatesInERX = importedPhysician.SelectToken("erx").ToString() == "Y";
                                physicianRow["ParticipatesInERX"] =
                                    importedPhysician.SelectToken("erx").ToString() == "Y" ? 1 : 0;
                            }
                            else
                            {
                                physicianRow["ParticipatesInERX"] = DBNull.Value;
                            }

                            if (importedPhysician.SelectToken("pqrs") != null)
                            {
                                //physician.ParticipatesInPQRS = importedPhysician.SelectToken("pqrs").ToString() == "Y";
                                physicianRow["ParticipatesInPQRS"] =
                                    importedPhysician.SelectToken("pqrs").ToString() == "Y" ? 1 : 0;
                            }
                            else
                            {
                                physicianRow["ParticipatesInPQRS"] = DBNull.Value;
                            }

                            if (importedPhysician.SelectToken("ehr") != null)
                            {
                                //physician.ParticipatesInEHR = importedPhysician.SelectToken("ehr").ToString() == "Y";
                                physicianRow["ParticipatesInEHR"] =
                                    importedPhysician.SelectToken("ehr").ToString() == "Y" ? 1 : 0;
                            }
                            else
                            {
                                physicianRow["ParticipatesInEHR"] = DBNull.Value;
                            }

                            physiciansTable.Rows.Add(physicianRow);
                        }

                        //physician.CouncilBoardCertification = (importedPhysician.SelectToken("pri_spec") == null) ? string.Empty : importedPhysician.SelectToken("pri_spec").ToString();

                        //ForeignLanguages = new List<LanguageModeEnum>();

                        // Save physician
                        var physicianId = Save(physiciansTable, session);

                        if (physicianRow["Id"] == DBNull.Value && physicianId > 0)
                            physicianRow["Id"] = physicianId;

                        // Add the medical practice
							if (importedPhysician.SelectToken("org_pac_id") != null && (importedPhysician.SelectToken("org_lgl_nm") != null || importedPhysician.SelectToken("org_lgl_nm").ToString() != ""))
                        {
                            string orgPacID = importedPhysician.SelectToken("org_pac_id").ToString();

                            // Check to see if pac ID is associated with the physician.
                            // var medicalPractice = session.Query<MedicalPractice>().FirstOrDefault(x => x.GroupPracticePacId == orgPacID);
                            DataRow medicalPractRow = null;

                            if (medicalPractRow == null)
                            {
                                query = new StringBuilder();
                                query.AppendLine("SELECT TOP 1 * FROM [dbo].[MedicalPractices] ");
                                query.AppendLine("WHERE [GroupPracticePacId]='" + orgPacID + "'; ");

                                medicalPractRow = medicalPracticesTable.NewRow();
                                using (
                                    var sqlCommand = new SqlCommand(query.ToString(),
                                                                    session.Connection as SqlConnection))
                                {
                                    sqlCommand.CommandType = CommandType.Text;
                                    sqlCommand.CommandTimeout = TIME_OUT;

                                    //session.Transaction.Enlist(sqlCommand);
                                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                                    {
                                        if (reader.HasRows)
                                        {
                                            while (reader.Read())
                                                medicalPractRow = reader.ToDataTableRow(medicalPracticesTable);
                                        }
                                        else
                                        {
                                            // Create a new physician.
                                            medicalPractRow["GroupPracticePacId"] = orgPacID;
                                            medicalPractRow["State"] = DBNull.Value;
                                            if (versionFileDate.HasValue)
                                                medicalPractRow["Version"] = versionFileDate.Value;
                                            else
                                                medicalPractRow["Version"] = DBNull.Value;

                                            //medicalPracticesTable.Rows.Add(medicalPractRow);

                                            TotalMedicalPracticesSaved++;
                                        }
                                        reader.Close();
                                    }
                                }
                            }

                            if (medicalPractRow != null)
                            {
                                medicalPractRow["Name"] =
                                    (importedPhysician.SelectToken("org_lgl_nm") == null)
                                        ? string.Empty
                                        : importedPhysician.SelectToken("org_lgl_nm").ToString();


                                if (importedPhysician.SelectToken("num_org_mem") != null)
                                {
                                    if (Int32.TryParse(importedPhysician.SelectToken("num_org_mem").ToString(), out result))
                                    {
                                        if (result > 0)
                                            medicalPractRow["NumberofGroupPracticeMembers"] = result;
                                        else
                                            medicalPractRow["NumberofGroupPracticeMembers"] = DBNull.Value;
                                    }
                                    else
                                        medicalPractRow["NumberofGroupPracticeMembers"] = DBNull.Value;
                                }
                                else
                                    medicalPractRow["NumberofGroupPracticeMembers"] = DBNull.Value;

                                medicalPracticesTable.Rows.Add(medicalPractRow);
                            }

                            // Save Medical Practice
                            var medicalPracticeId = Save(medicalPracticesTable, session);

                            if ((medicalPractRow["Id"] == DBNull.Value) && medicalPracticeId > 0)
                                medicalPractRow["Id"] = medicalPracticeId;

                            // Check to see if medicial practice / physician relationship is saved.
                            query = new StringBuilder();
                            query.AppendLine("SELECT TOP 1 * FROM [dbo].[" + physicinMedPracticeTable.TableName +
                                             "] ");
								query.AppendLine("WHERE [Physician_Id]=" + physicianId + " ");
								query.AppendLine("  AND [MedicalPractice_Id]=" + medicalPracticeId + "; "); // 

                            DataRow physicinMedPracticeRow = physicinMedPracticeTable.NewRow();

                            using (var sqlCommand = new SqlCommand(query.ToString(), session.Connection as SqlConnection))
                            {
                                sqlCommand.CommandType = CommandType.Text;
                                sqlCommand.CommandTimeout = TIME_OUT;

                                //session.Transaction.Enlist(sqlCommand);
                                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            physicinMedPracticeRow =
                                                reader.ToDataTableRow(physicinMedPracticeTable);
                                        }
                                    }
                                    else
                                    {
                                        // Create a new physician address.
											physicinMedPracticeRow["Physician_Id"] = physicianId;
											physicinMedPracticeRow["MedicalPractice_Id"] = medicalPracticeId;
                                    }
                                    reader.Close();
                                }
                            }

                            if (physicinMedPracticeRow != null)
                                physicinMedPracticeTable.Rows.Add(physicinMedPracticeRow);

                            SavePMPRelation(physicinMedPracticeTable, session); // save relationship
                            
                            // Check to see if the address is part of the medical practice.
                            // Get the physician's address.
                            if (importedPhysician.SelectToken("adr_ln_1") != null ||
                                importedPhysician.SelectToken("adr_ln_2") != null ||
                                importedPhysician.SelectToken("cty") != null ||
                                importedPhysician.SelectToken("st") != null ||
                                importedPhysician.SelectToken("zip") != null)
                            {
                                var addressLine1 = (importedPhysician.SelectToken("adr_ln_1") == null)
                                                       ? string.Empty
                                                       : importedPhysician.SelectToken("adr_ln_1").ToString();

                                var addressLine2 = (importedPhysician.SelectToken("adr_ln_2") == null)
                                                       ? null
                                                       : importedPhysician.SelectToken("adr_ln_2").ToString();

                                var city = (importedPhysician.SelectToken("cty") == null)
                                               ? string.Empty
                                               : importedPhysician.SelectToken("cty").ToString();

                                var state = (importedPhysician.SelectToken("st") == null)
                                                ? string.Empty
                                                : importedPhysician.SelectToken("st").ToString();

                                var zipCode = (importedPhysician.SelectToken("zip") == null)
                                                  ? string.Empty
                                                  : importedPhysician.SelectToken("zip").ToString();


                                query = new StringBuilder();
                                query.AppendLine("SELECT TOP 1 * FROM [dbo].[Addresses] ");
									query.AppendLine("WHERE [AddressType]='" + typeof(MedicalPractice).Name + "' AND [Line1]='" + addressLine1.Replace("'", "''") + "' ");
                                if (!string.IsNullOrEmpty(addressLine2))
                                    query.Append(" AND [Line2]='" + addressLine2.Replace("'", "''") +
                                                 "' ");
                                else
                                    query.Append(" AND [Line2] is NULL ");

                                query.AppendLine(" AND [City] ='" + city + "' AND [State]='" + state +
                                                 "' AND [ZipCode]='" + zipCode +
                                                 "' AND [MedicalPractice_Id]=" + medicalPracticeId +
                                                 "; "); // 

                                DataRow medicalPractAddressRow = addressesTable.NewRow();

									using (var sqlCommand = new SqlCommand(query.ToString(), session.Connection as SqlConnection))
                                {
                                    sqlCommand.CommandType = CommandType.Text;
                                    sqlCommand.CommandTimeout = TIME_OUT;

                                    //session.Transaction.Enlist(sqlCommand);
                                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                                    {
                                        if (reader.HasRows)
                                        {
                                            while (reader.Read())
                                            {
                                                medicalPractAddressRow = reader.ToDataTableRow(addressesTable);
                                            }
                                        }
                                        else
                                        {
                                            // Create a new physician address.
                                            medicalPractAddressRow["AddressType"] = "MedicalPractice";
                                            medicalPractAddressRow["Index"] = 0;
                                            medicalPractAddressRow["Version"] = DateTime.Now.Ticks;
                                            medicalPractAddressRow["MedicalPractice_Id"] = medicalPracticeId;
                                        }
                                        reader.Close();
                                    }
                                }

                                if (medicalPractAddressRow != null)
                                {
                                    medicalPractAddressRow["Line1"] = addressLine1;

                                    if (string.IsNullOrEmpty(addressLine2))
                                        medicalPractAddressRow["Line2"] = DBNull.Value;
                                    else
                                        medicalPractAddressRow["Line2"] = addressLine2;

                                    medicalPractAddressRow["city"] = city;
                                    medicalPractAddressRow["state"] = state;
                                    medicalPractAddressRow["ZipCode"] = zipCode;

                                    addressesTable.Rows.Add(medicalPractAddressRow);
                                }

                                // Save medical practice address
                                Save(addressesTable, session);
                            }
                        }
                        else if (importedPhysician.SelectToken("adr_ln_1") != null ||
                                 importedPhysician.SelectToken("adr_ln_2") != null ||
                                 importedPhysician.SelectToken("cty") != null ||
                                 importedPhysician.SelectToken("st") != null ||
                                 importedPhysician.SelectToken("zip") != null)
                            // The address is associated with physician.
                        {
                            var addressLine1 = (importedPhysician.SelectToken("adr_ln_1") == null)
                                                   ? string.Empty
                                                   : importedPhysician.SelectToken("adr_ln_1").ToString();

                            var addressLine2 = (importedPhysician.SelectToken("adr_ln_2") == null)
                                                   ? string.Empty
                                                   : importedPhysician.SelectToken("adr_ln_2").ToString();

                            var city = (importedPhysician.SelectToken("cty") == null)
                                           ? string.Empty
                                           : importedPhysician.SelectToken("cty").ToString();

                            var state = (importedPhysician.SelectToken("st") == null)
                                            ? string.Empty
                                            : importedPhysician.SelectToken("st").ToString();

                            var zipCode = (importedPhysician.SelectToken("zip") == null)
                                              ? string.Empty
                                              : importedPhysician.SelectToken("zip").ToString();

                            query = new StringBuilder();
                            query.AppendLine("SELECT TOP 1 * FROM [dbo].[Addresses] ");
								query.AppendLine("WHERE [AddressType]='" + typeof(Physician).Name +
                                             "' AND [Line1]='" + addressLine1.Replace("'", "''") + "' ");
                            if (!string.IsNullOrEmpty(addressLine2))
                                query.AppendLine(" AND [Line2]='" + addressLine2.Replace("'", "''") + "' ");
                            else
                                query.AppendLine(" AND [Line2] is NULL ");
                            query.AppendLine("  AND [City] ='" + city.Replace("'", "''") +
                                             "' AND [State]='" + state + "' AND [ZipCode]='" + zipCode +
                                             "' AND [Physician_Id]=" + physicianId + "; ");

                            DataRow physicianAddressRow = addressesTable.NewRow();
                            using (var sqlCommand = new SqlCommand(query.ToString(),
                                                                   session.Connection as SqlConnection))
                            {
                                //sqlCommand.CommandText = query.ToString();
                                sqlCommand.CommandType = CommandType.Text;
                                sqlCommand.CommandTimeout = TIME_OUT;

                                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            physicianAddressRow = reader.ToDataTableRow(addressesTable);
                                        }
                                    }
                                    else
                                    {
                                        // Create a new physician address.
                                        physicianAddressRow["AddressType"] = "Physician";
                                        physicianAddressRow["Index"] = 0;
                                        physicianAddressRow["Version"] = DateTime.Now.Ticks;
                                        physicianAddressRow["Physician_Id"] = physicianId;
                                    }
                                    reader.Close();
                                }
                            }

                            if (physicianAddressRow != null)
                            {
                                physicianAddressRow["Line1"] = addressLine1;
                                if (!string.IsNullOrEmpty(addressLine2))
                                    physicianAddressRow["Line2"] = addressLine2;
                                else
                                    physicianAddressRow["Line2"] = DBNull.Value;

                                physicianAddressRow["City"] = city;
                                physicianAddressRow["State"] = state;
                                physicianAddressRow["ZipCode"] = zipCode;

                                addressesTable.Rows.Add(physicianAddressRow);
                            }

                            // Save physician's Address
                            Save(addressesTable, session);
                        }

                        progressCallBack(new CallBackResults
                            {
                                Status = true, 
                                State = stateToProcess
                            });
                    }
                }
            }
            }
            catch (Exception exc)
            {
                exceptionCallBack(new CallBackResults
                    {
                        Status = false,
                        State = stateToProcess,
                        Exception = exc.GetBaseException()
                    });
            }
        }

        /// <summary>
        /// Saves the specified data table.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        static int Save(DataTable dataTable, IStatelessSession session)
        {
            try
            {
                var rowId = 0;
                var isNewItem = false;

                if (dataTable == null || dataTable.Rows == null || dataTable.Rows.Count < 1)
                    return rowId;

                DataRow row = dataTable.Rows[0];
                IList<DataColumn> columns = new List<DataColumn>();
                if (row.Table.Columns.Contains("Id") && row["Id"] != null)
                {
                    int.TryParse(row["Id"].ToString(), out rowId);
                    isNewItem = rowId == 0;
                }

                columns = dataTable.Columns.OfType<DataColumn>().Where(c => c.ColumnName.ToLower() != "id")
                                   .ToList();

                var sqlScriptBuilder = new StringBuilder();
                if (isNewItem)
                {
                    sqlScriptBuilder.AppendLine("insert into [dbo].[" + dataTable + "] (" +
                                                string.Join(",", columns.Select(x => "[" + x.ColumnName + "]").ToList()) +
                                                ") ");
                    sqlScriptBuilder.AppendLine("values (");

                    //for (var columnCnt = 1; columnCnt < (columns.Count - 1); columnCnt++)
                    //{
                    //    values += ParseValue(columns[columnCnt], row[columnCnt]) + ",";
                    //}
                    var values = columns.Aggregate(string.Empty,
                                                   (current, column) =>
                                                   current + (ParseValue(column, row[column.ColumnName]) + ","));

                    values = values.SubStrBeforeLast(",");
                    sqlScriptBuilder.Append(values + " ); ");
                }
                else
                {
                    sqlScriptBuilder.AppendLine("UPDATE [dbo].[" + dataTable + "] ");
                    sqlScriptBuilder.AppendLine("SET ");

                    var values = columns.Aggregate(string.Empty,
                                                   (current, col) =>
                                                   current +
                                                   (" [" + col.ColumnName + "] = " +
                                                    ParseValue(col, row[col.ColumnName]) + ","));
                    values = values.SubStrBeforeLast(",");
                    sqlScriptBuilder.AppendLine(values);
                    sqlScriptBuilder.AppendLine("WHERE Id= " + rowId + "; ");
                }
                sqlScriptBuilder.AppendLine("SELECT CAST(SCOPE_IDENTITY() AS int);");
                //sqlScriptBuilder.AppendLine(" SELECT @@IDENTITY;");

                if (isNewItem)
                    rowId = session.CreateSQLQuery(sqlScriptBuilder.ToString()).UniqueResult<int>();
                else
                    session.CreateSQLQuery(sqlScriptBuilder.ToString()).ExecuteUpdate();

                return rowId;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Saves the PMP relation.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="session">The session.</param>
        static void SavePMPRelation(DataTable dataTable, IStatelessSession session)
        {
            if (dataTable.Rows.Count < 1)
                return;

            var physicianId = 0;
            var practiceId = 0;

            DataRow row = dataTable.Rows[0];
			if (row.Table.Columns.Contains("Physician_Id") && row["Physician_Id"] != DBNull.Value)
            {
				int.TryParse(row["Physician_Id"].ToString(), out physicianId);
            }
			if (row.Table.Columns.Contains("MedicalPractice_Id") && row["MedicalPractice_Id"] != DBNull.Value)
            {
				int.TryParse(row["MedicalPractice_Id"].ToString(), out practiceId);
            }

            var columns = dataTable.Columns.OfType<DataColumn>().Where(c => c.ColumnName.ToLower() != "id").ToList();

            var values = columns.Aggregate(string.Empty, (current, column) => current + (ParseValue(column, row[column.ColumnName]) + ","));
            values = values.SubStrBeforeLast(",");

            var sqlScriptBuilder = new StringBuilder();
			sqlScriptBuilder.AppendLine("IF NOT EXISTS(SELECT * FROM [dbo].[" + dataTable.TableName + "] WHERE [Physician_Id]=" + physicianId + " AND [MedicalPractice_Id]=" + practiceId + ")");
            sqlScriptBuilder.AppendLine("BEGIN");
            sqlScriptBuilder.AppendLine("   INSERT INTO [dbo].[" + dataTable.TableName + "] (" + string.Join(",", columns.Select(x => "[" + x.ColumnName + "]").ToList()) + ") ");
            sqlScriptBuilder.AppendLine("   VALUES (" + values + "); ");
            sqlScriptBuilder.AppendLine("END");
            //sqlScriptBuilder.AppendLine(" SELECT @@IDENTITY;");

            session.CreateSQLQuery(sqlScriptBuilder.ToString()).ExecuteUpdate();
        }

        /// <summary>
        /// Parses the value.
        /// </summary>
        /// <param name="col">The col.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        static object ParseValue(DataColumn col, object value)
        {
            if (value == null || value == DBNull.Value) return "NULL";

            var valueType = col.DataType;
            switch (valueType.Name)
            {
                case "Int16":
                case "Int32":
                case "Int64":
                case "Decimal":
                case "Double":
                case "Float":
                case "Long":
                case "Single":
                    return value;
                case "Date":
                case "DateTime":
                    return DateTime.Parse(value.ToString());
                case "Boolean":
                    return Boolean.Parse(value.ToString()) ? 1 : 0;
                default:
                    return "'" + value.ToString().Replace("'", "''") + "'";
            }
        }

        /// <summary>
        /// Initializes the data table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        DataTable InitializeDataTable(string tableName, string query = null)
        {
            DataTable dataTable = new DataTable(tableName);
            using (var session = _provider.SessionFactory.OpenStatelessSession())
            {
                var sqlStatement = query ?? "SELECT TOP 0 * FROM " + tableName;    // Dummy select to return 0 rows.
                //HospCompDataDt = new DataTable();
                using (var sqlDa = new SqlDataAdapter(sqlStatement, session.Connection as SqlConnection))
                    sqlDa.Fill(dataTable);
            }
            return dataTable;
        }

        /// <summary>
        /// Inserts the specified entity using sqlbulkcopy.
        /// </summary>
        /// <param name="targetTable">The target date table.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="batchSize">Size of the batch.</param>
        static void BulkInsert(DataTable targetTable, string connectionString, int batchSize = BATCH_SIZE)
        {
            var targetTableName = string.Format("dbo.[{0}]", targetTable.TableName);

            // Open a sourceConnection to the AdventureWorks database. 
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.DestinationTableName = targetTableName;
                        bulkCopy.BatchSize = targetTable.Rows.Count < batchSize ? targetTable.Rows.Count : batchSize;
                        bulkCopy.BulkCopyTimeout = 50000;

                        foreach (DataColumn column in targetTable.Columns)
                            bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName,
                                                                                     column.ColumnName));
                        // Write from the source to the destination.
                        bulkCopy.WriteToServer(targetTable);
                    }

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Checks the hospital.
        /// </summary>
        /// <param name="physician">The physician.</param>
        /// <param name="cmdId">The command identifier.</param>
        static void CheckHospital(Physician physician, string cmdId)
        {
            if (physician.AffiliatedHospitals == null)
            {
                physician.AffiliatedHospitals = new List<PhysicianAffiliatedHospital>();
            }

            if (physician.AffiliatedHospitals.All(x => x.HospitalCmsProviderId != cmdId))
            {
                PhysicianAffiliatedHospital affHosp = new PhysicianAffiliatedHospital();
                affHosp.SkipAudit = true;
                affHosp.HospitalCmsProviderId = cmdId;
                physician.AffiliatedHospitals.Add(affHosp);
            }
        }

        /// <summary>
        /// Called when [value changed].
        /// </summary>
        public void OnValueChanged()
        {
            if (ValueChanged != null)
                ValueChanged(this, new EventArgs());
        }

        /// <summary>
        /// Called when [fetching batch changed].
        /// </summary>
        public void OnFetchingBatchChanged()
        {
            if (FetchingBatchChanged != null)
                FetchingBatchChanged(this, new EventArgs());
        }
        #endregion

        /// <summary>
        /// Enables the disable table indexes.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="disable">if set to <c>true</c> [disable].</param>
        /// <param name="tableNames">The table names.</param>
        private void EnableDisableTableIndexes(IStatelessSession session, bool disable, params string[] tableNames)
        {
            //using (var session = Provi.SessionFactory.OpenStatelessSession())
            //{
            foreach (var tableName in tableNames)
            {
                session.CreateSQLQuery(EnableDisableNonClusteredIndexes(tableName, disable)) //ALL
                    .SetTimeout(5000)
                    .ExecuteUpdate();
            }
            //}
        }

        /// <summary>
        /// Enables the disable non clustered indexes.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="disable">if set to <c>true</c> [disable].</param>
        /// <returns></returns>
        private string EnableDisableNonClusteredIndexes(string tableName, bool disable = false)
        {
            var sqlStatement = new StringBuilder();

            sqlStatement.AppendLine("DECLARE @sql AS VARCHAR(MAX)='';" + System.Environment.NewLine);
            sqlStatement.Append("SELECT	 @sql = @sql + 'ALTER INDEX ' + sys.indexes.Name + ' ON " + tableName + " " + (disable ? "DISABLE" : "REBUILD") + "; ");

            if (!disable)
            {
                sqlStatement.Append(" ALTER INDEX ' + sys.indexes.Name + ' ON " + tableName + " REORGANIZE;");
            }

            sqlStatement.Append("' + CHAR(13) + CHAR(10)");
            sqlStatement.AppendLine();

            sqlStatement.AppendLine("FROM	 sys.indexes" + System.Environment.NewLine);
            sqlStatement.AppendLine("JOIN    sys.objects ON sys.indexes.object_id = sys.objects.object_id");
            sqlStatement.AppendLine("WHERE sys.indexes.type_desc = 'NONCLUSTERED'");
            sqlStatement.AppendLine("  AND sys.objects.type_desc = 'USER_TABLE'");
            sqlStatement.AppendLine("  AND sys.objects.Name = '" + tableName + "';");

            sqlStatement.AppendLine();
            sqlStatement.AppendLine("exec(@sql);");

            return sqlStatement.ToString();
        }
    }
}
