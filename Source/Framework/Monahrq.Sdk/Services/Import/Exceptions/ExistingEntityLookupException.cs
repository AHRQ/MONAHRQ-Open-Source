using System;
using System.ComponentModel.Composition;
using System.Runtime.Serialization;

namespace Monahrq.Sdk.Services.Import.Exceptions
{
    /// <summary>
    /// The custom exception that designates the entity that is being imported can't be found or resolved. 
    /// This exception is used in the <see cref="HospitalImporter"./>
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class ExistingEntityLookupException : Exception
    {
        /// <summary>
        /// The error MSG FRMT string
        /// </summary>
        const string ErrMsgFrmtStr = "Check on existing {0}, Id {1}, failed: {2}";

        /// <summary>
        /// Initializes a new instance of the <see cref="ExistingEntityLookupException"/> class.
        /// </summary>
        /// <param name="ExportAttribute">The export attribute.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="ex">The ex.</param>
        public ExistingEntityLookupException(ExportAttribute ExportAttribute, object id, Exception ex)
            :base(string.Format(ErrMsgFrmtStr, ExportAttribute.ContractName.ToLower(), id, ex.Message), ex)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ExistingEntityLookupException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected ExistingEntityLookupException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}
