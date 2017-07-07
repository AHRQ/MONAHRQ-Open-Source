namespace Monahrq.Infrastructure.Entities.Core
{
    /// <summary>
    /// The Reader Definition class. 
    /// </summary>
    public class ReaderDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderDefinition"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ReaderDefinition(string name): this(name,name) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderDefinition"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        public ReaderDefinition(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; private set; }
    }
}
