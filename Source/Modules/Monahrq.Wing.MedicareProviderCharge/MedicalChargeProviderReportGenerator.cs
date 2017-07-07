using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Domain.Websites.Maps;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Events;
using Monahrq.Sdk.Generators;
using NHibernate.Criterion;

namespace Monahrq.Wing.MedicareProviderCharge
{
    /// <summary>
    /// Class for medical charge provider.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Generators.BaseReportGenerator" />
    [Export(typeof(IReportGenerator)), PartCreationPolicy(CreationPolicy.Shared)]
    [ReportGenerator(new[] { "7AF51434-5745-4538-B972-193F58E737D7" },
                     new[] { "Medicare Provider Charge Data" },
					 new[] { typeof(MedicareProviderChargeTarget) },
					 1)]
    public class MedicalChargeProviderReportGenerator : BaseReportGenerator
    {
        private readonly IList<DataTable> _datatables;
        private const string JSON_DOMAIN = "$.monahrq.medicareprovidercharge=";

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicalChargeProviderReportGenerator"/> class.
        /// </summary>
        public MedicalChargeProviderReportGenerator()
        {
            _datatables = new List<DataTable>();
        }

        /// <summary>
        /// Creates the medicare provider cost t SQL statement.
        /// </summary>
        /// <param name="selectedStates">The selected states.</param>
        /// <param name="hospitalId">The hospital identifier.</param>
        /// <returns></returns>
        private string CreateMedicareProviderCostTSqlStatement(IEnumerable<string> selectedStates, int hospitalId)
        {
            var selectedStatesString = selectedStates.ToList().Aggregate(string.Empty, (current, state) => current + string.Format("'{0}',", state));
            if (selectedStatesString.EndsWith(","))
                selectedStatesString = selectedStatesString.SubStrBeforeLast(",");

			var datasetsIds = CurrentWebsite.Datasets.Select(ds => ds.Dataset.Id).ToList();
			datasetsIds.Add(-1);

            var sqlStatement = new StringBuilder();
            sqlStatement.AppendLine("select distinct top 25 h.[Id], ");
            sqlStatement.AppendLine("                h.[Name], ");
            sqlStatement.AppendLine("                m.[Provider_Id], ");
            sqlStatement.AppendLine("                m.[DRG_Id], ");
            sqlStatement.AppendLine("                m.[DRG], ");
            sqlStatement.AppendLine("                m.[TotalDischarges], ");
            sqlStatement.AppendLine("                m.[AverageCoveredCharges], ");
            sqlStatement.AppendLine("                Round(m.AverageCoveredCharges*cast(wh.CCR as real), 2) as 'MeanCostInDollars', ");
            sqlStatement.AppendLine("                m.[AverageTotalPayments], ");
            sqlStatement.AppendLine("                m.[Dataset_id] ");
            sqlStatement.AppendLine("from " + typeof(MedicareProviderChargeTarget).EntityTableName() + " m ");

            // New reading from website hospitals
            sqlStatement.AppendLine();
            sqlStatement.AppendLine("     inner join (" + typeof(Hospital).EntityTableName() + " h ");
            sqlStatement.AppendLine();
            sqlStatement.AppendLine("     inner join " + WebsiteTableNames.WebsiteHospitalsTable + " wh on wh.Hospital_Id=" + hospitalId + " and wh.Website_Id= " + CurrentWebsite.Id + " ) on h.CmsProviderID = m.Provider_Id ");
            sqlStatement.AppendLine();
            sqlStatement.AppendFormat("where upper(h.[State]) in (select distinct upper(s.[Abbreviation]) from " + typeof(State).EntityTableName() + " s where s.[Abbreviation] in ({0})) and h.Id={1} ", selectedStatesString, hospitalId);
            sqlStatement.AppendLine();
            sqlStatement.AppendFormat("	and	m.[Dataset_id] in ({0}) ", string.Join(",", datasetsIds));
            sqlStatement.AppendLine();
            sqlStatement.AppendLine("order by m.TotalDischarges desc, h.Name;");

            return sqlStatement.ToString();
        }

        /// <summary>
        /// Loads the report data needed for the data output.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Something seriously went wrong. The reporting website can not be null.</exception>
        protected override bool LoadReportData()
        {
            try
            {
                if (CurrentWebsite == null)
                    throw new InvalidOperationException("Something seriously went wrong. The reporting website can not be null.");

                if (_datatables != null && _datatables.Count > 0)
                    _datatables.Clear();

                var hospitalIds = Hospitals.Select(h => h.Id).ToList().ToList();

                if (hospitalIds.Any())
                {
                    foreach (var hospitalId in hospitalIds.ToList())
                    {
                        var sqlStstement = CreateMedicareProviderCostTSqlStatement(CurrentWebsite.SelectedReportingStates, hospitalId);
                        DataTable medicareDataTablesByDRG = base.RunSqlReturnDataTable(sqlStstement, null);
                        medicareDataTablesByDRG.TableName = String.Format("Hospital_{0}", hospitalId.ToString());

                        if (!_datatables.Any(dt => dt.TableName.EqualsIgnoreCase(String.Format("Hospital_{0}", hospitalId.ToString())) && dt.Rows.Count > 0))
                            _datatables.Add(medicareDataTablesByDRG);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Write(ex.InnerException ?? ex);
                return false;
            }
        }

        /// <summary>
        /// Outputs the report data files.
        /// </summary>
        /// <returns></returns>
        protected override bool OutputDataFiles()
        {
            try
            {
                var directoryPath = Path.Combine(BaseDataDirectoryPath, "medicare", "drg");
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                foreach (var dataTable in _datatables.ToList())
                {
                    var hositalFileName = Path.Combine(directoryPath, "hospital", dataTable.TableName, "summary.js");

                    var tableData = new TableData();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        var rowToSerialize = new ChargeData
                            {
                                DRGID = row["DRG_Id"].ToString(),
                                TotalDischarges = row["TotalDischarges"].ToString(),
                                AverageCoveredCharges = row["AverageCoveredCharges"].ToString(),
                                MeanCostInDollars = row["MeanCostInDollars"].ToString(),
                                AverageTotalPayments = row["AverageTotalPayments"].ToString()
                            };

                        tableData.Rows.Add(rowToSerialize);
                    }
                    GenerateJsonFile(tableData, hositalFileName, JSON_DOMAIN);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Write(ex.InnerException ?? ex);
                return false;
            }
        }

        /// <summary>
        /// Initializes any data objects needed by the report generator while executing. This method is call during the application initialization/bootstrap
        /// </summary>
        public override void InitGenerator()
        {
            EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Loading supporting database objects for Hospital Profile ( Medicare Charge ) reports" });
        }

        /// <summary>
        /// Validates the report(S) dependencies needed to generate the report.
        /// </summary>
        /// <param name="website">The website.</param>
        /// <param name="validationResults">The validation results.</param>
        /// <returns></returns>
        public override bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
        {
            base.ValidateDependencies(website, validationResults);

            if (ActiveReport != null)
            {
                if (!ActiveReport.Filters.Any(f => f.Type == Infrastructure.Entities.Domain.Reports.ReportFilterTypeEnum.Display &&
                                      f.Values.Any(fv => fv.Name.EqualsIgnoreCase("Cost and Charge Data (Medicare)") && fv.Value)))
                {
                    validationResults.Add(new ValidationResult("The Hospital Profile report is set up to not generate the Medicare Cost Charge Data. Skipping Generation."));
                }

                if (validationResults == null || validationResults.Count == 0)
                {
                    // TODO add logic for correct validation.
                    if (!website.Datasets.Any(ds => ds.Dataset.ContentType.Name.EqualsIgnoreCase("Medicare Provider Charge Data")))
                    {
                        validationResults.Add(new ValidationResult("The medicare portion of the Hospital Profile report was not generated due to no medicare provider charge data being selected with website."));
                    }
                }

            }
            else
                validationResults.Add(new ValidationResult("The Hospital Profile report was not selected. Please make sure to select the correct report and try again."));

            return validationResults == null || validationResults.Count == 0;
        }

        /// <summary>
        /// Model class for Table data
        /// </summary>
        [DataContract(Name = "")]
        class TableData
        {
            public TableData()
            {
                Rows = new List<ChargeData>();
            }


            [DataMember(Name = "TableData")]
            public IList<ChargeData> Rows { get; set; }
        }

        /// <summary>
        /// Model class for Charge data
        /// </summary>
        [DataContract(Name = "")]
        class ChargeData
        {
            //[IgnoreDataMember]
            //public string Id;
            //[IgnoreDataMember]
            //public string Hospital;
            //[IgnoreDataMember]
            //public string ProviderID;
            [DataMember(Name = "DRGID")]
            public string DRGID;
            //[IgnoreDataMember]
            //public string DRG;
            [DataMember(Name = "NumDischarges")]
            public string TotalDischarges;
            [DataMember(Name = "MeanCharges")]
            public string AverageCoveredCharges;
            [DataMember(Name = "MeanCost")]
            public string MeanCostInDollars;
            [DataMember(Name = "MeanTotalPayments")]
            public string AverageTotalPayments;
        }
        /*
             “DRGID”: 123
      "NumDischarges": 12495,
      "MeanCharges": 1679,
      “MeanCost”: “46678”
       “MeanTotalPayment”: “13495” 

        */
    }
}
