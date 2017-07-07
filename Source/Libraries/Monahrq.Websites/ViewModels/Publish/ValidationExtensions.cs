using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LinqKit;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Domain.Websites;
using Monahrq.Infrastructure.Entities.Domain.Hospitals;

using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Configuration;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Websites.ViewModels.Publish
{
    public static class ValidationExtensions
    {
        public static bool RequiresCmsId(this WebsiteReport report)
        {
            return report.Report.RequiresCmsProviderId;
        }

        public static bool RequiresCostToCharge(this WebsiteReport report)
        {
            return report.Report.RequiresCostToChargeRatio;
        }

        public static bool HospitalMissingCostToCharge(this WebsiteViewModel viewmodel)
        {
            //var crit = PredicateBuilder.False<WebsiteHospital>();
            //crit = crit.Or(h => h.CCR == null);
            var missingCostToCharge = viewmodel.Website.Hospitals.Count(wh => string.IsNullOrEmpty(wh.CCR) || wh.CCR == "0") != 0;
            //var missingCostToCharge = viewmodel.WebsiteDataService.GetHospitalsCountForValidation(crit) != 0;
            return missingCostToCharge;
        }

        public static bool IsMissingCmsProviderIds(this WebsiteViewModel viewmodel)
        {
            // var crit = PredicateBuilder.False<Hospital>();
            // crit = crit.Or(h => h.CmsProviderID==null || h.CmsProviderID=="" );
            return viewmodel.Website.Hospitals.Count(wh => string.IsNullOrEmpty(wh.Hospital.CmsProviderID)) != 0;
        }

        public static bool HasRegions(this WebsiteViewModel viewmodel)
        {
            var configService = ServiceLocator.Current.GetInstance<IConfigurationService>();
            var crit = PredicateBuilder.False<Hospital>();

            var selectedRegionType = configService.HospitalRegion.SelectedRegionType;
            if (selectedRegionType == typeof(CustomRegion))
                crit = crit.Or(h => h.CustomRegion == null);
            else if (selectedRegionType == typeof(HealthReferralRegion))
                crit = crit.Or(h => h.CustomRegion == null).Or(h => h.HealthReferralRegion == null);
            else if (selectedRegionType == typeof(HospitalServiceArea))
                crit = crit.Or(h => h.CustomRegion == null).Or(h => h.HospitalServiceArea == null);

            var missingRegion = viewmodel.WebsiteDataService.GetHospitalsCountForValidation(crit, viewmodel.Website.StateContext) != 0;
            return missingRegion;

        }

        public static bool HasState(this WebsiteViewModel viewmodel)
        {
            return (viewmodel != null &&
                    viewmodel.Website != null &&
                    viewmodel.Website.SelectedReportingStates.Any());
        }

        public static bool HasCustomRegionSelected(this WebsiteViewModel viewmodel)
        {
            return viewmodel.Website.RegionTypeContext.EqualsIgnoreCase("CustomRegion");
        }

    }
}
