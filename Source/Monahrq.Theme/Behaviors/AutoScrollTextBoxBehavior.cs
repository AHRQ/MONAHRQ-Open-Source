using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Monahrq.Theme.Behaviors
{
	/// <summary>
	/// Behavior for auto scroll on textboxes.
	/// </summary>
	public class AutoScrollTextBoxBehavior
    {
		/// <summary>
		/// The associations
		/// </summary>
		static readonly Dictionary<TextBox, Capture> _associations = new Dictionary<TextBox, Capture>();

		/// <summary>
		/// Gets the scroll on text changed.
		/// </summary>
		/// <param name="dependencyObject">The dependency object.</param>
		/// <returns></returns>
		public static bool GetScrollOnTextChanged(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(ScrollOnTextChangedProperty);
        }

		/// <summary>
		/// Sets the scroll on text changed.
		/// </summary>
		/// <param name="dependencyObject">The dependency object.</param>
		/// <param name="value">if set to <c>true</c> [value].</param>
		public static void SetScrollOnTextChanged(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(ScrollOnTextChangedProperty, value);
        }

		/// <summary>
		/// The scroll on text changed property
		/// </summary>
		public static readonly DependencyProperty ScrollOnTextChangedProperty =
            DependencyProperty.RegisterAttached("ScrollOnTextChanged", typeof(bool), typeof(AutoScrollTextBoxBehavior), new UIPropertyMetadata(false, OnScrollOnTextChanged));

		/// <summary>
		/// Called when [scroll on text changed].
		/// </summary>
		/// <param name="dependencyObject">The dependency object.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		static void OnScrollOnTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var textBox = dependencyObject as TextBox;
            if (textBox == null)
            {
                return;
            }
            bool oldValue = (bool)e.OldValue, newValue = (bool)e.NewValue;
            if (newValue == oldValue)
            {
                return;
            }
            if (newValue)
            {
                textBox.Loaded += TextBoxLoaded;
                textBox.Unloaded += TextBoxUnloaded;
            }
            else
            {
                textBox.Loaded -= TextBoxLoaded;
                textBox.Unloaded -= TextBoxUnloaded;
                if (_associations.ContainsKey(textBox))
                {
                    _associations[textBox].Dispose();
                }
            }
        }

		/// <summary>
		/// Texts the box unloaded.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="routedEventArgs">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		static void TextBoxUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var textBox = (TextBox)sender;
            _associations[textBox].Dispose();
            textBox.Unloaded -= TextBoxUnloaded;
        }

		/// <summary>
		/// Texts the box loaded.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="routedEventArgs">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		static void TextBoxLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var textBox = (TextBox)sender;
            textBox.Loaded -= TextBoxLoaded;
            _associations[textBox] = new Capture(textBox);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <seealso cref="System.IDisposable" />
		class Capture : IDisposable
        {
			/// <summary>
			/// Gets or sets the text box.
			/// </summary>
			/// <value>
			/// The text box.
			/// </value>
			private TextBox TextBox { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Capture"/> class.
			/// </summary>
			/// <param name="textBox">The text box.</param>
			public Capture(TextBox textBox)
            {
                TextBox = textBox;
                TextBox.TextChanged += OnTextBoxOnTextChanged;
            }

			/// <summary>
			/// Called when [text box on text changed].
			/// </summary>
			/// <param name="sender">The sender.</param>
			/// <param name="args">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
			private void OnTextBoxOnTextChanged(object sender, TextChangedEventArgs args)
            {
                TextBox.ScrollToEnd();
            }

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose()
            {
                TextBox.TextChanged -= OnTextBoxOnTextChanged;
            }
        }


    }
}
