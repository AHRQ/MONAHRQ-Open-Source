using System;
using System.Runtime.Serialization;

namespace Monahrq.Infrastructure.Exceptions
{
    [Serializable]
    public class MonahrqCoreException : ApplicationException {


        public MonahrqCoreException() : base() { }

        public MonahrqCoreException(string message)
            : base(message) {
        }

        public MonahrqCoreException(string message, Exception innerException)
            : base(message, innerException) {

        }

        protected MonahrqCoreException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }

       
    }
}