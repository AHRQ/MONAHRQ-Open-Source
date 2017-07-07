using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Monahrq.Theme.Controls
{
	/// <summary>
	/// DockPanel control that resizes with parent.
	/// </summary>
	/// <seealso cref="System.Windows.Controls.Control" />
	public class DockPanelSplitter : Control
    {
		/// <summary>
		/// Initializes the <see cref="DockPanelSplitter"/> class.
		/// </summary>
		static DockPanelSplitter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockPanelSplitter),
                new FrameworkPropertyMetadata(typeof(DockPanelSplitter)));

            // override the Background property
            BackgroundProperty.OverrideMetadata(typeof(DockPanelSplitter), new FrameworkPropertyMetadata(Brushes.Transparent));

            // override the Dock property to get notifications when Dock is changed
            DockPanel.DockProperty.OverrideMetadata(typeof(DockPanelSplitter),
                new FrameworkPropertyMetadata(Dock.Left, DockChanged));
        }

		/// <summary>
		/// Resize the target element proportionally with the parent container
		/// Set to false if you don't want the element to be resized when the parent is resized.
		/// </summary>
		/// <value>
		///   <c>true</c> if [proportional resize]; otherwise, <c>false</c>.
		/// </value>
		public bool ProportionalResize
        {
            get { return (bool)GetValue(ProportionalResizeProperty); }
            set { SetValue(ProportionalResizeProperty, value); }
        }

		/// <summary>
		/// The proportional resize property
		/// </summary>
		public static readonly DependencyProperty ProportionalResizeProperty =
            DependencyProperty.Register("ProportionalResize", typeof(bool), typeof(DockPanelSplitter),
            new UIPropertyMetadata(true));

		/// <summary>
		/// Height or width of splitter, depends of orientation of the splitter
		/// </summary>
		/// <value>
		/// The thickness.
		/// </value>
		public double Thickness
        {
            get { return (double)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

		/// <summary>
		/// The thickness property
		/// </summary>
		public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness", typeof(double), typeof(DockPanelSplitter),
            new UIPropertyMetadata(4.0, ThicknessChanged));


		#region Private fields
		/// <summary>
		/// The element
		/// </summary>
		private FrameworkElement _element;     // element to resize (target element)
											   /// <summary>
											   /// The width
											   /// </summary>
		private double _width;                 // current desired width of the element, can be less than minwidth
											   /// <summary>
											   /// The height
											   /// </summary>
		private double _height;                // current desired height of the element, can be less than minheight
											   /// <summary>
											   /// The previous parent width
											   /// </summary>
		private double _previousParentWidth;   // current width of parent element, used for proportional resize
											   /// <summary>
											   /// The previous parent height
											   /// </summary>
		private double _previousParentHeight;  // current height of parent element, used for proportional resize
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="DockPanelSplitter"/> class.
		/// </summary>
		public DockPanelSplitter()
        {
            Loaded += DockPanelSplitterLoaded;
            Unloaded += DockPanelSplitterUnloaded;

            UpdateHeightOrWidth();
        }

		/// <summary>
		/// DockPanel splitter loaded.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		void DockPanelSplitterLoaded(object sender, RoutedEventArgs e)
        {
            var dp = Parent as Panel;
            if (dp == null) return;

            // Subscribe to the parent's size changed event
            dp.SizeChanged += ParentSizeChanged;

            // Store the current size of the parent DockPanel
            _previousParentWidth = dp.ActualWidth;
            _previousParentHeight = dp.ActualHeight;

            // Find the target element
            UpdateTargetElement();

        }

		/// <summary>
		/// DockPanel splitter unloaded.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		void DockPanelSplitterUnloaded(object sender, RoutedEventArgs e)
        {
            var dp = Parent as Panel;
            if (dp == null) return;

            // Unsubscribe
            dp.SizeChanged -= ParentSizeChanged;
        }

		/// <summary>
		/// Docks the changed.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void DockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockPanelSplitter)d).UpdateHeightOrWidth();
        }

		/// <summary>
		/// Thicknesses the changed.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void ThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockPanelSplitter)d).UpdateHeightOrWidth();
        }

		/// <summary>
		/// Updates the width of the height or.
		/// </summary>
		private void UpdateHeightOrWidth()
        {
            if (IsHorizontal)
            {
                Height = Thickness;
                Width = double.NaN;
            }
            else
            {
                Width = Thickness;
                Height = double.NaN;
            }
        }

		/// <summary>
		/// Gets a value indicating whether this instance is horizontal.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is horizontal; otherwise, <c>false</c>.
		/// </value>
		public bool IsHorizontal
        {
            get
            {
                var dock = DockPanel.GetDock(this);
                return dock == Dock.Top || dock == Dock.Bottom;
            }
        }

		/// <summary>
		/// Update the target element (the element the DockPanelSplitter works on)
		/// </summary>
		private void UpdateTargetElement()
        {
            var dp = Parent as Panel;
            if (dp == null) return;

            var i = dp.Children.IndexOf(this);

            // The splitter cannot be the first child of the parent DockPanel
            // The splitter works on the 'older' sibling 
            if (i > 0 && dp.Children.Count > 0)
            {
                _element = dp.Children[i - 1] as FrameworkElement;
            }
        }

		/// <summary>
		/// Sets the width of the target.
		/// </summary>
		/// <param name="newWidth">The new width.</param>
		private void SetTargetWidth(double newWidth)
        {
            if (newWidth < _element.MinWidth)
                newWidth = _element.MinWidth;
            if (newWidth > _element.MaxWidth)
                newWidth = _element.MaxWidth;

            // todo - constrain the width of the element to the available client area
            var dp = Parent as Panel;
            var dock = DockPanel.GetDock(this);
            var t = _element.TransformToAncestor(dp) as MatrixTransform;
            if (dock == Dock.Left && newWidth > dp.ActualWidth - t.Matrix.OffsetX - Thickness)
                newWidth = dp.ActualWidth - t.Matrix.OffsetX - Thickness;
            
            _element.Width = newWidth;
        }

		/// <summary>
		/// Sets the height of the target.
		/// </summary>
		/// <param name="newHeight">The new height.</param>
		private void SetTargetHeight(double newHeight)
        {
            if (newHeight < _element.MinHeight)
                newHeight = _element.MinHeight;
            if (newHeight > _element.MaxHeight)
                newHeight = _element.MaxHeight;

            // todo - constrain the height of the element to the available client area
            var dp = Parent as Panel;
            var dock = DockPanel.GetDock(this);
            var t = _element.TransformToAncestor(dp) as MatrixTransform;
            if (dock == Dock.Top && newHeight > dp.ActualHeight - t.Matrix.OffsetY - Thickness)
                newHeight = dp.ActualHeight - t.Matrix.OffsetY - Thickness;

            _element.Height = newHeight;
        }

		/// <summary>
		/// Parents the size changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
		private void ParentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!ProportionalResize) return;

            var dp = Parent as DockPanel;
            if (dp == null) return;

            double sx = dp.ActualWidth / _previousParentWidth;
            double sy = dp.ActualHeight / _previousParentHeight;

            if (!double.IsInfinity(sx))
                SetTargetWidth(_element.Width * sx);
            if (!double.IsInfinity(sy))
                SetTargetHeight(_element.Height * sy);

            _previousParentWidth = dp.ActualWidth;
            _previousParentHeight = dp.ActualHeight;

        }

		/// <summary>
		/// Adjusts the width.
		/// </summary>
		/// <param name="dx">The dx.</param>
		/// <param name="dock">The dock.</param>
		/// <returns></returns>
		double AdjustWidth(double dx, Dock dock)
        {
            if (dock == Dock.Right)
                dx = -dx;

            _width += dx;
            SetTargetWidth(_width);

            return dx;
        }

		/// <summary>
		/// Adjusts the height.
		/// </summary>
		/// <param name="dy">The dy.</param>
		/// <param name="dock">The dock.</param>
		/// <returns></returns>
		double AdjustHeight(double dy, Dock dock)
        {
            if (dock == Dock.Bottom)
                dy = -dy;

            _height += dy;
            SetTargetHeight(_height);

            return dy;
        }

		/// <summary>
		/// The start drag point
		/// </summary>
		Point _startDragPoint;

		/// <summary>
		/// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseEnter" /> attached event is raised on this element. Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
		protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (!IsEnabled) return;
            Cursor = IsHorizontal ? Cursors.SizeNS : Cursors.SizeWE;
        }

		/// <summary>
		/// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. This event data reports details about the mouse button that was pressed and the handled state.</param>
		protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;

            if (!IsMouseCaptured)
            {
                _startDragPoint = e.GetPosition(Parent as IInputElement);
                UpdateTargetElement();
                if (_element != null)
                {
                    _width = _element.ActualWidth;
                    _height = _element.ActualHeight;
                    CaptureMouse();
                }
            }

            base.OnMouseDown(e);
        }

		/// <summary>
		/// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseMove" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs" /> that contains the event data.</param>
		protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                var ptCurrent = e.GetPosition(Parent as IInputElement);
                var delta = new Point(ptCurrent.X - _startDragPoint.X, ptCurrent.Y - _startDragPoint.Y);
                var dock = DockPanel.GetDock(this);

                if (IsHorizontal)
                    delta.Y = AdjustHeight(delta.Y, dock);
                else
                    delta.X = AdjustWidth(delta.X, dock);

                var isBottomOrRight = (dock == Dock.Right || dock == Dock.Bottom);

                // When docked to the bottom or right, the position has changed after adjusting the size
                _startDragPoint = isBottomOrRight ? e.GetPosition(Parent as IInputElement) : new Point(_startDragPoint.X + delta.X, _startDragPoint.Y + delta.Y);
            }
            base.OnMouseMove(e);
        }

		/// <summary>
		/// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseUp" /> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs" /> that contains the event data. The event data reports that the mouse button was released.</param>
		protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
            base.OnMouseUp(e);
        }

    }
}
