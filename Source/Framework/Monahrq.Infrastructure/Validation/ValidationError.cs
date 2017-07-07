using System.Reflection;
namespace Monahrq.Infrastructure.Validation
{
    /// <summary>
    /// Represents a validation error from a <see cref="IEntityValidator{TEntity}.Validate"/> method
    /// call.
    /// </summary>
    public struct ValidationError
    {
        #region fields
        /// <summary>
        /// The validation error message.
        /// </summary>
        public readonly string Message;
        /// <summary>
        /// The validation error property.
        /// </summary>
        public readonly PropertyInfo Property;

        /// <summary>
        /// 
        /// </summary>
        public readonly object AttemptedValue;

        public readonly ValidationErrorState ErrorState;
        #endregion

        #region ctor
        /// <summary>
        /// Default Constructor.
        /// Creates a new instance of the <see cref="ValidationError" /> data structure.
        /// </summary>
        /// <param name="message">string. The validation error message.</param>
        /// <param name="property">string. The property that was validated.</param>
        /// <param name="errorState">State of the error.</param>
        public ValidationError(string message, PropertyInfo property, ValidationErrorState errorState = ValidationErrorState.ValidationError)
        {
            AttemptedValue = null;
            Message = message;
            Property = property;
            ErrorState = errorState;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError" /> struct.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <param name="errorState">State of the error.</param>
        public ValidationError(string message, PropertyInfo property, object value, ValidationErrorState errorState = ValidationErrorState.ValidationError)
        {
            Message = message;
            Property = property;
            AttemptedValue = value;
            ErrorState = errorState;
        }
        #endregion

        #region methods
        /// <summary>
        /// Overriden. Gets a string that represents the validation error.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("({0}) - {1}", Property.Name, Message);
        }

        /// <summary>
        /// Overridden. Compares if an object is equal to the <see cref="ValidationError"/> instance.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof (ValidationError)) return false;
            return Equals((ValidationError) obj);
        }

        /// <summary>
        /// Overriden. Compares if a <see cref="ValidationError"/> instance is equal to this
        /// <see cref="ValidationError"/> instance.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Equals(ValidationError obj)
        {
            return Equals(obj.Message, Message) && Equals(obj.Property, Property);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Message.GetHashCode() * 397) ^ Property.GetHashCode();
            }
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ValidationError left, ValidationError right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ValidationError left, ValidationError right)
        {
            return !left.Equals(right);
        }
        #endregion
    }

    public enum ValidationErrorState
    {
        ValidationError, Warning, ExcludedByCrosswalk
    }
}