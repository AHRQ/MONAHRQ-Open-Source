using Monahrq.DataSets.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Monahrq.DataSets.ViewModels
{
    /// <summary>
    /// The default dataset import wizard column mapping viewmodel.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.ViewModels.ElementMappingViewModelBase{Monahrq.DataSets.Model.DatasetContext}" />
    public class DefaultWizardColumnMappingViewModel : ElementMappingViewModelBase<DatasetContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultWizardColumnMappingViewModel"/> class.
        /// </summary>
        /// <param name="c">The c.</param>
        public DefaultWizardColumnMappingViewModel(DatasetContext c)
            : base(c)
        {
            var type = Type.GetType(DataContextObject.SelectedDataType.Target.ClrType);
            PropertyLookup = type.GetProperties().ToDictionary(p => p.Name.ToLower());
        }

        /// <summary>
        /// Gets or sets the property lookup.
        /// </summary>
        /// <value>
        /// The property lookup.
        /// </value>
        private IDictionary<string, PropertyInfo> PropertyLookup
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the mappings.
        /// </summary>
        /// <value>
        /// The mappings.
        /// </value>
        protected override MappingDictionary Mappings
        {
            get { return DataContextObject.RequiredMappings; }
        }

        /// <summary>
        /// Elements the filter.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        protected override bool ElementFilter(Infrastructure.Entities.Domain.Wings.Element element)
        {
            return true;
        }

        /// <summary>
        /// Gets the user instructions.
        /// </summary>
        /// <value>
        /// The user instructions.
        /// </value>
        public string UserInstructions
        {
            get { return "Please map all " + DataContextObject.SelectedDataType.Target.Name + " fields."; }
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public override string DisplayName
        {
            get { return DataContextObject.SelectedDataType.Target.Name + " Fields"; }
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return DataTypeRequiredFieldCount == MappedFieldsCount;
        }
    }
}
