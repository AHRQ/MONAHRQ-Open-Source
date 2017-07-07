using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Monahrq.Theme.Controls
{

	//	http://stackoverflow.com/questions/1317891/simple-wpf-radiobutton-binding
	/// <summary>
	/// RadioButton Control.
	/// </summary>
	/// <seealso cref="System.Windows.Controls.RadioButton" />
	public class RadioButtonEx : RadioButton
	{
		/// <summary>
		/// Gets or sets the radio value.
		/// </summary>
		/// <value>
		/// The radio value.
		/// </value>
		public object RadioValue
		{
			get { return (object)GetValue(RadioValueProperty); }
			set { SetValue(RadioValueProperty, value); }
		}

		//	Using a DependencyProperty as the backing store for RadioValue.
		//	This enables animation, styling, binding, etc...

		/// <summary>
		/// The radio value property
		/// </summary>
		public static readonly DependencyProperty RadioValueProperty =
			DependencyProperty.Register(
				"RadioValue",
				typeof(object),
				typeof(RadioButtonEx),
				new UIPropertyMetadata(null));

		/// <summary>
		/// Gets or sets the radio binding.
		/// </summary>
		/// <value>
		/// The radio binding.
		/// </value>
		public object RadioBinding
		{
			get { return (object)GetValue(RadioBindingProperty); }
			set { SetValue(RadioBindingProperty, value); }
		}

		//	Using a DependencyProperty as the backing store for RadioBinding.
		//	This enables animation, styling, binding, etc...

		/// <summary>
		/// The radio binding property
		/// </summary>
		public static readonly DependencyProperty RadioBindingProperty =
			DependencyProperty.Register(
				"RadioBinding",
				typeof(object),
				typeof(RadioButtonEx),
				new FrameworkPropertyMetadata(
					null,
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
					OnRadioBindingChanged));

		/// <summary>
		/// Called when [radio binding changed].
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnRadioBindingChanged(
			DependencyObject d,
			DependencyPropertyChangedEventArgs e)
		{
			RadioButtonEx rb = (RadioButtonEx)d;
			if (rb.RadioValue.Equals(e.NewValue))
				rb.SetCurrentValue(RadioButton.IsCheckedProperty, true);
		}

		/// <summary>
		/// Called when the <see cref="P:System.Windows.Controls.Primitives.ToggleButton.IsChecked" /> property becomes true.
		/// </summary>
		/// <param name="e">Provides data for <see cref="T:System.Windows.RoutedEventArgs" />.</param>
		protected override void OnChecked(RoutedEventArgs e)
		{
			base.OnChecked(e);
			SetCurrentValue(RadioBindingProperty, RadioValue);
		}
	}
}
