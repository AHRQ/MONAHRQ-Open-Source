using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monahrq.Infrastructure.Entities.Domain.Reports;
using Monahrq.Infrastructure.Entities.Domain.Reports.Attributes;
using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility.Extensions;

namespace Monahrq.Infrastructure.Test.Domain.Reports.Filters
{
	using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
	using System.ComponentModel;

    [TestClass]
    public class ReportFilterTest
    {
        //[TestMethod]
        //public void ExtractGroupPositive()
        //{
        //    var groups = ReportFilterExtensions.FilterGroups.ToList();
        //    Assert.AreEqual(3, groups.Count);
        //}
 
 

        [TestMethod]
        public void ExtractDescriptionPositive()
        {
            var values = Enum.GetValues(typeof(ReportFilter)).OfType<ReportFilter>();
            ReportFilter filters = default(ReportFilter);
            values.ToList().ForEach(filter => filters |= filter);
            var descriptions = filters.GetAttributeValues<DescriptionAttribute, string>(x => x.Description, x => x.ToString());
            var dict = descriptions.ToDictionary(tuple => tuple.Item1, value => value.Item2);
            Assert.AreEqual(values.Count(), descriptions.Count());
            Assert.AreEqual(values.Count(), dict.Count());
            Assert.AreEqual(dict[ReportFilter.HospitalFilters], Constants.HOSPITAL_FILTER_DESCRIPTION);
            Assert.AreEqual(dict[ReportFilter.HospitalName], Constants.HOSPITAL_NAME_DESCRIPTION);
            Assert.AreEqual(dict[ReportFilter.Category], Constants.CATEGORY_DESCRIPTION);
          //  Assert.AreEqual(dict[ReportFilter.ZipCode], Constants.ZipCodeDescription);
            Assert.AreEqual(dict[ReportFilter.Region], Constants.REGION_DESCRIPTION);
            //Assert.AreEqual(dict[ReportFilter.AllHospitals], Constants.AllHospitalsDescription);
            Assert.AreEqual(dict[ReportFilter.DRGsDischargesFilters], Constants.DR_GS_DISCHARGES_FILTER_DESCRIPTION);
            Assert.AreEqual(dict[ReportFilter.MDC], Constants.MDC_DESCRIPTION);
            Assert.AreEqual(dict[ReportFilter.DRG], Constants.DRG_DESCRIPTION);
            Assert.AreEqual(dict[ReportFilter.DRGCondition], Constants.DRG_CONDITION_DESCRIPTION);
            Assert.AreEqual(dict[ReportFilter.Procedure], Constants.PROCEDURE_DESCRIPTION);
            //Assert.AreEqual(dict[ReportFilter.AllDischargesCombined], Constants.AllDischargesCombinedDescription);
            Assert.AreEqual(dict[ReportFilter.ConditionsAndDiagnosisFilters], Constants.CONDITIONS_AND_DIAGNOSIS_FILTER_DESCRIPTION);
            Assert.AreEqual(dict[ReportFilter.Conditions], Constants.CONDITIONS_DESCRIPTION);
            //Assert.AreEqual(dict[ReportFilter.AllDiagnosesCombined], Constants.AllDiagnosesCombinedDescription);
        }

        [TestMethod]
        public void FilterSetCollectionValueInitialCtorPositive()
        {
            var collection = new FilterSetCollection();
            var actual = collection.Value;
            var expected = default(ReportFilter);
            Enum.GetValues(typeof(ReportFilter))
                .OfType<ReportFilter>()
                .ToList()
                .ForEach(filter => expected |= filter);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestFilterItemCaption()
        {
            var item = FilterSet.Create(ReportFilter.DRG, ()=>{});
            Assert.AreEqual(Constants.DRG_DESCRIPTION, item.Caption);
        }

    }
}
