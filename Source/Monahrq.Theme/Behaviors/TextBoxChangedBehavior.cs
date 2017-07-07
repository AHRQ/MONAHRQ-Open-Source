using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Monahrq.Theme.Behaviors
{
	/// <summary>
	/// Behavior to track changes in a textbox.
	/// </summary>
	/// <seealso cref="System.Windows.Interactivity.Behavior{System.Windows.Controls.TextBox}" />
	public class TextBoxChangedBehavior : Behavior<TextBox>
    {
		/// <summary>
		/// Called after the behavior is attached to an AssociatedObject.
		/// </summary>
		/// <remarks>
		/// Override this to hook up functionality to the AssociatedObject.
		/// </remarks>
		protected override void OnAttached()
        {
            AssociatedObject.TextChanged += AssociatedObject_TextChanged;
        }

		/// <summary>
		/// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
		/// </summary>
		/// <remarks>
		/// Override this to unhook functionality from the AssociatedObject.
		/// </remarks>
		protected override void OnDetaching()
        {
            AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
        }

		/// <summary>
		/// Handles the TextChanged event of the AssociatedObject control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
		void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;
            var binding = tb.GetBindingExpression(TextBox.TextProperty);
            if (binding != null)
            {
                binding.UpdateSource();
            }
        }
    }
}
