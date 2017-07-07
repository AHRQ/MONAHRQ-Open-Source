using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Monahrq.Infrastructure.Validation
{
    /// <summary>
    /// Provides a simple method for suppressing rows that contain unwanted values
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RejectIfAnyPropertyHasValueAttribute : ValidationAttribute
    {
        /// <summary>
        /// A list of values that, when found, will cause the entire row to be ignored
        /// </summary>
        List<object> Test { get; set; }

        List<PropertyInfo> Properties { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="RejectIfAnyPropertyHasValueAttribute"/>
        /// </summary>
        /// <param name="typetoValidate">The CLR type of the object to be validated</param>
        /// <param name="values">A list of values that, when found, will cause the entire row to be ignored</param>
        public RejectIfAnyPropertyHasValueAttribute(Type typetoValidate, params object[] values)
        {
            Test = new List<object>(values);
            Properties = typetoValidate.GetProperties().ToList();
        }

        public override bool RequiresValidationContext
        {
            get
            {
                return true;
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var target = validationContext.ObjectInstance;
            var vals = Properties.Select(p => new { Member = p.Name, Value = p.GetValue(target) });
            var test = vals.Where(v => Test.Contains(v.Value)).ToList();
            return test.Any()
                            ? new ValidationResult("The values mapped to these properties caused the item to be excluded", test.Select(v => v.Member).ToList())
                            : ValidationResult.Success;
        }
    }
}
