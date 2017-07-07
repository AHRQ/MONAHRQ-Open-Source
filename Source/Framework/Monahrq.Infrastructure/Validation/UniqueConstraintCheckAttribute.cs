using Microsoft.Practices.ServiceLocation;
using Monahrq.Infrastructure.Entities.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Monahrq.Infrastructure.Extensions;
using Monahrq.Infrastructure.Utility;
using Monahrq.Sdk.Utilities;

namespace Monahrq.Infrastructure.Validation
{
    /// <summary>
    /// Validates that the <see cref="DbColumnName"/> column of the database table <see cref="DbSchemaName"/> does not have any existing entries with the given value
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UniqueConstraintCheckAttribute : ValidationAttribute
    {
        /// <summary>
        /// The name of the database table to query
        /// </summary>
        public string DbSchemaName { get; set; }

        /// <summary>
        /// The name of the column to be searched for matching records
        /// </summary>
        public string DbColumnName { get; set; }

        /// <summary>
        /// The message returned if the unique constraint check fails. If the value of this property is null or empty, a default message is used instead.
        /// </summary>
        public string ValidationMessage { get; set; }

        private System.ComponentModel.DataAnnotations.ValidationResult _validationResult = null;

        public UniqueConstraintCheckAttribute()
        { }

        public override bool IsValid(object value)
        {
            return _validationResult == null;
        }

        public override bool RequiresValidationContext
        {
            get
            {
                return true;
            }
        }

        protected override System.ComponentModel.DataAnnotations.ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            _validationResult = null;
            _validationResult = base.IsValid(value, validationContext);

            var ownerToTest = validationContext.ObjectInstance as Entity<int>;
            if (ownerToTest == null) return _validationResult;

            var entityType = validationContext.ObjectType;
            DbSchemaName = string.IsNullOrEmpty(DbSchemaName) ? entityType.EntityTableName() : DbSchemaName;
            DbColumnName = string.IsNullOrEmpty(DbColumnName) ? validationContext.MemberName : DbColumnName;
            ValidationMessage = string.IsNullOrEmpty(ValidationMessage)
                                                ? string.Format("Property {0} must be unique across all \"{1}\" entities.", validationContext.MemberName, Inflector.Pluralize(entityType.Name))
                                                : ValidationMessage;

            var provider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();

            if (_validationResult == null)
            {
                int npiCount;
                using (var session = provider.SessionFactory.OpenStatelessSession())
                {
                    var query = String.Format("select Count({1}) from [dbo].[{0}] where [{1}]={2} {3}", DbSchemaName, DbColumnName, value, ownerToTest.IsPersisted ? "and Id <> " + ownerToTest.Id : "");
                    npiCount = session.CreateSQLQuery(query)
                                    .UniqueResult<int>();
                }

                if (npiCount > 0)
                    _validationResult = new System.ComponentModel.DataAnnotations.ValidationResult(ValidationMessage, new string[] { validationContext.MemberName });
            }
            return _validationResult;
        }

        public override string FormatErrorMessage(string name)
        {
            return ValidationMessage;
        }
    }
}
