using System;
using System.Collections.Generic;
using System.Globalization;
using PropertyChanged;

namespace Monahrq.DataSets.Model
{
    /// <summary>
    /// The design time hospital model.
    /// </summary>
    [ImplementPropertyChanged]
    public class DesignHospitalModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DesignHospitalModel"/> class.
        /// </summary>
        public DesignHospitalModel()
        {
            Categories=new List<string>();
        }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public List<string> Categories { get; set; }
        /// <summary>
        /// Gets or sets the region.
        /// </summary>
        /// <value>
        /// The region.
        /// </value>
        public string Region { get; set; }
        /// <summary>
        /// Gets or sets the name of the CMS provider.
        /// </summary>
        /// <value>
        /// The name of the CMS provider.
        /// </value>
        public string CmsProviderName { get; set; }
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url { get; set; }
        /// <summary>
        /// Gets or sets the hours of operation.
        /// </summary>
        /// <value>
        /// The hours of operation.
        /// </value>
        public string HoursOfOperation { get; set; }
        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public DesignAddress Address { get; set; } 
    }

    /// <summary>
    /// The design time address entity.
    /// </summary>
    public class  DesignAddress
    {
        /// <summary>
        /// Gets or sets the county.
        /// </summary>
        /// <value>
        /// The county.
        /// </value>
        public string County { get; set; }
        /// <summary>
        /// Gets or sets the street.
        /// </summary>
        /// <value>
        /// The street.
        /// </value>
        public string Street { get; set; }
        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        public string City { get; set; }
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State { get; set; }
        /// <summary>
        /// Gets or sets the zip.
        /// </summary>
        /// <value>
        /// The zip.
        /// </value>
        public string Zip { get; set; }
    }

    /// <summary>
    /// The custom hospital data generator.
    /// </summary>
    public static class HospitalDataGenerator
    {
        /// <summary>
        /// Gets or sets the names.
        /// </summary>
        /// <value>
        /// The names.
        /// </value>
        public static List<string> Names { get; set; }
        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public static List<string> Categories { get; set; }
        /// <summary>
        /// Gets or sets the regions.
        /// </summary>
        /// <value>
        /// The regions.
        /// </value>
        public static List<string> Regions { get; set; }
        /// <summary>
        /// Gets or sets the CMS provider names.
        /// </summary>
        /// <value>
        /// The CMS provider names.
        /// </value>
        public static List<string> CmsProviderNames { get; set; }
        /// <summary>
        /// Gets or sets the urls.
        /// </summary>
        /// <value>
        /// The urls.
        /// </value>
        public static List<string> Urls { get; set; }
        /// <summary>
        /// Gets or sets the hours of operations.
        /// </summary>
        /// <value>
        /// The hours of operations.
        /// </value>
        public static List<string> HoursOfOperations { get; set; }
        /// <summary>
        /// Gets or sets the countys.
        /// </summary>
        /// <value>
        /// The countys.
        /// </value>
        public static List<string> Countys { get; set; }
        /// <summary>
        /// Gets or sets the streets.
        /// </summary>
        /// <value>
        /// The streets.
        /// </value>
        public static List<string> Streets { get; set; }
        /// <summary>
        /// Gets or sets the citys.
        /// </summary>
        /// <value>
        /// The citys.
        /// </value>
        public static List<string> Citys { get; set; }
        /// <summary>
        /// Gets or sets the states.
        /// </summary>
        /// <value>
        /// The states.
        /// </value>
        public static List<string> States { get; set; }
        /// <summary>
        /// Gets or sets the zips.
        /// </summary>
        /// <value>
        /// The zips.
        /// </value>
        public static List<string> Zips { get; set; }
        /// <summary>
        /// Gets or sets the addresses.
        /// </summary>
        /// <value>
        /// The addresses.
        /// </value>
        public static List<DesignAddress> Addresses { get; set; }

        /// <summary>
        /// Initializes the <see cref="HospitalDataGenerator"/> class.
        /// </summary>
        static HospitalDataGenerator()
        {
            Names = GenerateList("Hospital");
            Categories = GenerateCategories();
            Regions = GenerateRegions();
            CmsProviderNames = GenerateList("Provider");
            Urls = GenerateList("http://www.some.com/url?=");
            HoursOfOperations = GenerateList("Hour");
            Countys = GenerateList("County");
            Streets = GenerateList("Street");
            Citys = GenerateList("City");
            States = GenerateStates();
            Zips = GenerateZips();
            Addresses = GenerateAddresses();
        }

        /// <summary>
        /// Generates the hospitals.
        /// </summary>
        /// <returns></returns>
        public static List<DesignHospitalModel> GenerateHospitals()
        {
            var next = Rand.Next(0, Max);
            var list = new List<DesignHospitalModel>();

            for (int i = 0; i < 50; i++)
            {
                var h = new DesignHospitalModel
                {
                    Categories = GetRandomListOfCatgories(),
                    CmsProviderName = CmsProviderNames[next],
                    HoursOfOperation = HoursOfOperations[Rand.Next(0, Max - 1)],
                    Id = Rand.Next(0, 9999).ToString(CultureInfo.InvariantCulture),
                    Name = Names[Rand.Next(0, Max - 1)],
                    Region = Regions[Rand.Next(0, Regions.Count - 1)],
                    Url = Urls[Rand.Next(0, Max - 1)],
                    Address = Addresses[Rand.Next(0, Addresses.Count - 1)]
                };

                list.Add(h);
            }

            return list;
        }

        /// <summary>
        /// Gets the random list of catgories.
        /// </summary>
        /// <returns></returns>
        private static List<string> GetRandomListOfCatgories()
        {
            var count = Rand.Next(0, 10);
            var list = new List<string>();

            for (int i = 0; i < count; i++)
            {
                var next = Rand.Next(0, Categories.Count - 1);
                list.Add(Categories[next]);
            }

            return list;
        }

        /// <summary>
        /// Generates the addresses.
        /// </summary>
        /// <returns></returns>
        private static List<DesignAddress> GenerateAddresses()
        {
            var list = new List<DesignAddress>();

            for (var i = 0; i < 100; i++)
            {
                var z = Rand.Next(0, Max-1);
                var a = new DesignAddress {City = Citys[z]};
                z = Rand.Next(0, Max - 1);
                a.County = Countys[z];
                a.State = States[Rand.Next(0, States.Count - 1)];
                a.Street = Streets[Rand.Next(0, Max-1)];
                a.Zip = Zips[Rand.Next(0, Zips.Count - 1)];

            }

            return list;
        }

        /// <summary>
        /// The maximum
        /// </summary>
        private static int Max=30;

        /// <summary>
        /// The rand
        /// </summary>
        private static readonly Random Rand = new Random();

        /// <summary>
        /// Generates the regions.
        /// </summary>
        /// <returns></returns>
        public static List<string> GenerateRegions()
        {
            var regions = new List<string>();
            for (var i = 1; i < Max; i++)
            {
                regions.Add("Region "+i.ToString(CultureInfo.InvariantCulture));
            }
            
            return regions;
        }

        /// <summary>
        /// Generates the categories.
        /// </summary>
        /// <returns></returns>
        public static List<string> GenerateCategories()
        {
             var cats = new List<string>();
             for (var i = 1; i < Max; i++)
            {
                cats.Add("Category "+i.ToString(CultureInfo.InvariantCulture));
            }
            
            return cats;
        }

        /// <summary>
        /// Generates the states.
        /// </summary>
        /// <returns></returns>
        public static List<string> GenerateStates()
        {
            var states = new List<string> {"AL", "FL", "MD", "DC", "VA", "CA", "AZ"};

            return states;
        }

        /// <summary>
        /// Generates the list.
        /// </summary>
        /// <param name="basew">The basew.</param>
        /// <returns></returns>
        public static List<string> GenerateList(String basew)
        {
            
            var list = new List<string>();
            for (var i = 1; i < Max; i++)
            {
                list.Add(basew+i.ToString(CultureInfo.InvariantCulture));
            }
            
            return list;
       }

        /// <summary>
        /// Generates the zips.
        /// </summary>
        /// <returns></returns>
        public static List<string> GenerateZips()
        {
            var zips = new List<string>();

            for (var i = 0; i < Max; i++)
            {
                var zip = string.Empty;
                for (var j = 0; j < 5; j++)
                {
                    var next = Rand.Next(0, 9);
                    zip += next.ToString(CultureInfo.InvariantCulture);
                }
                
                zips.Add(zip);
            }

            return zips;
        }
    }
}
