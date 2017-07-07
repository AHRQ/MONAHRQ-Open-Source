using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Monahrq.Theme.Behaviors
{
	/// <summary>
	/// Behavior to add Mouse drag to Popups.
	/// </summary>
	/// <seealso cref="System.Windows.Interactivity.Behavior{System.Windows.Controls.Primitives.Popup}" />
	public class MouseDragPopupBehavior : Behavior<Popup>
	{
		/// <summary>
		/// The mouse down
		/// </summary>
		private bool mouseDown;
		/// <summary>
		/// The old mouse position
		/// </summary>
		private Point oldMousePosition;

		/// <summary>
		/// Called after the behavior is attached to an AssociatedObject.
		/// </summary>
		/// <remarks>
		/// Override this to hook up functionality to the AssociatedObject.
		/// </remarks>
		protected override void OnAttached()
		{
			AssociatedObject.MouseLeftButtonDown += (s, e) =>
			{
				mouseDown = true;
				oldMousePosition = AssociatedObject.PointToScreen(e.GetPosition(AssociatedObject));
				AssociatedObject.Child.CaptureMouse();
			};
			AssociatedObject.MouseMove += (s, e) =>
			{
				if (!mouseDown) return;
				var newMousePosition = AssociatedObject.PointToScreen(e.GetPosition(AssociatedObject));
				var offset = newMousePosition - oldMousePosition;
				oldMousePosition = newMousePosition;
				AssociatedObject.HorizontalOffset += offset.X;
				AssociatedObject.VerticalOffset += offset.Y;
			};
			AssociatedObject.MouseLeftButtonUp += (s, e) =>
			{
				mouseDown = false;
				AssociatedObject.Child.ReleaseMouseCapture();
			};
		}
	}
}
