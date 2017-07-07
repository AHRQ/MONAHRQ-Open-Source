using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Monahrq.Theme.Controls
{
	/// <summary>
	/// ComboBox control with custom sizing functionality.
	/// </summary>
	/// <seealso cref="System.Windows.Controls.ComboBox" />
	public class ComboBoxEx : ComboBox
    {
		/// <summary>
		/// The selected
		/// </summary>
		private int _selected;

		/// <summary>
		/// Called when <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" /> is called.
		/// </summary>
		public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _selected = SelectedIndex;
            SelectedIndex = -1;
            Loaded += ComboBoxEx_Loaded;
        }

		/// <summary>
		/// Handles the Loaded event of the ComboBoxEx control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		void ComboBoxEx_Loaded(object sender, RoutedEventArgs e)
        {
            var popup = GetTemplateChild("PART_Popup") as Popup;
            var content = popup.Child as FrameworkElement;
            content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            MinWidth = content.DesiredSize.Width + ActualWidth;
            SelectedIndex = _selected;
        }
    }
}
