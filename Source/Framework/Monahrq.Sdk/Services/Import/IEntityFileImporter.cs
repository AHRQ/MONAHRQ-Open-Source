using System;
using Monahrq.Sdk.Events;

namespace Monahrq.Sdk.Services.Import
{
    /// <summary>
    /// The importer contract used from MEF registration.
    /// </summary>
    public static class ImporterContract
    {
        /// <summary>
        /// The regions population
        /// </summary>
        public const string RegionsPopulation = "Region Population";
        /// <summary>
        /// The custom region
        /// </summary>
        public const string CustomRegion = "Custom Region";
        /// <summary>
        /// The hospital
        /// </summary>
        public const string Hospital = "Hospital";
        /// <summary>
        /// The physician
        /// </summary>
        public const string Physician = "Physician";
    }


    /// <summary>
    /// The Entity File Importer interface.
    /// </summary>
    public interface IEntityFileImporter
    {
        /// <summary>
        /// Occurs when [importing].
        /// </summary>
        event EventHandler Importing;
        /// <summary>
        /// Occurs when [imported].
        /// </summary>
        event EventHandler Imported;
        /// <summary>
        /// Creates the please stand by event payload.
        /// </summary>
        /// <returns></returns>
        PleaseStandByEventPayload CreatePleaseStandByEventPayload();
        /// <summary>
        /// Executes this instance.
        /// </summary>
        void Execute();
    } 
}
