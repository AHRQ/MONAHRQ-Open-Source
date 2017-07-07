using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Monahrq.DataSets.Model;
using Monahrq.DataSets.ViewModels;
using Monahrq.Infrastructure.Entities.Domain.Wings;
using Monahrq.Infrastructure.Validation;
using Monahrq.Wing.Dynamic.Models;

namespace Monahrq.Wing.Dynamic.ViewModels
{
    /// <summary>
    /// View model Class for full wizard column mapping
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.ViewModels.ElementMappingViewModelBase{Monahrq.Wing.Dynamic.Models.WizardContext}" />
    public class FullWizardColumnMappingViewModel : ElementMappingViewModelBase<WizardContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FullWizardColumnMappingViewModel"/> class.
        /// </summary>
        /// <param name="c">The c.</param>
        public FullWizardColumnMappingViewModel(WizardContext c)
            : base(c)
        {

            //var type = Type.GetType(DataContextObject.SelectedDataType.Target.ClrType);


            //PropertyLookup = c.TargetType.GetProperties().ToDictionary(p => p.Name.ToLower());
            PropertyLookup = c.TargetProperties.ToDictionary(p => p.Key.ToLower(), p => p.Value);
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
        /// Gets the mappings from data context object.
        /// </summary>
        /// <value>
        /// The mappings.
        /// </value>
        protected override MappingDictionary Mappings
        {
            get { return DataContextObject.RequiredMappings; }
        }

        /// <summary>
        /// Element filter implementation.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        protected override bool ElementFilter(Element element)
        {
            return true; // element.IsRequired && !SuppressRequired(element);
        }

        //private bool SuppressRequired(Element element)
        //{
        //    var prop = PropertyLookup[element.Name.ToLower()];
        //    var attr = prop.GetCustomAttribute<OptionalRequiredAttribute>();
        //    if (attr == null) return false;
        //    return DataContextObject.SuppressedConstraints.Contains(attr.ConstraintGuid);
        //}

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

        //public override bool IsValid()
        //{
        //    return MappedFieldsCount == TargetFields.OfType<MTargetField>().Count(f => f.);
        //}

        /// <summary>
        /// Returns true if wizard column mapping is valid.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsValid()
        {
            //int sourceMappedRequiredFieldCount = SourceFields.OfType<MOriginalField>().Count(f => f.TargetField != null && f.TargetField.IsRequired);
            //int requireMonahrqFieldCount = TargetFields.OfType<MTargetField>().Count(f => f.IsRequired);
            //return MappedFieldsCount == TargetFields.OfType<MTargetField>().Count(f => f.IsMapped && f.IsRequired);
            return DataTypeRequiredFieldCount == MappedFieldsCount;
        }
    }
}
