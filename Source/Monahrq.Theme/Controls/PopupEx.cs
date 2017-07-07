using Microsoft.Practices.Prism;
using Monahrq.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Monahrq.Theme.Controls
{
	// http://stackoverflow.com/questions/1267349/wpf-popup-zorder
	/// <summary>
	/// Popup Control.
	/// </summary>
	/// <seealso cref="System.Windows.Controls.Primitives.Popup" />
	public class PopupEx : Popup
	{
		/// <summary>
		/// Initializes the <see cref="PopupEx"/> class.
		/// </summary>
		static PopupEx()
		{
			//DefaultStyleKeyProperty.OverrideMetadata(typeof(PopupEx), new FrameworkPropertyMetadata(typeof(PopupEx)));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PopupEx"/> class.
		/// </summary>
		public PopupEx()
		{
			Type baseType = this.GetType().BaseType;
		//	dynamic popupSecHelper = GetHiddenField(this, baseType, "_secHelper");
			dynamic popupSecHelper = GetHiddenField(this, typeof(Popup), "_secHelper");
			SetHiddenField(popupSecHelper, "_isChildPopupInitialized", true);
			SetHiddenField(popupSecHelper, "_isChildPopup", true);
		}

		/// <summary>
		/// Gets the hidden field.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <returns></returns>
		protected static dynamic GetHiddenField(object container, string fieldName)
		{
			return GetHiddenField(container, container.GetType(), fieldName);
		}

		/// <summary>
		/// Gets the hidden field.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="containerType">Type of the container.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <returns></returns>
		protected static dynamic GetHiddenField(object container, Type containerType, string fieldName)
		{
			dynamic retVal = null;
			FieldInfo fieldInfo = containerType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
			if (fieldInfo != null)
			{
				retVal = fieldInfo.GetValue(container);
			}
			return retVal;
		}

		/// <summary>
		/// Sets the hidden field.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <param name="value">The value.</param>
		protected static void SetHiddenField(object container, string fieldName, object value)
		{
			SetHiddenField(container, container.GetType(), fieldName, value);
		}

		/// <summary>
		/// Sets the hidden field.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="containerType">Type of the container.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <param name="value">The value.</param>
		protected static void SetHiddenField(object container, Type containerType, string fieldName, object value)
		{
			FieldInfo fieldInfo = containerType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
			if (fieldInfo != null)
			{
				fieldInfo.SetValue(container, value);
			}
		}


		#region Dragging Handler
		/// <summary>
		/// The anchor point
		/// </summary>
		private Point anchorPoint;
		/// <summary>
		/// The current point
		/// </summary>
		private Point currentPoint;
		/// <summary>
		/// The is in drag
		/// </summary>
		private bool isInDrag;
		/// <summary>
		/// The in dragging element
		/// </summary>
		private bool inDraggingElement;
		/// <summary>
		/// The transform
		/// </summary>
		private readonly TranslateTransform transform = new TranslateTransform();

		/// <summary>
		/// Handles the mouse down.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
		public void HandleMouseDown(object sender, MouseButtonEventArgs e)
		{
			//var popupEx = sender as PopupEx;
			if (!inDraggingElement) return;
			anchorPoint = e.GetPosition(null);
			CaptureMouse();
			MouseMove += HandleMouseMove;
			isInDrag = true;
			e.Handled = true;
		}
		/// <summary>
		/// Handles the mouse move.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void HandleMouseMove(object sender, MouseEventArgs e)
		{
			if (!isInDrag) return;
			currentPoint = e.GetPosition(null);
			
		//	transform.X += currentPoint.X - anchorPoint.X;
		//	transform.Y += (currentPoint.Y - anchorPoint.Y);
		//	RenderTransform = transform;

			var offset = currentPoint - anchorPoint;
			HorizontalOffset += offset.X;
			VerticalOffset += offset.Y;

			anchorPoint = currentPoint;
		}
		/// <summary>
		/// Handles the mouse up.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
		private void HandleMouseUp(object sender, MouseButtonEventArgs e)
		{
			if (!inDraggingElement) return;
			if (!isInDrag) return;
			ReleaseMouseCapture();
			isInDrag = false;
			e.Handled = true;
		}
		/// <summary>
		/// Handles the mouse enter.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
		private void HandleMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var popupEx = sender as PopupEx;
			Monahrq.Infrastructure.Types.ApplicationCursor.CurrentCursor().Push(System.Windows.Input.Cursors.Hand);
			popupEx.inDraggingElement = true;
		}
		/// <summary>
		/// Handles the mouse leave.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void HandleMouseLeave(object sender, MouseEventArgs e)
		{
			var popupEx = sender as PopupEx;
			Monahrq.Infrastructure.Types.ApplicationCursor.CurrentCursor().Pop();
			popupEx.inDraggingElement = false;
		}
		#endregion


		#region IsDraggable.
		/// <summary>
		/// IsDraggable Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty IsDraggableProperty =
			DependencyProperty.RegisterAttached("IsDraggable", typeof(bool), typeof(PopupEx),
				new FrameworkPropertyMetadata((bool)false,
					new PropertyChangedCallback(OnIsDraggableChanged)));

		/// <summary>
		/// Gets the IsDraggable property.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <returns></returns>
		public static bool GetIsDraggable(DependencyObject d)
		{
			return (bool)d.GetValue(IsDraggableProperty);
		}
		/// <summary>
		/// Sets the IsDraggable property.
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
			var popupEx = d as PopupEx;
			bool oldIsDraggable = (bool)e.OldValue;
			bool newIsDraggable = (bool)d.GetValue(IsDraggableProperty);

			popupEx.MouseEnter -= popupEx.HandleMouseEnter;
			popupEx.MouseLeave -= popupEx.HandleMouseLeave;
			popupEx.MouseDown -= popupEx.HandleMouseDown;
			popupEx.MouseUp -= popupEx.HandleMouseUp;

			if (newIsDraggable)
			{
				popupEx.MouseEnter += popupEx.HandleMouseEnter;
				popupEx.MouseLeave += popupEx.HandleMouseLeave;
				popupEx.MouseDown += popupEx.HandleMouseDown;
				popupEx.MouseUp += popupEx.HandleMouseUp;
			}
		}
		#endregion

		#region Resizable.
		/// <summary>
		/// The resizable thumb property
		/// </summary>
		public static readonly DependencyProperty ResizableThumbProperty =
			DependencyProperty.RegisterAttached("ResizableThumb", typeof(Thumb), typeof(PopupEx),
				new FrameworkPropertyMetadata(null,
					new PropertyChangedCallback(OnResizableThumbChanged)));

		/// <summary>
		/// Called when [thumb drag started].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="DragStartedEventArgs"/> instance containing the event data.</param>
		private void OnThumbDragStarted(object sender, DragStartedEventArgs e)
		{
			Thumb t = (Thumb)sender;
			t.Cursor = Cursors.Hand;
		}
		/// <summary>
		/// Called when [thumb drag delta].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="DragDeltaEventArgs"/> instance containing the event data.</param>
		private void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
		{

			
			double xadjust = this.Width + e.HorizontalChange;
			double yadjust = this.Height + e.VerticalChange;
			if ((xadjust >= 0) && (yadjust >= 0))
			{
				this.Placement = PlacementMode.AbsolutePoint;
				this.Width = xadjust.Clamp(this.MinWidth, this.MaxWidth);
				this.Height = yadjust.Clamp(this.MinHeight, this.MaxHeight);
			}
		}
		/// <summary>
		/// Called when [thumb drag completed].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="DragCompletedEventArgs"/> instance containing the event data.</param>
		private void OnThumbDragCompleted(object sender, DragCompletedEventArgs e)
		{
			Thumb t = (Thumb)sender;
			t.Cursor = null;
		}


		/// <summary>
		/// Gets the resizable thumb.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <returns></returns>
		public static Thumb GetResizableThumb(DependencyObject d)
		{
			return (Thumb)d.GetValue(ResizableThumbProperty);
		}
		/// <summary>
		/// Sets the resizable thumb.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="value">The value.</param>
		public static void SetResizableThumb(DependencyObject d, Thumb value)
		{
			d.SetValue(ResizableThumbProperty, value);
		}

		/// <summary>
		/// Called when [resizable thumb changed].
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnResizableThumbChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			//var popupEx = d as PopupEx;
			var popupEx = (d is PopupEx) ? d as PopupEx : (d as FrameworkElement).GetParent<PopupEx>();
			if (popupEx == null) return;

			var oldResizableThumb = (Thumb)e.OldValue;
			var newResizableThumb = (Thumb)d.GetValue(ResizableThumbProperty);

			if (oldResizableThumb != null)
			{
				oldResizableThumb.DragStarted -= popupEx.OnThumbDragStarted;
				oldResizableThumb.DragDelta -= popupEx.OnThumbDragDelta;
				oldResizableThumb.DragCompleted -= popupEx.OnThumbDragCompleted;
			}
			if (newResizableThumb != null)
			{
				newResizableThumb.DragStarted -= popupEx.OnThumbDragStarted;
				newResizableThumb.DragDelta -= popupEx.OnThumbDragDelta;
				newResizableThumb.DragCompleted -= popupEx.OnThumbDragCompleted;

				newResizableThumb.DragStarted += popupEx.OnThumbDragStarted;
				newResizableThumb.DragDelta += popupEx.OnThumbDragDelta;
				newResizableThumb.DragCompleted += popupEx.OnThumbDragCompleted;
			}
		}

		#endregion


		#region IsChildPopup.
		/// <summary>
		/// IsDraggable Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty IsChildPopupProperty =
			DependencyProperty.RegisterAttached("IsChildPopup", typeof(bool), typeof(PopupEx),
				new FrameworkPropertyMetadata((bool)true,
					new PropertyChangedCallback(OnIsChildPopupChanged)));

		/// <summary>
		/// Gets the is child popup.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <returns></returns>
		public static bool GetIsChildPopup(DependencyObject d)
		{
			return (bool)d.GetValue(IsChildPopupProperty);
		}
		/// <summary>
		/// Sets the IsDraggable property.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="value">if set to <c>true</c> [value].</param>
		public static void SetIsChildPopup(DependencyObject d, bool value)
		{
			d.SetValue(IsChildPopupProperty, value);
		}

		/// <summary>
		/// Called when [is child popup changed].
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnIsChildPopupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var popupEx = d as PopupEx;
			bool oldIsChildPopup = (bool)e.OldValue;
			bool newIsChildPopup = (bool)d.GetValue(IsChildPopupProperty);


			dynamic popupSecHelper = GetHiddenField(popupEx, typeof(Popup), "_secHelper");
			SetHiddenField(popupSecHelper, "_isChildPopupInitialized", newIsChildPopup);
			SetHiddenField(popupSecHelper, "_isChildPopup", newIsChildPopup);
		}
		#endregion


	}


	/*
	public class PopupEx : Popup
	{

		//	Code from: https://chriscavanagh.wordpress.com/2008/08/13/non-topmost-wpf-popup/
		//	Chris Cavanagh:
		//	The WPF Popup control is always “topmost” over other application windows.  If you’re happy with a
		//	dirty workaround to remove the Topmost state, you can derive your own Popup control similar to
		//	this:

		//	Joe Gershgorin:
		//	This solution almost got me there. It traded one problem for another. I have a draggable Popup,
		//	and with the above code it’s always under other windows, even if the popup has focus.  I came up
		//	with solution that I’m mostly happy with. To make it ideal I really wish the GotFocus/LostFocus
		//	events of the popup control behaved/fired as described.  My solution, sets the PopUp OnTopMost on
		//	leftbuttondown, and sets it back to the istopmost property when another window gets focus. I find
		//	out if a another window gets focus by hooking into the PopUp’s parent window deactivated event.
		//	To make sure the deactivated event gets fired I first have to activate it on mouse
		//	leftbuttondown, which means on the parent window comes to front also (the only possible downside,
		//	but no biggie for me).
		//	
		//	The side effect of this solution is that it should also solve Dave’s Combobox issue.

		public static readonly DependencyProperty IsTopmostProperty =
			DependencyProperty.Register(
			"IsTopmost",
			typeof(bool),
			typeof(PopupEx),
			new FrameworkPropertyMetadata(false, OnIsTopmostChanged));

		private bool? _appliedTopMost;
		public bool IsTopmost
		{
			get { return (bool)GetValue(IsTopmostProperty); }
			set { SetValue(IsTopmostProperty, value); }
		}
		private bool _alreadyLoaded;
		public PopupEx()
		{
			this.Loaded += new RoutedEventHandler(PopupEx_Loaded);
			this.Unloaded += new RoutedEventHandler(PopupEx_Unloaded);
			//MakePopupDraggable();
		}
// 		private void MakePopupDraggable()
// 		{
// 			var thumb = new Thumb
// 			{
// 				Width = 0,
// 				Height = 0,
// 			};
// 			ContentCanvas.Children.Add(thumb);
// 
// 			MouseDown += (sender, e) =>
// 			{
// 				thumb.RaiseEvent(e);
// 			};
// 
// 			thumb.DragDelta += (sender, e) =>
// 			{
// 				HorizontalOffset += e.HorizontalChange;
// 				VerticalOffset += e.VerticalChange;
// 			};
// 		}
		private Window _parentWindow;
		void PopupEx_Loaded(object sender, RoutedEventArgs e)
		{
			if (!_alreadyLoaded)
			{
				_alreadyLoaded = true;

				if (this.Child != null)
				{
					this.Child.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(Child_PreviewMouseLeftButtonDown), true);
				}

				_parentWindow = Window.GetWindow(this);

				if (_parentWindow != null)
				{
					_parentWindow.Activated += new EventHandler(_parentWindow_Activated);
					_parentWindow.Deactivated += new EventHandler(ParentWindow_Deactivated);
				}
			}

		}
		void PopupEx_Unloaded(object sender, RoutedEventArgs e)
		{
			if (_parentWindow != null)
			{
				_parentWindow.Activated -= new EventHandler(_parentWindow_Activated);
				_parentWindow.Deactivated -= new EventHandler(ParentWindow_Deactivated);
			}
		}
		void _parentWindow_Activated(object sender, EventArgs e)
		{
			Console.WriteLine("Parent Window Activated");
			SetTopmostState(true);
		}
		void ParentWindow_Deactivated(object sender, EventArgs e)
		{
			Console.WriteLine("Parent Window Deactivated");

			if (IsTopmost == false)
			{
				SetTopmostState(IsTopmost);
			}
		}
		void Child_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Console.WriteLine("Child Mouse Left Button Down");

			SetTopmostState(true);

			if (!_parentWindow.IsActive && IsTopmost == false)
			{
				_parentWindow.Activate();
				Console.WriteLine("Activating Parent from child Left Button Down");
			}
		}
		private static void OnIsTopmostChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var thisobj = obj as PopupEx;

			thisobj.SetTopmostState(thisobj.IsTopmost);
		}
		protected override void OnOpened(EventArgs e)
		{
			SetTopmostState(IsTopmost);
		}
		private void SetTopmostState(bool isTop)
		{
			// Don’t apply state if it’s the same as incoming state
			if (_appliedTopMost.HasValue && _appliedTopMost == isTop)
			{
				return;
			}

			if (this.Child != null)
			{
				var hwndSource = (PresentationSource.FromVisual(this.Child)) as HwndSource;

				if (hwndSource != null)
				{
					var hwnd = hwndSource.Handle;

					RECT rect;

					if (GetWindowRect(hwnd, out rect))
					{
						Console.WriteLine("setting z-order " + isTop);
						if (isTop)
						{
							SetWindowPos(hwnd, HWND_TOPMOST, rect.Left, rect.Top, (int)this.Width, (int)this.Height, TOPMOST_FLAGS);
						}
						else
						{
							// Z-Order would only get refreshed/reflected if clicking the
							// the titlebar (as opposed to other parts of the external
							// window) unless I first set the popup to HWND_BOTTOM
							// then HWND_TOP before HWND_NOTOPMOST
							SetWindowPos(hwnd, HWND_BOTTOM, rect.Left, rect.Top, (int)this.Width, (int)this.Height, TOPMOST_FLAGS);
							SetWindowPos(hwnd, HWND_TOP, rect.Left, rect.Top, (int)this.Width, (int)this.Height, TOPMOST_FLAGS);
							SetWindowPos(hwnd, HWND_NOTOPMOST, rect.Left, rect.Top, (int)this.Width, (int)this.Height, TOPMOST_FLAGS);
						}

						_appliedTopMost = isTop;
					}
				}
			}
		}

		#region P/Invoke imports & definitions

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X,
		int Y, int cx, int cy, uint uFlags);

		static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
		static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
		static readonly IntPtr HWND_TOP = new IntPtr(0);
		static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

		const UInt32 SWP_NOSIZE = 0x0001;
		const UInt32 SWP_NOMOVE = 0x0002;
		const UInt32 SWP_NOZORDER = 0x0004;
		const UInt32 SWP_NOREDRAW = 0x0008;
		const UInt32 SWP_NOACTIVATE = 0x0010;
		const UInt32 SWP_FRAMECHANGED = 0x0020;		// The frame changed: send WM_NCCALCSIZE
		const UInt32 SWP_SHOWWINDOW = 0x0040;
		const UInt32 SWP_HIDEWINDOW = 0x0080;
		const UInt32 SWP_NOCOPYBITS = 0x0100;
		const UInt32 SWP_NOOWNERZORDER = 0x0200;	// Don’t do owner Z ordering
		const UInt32 SWP_NOSENDCHANGING = 0x0400;	// Don’t send WM_WINDOWPOSCHANGING

		const UInt32 TOPMOST_FLAGS = SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOSIZE | SWP_NOMOVE | SWP_NOREDRAW | SWP_NOSENDCHANGING;

		#endregion
	}*/
}
