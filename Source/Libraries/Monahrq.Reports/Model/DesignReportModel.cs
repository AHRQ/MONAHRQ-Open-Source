using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Monahrq.Reports.Model
{

    public class DesignReportModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DesignReportModel"/> class.
        /// </summary>
        public DesignReportModel()
        {
            Websites = new List<string>();
            Audiences = new List<string>();
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the websites in the report.
        /// </summary>
        /// <value>
        /// The websites.
        /// </value>
        public List<string> Websites { get; set; }

        /// <summary>
        /// Gets or sets the type of the report.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the category of the report.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the audiences.
        /// </summary>
        /// <value>
        /// The audiences.
        /// </value>
        public List<string> Audiences { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the footnotes.
        /// </summary>
        /// <value>
        /// The footnotes.
        /// </value>
        public string Footnotes { get; set; }
        /// <summary>
        /// Gets or sets the filters for the report.
        /// </summary>
        /// <value>
        /// The filters.
        /// </value>
        public List<string> Filters { get; set; }
        /// <summary>
        /// Gets or sets the report columns.
        /// </summary>
        /// <value>
        /// The report columns.
        /// </value>
        public List<string> ReportColumns { get; set; }
        /// <summary>
        /// Gets or sets the hospitals for the report.
        /// </summary>
        /// <value>
        /// The hospitals.
        /// </value>
        public List<string> Hospitals { get; set; }


    }


    /// <summary>
    /// Demo report data generation class.
    /// </summary>
    public static class ReportDataGenerator
    {
        static readonly Random Rand = new Random();

        public static List<string> Names = new List<string>();
        public static List<string> Websites = new List<string>();
        public static List<string> Types = new List<string>();
        public static List<string> Categories = new List<string>();
        public static List<string> Audiences = new List<string>();
        public static List<string> FilterEnumerations = new List<string>();

        /// <summary>
        /// Initializes the <see cref="ReportDataGenerator"/> class.
        /// </summary>
        static ReportDataGenerator()
        {
            Names.Add("Hospitals Quality raqting - Details Tabular View");
            Names.Add("Hospitals Comparison Report");
            Names.Add("Hospital Profile Report");
            Names.Add("IP Utilization Detail Report");
            Names.Add("Emergency Department Treat and Release");
            Websites.Add("My Website");
            Websites.Add("Second Website");
            Websites.Add("Third Website");
            Websites.Add("Another Website");
            Websites.Add("Long Name Some Kind of Complicated Website");
            Types.Add("Hospitals Quality Rating");
            Types.Add("Hospital Comparison Report");
            Types.Add("IP Utilization Details");
            Types.Add("Emergency Report"); 
            Types.Add("Hospital Profile");
            Categories.Add("Quality");
            Categories.Add("Other");
            Categories.Add("Comparison");
            Categories.Add("Public Health Data");
            Audiences.Add("Provider");
            Audiences.Add("Researcher");
            Audiences.Add("Consumer");
            FilterEnumerations.Add("Report name");
            FilterEnumerations.Add("Report type");
            FilterEnumerations.Add("Website");
            FilterEnumerations.Add("Audience");
        }



        /// <summary>
        /// Generates the report collection.
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<DesignReportModel> GenerateReportCollection()
        {
            var list= Names.Select(name => new DesignReportModel
                {
                }).ToList();

            return new ObservableCollection<DesignReportModel>(list);
        }

        /// <summary>
        /// Gets the audiences.
        /// </summary>
        /// <returns></returns>
        private static List<string> GetAudiences()
        {
            var list = new List<string>();
            var count = Rand.Next(1, 3);
            for (var i = 0; i < count; i++)
            {
                var index = Rand.Next(0, Audiences.Count - 1);
                list.Add(Audiences[index]);
            }
            return list;
        }

        /// <summary>
        /// Gets the websites.
        /// </summary>
        /// <returns></returns>
        private static List<string> GetWebsites()
        {
            var list = new List<string>();
            var count = Rand.Next(0, 5);
            for (var i = 0; i < count; i++)
            {
                var index = Rand.Next(0, Websites.Count - 1);
                list.Add(Websites[index]);
            }
            return list;
        }

        /// <summary>
        /// Gets the report category.
        /// </summary>
        /// <returns></returns>
        private static string GetReportCategory()
        {
            var index = Rand.Next(0, Categories.Count - 1);
            return Categories[index];
        }

        /// <summary>
        /// Gets the type of the report.
        /// </summary>
        /// <returns></returns>
        private static string GetReportType()
        {
           var index =  Rand.Next(0, Types.Count - 1);
           return Types[index];
        }
    }
}
