using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monahrq.Infrastructure.Entities.Domain;
using Microsoft.Practices.ServiceLocation;

namespace Monahrq.Sdk.ViewModels
{
    /// <summary>
    /// Custom Validation to ensure the uniqueness of a field. If false, return a failed <see cref="ValidationResult"/>.
    /// </summary>
    /// <seealso cref="System.ComponentModel.DataAnnotations.ValidationAttribute" />
    public class UniqueAttribute : ValidationAttribute
    {
        /// <summary>
        /// Gets or sets the data service provider.
        /// </summary>
        /// <value>
        /// The data service provider.
        /// </value>
        public IDomainSessionFactoryProvider DataServiceProvider { get; set; }

        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        /// <value>
        /// The name of the column.
        /// </value>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        /// <value>
        /// The type of the property.
        /// </value>
        public Type PropertyType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueAttribute"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="errorMessage">The error message.</param>
        public UniqueAttribute(string tableName, string columnName, string errorMessage)
        {
            DataServiceProvider = ServiceLocator.Current.GetInstance<IDomainSessionFactoryProvider>();
            ColumnName = columnName;
            TableName = tableName;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            if (DataServiceProvider == null || (value != null && string.IsNullOrEmpty(value.ToString()))) return ValidationResult.Success;

            var query = string.Format("SELECT COUNT(*) FROM {0} WHERE {1} = '{2}'", TableName, ColumnName, value.ToString());

            using (var session = DataServiceProvider.SessionFactory.OpenSession())
            {
                var result = session.CreateSQLQuery(query).UniqueResult<int>();
                if ((int)result > 0)
                {
                    return new ValidationResult(FormatErrorMessage(ErrorMessage), new List<string> { validationContext.MemberName });
                }
            }
            return ValidationResult.Success;
        }


    }
}
