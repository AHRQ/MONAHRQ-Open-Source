using System;
using System.Windows;
using System.Windows.Data;

namespace Monahrq.DataSets.ViewModels.Validation
{
    /// <summary>
    /// A custom Visibility converter.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public abstract class VisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets the mode to apply.
        /// </summary>
        /// <value>
        /// The mode to apply.
        /// </value>
        protected abstract Visibility ModeToApply
        {
            get;
        }

        /// <summary>
        /// Evaluates the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected abstract bool Evaluate(object value);

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var applyValue = Evaluate(value);
            var invert = string.Equals("Invert", (parameter ?? string.Empty).ToString(), StringComparison.OrdinalIgnoreCase);
            if (invert) applyValue = !applyValue;
            var result = applyValue ? ModeToApply : Visibility.Visible;
            return result;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A custom boolean visibility converter.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.ViewModels.Validation.VisibilityConverter" />
    public abstract class BooleanVisibilityConverter : VisibilityConverter
    {
        /// <summary>
        /// Evaluates the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected override bool Evaluate(object value)
        {
            return bool.Equals(value, true);
        }
    }

    /// <summary>
    /// A boolean collapse converter.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.ViewModels.Validation.BooleanVisibilityConverter" />
    public class BooleanCollapseConverter : BooleanVisibilityConverter
    {
        /// <summary>
        /// Gets the mode to apply.
        /// </summary>
        /// <value>
        /// The mode to apply.
        /// </value>
        protected override Visibility ModeToApply
        {
            get { return Visibility.Collapsed; }
        }
    }

    /// <summary>
    /// A bool hidden converter.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.ViewModels.Validation.BooleanVisibilityConverter" />
    public class BooleanHiddenConverter : BooleanVisibilityConverter
    {
        /// <summary>
        /// Gets the mode to apply.
        /// </summary>
        /// <value>
        /// The mode to apply.
        /// </value>
        protected override Visibility ModeToApply
        {
            get { return Visibility.Hidden; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.ViewModels.Validation.VisibilityConverter" />
    public abstract class NullVisibilityConverter : VisibilityConverter
    {
        /// <summary>
        /// Evaluates the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected override bool Evaluate(object value)
        {
            return value == null;
        }
    }

    /// <summary>
    /// A null collapse converter.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.ViewModels.Validation.NullVisibilityConverter" />
    public class NullCollapseConverter : NullVisibilityConverter
    {

        /// <summary>
        /// Gets the mode to apply.
        /// </summary>
        /// <value>
        /// The mode to apply.
        /// </value>
        protected override Visibility ModeToApply
        {
            get { return Visibility.Collapsed; }
        }
    }

    /// <summary>
    /// The null hidden converter.
    /// </summary>
    /// <seealso cref="Monahrq.DataSets.ViewModels.Validation.NullVisibilityConverter" />
    public class NullHiddenConverter : NullVisibilityConverter
    {
        /// <summary>
        /// Gets the mode to apply.
        /// </summary>
        /// <value>
        /// The mode to apply.
        /// </value>
        protected override Visibility ModeToApply
        {
            get { return Visibility.Hidden; }
        }
    }
}
