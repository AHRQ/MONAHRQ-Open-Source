using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Monahrq.Infrastructure.Validation
{
    public class InstanceValidator
    {
        class PropertyDescriptor
        {
            public PropertyDescriptor(PropertyInfo prop)
            {
                Property = prop;
                var desc = prop.GetCustomAttribute<DescriptionAttribute>();
                Description = desc == null ? prop.Name : desc.Description;
            }

            public string Description { get; private set; }
            public PropertyInfo Property { get; private set; }

        }

        public InstanceValidator(Type targetType)
        {
            TargetType = targetType;
            LoadValidators();
        }

        private Dictionary<PropertyDescriptor, List<ValidationAttribute>> ErrorValidators { get; set; }
        private Dictionary<PropertyDescriptor, List<ValidationAttribute>> WarningValidators { get; set; }
        Dictionary<string, PropertyDescriptor> PropertyDescriptors { get; set; }
        public IList<ValidationAttribute> ClassValidators { get; private set; }
        private void LoadValidators()
        {
            var props = TargetType.GetProperties();
            PropertyDescriptors = props.Select(prop => new PropertyDescriptor(prop)).ToDictionary(p => p.Property.Name);

            ErrorValidators = props.ToDictionary(key =>
                    new PropertyDescriptor(key),
                value =>
                {
                    return value.GetCustomAttributes<ValidationAttribute>(true)
                         .Where(a => !(a is WarningValidationAttribute)).ToList();
                }
                );
            WarningValidators = props.ToDictionary(key => new PropertyDescriptor(key),
                value =>
                {
                    return value.GetCustomAttributes<ValidationAttribute>(true)
                         .Where(a => (a is WarningValidationAttribute)).ToList();
                }
                );
            ClassValidators = TargetType.GetCustomAttributes<ValidationAttribute>(false).ToList();

        }

        protected Type TargetType { get; set; }
        public InstanceValidatorResults ValidateInstance(object entity)
        {
            if (entity == null) throw new NullReferenceException("Validation Target cannot be null");

            //var target = entity as Monahrq.Infrastructure.Data.Extensibility.ContentManagement.Records.Dataset;
            //if (target == null || !target.ContentType.IsCustom)
            {
                if (!TargetType.IsAssignableFrom(entity.GetType()))
                {
                    throw new ArgumentException(string.Format("Validation target is not of type {0}", TargetType.FullName));
                }
            }

            var result = new InstanceValidatorResults();

            BuildClassResults(entity, result.ClassErrors);
            if (result.ClassErrors != null && result.ClassErrors.Count > 0) return result;

            BuildPropertyResults(entity, result.PropertyErrors, ErrorValidators);
            BuildPropertyResults(entity, result.PropertyWarnings, WarningValidators);
  
            return result;
        }

        //readonly ILoggerFacade logger = ServiceLocator.Current.GetInstance<ILoggerFacade>(LogNames.Session);
        private void BuildClassResults(object entity, IList<ValidationError> errors)
        {
            if (ClassValidators == null || !ClassValidators.Any()) return;
            //var timer = new Stopwatch();
#if DEBUG
            var timer2 = new Stopwatch();
#endif
           // timer.Start();

            var ctxt = new ValidationContext(entity);
            foreach (var attr in ClassValidators)
            {
#if DEBUG
                timer2.Start();
#endif

                var result = attr.GetValidationResult(entity, ctxt);

#if DEBUG
                timer2.Stop();
                var message = string.Format("IP/ED dataset imports - Time of Execution of class validation {2}: {0}:{1}",
                timer2.Elapsed.Seconds, timer2.Elapsed.Milliseconds, attr.GetType().Name);

                Trace.WriteLine(message);
                timer2.Reset();
#endif
                if (result != null && !string.IsNullOrEmpty(result.ErrorMessage))
                {
#if DEBUG
                    timer2.Start();
#endif
                    ValidationErrorState errorState = (attr.GetType() == typeof (RejectIfAnyPropertyHasValueAttribute))
                                                                        ? ValidationErrorState.ExcludedByCrosswalk
                                                                        : ValidationErrorState.ValidationError;

                    if (result.MemberNames != null && result.MemberNames.Any())
                    {
                        foreach (var memberName in result.MemberNames.ToList())
                        {
                            PropertyDescriptor propDesc;
                            if (PropertyDescriptors.TryGetValue(memberName, out propDesc))
                            {
                                errors.Add(new ValidationError(result.ErrorMessage, PropertyDescriptors[memberName].Property, errorState));
                            }
                            else
                            {
                                errors.Add(new ValidationError(result.ErrorMessage, null, errorState));
                            }
                        }
                    }
                    else
                    {
                        errors.Add(new ValidationError(result.ErrorMessage, null, errorState));
                    }

#if DEBUG
                    timer2.Stop(); 
                    var message2 = string.Format("IP/ED dataset imports - Time of Execution of message processing: {0}:{1}",
                    timer2.Elapsed.Seconds, timer2.Elapsed.Milliseconds);
                    Trace.WriteLine(message2);
#endif
                }
            }
           // timer.Stop();
           // var message3 = string.Format("IP/ED dataset imports - Overall Time of Execution of all class validations: {0}:{1}",
           // timer.Elapsed.Seconds, timer.Elapsed.Milliseconds);

           // Trace.WriteLine(message3);

            //if(logger != null)
            //    logger.Log(message, Category.Debug, Priority.High);
        }

        private void BuildPropertyResults(object entity, IList<ValidationError> list, Dictionary<PropertyDescriptor, List<ValidationAttribute>> validationSet)
        {
            var ctxt = new ValidationContext(entity);

            foreach (var item in validationSet)
            {
                var value = item.Key.Property.GetValue(entity, null);
                foreach (var validator in item.Value)
                {
                    var valResult = new List<ValidationResult>();
                    var result = Validator.TryValidateValue(value, ctxt, valResult, new[] { validator });
                    if (!result)
                    {
                        var desc = item.Key.Description;
                        var errorMessage = validator.FormatErrorMessage(desc);
                        list.Add(new ValidationError(string.Format("{0} {1} value :{2}",desc, errorMessage,value), item.Key.Property, value));
                    }
                }
            }
        }
    }

    public class InstanceValidator<T> : InstanceValidator
    {
        public InstanceValidator()
            : base(typeof(T))
        {
        }
    }

    public class InstanceValidatorResults
    {
        public InstanceValidatorResults()
        {
            PropertyErrors = new List<ValidationError>();
            PropertyWarnings = new List<ValidationError>();
            ClassErrors = new List<ValidationError>();
        }
        public IList<ValidationError> PropertyErrors { get; private set; }
        public IList<ValidationError> PropertyWarnings { get; private set; }
        public IList<ValidationError> ClassErrors { get; private set; }
    }


    class ValidationTarget
    {
        public PropertyInfo Property { get; set; }
        public List<ValidationAttribute> ErrorValidators { get; set; }
        public List<ValidationAttribute> WarningValidators { get; set; }
    }

}
