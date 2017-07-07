using Monahrq.Infrastructure.Generators;

namespace Monahrq.DataSets.Model
{
    /// <summary>
    /// The hospital regions populatiion model used in exporting the regions population for later re-import into Monahrq.
    /// </summary>
    public class RegionsPopulationModel
    {
        /// <summary>
        /// Gets or sets the region identifier.
        /// </summary>
        /// <value>
        /// The region identifier.
        /// </value>
        [CSVMap(Ordinal = 1)]
        public int? RegionId { get; set; }

        /// <summary>
        /// Gets or sets the sex.
        /// </summary>
        /// <value>
        /// The sex.
        /// </value>
        [CSVMap(Ordinal = 4)]
        public int Sex { get; set; }

        /// <summary>
        /// Gets or sets the age group.
        /// </summary>
        /// <value>
        /// The age group.
        /// </value>
        [CSVMap(Ordinal = 5)]
        public int AgeGroup { get; set; }

        /// <summary>
        /// Gets or sets the race.
        /// </summary>
        /// <value>
        /// The race.
        /// </value>
        [CSVMap(Ordinal = 6)]
        public int Race { get; set; }

        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        /// <value>
        /// The year.
        /// </value>
        [CSVMap(Ordinal = 7)]
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the population.
        /// </summary>
        /// <value>
        /// The population.
        /// </value>
        [CSVMap(Ordinal = 8)]
        public int Population { get; set; }
    }
}
