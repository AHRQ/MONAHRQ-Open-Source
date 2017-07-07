
namespace Monahrq.Theme.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// An implementation of <see cref="IValueConverter"/> that converts boolean values to <see cref="Visibility"/> values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>BooleanToVisibilityConverter</c> class can be used to convert boolean values (or values that can be converted to boolean values) to
    /// <see cref="Visibility"/> values. By default, <see langword="true"/> is converted to <see cref="Visibility.Visible"/> and <see langword="false"/>
    /// is converted to <see cref="Visibility.Collapsed"/>. However, the <see cref="UseHidden"/> property can be set to <see langword="true"/> in order
    /// to return <see cref="Visibility.Hidden"/> instead of <see cref="Visibility.Collapsed"/>. In addition, the <see cref="IsInverted"/> property
    /// can be set to <see langword="true"/> to reverse the returned values.
    /// </para>
    /// </remarks>
    /// <example>
    /// The following example shows how a <c>BooleanToVisibilityConverter</c> can be used to display a <c>TextBox</c> only when a property is <c>true</c>:
    /// <code lang="xml">
    /// <![CDATA[
    /// <TextBox Visibility="{Binding ShowTheTextBox, Converter={BooleanToVisibilityConverter}}"/>
    /// ]]>
    /// </code>
    /// </example>
    /// <example>
    /// The following example shows how a <c>BooleanToVisibilityConverter</c> can be used to display a <c>TextBox</c> only when a property is <c>true</c>.
    /// Rather than collapsing the <c>TextBox</c>, it is hidden:
    /// <code lang="xml">
    /// <![CDATA[
    /// <TextBox Visibility="{Binding ShowTheTextBox, Converter={BooleanToVisibilityConverter UseHidden=true}}"/>
    /// ]]>
    /// </code>
    /// </example>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        private bool isInverted;
        private bool useHidden;

        /// <summary>
        /// Initializes a new instance of the BooleanToVisibilityConverter class.
        /// </summary>
        public BooleanToVisibilityConverter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BooleanToVisibilityConverter class.
        /// </summary>
        /// <param name="isInverted">
        /// Whether the return values should be reversed.
        /// </param>
        /// <param name="useHidden">
        /// Whether <see cref="Visibility.Hidden"/> should be used instead of <see cref="Visibility.Collapsed"/>.
        /// </param>
        public BooleanToVisibilityConverter(bool isInverted, bool useHidden)
        {
            this.isInverted = isInverted;
            this.useHidden = useHidden;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the return values should be reversed.
        /// </summary>
        public bool IsInverted
        {
            get { return this.isInverted; }
            set { this.isInverted = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="Visibility.Hidden"/> should be returned instead of <see cref="Visibility.Collapsed"/>.
        /// </summary>
        public bool UseHidden
        {
            get { return this.useHidden; }
            set { this.useHidden = value; }
        }

        /// <summary>
        /// Attempts to convert the specified value.
        /// </summary>
        /// <param name="value">
        /// The value to convert.
        /// </param>
        /// <param name="targetType">
        /// The type of the binding target property.
        /// </param>
        /// <param name="parameter">
        /// The converter parameter to use.
        /// </param>
        /// <param name="culture">
        /// The culture to use in the converter.
        /// </param>
        /// <returns>
        /// A converted value.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);

            if (this.IsInverted)
            {
                val = !val;
            }

            if (val)
            {
                return Visibility.Visible;
            }

            return this.UseHidden ? Visibility.Hidden : Visibility.Collapsed;
        }

        /// <summary>
        /// Attempts to convert the specified value back.
        /// </summary>
        /// <param name="value">
        /// The value to convert.
        /// </param>
        /// <param name="targetType">
        /// The type of the binding target property.
        /// </param>
        /// <param name="parameter">
        /// The converter parameter to use.
        /// </param>
        /// <param name="culture">
        /// The culture to use in the converter.
        /// </param>
        /// <returns>
        /// A converted value.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Visibility))
            {
                return DependencyProperty.UnsetValue;
            }

            var visibility = (Visibility)value;
            var result = visibility == Visibility.Visible;

            if (this.IsInverted)
            {
                result = !result;
            }

            return result;
        }
    }
}