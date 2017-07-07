#if !SILVERLIGHT

namespace Monahrq.Theme.Converters
{
    using System.Collections.ObjectModel;
    using System.Windows.Data;
    using System.Windows.Markup;

    /// <summary>
    /// Represents a single step in a <see cref="MultiConverterGroup"/>.
    /// </summary>
    [ContentProperty("Converters")]
    public class ConverterAccumulator
    {
        private readonly Collection</*IMultiValueConverter*/object> converters;

        /// <summary>
        /// Initializes a new instance of the MultiConverterGroupStep class.
        /// </summary>
		public ConverterAccumulator()
        {
            this.converters = new Collection</*IMultiValueConverter*/object>();
        }

        /// <summary>
        /// Gets the collection of <see cref="Object"/>s in this <c>MultiConverterGroupStep</c>.
		/// We would like Object limited to either IValueConverter or IMultiValueConverter.
        /// </summary>
        public Collection</*IMultiValueConverter*/object> Converters
        {
            get { return this.converters; }
        }
    }
}

#endif