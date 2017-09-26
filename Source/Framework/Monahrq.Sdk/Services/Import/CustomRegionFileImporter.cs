using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Monahrq.Infrastructure.Domain.Regions;
using Monahrq.Infrastructure.Entities.Domain;
using Monahrq.Infrastructure.Entities.Domain.BaseData;
using Monahrq.Infrastructure.FileSystem;
using Monahrq.Infrastructure.Services.Hospitals;
using Monahrq.Sdk.Events;
using LinqKit;
using Monahrq.Infrastructure;

namespace Monahrq.Sdk.Services.Import
{
    /// <summary>
    /// The custom region file importer class.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Services.Import.EntityFileImporter" />
    [Export(ImporterContract.CustomRegion, typeof(IEntityFileImporter))]
    public class CustomRegionFileImporter : EntityFileImporter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRegionFileImporter"/> class.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="hospitalRegistryService">The hospital registry service.</param>
        /// <param name="events">The events.</param>
        /// <param name="logger">The logger.</param>
        [ImportingConstructor]
        public CustomRegionFileImporter(IUserFolder folder 
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
            get { return 3; }
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
                return "Importing this file may overwrite existing geographic regions and/or remove " +
                    "the associations with the hospitals, if any.\n\n Do you want to continue?";
            }
        }

        /// <summary>
        /// Processes the region.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <param name="errors">The errors.</param>
        private void ProcessRegion(CustomRegion region, IList<ImportError> errors)
        {
            var criteria = PredicateBuilder.False<CustomRegion>().Or(r => r.ImportRegionId == region.ImportRegionId);
            var stateCriteria = PredicateBuilder.False<CustomRegion>();
            CurrentStatesBeingManaged.ToList().ForEach(abbr => stateCriteria = stateCriteria.Or(r => region.State.ToUpper() == abbr.ToUpper())); 
            criteria = criteria.And(stateCriteria);

            var reg = GetExistingRegion(criteria);

            if (reg != null)
            {
                // if the found region is custom type, overwrite it in the database with the attributes being imported
                region.Code = region.Code;
                reg.Name = region.Name;
                reg.State = region.State;
                reg.ImportRegionId = region.ImportRegionId;
            }
            else
            {
                reg = region;
            }

            if (CurrentStatesBeingManaged.All(s => s.ToLower() != reg.State.ToLower()))
            {
                var errorMessage =
                    string.Format(
                        "The region \"{0}\" can't be added because it is not in the correct geo context state. CMS NUMBER: {0}",
                        reg.Name);

                errors.Add(ImportError.Create("CustomRegion", reg.Name, errorMessage));
                return;
            }

            if (errors.Any()) return;

            HospitalRegistryService.Save(reg);

            Events.GetEvent<EntityImportedEvent<CustomRegion>>().Publish(reg);
        }

        /// <summary>
        /// Processes the values.
        /// </summary>
        /// <param name="vals">The vals.</param>
        /// <param name="errors">The errors.</param>
        protected override void ProcessValues(string[] vals, IList<ImportError> errors)
        {
            vals.ToList().ForEach(TestForEmpty);

            dynamic dynRegion = new { RegionID = int.Parse(vals[0].Trim()), RegionName = vals[1].Trim(), State = vals[2].Trim() };

            State state = GetState(dynRegion.State.ToString());
            var customRegion = HospitalRegistryService.CreateRegion();
            customRegion.Name = dynRegion.RegionName;
            customRegion.State = state.Abbreviation;
            customRegion.ImportRegionId = dynRegion.RegionID;
            customRegion.Code = string.Format("CUS{0}{1}", dynRegion.RegionID, state.Abbreviation);

            ProcessRegion(customRegion, errors);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="vals">The vals.</param>
        /// <returns></returns>
        protected override string GetName(string[] vals)
        {
            return vals[1].Trim();
        }
    }
}
