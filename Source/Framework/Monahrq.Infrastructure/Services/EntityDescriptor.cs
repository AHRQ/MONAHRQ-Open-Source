using Monahrq.Infrastructure.Entities.Domain;

namespace Monahrq.Infrastructure.Services
{
    /// <summary>
    /// The Monahrq entity descriptor.
    /// </summary>
    public class EntityDescriptor
    {
        /// <summary>
        /// The type
        /// </summary>
        public string Type;
        /// <summary>
        /// The name
        /// </summary>
        public string Name;
        /// <summary>
        /// The identifier
        /// </summary>
        public string Id;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDescriptor"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="id">The identifier.</param>
        public EntityDescriptor(string type, string id)
        {
            Type = type;
            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDescriptor"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public EntityDescriptor(string type)
        {
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDescriptor"/> class.
        /// </summary>
        /// <param name="en">The en.</param>
        public EntityDescriptor(IEntity en)
        {
            var namedEntity = en as Entity<object>;
            Type = en.GetType().Name;
            Name = en.Name;
            Id = namedEntity != null ? namedEntity.Id.ToString() : string.Empty;
        }
    }
}