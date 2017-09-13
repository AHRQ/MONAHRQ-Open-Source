using System;

namespace Monahrq.Infrastructure.Services
{
    /// <summary>
    /// The custom Service Error Event Arguments.
    /// </summary>
    public class ServiceErrorEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceErrorEventArgs"/> class.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        public ServiceErrorEventArgs(Exception e, string type, string name)
        {
            EnitytType = type;
            EntityName = name;
            Exception = e;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceErrorEventArgs"/> class.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="entity">The entity.</param>
        public ServiceErrorEventArgs(Exception e, EntityDescriptor entity)
        {
            EnitytType = entity.Type;
            EntityName = entity.Name;
            EntityId = entity.Id;
            Exception = e;
        }

        /// <summary>
        /// The error messsage
        /// </summary>
        public Exception Exception { get; set; }
        /// <summary>
        /// The enityt type
        /// </summary>
        public string EnitytType { get; set; }
        /// <summary>
        /// The entity name
        /// </summary>
        public string EntityName { get; set; }
        /// <summary>
        /// The entity identifier
        /// </summary>
        public string EntityId { get; set; }
    }
}