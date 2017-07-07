using PropertyChanged;

namespace Monahrq.Measures.ViewModels
{
    /// <summary>
    /// Class for alternate name of measures
    /// </summary>
    [ImplementPropertyChanged]
    public class AlternateName
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlternateName"/> class.
        /// </summary>
        public AlternateName()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlternateName"/> class and initializes the Name and IsSelected property
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="isselected">if set to <c>true</c> [isselected].</param>
        public AlternateName(string name, bool isselected)
        {
            Name = name;
            IsSelected = isselected;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
       

    }
}
