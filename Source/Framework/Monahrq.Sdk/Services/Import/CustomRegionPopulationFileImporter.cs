using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.FileSystem;
using Monahrq.Infrastructure.Services.Hospitals;
using Monahrq.Infrastructure.Utility;

namespace Monahrq.Sdk.Services.Import
{
    /// <summary>
    /// The custom region population file importer cclass.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Services.Import.EntityFileImporter" />
    [Export(ImporterContract.RegionsPopulation, typeof(IEntityFileImporter))]
    public class CustomRegionPopulationFileImporter : EntityFileImporter
    {
        /// <summary>
        /// The previous values
        /// </summary>
        private readonly IList<Tuple<int, int, int, int>> _previousValues = new List<Tuple<int, int, int, int>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRegionPopulationFileImporter"/> class.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="hospitalRegistryService">The hospital registry service.</param>
        /// <param name="events">The events.</param>
        /// <param name="logger">The logger.</param>
        [ImportingConstructor]
        public CustomRegionPopulationFileImporter(IUserFolder folder
            , IDomainSessionFactoryProvider provider
            , IHospitalRegistryService hospitalRegistryService
            , IEventAggregator events
            , [Import(LogNames.Session)] ILogWriter logger)
            : base(folder, provider, hospitalRegistryService, events, logger) { }

        /// <summary>
        /// Gets the expected count of values per line.
        /// </summary>
        /// <value>
        /// The expected count of values per line.
        /// </value>
        protected override int ExpectedCountOfValuesPerLine
        {
            get { return 6; }
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
                return "Importing this file may overwrite existing population data.\n\n Do you want to continue?";
            }
        }

        /// <summary>
        /// Processes the region population strats.
        /// </summary>
        /// <param name="regionPopulationStrats">The region population strats.</param>
        /// <param name="errors">The errors.</param>
        private void ProcessRegionPopulationStrats(RegionPopulationStrats regionPopulationStrats, IList<ImportError> errors)
        {
            try
            {
                if (errors.Any()) return;

                HospitalRegistryService.Save(regionPopulationStrats);
            }
            catch (Exception exc)
            {
                errors.Add(ImportError.Create(typeof(RegionPopulationStrats).Name, regionPopulationStrats.RegionID.ToString(), (exc.InnerException ?? exc).Message));
            }
        }

        /// <summary>
        /// Begins the import.
        /// </summary>
        protected override void BeginImport()
        {
            base.BeginImport();

            using (var session = Provider.SessionFactory.OpenStatelessSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.CreateSQLQuery("Delete from [dbo].[" + typeof(RegionPopulationStrats).EntityTableName() + "] where [RegionType]=0").ExecuteUpdate();

                    tx.Commit();
                }
            }
        }

        /// <summary>
        /// Processes the values.
        /// </summary>
        /// <param name="vals">The vals.</param>
        /// <param name="errors">The errors.</param>
        protected override void ProcessValues(string[] vals, IList<ImportError> errors)
        {
            vals.ToList().ForEach(TestForEmpty);
            int sex = int.Parse(vals[1].Trim());
            int ageGroup = int.Parse(vals[2].Trim());
            int race = int.Parse(vals[3].Trim());
            int catid=0;
            int catval=0;

            if (sex == 0 && ageGroup == 0 && race == 0)
            {
                catid = 0;
                catval = 0;
            }
            else if (sex == 0 && ageGroup > 0 && race == 0)
            {
                catid = 1;
                catval = ageGroup;
            }
            else if (sex > 0 && ageGroup == 0 && race == 0)
            {
                catid = 2;
                catval = sex;
            }
            else if (sex == 0 && ageGroup == 0 && race > 0)
            {
                catid = 4;
                catval = race;
            }
            else
            {
                errors.Add(new ImportError() { EntityName = "RegionPopulationStrats", ErrorMessage = string.Format("unsupported combination Sex : {0} AgeGroup : {1} Race : {2}", sex, ageGroup, race) });
            }

            if (errors.Any())
                return;

            var regionPopulationStrats = new RegionPopulationStrats
                {
                    RegionType = 0,
                    RegionID = int.Parse(vals[0].Trim()),
                    CatID=catid,
                    CatVal=catval,                  
                    Year = int.Parse(vals[4].Trim()),
                    Population = int.Parse(vals[5].Trim())
                };


            if (_previousValues.Any(pop => pop.Item1 == regionPopulationStrats.Year && pop.Item2 == regionPopulationStrats.RegionID && pop.Item3 == regionPopulationStrats.CatID && pop.Item4 == regionPopulationStrats.CatVal))
                errors.Add(new ImportError { EntityName = "RegionPopulationStrats", ErrorMessage = string.Format("duplicate record for a given year ({0}) and Region ID ({1}): CatID : {2} CatVal : {3}", regionPopulationStrats.Year, regionPopulationStrats.RegionID, regionPopulationStrats.CatID, regionPopulationStrats.CatVal) });

            ProcessRegionPopulationStrats(regionPopulationStrats, errors);

            if (errors == null || !errors.Any())
            {
                _previousValues.Add(new Tuple<int, int, int, int>(regionPopulationStrats.Year, regionPopulationStrats.RegionID, regionPopulationStrats.CatID, regionPopulationStrats.CatVal));
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="vals">The vals.</param>
        /// <returns></returns>
        protected override string GetName(string[] vals)
        {
            return string.Join("|", vals);
        }
    }
}
