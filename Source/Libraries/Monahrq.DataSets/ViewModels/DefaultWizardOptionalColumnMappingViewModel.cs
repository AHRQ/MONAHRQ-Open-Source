using Monahrq.DataSets.Model;
using Monahrq.Infrastructure.Entities.Domain.Wings;

namespace Monahrq.DataSets.ViewModels
{
    /// <summary>
    /// the default dataset import wizard optional column mapping view model.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.ViewModels.ElementMappingViewModelBase{Monahrq.DataSets.Model.DatasetContext}" />
    public class DefaultWizardOptionalColumnMappingViewModel : ElementMappingViewModelBase<DatasetContext>
    {
        /// <summary>
        /// Gets the mappings.
        /// </summary>
        /// <value>
        /// The mappings.
        /// </value>
        protected override MappingDictionary Mappings
        {
            get { return DataContextObject.OptionalMappings; }
        }

        /// <summary>
        /// Elements the filter.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        protected override bool ElementFilter(Element element)
        {
            return !element.IsRequired;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWizardOptionalColumnMappingViewModel"/> class.
        /// </summary>
        /// <param name="c">The c.</param>
        public DefaultWizardOptionalColumnMappingViewModel(DatasetContext c)
            : base(c)
        {
        }

        /// <summary>
        /// Gets the user instructions.
        /// </summary>
        /// <value>
        /// The user instructions.
        /// </value>
        public string UserInstructions
        {
            get { return "The fields below can be mapped optionally. They are not required."; }
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public override string DisplayName
        {
            get { return "Optional Fields"; }
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return true;
        }
    }
}
