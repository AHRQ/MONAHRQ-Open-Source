using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Monahrq.Theme.Converters;

namespace Monahrq.Theme.Controls
{
	/// <summary>
	/// Toggle Switch control.
	/// </summary>
	/// <seealso cref="System.Windows.Controls.ContentControl" />
	[TemplateVisualState(Name = NORMAL_STATE, GroupName = COMMON_STATES)]
    [TemplateVisualState(Name = DISABLED_STATE, GroupName = COMMON_STATES)]
    [TemplatePart(Name = SWITCH_PART, Type = typeof(ToggleButton))]
    public class ToggleSwitch : ContentControl
    {
		/// <summary>
		/// The common states
		/// </summary>
		private const string COMMON_STATES = "CommonStates";
		/// <summary>
		/// The normal state
		/// </summary>
		private const string NORMAL_STATE = "Normal";
		/// <summary>
		/// The disabled state
		/// </summary>
		private const string DISABLED_STATE = "Disabled";
		/// <summary>
		/// The switch part
		/// </summary>
		private const string SWITCH_PART = "Switch";

		/// <summary>
		/// The toggle button
		/// </summary>
		private ToggleButton _toggleButton;
		/// <summary>
		/// The was content set
		/// </summary>
		private bool _wasContentSet;

		/// <summary>
		/// The on property
		/// </summary>
		public static readonly DependencyProperty OnProperty = DependencyProperty.Register("OnLabel", typeof(string), typeof(ToggleSwitch), new PropertyMetadata("On"));
		/// <summary>
		/// The off property
		/// </summary>
		public static readonly DependencyProperty OffProperty = DependencyProperty.Register("OffLabel", typeof(string), typeof(ToggleSwitch), new PropertyMetadata("Off"));
		/// <summary>
		/// The header property
		/// </summary>
		public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(ToggleSwitch), new PropertyMetadata(null));
		/// <summary>
		/// The header template property
		/// </summary>
		public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(ToggleSwitch), new PropertyMetadata(null));
		/// <summary>
		/// The switch foreground property
		/// </summary>
		public static readonly DependencyProperty SwitchForegroundProperty = DependencyProperty.Register("SwitchForeground", typeof(Brush), typeof(ToggleSwitch), null);
		/// <summary>
		/// The is checked property
		/// </summary>
		public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool?), typeof(ToggleSwitch), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsCheckedChanged));

		/// <summary>
		/// Occurs when [checked].
		/// </summary>
		public event EventHandler<RoutedEventArgs> Checked;
		/// <summary>
		/// Occurs when [unchecked].
		/// </summary>
		public event EventHandler<RoutedEventArgs> Unchecked;
		/// <summary>
		/// Occurs when [indeterminate].
		/// </summary>
		public event EventHandler<RoutedEventArgs> Indeterminate;
		/// <summary>
		/// Occurs when [click].
		/// </summary>
		public event EventHandler<RoutedEventArgs> Click;

		/// <summary>
		/// Gets or sets the on label.
		/// </summary>
		/// <value>
		/// The on label.
		/// </value>
		public string OnLabel
        {
            get { return (string)GetValue(OnProperty); }
            set { SetValue(OnProperty, value); }
        }

		/// <summary>
		/// Gets or sets the off label.
		/// </summary>
		/// <value>
		/// The off label.
		/// </value>
		public string OffLabel
        {
            get { return (string)GetValue(OffProperty); }
            set { SetValue(OffProperty, value); }
        }

		/// <summary>
		/// Gets or sets the header.
		/// </summary>
		/// <value>
		/// The header.
		/// </value>
		public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

		/// <summary>
		/// Gets or sets the header template.
		/// </summary>
		/// <value>
		/// The header template.
		/// </value>
		public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

		/// <summary>
		/// Gets or sets the switch foreground.
		/// </summary>
		/// <value>
		/// The switch foreground.
		/// </value>
		public Brush SwitchForeground
        {
            get { return (Brush)GetValue(SwitchForegroundProperty); }
            set
            {
                SetValue(SwitchForegroundProperty, value);
            }
        }

		/// <summary>
		/// Gets or sets the is checked.
		/// </summary>
		/// <value>
		/// The is checked.
		/// </value>
		[TypeConverter(typeof(NullableBoolConverter))]
        public bool? IsChecked
        {
            get { return (bool?)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

		/// <summary>
		/// Occurs when [is checked changed].
		/// </summary>
		public event EventHandler IsCheckedChanged;

		/// <summary>
		/// Called when [is checked changed].
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleSwitch = (ToggleSwitch)d;
            if (toggleSwitch._toggleButton != null)
            {
                var oldValue = (bool?)e.OldValue;
                var newValue = (bool?)e.NewValue;

                toggleSwitch._toggleButton.IsChecked = newValue;

                if (oldValue != newValue
                    && toggleSwitch.IsCheckedChanged != null)
                    {
                        toggleSwitch.IsCheckedChanged(toggleSwitch, EventArgs.Empty);
                    }
            }
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="ToggleSwitch"/> class.
		/// </summary>
		public ToggleSwitch()
        {
            DefaultStyleKey = typeof(ToggleSwitch);
        }

		/// <summary>
		/// Sets the default content.
		/// </summary>
		private void SetDefaultContent()
        {
            Binding binding = new Binding("IsChecked") { Source = this, Converter = new OffOnConverter(), ConverterParameter = this };
            SetBinding(ContentProperty, binding);
        }

		/// <summary>
		/// Changes the state of the visual.
		/// </summary>
		/// <param name="useTransitions">if set to <c>true</c> [use transitions].</param>
		private void ChangeVisualState(bool useTransitions)
        {
            VisualStateManager.GoToState(this, IsEnabled ? NORMAL_STATE : DISABLED_STATE, useTransitions);
        }

		/// <summary>
		/// Called when the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property changes.
		/// </summary>
		/// <param name="oldContent">The old value of the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property.</param>
		/// <param name="newContent">The new value of the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property.</param>
		protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            _wasContentSet = true;
        }

		/// <summary>
		/// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
		/// </summary>
		public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (!_wasContentSet && GetBindingExpression(ContentProperty) == null)
            {
                SetDefaultContent();
            }

            if (_toggleButton != null)
            {
                _toggleButton.Checked -= CheckedHandler;
                _toggleButton.Unchecked -= UncheckedHandler;
                _toggleButton.Indeterminate -= IndeterminateHandler;
                _toggleButton.Click -= ClickHandler;
            }
            _toggleButton = GetTemplateChild(SWITCH_PART) as ToggleButton;
            if (_toggleButton != null)
            {
                _toggleButton.Checked += CheckedHandler;
                _toggleButton.Unchecked += UncheckedHandler;
                _toggleButton.Indeterminate += IndeterminateHandler;
                _toggleButton.Click += ClickHandler;
                _toggleButton.IsChecked = IsChecked;
            }
            ChangeVisualState(false);
        }

		/// <summary>
		/// Checkeds the handler.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void CheckedHandler(object sender, RoutedEventArgs e)
        {
            IsChecked = true;
            SafeRaise.Raise(Checked, this, e);
        }

		/// <summary>
		/// Uncheckeds the handler.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void UncheckedHandler(object sender, RoutedEventArgs e)
        {
            IsChecked = false;
            SafeRaise.Raise(Unchecked, this, e);
        }

		/// <summary>
		/// Indeterminates the handler.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void IndeterminateHandler(object sender, RoutedEventArgs e)
        {
            IsChecked = null;
            SafeRaise.Raise(Indeterminate, this, e);
        }

		/// <summary>
		/// Clicks the handler.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void ClickHandler(object sender, RoutedEventArgs e)
        {
            SafeRaise.Raise(Click, this, e);
        }

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{ToggleSwitch IsChecked={0}, Content={1}}}",
                IsChecked,
                Content
            );
        }
    }
}


