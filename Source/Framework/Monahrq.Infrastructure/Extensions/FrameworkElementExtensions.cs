using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Monahrq.Infrastructure.Extensions
{
	public static class FrameworkElementExtensions
	{
#if !SILVERLIGHT

		/// <summary>
		/// Get the window container of framework element.
		/// </summary>
		public static Window GetParentWindow(this FrameworkElement element)
		{
			DependencyObject dp = element;
			while (dp != null)
			{
				DependencyObject tp = LogicalTreeHelper.GetParent(dp);
				if (tp is Window) return tp as Window;
				else dp = tp;
			}
			return null;
		}

		public static T GetParent<T>(this FrameworkElement child) where T : FrameworkElement
		{
			//  Get the visual parent.
			FrameworkElement parent = child.Parent as FrameworkElement;

			//  If we've got the parent, return it if it is the correct type - otherwise
			//  continue up the tree.
			if (parent == null) return null;
			return parent is T ? parent as T : GetParent<T>(parent);
		}

#endif

		public static FrameworkElement GetTopLevelParent(this FrameworkElement element)
		{
			FrameworkElement p = element;
			while (p != null)
			{
				if (p.Parent == null)
					return p;
				p = p.Parent as FrameworkElement;
			}
			return null;
		}

		public static BitmapSource RenderBitmap(this FrameworkElement element)
		{
#if SILVERLIGHT

      //  We'll use the writable bitmap.
      WriteableBitmap wb = new WriteableBitmap((int)element.ActualWidth, (int)element.ActualHeight);
      wb.Render(element, new TranslateTransform());
      wb.Invalidate();
      return wb;

#else

			//  We're in WPF, so use the render bitmap.



			//  Create a visual brush from the element.
			VisualBrush elementBrush = new VisualBrush(element);

			//  Create a visual.
			DrawingVisual visual = new DrawingVisual();

			//  Open the visual to get a drawing context.
			DrawingContext dc = visual.RenderOpen();

			//  Draw the element in the appropriately sized rectangle.
			dc.DrawRectangle(elementBrush, null, new Rect(0, 0, element.ActualWidth, element.ActualHeight));

			//  Close the drawing context.
			dc.Close();

			//  WPF uses 96 DPI - this is defined in System.Windows.SystemParameters.DPI
			//  but it is internal, so we must use a magic number.
			double systemDPI = 96;

			//  Create the bitmap and render it.
			RenderTargetBitmap bitmap = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, systemDPI, systemDPI, PixelFormats.Default);
			bitmap.Render(visual);

			//  Return the bitmap.
			return bitmap;
#endif
		}


		#region Position Methods.

		public static double GetLeft(this FrameworkElement element)
		{
			var pos = element.TranslatePoint(new Point(0, 0), null);
			return pos.X;
		}
		public static double GetTop(this FrameworkElement element)
		{
			var pos = element.TranslatePoint(new Point(0, 0), null);
			return pos.Y;
		}
		public static Point GetTopLeft(this FrameworkElement element)
		{
			var pos = element.TranslatePoint(new Point(0, 0), null);
			return pos;
		}
		public static Point GetCenter(this FrameworkElement element,bool useActual=true)
		{
			var pos = element.TranslatePoint(new Point(0, 0), null);
			pos.X += (useActual ? element.ActualWidth : element.Width) / 2.0;
			pos.Y += (useActual ? element.ActualHeight : element.Height) / 2.0;
			return pos;
		}
		public static Point GetCenteredTopLeft(this FrameworkElement element, FrameworkElement elementToBeCentered)
		{
			var pos = element.GetCenter();
			pos.X -= elementToBeCentered.ActualWidth / 2.0;
			pos.Y -= elementToBeCentered.ActualHeight / 2.0;
			return pos;
		}
		
		#endregion
	}
}
