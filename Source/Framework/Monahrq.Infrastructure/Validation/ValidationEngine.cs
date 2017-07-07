using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Linq;
using System;

namespace Monahrq.Infrastructure.Validation
{

    /// <summary>
    /// 
    /// </summary>
    public class ValidationEngine
    {
        IList<Guid> Suppressions { get; set; }
        public ValidationEngine(IList<Guid> supressConstraints)
        {
            Suppressions = supressConstraints ?? new List<Guid>();
        }

        #region Old Code - Custom DataAnnotation Validation Engine logic
        /// <summary>
        /// Validates the attributes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public bool ValidateAttributes(object entity, IList<ValidationError> errors, IList<ValidationError> warnings)
        {
            return ValidateAttributesInternal(entity, errors, warnings);
        }

        public bool ValidateAttributes(object entity, IList<ValidationError> errors)
        {
            var warnings = new List<ValidationError>();
            var result = ValidateAttributes(entity, errors, warnings);
            warnings.ForEach(item => errors.Add(item));
            return result;
        }

        /// <summary>
        /// Validates the attributes internal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        protected bool ValidateAttributesInternal(object entity, IList<ValidationError> errors, IList<ValidationError> warnings)
        {
            // Get list of properties from validationModel
            PropertyInfo[] props = entity.GetType().GetProperties();

            // Perform validation on each property
            foreach (PropertyInfo prop in props)
                ValidateProperty(errors, warnings, entity, prop);

            // TODO: CHECK THIS: IS WARNINGS.COUNT == 0 OK?????
            return (errors.Count == 0 && warnings.Count == 0);
        }

        /// <summary>
        /// Validates the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="validationIssues">The validation issues.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="property">The property.</param>
        protected void ValidateProperty(IList<ValidationError> validationErrors, IList<ValidationError> validationWarnings, object entity, PropertyInfo property)
        {
            // Get list of validator attributes
            var validators = property.GetCustomAttributes(typeof(ValidationAttribute), true)
                .Where(v => !(v is OptionalConstraintAttribute) || !Suppressions.Contains((v as OptionalConstraintAttribute).ConstraintGuid)); 
            foreach (ValidationAttribute validator in validators)
            {
                ValidateValidator(validator is WarningValidationAttribute ? validationWarnings : validationErrors, entity, property, validator);
            }
        }

        /// <summary>
        /// Validates the validator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="validationIssues">The validation issues.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="property">The property.</param>
        /// <param name="validator">The validator.</param>
        protected void ValidateValidator(IList<ValidationError> validationIssues, object entity, PropertyInfo property, ValidationAttribute validator)
        {
            var value = property.GetValue(entity, null);
            var ctxt = new ValidationContext(entity);
            var valResult = new List<ValidationResult>();
            var result = Validator.TryValidateValue(value, ctxt, valResult, new[] { validator });
            if (!result)
            {
                var desc = property.GetCustomAttributes<System.ComponentModel.DescriptionAttribute>(true).FirstOrDefault();
                var errorMessage = desc == null ? validator.FormatErrorMessage(property.Name)
                    :  validator.FormatErrorMessage(desc.Description);
                validationIssues.Add(new ValidationError(errorMessage, property, value));
            }
        }
        #endregion
    }
}
