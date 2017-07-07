using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Monahrq.Theme.Controls
{
	/// <summary>
	/// Popup control.
	/// </summary>
	/// <seealso cref="System.Windows.Documents.AdornerDecorator" />
	/// <seealso cref="System.IDisposable" />
	public class xPopup : AdornerDecorator, IDisposable
    {

		#region Constructors
		/// <summary>
		/// Initializes the <see cref="xPopup"/> class.
		/// </summary>
		static xPopup()
        {
            VisibilityProperty.OverrideMetadata(typeof(xPopup)
                , new FrameworkPropertyMetadata(Visibility.Collapsed));

        }
		/// <summary>
		/// The active popup
		/// </summary>
		static xPopup ActivePopup;
		/// <summary>
		/// Initializes a new instance of the <see cref="xPopup"/> class.
		/// </summary>
		public xPopup()
        {
            AdornedElements = new ObservableCollection<UIElement>();
            this.IsVisibleChanged += MyPopupControl_IsVisibleChanged;
            xPopup.ActivePopup = this;
        }

		/// <summary>
		/// The active cursor
		/// </summary>
		static private OverrideCursor activeCursor = new OverrideCursor();


		/// <summary>
		/// Translates the location.
		/// </summary>
		private void TranslateLocation()
        {
            var rootWindow = Application.Current.MainWindow;

            var placementTransform = PlacementTarget.TransformToAncestor(rootWindow);
            var popupTransform = this.TransformToAncestor(rootWindow);
            var placementPoint = placementTransform.Transform(new Point(0, 0));
            var popupPoint = placementTransform.Transform(new Point(0, 0));

            var translateTransform = new TranslateTransform(placementPoint.X - popupPoint.X, placementPoint.Y - popupPoint.Y);
            RenderTransform = translateTransform;
        }



		#endregion

		//Presents the content of this control in a Adorner Layer
		#region Adorner
		/// <summary>
		/// Gets the adorned elements.
		/// </summary>
		/// <value>
		/// The adorned elements.
		/// </value>
		public ObservableCollection<UIElement> AdornedElements { get; private set; }
		/// <summary>
		/// Gets the popup adorner.
		/// </summary>
		/// <value>
		/// The popup adorner.
		/// </value>
		public PopupAdorner PopupAdorner { get; private set; }
		/// <summary>
		/// Show Popup content
		/// </summary>
		public void ShowPopup()
        {
            PopupAdorner = new PopupAdorner(this.Child);
            this.Visibility = System.Windows.Visibility.Visible;
        }

		/// <summary>
		/// Handles the IsVisibleChanged event of the MyPopupControl control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		void MyPopupControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true) ShowPopup();
            else HidePopup();
        }
		/// <summary>
		/// Hide Popup content
		/// </summary>
		public void HidePopup()
        {
            if (mainAdornerLayer != null) mainAdornerLayer.Remove(PopupAdorner);
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

		#endregion

		//Moves the popup after the mouse
		#region Dragging Handler
		/// <summary>
		/// The anchor point
		/// </summary>
		Point _anchorPoint;
		/// <summary>
		/// The current point
		/// </summary>
		Point _currentPoint;
		/// <summary>
		/// The is in drag
		/// </summary>
		bool _isInDrag;

		/// <summary>
		/// Handles the MouseDown event of the xPopup control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
		public void xPopup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!ActivePopup._inDraggingElement) return;
            var element = sender as FrameworkElement;
            _anchorPoint = e.GetPosition(null);
            if (element != null) element.CaptureMouse();
            element.MouseMove += element_MouseMove;
            _isInDrag = true;
            e.Handled = true;
        }

		/// <summary>
		/// The transform
		/// </summary>
		private readonly TranslateTransform _transform = new TranslateTransform();
		/// <summary>
		/// Handles the MouseMove event of the element control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void element_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isInDrag) return;
            _currentPoint = e.GetPosition(null);

            _transform.X += _currentPoint.X - _anchorPoint.X;
            _transform.Y += (_currentPoint.Y - _anchorPoint.Y);
            RenderTransform = _transform;
            _anchorPoint = _currentPoint;
        }

		/// <summary>
		/// Handles the MouseUp event of the xPopup control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
		private void xPopup_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!ActivePopup._inDraggingElement) return;
            if (!_isInDrag) return;
            var element = sender as FrameworkElement;
            if (element != null) element.ReleaseMouseCapture();
            _isInDrag = false;
            e.Handled = true;
        }
		#endregion

		#region Dependency Properties

		//Identifies the location where the popup will be located (Work in Progress)
		#region PlacementTarget

		/// <summary>
		/// PlacementTarget Dependency Property
		/// </summary>
		public static readonly DependencyProperty PlacementTargetProperty =
            DependencyProperty.Register("PlacementTarget", typeof(UIElement), typeof(xPopup),
                new FrameworkPropertyMetadata((UIElement)null,
                    new PropertyChangedCallback(OnPlacementTargetChanged)));

		/// <summary>
		/// Gets or sets the PlacementTarget property. This dependency property
		/// indicates ....
		/// </summary>
		/// <value>
		/// The placement target.
		/// </value>
		public UIElement PlacementTarget
        {
            get { return (UIElement)GetValue(PlacementTargetProperty); }
            set { SetValue(PlacementTargetProperty, value); }
        }

		/// <summary>
		/// Handles changes to the PlacementTarget property.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnPlacementTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            xPopup target = (xPopup)d;
            UIElement oldPlacementTarget = (UIElement)e.OldValue;
            UIElement newPlacementTarget = target.PlacementTarget;
            target.OnPlacementTargetChanged(oldPlacementTarget, newPlacementTarget);
        }

		#region Adorner Layer
		/// <summary>
		/// The main adorner layer
		/// </summary>
		private AdornerLayer _mainAdornerLayer;
		//AodrnerLayer for all controls but the Popup window
		/// <summary>
		/// Gets or sets the main adorner layer.
		/// </summary>
		/// <value>
		/// The main adorner layer.
		/// </value>
		public AdornerLayer mainAdornerLayer
        {
            get
            {
                //find AdornerLayer for first time use
                if (_mainAdornerLayer == null)
                {
                    if (PlacementTarget != null) _mainAdornerLayer = AdornerLayer.GetAdornerLayer(PlacementTarget);
                    else _mainAdornerLayer = AdornerLayer.GetAdornerLayer(this.Parent as Visual);
                }
                return _mainAdornerLayer;
            }
            set { _mainAdornerLayer = value; }
        }

		/// <summary>
		/// The popup adorner layer
		/// </summary>
		private AdornerLayer _popupAdornerLayer;
		//Adorner layer for the Popup Window
		/// <summary>
		/// Gets or sets the popup adorner layer.
		/// </summary>
		/// <value>
		/// The popup adorner layer.
		/// </value>
		public AdornerLayer PopupAdornerLayer
        {
            get
            {
                if (_popupAdornerLayer == null)
                {
                    _popupAdornerLayer = AdornerLayer.GetAdornerLayer(this);
                }
                return _popupAdornerLayer;
            }
            set { _popupAdornerLayer = value; }
        }

		/// <summary>
		/// Update Adorner Layer if PlacementTarget changes
		/// </summary>
		/// <param name="oldPlacementTarget">The old placement target.</param>
		/// <param name="newPlacementTarget">The new placement target.</param>
		protected virtual void OnPlacementTargetChanged(UIElement oldPlacementTarget, UIElement newPlacementTarget)
        {
            if (newPlacementTarget == null) return;
            //Find the PlacementTarget's closest AdornerLayer
            mainAdornerLayer = AdornerLayer.GetAdornerLayer(newPlacementTarget);
        }
		#endregion



		#endregion

		//Identifies the Elements responsible for dragging the Popup
		#region IsDraggable

		/// <summary>
		/// IsDraggable Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty IsDraggableProperty =
            DependencyProperty.RegisterAttached("IsDraggable", typeof(bool), typeof(xPopup),
                new FrameworkPropertyMetadata((bool)false,
                    new PropertyChangedCallback(OnIsDraggableChanged)));

		/// <summary>
		/// Gets the IsDraggable property. This dependency property
		/// indicates ....
		/// </summary>
		/// <param name="d">The d.</param>
		/// <returns></returns>
		public static bool GetIsDraggable(DependencyObject d)
        {
            return (bool)d.GetValue(IsDraggableProperty);
        }

		/// <summary>
		/// Sets the IsDraggable property. This dependency property
		/// indicates ....
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="value">if set to <c>true</c> [value].</param>
		public static void SetIsDraggable(DependencyObject d, bool value)
        {
            d.SetValue(IsDraggableProperty, value);
        }

		/// <summary>
		/// Handles changes to the IsDraggable property.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnIsDraggableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool oldIsDraggable = (bool)e.OldValue;
            bool newIsDraggable = (bool)d.GetValue(IsDraggableProperty);
            if (newIsDraggable)
            {
                ((FrameworkElement)d).MouseEnter += xPopup_MouseEnter;
                ((FrameworkElement)d).MouseLeave += xPopup_MouseLeave;
                ((FrameworkElement)d).MouseDown += ActivePopup.xPopup_MouseDown;
                ((FrameworkElement)d).MouseUp += ActivePopup.xPopup_MouseUp;
            }
            else
            {
                ((FrameworkElement)d).MouseEnter -= xPopup_MouseEnter;
                ((FrameworkElement)d).MouseLeave -= xPopup_MouseLeave;
                ((FrameworkElement)d).MouseDown -= ActivePopup.xPopup_MouseDown;
                ((FrameworkElement)d).MouseUp -= ActivePopup.xPopup_MouseUp;
            }
        }
		/// <summary>
		/// The in dragging element
		/// </summary>
		bool _inDraggingElement;
		/// <summary>
		/// Handles the MouseEnter event of the xPopup control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
		static void xPopup_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            activeCursor.PushCursor(Cursors.Hand);
            //Mouse.OverrideCursor = Cursors.Hand;
            ActivePopup._inDraggingElement = true;
        }
		/// <summary>
		/// Handles the MouseLeave event of the xPopup control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		static void xPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            activeCursor.PopCursor();
            //Mouse.OverrideCursor = null;
            ActivePopup._inDraggingElement = false;
        }

		#endregion

		#endregion




		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
        {
            activeCursor.Dispose();
        }
    }
}
