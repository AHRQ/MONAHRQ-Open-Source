using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Entities.Domain.Reports.Validators
{
    /// <summary>
    /// Validates that a <see cref="IList"/> contains at least one element
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class NonEmptyListAttribute : ValidationAttribute
    {

        string _modelPropertiesChain;

        /// <summary>
        /// Allows verification of a nested list found at the path specified by <paramref name="modelProperty"/>
        /// </summary>
        /// <param name="modelProperty">The path to the nested list (e.g.: "PropA.PropB.MyList") </param>
        public NonEmptyListAttribute(string modelProperty)
        {
            _modelPropertiesChain = modelProperty;
        }

        /// <summary>
        /// Validates that a <see cref="IList"/> contains at least one element
        /// </summary>
        public NonEmptyListAttribute() { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var viewPropName = validationContext.DisplayName;


            object modelPropValue = null;
            if (_modelPropertiesChain != null)
            {
                modelPropValue = validationContext.ObjectInstance;
                try
                {
                    var propertyNames = _modelPropertiesChain.Split('.');
                    foreach (var name in propertyNames)
                    {
                        var modelPropInfo = modelPropValue.GetType().GetProperty(name);
                        modelPropValue = modelPropInfo.GetValue(modelPropValue);
                    }

                }
                catch (Exception) { return ValidationResult.Success; }
            }
            else modelPropValue = value;

            if (modelPropValue == null)
                return ValidationResult.Success;
            IList objectsList = null;
            // todo: convert to "as" to avoid exception
            try { objectsList = (IList)modelPropValue; }
            catch (Exception)
            {
                if (objectsList == null)
                    return ValidationResult.Success;
            }

            if (objectsList.Count == 0)
                return new ValidationResult(string.Format("Must have, at least, one item selected in {0}", viewPropName));

            return ValidationResult.Success;
        }
    }
}
