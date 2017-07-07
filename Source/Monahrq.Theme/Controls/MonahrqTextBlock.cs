using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Monahrq.Theme.Controls
{
	/// <summary>
	/// TextBox Control.
	/// </summary>
	/// <seealso cref="System.Windows.Controls.TextBox" />
	public class MonahrqTextBlock : TextBox
    {
		/// <summary>
		/// Is called when a control template is applied.
		/// </summary>
		public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var zeroThickness = new Thickness(0);
            var transparent = new SolidColorBrush(Colors.Transparent);
            var contentElement = GetTemplateChild("ContentElement") as ScrollViewer;

            Background = transparent;
            this.BorderBrush = transparent;
            BorderThickness = zeroThickness;

            Padding = zeroThickness;
            TextWrapping = TextWrapping.Wrap;
            IsReadOnly = true;
        }
    }
}
