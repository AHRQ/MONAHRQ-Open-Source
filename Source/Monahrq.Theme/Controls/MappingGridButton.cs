using System.Windows;
using System.Windows.Controls;

namespace Monahrq.Theme.Controls
{
	/// <summary>
	/// Button control used for Mapping screens.  Adds IsMapped property for xaml.
	/// </summary>
	/// <seealso cref="System.Windows.Controls.Button" />
	[TemplateVisualState(Name = "IsMapped", GroupName = "Mapping")]
    [TemplateVisualState(Name = "IsNotMapped", GroupName = "Mapping")]

    public class MappingGridButton : Button
    {
		/// <summary>
		/// The is mapped property
		/// </summary>
		public static DependencyProperty IsMappedProperty = DependencyProperty.Register(
         "IsMapped",
         typeof(bool),
         typeof(MappingGridButton),
         new PropertyMetadata(OnValueChanged));

		/// <summary>
		/// Gets or sets a value indicating whether this instance is mapped.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is mapped; otherwise, <c>false</c>.
		/// </value>
		public bool IsMapped
        {
            get
            {
                return (bool)GetValue(IsMappedProperty);
            }
            set
            {
                SetValue(IsMappedProperty, value);
            }
        }

		/// <summary>
		/// Called when [value changed].
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (MappingGridButton)d;
            var isMapped = (bool)e.NewValue;
            VisualStateManager.GoToState(button, isMapped ? "IsMapped" : "IsNotMapped", false);
        }

		/// <summary>
		/// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
		/// </summary>
		public override void OnApplyTemplate()
        {
            SetState();
            base.OnApplyTemplate();
        }

		/// <summary>
		/// Sets the state.
		/// </summary>
		private void SetState()
        {
            VisualStateManager.GoToState(this, IsMapped ? "IsMapped" : "IsNotMapped", false);
        }
    }
}
