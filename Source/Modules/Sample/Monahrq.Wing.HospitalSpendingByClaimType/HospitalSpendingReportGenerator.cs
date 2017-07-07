using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Sdk.Generators;

namespace Monahrq.Wing.HospitalSpendingByClaimType
{
    [Export(typeof(IReportGenerator))]
    [ReportGenerator(
        // must match GUID from report XML definition
        new[] { "{E5C24EC4-6583-41D8-87A1-9F45B143819B}" }, 
        new string[] { },
        new[] {  typeof(HospitalSpendingByClaimTypeTarget) },
        0,
        "Hospital Spending Sample Report")]
    public class HospitalSpendingReportGenerator : BaseReportGenerator
    {
        // if we wanted to access entities here, we could do that
        //[Import(RequiredCreationPolicy = CreationPolicy.NonShared)]
        //protected IDomainSessionFactoryProvider SessionFactoryProvider { get; private set; }

        /// <inheritdoc/>
        public override void InitGenerator()
        {
            base.EventAggregator.GetEvent<MessageUpdateEvent>().Publish(new MessageUpdateEvent { Message = "Doing nothing at all in relation to the Hospital Spending Sample Report" });
        }

        /// <inheritdoc/>
        public override bool ValidateDependencies(Website website, IList<ValidationResult> validationResults)
        {
            if (!base.ValidateDependencies(website, validationResults))
                return false;

            if (!website.Datasets.Any(d => HospitalSpendingByClaimTypeTarget.Guid.Equals(d.Dataset.ContentType.Guid)))
            {
                validationResults.Add(new ValidationResult("Hospital spending sample report cannot be generated because no 'Hospital Spending by Claim Type' dataset has been loaded"));
                return false;
            }

            return true;
        }

        private JsonReportItem[] data;

        /// <inheritdoc/>
        protected override bool LoadReportData()
        {
            // verify that we have a dataset to work with
            var dataset = base.CurrentWebsite.Datasets.FirstOrDefault(
                d => HospitalSpendingByClaimTypeTarget.Guid.Equals(d.Dataset.ContentType.Guid));
            if (dataset == null)
            {
                base.Logger.Warning("Hospital spending sample report cannot be generated because no 'Hospital Spending by Claim Type' dataset has been loaded");
                return false;
            }

            // get all data from the dataset
            var rawData = base.RunSqlReturnDataTable(
                "SELECT * FROM Targets_HospitalSpendingByClaimTypeTargets WHERE dataset_id = @id",
                new KeyValuePair<string, object>("@id", dataset.Dataset.Id));
            this.data = rawData
                    .AsEnumerable()
                    .Select(
                        x => new JsonReportItem
                        {
                            CmsProviderId = x["CmsProviderId"].ToString(),
                            ClaimType = (ClaimType)x["ClaimType"],
                            Period = (Period)x["Period"],
                            AverageSpendingPerEpisodeHospital = (double)x["AverageSpendingPerEpisodeHospital"],
                            AverageSpendingPerEpisodeNational = (double)x["AverageSpendingPerEpisodeNational"],
                            AverageSpendingPerEpisodeState = (double)x["AverageSpendingPerEpisodeState"],
                            PercentSpendingPerEpisodeHospital = (double)x["PercentSpendingPerEpisodeHospital"],
                            PercentSpendingPerEpisodeNational = (double)x["PercentSpendingPerEpisodeNational"],
                            PercentSpendingPerEpisodeState = (double)x["PercentSpendingPerEpisodeState"]
                        })
                    .ToArray();
            return true;
        }

        /// <inheritdoc/>
        protected override bool OutputDataFiles()
        {
            // spit out data to arbitrary file
            var relativePath = Path.Combine("Data", "Base", "HospitalSpendingReportSample.js");
            base.GenerateJsonFile(this.data, Path.Combine(CurrentWebsite.OutPutDirectory, relativePath));
            this.data = null;
            return true;
        }


        class JsonReportItem
        {
            public string CmsProviderId { get; set; }
            public Period Period { get; set; }
            public ClaimType ClaimType { get; set; }
            public double AverageSpendingPerEpisodeHospital { get; set; }
            public double AverageSpendingPerEpisodeState { get; set; }
            public double AverageSpendingPerEpisodeNational { get; set; }
            public double PercentSpendingPerEpisodeHospital { get; set; }
            public double PercentSpendingPerEpisodeState { get; set; }
            public double PercentSpendingPerEpisodeNational { get; set; }
        }
    }
}
