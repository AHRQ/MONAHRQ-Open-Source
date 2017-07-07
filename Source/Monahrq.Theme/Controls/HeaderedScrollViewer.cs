using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Monahrq.Theme.Controls {
	/// <summary>
	/// ScrollViewer control with header and footer.
	/// </summary>
	/// <seealso cref="System.Windows.Controls.ScrollViewer" />
	public class HeaderedScrollViewer: ScrollViewer {

		/// <summary>
		/// Initializes the <see cref="HeaderedScrollViewer"/> class.
		/// </summary>
		static HeaderedScrollViewer() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderedScrollViewer), new FrameworkPropertyMetadata(typeof(HeaderedScrollViewer)));
        }


		#region Header

		/// <summary>
		/// Header Dependency Property
		/// </summary>
		public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(FrameworkElement), typeof(HeaderedScrollViewer),
                new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets or sets the Header property. This dependency property
		/// indicates ....
		/// </summary>
		/// <value>
		/// The header.
		/// </value>
		public FrameworkElement Header {
            get { return (FrameworkElement)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

		#endregion

		#region Footer

		/// <summary>
		/// Footer Dependency Property
		/// </summary>
		public static readonly DependencyProperty FooterProperty =
            DependencyProperty.Register("Footer", typeof(FrameworkElement), typeof(HeaderedScrollViewer),
                new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets or sets the Footer property. This dependency property
		/// indicates ....
		/// </summary>
		/// <value>
		/// The footer.
		/// </value>
		public FrameworkElement Footer {
            get { return (FrameworkElement)GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        #endregion

        

    }
}
