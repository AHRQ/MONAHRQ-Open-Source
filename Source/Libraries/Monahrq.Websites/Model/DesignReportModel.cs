using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Monahrq.Websites.Model
{
   public class DesignReportModel
    {
       public DesignReportModel()
       {
           Websites=new List<string>();
           Audiences=new List<string>();
       }
       public string Name { get; set; }
       public List<string> Websites { get; set; }
       public string Type { get; set; }
       public string Category { get; set; }
       public List<string> Audiences { get; set; }
       public string Description { get; set; }
       public string Footnotes { get; set; }
       public List<string> Filters { get; set; }
       public List<string> ReportColumns { get; set; }
       public List<string> Hospitals { get; set; } 
 

    }


    public static class ReportDataGenerator
    {
        static readonly Random Rand = new Random();

        public static List<string> Names = new List<string>();
        public static List<string> Websites = new List<string>();
        public static List<string> Types = new List<string>();
        public static List<string> Categories = new List<string>();
        public static List<string> Audiences = new List<string>();
        public static List<string> FilterEnumerations = new List<string>(); 

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

        

        public static ObservableCollection<DesignReportModel> GenerateReportCollection()
        {
            var list= Names.Select(name => new DesignReportModel
                {
                }).ToList();

            return new ObservableCollection<DesignReportModel>(list);
        }

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

        private static string GetReportCategory()
        {
            var index = Rand.Next(0, Categories.Count - 1);
            return Categories[index];
        }

        private static string GetReportType()
        {
           var index =  Rand.Next(0, Types.Count - 1);
           return Types[index];
        }
    }
}
