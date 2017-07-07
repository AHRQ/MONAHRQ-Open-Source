using System;
using System.Runtime.Serialization;

namespace Monahrq.Sdk.Services.Import.Exceptions
{
    /// <summary>
    /// Custom region to designate when a region can not be resoved/found. Use specifically for the <see cref="HospitalImporter"/>.
    /// </summary>
    /// <seealso cref="Monahrq.Sdk.Services.Import.Exceptions.EntityFileImportException" />
    [Serializable]
    public class ImportHospitalRegionNotFoundException:EntityFileImportException 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportHospitalRegionNotFoundException"/> class.
        /// </summary>
        /// <param name="regionId">The region identifier.</param>
        public ImportHospitalRegionNotFoundException(int regionId)
            :base(string.Format("Specified Custom Region [ {0} ] not defined in system.", regionId))
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportHospitalRegionNotFoundException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected ImportHospitalRegionNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}