using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Monahrq.Theme.Controls
{
	/// <summary>
	/// Add a popup handle to a control.
	/// </summary>
	/// <seealso cref="System.Windows.Documents.Adorner" />
	public class PopupAdorner : Adorner
    {
		/// <summary>
		/// The visuals
		/// </summary>
		private VisualCollection _Visuals;
		/// <summary>
		/// The content presenter
		/// </summary>
		private ContentPresenter _ContentPresenter;

		/// <summary>
		/// Initializes a new instance of the <see cref="PopupAdorner"/> class.
		/// </summary>
		/// <param name="adornedElement">The element to bind the adorner to.</param>
		public PopupAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            _Visuals = new VisualCollection(this);
            _ContentPresenter = new ContentPresenter();
            _Visuals.Add(_ContentPresenter);
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="PopupAdorner"/> class.
		/// </summary>
		/// <param name="adornedElement">The adorned element.</param>
		/// <param name="content">The content.</param>
		public PopupAdorner(UIElement adornedElement, Visual content)
            : this(adornedElement) { Content = content; }

		/// <summary>
		/// Implements any custom measuring behavior for the adorner.
		/// </summary>
		/// <param name="constraint">A size to constrain the adorner to.</param>
		/// <returns>
		/// A <see cref="T:System.Windows.Size" /> object representing the amount of layout space needed by the adorner.
		/// </returns>
		protected override Size MeasureOverride(Size constraint)
        {
            _ContentPresenter.Measure(constraint);
            return _ContentPresenter.DesiredSize;
        }

		/// <summary>
		/// When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement" /> derived class.
		/// </summary>
		/// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
		/// <returns>
		/// The actual size used.
		/// </returns>
		protected override Size ArrangeOverride(Size finalSize)
        {
            _ContentPresenter.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            return _ContentPresenter.RenderSize;
        }

		/// <summary>
		/// Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)" />, and returns a child at the specified index from a collection of child elements.
		/// </summary>
		/// <param name="index">The zero-based index of the requested child element in the collection.</param>
		/// <returns>
		/// The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.
		/// </returns>
		protected override Visual GetVisualChild(int index) { return _Visuals[index]; }

		/// <summary>
		/// Gets the number of visual child elements within this element.
		/// </summary>
		protected override int VisualChildrenCount { get { return _Visuals.Count; } }

		/// <summary>
		/// Gets or sets the content.
		/// </summary>
		/// <value>
		/// The content.
		/// </value>
		public object Content
        {
            get { return _ContentPresenter.Content; }
            set { _ContentPresenter.Content = value; }
        }
    }


}
