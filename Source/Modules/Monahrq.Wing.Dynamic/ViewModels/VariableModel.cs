using Monahrq.DataSets.Model;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.Wing.Dynamic.ViewModels
{
    /// <summary>
    /// Variable model class
    /// </summary>
    class VariableModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableModel"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="source">The source.</param>
        public VariableModel(Element element, FieldEntry source)
        {
            this.Element = element;
            this.Source = source;
        }

        /// <summary>
        /// Gets or sets the element.
        /// </summary>
        /// <value>
        /// The element.
        /// </value>
        public Element Element { get; set; }
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public FieldEntry Source {get;set;}
        /// <summary>
        /// Gets the ordinal.
        /// </summary>
        /// <value>
        /// The ordinal.
        /// </value>
        public int? Ordinal
        {
            get
            {
                return this.Element == null || this.Source == null ? null : (int?)this.Source.Column.Ordinal;
            }
        }

    }
}